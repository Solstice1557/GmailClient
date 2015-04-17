namespace GmailClient.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GmailClient.Model.Entities;

    using S22.Imap;

    public class EmailManager : IEmailManager
    {
        private readonly string user;

        private readonly string password;

        public EmailManager(string user, string password)
        {
            this.user = user;
            this.password = password;
        }

        public bool IsCorrect
        {
            get
            {
                return !string.IsNullOrEmpty(this.user) && !string.IsNullOrEmpty(this.password);
            }
        }

        public Task<EmailHeaderResult> GetInboxEmailHeadersAsync(int page, int pageSize)
        {
            return Task.Factory.StartNew(() => this.GetInboxEmailHeaders(page, pageSize));
        }

        public EmailHeaderResult GetInboxEmailHeaders(int page, int pageSize)
        {
            using (var client = new ImapClient("imap.gmail.com", 993, this.user, this.password, AuthMethod.Login, true))
            {
                var uids = client.Search(SearchCondition.All(), "inbox").OrderByDescending(u => u).ToList();
                var total = uids.Count;
                uids = uids.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                var list = new List<EmailHeader>();
                foreach (var uid in uids)
                {
                    var msg = client.GetMessage(uid, FetchOptions.HeadersOnly, mailbox: "inbox");
                    list.Add(new EmailHeader(msg, uid));
                }

                return new EmailHeaderResult
                       {
                           Mails = list,
                           Page = page,
                           PageSize = pageSize,
                           Total = total
                       };
            }
        }

        public Email GetEmail(uint id)
        {
            using (var client = new ImapClient("imap.gmail.com", 993, this.user, this.password, AuthMethod.Login, true))
            {
                var msg = client.GetMessage(id, FetchOptions.Normal, mailbox: "inbox");
                return new Email(msg, id);
            }
        }

        public void Delete(uint id)
        {
            using (var client = new ImapClient("imap.gmail.com", 993, this.user, this.password, AuthMethod.Login, true))
            {
                client.DeleteMessage(id, mailbox: "inbox");
            }
        }
    }
}
