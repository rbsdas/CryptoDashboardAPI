using System.Net;
using CryptoDashboardAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CryptoDashboardAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, "Not Found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            CooldownException => (HttpStatusCode.TooManyRequests, "Too Many Requests"),
            ExternalApiException => (HttpStatusCode.BadGateway, "External API Error"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ArgumentException => (HttpStatusCode.BadRequest, "Bad Request"),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Title}", title);

        if (exception is CooldownException cooldown)
            context.Response.Headers["Retry-After"] = cooldown.RetryAfterSeconds.ToString();

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
