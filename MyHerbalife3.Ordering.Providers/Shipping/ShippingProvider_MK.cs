using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using MyHerbalife3.Shared.UI.Helpers;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_MK : ShippingProviderBase
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

            string formattedAddress = string.Empty;

            if (type == DeliveryOptionType.Shipping)
            {
                formattedAddress = includeName
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4}, {5}<br>{6}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                        address.Address.City, address.Address.PostalCode,
                                                       address.Address.StateProvinceTerritory,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}, {4}<br>{5}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City, address.Address.PostalCode, 
                                                       address.Address.StateProvinceTerritory,
                                                       formatPhone(address.Phone));
            }
            else
            {
                return string.Format("{0}<br>{1},{2}<br>{3} ",
                                     address.Address.Line1, address.Address.Line2 ?? string.Empty,address.Address.PostalCode, address.Address.City);
            }

            return formattedAddress;
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID,
                                                            string locale)
        {
            return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale) + getShippingInfoForInvoiceToFirm(shoppingCart);
        }

        private string getShippingInfoForInvoiceToFirm(MyHLShoppingCart shoppingCart)
        {
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(shoppingCart.DistributorID, true);
            TaxIdentification tid = null;
            if ((tid = tins.Find(t => t.IDType.Key == "MKTX")) != null && shoppingCart.InvoiceOption == InvoiceHandlingType.RecycleInvoice.ToString())
            {
                var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("InvoiceOptions");

                foreach (var entry in entries)
                {
                    var key = entry.Key;
                    if (key == shoppingCart.InvoiceOption)
                    {
                        var parts = key.Split('_');
                        if (parts.Length > 1)
                        {
                            key = parts[0];
                        }
                        shoppingCart.InvoiceOption = InvoiceHandlingType.WithPackage.ToString();
                        return " " + entry.Value;
                    }
                }
            }
            return string.Empty;
        }


        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            return date < DateTime.Now.AddDays(5);
        }
    }
}
