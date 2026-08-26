using System;
using System.Net;
using API_v2.Models.Constants;

namespace API_v2.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ErrorCode { get; }

        public ApiException(HttpStatusCode statusCode, string errorCode, string message) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }

        public static ApiException BadRequest(string message, string errorCode = ErrorCodes.ValidationFailed) =>
            new ApiException(HttpStatusCode.BadRequest, errorCode, message);

        public static ApiException Unauthorized(string message, string errorCode = ErrorCodes.Unauthorized) =>
            new ApiException(HttpStatusCode.Unauthorized, errorCode, message);

        public static ApiException Forbidden(string message, string errorCode = ErrorCodes.Forbidden) =>
            new ApiException(HttpStatusCode.Forbidden, errorCode, message);

        public static ApiException NotFound(string message, string errorCode = ErrorCodes.ResourceNotFound) =>
            new ApiException(HttpStatusCode.NotFound, errorCode, message);

        public static ApiException Conflict(string message, string errorCode = ErrorCodes.Conflict) =>
            new ApiException(HttpStatusCode.Conflict, errorCode, message);

        public static ApiException InternalServerError(string message) =>
            new ApiException(HttpStatusCode.InternalServerError, ErrorCodes.InternalServerError, message);
    }
}
