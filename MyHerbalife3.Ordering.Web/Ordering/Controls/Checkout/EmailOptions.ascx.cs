using System;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class EmailOptions : UserControlBase
    {
        public string SelectedEmail
        {
            get
            {
                if (!string.IsNullOrEmpty(txtEmail.Text))
                {
                    return txtEmail.Text;
                }
                else
                    return null;
            }
            set { txtEmail.Text = value; }
        }

        public bool IsReadOnly { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            txtEmail.Text = Email;
            ShoppingCart.EmailAddress = Email;

            var currentSession = SessionInfo;
            if (!String.IsNullOrEmpty(currentSession.ChangedEmail))
            {
                txtEmail.Text = currentSession.ChangedEmail;
                ShoppingCart.EmailAddress = currentSession.ChangedEmail;
            }

            lblShortEmail.Text = ShoppingCart.EmailAddress;
            txtLongEmailAddress.Text = ShoppingCart.EmailAddress;

            if (IsReadOnly)
            {
                dvEmailReadOnly.Visible = true;
                dvEmailEdit.Visible = false;
                if (ShoppingCart.EmailAddress.Length > 55)
                {
                    txtLongEmailAddress.Visible = true;
                    lblShortEmail.Visible = false;
                }
                else
                {
                    txtLongEmailAddress.Visible = false;
                    lblShortEmail.Visible = true;
                }
            }
            else
            {
                dvEmailReadOnly.Visible = false;
                dvEmailEdit.Visible = true;
            }
        }

        protected void tbEmailAddress_Changed(object sender, EventArgs e)
        {
            CheckEmail();
        }

        public bool CheckEmail()
        {
            lbEmailError.Visible = false;
            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail && String.IsNullOrEmpty(txtEmail.Text))
            {
                lbEmailError.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoEmailAddress");
                lbEmailError.Visible = true;
                return false;
            }

            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail && !isValidEmail(txtEmail.Text))
            {
                lbEmailError.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidEmailAddress");
                lbEmailError.Visible = true;
                return false;
            }

            if (Email.ToLower().Trim() != txtEmail.Text.ToLower().Trim())
            {
                SessionInfo.ChangedEmail = txtEmail.Text;
            }

            return true;
        }

        private bool isValidEmail(string inputEmail)
        {
            if (!string.IsNullOrEmpty(inputEmail))
            {
                string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
                //string strRegex = @"/^[-_a-z0-9\'+*$^&%=~!?{}]++(?:\.[-_a-z0-9\'+*$^&%=~!?{}]+)*+@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d++)?$/iD";
                var re = new Regex(strRegex);
                if (re.IsMatch(inputEmail))
                    return (true);
                else
                    return (false);
            }
            else
            {
                return true;
            }
        }
    }
}