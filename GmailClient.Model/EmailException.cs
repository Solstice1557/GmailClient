namespace GmailClient.Model
{
    using System;

    public class EmailException : Exception
    {
        public EmailException(string message)
            : base(message)
        {
        }
    }
}
