using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class MessageBox2 : System.Web.UI.UserControl
    {
        #region Constants
        protected const string AnnouncementKey = "ANNOUNCEMENT_";
        #endregion Constants

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void cbDontShowAgain_CheckedChanged(object sender, EventArgs e)
        {
            var AnnouncementKey = getCookieKey();
            if (cbDontShowAgain.Checked == true || cbDontShowAgain2.Checked == true)
            {
                var cookie = new HttpCookie(AnnouncementKey) { Value = "DontShowAgain" };

                cookie.Expires = DateTime.Now.AddYears(10);
                
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
            else
            {
                // remove cookies
                if (HttpContext.Current.Request.Cookies[AnnouncementKey] != null)
                {
                    var cookie = new HttpCookie(AnnouncementKey);
                    cookie.Domain = FormsAuthentication.CookieDomain;
                    cookie.Expires = DateTime.Now.AddYears(-11);
                    cookie.Path = "/";
                    HttpContext.Current.Response.Cookies.Add(cookie);
                }
            }
        }

        private string getCookieKey()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (member != null)
            {
                var user = member.Value;
                return string.Format("{0}{1}",AnnouncementKey,user.Id);
            }
            return AnnouncementKey;
        }

        public bool ShouldShowMessage()
        {
            return HLConfigManager.Configurations.DOConfiguration.ShowPopupOnPage &&  HttpContext.Current.Request.Cookies[getCookieKey()] == null;
        }

        public void ShowMessage()
        {
            if (ShouldShowMessage())
            {
                popup_MessageBox.Show();
                upMessageBox.Update();

                string Clientscript = "<script>hidePromoPopup();</script>";   //registramos el script 
                if (!Page.ClientScript.IsStartupScriptRegistered("promoPopup"))
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "promoPopup", Clientscript);  
            }
        }

        private void Hide()
        {
            txtTitle.Text = "";
            popup_MessageBox.Hide();
        }

        protected void OnYes(object sender, EventArgs e)
        {
            popup_MessageBox.Hide();
        }

        protected void OnNo(object sender, EventArgs e)
        {
            popup_MessageBox.Hide();
        }
    }
}