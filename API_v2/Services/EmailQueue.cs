using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using API_v2.Services.Interfaces;

namespace API_v2.Services
{
    public class EmailQueue : IEmailQueue
    {
        private readonly Channel<EmailQueueItem> _channel;

        public EmailQueue()
        {
            var options = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            };
            _channel = Channel.CreateUnbounded<EmailQueueItem>(options);
        }

        public void QueueEmail(string toEmail, string subject, string body)
        {
            _channel.Writer.TryWrite(new EmailQueueItem
            {
                ToEmail = toEmail,
                Subject = subject,
                Body = body
            });
        }

        public ValueTask<EmailQueueItem> DequeueAsync(CancellationToken cancellationToken)
        {
            return _channel.Reader.ReadAsync(cancellationToken);
        }
    }
}
