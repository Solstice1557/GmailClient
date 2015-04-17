namespace GmailClient
{
    using System.Linq;
    using System.Web;
    using System.Web.Http.Dispatcher;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Mvc;

    using GmailClient.Controllers;
    using GmailClient.Model;

    public static class Bootstrapper
    {
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
                            }
                        }
                    }));
        }
    }
}