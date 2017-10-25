using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using System.Xml.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Providers
{

    public class DateTimeSurrogate : IDataContractSurrogate
    {

        #region IDataContractSurrogate

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            return obj;
        }

        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {

        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            if (obj.GetType() == typeof(DateTime))
            {
                DateTime dt = (DateTime)obj;
                if (dt == DateTime.MinValue)
                {
                    dt = DateTime.MinValue.ToUniversalTime();
                    return dt;
                }
                return dt;
            }
            if (obj == null)
            {
                return null;
            }
            var q = from p in obj.GetType().GetProperties()
                    where (p.PropertyType == typeof(DateTime)) && (DateTime)p.GetValue(obj, null) == DateTime.MinValue
                    select p;
            q.ToList().ForEach(p =>
            {
                p.SetValue(obj, DateTime.MinValue.ToUniversalTime(), null);
            });
            return obj;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            return null;
        }

        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
        {
            return typeDeclaration;
        }

        #endregion
    }


    public static partial class OrderCreationHelper
    {

        public static string Serialize<T>(T configuration)
        {
            try
            {
                Type[] knownTypes = new Type[] { typeof(ServiceProvider.OrderChinaSvc.OrderTotals_V02), typeof(ServiceProvider.OrderSvc.ItemTotal_V01) };

                var serializer = new DataContractJsonSerializer(configuration.GetType(), knownTypes, int.MaxValue, false, new DateTimeSurrogate(), false);
                var stream = new MemoryStream();
                serializer.WriteObject(stream, configuration);
                string result = Encoding.UTF8.GetString(stream.GetBuffer().Where(b => b != '\0').ToArray());
                return result;
            }
            catch 
            {
                return string.Empty;
            }
            
        }

        public static string GetSerializedString<T>(T value)
        {
            Type[] knownTypes = new Type[] { typeof(OrderTotals_V02), typeof(ServiceProvider.OrderChinaSvc.ItemTotal_V01) };
            Type typeInput = typeof(T);
            XmlSerializer ser = null;
            if (typeInput == typeof(T))
            {
                ser = new XmlSerializer(typeof(T), knownTypes);
            }
            else if (typeInput == typeof(T))
            {
                ser = new XmlSerializer(typeof(T), knownTypes);
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter writer = new System.IO.StringWriter(sb);
            try
            {
                ser.Serialize(writer, value);
                return sb.ToString();
            }
            catch
            {
                return "Unable to Serialize value";
            }
        }

        public static ItemTotalsList InitializeTotals(Order_V01 order)
        {
            var itemTotals = new ItemTotalsList();
            if (order != null)
            {
                itemTotals.AddRange(from oItem in order.OrderItems
                                    where oItem is OrderItem_V01
                                    select
                                        (ItemTotal)
                                        new ItemTotal_V01(){ SKU = (oItem).SKU, Quantity = 0, LinePrice = 0, RetailPrice = 0, DiscountedPrice = 0, LineTax = 0, EarnBase = 0, ChargeList = null});
            }
            return (ItemTotalsList) itemTotals.OrderBy(i => (i as ItemTotal_V01).SKU);
        }

        public static Order CreateOrderObject(List<ShoppingCartItem_V01> cartItems)
        {
            var order = new Order_V01();
           
            var sortedItems = new List<OrderItem>();

            sortedItems.AddRange(from item in cartItems
                                 where null != item
                                 select (OrderItem) new OrderItem_V01(){ SKU = item.SKU.Trim(), Quantity = item.Quantity });
            order.OrderItems = new OrderItems();
            order.OrderItems.AddRange(sortedItems.OrderBy(i => i.SKU).ToList());

            return order;
        }

        public static Order CreateOrderObject(List<Shared.ViewModel.CatalogSvc.ShoppingCartItem_V01> cartItems)
        {
            var order = new Order_V01();

            var sortedItems = new List<OrderItem>();

            sortedItems.AddRange(from item in cartItems
                                 where null != item
                                 select (OrderItem)new OrderItem_V01() { SKU = item.SKU.Trim(), Quantity = item.Quantity });
            order.OrderItems = new OrderItems();
            order.OrderItems.AddRange(sortedItems.OrderBy(i => i.SKU).ToList());

            return order;
        }

        public static Order CreateOrderObject(MyHLShoppingCart shoppingCart)
        {
            var order = HLConfigManager.Configurations.DOConfiguration.IsChina ?
                 China.OrderProvider.CreateOrderObject(
                    (from s in shoppingCart.ShoppingCartItems select (DistributorShoppingCartItem) s)
                    .ToList<DistributorShoppingCartItem>()) as Order 
            :
                CreateOrderObject(
                    (from s in shoppingCart.ShoppingCartItems select (ShoppingCartItem_V01) s)
                        .ToList<ShoppingCartItem_V01>()) as Order_V01;
            Order_V01 orderV01 = order as Order_V01;
            if (PurchasingLimitProvider.RequirePurchasingLimits(shoppingCart.DistributorID, shoppingCart.CountryCode))
            {
                orderV01.PurchasingLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(shoppingCart.DistributorID);
            }
            if (HLConfigManager.Configurations.DOConfiguration.UsesTaxRules)
            {
                if (null != order)
                {
                    if (null != shoppingCart.DeliveryInfo)
                    {
                        orderV01.Shipment = GetShippingInfoFromCart(shoppingCart);
                    }
                    HLRulesManager.Manager.PerformTaxationRules(orderV01, shoppingCart.Locale);
                }
            }

            return order;
        }

        public static ServiceProvider.OrderSvc.Address_V01 CreateDefaultAddress()
        {
            var address = new ServiceProvider.OrderSvc.Address_V01();
            address.Country = "US";
            address.Line1 = "950 West 190th St.,";
            address.City = "Torrance";
            address.PostalCode = "90502-1001";
            address.StateProvinceTerritory = "CA";
            return address;
        }

        private static bool addressNotValid(ShippingInfo deliveryInfo)
        {
            if (deliveryInfo == null || deliveryInfo.Address == null ||
                deliveryInfo.Address.Address == null ||
                (string.IsNullOrEmpty(deliveryInfo.Address.Address.PostalCode) &&
                 string.IsNullOrEmpty(deliveryInfo.Address.Address.Line1) &&
                 string.IsNullOrEmpty(deliveryInfo.Address.Address.City))
                )
            {
                return true;
            }

            return false;
        }

        public static Order FillOrderInfo(Order _order, MyHLShoppingCart shoppingCart)
        {
            Order_V01 order = _order as Order_V01;
            order.DistributorID = shoppingCart.DistributorID;
            order.Shipment = GetShippingInfoFromCart(shoppingCart);
            order.InputMethod = InputMethodType.Internet;
            var recvdDate = DateUtils.GetCurrentLocalTime(shoppingCart.Locale.Substring(3, 2));
            order.ReceivedDate = recvdDate;
            
            // TODO : Order CATEGORY
            order.OrderCategory =
                (ServiceProvider.OrderSvc.OrderCategoryType)Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), shoppingCart.OrderCategory.ToString());
            //OrderCategoryType.RSO;
            if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.ETO)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType))
                {
                    order.OrderCategory =
                        (ServiceProvider.OrderSvc.OrderCategoryType)
                        Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                                   HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType);
                }
            }

            var orderMonth = new OrderMonth(shoppingCart.CountryCode);
            order.OrderMonth = orderMonth.OrderMonthShortString;
            //order.OrderMonth = DateTime.Now.ToString("yyMM");

            order.UseSlidingScale = HLConfigManager.Configurations.CheckoutConfiguration.UseSlidingScale;
            order.CountryOfProcessing = shoppingCart.CountryCode;

            DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID, shoppingCart.CountryCode);
            if (null != distributorOrderingProfile)
            {
                //order.DiscountPercentage = order.UseSlidingScale && distributorOrderingProfile.StaticDiscount < 50
                //                               ? 0
                //                               : distributorOrderingProfile.StaticDiscount;
                order.DiscountPercentage =  distributorOrderingProfile.StaticDiscount; // always pass profile discount
            }
            if (HLConfigManager.Configurations.DOConfiguration.UsesDiscountRules)
            {
                HLRulesManager.Manager.PerformDiscountRules(shoppingCart, order, shoppingCart.Locale, ShoppingCartRuleReason.CartBeingCalculated );
            }
            if (HLConfigManager.Configurations.DOConfiguration.usesOrderManagementRules)
            {
                HLRulesManager.Manager.PerformOrderManagementRules(shoppingCart, order, shoppingCart.Locale, OrderManagementRuleReason.OrderFilled);
            }

            // some countries, like IT, need OrderSubType for correct Fusion pricing.
            // Added for User Story 226150
            if (HLConfigManager.Configurations.DOConfiguration.SaveDSSubType)
            {
                order.OrderSubType = shoppingCart.SelectedDSSubType;
            }
            return order;
        }

        public static ShippingInfo_V01 GetShippingInfoFromCart(MyHLShoppingCart cart)
        {
            var shipping = new ShippingInfo_V01();
            var deliveryInfo = cart.DeliveryInfo;
            if (deliveryInfo == null)
            {
                shipping.FreightVariant = HLConfigManager.Configurations.DOConfiguration.IsChina
                                              ? "EXP"
                                              : ShippingProvider.GetShippingProvider(cart.CountryCode).GetFreightVariant(null);
                shipping.ShippingMethodID = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                shipping.WarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
            }
            else
            {
                shipping.FreightVariant = HLConfigManager.Configurations.DOConfiguration.IsChina
                                              ? deliveryInfo.AddressType
                                              : deliveryInfo.FreightVariant;
                shipping.ShippingMethodID = deliveryInfo.FreightCode;
                shipping.WarehouseCode = deliveryInfo.WarehouseCode;
            }
            shipping.Address = addressNotValid(deliveryInfo) ? CreateDefaultAddress() : ObjectMappingHelper.Instance.GetToOrder(deliveryInfo.Address.Address);

            var provider = ShippingProvider.GetShippingProvider(null);
            if (provider != null)
            {
                provider.GetDistributorShippingInfoForHMS(cart, shipping);
            }
            return shipping;
        }


        
    }
}