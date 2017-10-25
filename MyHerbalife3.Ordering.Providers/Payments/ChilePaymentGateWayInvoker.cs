namespace MyHerbalife3.Ordering.Providers.Payments
{
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using System;
    using System.Reflection;
    using System.Web;

    public class ChilePaymentGateWayInvoker : PaymentGatewayInvoker
    {
        #region Constants and Fields

        private readonly PaymentGatewayInvoker _theInvoker;

        #endregion

        #region Constructors and Destructors

        private ChilePaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("ChilePaymentGateway", paymentMethod, amount)
        {
            var payment =
                HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as CreditPayment_V01;
            if (null != payment)
            {
                string invokerType = string.Empty;
                switch (payment.Card.IssuerAssociation)
                {
                    case IssuerAssociationType.Visa:
                    case IssuerAssociationType.MasterCard:
                    case IssuerAssociationType.AmericanExpress:
                    case IssuerAssociationType.GenericDebitCard:
                        {
                            invokerType = "WebPayPaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    case IssuerAssociationType.PaymentGateway:
                        {
                            invokerType = "CL_ServiPagPaymentGatewayInvoker";
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