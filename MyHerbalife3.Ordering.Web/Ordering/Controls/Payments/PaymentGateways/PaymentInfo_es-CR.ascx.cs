using System;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers.Payments;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_CR : PaymentGatewayControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblError.Text = string.Empty;
            if (!IsPostBack)
            {
                SetExpirationYears();
            }

            this.lnkCVVHelp.NavigateUrl = string.Concat(@"/content/", (Page as ProductsBase).Locale, @"/pdf/ordering/cvvhelp.pdf");
        }

        public override bool HasCardData
        {
            get { return true;}
        }

        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            bool isValid = false;

            if (!Settings.GetRequiredAppSetting<bool>("TokenizationDisabled", false)) 
            {
                return true;
            }

            isValid = ValidateCreditCardInfo(ref errorMessage);

            //if (isValid)
            //{
            //    GetPaymentInfo();
            //}

            return isValid;
        }

        public override Payment GetPaymentInfo()
        {
            CreditPayment_V01 payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            payment.Card.AccountNumber = txtCardNumber.Text.ToString().Trim();
            payment.Card.CVV = txtCVV.Text.ToString().Trim();
            payment.Card.Expiration = convertExpDateToDatetimeFormat();
            string payCode = ddlCards.SelectedValue;
            if (!string.IsNullOrEmpty(payCode))
            {
                payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
            }

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }

        /// <summary>Create the Card Expiration date from the dropdown choices</summary>
        /// <returns></returns>
        private DateTime convertExpDateToDatetimeFormat()
        {
            return DateTimeUtils.GetLastDayOfMonth(new DateTime(int.Parse(ddlExpYear.SelectedValue), int.Parse(ddlExpMonth.SelectedValue), 1));

            //int month, year ;
            //int.TryParse(ddlExpMonth.SelectedItem.Text, out month);
            //int.TryParse(ddlExpYear.SelectedItem.Text, out year);

            //if (month == 0 || year == 0)
            //{
            //    LoggerHelper.WriteInfo(string.Format("PaymentInfo_es_CR.convertExpDateToDatetimeFormat - Received Invalid Month or Year. Month - {0}, Year - {1}", ddlExpMonth.SelectedItem.Text, ddlExpYear.SelectedItem.Text), "Checkout");
            //}

            //month = month == 0 ? 0 : month;
            //year = year == 0 ? 0 : year;

            //return DateTimeUtils.GetLastDayOfMonth(new DateTime(year, month, 1));
        }

        private void SetExpirationYears()
        {
            ddlExpYear.Items.Clear();
            int year = DateTime.Now.Year;
            //int century = 2000; //good for next 88 years
            for (int i = year; i < year + 16; i++)
            {
                ddlExpYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
            }
        }

        private bool ValidateCreditCardInfo(ref string errorMessage)
        {
            bool isValid = true;
            int month, year;
            string cardType = ddlCards.SelectedValue;
            string ccNumber = txtCardNumber.Text.ToString().Trim();
            string cvv = txtCVV.Text.ToString().Trim();

            // check credit card number length
            if(string.IsNullOrEmpty(ccNumber) || ccNumber.Length < 15)
            {
                errorMessage = GetLocalResourceObject("InvalidCreditCard") as string;
                return false;
            }

            // validate credit card number using regular expression
            if (!ValidateCreditCardNumber(cardType, ccNumber))
            {
                errorMessage = GetLocalResourceObject("InvalidCreditCard") as string;
                return false;
            }

            if (!int.TryParse(ddlExpYear.SelectedValue, out year))
            {
                LoggerHelper.Info(string.Format("PaymentInfo_es_CR.ValidateCreditCardInfo - Invalid Year - {0}", ddlExpYear.SelectedValue));
                errorMessage = GetLocalResourceObject("InvalidExpirationDate") as string;
                return false;
            }

            if (!int.TryParse(ddlExpMonth.SelectedValue, out month))
            {
                LoggerHelper.Info(string.Format("PaymentInfo_es_CR.ValidateCreditCardInfo - Invalid Month - {0}", ddlExpYear.SelectedValue));
                errorMessage = GetLocalResourceObject("InvalidExpirationDate") as string;
                return false;
            }

            // validate expiration date. Assumption: load the Year drop down by code and won't be earlier than this year
            if (Convert.ToInt32(ddlExpYear.SelectedValue) == DateTime.Now.Year)
            {
                if (month < DateTime.Now.Month)
                {
                    errorMessage = GetLocalResourceObject("InvalidExpirationDate") as string;
                    return false;
                }
            }

            // check CVV
            switch (cardType)
            {
                case "VI":
                case "MC":
                    if (txtCVV.Text.ToString().Trim().Length != 3)
                    {
                        errorMessage = GetLocalResourceObject("InvalidCVV") as string;
                        return false;
                    }
                    break;
                case "AX":
                    if (txtCVV.Text.ToString().Trim().Length != 4)
                    {
                        errorMessage = GetLocalResourceObject("InvalidCVV") as string;
                        return false;
                    }
                    break;
                default:
                    break;
            }

            // 3. validate expiration date
            return isValid;
        }

        private bool ValidateCreditCardNumber(string cardType, string creditCardNumber)
        {
            string ccRegEx = string.Empty;
            
            //@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$";
            switch(cardType)
            {
                case "VI":
                    ccRegEx = @"^(?:4[0-9]{12}(?:[0-9]{3})?)$";
                    break;
                case "MC":
                    ccRegEx = @"^5[1-5][0-9]{14}$";
                    break;
                case "AX":
                    ccRegEx = @"^3[47][0-9]{13}$";
                    break;
                default:
                    ccRegEx = string.Empty;
                    break;
            }

            Regex reg = new Regex(ccRegEx, RegexOptions.IgnoreCase);
            Match mat = reg.Match(creditCardNumber);

            return mat.Success;
        }

    }
}