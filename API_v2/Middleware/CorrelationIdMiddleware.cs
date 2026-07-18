using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace API_v2.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeaderKey = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderKey, out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            // Ensure the correlation ID is returned in the response
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderKey))
                {
                    context.Response.Headers.Append(CorrelationIdHeaderKey, correlationId);
                }
                return Task.CompletedTask;
            });

            // Push CorrelationId into Serilog LogContext for structured logging
            using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
            {
                await _next(context);
            }
        }
    }
}
