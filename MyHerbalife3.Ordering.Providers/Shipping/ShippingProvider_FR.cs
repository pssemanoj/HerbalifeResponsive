using System;
using MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_FR : ShippingProviderBase
    {
        public override void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.OrderSvc.ShippingInfo_V01 address)
        {
            if (shoppingCart != null)
            {
                if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    address.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                    address.ShippingMethodID = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                }
                else
                {
                    shoppingCart.CheckShippingForNonStandAloneAPF();

                    SessionInfo sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                    if (sessionInfo.IsEventTicketMode && shoppingCart.DeliveryInfo != null)
                    {
                        shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
                        shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;

                        // Take address from DB
                        var deliveryOptions = base.GetDeliveryOptions(shoppingCart.Locale);
                        var addressETO = deliveryOptions.Find(d =>
                            d.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping &&
                            d.OrderCategory == ServiceProvider.ShippingSvc.OrderCategoryType.ETO);

                        if (addressETO != null)
                        {
                            shoppingCart.DeliveryInfo.Address.Address = addressETO.Address;
                        }

                        // Set MB's Name in the Recipient name
                        var loader = new Core.DistributorProvider.DistributorLoader();
                        var distributor = loader.Load(shoppingCart.DistributorID, shoppingCart.CountryCode);

                        if (distributor != null && distributor.EnglishName != null)
                        {
                            shoppingCart.DeliveryInfo.Address.Recipient = string.Format("{0} {1}{2}",
                                distributor.EnglishName.First,
                                !string.IsNullOrEmpty(distributor.EnglishName.Middle) ? distributor.EnglishName.Middle + " " : string.Empty,
                                distributor.EnglishName.Last);
                        }
                    }
                }
            }
        }

        public override bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, Shipment shippment)
        {
            if (shoppingCart.DeliveryInfo != null && shoppingCart.DeliveryInfo.Address != null &&
                shoppingCart.DeliveryInfo.Address.Address != null &&
                shoppingCart.DeliveryOption == ServiceProvider.CatalogSvc.DeliveryOptionType.Shipping)
            {
                if (APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, shoppingCart.Locale))
                {
                    shippment.WarehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
                    shippment.ShippingMethodID = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
                }
                else
                {
                    shoppingCart.CheckShippingForNonStandAloneAPF();

                    SessionInfo sessionInfo = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, shoppingCart.Locale);
                    if (sessionInfo.IsEventTicketMode && shoppingCart.DeliveryInfo != null)
                    {
                        shoppingCart.DeliveryInfo.WarehouseCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
                        shoppingCart.DeliveryInfo.FreightCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                        shippment.WarehouseCode = shoppingCart.DeliveryInfo.WarehouseCode;
                        shippment.ShippingMethodID = shoppingCart.DeliveryInfo.FreightCode;

                        // Take address from DB in the ShoppingCart.
                        var deliveryOptions = base.GetDeliveryOptions(shoppingCart.Locale);
                        var addressETO = deliveryOptions.Find(d =>
                            d.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping &&
                            d.OrderCategory == ServiceProvider.ShippingSvc.OrderCategoryType.ETO);

                        if (addressETO != null)
                        {
                            shoppingCart.DeliveryInfo.Address.Address = addressETO.Address;
                        }

                        // Take mailing address for HMS
                        var mailingAddress = DistributorOrderingProfileProvider.GetAddress(
                                    ServiceProvider.OrderSvc.AddressType.Mailing, shoppingCart.DistributorID,
                                    shoppingCart.CountryCode);

                        if (mailingAddress != null)
                        {
                            shippment.Address.Line1 = mailingAddress.Line1;
                            shippment.Address.Line2 = mailingAddress.Line2;
                            shippment.Address.Line3 = mailingAddress.Line3;
                            shippment.Address.Line4 = mailingAddress.Line4;
                            shippment.Address.City = mailingAddress.City;
                            shippment.Address.StateProvinceTerritory = mailingAddress.StateProvinceTerritory;
                            shippment.Address.PostalCode = mailingAddress.PostalCode;
                            shippment.Address.CountyDistrict = mailingAddress.CountyDistrict;
                            shippment.Address.Country = mailingAddress.Country;
                        }

                        // Set MB's Name in the Recipient name
                        var loader = new Core.DistributorProvider.DistributorLoader();
                        var distributor = loader.Load(shoppingCart.DistributorID, shoppingCart.CountryCode);

                        if (distributor != null && distributor.EnglishName != null)
                        {
                            shoppingCart.DeliveryInfo.Address.Recipient = string.Format("{0} {1}{2}",
                                distributor.EnglishName.First,
                                !string.IsNullOrEmpty(distributor.EnglishName.Middle) ? distributor.EnglishName.Middle + " " : string.Empty,
                                distributor.EnglishName.Last);
                        }
                    }
                }
            }
            return true;
        }

        public override string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(string.Empty, string.Empty);
            if (sessionInfo.IsEventTicketMode)
            {
                return new[]
                    {
                        HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode,
                        HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode
                    };
            }

            var freightCodeAndWarehouse = new[] { "EXX", "F2" }; //default to mainland france: 01000 - 96999
            if (null != address && null != address.Address)
            {
                int postCode = 0;
                if (Int32.TryParse(address.Address.PostalCode, out postCode))
                {
                    if (postCode >= 98800 & postCode <= 98899)
                    {
                        freightCodeAndWarehouse = new[] { "NCD", "F8" };
                    }
                }
            }

            return freightCodeAndWarehouse;
        }
        public override bool ValidatePostalCode(string country, string state, string city, string postalCode)
        {
            int postCode = 0;
            if (Int32.TryParse(postalCode, out postCode))
            {
                if (postCode >= 98700 & postCode <= 98799)
                {
                    return false;
                }
            }
            return true;
        }

    }
}