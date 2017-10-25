using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_TW : ShippingProviderBase
    {
        public override string FormatShippingAddress(ShippingAddress_V01 address,
                                                     DeliveryOptionType type,
                                                     string description,
                                                     bool includeName)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }

            if (type == DeliveryOptionType.Shipping)
            {
                return includeName
                           ? string.Format("{0}<br>{1}, {2}, {3}, {4}", address.Recipient ?? string.Empty,
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.Line1)
                           : string.Format("{0}, {1}, {2}, {3}",
                                           address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                           address.Address.City, address.Address.Line1);
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3} {4}, {5}", description,
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                     address.Address.PostalCode, address.Address.StateProvinceTerritory,
                                     address.Address.City);
            }
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart,
                                                            string distributorID,
                                                            string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            return currentShippingInfo == null ? String.Empty : currentShippingInfo.Instruction;
        }
        public override List<ServiceProvider.OrderSvc.InvoiceHandlingType> GetInvoiceOptions(ShippingAddress_V01 address,
                                                                  List<ServiceProvider.CatalogSvc.CatalogItem_V01> cartItems,
                                                                  ServiceProvider.CatalogSvc.ShoppingCart_V01 cart)
        {
            var listInvoiceOptions = base.GetInvoiceOptions(address, cartItems, cart);
            var shoppingCart = cart as MyHLShoppingCart;
            
          if(shoppingCart !=null && shoppingCart.CustomerOrderDetail != null)
          {
                listInvoiceOptions.Clear();
                listInvoiceOptions.Add(ServiceProvider.OrderSvc.InvoiceHandlingType.WithPackage);
                listInvoiceOptions.Add(ServiceProvider.OrderSvc.InvoiceHandlingType.SendToDistributor);
          }
            return listInvoiceOptions;
        }
    }
}