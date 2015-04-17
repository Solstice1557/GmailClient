namespace GmailClient.Controllers
{
    using System.Web.Mvc;

    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Gmail client by Salavat Gaynetdinov.";
            return View();
        }
    }
}