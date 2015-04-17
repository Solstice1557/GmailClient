namespace GmailClient.Model.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    public class Email : EmailHeader
    {
        public Email()
        {
        }

        public Email(MailMessage mail, uint uid)
            : base(mail, uid)
        {
            this.Body = mail.Body;
            if (!mail.IsBodyHtml)
            {
                this.Body = this.Body.Replace("\n", "<br/>\n");
            }

            this.Attachments = mail.Attachments.Select(a => a.Name).ToList();
        }

        public string Body { get; set; }

        public List<string> Attachments { get; set; }
    }
}
