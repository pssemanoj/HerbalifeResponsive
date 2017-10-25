using System;
using System.Web;
using System.Reflection;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PeruPaymentGateWayInvoker : PaymentGatewayInvoker
    {
        private PaymentGatewayInvoker _theInvoker = null;

        private PeruPaymentGateWayInvoker(string paymentMethod, decimal amount) : base("PeruPaymentGateway", paymentMethod, amount)
        {
            CreditPayment_V01 payment = HttpContext.Current.Session[PaymentGatewayInvoker.PaymentInformation] as CreditPayment_V01;
            if(null != payment)
            {
                string invokerType = string.Empty;
                switch(payment.Card.IssuerAssociation)
                {
                    case IssuerAssociationType.Visa:
                    {
                        invokerType = "MultiMerchantVisaNetPaymentGatewayInvoker";
                        HttpContext.Current.Session.Remove(PaymentGatewayInvoker.PaymentInformation);
                        break;
                    }
                    case IssuerAssociationType.MasterCard:
                    {
                        invokerType = "PuntoWebPaymentGatewayInvoker";
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
