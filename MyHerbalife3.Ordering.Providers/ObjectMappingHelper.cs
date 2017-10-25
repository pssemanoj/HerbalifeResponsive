using AutoMapper;
using System.Linq;

namespace MyHerbalife3.Ordering.Providers
{
    public class ObjectMappingHelper
    {
        #region Mapping profiles
        public class DistShoppingCartItemMappingProfile : Profile
        {
            protected override void Configure()
            {
                this.CreateMap<MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem, MyHerbalife3.Shared.ViewModel.Models.DistributorShoppingCartItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem, MyHerbalife3.Shared.ViewModel.CatalogSvc.ShoppingCartItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.ShoppingCartItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItemList, MyHerbalife3.Shared.ViewModel.CatalogSvc.ShoppingCartItemList>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductInfo_V02, MyHerbalife3.Shared.ViewModel.CatalogSvc.ProductInfo_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category, MyHerbalife3.Shared.ViewModel.CatalogSvc.Category>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category_V02, MyHerbalife3.Shared.ViewModel.CatalogSvc.Category_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category_V02ItemCollection, MyHerbalife3.Shared.ViewModel.CatalogSvc.Category_V02ItemCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem, MyHerbalife3.Shared.ViewModel.CatalogSvc.CatalogItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.CatalogItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU, MyHerbalife3.Shared.ViewModel.CatalogSvc.SKU>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.SKU_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01ItemCollection, MyHerbalife3.Shared.ViewModel.CatalogSvc.SKU_V01ItemCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Disclaimer, MyHerbalife3.Shared.ViewModel.CatalogSvc.Disclaimer>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Disclaimer_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.Disclaimer_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Icon, MyHerbalife3.Shared.ViewModel.CatalogSvc.Icon>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Icon_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.Icon_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Link, MyHerbalife3.Shared.ViewModel.CatalogSvc.Link>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Link_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.Link_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType, MyHerbalife3.Shared.ViewModel.CatalogSvc.ProductType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory, MyHerbalife3.Shared.ViewModel.CatalogSvc.WarehouseInventory>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory_V01, MyHerbalife3.Shared.ViewModel.CatalogSvc.WarehouseInventory_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory_V02, MyHerbalife3.Shared.ViewModel.CatalogSvc.WarehouseInventory_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventoryList, MyHerbalife3.Shared.ViewModel.CatalogSvc.WarehouseInventoryList>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem, MyHerbalife3.Shared.UI.CatalogSvc.ShoppingCartItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01, MyHerbalife3.Shared.UI.CatalogSvc.ShoppingCartItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItemList, MyHerbalife3.Shared.UI.CatalogSvc.ShoppingCartItemList>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU, MyHerbalife3.Shared.UI.CatalogSvc.SKU>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01, MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01ItemCollection, MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01ItemCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductInfo_V02, MyHerbalife3.Shared.UI.CatalogSvc.ProductInfo_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category, MyHerbalife3.Shared.UI.CatalogSvc.Category>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category_V02, MyHerbalife3.Shared.UI.CatalogSvc.Category_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Category_V02ItemCollection, MyHerbalife3.Shared.UI.CatalogSvc.Category_V02ItemCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem, MyHerbalife3.Shared.UI.CatalogSvc.CatalogItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.CatalogItem_V01, MyHerbalife3.Shared.UI.CatalogSvc.CatalogItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU, MyHerbalife3.Shared.UI.CatalogSvc.SKU>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01, MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01ItemCollection, MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01ItemCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Disclaimer, MyHerbalife3.Shared.UI.CatalogSvc.Disclaimer>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Disclaimer_V01, MyHerbalife3.Shared.UI.CatalogSvc.Disclaimer_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Icon, MyHerbalife3.Shared.UI.CatalogSvc.Icon>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Icon_V01, MyHerbalife3.Shared.UI.CatalogSvc.Icon_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Link, MyHerbalife3.Shared.UI.CatalogSvc.Link>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Link_V01, MyHerbalife3.Shared.UI.CatalogSvc.Link_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType, MyHerbalife3.Shared.UI.CatalogSvc.ProductType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory, MyHerbalife3.Shared.UI.CatalogSvc.WarehouseInventory>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory_V01, MyHerbalife3.Shared.UI.CatalogSvc.WarehouseInventory_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventory_V02, MyHerbalife3.Shared.UI.CatalogSvc.WarehouseInventory_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.WarehouseInventoryList, MyHerbalife3.Shared.UI.CatalogSvc.WarehouseInventoryList>();
            }
        }

        public class AddressMappingProfile : Profile
        {
            protected override void Configure()
            {
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Address, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType, MyHerbalife3.Shared.LegacyProviders.DistributorService.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType, MyHerbalife3.Shared.LegacyProviders.DistributorService.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.Address, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.AddressesCollection, HL.Common.ValueObjects.AddressCollection>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.AddressType, HL.Common.ValueObjects.AddressType>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address, HL.Common.ValueObjects.Address>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V01, HL.Common.ValueObjects.Address_V01>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02, HL.Common.ValueObjects.Address_V02>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V03, HL.Common.ValueObjects.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Address, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType, HL.Common.ValueObjects.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address, HL.Common.ValueObjects.Address>();                
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01, HL.Common.ValueObjects.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02, HL.Common.ValueObjects.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V03, HL.Common.ValueObjects.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address_V02, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address_V03, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.AddressType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01, MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01>();
            }
        }

        public class DeliveryOptionMappingProfile : Profile
        {
            protected override void Configure()
            {
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingOption, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingOption>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingOption_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingOption_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingSource_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingSource_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.PickupOption_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.PickupOption_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingAddress, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingAddress_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V01>();
            }
        }

        public class DistributorMappingProfile : Profile
        {
            protected override void Configure()
            {
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.Name_V01, MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Name_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Name_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Name_V01>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentification, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentification_V01, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification_V01>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentificationBase, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentificationBase>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentificationType, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentificationType>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorTinList, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorTinList>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorVolume>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume_V01, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorVolume_V01>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumberBase, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumberBase>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumber_V03, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03>();
                this.CreateMap<MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneType, MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneType>();
            }
        }

        public class OrderMappingProfile : Profile
        {
            protected override void Configure()
            {
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Order, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Order>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrder_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CardAuthorization, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CardAuthorization>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CardAuthorization_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CardAuthorization_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.DistributorInformation, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.DistributorInformation>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Name_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Name_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderFulfillerType, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderFulfillerType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderItems, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItems>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderItem, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItem>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderItem_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderItem_V02, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItem_V02>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderItem_V03, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderItem_V03>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrderItem_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrderItem_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrderStatusType>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.PaymentGatewayName, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayName>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerPaymentMethodChoice, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerPaymentMethodChoice>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.PaymentCollection, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentCollection>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.Payment, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Payment>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.WirePayment, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.WirePayment>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.WirePayment_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.WirePayment_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.PayPalPayment, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PayPalPayment>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.PayPalPayment_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PayPalPayment_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CreditPayment, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CreditPayment_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CreditPayment_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.LocalPayment, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.LocalPayment>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.LocalPayment_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.LocalPayment_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.LegacyPayment_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.LegacyPayment_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.ShippingInfo, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.ShippingInfo_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrderStatusTag, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrderStatusTag>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.OrderTotals, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrderTotals_V01, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrderTotals_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.AdditionalPaymentInformation, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AdditionalPaymentInformation>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.PaymentGatewayParameters, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayParameters>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.HLPrice, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.HLPrice>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals, MyHerbalife3.Shared.UI.OrderSvc.OrderTotals>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01, MyHerbalife3.Shared.UI.OrderSvc.OrderTotals_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ChargeList, MyHerbalife3.Shared.UI.OrderSvc.ChargesList>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge, MyHerbalife3.Shared.UI.OrderSvc.Charge>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge_V01, MyHerbalife3.Shared.UI.OrderSvc.Charge_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotalsList, MyHerbalife3.Shared.UI.OrderSvc.ItemTotalsList>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal, MyHerbalife3.Shared.UI.OrderSvc.ItemTotal>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal_V01, MyHerbalife3.Shared.UI.OrderSvc.ItemTotal_V01>();
                this.CreateMap<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ProductType, MyHerbalife3.Shared.UI.OrderSvc.ProductType>();
                this.CreateMap<HL.Mobile.ValueObjects.ShippingInfo, MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ShippingInfo>();
            }
        }

        #endregion

        #region Singleton Implementation

        private static readonly ObjectMappingHelper _ObjectMappingHelper = new ObjectMappingHelper();

        static ObjectMappingHelper()
        {
            Instance.RegisterProfiles();
        }

        public static ObjectMappingHelper Instance
        {
            get { return _ObjectMappingHelper; }
        }

        #endregion Singleton Implementation

        private void RegisterProfiles()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<DistShoppingCartItemMappingProfile>();
                x.AddProfile<AddressMappingProfile>();
                x.AddProfile<DeliveryOptionMappingProfile>();
                x.AddProfile<DistributorMappingProfile>();
                x.AddProfile<OrderMappingProfile>();
            });
        }

        #region Shopping cart
        public MyHerbalife3.Shared.ViewModel.Models.DistributorShoppingCartItem GetToShared(MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem distributorShoppingCartItem)
        {
            if (distributorShoppingCartItem == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.ViewModel.Models.DistributorShoppingCartItem>(distributorShoppingCartItem);
        }

        public MyHerbalife3.Shared.UI.CatalogSvc.ShoppingCartItem_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ShoppingCartItem_V01 shoppingCartItemV01)
        {
            if (shoppingCartItemV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.UI.CatalogSvc.ShoppingCartItem_V01>(shoppingCartItemV01);
        }

        public MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.SKU_V01 skuV01)
        {
            if (skuV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.UI.CatalogSvc.SKU_V01>(skuV01);
        }

        #endregion

        #region Address
        public HL.Common.ValueObjects.Address GetToCommon(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address address)
        {
            if (address == null)
                return null;

            return Mapper.Map<HL.Common.ValueObjects.Address>(address);
        }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address GetToShipping(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.Address address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>(address);
        }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 GetToShipping(MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.Address_V01 addressV01)
        {
            if (addressV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>(addressV01);
        }

        public MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01 GetToOrder(MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 addressV01)
        {
            if (addressV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01>(addressV01);
        }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 GetToShipping(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01 addressV01)
        {
            if (addressV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01>(addressV01);
        }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address GetToShipping(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address>(address);
        }

        public HL.Common.ValueObjects.Address_V01 GetToCommon(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V01 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<HL.Common.ValueObjects.Address_V01>(address);
        }

        public HL.Common.ValueObjects.Address_V01 GetToCommon(MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 addressV01)
        {
            if (addressV01 == null)
                return null;

            return Mapper.Map<HL.Common.ValueObjects.Address_V01>(addressV01);
        }

        public HL.Common.ValueObjects.Address_V01 GetToCommon(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01 addressV01)
        {
            if (addressV01 == null)
                return null;

            return Mapper.Map<HL.Common.ValueObjects.Address_V01>(addressV01);
        }

        public HL.Common.ValueObjects.Address_V02 GetToCommon(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<HL.Common.ValueObjects.Address_V02>(address);
        }

        public MyHerbalife3.Shared.LegacyProviders.DistributorService.Address GetToShared(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.LegacyProviders.DistributorService.Address>(address);
        }

        public MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01>(address);
        }

        public MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V01>(address);
        }

        public MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V02 GetToShared(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V02>(address);
        }

        public MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V03 GetToShared(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V03 address)
        {
            if (address == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.LegacyProviders.DistributorService.Address_V03>(address);
        }

        public HL.Common.ValueObjects.AddressCollection GetToCommon(MyHerbalife3.Core.DistributorProvider.DistributorSvc.AddressesCollection addresses)
        {
            if (addresses == null || !addresses.Any())
                return null;

            var newAddrColl = new HL.Common.ValueObjects.AddressCollection();
            if (addresses[0] != null && addresses[0].GetType() == typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address))
            {
                var addss = (from a in addresses select ObjectMappingHelper.Instance.GetToCommon(a as MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address)).ToList();
                newAddrColl.AddRange(addss);
            }
            if (addresses[0] != null && addresses[0].GetType() == typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V01))
            {
                var addss1 = (from a in addresses select ObjectMappingHelper.Instance.GetToCommon(a as MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V01)).ToList();
                newAddrColl.AddRange(addss1);
            }
            if (addresses[0] != null && addresses[0].GetType() == typeof(MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02))
            {
                var addss2 = (from a in addresses select ObjectMappingHelper.Instance.GetToCommon(a as MyHerbalife3.Core.DistributorProvider.DistributorSvc.Address_V02)).ToList();
                newAddrColl.AddRange(addss2);
            }
            return newAddrColl;
        }

        #endregion

        #region Delivery option
        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.PickupOption_V01 GetToShipping(MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.PickupOption_V01 pickupOptionV01)
        {
            if (pickupOptionV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.PickupOption_V01>(pickupOptionV01);
        }

        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingOption_V01 GetToShipping(MyHerbalife3.Ordering.ServiceProvider.ShippingMexicoSvc.ShippingOption_V01 shippingOptionV01)
        {
            if (shippingOptionV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingOption_V01>(shippingOptionV01);
        }

        #endregion

        #region Distributor
        public MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorTinList GetToDistributor(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorTinList dsTinList)
        {
            if (dsTinList == null)
                return null;

            var tinList = (from t in dsTinList select ObjectMappingHelper.Instance.GetToDistributor(t)).ToList();
            var newTinList = new MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorTinList();
            newTinList.AddRange(tinList);
            return newTinList;
        }

        public MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification GetToDistributor(MyHerbalife3.Core.DistributorProvider.DistributorSvc.TaxIdentification taxId)
        {
            if (taxId == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification>(taxId);
        }

        public MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorVolume_V01 GetToDistributor(MyHerbalife3.Core.DistributorProvider.DistributorSvc.DistributorVolume_V01 dsVolumeV01)
        {
            if (dsVolumeV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.DistributorVolume_V01>(dsVolumeV01);
        }

        public MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03 GetToDistributor(MyHerbalife3.Core.DistributorProvider.DistributorSvc.PhoneNumber_V03 phoneNumberV03)
        {
            if (phoneNumberV03 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03>(phoneNumberV03);
        }
        public MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03 GetToDistributor(MyHerbalife3.Core.DistributorProvider.DistributorSvc.TrackedValue_V01PhoneNumberBase phoneNumberV03)
        {
            if (phoneNumberV03 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.PhoneNumber_V03>(phoneNumberV03.Value);
        }
        #endregion

        #region Order
        public MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01 GetToOrder(MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc.CustomerOrder_V01 customerOrderV01)
        {
            if (customerOrderV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Ordering.ServiceProvider.OrderSvc.CustomerOrder_V01>(customerOrderV01);
        }

        public MyHerbalife3.Shared.UI.OrderSvc.OrderTotals_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01 orderTotalsV01)
        {
            if (orderTotalsV01 == null)
                return null;

            var data = Mapper.Map<MyHerbalife3.Shared.UI.OrderSvc.OrderTotals_V01>(orderTotalsV01);
            data.ChargeList = ObjectMappingHelper.Instance.GetToShared(orderTotalsV01.ChargeList);
            data.ItemTotalsList = ObjectMappingHelper.Instance.GetToShared(orderTotalsV01.ItemTotalsList);
            return data;
        }

        public MyHerbalife3.Shared.UI.OrderSvc.Charge GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge charge)
        {
            if (charge == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.UI.OrderSvc.Charge>(charge);
        }

        public MyHerbalife3.Shared.UI.OrderSvc.Charge_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge_V01 chargeV01)
        {
            if (chargeV01 == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.UI.OrderSvc.Charge_V01>(chargeV01);
        }

        public MyHerbalife3.Shared.UI.OrderSvc.ChargesList GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ChargeList chargeList)
        {
            if (chargeList == null)
                return null;

            var newChargeList = new MyHerbalife3.Shared.UI.OrderSvc.ChargesList();
            if (chargeList.Count > 0 && chargeList[0].GetType() == typeof(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge))
            {
                var cList = (from c in chargeList select ObjectMappingHelper.Instance.GetToShared(c)).ToList();
                newChargeList.AddRange(cList);
            }
            else if (chargeList.Count > 0 && chargeList[0].GetType() == typeof(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge_V01))
            {
                var cList = (from c in chargeList select ObjectMappingHelper.Instance.GetToShared(c as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge_V01)).ToList();
                newChargeList.AddRange(cList);
            }
            return newChargeList;
        }

        public MyHerbalife3.Shared.UI.OrderSvc.ItemTotal GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal itemTotal)
        {
            if (itemTotal == null)
                return null;

            return Mapper.Map<MyHerbalife3.Shared.UI.OrderSvc.ItemTotal>(itemTotal);
        }

        public MyHerbalife3.Shared.UI.OrderSvc.ItemTotal_V01 GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal_V01 itemTotalV01)
        {
            if (itemTotalV01 == null)
                return null;

            var itemTotal = Mapper.Map<MyHerbalife3.Shared.UI.OrderSvc.ItemTotal_V01>(itemTotalV01);
            itemTotal.ChargeList = ObjectMappingHelper.Instance.GetToShared(itemTotalV01.ChargeList);
            return itemTotal;
        }

        public MyHerbalife3.Shared.UI.OrderSvc.ItemTotalsList GetToShared(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotalsList itemTotalsList)
        {
            if (itemTotalsList == null)
                return null;

            var newItemTotalsList = new MyHerbalife3.Shared.UI.OrderSvc.ItemTotalsList();
            if (itemTotalsList.Count > 0 && itemTotalsList[0].GetType() == typeof(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal))
            {
                var iList = (from i in itemTotalsList select ObjectMappingHelper.Instance.GetToShared(i)).ToList();
                newItemTotalsList.AddRange(iList);
            }
            else if (itemTotalsList.Count > 0 && itemTotalsList[0].GetType() == typeof(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal_V01))
            {
                var iList = (from i in itemTotalsList select ObjectMappingHelper.Instance.GetToShared(i as MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal_V01)).ToList();
                newItemTotalsList.AddRange(iList);
            }
            return newItemTotalsList;
        }
        #endregion
    }
}
