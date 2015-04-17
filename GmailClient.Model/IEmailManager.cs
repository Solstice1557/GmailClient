namespace GmailClient.Model
{
    using System.Threading.Tasks;

    using GmailClient.Model.Entities;

    public interface IEmailManager
    {
        bool IsCorrect { get; }

        Task<EmailHeaderResult> GetInboxEmailHeadersAsync(int page, int pageSize);

        EmailHeaderResult GetInboxEmailHeaders(int page, int pageSize);

        Email GetEmail(uint uid);

        void Delete(uint id);
    }
}