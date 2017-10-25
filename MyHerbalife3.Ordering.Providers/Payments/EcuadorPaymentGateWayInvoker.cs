using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Reflection;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class EcuadorPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        private PaymentGatewayInvoker _theInvoker = null;

        private EcuadorPaymentGateWayInvoker(string paymentMethod, decimal amount)
            : base("EcuadorPaymentGateway", paymentMethod, amount)
        {
            CreditPayment_V01 payment = HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as CreditPayment_V01;
            if (null != payment)
            {
                string invokerType = string.Empty;
                switch (payment.Card.IssuerAssociation)
                {
                    case IssuerAssociationType.Visa:
                        {
                            invokerType = "ProdubancoPaymentGateWayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    case IssuerAssociationType.MasterCard:
                        {
                            invokerType = "ProdubancoPaymentGateWayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                    case IssuerAssociationType.Diners:
                        {
                            invokerType = "PayclubPaymentGateWayInvoker";
                            HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                            break;
                        }
                }
                object[] args = new object[] { paymentMethod, amount };
                Type type = Type.GetType(string.Concat(RootNameSpace, invokerType), true, true);
                _theInvoker = Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as PaymentGatewayInvoker;
            }
        }

        public override void Submit()
        {
            _theInvoker.Submit();
        }

        protected override void GetOrderNumber()
        {
            Type type = _theInvoker.GetType();
            MethodInfo methodInfo = type.GetMethod("GetOrderNumber", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(_theInvoker, new object[] { });
        }
    }
}