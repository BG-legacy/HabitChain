using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace HabitChain.WebAPI.Middleware;

public class TokenExpirationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenExpirationMiddleware> _logger;

    public TokenExpirationMiddleware(RequestDelegate next, ILogger<TokenExpirationMiddleware> logger)
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
        catch (Exception ex) when (IsTokenExpirationException(ex))
        {
            _logger.LogWarning("Token expiration detected: {Message}", ex.Message);
            
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                message = "Token has expired. Please login again.",
                error = "TOKEN_EXPIRED",
                timestamp = DateTime.UtcNow
            };
            
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    private static bool IsTokenExpirationException(Exception ex)
    {
        // Check for common JWT token expiration exceptions
        return ex.Message.Contains("expired", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("token", StringComparison.OrdinalIgnoreCase) ||
               ex.GetType().Name.Contains("SecurityToken", StringComparison.OrdinalIgnoreCase);
    }
}

// Extension method for easy middleware registration
public static class TokenExpirationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenExpirationHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenExpirationMiddleware>();
    }
} 