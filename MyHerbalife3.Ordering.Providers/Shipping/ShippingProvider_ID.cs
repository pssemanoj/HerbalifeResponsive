using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_ID : ShippingProviderBase
    {
        public override ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID)
        {
            ShippingInfo shippingInfo = base.GetShippingInfoFromID(distributorID, locale, type, deliveryOptionID, shippingAddressID);
            if (shippingInfo != null && shippingInfo.Address != null && type == DeliveryOptionType.Shipping)
            {
                shippingInfo = GetShippingInfo(shippingInfo);
            }
            return shippingInfo;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            
            // Checking for null, avoiding an exception.
            if (shoppingCart.InvoiceOption == null)
            {
                shoppingCart.InvoiceOption = string.Empty;
            }

            var invoiceOption = HttpContext.GetGlobalResourceObject("InvoiceOptions", shoppingCart.InvoiceOption.Trim(), CultureInfo.CurrentCulture);
            if (invoiceOption == null)
            {
                invoiceOption = shoppingCart.InvoiceOption.Trim();
            }
            string instruction = string.Empty;
            if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
            {
                return instruction = string.Format("{0},{1},{2}", shoppingCart.DeliveryInfo.Address.Recipient, shoppingCart.DeliveryInfo.Address.Phone, invoiceOption);
            }
            else
            {
                instruction = currentShippingInfo == null ? invoiceOption as string : string.Format("{0} {1}", currentShippingInfo.Instruction, invoiceOption);
            }
            return instruction.Trim();
        }

        public override string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string formattedAddress = string.Empty;
            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName ? string.Format("{0}<br>{1} {2}<br>{3} {4}<br>{5},{6}", address.Recipient ?? string.Empty,
                    address.Address.Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone)) :
                    string.Format("{0} {1}<br>{2} {3}<br>{4},{5}",
                    address.Address.Line1, address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode,
                    formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{5}<br>{0}<br>{1}<br>{2}, {3} {4}", address.Address.Line1,
                    address.Address.Line2, address.Address.City, address.Address.StateProvinceTerritory, address.Address.PostalCode, description);
            }
            return formattedAddress;
        }

        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
            {
                if (address != null && address.Address != null && shoppingCart.ShippingAddressID != 0)
                {
                    address.Address.Line4 = shoppingCart.DeliveryInfo.Address.Recipient;
                }
            }
        }

        /// <summary>
        /// Gets the shipment information to import into HMS.
        /// </summary>
        /// <param name="shoppingCart">The shopping cart.</param>
        /// <param name="shippment">The order shipment.</param>
        /// <returns></returns>
        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            var session = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
            if (session.IsEventTicketMode)
            {
                return true;
            }

            if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
            {
                return true;
            }

            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping &&
                (shoppingCart.DeliveryInfo.FreightCode.Equals(HLConfigManager.Configurations.APFConfiguration.APFFreightCode) ||
                shoppingCart.DeliveryInfo.FreightCode.Equals(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode)))
            {
                var shippingInfo = GetShippingInfo(shoppingCart.DeliveryInfo);
                shoppingCart.DeliveryInfo.FreightCode = shippingInfo.FreightCode;
                shoppingCart.DeliveryInfo.WarehouseCode = shippingInfo.WarehouseCode;
                shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;
                shippment.WarehouseCode = shoppingCart.DeliveryInfo.WarehouseCode;
            }
            return true;
        }

        private ShippingInfo GetShippingInfo(ShippingInfo shippingInfo)
        {
            var lstOptions = GetDeliveryOptions(DeliveryOptionType.Shipping, shippingInfo.Address);
            if (lstOptions != null && shippingInfo.Address.Address != null)
            {
                if (lstOptions.Count > 0)
                {
                    var cityName = new StringBuilder(shippingInfo.Address.Address.City.Trim().ToUpper())
                        .Replace(char.ConvertFromUtf32(160), char.ConvertFromUtf32(32));

                    var stateName = shippingInfo.Address.Address.StateProvinceTerritory.ToString();

                    // Looking by city and state
                    var byCity = (from o in lstOptions
                                  where o.Address.City.Trim().ToUpper().Equals(cityName.ToString().Trim().ToUpper())
                                    && o.State.Trim().ToUpper().Equals(stateName.Trim().ToUpper())
                                  select o).FirstOrDefault();
                    if (byCity != null)
                    {
                        shippingInfo.WarehouseCode = byCity.WarehouseCode;
                        shippingInfo.FreightCode = byCity.FreightCode;
                        return shippingInfo;
                    }

                    // Looking by state
                    var byState = (from o in lstOptions
                                   where o.State.Trim().ToUpper().Equals(stateName.Trim().ToUpper())
                                   select o).FirstOrDefault();
                    if (byState != null)
                    {
                        shippingInfo.WarehouseCode = byState.WarehouseCode;
                        shippingInfo.FreightCode = byState.FreightCode;
                        return shippingInfo;
                    }
                }
            }
            return shippingInfo;
        }
    }
}