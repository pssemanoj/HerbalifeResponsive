using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using HL.PGH.Api;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_vi_VN : PaymentGatewayControl
    {
        struct PaymentConfig
        {
            public string paymentName;
            public string paymentCode;
            public string htmlFragment;
        }

        List<PaymentConfig> _pghPayments;

        List<PaymentConfig> PGHPayments
        {
            get
            {
                if (_pghPayments == null || _pghPayments.Count < 1)
                {
                    string paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration.PGHPaymentsConfiguration;

                    if (!string.IsNullOrEmpty(paymentsConfig))
                    {
                        List<PaymentConfig> paymentsList = new List<PaymentConfig>();
                        var payments = paymentsConfig.Split('|');
                        foreach (string payment in payments)
                        {
                            var config = payment.Split(';');
                            if (config.Length > 2)
                            {
                                paymentsList.Add(new PaymentConfig
                                {
                                    paymentName = config[0].Trim(),
                                    paymentCode = config[1].Trim(),
                                    htmlFragment = config[2].Trim()
                                });
                            }
                        }
                        _pghPayments = paymentsList;
                    }
                }

                return _pghPayments;
            }
            set { _pghPayments = value; }
        }

        [Publishes(MyHLEventTypes.OnPaymentGatewayChanged)]
        public event EventHandler onPaymentGatewayChanged;

        #region Public Methods
        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            //To do : uncomment the next line  once servipag works.
            string payCode = ddlCards.SelectedValue;

            if (!string.IsNullOrEmpty(payCode))
            {
                payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
            }

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }

        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public override PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;

                var payments = HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as CreditPayment_V01;
                if (payments.Card.IssuerAssociation == IssuerAssociationType.VietnamLocalCard)
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.LocalVietnamPayment;
                }
                else
                {
                    request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.CreditCard;
                }

                return request;
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (PGHPayments != null)
                {
                    foreach (PaymentConfig p in PGHPayments)
                    {
                        ddlCards.Items.Add(new ListItem(p.paymentName, p.paymentCode));
                    }

                    if (ddlCards.Items.Count > 0)
                    {
                        ddlCards.Items[0].Selected = true;
                    }
                }
            }
            ddlCards_SelectedIndexChanged(null, null);
        }

        protected void ddlCards_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedPayment = PGHPayments.Find(p => p.paymentCode == ddlCards.SelectedValue);
            if (!string.IsNullOrEmpty(selectedPayment.htmlFragment))
            {
                onPaymentGatewayChanged(selectedPayment.htmlFragment, null);
            }
        }
    }
}