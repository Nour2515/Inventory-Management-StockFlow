using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Data;
using StockFlow.DTOs.Errors;
using StockFlow.Hubs;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Jobs;
using StockFlow.Middleware;
using StockFlow.Models;
using StockFlow.Repositories;
using StockFlow.Services;
using System.Text.Json;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString =builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        //Validation Error
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value!.Errors
                        .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? "The input was not valid."
                            : error.ErrorMessage)
                        .ToArray());

            return new BadRequestObjectResult(new ErrorResponse
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Message = "One or more validation errors occurred.",
                TraceId = context.HttpContext.TraceIdentifier,
                Errors = errors
            });
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddIdentity<User, IdentityRole<int>>(options=>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();
builder.Services.AddScoped<IProductRepository, ProductRepo>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepo>();
builder.Services.AddScoped<IGenericRepository<Warehouse>, WarehouseRepo>();

builder.Services.AddScoped<ICategoryService, CategoryServices>();
builder.Services.AddScoped<IProductService, ProductServices>();

builder.Services.AddScoped<IinventoryServices, InventoryServices>();
builder.Services.AddScoped<IWarehouseService, WarehouseServices>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IOrderServices, OrderServices>();

builder.Services.AddScoped<IStockReservationRepository, StockReservationRepo>();
builder.Services.AddScoped<IStockReservationService, StockReservationService>();

builder.Services.AddScoped<IInventoryTransactionRepo, InventoryTransactionRepo>();
builder.Services.AddScoped<IinventoryTransactionservice, inventoryTransactionservice>();

builder.Services.AddScoped<IAuthService, AuthServices>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddSignalR();

builder.Services.AddHangfire(config =>
{
    config
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString);
});


builder.Services.AddHangfireServer();


builder.Services.AddScoped<ReservationExpirationJob>();
builder.Services.AddScoped<RefreshTokenCleanupJob>();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration
            .GetSection("Jwt");

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwt["Key"]!
                        )
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:ConnectionString"];

    options.InstanceName =
        builder.Configuration["Redis:InstanceName"];
});
builder.Services.AddSingleton<ICacheService, RedisCacheService>();


builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//Unexpected Exception
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;

    if (httpContext.Response.HasStarted ||
        httpContext.Response.ContentLength.HasValue ||
        httpContext.Response.ContentType != null)
    {
        return;
    }

    var response = httpContext.Response.StatusCode switch
    {
        StatusCodes.Status401Unauthorized => new ErrorResponse
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Message = "Authentication is required or the supplied token is invalid.",
            TraceId = httpContext.TraceIdentifier
        },
        StatusCodes.Status403Forbidden => new ErrorResponse
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Message = "You do not have permission to access this resource.",
            TraceId = httpContext.TraceIdentifier
        },
        _ => null
    };

    if (response == null)
    {
        return;
    }

    httpContext.Response.ContentType = "application/json";
    await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }));
});

app.UseHangfireDashboard("/hangfire");


app.UseHttpsRedirection();

app.UseCors("frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<InventoryHub>("/hubs/inventory");

app.MapControllers();

var recurringJobManager =app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<ReservationExpirationJob>(
    "reservation-expiration",
    job => job.RunAsync(),
    Cron.Minutely
);
recurringJobManager.AddOrUpdate<RefreshTokenCleanupJob>("refresh-token-cleanup",job => job.RunAsync(),Cron.Daily);
app.Run();
