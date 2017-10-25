using System;
using System.Reflection;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BrazilPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private readonly PaymentGatewayInvoker _theInvoker;

        private BrazilPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("BrazilPaymentGateway", paymentMethod, amount)
        {
            var payment = HttpContext.Current.Session[PaymentInformation] as WirePayment_V01;

            if (null != payment)
            {
                string invokerType = string.Empty;
                switch (payment.PaymentCode)
                {
                    case "TB":
                        {
                            invokerType = "BradescoElectronicTransferPaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentInformation);
                            break;
                        }
                    case "BB":
                        {
                            invokerType = "BancodoBrazilElectronicTransferPaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentInformation);
                            break;
                        }
                    case "ET":
                        {
                            invokerType = "ItauBoldCronElectronicTransferPaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentInformation);
                            break; 
                        }
                    case "BT":
                        {
                            if (HLConfigManager.Configurations.PaymentsConfiguration.TurnOnBrasPagBankSlip)
                            {
                                invokerType = "BankSlipBrasPagPaymentGatewayInvoker";
                                HttpContext.Current.Session.Remove(PaymentInformation);
                                break;
                            }

                            else { 
                            if (HLConfigManager.Configurations.PaymentsConfiguration.TurnOnTivitBankSlip)
                            {
                                invokerType = "BankSlipPaymentGatewayInvoker";
                                HttpContext.Current.Session.Remove(PaymentInformation);
                                break;
                            }
                            else
                            {
                                invokerType = "BankSlipBoldCronPaymentGatewayInvoker";
                                HttpContext.Current.Session.Remove(PaymentInformation);
                                break;
                            }
                            }
                        }
                    case "VE":
                        {
                            invokerType = "PGHPaymentGatewayInvoker";
                            HttpContext.Current.Session.Remove(PaymentInformation);
                            break;
                        }
                }

                var args = new object[] {paymentMethod, amount};
                var type = Type.GetType(string.Concat(RootNameSpace, invokerType), true, true);
                _theInvoker =
                    Activator.CreateInstance(type, BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as
                    PaymentGatewayInvoker;
            }
        }

        public override void Submit()
        {
            _theInvoker.Submit();
        }

        protected override void GetOrderNumber()
        {
            var type = _theInvoker.GetType();
            var methodInfo = type.GetMethod("GetOrderNumber",
                                            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(_theInvoker, new object[] {});
        }
    }
}