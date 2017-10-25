using System.Collections.Generic;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_MY : ShippingProviderBase
    {
        private const decimal PaymentGateway_MaxAmount = 30000.00M;
        public override bool ShouldRecalculate(string oldFreightCode,
                                               string newFreightCode,
                                               Address_V01 oldaddress,
                                               Address_V01 newaddress)
        {
            if (oldaddress == null || newaddress == null)
            {
                return true;
            }
            return (oldaddress.StateProvinceTerritory != newaddress.StateProvinceTerritory ||
                    oldFreightCode != newFreightCode);
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            string[] freightCodeAndWarehouse = new[]
                {
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                    HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                };


            if (address != null && address.Address != null)
            {

                string state = address.Address.StateProvinceTerritory != null
                                   ? address.Address.StateProvinceTerritory.ToUpper()
                                   : "";
                if ( state.Equals("SABAH") || state.Equals("LABUAN WILAYAH PERSEKUTUAN") || state.Equals("LABUAN"))
                {
                    freightCodeAndWarehouse[0] = "MYE";
                    freightCodeAndWarehouse[1] = "K2";
                }
                else if (state.Equals("SARAWAK"))
                {
                    freightCodeAndWarehouse[0] = "MYE";
                    freightCodeAndWarehouse[1] = "KQ";
                }
            }

            return freightCodeAndWarehouse;
        }

        //Bug 239543:GDO – Regression – QA3/PROD- MY/MY-en/MY-zh- Incorrect freight and WH
        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                var currentAddress = shoppingCart.DeliveryInfo.Address;
                var values = GetFreightCodeAndWarehouse(currentAddress);
                if (values != null)
                {
                    shoppingCart.DeliveryInfo.WarehouseCode = values[1];
                    shoppingCart.DeliveryInfo.FreightCode = values[0];
                }
            }
            else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                var pickUpOptions = base.GetDeliveryOptions(DeliveryOptionType.Pickup,
                                                                       base.GetDefaultAddress());
                var selected = pickUpOptions.FirstOrDefault(x => x.State == address.Address.StateProvinceTerritory && x.Address.City==address.Address.City);
                if (null != selected)
                {
                    shoppingCart.DeliveryInfo.WarehouseCode = selected.WarehouseCode;
                    shoppingCart.DeliveryInfo.FreightCode = selected.FreightCode;
                }
            }
        }
       
        public override bool ValidateTotalAmountForPaymentOption(string paymentOption, decimal TotalAmount)
        {
            var isValid = true;
            if (!string.IsNullOrEmpty(paymentOption) && paymentOption == "PaymentGateway")
            {
                if (TotalAmount > PaymentGateway_MaxAmount || TotalAmount < 1)
                    isValid = false;
            }
            else
            {
                isValid = base.ValidateTotalAmountForPaymentOption(paymentOption, TotalAmount);
            }
            return isValid;
        }

        public override bool IsValidShippingAddress(MyHLShoppingCart shoppingCart)
        {
            return !(shoppingCart != null 
                     && shoppingCart.DeliveryInfo != null
                     && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping
                     && shoppingCart.DeliveryInfo.Address != null 
                     && shoppingCart.DeliveryInfo.Address.Address != null
                     && string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory));
        }
    }
}
