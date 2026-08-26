namespace API_v2.Models.DTOs
{
    public class ApiErrorResponse
    {
        public bool Success { get; init; } = false;
        public string ErrorCode { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
        public IReadOnlyList<string>? Errors { get; init; }
    }
}
