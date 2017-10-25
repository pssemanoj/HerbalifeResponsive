#region

using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;

#endregion

namespace MyHerbalife3.Ordering.Controllers
{
    public class OrderingAuthController : Controller
    {
       
        public bool Logon(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                return LogonDeniedResult("Login Error");
            }

            // ensure all subsequent usage of username are uppercase.
            userName = userName.ToUpper(CultureInfo.InvariantCulture);

            //if (!Membership.ValidateUser(userName, password))
            //{
            //    return LogonDeniedResult("Login Error");
            //}

            return LogonMemberImplementation(userName);
        }


        internal bool LogonMemberImplementation(string userName)
        {
            var membershipUser = (Membership.GetUser(userName)) as MembershipUser<DistributorProfileModel>;

            if (membershipUser == null || membershipUser.Value == null)
            {
                return LogonDeniedResult("LoginError");
            }
            FormsAuthentication.SetAuthCookie(userName, false);
            return true;
        }

        private string GetPostAuthUrl()
        {
            const string redirectTo = "/Ordering/Catalog.aspx";
            return redirectTo;
        }

        private bool LogonDeniedResult(string reasonText)
        {
            ModelState.AddModelError("LoginError", reasonText);
            TempData["LoginError"] = reasonText;
            FormsAuthentication.SignOut();
            return false;
        }
    }
}