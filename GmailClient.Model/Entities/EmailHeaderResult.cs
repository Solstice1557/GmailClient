namespace GmailClient.Model.Entities
{
    using System.Collections.Generic;

    public class EmailHeaderResult
    {
        public List<EmailHeader> Mails { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int Total { get; set; }
    }
}
