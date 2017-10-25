using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Linq;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using InvoiceHandlingType = MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc.InvoiceHandlingType;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    class ShippingProvider_SI : ShippingProviderBase
    {
        public override List<DeliveryOption> GetDeliveryOptionsListForShipping(string country,
                                                                              string locale,
                                                                              ShippingAddress_V01 address)
        {
            var proxy = ServiceClientProvider.GetShippingServiceProxy();
            var request = new DeliveryOptionForCountryRequest_V01();
            request.Country = country;
            request.State = string.Empty;
            request.Locale = locale;
            var response = proxy.GetDeliveryOptions(new GetDeliveryOptionsRequest(request)).GetDeliveryOptionsResult as ShippingAlternativesResponse_V01;
            var final = new List<DeliveryOption>();
            foreach (ShippingOption_V01 option in response.DeliveryAlternatives)
            {
                var newOption = new DeliveryOption(option);
                newOption.Name = newOption.Description;
                final.Add(newOption);
            }

            return final;
        }


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
                                       ? string.Format("{0}<br>{1},{2}<br>{3}, {4} <br>{5}",
                                                       address.Recipient ?? string.Empty,
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                       address.Address.City,
                                                       address.Address.PostalCode,
                                                       formatPhone(address.Phone))
                                       : string.Format("{0},{1}<br>{2}, {3}<br>{4}",
                                                       address.Address.Line1, address.Address.Line2 ?? string.Empty,
                                                        address.Address.City,
                                                        address.Address.PostalCode,
                                                      formatPhone(address.Phone));
            }
            else
            {
                formattedAddress = string.Format("{0}<br>{1}, {2}<br>{3}, {4}, {5}", description,
                                                 address.Address.Line1, address.Address.Line2 ?? string.Empty, address.Address.PostalCode,
                                                 address.Address.City, address.Address.StateProvinceTerritory
                                                 );
            }
            if (formattedAddress.IndexOf(",,") > -1 || formattedAddress.IndexOf(", ,") > -1)
            {
                return formattedAddress.Replace(",,,", ",").Replace(", , ,", ",").Replace(",,", ",").Replace(", ,", ",");
            }
            else
            {
                return formattedAddress;
            }
        }
        public override List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address)
        {
            var baseDeliveryOption = base.GetDeliveryOptions(type, address);
            if (type.Equals(DeliveryOptionType.Pickup))
            {
                foreach (var deliveryOption in baseDeliveryOption)
                {
                    deliveryOption.Address.CountyDistrict = deliveryOption.Option.ToString();// "Pickup";
                    deliveryOption.Address.Line4 = deliveryOption.Description;
                }
                return baseDeliveryOption;
            }
            else if (type.Equals(DeliveryOptionType.Shipping))
            {
                var deliveryOption = GetDeliveryOptionsListForShipping("SI", "sl-SI", address);
                var deliveryOptions = new List<DeliveryOption>();
                foreach (var option in deliveryOption)
                {
                    option.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                    deliveryOptions.Add(option);
                }
                return deliveryOptions;
            }


            return baseDeliveryOption;
        }
        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            if (shoppingCart != null && shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null)
            {
                if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                {
                    if (null != shoppingCart.DeliveryInfo.Name)
                    {
                        string option = shoppingCart.DeliveryInfo.Name.Trim();
                        if (!string.IsNullOrEmpty(option))
                        {
                            if (option.IndexOf('-') > 1)
                            {
                                option = option.Split('-')[0].Trim();
                            }
                            string text = string.Empty;
                            switch (option)
                            {
                                case "Dostava s hitro pošto":
                                    {
                                        
                                        if (
                                            shoppingCart.InvoiceOption.Equals(
                                                InvoiceHandlingType.SendToDistributor.ToString()))
                                        {
                                            text = "Dostava s hitro pošto , Pošlji račun ločeno";

                                        }
                                        else
                                        {
                                            text = "Dostava s hitro pošto";
                                        }

                                        text += getShippingInfoForInvoiceToFirm(shoppingCart);
                                        return text;

                                    }

                                default:
                                    {
                                        if (
                                            shoppingCart.InvoiceOption.Equals(
                                                InvoiceHandlingType.SendToDistributor.ToString()))
                                        {
                                            text = "Pošlji račun ločeno";
                                        }
                                        else
                                        {
                                            text = string.Empty;
                                        }
                                        text += getShippingInfoForInvoiceToFirm(shoppingCart);
                                        return text;
                                    }
                            }
                        }
                    }
                }
                else if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    string text = string.Empty;
                    string instruction = string.IsNullOrEmpty(shoppingCart.DeliveryInfo.Instruction)
                                             ? string.Empty
                                             : shoppingCart.DeliveryInfo.Instruction;
                    if (shoppingCart.InvoiceOption.Equals(InvoiceHandlingType.SendToDistributor.ToString()))
                    {
                        text = instruction += ", Pošlji račun ločeno";

                    }
                    else
                    {
                        text =
                            instruction;
                    }
                    text += getShippingInfoForInvoiceToFirm(shoppingCart);
                    return text;
                }
                else
                {
                    string text = string.Empty;
                    text= base.GetShippingInstructionsForDS(shoppingCart, distributorID, locale);
                    text += getShippingInfoForInvoiceToFirm(shoppingCart);
                    return text;
                }
            }
            return String.Empty;
        }

        private string getShippingInfoForInvoiceToFirm(MyHLShoppingCart shoppingCart)
        {
            List<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(shoppingCart.DistributorID, true);
            MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification tid = null;
            if ((tid = tins.Find(t => t.IDType.Key == "SITX")) != null && shoppingCart.InvoiceOption==InvoiceHandlingType.RecycleInvoice.ToString())
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
                        //shoppingCart.InvoiceOption = InvoiceHandlingType.WithPackage.ToString();
                        return " " + entry.Value;
                    }
                }
            }
            return string.Empty;
        }

        public override string GetDifferentHtmlFragment(string deliverymethodoption)
        {
            string option = deliverymethodoption.Trim();
            if (!string.IsNullOrEmpty(option))
            {
                switch (option)
                {
                    case "Dostava s hitro pošto":
                        {
                            return "ExpressDelivery.html";
                        }
                    default:
                        {
                            return "shippingmethod.html";
                        }
                }
            }
            return string.Empty;
        }
        
        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            if (shippment.Address.CountyDistrict == "Pickup")
            {
                ServiceProvider.SubmitOrderBTSvc.Address formatedAddress = new ServiceProvider.SubmitOrderBTSvc.Address();
                formatedAddress.Line1 = shippment.Address.Line4;
                formatedAddress.City = shippment.Address.Line1;
                formatedAddress.StateProvinceTerritory = shippment.Address.City;
                formatedAddress.Country = shippment.Address.Country;
                formatedAddress.PostalCode = shippment.Address.PostalCode;
                shippment.Address = formatedAddress;
            }
            return true;
        }

        public override bool ValidatePickupInstructionsDate(DateTime date)
        {
            if (Convert.ToInt32(date.DayOfWeek) == 0 || Convert.ToInt32(date.DayOfWeek) == 6) 
            { 
                return false; 
            }
            else 
            { 
                return true; 
            }
        }

        public override int GetAllowPickupDays(DateTime date)
        {
            int daysAllowed = HLConfigManager.Configurations.PickupOrDeliveryConfiguration.AllowDaysPickUp;
            
            //Specify non working days, Sunday = 0
            int[] nonWorkingDays = { 0, 6 };

            int days = 0;
            int totalDays = 0;

            while (days < daysAllowed)
            {
                DateTime temDate = date.AddDays(++totalDays);
                if (!nonWorkingDays.Contains(Convert.ToInt32(temDate.DayOfWeek)))
                {
                    days++;
                }
                //totalDays++;
            }

            return totalDays;
        }
    }
}