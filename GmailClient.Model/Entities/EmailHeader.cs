namespace GmailClient.Model.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    /// <summary>
    /// The email header.
    /// </summary>
    public class EmailHeader
    {
        public EmailHeader()
        {
        }

        public EmailHeader(MailMessage mail, uint uid)
        {
            this.Uid = uid;
            this.Subject = mail.Subject;
            this.From = mail.From.Address;
            this.To = GetMailAddresesString(mail.To);
            var dateStr = mail.Headers["Date"];
            DateTime date;
            if (!string.IsNullOrEmpty(dateStr) && DateTime.TryParse(dateStr, out date))
            {
                this.Date = date;
            }
        }

        public uint Uid { get; set; }

        public string Subject { get; set; }

        public string From { get; set; } 

        public DateTime? Date { get; set; }

        public string To { get; set; }

        private static string GetMailAddresesString(IEnumerable<MailAddress> addresses)
        {
            return addresses.Aggregate(string.Empty, (s, m) => string.IsNullOrEmpty(s) ? m.Address : string.Format("{0}, {1}", s, m.Address));
        }
    }
}