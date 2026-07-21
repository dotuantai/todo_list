using System;
using System.Net;

namespace API_v2.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApiException(HttpStatusCode statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public static ApiException BadRequest(string message) =>
            new ApiException(HttpStatusCode.BadRequest, message);

        public static ApiException Unauthorized(string message) =>
            new ApiException(HttpStatusCode.Unauthorized, message);

        public static ApiException Forbidden(string message) =>
            new ApiException(HttpStatusCode.Forbidden, message);

        public static ApiException NotFound(string message) =>
            new ApiException(HttpStatusCode.NotFound, message);

        public static ApiException Conflict(string message) =>
            new ApiException(HttpStatusCode.Conflict, message);

        public static ApiException InternalServerError(string message) =>
            new ApiException(HttpStatusCode.InternalServerError, message);
    }
}
