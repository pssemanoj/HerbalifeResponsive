using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using AjaxControlToolkit;
using HL.Common.EventHandling;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    /// <summary>The grid for displaying and managing Payment methods</summary>
    public partial class PaymentInfoGrid_KS_Non3D : PaymentInfoGrid
    {
        #region Constants
        
        #endregion

        #region Fields
        private bool _allowDecimal;
        private bool _enableInstallments = true;
        private OrderTotals_V01 _orderTotals;
        private int _maxCVV = 2;
        private string _currencySymbol = string.Empty;
        #endregion

        #region PaymentInfoBase interface implementation

        public override List<Payment> GetPayments(ServiceProvider.OrderSvc.Address_V01 shippingAddress)
        {
            var paymentList = new List<Payment>();
            CheckForPaymentGatewayResponse();
            var choice = GetCurrentPaymentOption();
            switch (choice)
            {
                case PaymentOptionChoice.DirectDeposit:
                case PaymentOptionChoice.WireTransfer:
                case PaymentOptionChoice.PaymentGateway:
                    {
                        paymentList.Add(CreateDummyPayment(shippingAddress));
                        break;
                    }
                case PaymentOptionChoice.CreditCard:
                    {
                        var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);

                        foreach (GridViewRow row in gridViewCardInfo.Rows)
                        {
                            var cardType = row.FindControl("lblCardType") as Label;
                            var lblIssueNumber = row.FindControl("lblIssueNumber") as TextBox;
                            var txtCVV = row.FindControl("txtCVV") as TextBox;
                            var txtAmount = row.FindControl("txtAmount") as TextBox;
                            var id = row.FindControl("cardID") as TextBox;
                            var payOption = row.FindControl("txtOption") as TextBox;
                            var choice1 = row.FindControl("txtChoice1") as TextBox;
                            var choice2 = row.FindControl("txtChoice2") as TextBox;
                            var ddInstallments = (DropDownList)row.FindControl("drpInstallments");
                            var currentOptions = row.FindControl("lnkPaymentOptions") as LinkButton;
                            int cardID = int.Parse(id.Text);
                            var ddBCPoints = row.FindControl("drpUsePoints") as DropDownList;

                            decimal cardAmount;
                            if (decimal.TryParse(txtAmount.Text, out cardAmount) && cardID > 0)
                            {
                                var info = paymentInfo.Find(p => p.ID == cardID);

                                var thePayment = new CreditPayment();
                                if(info.CardType.Trim() == "국민카드" || info.CardType.Trim() == "비씨카드")
                                {
                                    var payment = new KoreaISPPayment_V01();
                                    payment.Amount = cardAmount;
                                    payment.AuthorizationMethod =  AuthorizationMethodType.Online;
                                    payment.Card = new CreditCard();
                                    payment.Card.AccountNumber = info.CardNumber;
                                    payment.Card.NameOnCard = (info.CardHolder.First.Trim() + " " + info.CardHolder.Last.Trim()).Trim();
                                    payment.Card.Expiration = info.Expiration;
                                    payment.Card.CVV = txtCVV.Text.Trim();
                                    payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                                    payment.Address = (null != info.BillingAddress) ? info.BillingAddress : shippingAddress;
                                    payment.BCTopPoints = info.CardType.Trim() == "국민카드" ? int.Parse(ddBCPoints.SelectedValue) : 0;
                                    var options = new PaymentOptions_V01();
                                    options.NumberOfInstallments = int.Parse(ddInstallments.SelectedValue) + payment.BCTopPoints;
                                    payment.PaymentOptions = options;
                                    payment.KvpEncryptedData = info.IssueNumber;
                                    payment.Card.IssuingBankID = info.CardType;
                                    payment.KvpSessionKey = "FA";

                                    thePayment = payment;
                                }
                                else
                                {
                                    var payment = new KoreaMPIPayment_V01();
                                    payment.Amount = cardAmount;
                                    payment.AuthorizationMethod = AuthorizationMethodType.Online;
                                    payment.Card = new CreditCard();
                                    payment.Card.AccountNumber = info.CardNumber;
                                    payment.Card.NameOnCard = (info.CardHolder.First.Trim() + " " + info.CardHolder.Last.Trim()).Trim();
                                    payment.Card.Expiration = info.Expiration;
                                    payment.Card.CVV = txtCVV.Text.Trim();
                                    payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                                    payment.Address = (null != info.BillingAddress) ? info.BillingAddress : shippingAddress;
                                    var options = new PaymentOptions_V01();
                                    options.NumberOfInstallments = int.Parse(ddInstallments.SelectedValue);
                                    payment.PaymentOptions = options;
                                    payment.CAVV = info.IssueNumber;
                                    payment.Card.IssuingBankID = info.CardType;
                                    payment.ECI = "";
                                    payment.XID = "FA";

                                    thePayment = payment;
                                }
                                
                                thePayment.ReferenceID = currentOptions.Text; 
                                paymentList.Add(thePayment);
                            }
                        }
                        break;
                    }
            }

            string currentKey = PaymentsConfiguration.GetCurrentPaymentSessionKey(_locale, _distributorId);
            Session[currentKey] = paymentList;
            return paymentList;
        }

        /// <summary>Hook up client events for the data entry controls and resolve Payment Options</summary>
        protected override void SetupGridRows()
        {
            //this.Page.Form.SubmitDisabledControls = true; // !_allowMultipleCardsInTransaction;
            var paymentInfo = GetCurrentPaymentInformation(_locale, _distributorId);
            int id = 0;
            int counter = 0;
            foreach (GridViewRow row in gridViewCardInfo.Rows)
            {
                var txtAmount = row.FindControl("txtAmount") as TextBox;
                var txtCVV = row.FindControl("txtCVV") as TextBox;
                var txtId = row.FindControl("cardID") as TextBox;
                int.TryParse(txtId.Text, out id);
                var ddlCards = row.FindControl("ddlCards") as NostalgicDropDownList;

                if (IsPostBack)
                {
                    SetupGridRow(row, (!string.IsNullOrEmpty(ddlCards.SelectedValue)));
                }
                txtCVV.MaxLength = 2;
                _orderTotals = GetTotals();
                if (txtAmount != null && _orderTotals != null)
                {
                    if (counter == 0 && string.IsNullOrEmpty(txtAmount.Text))
                    {
                        txtAmount.Text = _orderTotals.AmountDue.ToString();
                    }

                    txtAmount.Attributes["onblur"] = string.Format("AmountLosingFocus(event,this,'{0}', {1})", string.Format("{0:F2}", _orderTotals.AmountDue), _allowDecimal ? 1 : 0);
                    txtAmount.Attributes["onkeypress"] = string.Format("checkAmount(event,this,{0},'{1}')", _allowDecimal ? 1 : 0, string.Format("{0:F2}", _orderTotals.AmountDue));
                }
                if (txtCVV != null && _orderTotals != null)
                {
                    txtCVV.Attributes["onkeypress"] = string.Format("CVVKeyPress(event,this,'{0}',{1})", string.Format("{0:F2}", _orderTotals.AmountDue), _maxCVV);
                }

                if (null != paymentInfo && paymentInfo.Count > 0 && id > 0)
                {
                    var po = paymentInfo.Find(p => p.ID == id);
                    if (null != po)
                    {
                        SetPaymentOptions(po, row);
                    }
                }

                // display installment options according to amount
                if (null != txtAmount)
                {
                    bool fiftyplus = false;
                    int aNum = 0;
                    if (Int32.TryParse(txtAmount.Text, out aNum))
                    {
                        if (aNum > 50000)
                        {
                            fiftyplus = true;
                        }
                    }

                    var ddlInstallments = row.FindControl("drpInstallments") as DropDownList;

                    if (ddlInstallments.Items.Count == 0) // first time to load, no item
                    {
                        ddlInstallments.Items.Add(new ListItem("일시불", "00"));
                        ddlInstallments.Enabled = false;
                        ddlInstallments.Visible = true;
                    }
                    else if (ddlInstallments.Items.Count == 1) // not first time, with only LumpSum
                    {
                        if (fiftyplus)
                        {
                            ddlInstallments.Items.Add(new ListItem("2개월", "02"));
                            ddlInstallments.Items.Add(new ListItem("3개월", "03"));
                            ddlInstallments.Items.Add(new ListItem("4개월", "04"));
                            ddlInstallments.Items.Add(new ListItem("5개월", "05"));
                            ddlInstallments.Items.Add(new ListItem("6개월", "06"));
                            ddlInstallments.Items.Add(new ListItem("7개월", "07"));
                            ddlInstallments.Items.Add(new ListItem("8개월", "08"));
                            ddlInstallments.Items.Add(new ListItem("9개월", "09"));
                            ddlInstallments.Items.Add(new ListItem("10개월", "10"));
                            ddlInstallments.Items.Add(new ListItem("11개월", "11"));
                            ddlInstallments.Items.Add(new ListItem("12개월", "12"));

                            ddlInstallments.Enabled = true;
                        }
                    }
                    else if (ddlInstallments.Items.Count == 12) // not first time, alreay load 12 installment options
                    {
                        if (!fiftyplus)
                        {
                            ddlInstallments.Items.Clear();
                            ddlInstallments.Items.Add(new ListItem("일시불", "00"));
                            ddlInstallments.Enabled = false;
                            ddlInstallments.Visible = true;
                        }
                    }
                }

                counter++;
            }

            if (null != _PaymentsList && _PaymentsList.Count > 0 && null != _CurrentPaymentsList && _CurrentPaymentsList.Count > 1)
            {
                pnlTotalDue.Style.Remove("display");
            }
            else
            {
                pnlTotalDue.Style.Add("display", "none");
            }

            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "PGControlNamingPrefix", string.Format(ScriptBlock, string.Concat("var pgPrefix = '", txtGrandTotal.ClientID.Replace(txtGrandTotal.ID, string.Empty), "';")));
            Page.ClientScript.RegisterClientScriptBlock(typeof(string), "CurrencySymbol", string.Format(ScriptBlock, string.Concat("var currencySymbol = '", _currencySymbol, "';")));
        }

        #endregion PaymentInfoBase interface implementation

        #region Grid Events

        /// <summary>Even hook for new grid row - used to configure the Payment Options</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected new void gridViewCardInfo_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            switch (e.Row.RowType)
            {
                case DataControlRowType.DataRow:
                    {
                        var cardId = e.Row.FindControl("cardID") as TextBox;
                        var cardType = e.Row.FindControl("lblCardType") as Label;
                        var cvv = e.Row.FindControl("txtCVV") as TextBox;
                        var cards = e.Row.FindControl("ddlCards") as DropDownList;

                        // Here

                        if (null != cards)
                        {
                            var pi = e.Row.DataItem as PaymentInformation;

                            if (HLConfigManager.Configurations.PaymentsConfiguration.RestrictAmexCard &&
                                CreditCard.GetCardType(pi.CardType) == IssuerAssociationType.AmericanExpress)
                            {
                                cvv.Enabled = false;
                            }
                            if (!isCvvVisible(pi))
                            {
                                cvv.Attributes.Add("style", "display:none");
                            }
                            cards.Items.Clear();
                            var currentCards = GetCurrentPaymentInformation(_locale, _distributorId);
                            var currentIds = (null != currentCards)
                                                 ? (from p in currentCards select p.ID).ToList()
                                                 : new List<int>();
                            if (null != pi && !string.IsNullOrEmpty(pi.CardType))
                            {
                                AddCardRowOptions(e.Row);
                                cards.Items.Add(new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                                cards.Items.AddRange((from p in _PaymentsList
                                                      where (!currentIds.Contains(p.ID) || p.ID == pi.ID)
                                                      select new ListItem(p.Alias, p.ID.ToString())).Distinct().ToArray());
                                cards.SelectedValue = pi.ID.ToString();
                            }
                            else
                            {
                                cards.Items.Add(new ListItem(base.GetLocalResourceObject("Select") as string, string.Empty));
                                cards.Items.AddRange((from p in _PaymentsList
                                                      where !currentIds.Contains(p.ID)
                                                      select new ListItem(p.Alias, p.ID.ToString())).ToArray());
                                SetupGridRow(e.Row, false);
                            }
                            if (pi.AuthorizationFailures > 0 || isExpires(pi.Expiration))
                            {
                                var img = e.Row.FindControl("imgDeclined") as Image;
                                if (null != img)
                                {
                                    img.ImageUrl = GetStatusImageUrl(pi.ID);
                                    img.ToolTip = GetStatusImageToolTip(pi.ID);
                                    img.Visible = true;
                                }
                            }
                            if (isExpires(pi.Expiration))
                            {
                                if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
                                {
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCardHAP");
                                }
                                else
                                {
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpiredForSavedCard");
                                }

                                if (HLConfigManager.Configurations.PaymentsConfiguration.RestrictAmexCard &&
                                    CreditCard.GetCardType(pi.CardType) == IssuerAssociationType.AmericanExpress)
                                {
                                    lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "BlockAmexCard");
                                }
                            }
                            else if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO && showWarningExpiring(pi.Expiration))
                            {
                                lblErrorMessages.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CardExpirationWarning");
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected void gridViewCardInfo_RowAmountChange(object sender, EventArgs e)
        {
            TextBox txtAmount = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtAmount.Parent.Parent;

            // display installment options according to amount
            if (null != txtAmount)
            {
                bool fiftyplus = false;
                int aNum = 0;
                if (Int32.TryParse(txtAmount.Text, out aNum))
                {
                    if (aNum > 50000)
                    {
                        fiftyplus = true;
                    }
                }

                var ddlInstallments = row.FindControl("drpInstallments") as DropDownList;

                if (ddlInstallments.Items.Count == 0) // first time to load, no item
                {
                    ddlInstallments.Items.Add(new ListItem("일시불", "00"));
                    ddlInstallments.Enabled = false;
                    ddlInstallments.Visible = true;
                }
                else if (ddlInstallments.Items.Count == 1) // not first time, with only LumpSum
                {
                    if (fiftyplus)
                    {
                        ddlInstallments.Items.Add(new ListItem("2개월", "02"));
                        ddlInstallments.Items.Add(new ListItem("3개월", "03"));
                        ddlInstallments.Items.Add(new ListItem("4개월", "04"));
                        ddlInstallments.Items.Add(new ListItem("5개월", "05"));
                        ddlInstallments.Items.Add(new ListItem("6개월", "06"));
                        ddlInstallments.Items.Add(new ListItem("7개월", "07"));
                        ddlInstallments.Items.Add(new ListItem("8개월", "08"));
                        ddlInstallments.Items.Add(new ListItem("9개월", "09"));
                        ddlInstallments.Items.Add(new ListItem("10개월", "10"));
                        ddlInstallments.Items.Add(new ListItem("11개월", "11"));
                        ddlInstallments.Items.Add(new ListItem("12개월", "12"));

                        ddlInstallments.Enabled = true;
                    }
                }
                else if (ddlInstallments.Items.Count == 12) // not first time, alreay load 12 installment options
                {
                    if (!fiftyplus)
                    {
                        ddlInstallments.Items.Clear();
                        ddlInstallments.Items.Add(new ListItem("일시불", "00"));
                        ddlInstallments.Enabled = false;
                        ddlInstallments.Visible = true;
                    }
                }
            }

            UpdateBalance();
        }

        private void UpdateBalance()
        {
            decimal totalDue = GetTotals().AmountDue;
            decimal amountAdd = 0;
            foreach (GridViewRow row in gridViewCardInfo.Rows)
            {
                var txtAmount = row.FindControl("txtAmount") as TextBox;
                if(null != txtAmount)
                {
                    int aNum = 0;
                    if (Int32.TryParse(txtAmount.Text, out aNum))
                    {
                        amountAdd += aNum;
                    }
                }           
            }
            
            totalAmountBalance.Text = DisplayAsCurrency((totalDue - amountAdd), false);
        }

        #endregion Grid Events

        #region Payment Options

        /// <summary>Add Payment Options links where appropriate</summary>
        /// <param name = "row" > The current Grid Row</param>
        /// <returns>true if the card requires payment options</returns>
        private bool AddCardRowOptions(GridViewRow row)
        {
            bool showPayOptions = false;

            var pi = row.DataItem as PaymentInformation;

            if (null != pi && !string.IsNullOrEmpty(pi.CardType))
            {
                string cardType = pi.CardType.TrimEnd();

                // Installments
                if (_enableInstallments)
                {
                    gridViewCardInfo.Columns[9].Visible = true;

                    var ddlInstallments = row.FindControl("drpInstallments") as DropDownList;
                    ddlInstallments.Items.Clear();
                    ddlInstallments.Items.Add(new ListItem("일시불", "00"));
                    ddlInstallments.Enabled = false;
                    ddlInstallments.Visible = true;
                }

                // Points
                var ddUsePoints = row.FindControl("drpUsePoints") as DropDownList;
                ddUsePoints.Items.Clear();
                ddUsePoints.Items.Add(new ListItem("No", "0"));
                ddUsePoints.Enabled = false;

                //if (cardType == "국민카드" || cardType == "비씨카드")   // KB Card and BC Card, enable points
                if (cardType == "국민카드")   // KB Card, enable points; BC card no more points according to MinCop
                {
                    ddUsePoints.Items.Add(new ListItem("Yes", "60"));
                    ddUsePoints.Enabled = true;
                }

                ddUsePoints.Visible = true;

            }

            return showPayOptions;
        }

        private PaymentOptions GetPaymentOptions(TextBox paymentOption, TextBox option1, TextBox option2, DropDownList installments)
        {
            var options = new PaymentOptions_V01();
            int installmentValue = 0;
            int.TryParse(installments.SelectedValue, out installmentValue);
            options.NumberOfInstallments = installmentValue;          

            return options;
        }

        #endregion Payment Options

        #region Validation

        //public override bool ValidateAndGetPayments(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01 shippingAddress, out List<Payment> payments, bool showErrors)
        //{
        //    bool isValid = base.ValidateAndGetPayments(shippingAddress, out payments, showErrors);

        //    if (isValid)
        //    {
        //        
        //    }

        //    return isValid;
        //}

        #endregion Validation

    }
}