using System;
using System.Web.UI;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class MessageBox : UserControl
    {
        public string Title { get; set; }
        public string Message { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        public void ShowMessage(string Message, string Caption)
        {
            txtMessage.InnerHtml = Message;
            contentReader.ContentPath = string.Empty;
            txtTitle.Text = Caption;
            popup_MessageBox.Show();
            upMessageBox.Update();
        }

        private void Hide()
        {
            txtMessage.InnerHtml = "";
            txtTitle.Text = "";
            contentReader.ContentPath = string.Empty;
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

        public void DisplayHtml(string fragmentName)
        {
            txtMessage.InnerHtml = string.Empty;
            contentReader.ContentPath = fragmentName;
            contentReader.LoadContent();
            txtTitle.Text = string.Empty;
            popup_MessageBox.Show();
            upMessageBox.Update();
        }
    }
}