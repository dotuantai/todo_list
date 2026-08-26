namespace API_v2.Models.Constants
{
    public static class ErrorCodes
    {
        public const string InvalidCredentials = "B13-01";
        public const string Unauthorized = "B13-02";
        public const string Forbidden = "B13-03";
        public const string ValidationFailed = "B13-04";
        public const string ResourceNotFound = "B13-05";
        public const string Conflict = "B13-06";
        public const string OtpInvalid = "B13-07";
        public const string ProjectNotFound = "B14-01";
        public const string ProjectForbidden = "B14-02";
        public const string TaskNotFound = "B15-01";
        public const string FileNotFound = "B16-01";
        public const string FolderNotFound = "B16-02";
        public const string InternalServerError = "B99-01";
    }
}
