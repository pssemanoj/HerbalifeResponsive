using System;
using System.Web;
using System.Web.Security;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using HL.PGH.Contracts.ValueObjects;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentGatewayControl : PaymentGatewayControlBase
    {
        public override PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest pghRequest = new PaymentRequest();
                try
                {
                    var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    var user = member.Value;
                    MyHLShoppingCart myCart;
                    SessionInfo SessionInfoMyCart = SessionInfo.GetSessionInfo(user.Id, TheBase.Locale);
                    myCart = SessionInfoMyCart.ShoppingCart;
                    if (myCart == null)
                        myCart = ShoppingCartProvider.GetShoppingCart(user.Id, TheBase.Locale);
                    string email = (null != myCart && !string.IsNullOrEmpty(myCart.EmailAddress)) ? myCart.EmailAddress.ToString() : string.Empty;
                    if (!string.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayCallBackUrl))
                    {
                        pghRequest.CallBackUrl = CreateFullUrl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayCallBackUrl);
                    }
                    else
                    {
                        pghRequest.CallBackUrl = string.Empty;
                    }

                    //Diagnostic Logging
                    LoggerHelper.Info("Creating the ReturnUrl. The Config Entry is: " + HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayReturnUrl);
                    pghRequest.ReturnUrl = CreateFullUrl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayReturnUrl);
                    LoggerHelper.Info("The returnUrl is: " + pghRequest.ReturnUrl + " Config Entry is: " + HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayReturnUrl);
                    //End Diagnostic Logging

                    pghRequest.SubmitOrderOnAuthorizationSuccess = HLConfigManager.Configurations.PaymentsConfiguration.SubmitOnAuthorization;
                    pghRequest.SuppressCallBack = HLConfigManager.Configurations.PaymentsConfiguration.SuppressCallBack;
                    pghRequest.Installments = "1";
                    pghRequest.ClientApplication = HL.PGH.Contracts.ValueObjects.ClientApplicationType.GDO;
                    pghRequest.Country = (IsoCountryCode)Enum.Parse(IsoCountryCode.Unknown.GetType(), TheBase.CountryCode, true);
                    pghRequest.DistributorId = user.Id;
                    pghRequest.DistributorName = string.Concat(user.FirstName, " ", user.LastName);
                    pghRequest.DistributorFirstName = user.FirstName;
                    pghRequest.DistributorLastName = user.LastName;
                    pghRequest.EmailAddress = email;
                    pghRequest.Currency = HLConfigManager.Configurations.CheckoutConfiguration.Currency.Trim();
                    pghRequest.Locale = TheBase.Locale;
                    pghRequest.Amount = (ShoppingCart.Totals as OrderTotals_V01).AmountDue;
                    pghRequest.ClientKey = HLConfigManager.Configurations.PaymentsConfiguration.ClientKey;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("PaymentGatewayControl.PaymentRequest", ex, "Exception occurred inside the PaymentRequest generator.");
                    try
                    {
                        //Diagnostic log
                        LoggerHelper.Error("Exception occurred inside the PaymentRequest generator. The returnUrl is: " + pghRequest.ReturnUrl + " Config Entry is: " + HLConfigManager.Configurations.PaymentsConfiguration.PaymentGatewayReturnUrl);
                    }
                    catch(Exception ex1)
                    {
                        LoggerHelper.Exception("PaymentGatewayControl.PaymentRequest", ex1, "Exception occurred inside the Catch handler");
                    }

                    throw;
                }

                return pghRequest;
            }
        }

        public override Payment GetPaymentInfo()
        {
            throw new NotSupportedException();
        }
        public override bool Validate(out string errorMessage)
        {
            throw new NotSupportedException();
        }

        public virtual bool HasCardData
        {
            get { return false; }
        }

        protected MyHLShoppingCart ShoppingCart
        {
            get { return (null != TheBase) ? TheBase.ShoppingCart : null; }
        }
        public ProductsBase TheBase
        {
            get;
            set;
        }

        public bool CanUsePGHWindow(PaymentMethodType paymentMethod)
        {
            //This is screwed up by Brazil pop-ups. Will pursue next phase
            return false;
            /*return new List<PaymentMethodType>(new PaymentMethodType[]{PaymentMethodType.CreditCard,
                                                                       PaymentMethodType.IDEAL, 
                                                                       PaymentMethodType.VisaElectron, 
                                                                       PaymentMethodType.BankWire, 
                                                                       PaymentMethodType.BankWithdrawal}).Contains(paymentMethod);
             */
        }

        private string CreateFullUrl(string relativeUrl)
        {
            string scheme = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
            if (HLConfigManager.Configurations.PaymentsConfiguration.IsFullPaymentGatewayReturnUrl)
            {
                return string.Format("{0}{1}", scheme, relativeUrl);
            }
            else
            {
                var uri = new Uri(string.Format("{0}{1}{2}", scheme, HttpContext.Current.Request.Url.Authority, relativeUrl));
                // Return full url excluding the port (~UriComponents.Port)
                return uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.UriEscaped);
            }
        }
    }
}