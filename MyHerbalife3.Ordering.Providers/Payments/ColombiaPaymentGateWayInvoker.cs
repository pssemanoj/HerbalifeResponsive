using System;
using System.Reflection;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class ColombiaPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constants and Fields

        private readonly PaymentGatewayInvoker _theInvoker;

        #endregion

        #region Constructors and Destructors

        private ColombiaPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("ColombiaPaymentGateway", paymentMethod, amount)
        {
            var payment =
                HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            if (null != payment)
            {
                string invokerType = string.Empty;
                switch (payment.Card.IssuerAssociation)
                {
                    case IssuerAssociationType.Visa:
                    case IssuerAssociationType.MasterCard:
                    case IssuerAssociationType.AmericanExpress:
                    case IssuerAssociationType.Diners:
                    case IssuerAssociationType.GenericDebitCard:
                        {
                            invokerType = "PagosOnlinePaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    case IssuerAssociationType.PaymentGateway:
                        {
                            invokerType = "CO_PsePaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    default:
                        {
                            invokerType = "PagosOnlinePaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                }
                var args = new object[] { paymentMethod, amount };
                var type = Type.GetType(string.Concat(RootNameSpace, invokerType), true, true);
                this._theInvoker =
                    Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as
                    PaymentGatewayInvoker;
            }
        }

        #endregion

        #region Public Methods and Operators

        public override void Submit()
        {
            this._theInvoker.Submit();
        }

        #endregion

        #region Methods

        protected override void GetOrderNumber()
        {
            var type = this._theInvoker.GetType();
            var methodInfo = type.GetMethod(
                "GetOrderNumber", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(this._theInvoker, new object[] { });
        }

        #endregion
    }
}
