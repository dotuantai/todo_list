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
        private const int MaxLogBodySizeBytes = 32 * 1024;
        private const string MaskedValue = "***MASKED***";
        private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "currentPassword",
            "newPassword",
            "temporaryPassword",
            "otp",
            "code",
            "token",
            "refreshToken",
            "accessToken",
            "idToken",
            "id_token",
            "access_token",
            "secret",
            "clientSecret",
            "cardNumber",
            "cvv"
        };

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

            var requestBody = await ReadRequestBodyForLoggingAsync(request);

            var path = request.Path;
            var method = request.Method;
            var query = request.QueryString.ToString();
            
            if (query.Contains("access_token=", StringComparison.OrdinalIgnoreCase))
            {
                query = System.Text.RegularExpressions.Regex.Replace(query, @"access_token=[^&]*", "access_token=***MASKED***");
            }

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

            _logger.LogInformation(">>> REQUEST  {Method} {Path} | Query: {Query} | IP: {IP} | User: {User} | UA: {UA} | Body: {Body}", 
                method, path, query, ip, user, userAgent, requestBody);

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
            var ip = context.Connection.RemoteIpAddress?.ToString();
            if (ip == "::1") return "127.0.0.1";
            return ip ?? "unknown";
        }

        private static async Task<string> ReadRequestBodyForLoggingAsync(HttpRequest request)
        {
            if (request.ContentLength == 0)
            {
                return "[Empty body]";
            }

            var contentType = request.ContentType ?? string.Empty;
            if (contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) ||
                contentType.StartsWith("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                return $"[Binary or multipart payload omitted - Length: {request.ContentLength ?? 0} bytes]";
            }

            if (request.Path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
            {
                return "[Sensitive authentication payload omitted]";
            }

            if (request.ContentLength is null)
            {
                return "[Payload with unknown length omitted]";
            }

            if (request.ContentLength > MaxLogBodySizeBytes)
            {
                return $"[Payload exceeds {MaxLogBodySizeBytes / 1024}KB log limit - Length: {request.ContentLength} bytes]";
            }

            var isJson = contentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ||
                         contentType.Contains("+json", StringComparison.OrdinalIgnoreCase);
            if (!isJson)
            {
                return $"[Non-JSON payload omitted - Content-Type: {contentType}]";
            }

            request.EnableBuffering();
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 4096,
                leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return MaskSensitiveFields(body);
        }

        private static string MaskSensitiveFields(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return string.Empty;
            try
            {
                var node = JsonNode.Parse(body);
                if (node is null) return "[Empty JSON body]";
                MaskNode(node);
                return node.ToJsonString();
            }
            catch
            {
                return "[Malformed JSON payload omitted]";
            }
        }

        private static void MaskNode(JsonNode node)
        {
            if (node is JsonObject obj)
            {
                foreach (var property in obj.ToList())
                {
                    if (SensitiveKeys.Contains(property.Key))
                    {
                        obj[property.Key] = MaskedValue;
                    }
                    else if (property.Value is not null)
                    {
                        MaskNode(property.Value);
                    }
                }
            }
            else if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is not null) MaskNode(item);
                }
            }
        }
    }
}
