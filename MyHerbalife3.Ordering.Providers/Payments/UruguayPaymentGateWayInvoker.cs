namespace MyHerbalife3.Ordering.Providers.Payments
{
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using System;
    using System.Reflection;
    using System.Web;

    public class UruguayPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constants and Fields

        private readonly PaymentGatewayInvoker _theInvoker;

        #endregion

        #region Constructors and Destructors

        private UruguayPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("UruguayPaymentGateWay", paymentMethod, amount)
        {
            var payment =
                HttpContext.Current.Session[PaymentInformation] as CreditPayment_V01;
            if (null != payment)
            {
                string invokerType = string.Empty;
                switch (payment.Card.IssuerAssociation)
                {
                    case IssuerAssociationType.Visa:
                        {
                            invokerType = "UY_VpaymentPaymentGateWayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    case IssuerAssociationType.Oca:
                        {
                            invokerType = "UY_OcaPaymentGateWayInvoker";                    
                            break;
                        }
                    default:
                        {
                            invokerType = "UY_VpaymentPaymentGateWayInvoker";
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


