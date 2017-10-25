
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    /// <summary>
    /// Helper class to generate a shopping cart.
    /// </summary>
    internal static class MyHLShoppingCartGenerator
    {
        internal static ShoppingCart_V02 GetShoppingCart(int cartID, DateTime lastUpdated, string locale, int deliveryOptionID, MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType orderCategory, MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.DeliveryOptionType deliveryOption, int shippingAddressID, string distributorID, string freightCode, string orderSubType)
        {
            var itemList = new ShoppingCartItemList { new ShoppingCartItem_V01() };

            var orderDetail = new CustomerOrderDetail();

            return new ShoppingCart_V02() { ShoppingCartID = cartID, LastUpdated = lastUpdated, Locale = locale, DeliveryOptionID = deliveryOptionID, OrderCategory = orderCategory, DeliveryOption = deliveryOption, ShippingAddressID = shippingAddressID, CartItems = itemList, DistributorID = distributorID, FreightCode = freightCode, OrderSubType = orderSubType, CustomerOrderDetail = orderDetail };
        }

        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, ShippingAddress_V02 shiptoaddress)
        {
            if (string.IsNullOrEmpty(locale))
            {
                locale = "en-US";
            }

            var shoppingCart = new MyHLShoppingCart
            {
                Locale = locale,
                ShoppingCartItems = new List<DistributorShoppingCartItem>
                        {
                            ShoppingCartItemHelper.GetShoppingCartItem(1, 1, "1231"),
                            ShoppingCartItemHelper.GetShoppingCartItem(2, 1, "0138"),
                            ShoppingCartItemHelper.GetShoppingCartItem(3, 1, "0139")
                        },
                CartItems = new ShoppingCartItemList
                        {
                            ShoppingCartItemHelper.GetCartItem(1, 1, "1231"),
                            ShoppingCartItemHelper.GetCartItem(2, 1, "0138"),
                            ShoppingCartItemHelper.GetCartItem(3, 1, "0139")
                        },
                DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                FreightCode = freightCode,
                DeliveryInfo = new ShippingInfo
                {
                    FreightCode = freightCode,
                    WarehouseCode = wareHouseCode,
                    Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping,
                    Address = shiptoaddress
                },
                CountryCode = locale.Substring(3)
            };
            shoppingCart.Totals = new OrderTotals_V01()
            {
                AmountDue = 1000M,
                BalanceAmount = 900M,
                DiscountPercentage = 50M,
                VolumePoints = 1000
            };

            return shoppingCart;
        }

        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, ShoppingCartItemList CartItem, MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType OrderCType)
        {
            if (string.IsNullOrEmpty(locale))
            {
                locale = "en-US";
            }

            if (string.IsNullOrEmpty(freightCode))
            {
                freightCode = HLConfigManager.CurrentPlatformConfigs[locale].ShoppingCartConfiguration.DefaultFreightCode;
            }

            if (string.IsNullOrEmpty(wareHouseCode))
            {
                wareHouseCode = HLConfigManager.CurrentPlatformConfigs[locale].ShoppingCartConfiguration.DefaultWarehouse;
            }

            var shoppingCart = new MyHLShoppingCart
            {
                Locale = locale,
                ShoppingCartItems = (from c in CartItem
                                     select new DistributorShoppingCartItem { SKU = c.SKU, Quantity = c.Quantity }).ToList(),
                CartItems = CartItem,
                DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                FreightCode = freightCode,
                CurrentItems = CartItem,
                DeliveryInfo = new ShippingInfo
                {

                    FreightCode = freightCode,
                    WarehouseCode = wareHouseCode,
                    Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping,
                    Address = new ShippingAddress_V02
                    {
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        Recipient = string.Empty
                    }
                },
                CountryCode = locale.Substring(3),
                OrderCategory = OrderCType
            };

            shoppingCart.Totals = new OrderTotals_V01
            {
                AmountDue = 1000M,
                BalanceAmount = 900M,
                DiscountPercentage = 50M,
                VolumePoints = 1000
            };
        
            // Rules results
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();

            return shoppingCart;
        }

        /// <summary>
        /// Adds to cart.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="item">The item.</param>
        internal static void PrepareAddToCart(MyHLShoppingCart cart, ShoppingCartItem_V01 item)
        {
            var list = new ShoppingCartItemList 
            {
                item
            };

            //cart.CartItems.Add(item);
            cart.CurrentItems = list;
        }
    }
}
