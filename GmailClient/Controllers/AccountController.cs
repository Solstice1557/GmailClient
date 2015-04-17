namespace GmailClient.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using GmailClient.Model;
    using GmailClient.Models;

    using WebMatrix.WebData;

    [Authorize]
    public class AccountController : Controller
    {
        private readonly GmailClientContext dbContext;

        public AccountController(GmailClientContext dbContext)
        {
            this.dbContext = dbContext;
        }

        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                return this.RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            this.ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return this.View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return this.RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (this.ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                    WebSecurity.Login(model.UserName, model.Password);
                    return this.RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    this.ModelState.AddModelError(string.Empty, ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            this.ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : string.Empty;
            this.ViewBag.ReturnUrl = Url.Action("Manage");
            return this.View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
            bool changePasswordSucceeded;
            try
            {
                changePasswordSucceeded = WebSecurity.ChangePassword(this.User.Identity.Name, model.OldPassword, model.NewPassword);
            }
            catch (Exception)
            {
                changePasswordSucceeded = false;
            }

            if (changePasswordSucceeded)
            {
                return this.RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            this.ModelState.AddModelError(string.Empty, "The current password is incorrect or the new password is invalid.");

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        public string GetGmailAccount()
        {
            var user = this.dbContext.Users.FirstOrDefault(u => u.UserName == this.User.Identity.Name);
            return user != null ? (user.GmailAccount ?? string.Empty) : string.Empty;
        }

        public ActionResult SetGmailAccount(MailSettingsModel model)
        {
            if (string.IsNullOrEmpty(model.UserName))
            {
                return this.Json(new OperationResultModel(false, "Please enter account"));
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                return this.Json(new OperationResultModel(false, "Please enter password"));
            }

            var user = this.dbContext.Users.FirstOrDefault(u => u.UserName == this.User.Identity.Name);
            if (user == null)
            {
                return this.Json(new OperationResultModel(false, "User not found. Please register."));
            }

            user.GmailAccount = model.UserName;
            user.GmailPassword = model.Password;
            this.dbContext.SaveChanges();

            return this.Json(new OperationResultModel(true));
        }

        public ActionResult ResetGmailAccount()
        {
            var user = this.dbContext.Users.First(u => u.UserName == this.User.Identity.Name);
            user.GmailAccount = null;
            user.GmailPassword = null;
            this.dbContext.SaveChanges();

            return this.Json(new OperationResultModel(true));
        }


        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
