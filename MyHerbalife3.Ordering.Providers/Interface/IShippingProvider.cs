using System.Collections.Generic;
using System;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Interfaces
{
    public interface IShippingProvider
    {
        MyHerbalife3.Ordering.ServiceProvider.ShippingChinaSvc.IChinaShipping ChinaShippingServiceProxy { get; set; }

        OrderCategoryType OrderType { get; set; }

        List<DeliveryOption> GetShippingAddresses(string distributorID, string locale);

        List<Address_V01> AddressSearch(string SearchTerm);

        int SaveShippingAddress(string distributorID, string locale, ShippingAddress_V02 shippingAddressToSave, bool toSession, bool addrNoChanged, bool bCheckNickname);

        int DeleteShippingAddress(string distributorID, string locale, ShippingAddress_V02 addressToDelete);

        string GetAddressDisplayName(ShippingAddress_V02 address);

        ShippingAddress_V02 GetShippingAddressFromDefaultAddress(string distributorID);

        ShippingAddress_V02 GetShippingAddressFromAddressGuidOrId(Guid addressGuid, int id);

        string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale);

        string GetShippingInstructionsForShippingInfo(ServiceProvider.CatalogSvc.ShoppingCart_V02 cart, MyHLShoppingCart shoppingCart, string distributorID, string locale);

        string GetRecipientName(ShippingInfo currentShippingInfo);

        string GetFreightVariant(ShippingInfo shippingInfo);

        DeliveryOption GetDefaultAddress();

        Address_V01 GetHFFDefaultAddress(ShippingAddress_V01 address);

        DeliveryOption GetEventTicketShippingInfo();

        DeliveryOption GetAPFShippingInfo();

        List<DeliveryOption> GetDeliveryOptions(string locale);

        List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address);

        List<DeliveryOption> GetDeliveryOptions(DeliveryOptionType type, ShippingAddress_V01 address, string option);

        List<DeliveryOption> GetDeliveryOptionsListForShipping(string Country, string locale, ShippingAddress_V01 address);

        List<DeliveryOption> GetDeliveryOptionsListForMobileShipping(string Country, string locale, ShippingAddress_V01 address, string memberId);

        List<DeliveryOption> GetDeliveryOptionsListForPickup(string country, string locale, ShippingAddress_V01 address);

        ShippingInfo GetShippingInfoFromID(string distributorID, string locale, DeliveryOptionType type, int deliveryOptionID, int shippingAddressID);

        ShippingInfo GetShippingInfoForMobile(string distributorID, string locale, DeliveryOptionType type, MyHLShoppingCart shoppingCart);

        void ReloadOrderShippingAddressFromService(string distributorID, string locale);

        List<DeliveryOption> GetShippingAddressesFromService(string distributorID, string locale);

        bool UpdateShippingInfo(int shoppingCartID, ServiceProvider.CatalogSvc.OrderCategoryType type, ServiceProvider.CatalogSvc.DeliveryOptionType option, int deliveryOptionID, int shippingAddressID);

        List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country);

        List<PickupLocationPreference_V02> GetPickupLocationsPreferences(string distributorId, string country,
                                                                         string locationType);

        int SavePickupLocationsPreferences(string distributorID, bool toSession, int pickupLocationID, string pickupLocationNickname, string pickupLocationName, string country, bool isPrimary);

        int SavePickupLocationsPreferences(string distributorID, bool toSession, int pickupLocationID, string pickupLocationNickname, string pickupLocationName, string country, bool isPrimary, string courierType);

        int DeletePickupLocationsPreferences(string distributorID, int pickupLocationID, string country);

        int DeletePickupLocationsPreferences(string distributorID, int pickupLocationID, string country, string courierType);

        bool ShouldRecalculate(string oldFreightCode, string newFreightCode, Address_V01 oldaddress, Address_V01 newaddress);

        bool ValidateAddress(ShippingAddress_V02 address);
        bool ValidateAddress(ShippingAddress_V02 address, out string errorCode, out ServiceProvider.AddressValidationSvc.Address avsAddress);
        bool ValidateAddress(MyHLShoppingCart shoppingCart);

        bool ValidateShipping(MyHLShoppingCart shoppingCart);

        string FormatPickupLocationAddress(Address_V01 address);
        string FormatOrderPreferencesAddress(ShippingAddress_V01 address);
        bool FormatAddressForHMS(ServiceProvider.SubmitOrderBTSvc.Address address);

        void GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01 address);

        bool GetDistributorShippingInfoForHMS(MyHLShoppingCart shoppingCart, ServiceProvider.SubmitOrderBTSvc.Shipment shippment);

        int? GetDeliveryEstimate(ShippingInfo shippingInfo, string locale);

        List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country, string locale, DeliveryOptionType deliveryType);

        List<PickupLocationPreference_V01> GetPickupLocationsPreferences(string distributorId, string country, string locale, DeliveryOptionType deliveryType, string courierType);

        int GetAllowPickupDays(DateTime date);
        
        bool ValidatePickupInstructionsDate(DateTime date);

        bool ValidateShippingInstructionsDate(DateTime date);

        bool NeedEnterAddress(string distributorID, string locale);

        string[] GetFreightCodeAndWarehouse(ShippingAddress_V01 address);

        void SetShippingInfo(ServiceProvider.CatalogSvc.ShoppingCart_V01 cart);

        List<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.InvoiceHandlingType> GetInvoiceOptions(ShippingAddress_V01 address, List<ServiceProvider.CatalogSvc.CatalogItem_V01> cartItems, ServiceProvider.CatalogSvc.ShoppingCart_V01 cart);

        string[] GetFreightCodeAndWarehouseForTaxRate(Address_V01 address);
        
        bool ShippingInstructionDateTodayNotAllowed();

        // address lookup provider
        List<string> GetStatesForCountry(string country);

        List<string> GetCitiesForState(string country, string state);

        List<string> GetZipsForCity(string country, string state, string city);

        List<string> GetZipsForStreet(string country, string state, string city, string street);

        List<StateCityLookup_V01> LookupCitiesByZip(string country, string zipcode);

        List<string> GetStreetsForCity(string country, string state, string city);

        List<string> GetAddressField(AddressFieldForCountryRequest_V01 request);

        bool ValidatePostalCode(string country, string state, string city, string postalCode);

        string LookupZipCode(string state, string municipality, string colony);

        string FormatShippingAddress(ShippingAddress_V01 address, DeliveryOptionType type, string description, bool includeName);

        bool AddressValidationRequired();
        bool DSAddressAsShippingAddress();
        bool CustomerAddressIsValid(string countrycode, ref ShippingAddress_V02 customerAddress);
        
        List<string> GetStreets(string countryCode, string state, string district);
        List<string> GetZipCodes(string country, string state, string district, string city);
        string GetDifferentHtmlFragment(string selectedValue);
        string GetDifferentHtmlFragment(string freightcode, ShippingAddress_V01 address);
        string GetDifferentHtmlFragment(MyHLShoppingCart shoppingCart);
        bool HasAdditionalPickupFromCourier();
        bool HasAdditionalPickup();
        string GetCourierTypeBySelection(string itemSelected);

        bool DisplayHoursOfOperation(DeliveryOptionType option);

        bool ValidateTotalAmountForPaymentOption(string paymentOption, decimal totalAmount);

        List<string> GetCountiesForCity(string country, string state, string city);

        List<string> GetZipsForCounty(string country, string state, string city, string county);

        bool IsValidShippingAddress(MyHLShoppingCart shoppingCart);

        string GetMapScript(ShippingMapType mapType);

        int SavePickupLocation(InsertCourierLookupRequest_V01 request);

        string GetWarehouseFromShippingMethod(string freighcode, ShippingAddress_V01 address);

        bool UpdatePrimaryPickupLocationPreference(int pickupLocationId);
    }
}
