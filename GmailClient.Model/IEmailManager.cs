namespace GmailClient.Model
{
    using System.Threading.Tasks;

    using GmailClient.Model.Entities;

    public interface IEmailManager
    {
        bool IsCorrect { get; }

        Task<EmailHeaderResult> GetInboxEmailHeadersAsync(int page, int pageSize);

        EmailHeaderResult GetInboxEmailHeaders(int page, int pageSize);

        Task<Email> GetEmailAsync(uint id);

        Email GetEmail(uint id);

        Task DeleteAsync(uint id);

        void Delete(uint id);

        Task SendAsync(string to, string subject, string body);

        void Send(string to, string subject, string body);
    }
}