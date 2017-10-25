using System;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.PGH.Contracts.ValueObjects;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_Generic : PaymentGatewayControl
    {
        #region Public Methods and Operators
        public override PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest pghRequest = new PaymentRequest();
                try
                {
                    pghRequest = base.PaymentRequest;
                    string method = HLConfigManager.Configurations.PaymentsConfiguration.Method;
                    if (!string.IsNullOrEmpty(method))
                    {
                        pghRequest.PaymentMethod = (PaymentMethodType)Enum.Parse(PaymentMethodType.Unknown.GetType(), method, true);
                    }
                    else
                    {
                        pghRequest.PaymentMethod = PaymentMethodType.CreditCard;
                    }
                }
                catch (Exception ex)
                {
                    //Logg
                    throw;
                }

                return pghRequest;
            }
        }

        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;

            return payment;
        }

        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
        #endregion
    }
}