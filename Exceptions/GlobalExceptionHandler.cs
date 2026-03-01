using ms_notification.DTOs;
using System.Text.Json;

namespace ms_notification.Exceptions;

public class GlobalExceptionHandler(RequestDelegate request)
{
    private readonly RequestDelegate _request = request;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _request(context);
        }
        catch (UserNotFoundException exception)
        {
            await WriteResponse(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (TemplateNotFoundException exception)
        {
            await WriteResponse(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (TemplateCodeNotFoundException exception)
        {
            await WriteResponse(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (NotificationChannelNotSupportedException exception)
        {
            await WriteResponse(context, StatusCodes.Status400BadRequest, exception.Message);
        }
        catch(UnauthorizedException exception)
        {
            await WriteResponse(context, StatusCodes.Status401Unauthorized, exception.Message);
        }
        catch (NotificationNotFoundException exception)
        {
            await WriteResponse(context, StatusCodes.Status404NotFound, exception.Message);
        }
    
    }

    private static Task WriteResponse(HttpContext context, int statusCode, string message)
    {
        ErrorResponse<string> response = new()
        {
            Message = message
        };

        string json = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(json);
    }

}

