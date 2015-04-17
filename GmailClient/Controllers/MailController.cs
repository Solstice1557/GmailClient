namespace GmailClient.Controllers
{
    using System.Web;
    using System.Web.Http;

    using GmailClient.Model;
    using GmailClient.Model.Entities;
    using GmailClient.Models;

    [System.Web.Mvc.Authorize]
    public class MailController : ApiController
    {
        private readonly IEmailManager emailManager;

        public MailController(IEmailManager emailManager)
        {
            this.emailManager = emailManager;
        }

        [HttpGet]
        public EmailHeaderResult Get(int page = 1, int count = 10)
        {
            if (page < 0 || count <= 0)
            {
                throw new HttpException(400, "Wrong page or count");
            }

            return this.emailManager.GetInboxEmailHeaders(page, count);
        }

        [HttpGet]
        public Email Get(uint id)
        {
            return this.emailManager.GetEmail(id);
        }

        [HttpDelete]
        public OperationResultModel Delete(uint id)
        {
            this.emailManager.Delete(id);
            return new OperationResultModel(true, string.Empty);
        }

        [HttpPost]
        public OperationResultModel Post(string to, string body, string subject)
        {
            return new OperationResultModel(true, string.Empty);
        }
    }
}
