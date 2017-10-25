using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public delegate void ValidatePinEventHandler(object sender, ValidatePinEventArgs e);

    public class ValidatePinEventArgs : EventArgs
    {
        public string MobilePin
        {
            get; set;
        }
    }

    public partial class OneTimePin : UserControlBase
    {
        public event ValidatePinEventHandler ValidatePinEvent;

        public string LastErrorMessage { get; set; }

        public string MobilePin
        {
            get
            {
                return txtOTP.Text;
            }
            set
            {
                txtOTP.Text = value;
            }
        }

        public string Token
        {
            get
            {
                if (ViewState["token"] == null)
                {
                    return "";
                }
                else
                {
                    return ViewState["token"].ToString();
                }
            }
            set
            {
                ViewState["token"] = value;
            }
        }

        public bool IsMobilePinProvided()
        {
            if (!string.IsNullOrEmpty(txtOTP.Text) && txtOTP.Text.Length == 6)
                return true;
            else
                return false;
        }

        public void RequestPin(string orderNumber, Order_V01 order, MyHLShoppingCart shoppingCart)
        {
            Token = "";
            var quickPayProvider = new CN_99BillQuickPayProvider();
            var isMobilePinRequested = quickPayProvider.RequestMobilePinForPurchase(orderNumber, order, shoppingCart);

            if (isMobilePinRequested)
            {
                Token = quickPayProvider.Token;
                OneTimePinPopupExtender.Show();
            }
            else
            {
                if (!String.IsNullOrEmpty(quickPayProvider.LastErrorMessage))
                {
                    LastErrorMessage = quickPayProvider.LastErrorMessage;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lblErrorMessage.Visible = false;
            lblErrorMessage.Text = "";
        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtOTP.Text) && txtOTP.Text.Length == 6)
            {
                if (ValidatePinEvent != null)
                {
                    ValidatePinEventArgs arg = new ValidatePinEventArgs();
                    arg.MobilePin = txtOTP.Text;
                    ValidatePinEvent(this, arg);
                }

                txtOTP.Text = "";
            }
            else
            {
                lblErrorMessage.Visible = true;
                OneTimePinPopupExtender.Show();
            }
        }
    }
}