namespace GmailClient.Model
{
    using System.Data.Entity;

    using GmailClient.Model.Entities;

    public class GmailClientContext : DbContext
    {
        public GmailClientContext()
            : base("DefaultConnection")
        {
        }

        public IDbSet<UserProfile> Users { get; set; }

        public static void Init()
        {
            using (var context = new GmailClientContext())
            {
                context.Database.CreateIfNotExists();
            }
        }
    }
}
