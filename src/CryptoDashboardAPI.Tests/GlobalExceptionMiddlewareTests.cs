using System.Net;
using System.Text.Json;
using CryptoDashboardAPI.Exceptions;
using CryptoDashboardAPI.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CryptoDashboardAPI.Tests;

public class GlobalExceptionMiddlewareTests
{
    private static async Task<(int StatusCode, string Body, IHeaderDictionary Headers)> InvokeWithException(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return (context.Response.StatusCode, body, context.Response.Headers);
    }

    [Theory]
    [InlineData(typeof(NotFoundException), (int)HttpStatusCode.NotFound)]
    [InlineData(typeof(ConflictException), (int)HttpStatusCode.Conflict)]
    [InlineData(typeof(ExternalApiException), (int)HttpStatusCode.BadGateway)]
    [InlineData(typeof(UnauthorizedAccessException), (int)HttpStatusCode.Unauthorized)]
    [InlineData(typeof(ArgumentException), (int)HttpStatusCode.BadRequest)]
    public async Task ExceptionType_MapsToCorrectStatusCode(Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "test message")!;

        var (status, _, _) = await InvokeWithException(exception);

        status.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task CooldownException_Returns429WithRetryAfterHeader()
    {
        var (status, _, headers) = await InvokeWithException(new CooldownException("On cooldown", 42));

        status.Should().Be((int)HttpStatusCode.TooManyRequests);
        headers["Retry-After"].ToString().Should().Be("42");
    }

    [Fact]
    public async Task UnhandledException_Returns500()
    {
        var (status, _, _) = await InvokeWithException(new InvalidOperationException("unexpected"));

        status.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Response_IsProblemDetailsJson()
    {
        var (_, body, _) = await InvokeWithException(new NotFoundException("coin not found"));

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetInt32().Should().Be(404);
        doc.RootElement.GetProperty("title").GetString().Should().Be("Not Found");
        doc.RootElement.GetProperty("detail").GetString().Should().Be("coin not found");
    }

    [Fact]
    public async Task Response_DoesNotContainStackTrace()
    {
        var (_, body, _) = await InvokeWithException(new InvalidOperationException("oops"));

        body.Should().NotContain("at ");
        body.Should().NotContain("StackTrace");
    }
}
