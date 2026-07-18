using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace API_v2.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly int _slowRequestThresholdMs;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger, IConfiguration config)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _slowRequestThresholdMs = config.GetValue<int>("LoggingSettings:SlowRequestThresholdMs", 1000);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var request = context.Request;

            // Enable buffering and read request body safely
            var requestBody = string.Empty;
            if (request.ContentLength > 0)
            {
                request.EnableBuffering();
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // Reset stream position for model binders
                }
            }

            var path = request.Path;
            var method = request.Method;
            var query = request.QueryString.ToString();
            var ip = GetClientIp(context);
            var userAgent = request.Headers["User-Agent"].ToString();

            var user = "anonymous";
            var emailClaim = context.User?.FindFirst(ClaimTypes.Email);
            if (emailClaim != null)
            {
                user = emailClaim.Value;
            }
            else if (context.User?.Identity?.IsAuthenticated == true)
            {
                user = context.User.Identity.Name ?? "authenticated";
            }

            var maskedRequestBody = MaskSensitiveFields(requestBody);

            _logger.LogInformation(">>> REQUEST  {Method} {Path} | Query: {Query} | IP: {IP} | User: {User} | UA: {UA} | Body: {Body}", 
                method, path, query, ip, user, userAgent, maskedRequestBody);

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var elapsed = sw.ElapsedMilliseconds;
                var statusCode = context.Response.StatusCode;

                if (statusCode >= 500)
                {
                    _logger.LogError("<<< RESPONSE {Method} {Path} | Status: {Status} | Elapsed: {Elapsed}ms | User: {User}", 
                        method, path, statusCode, elapsed, user);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning("<<< RESPONSE {Method} {Path} | Status: {Status} | Elapsed: {Elapsed}ms | User: {User}", 
                        method, path, statusCode, elapsed, user);
                }
                else
                {
                    _logger.LogInformation("<<< RESPONSE {Method} {Path} | Status: {Status} | Elapsed: {Elapsed}ms | User: {User}", 
                        method, path, statusCode, elapsed, user);
                }

                // Check and log slow requests
                if (elapsed > _slowRequestThresholdMs)
                {
                    _logger.LogWarning("⚠️ SLOW REQUEST DETECTED: {Method} {Path} took {Elapsed}ms (threshold: {Threshold}ms)", 
                        method, path, elapsed, _slowRequestThresholdMs);
                }
            }
        }

        private string GetClientIp(HttpContext context)
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (ip == "::1") return "127.0.0.1";
            return ip ?? "unknown";
        }

        private static string MaskSensitiveFields(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return string.Empty;
            try
            {
                var node = JsonNode.Parse(body);
                if (node is JsonObject obj)
                {
                    MaskKeys(obj);
                    return obj.ToJsonString();
                }
                return body;
            }
            catch
            {
                return body;
            }
        }

        private static void MaskKeys(JsonObject obj)
        {
            var sensitiveKeys = new[] { "password", "token", "secret", "cardnumber", "cvv" };
            foreach (var property in obj.ToList())
            {
                var keyLower = property.Key.ToLower();
                if (sensitiveKeys.Contains(keyLower))
                {
                    obj[property.Key] = "***";
                }
                else if (property.Value is JsonObject childObj)
                {
                    MaskKeys(childObj);
                }
                else if (property.Value is JsonArray childArray)
                {
                    foreach (var element in childArray)
                    {
                        if (element is JsonObject arrayObj)
                        {
                            MaskKeys(arrayObj);
                        }
                    }
                }
            }
        }
    }
}
