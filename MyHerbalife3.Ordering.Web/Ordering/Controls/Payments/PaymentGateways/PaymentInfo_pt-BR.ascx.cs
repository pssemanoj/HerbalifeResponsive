using System;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web.Security;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Installments;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using HL.PGH.Contracts.ValueObjects;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_pt_BR : PaymentGatewayControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadInfoRadioButton();
            }
            // To hide or show bankslip payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideBankSlip)
            {
                ShowBankSlip(false);
            }

            // To hide or show Itau payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideItau)
            {
                ShowItau(false);
            }

            // To hide or show Bradesco payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideBradesco)
            {
                ShowBradesco(false);
            }

            // To hide or show BancodoBrazil payment option
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideBancodoBrazil)
            {
                ShowBancodoBrazil(false);
            }

            // Hide Visa Electron if needed
            if (HLConfigManager.Configurations.PaymentsConfiguration.HideVisaElectron)
            {
                ShowVisaElectron(false);
            }
        }

        public override PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest pghRequest = base.PaymentRequest;
                try
                {
                    //pghRequest.SubmitOrderOnAuthorizationSuccess = true;
                    var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var user = member.Value;
                    pghRequest.PaymentMethod = GetPaymentMethod();
                    if (pghRequest.PaymentMethod == PaymentMethodType.Itau)
                    {
                        List<ServiceProvider.DistributorSvc.TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(ShoppingCart.DistributorID, true);
                        pghRequest.DistributorFirstName = user != null ? user.FirstName : string.Empty;
                        pghRequest.DistributorLastName = user != null ? user.LastName : string.Empty;
                        var BRPF = tins.Find(p => p.IDType.Key == "BRPF");
                        pghRequest.TaxId = BRPF != null ? BRPF.ID : string.Empty;
                    }
                    if (null != ShoppingCart.DeliveryInfo && null != ShoppingCart.DeliveryInfo.Address && null != ShoppingCart.DeliveryInfo.Address.Address)
                    {
                        var address = ShoppingCart.DeliveryInfo.Address.Address;
                        pghRequest.AddressLine1 = GetFilteredText(address.Line1, pghRequest.PaymentMethod);
                        pghRequest.AddressLine2 = GetFilteredText(address.Line2, pghRequest.PaymentMethod);
                        pghRequest.AddressLine3 = GetFilteredText(address.Line3, pghRequest.PaymentMethod);
                        pghRequest.AddressLine4 = GetFilteredText(address.Line4, pghRequest.PaymentMethod);
                        pghRequest.City = GetFilteredText(address.City, pghRequest.PaymentMethod);
                        pghRequest.StateProvinceTerritory = GetFilteredText(address.StateProvinceTerritory, pghRequest.PaymentMethod);
                        pghRequest.PostalCode = address.PostalCode;
                    }
                    if (pghRequest.PaymentMethod == PaymentMethodType.BankSlip)
                    {
                        DateTime currentDate;
                        InstallmentConfiguration _installmentsConfiguration;
                        if (null != ShoppingCart) 
                        {    
                            OrderMonth orderMonth = new OrderMonth(ShoppingCart.CountryCode);
                            currentDate = orderMonth.CurrentOrderMonth;
                            var orderType = "RSO";
                            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO) orderType = "ETO";
                            else if (ShoppingCart.OrderCategory == OrderCategoryType.APF) orderType = "APF";
                            _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration(ShoppingCart.CountryCode, new DateTime(currentDate.Year, currentDate.Month, 1), orderType);

                            if (_installmentsConfiguration == null)
                            {
                                DateTime today = DateTime.Today;
                                currentDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
                            }
                        }
                        else 
                        {
                            MyHLShoppingCart myCart;
                            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(user.Id, TheBase.Locale);
                            myCart = sessionInfoMyCart.ShoppingCart;
                            if (myCart == null)
                                myCart = ShoppingCartProvider.GetShoppingCart(user.Id, TheBase.Locale);
                            OrderMonth orderMonth = new OrderMonth(myCart.CountryCode);
                            currentDate = orderMonth.CurrentOrderMonth;
                            var orderType = "RSO";
                            if (myCart.OrderCategory == OrderCategoryType.ETO) orderType = "ETO";
                            else if (myCart.OrderCategory == OrderCategoryType.APF) orderType = "APF";
                            _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration(myCart.CountryCode, new DateTime(currentDate.Year, currentDate.Month, 1), orderType);

                            if (_installmentsConfiguration == null)
                            {
                                DateTime today = DateTime.Today;
                                currentDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
                            }
                        }
                        pghRequest.TicketDueDate =_installmentsConfiguration == null ? currentDate : _installmentsConfiguration.TicketDueDate;
                    }
                }
                catch (Exception ex)
                {
                    //Log
                    throw;
                }

                return pghRequest;
            }
        }

        public static string GetFilteredText(string text, PaymentMethodType type)
        {
            if (type == PaymentMethodType.BankSlip && text != null)
            {
                char[] restrictedCharacters = new char[] { '@', '/', '\\', '-', '(', ')', ':', '_', '"', '<', '>', '\'', '!', '*', '°', '´', '+', '=', '`', '|', '?' };
                string tmp  = restrictedCharacters.Aggregate(text, (c1, c2) => c1.Replace(c2, ' '));
                return tmp;
            }
            return text;
        }


        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            var tabs = TabControl as RadioButtonList;
            if (null != tabs && tabs.SelectedValue != "2")
            {
                return true;
            }

            //GetPaymentInfo();

            return true;
        }

        public override Payment GetPaymentInfo()
        {
            var payment = new WirePayment_V01();
            payment.PaymentCode = ddlGateways.SelectedValue;
            payment.TransactionType = ddlGateways.SelectedItem.Text;
            payment.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
            payment.Address = new Address_V01();

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }

        private PaymentMethodType GetPaymentMethod()
        {
            PaymentMethodType result = PaymentMethodType.Unknown;
            switch (ddlGateways.SelectedValue)
            {
                case "TB":
                {
                    result = PaymentMethodType.Bradesco;
                    break;
                }
                case "ET":
                {
                    result = PaymentMethodType.Itau;
                    break;
                }
                case "BB":
                {
                    result = PaymentMethodType.BancodoBrazil;
                    break;
                }
                case "BT":
                {
                    result = PaymentMethodType.BankSlip;
                    break;
                }
                case "VE":
                {
                    result = PaymentMethodType.VisaElectron;
                    break;
                }
            }

            return result;
        }

        public void ShowBankSlip(bool visible)
        {
            if (visible)
            {
                if (ddlGateways.CssClass.Contains("bankReference"))
                {
                    ddlGateways.CssClass = ddlGateways.CssClass.Replace("bankReference", string.Empty).Trim();
                }
            }
            else
            {
                if (!ddlGateways.CssClass.Contains("bankReference"))
                {
                    if (ddlGateways.SelectedIndex == 0)
                    {
                        ddlGateways.SelectedIndex = 1;
                    }
                    //ddlGateways.CssClass = string.Format("{0} bankReference", ddlGateways.CssClass).Trim();
                    ddlGateways.Items.FindByValue("BT").Attributes.Add("class", "bankReference");
                }
            }
        }

        public void ShowItau(bool visible)
        {
            if (!visible)
            {
                if (!ddlGateways.CssClass.Contains("itauReference"))
                {
                    ddlGateways.Items.FindByValue("ET").Attributes.Add("class", "itauReference");
                }
            }
        }

        public void ShowBradesco(bool visible)
        {
            if (!visible)
            {
                if (!ddlGateways.CssClass.Contains("bradescoReference"))
                {
                    ddlGateways.Items.FindByValue("TB").Attributes.Add("class", "bradescoReference");
                }
            }
        }

        public void ShowBancodoBrazil(bool visible)
        {
            if (!visible)
            {
                if (!ddlGateways.CssClass.Contains("bancodobrazilReference"))
                {
                    ddlGateways.Items.FindByValue("BB").Attributes.Add("class", "bancodobrazilReference");
                }
            }
        }

        public void ShowVisaElectron(bool visible)
        {
            if (!visible)
            {
                if (!ddlGateways.CssClass.Contains("visaelectronReference"))
                {
                    ddlGateways.Items.FindByValue("VE").Attributes.Add("class", "visaelectronReference");
                }
            }
        }

        private void LoadInfoRadioButton()
        {
            //Boleto Bancario
            var listItemBoletoBancario = new ListItem(string.Format(@"{0}", GetLocalResourceObject("lstItemTextBoletoBancario")), "BT");
            //listItemBoletoBancario.Selected = true;
            ddlGateways.Items.Add(listItemBoletoBancario);
            //Bradesco - Transferência Bancária 
            var listItemBradesco = new ListItem(string.Format(@"{0}", GetLocalResourceObject("lstItemTextBradesco")), "TB");
            ddlGateways.Items.Add(listItemBradesco);
            //Banco do Brasil - Transferência Bancária
            var listItemBancoDoBrasil = new ListItem(string.Format(@"{0}", GetLocalResourceObject("lstItemtextBancoDoBrasil")), "BB");
            listItemBancoDoBrasil.Selected = true;
            ddlGateways.Items.Add(listItemBancoDoBrasil);
            //Itaú - Transferência Bancária
            var listItemItau = new ListItem(string.Format(@"{0}", GetLocalResourceObject("lstItemTextItau")), "ET");
            ddlGateways.Items.Add(listItemItau);
            //Visa Electron - Transferência Bancária
            var listItemVisa = new ListItem(string.Format(@"{0}", GetLocalResourceObject("lstItemTextVisaElectron")), "VE");
            ddlGateways.Items.Add(listItemVisa);

        }
    }
}