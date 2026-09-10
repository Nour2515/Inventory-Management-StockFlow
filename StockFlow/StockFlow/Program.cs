using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockFlow.Data;
using StockFlow.Hubs;
using StockFlow.Interfaces;
using StockFlow.IRepository;
using StockFlow.Jobs;
using StockFlow.Models;
using StockFlow.Repositories;
using StockFlow.Services;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString =builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
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
app.UseHangfireDashboard("/hangfire");

app.MapHub<InventoryHub>("/hubs/inventory");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

var recurringJobManager =app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<ReservationExpirationJob>(
    "reservation-expiration",
    job => job.RunAsync(),
    Cron.Minutely
);
recurringJobManager.AddOrUpdate<RefreshTokenCleanupJob>("refresh-token-cleanup",job => job.RunAsync(),Cron.Daily);
app.Run();
