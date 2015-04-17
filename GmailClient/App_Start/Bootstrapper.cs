namespace GmailClient
{
    using System.Configuration;
    using System.Linq;
    using System.Web;
    using System.Web.Http.Dispatcher;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Mvc;

    using GmailClient.Controllers;
    using GmailClient.Model;

    /// <summary>
    /// IoC container initialization class
    /// </summary>
    public static class Bootstrapper
    {
        /// <summary>
        /// IoC container initialization
        /// </summary>
        /// <param name="container">IoC container</param>
        public static void InitContainer(WindsorContainer container)
        {
            container.Register(
                Component.For<IWindsorContainer>().Instance(container),
                Component.For<GmailClientContext>().LifestylePerWebRequest(),
                Component.For<MailController>().LifestylePerWebRequest(),
                Component.For<AccountController>().LifestylePerWebRequest(),
                Component.For<IHttpControllerActivator>().Instance(new WindsorControllerActivator(container.Kernel)),
                Component.For<IEmailManager>().ImplementedBy<EmailManager>().LifestylePerWebRequest().DynamicParameters(
                    (k, d) =>
                    {
                        if (HttpContext.Current.User.Identity.IsAuthenticated)
                        {
                            using (var db = container.Resolve<GmailClientContext>())
                            {
                                var user = db.Users.First(u => u.UserName == HttpContext.Current.User.Identity.Name);
                                d["user"] = user.GmailAccount;
                                d["password"] = user.GmailPassword;
                                d["smtpAddress"] = ConfigurationManager.AppSettings["smtpAddress"] ?? "smtp.gmail.com";
                                d["smtpPort"] = Utils.Utils.TryParseInt(ConfigurationManager.AppSettings["smtpPort"]) ?? 587;
                                d["imapAddress"] = ConfigurationManager.AppSettings["imapAddress"] ?? "imap.gmail.com";
                                d["imapPort"] = Utils.Utils.TryParseInt(ConfigurationManager.AppSettings["imapPort"]) ?? 993;
                            }
                        }
                    }));
        }
    }
}