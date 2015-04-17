namespace GmailClient.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
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
            if (!this.IsCorrect)
            {
                throw new Exception("Bad username or password");
            }

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

        public Task<Email> GetEmailAsync(uint id)
        {
            return Task.Factory.StartNew(() => this.GetEmail(id));
        }

        public Email GetEmail(uint id)
        {
            if (!this.IsCorrect)
            {
                throw new Exception("Bad username or password");
            }

            using (var client = new ImapClient("imap.gmail.com", 993, this.user, this.password, AuthMethod.Login, true))
            {
                var msg = client.GetMessage(id, FetchOptions.Normal, mailbox: "inbox");
                return new Email(msg, id);
            }
        }

        public Task DeleteAsync(uint id)
        {
            return Task.Factory.StartNew(() => this.Delete(id));
        }

        public void Delete(uint id)
        {
            if (!this.IsCorrect)
            {
                throw new Exception("Bad username or password");
            }

            using (var client = new ImapClient("imap.gmail.com", 993, this.user, this.password, AuthMethod.Login, true))
            {
                client.DeleteMessage(id, mailbox: "inbox");
            }
        }

        public Task SendAsync(string to, string subject, string body)
        {
            if (!this.IsCorrect)
            {
                throw new Exception("Bad username or password");
            }

            return Task.Factory.StartNew(() => this.Send(to, subject, body));
        }

        public void Send(string to, string subject, string body)
        {
            if (!this.IsCorrect)
            {
                throw new Exception("Bad username or password");
            }

            if (string.IsNullOrEmpty(to))
            {
                throw new ArgumentException("No addresses");
            }

            var client = new SmtpClient("smtp.gmail.com", 587) { Credentials = new NetworkCredential(this.user, this.password), EnableSsl = true };

            var addresses = to.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => new MailAddress(s)).ToList();
            if (addresses.Count == 0)
            {
                throw new ArgumentException("No addresses");
            }

            var from = new MailAddress(this.user);
            var message = new MailMessage(from, addresses[0])
                          {
                              Body = body,
                              BodyEncoding = Encoding.UTF8,
                              Subject = subject,
                              SubjectEncoding = Encoding.UTF8,
                              IsBodyHtml = false
                          };

            if (addresses.Count > 0)
            {
                for (int i = 1; i < addresses.Count; i++)
                {
                    message.Bcc.Add(addresses[i]);
                }
            }

            client.Send(message);
        }
    }
}
