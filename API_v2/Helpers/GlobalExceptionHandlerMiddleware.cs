using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using API_v2.Exceptions;
using API_v2.Models.DTOs;

namespace API_v2.Helpers
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            HttpStatusCode statusCode;
            string message;

            // Retrieve Correlation ID from response headers
            var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = context.TraceIdentifier;
            }

            if (exception is ApiException apiEx)
            {
                statusCode = apiEx.StatusCode;
                message = apiEx.Message;
                _logger.LogWarning("API Exception occurred: {Message} | Status: {Status} | CorrelationID: {CorrelationId}", 
                    message, statusCode, correlationId);
            }
            else
            {
                // Full details logged securely on server (Console/File)
                _logger.LogError(exception, "Unhandled system exception occurred: {Message} | CorrelationID: {CorrelationId}", 
                    exception.Message, correlationId);
                
                statusCode = HttpStatusCode.InternalServerError;
                message = "A system error occurred. Please try again later.";
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                Success = false,
                Message = message,
                CorrelationId = correlationId
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null
            });

            return context.Response.WriteAsync(json);
        }
    }
}
