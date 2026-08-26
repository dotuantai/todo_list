using API_v2.Models.Constants;
using API_v2.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API_v2.Helpers
{
    public class ApiErrorResponseFilter : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult result &&
                (result.StatusCode ?? StatusCodes.Status200OK) >= StatusCodes.Status400BadRequest &&
                result.Value is not ApiErrorResponse)
            {
                var statusCode = result.StatusCode ?? StatusCodes.Status500InternalServerError;
                var message = result.Value?.GetType().GetProperty("Message")?.GetValue(result.Value)?.ToString()
                    ?? "The request could not be completed.";
                result.Value = new ApiErrorResponse
                {
                    ErrorCode = GetErrorCode(statusCode),
                    Message = message,
                    CorrelationId = context.HttpContext.TraceIdentifier
                };
            }

            return next();
        }

        private static string GetErrorCode(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => ErrorCodes.ValidationFailed,
            StatusCodes.Status401Unauthorized => ErrorCodes.Unauthorized,
            StatusCodes.Status403Forbidden => ErrorCodes.Forbidden,
            StatusCodes.Status404NotFound => ErrorCodes.ResourceNotFound,
            StatusCodes.Status409Conflict => ErrorCodes.Conflict,
            _ => ErrorCodes.InternalServerError
        };
    }
}
