using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StockFlow.DTOs.Errors;
using StockFlow.Exceptions;

namespace StockFlow.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next,ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, message, errors) = MapException(exception);

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }
        else
        {
            _logger.LogInformation(
                "Handled application exception {ExceptionType}: {Message}",
                exception.GetType().Name,
                exception.Message);
        }

        if (context.Response.HasStarted)
        {
            throw exception;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ErrorResponse
        {
            Status = statusCode,
            Title = title,
            Message = message,
            TraceId = context.TraceIdentifier,
            Errors = errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static (int StatusCode, string Title, string Message, IDictionary<string, string[]>? Errors)
        MapException(Exception exception)
    {
        return exception switch
        {
            Exceptions.ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                validationException.Message,
                validationException.Errors),

            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                exception.Message,
                null),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                exception.Message,
                null),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Not Found",
                exception.Message,
                null),

            InsufficientStockException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message,
                null),

            ConflictException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message,
                null),

            BusinessRuleException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message,
                null),

            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "The resource was modified by another request. Please try again.",
                null),

            InvalidOperationException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                exception.Message,
                null),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.",
                null)
        };
    }
}
