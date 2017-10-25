namespace MyHerbalife3.Ordering.Providers.Payments
{
    using System;
    using System.Reflection;
    using System.Web;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class ParaguayPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constants and Fields

        private readonly PaymentGatewayInvoker _theInvoker;

        #endregion

        #region Constructors and Destructors

        private ParaguayPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("ParaguayPaymentGateWay", paymentMethod, amount)
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
                    case IssuerAssociationType.Cabal:
                    case IssuerAssociationType.Panal:
                        {
                            invokerType = "PY_BanCardPaymentGateWayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    default:
                        {
                            invokerType = "PY_BanCardPaymentGateWayInvoker";
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


