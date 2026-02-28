// Middleware/ExceptionMiddleware.cs
using ms_notification.Exceptions;
using System.Net;
using System.Text.Json;

namespace ms_notification.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex.Message);
            await WriteResponse(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning(ex.Message);
            await WriteResponse(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue");
            await WriteResponse(context, HttpStatusCode.InternalServerError,
                "Une erreur inattendue s'est produite");
        }
    }

    private static async Task WriteResponse(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}