using System;
using System.Globalization;
using System.Web.Security;
using System.Web.UI;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Shared.ViewModel;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PopUpMessage : System.Web.UI.UserControl
    {
        public string Title { get; set; }
        public string Message { get; set; }
        
        
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public void ShowMessage(string Message, string Caption)
        {
            txtMessage.InnerHtml = Message;
            txtTitle.Text = Caption;
            popup_MessageBox.Show();
            upMessageBox.Update();
        }

        private void Hide()
        {
            txtMessage.InnerHtml = "";
            txtTitle.Text = "";
            popup_MessageBox.Hide();
        }

        protected void OnYes(object sender, EventArgs e)
        {
            popup_MessageBox.Hide();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var currentSessionInfo = SessionInfo.GetSessionInfo(member.Value.Id, CultureInfo.CurrentCulture.Name);
            currentSessionInfo.TrainingBreached = true;
        }

        protected void OnNo(object sender, EventArgs e)
        {
            popup_MessageBox.Hide();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var currentSessionInfo = SessionInfo.GetSessionInfo(member.Value.Id, CultureInfo.CurrentCulture.Name);
            currentSessionInfo.TrainingBreached = false;
        }
    }
}