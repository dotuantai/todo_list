using System.Threading;
using System.Threading.Tasks;

namespace API_v2.Services.Interfaces
{
    public class EmailQueueItem
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
    }

    public interface IEmailQueue
    {
        void QueueEmail(string toEmail, string subject, string body);
        ValueTask<EmailQueueItem> DequeueAsync(CancellationToken cancellationToken);
    }
}
