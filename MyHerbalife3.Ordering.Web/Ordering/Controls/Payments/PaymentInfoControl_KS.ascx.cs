using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PaymentInfoControl_KS : UserControlBase
    {
        protected int BCPoints = 0;
        private PaymentInfoGrid_KS _parent;
        private CreditPayment _paymentInfo;
        protected int amount = 0;
        protected int cardCode = 0;
        protected int installments = 0;
        protected string name = string.Empty;
        protected string refreshCommand = string.Empty;
        protected string sourceUrl = string.Empty;

        public CreditPayment PaymentInfo
        {
            get { return _paymentInfo; }
            set
            {
                _paymentInfo = value;
                SetupPaymentChoices();
                DisplayCard(false);
            }
        }

        public PaymentInfoGrid_KS Parent
        {
            set { _parent = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //lblCreditCardMessage.Text = string.Empty;
            if (!IsPostBack)
            {
                SetupPaymentChoices();
            }
            else
            {
                theFrame.Text = string.Empty;
            }
        }

        public void DisplayCard(bool IsFinalAMount)
        {
            if (null != _paymentInfo)
            {
                txtId.Text = _paymentInfo.LineID;
                bool hideFields = (null != _paymentInfo && !string.IsNullOrEmpty(_paymentInfo.AuthorizationCode));
                if (hideFields)
                {
                    lblStaticPaymentMethodText.Text = _paymentInfo.Card.IssuingBankID;
                    lblStaticAmountText.Text =
                        string.Concat(HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol, " ",
                                      Convert.ToInt32(_paymentInfo.Amount).ToString());
                    dvCreditCardInfo.Style["display"] = "none";
                    dvCreditCardData.Style["display"] = "block";
                }
                else
                {
                    txtAmount.Text = Convert.ToInt32(_paymentInfo.Amount).ToString();
                    dvCreditCardInfo.Style["display"] = "block";
                    dvCreditCardData.Style["display"] = "none";
                    txtAmount.ReadOnly = IsFinalAMount;
                }

                btnContinue.Visible = !hideFields;
            }
        }

        public void FlagAsOverage()
        {
            lblStaticPaymentMethodText.ForeColor = Color.Red;
            lblStaticAmountText.ForeColor = Color.Red;
            lblStaticPaymentMethod.ForeColor = Color.Red;
            lblStaticAmount.ForeColor = Color.Red;
            //lblCreditCardMessage.Text = GetLocalResourceObject("MsgCardTotalExceedsOrderTotal").ToString();
            lblMessage.Text = GetLocalResourceObject("MsgCardTotalExceedsOrderTotal").ToString();
        }

        protected void btnContinue_Click(object sender, EventArgs e)
        {
            bool valid = true;
            string cardType = (ddlCardType.SelectedValue.Contains("x")) ? "x" : "i";
            name = txtCardholderName.Text;
            Int32.TryParse(ddlCardType.SelectedValue.Replace(cardType, string.Empty), out cardCode);
            Int32.TryParse(txtAmount.Text, out amount);
            Int32.TryParse(ddlInstallments.SelectedValue, out installments);
            Int32.TryParse(ddlBCPoint.SelectedValue, out BCPoints);
            valid = amount <= (ShoppingCart.Totals as OrderTotals_V01).AmountDue;
            if (valid)
            {
                var info = new KSAuthInfo(amount, cardCode, installments, BCPoints, name);
                Session[KSAuthInfo.Key] = info;

                if (cardType == "x")
                {
                    sourceUrl = "Controls/Payments/KsNet/MPIAuth.aspx";
                }
                else
                {
                    sourceUrl = "Controls/Payments/KsNet/ISPAuth.aspx";
                }

                theFrame.Text =
                    "<iframe runat=\"server\" id=\"AuthFrame\" name=\"AuthFrame\" style=\"display: none\" src=\"" +
                    sourceUrl + "\"></iframe>";
            }
        }

        private void SetupPaymentChoices()
        {
            string cardType = (ddlCardType.SelectedValue.Contains("x")) ? "x" : "i";
            int selection = int.Parse(ddlCardType.SelectedValue.Replace(cardType, string.Empty));
            if (selection == 0)
            {
                paymentMethod.Style[HtmlTextWriterStyle.Display] = "none";
                ispBCTopPoints.Style[HtmlTextWriterStyle.Display] = "none";
                btnContinue.Enabled = false;
            }
            else if (cardType == "x")
            {
                paymentMethod.Style[HtmlTextWriterStyle.Display] = string.Empty;
                ispBCTopPoints.Style[HtmlTextWriterStyle.Display] = "none";
                txtPaymentMethod.Text = GetLocalResourceObject("XMPI").ToString();
                txtPaymentMethodType.Text = "XMPI";
            }
            else
            {
                paymentMethod.Style[HtmlTextWriterStyle.Display] = string.Empty;
                ispBCTopPoints.Style[HtmlTextWriterStyle.Display] = string.Empty;
                txtPaymentMethod.Text = GetLocalResourceObject("ISP").ToString();
                txtPaymentMethodType.Text = "ISP";
            }

            btnContinue.Enabled = (selection > 0);

            CheckInstallmentChoice();
        }

        private void CheckInstallmentChoice()
        {
            bool isp = false;
            if (txtPaymentMethodType.Text == "ISP")
            {
                isp = true;
            }

            //bool business = false;
            bool fiftyplus = false;
            int aNum = 0;
            if (Int32.TryParse(txtAmount.Text, out aNum))
            {
                if (aNum > 50000)
                {
                    fiftyplus = true;
                }
            }

            bool shinhan = (ddlCardType.SelectedValue == "7i");

            //XMPI
            //if business OR less than 50000 = 'Lump Sum' only installment option
            //if personal AND more than 50000 = ALL (1-12) installment options.
            //if personal AND more than 50000 AND Shin Han (provider) = LIMITED (1-6) installment options.
            // This naming is incorrect - it is not shinhan - it is Citi Card

            //ISP
            //if more than 50000 = ALL (1-12) installment options.

            string cardType = (ddlCardType.SelectedValue.Contains("x")) ? "x" : "i";
            int selection = int.Parse(ddlCardType.SelectedValue.Replace(cardType, string.Empty));
            int installmentPeriod = (selection > 0) ? ddlInstallments.SelectedIndex : 0;
            int allowedInstallments = (fiftyplus) ? 12 : 0;

            if (!isp && fiftyplus && shinhan)
            {
                if (installmentPeriod > 6)
                {
                    //lblCreditCardMessage.Text = GetLocalResourceObject("ValidateCardLimitedOptions") as string;
                    installmentPeriod = 5;
                }
                allowedInstallments = 6;
            }

            SetInstallmentSelection(allowedInstallments, installmentPeriod);
        }

        private void SetInstallmentSelection(int allowedInstallments, int installmentPeriod)
        {
            ddlInstallments.Items.Clear();
            ddlInstallments.Items.Add(new ListItem(GetLocalResourceObject("LumpSum").ToString(), "00"));

            switch (allowedInstallments)
            {
                case 0:
                    {
                        break;
                    }
                case 6:
                    {
                        ddlInstallments.Items.AddRange(new[]
                            {
                                new ListItem(GetLocalResourceObject("Installments_2").ToString(), "02"),
                                new ListItem(GetLocalResourceObject("Installments_3").ToString(), "03"),
                                new ListItem(GetLocalResourceObject("Installments_4").ToString(), "04"),
                                new ListItem(GetLocalResourceObject("Installments_5").ToString(), "05"),
                                new ListItem(GetLocalResourceObject("Installments_6").ToString(), "06")
                            });
                        break;
                    }
                default:
                    {
                        ddlInstallments.Items.AddRange(new[]
                            {
                                new ListItem(GetLocalResourceObject("Installments_2").ToString(), "02"),
                                new ListItem(GetLocalResourceObject("Installments_3").ToString(), "03"),
                                new ListItem(GetLocalResourceObject("Installments_4").ToString(), "04"),
                                new ListItem(GetLocalResourceObject("Installments_5").ToString(), "05"),
                                new ListItem(GetLocalResourceObject("Installments_6").ToString(), "06"),
                                new ListItem(GetLocalResourceObject("Installments_7").ToString(), "07"),
                                new ListItem(GetLocalResourceObject("Installments_8").ToString(), "08"),
                                new ListItem(GetLocalResourceObject("Installments_9").ToString(), "09"),
                                new ListItem(GetLocalResourceObject("Installments_10").ToString(), "10"),
                                new ListItem(GetLocalResourceObject("Installments_11").ToString(), "11"),
                                new ListItem(GetLocalResourceObject("Installments_12").ToString(), "12")
                            });
                        break;
                    }
            }
            if (ddlInstallments.Items.Count >= installmentPeriod)
            {
                ddlInstallments.Items[installmentPeriod].Selected = true;
            }
        }

        protected void ddlCardType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetupPaymentChoices();
        }

        protected void ddlPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckInstallmentChoice();
        }

        protected void ddlInstallmentOption_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckInstallmentChoice();
        }

        protected void ddlInstallments_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckInstallmentChoice();
        }

        protected void ClearCard(object sender, EventArgs e)
        {
            _parent.ClearCards(this, e);
        }
    }
}