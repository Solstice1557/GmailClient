namespace GmailClient.Controllers
{
    using System.Web.Http;

    using GmailClient.Model;
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
        public OperationResultModel Get(int page = 1, int count = 10)
        {
            if (page < 0 || count <= 0)
            {
                return new OperationResultModel(false, "Wrong page or count");
            }

            if (!this.emailManager.IsCorrect)
            {
                return new OperationResultModel(false, "Plase enter corrent gmail account and password.");
            }

            try
            {
                var result = this.emailManager.GetInboxEmailHeaders(page, count);
                return new OperationResultModel(true, data: result);
            }
            catch (EmailException e)
            {
                return new OperationResultModel(false, msg: e.Message);
            }
        }

        [HttpGet]
        public OperationResultModel Get(uint id)
        {
            if (!this.emailManager.IsCorrect)
            {
                return new OperationResultModel(false, "Plase enter corrent gmail account and password.");
            }

            try
            {
                var result = this.emailManager.GetEmail(id);
                return new OperationResultModel(true, data: result);
            }
            catch (EmailException e)
            {
                return new OperationResultModel(false, msg: e.Message);
            }
        }

        [HttpGet]
        public OperationResultModel Delete(uint id)
        {
            if (!this.emailManager.IsCorrect)
            {
                return new OperationResultModel(false, "Plase enter corrent gmail account and password.");
            }

            try
            {
                this.emailManager.Delete(id);
                return new OperationResultModel(true);
            }
            catch (EmailException e)
            {
                return new OperationResultModel(false, msg: e.Message);
            }
        }

        [HttpGet]
        public OperationResultModel Send(string to, string body, string subject)
        {
            if (!this.emailManager.IsCorrect)
            {
                return new OperationResultModel(false, "Plase enter corrent gmail account and password.");
            }

            try
            {
                this.emailManager.Send(to, subject, body);
                return new OperationResultModel(true);
            }
            catch (EmailException e)
            {
                return new OperationResultModel(false, msg: e.Message);
            }
        }
    }
}
