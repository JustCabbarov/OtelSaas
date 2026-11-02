using System.Net;
using System.Text.Json;
using HotelQrSaaS.Application.Common;
using HotelQrSaaS.Application.Common.Exceptions;

namespace HotelQrSaaS.WebApi.Middlewares;

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

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string? errorCode = null;
        string message;
        List<string>? errors = null;

        switch (exception)
        {
            case TenantSuspendedException tse:
                statusCode = (int)HttpStatusCode.Forbidden;
                errorCode = tse.ErrorCode;
                message = tse.Message;
                break;

            case NotFoundException nfe:
                statusCode = (int)HttpStatusCode.NotFound;
                errorCode = nfe.ErrorCode;
                message = nfe.Message;
                break;

            case BadRequestException bre:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorCode = bre.ErrorCode;
                message = bre.Message;
                break;

            case UnauthorizedException ue:
                statusCode = (int)HttpStatusCode.Unauthorized;
                errorCode = ue.ErrorCode;
                message = ue.Message;
                break;

            case FluentValidation.ValidationException ve:
                statusCode = (int)HttpStatusCode.BadRequest;
                errorCode = "VALIDATION_ERROR";
                message = "Daxil edilən məlumatlarda səhvlər var.";
                errors = ve.Errors.Select(e => e.ErrorMessage).ToList();
                break;

            default:
                _logger.LogError(exception, "Gözlənilməz server xətası: {Message}", exception.Message);
                statusCode = (int)HttpStatusCode.InternalServerError;
                errorCode = "INTERNAL_SERVER_ERROR";
                message = "Gözlənilməz daxili server xətası baş verdi.";
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.Fail(message, errorCode, errors);
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}

