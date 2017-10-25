using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoGrid_VE : PaymentInfoGrid
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.divTIN.Visible = true;
        }

        public override bool ValidateAndGetPayments(Address_V01 shippingAddress, out List<Payment> payments, bool showErrors)
        {
            bool isValid = base.ValidateAndGetPayments(shippingAddress, out payments, showErrors);

            //List<Payment> currentPayments = GetPayments(shippingAddress);
            PaymentOptionChoice currentChoice = GetCurrentPaymentOption();
            if (currentChoice == PaymentOptionChoice.CreditCard)
            {

                if (string.IsNullOrEmpty(this.txtTIN.Text))
                {
                    isValid = false;
                    lblErrorMessages.Text = GetLocalResourceObject("NoTIN") as string;
                }
                else
                {
                    var validNumber = this.IsValidTIN(this.txtTIN.Text);
                    if (!validNumber)
                    {
                        isValid = validNumber;
                        lblErrorMessages.Text = GetLocalResourceObject("NotValidTIN") as string;
                    }
                    else
                    {
                        this.SessionInfo.NationaId = string.Format("{0}", this.txtTIN.Text);
                        SessionInfo.SetSessionInfo(DistributorID, Locale, this.SessionInfo);
                    }
                }
            }

            _paymentError = !isValid;

            if (_paymentError)
            {
                OnCardAddOrEdit(null, null);
            }
            UpdatePanel1.Update();
            return isValid;
        }

        /// <summary>
        /// Validate the TIN
        /// </summary>
        /// <param name="strTIN">10 digit or letters without special character</param>
        /// <returns>Validation flag</returns>
        private bool IsValidTIN(string strTIN)
        {
            if (string.IsNullOrEmpty(strTIN))
                return false;
            if (strTIN.Length > 10)
                return false;

            char[] myChars = strTIN.ToCharArray();
            foreach (char c in myChars)
            {
                if (!char.IsLetterOrDigit(c))
                    return false;
            }
            return true;
        }
        
    }
}