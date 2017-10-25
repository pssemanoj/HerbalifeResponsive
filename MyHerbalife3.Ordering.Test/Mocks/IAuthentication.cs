using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace MyHerbalife3.Ordering.Test.Mocks
{
    interface IAuthentication
    {
        void Login(string username);
        void Logout();
    }

    class FormsAuth : IAuthentication
    {
        public void Login(string username)
        {
            MembershipUser user = Membership.GetUser(username);
            GenericIdentity identity = new GenericIdentity(user.UserName);
            RolePrincipal principal = new RolePrincipal(identity);
            System.Threading.Thread.CurrentPrincipal = principal;
            HttpContext.Current.User = principal;

            FormsAuthentication.SetAuthCookie(username, false);
        }

        public void Logout()
        {
            FormsAuthentication.SignOut();
        }
    }

}
