using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using System.Text.RegularExpressions;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_BE : ShippingProviderBase
    {
        private const decimal TotalAmount = 5000.00M;
        public override bool ValidateTotalAmountForPaymentOption(string paymentOption, decimal totalAmount)
        {
            var isValid = true;

            //Restriccion for display 
            if(paymentOption == "PaymentGateway"){

                if(totalAmount > TotalAmount){
                    isValid = false;
                }

            }

            return isValid;
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            //BE: Street 1 and House Number will be saved and sent to fusion as Street1 + <Space> + House Number in Street1
            shippment.Address.Line1 = string.Format("{0} {1}", shippment.Address.Line1.Trim(), shippment.Address.Line2.Trim());
            shippment.Address.Line2 = string.Empty;
            return true;
        }

        public override bool IsValidShippingAddress(MyHLShoppingCart shoppingCart)
        {
            return !(shoppingCart != null 
                    && shoppingCart.DeliveryInfo != null 
                    && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping 
                    && shoppingCart.DeliveryInfo.Address != null 
                    && shoppingCart.DeliveryInfo.Address.Address != null 
                    && string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.Line2));            
        }
    }
}
