namespace API_v2.Services.Interfaces
{
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body);
    }
}
