using System;
using System.Collections.Generic;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_RS : ShippingProviderBase
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
                                       ? string.Format("{0}<br/>{1} {2}<br/>{3} {4}<br/>{5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.PostalCode, address.Address.City,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0} {1}<br/>{2} {3}<br/>{4}", address.Address.Line1,
                                                       address.Address.Line2 ?? string.Empty, address.Address.PostalCode,
                                                       address.Address.City, formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br/>{1} {2}<br/>{3} {4}<br/>{5}", description,
                                                 address.Address.Line1,
                                                 address.Address.Line2 ?? string.Empty, address.Address.PostalCode,
                                                 address.Address.City, address.Address.StateProvinceTerritory);
            }
            while (formattedAddress.IndexOf("  ") > -1)
            {
                formattedAddress = formattedAddress.Replace("  ", " ");
            }
            return formattedAddress;
        }

        public override string FormatOrderPreferencesAddress(ShippingAddress_V01 address)
        {
            if (null == address || address.Address == null)
            {
                return string.Empty;
            }
            string name = string.Empty;
            if (!String.IsNullOrEmpty(address.Alias))
                name = address.Alias;
            else
                name = address.Recipient;

            string formattedAddress = string.Format("{0}<br>{1}<br>{2}<br>{3}<br>{4}<br>{5}<br>{6}<br>{7}",
                                                    name,
                                                    address.Address.Line1,
                                                    address.Address.Line2 ?? string.Empty,
                                                    address.Address.PostalCode,
                                                    address.Address.CountyDistrict ?? string.Empty,
                                                    address.Address.City,
                                                    address.Address.StateProvinceTerritory,
                                                    formatPhone(address.Phone));
            formattedAddress = formattedAddress.Replace("<br><br><br>", "<br>");
            return formattedAddress.Replace("<br><br>", "<br>");
        }

        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            return date<DateTime.Now.AddDays(5);
        }

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID,
                                                           string locale)
        {
            return base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale) + getShippingInfoForInvoiceToFirm(shoppingCart);
        }

        private string getShippingInfoForInvoiceToFirm(MyHLShoppingCart shoppingCart)
        {
            List<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(shoppingCart.DistributorID, true);
            MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification tid = null;
            if ((tid = tins.Find(t => t.IDType.Key == "SRTX")) != null && shoppingCart.InvoiceOption == MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc.InvoiceHandlingType.RecycleInvoice.ToString())
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
                        shoppingCart.InvoiceOption = MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc.InvoiceHandlingType.WithPackage.ToString();
                        return " " + entry.Value;
                    }
                }
            }
            return string.Empty;
        }

    }
}