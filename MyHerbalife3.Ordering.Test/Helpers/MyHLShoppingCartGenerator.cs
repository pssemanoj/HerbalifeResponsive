// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyHLShoppingCartGenerator.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Helper class to generate a shopping cart.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Providers;
using HL.Catalog.ValueObjects;
using HL.Common.ValueObjects;
using HL.Order.ValueObjects;
using HL.Shipping.ValueObjects;
using System;
using System.Linq;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
namespace MyHerbalife3.Ordering.Test.Helpers
{
    /// <summary>
    /// Helper class to generate a shopping cart.
    /// </summary>
    internal static class MyHLShoppingCartGenerator
    {
        /// <summary>
        /// Gets the shopping cart.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns>
        /// Shopping cart with the parameters.
        /// </returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale)
        {
            return GetBasicShoppingCart(distributorId, locale, null, null, false, null);
        }

        /// <summary>
        /// Gets the shopping cart.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="totals">The totals.</param>
        /// <returns>
        /// Shopping cart with the parameters.
        /// </returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, OrderTotals_V01 totals)
        {
            return GetBasicShoppingCart(distributorId, locale, null, null, false, totals);
        }
        
        internal static ShoppingCart_V02 GetShoppingCart(int cartID, DateTime lastUpdated, string locale, int deliveryOptionID, OrderCategoryType orderCategory, DeliveryOptionType deliveryOption, int shippingAddressID, string distributorID, string freightCode, string orderSubType)
        {
            var itemList = new ShoppingCartItemList { new ShoppingCartItem_V01() };

            var orderDetail = new CustomerOrderDetail();

            return new ShoppingCart_V02(cartID, lastUpdated, locale, deliveryOptionID, orderCategory, deliveryOption, shippingAddressID, itemList, distributorID, freightCode, orderSubType, orderDetail);
        }

        /// <summary>
        /// Gets the shopping cart.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="freightCode">The freight code.</param>
        /// <param name="wareHouseCode">The ware house code.</param>
        /// <param name="calculate">if set to <c>true</c> [calculate].</param>
        /// <param name="totals">The totals.</param>
        /// <returns>
        /// Shopping cart with the parameters.
        /// </returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals)
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
                    Option = DeliveryOptionType.Shipping,
                    Address = new ShippingAddress_V02
                    {
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        Recipient = string.Empty
                    }
                },
                CountryCode = locale.Substring(3)
            };

            // Calculate totals.
            if (calculate)
            {
                shoppingCart.Totals = shoppingCart.Calculate();
            }
            else
            {
                // Use dummy totals
                shoppingCart.Totals = totals ?? new OrderTotals_V01
                {
                    AmountDue = 1000M,
                    BalanceAmount = 900M,
                    DiscountPercentage = 50M,
                    VolumePoints = 1000
                };
            }

            return shoppingCart;
        }

        /// <summary>
        /// Gets the basic shopping cart.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="freightCode">The freight code.</param>
        /// <param name="wareHouseCode">The ware house code.</param>
        /// <param name="calculate">if set to <c>true</c> [calculate].</param>
        /// <param name="totals">The totals.</param>
        /// <param name="deliveryOption">The delivery option.</param>
        /// <param name="sku">The sku.</param>
        /// <returns></returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals, DeliveryOptionType deliveryOption, string sku)
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
                ShoppingCartItems = new List<DistributorShoppingCartItem>
                        {
                            ShoppingCartItemHelper.GetShoppingCartItem(1, 1, sku)
                        },
                CartItems = new ShoppingCartItemList
                        {
                            ShoppingCartItemHelper.GetCartItem(1, 1, sku)
                        },
                DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                FreightCode = freightCode,
                DeliveryInfo = new ShippingInfo
                {
                    FreightCode = freightCode,
                    WarehouseCode = wareHouseCode,
                    Option = deliveryOption,
                    Address = new ShippingAddress_V02
                    {
                        FirstName = string.Empty,
                        LastName = string.Empty,
                        Recipient = string.Empty
                    }
                },
                CountryCode = locale.Substring(3)
            };

            // Calculate totals.
            if (calculate)
            {
                shoppingCart.Totals = shoppingCart.Calculate();
            }
            else
            {
                // Use dummy totals
                shoppingCart.Totals = totals ?? new OrderTotals_V01
                {
                    AmountDue = 1000M,
                    BalanceAmount = 900M,
                    DiscountPercentage = 50M,
                    VolumePoints = 1000
                };
            }

            return shoppingCart;
        }

        /// <summary>
        /// CurrentItems and CartItems are same
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="locale"></param>
        /// <param name="freightCode"></param>
        /// <param name="wareHouseCode"></param>
        /// <param name="calculate"></param>
        /// <param name="totals"></param>
        /// <param name="ShoppingCartItem"></param>
        /// <param name="CartItem"></param>
        /// <param name="OrderCType"></param>
        /// <returns></returns>
        [Obsolete("Please use GetBasicShoppingCartstring distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals, List<DistributorShoppingCartItem> shoppingCartItem, OrderCategoryType orderCType) instead.", false)]
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals, List<DistributorShoppingCartItem> ShoppingCartItem, ShoppingCartItemList CartItem, OrderCategoryType OrderCType)
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
                ShoppingCartItems = ShoppingCartItem,
                CartItems = CartItem,
                DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                FreightCode = freightCode,
                CurrentItems = CartItem,
                DeliveryInfo = new ShippingInfo
                {

                    FreightCode = freightCode,
                    WarehouseCode = wareHouseCode,
                    Option = DeliveryOptionType.Shipping,
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

            // Calculate totals.
            if (calculate)
            {
                shoppingCart.Totals = shoppingCart.Calculate();
            }
            else
            {
                // Use dummy totals
                shoppingCart.Totals = totals ?? new OrderTotals_V01
                {
                    AmountDue = 1000M,
                    BalanceAmount = 900M,
                    DiscountPercentage = 50M,
                    VolumePoints = 1000
                };
            }

            // Rules results
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();

            return shoppingCart;
        }

        /// <summary>
        /// CurrentItems and CartItems are different
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="locale"></param>
        /// <param name="freightCode"></param>
        /// <param name="wareHouseCode"></param>
        /// <param name="calculate"></param>
        /// <param name="totals"></param>
        /// <param name="ShoppingCartItem"></param>
        /// <param name="CartItem"></param>
        /// <param name="CurrentItem"></param>
        /// <param name="OrderCType"></param>
        /// <returns></returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals, List<DistributorShoppingCartItem> ShoppingCartItem, ShoppingCartItemList CartItem, ShoppingCartItemList CurrentItem, OrderCategoryType OrderCType)
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
                ShoppingCartItems = ShoppingCartItem,
                CartItems = CartItem,
                DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                FreightCode = freightCode,
                CurrentItems = CurrentItem,
                DeliveryInfo = new ShippingInfo
                {
                    FreightCode = freightCode,
                    WarehouseCode = wareHouseCode,
                    Option = DeliveryOptionType.Shipping,
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

            // Calculate totals.
            if (calculate)
            {
                shoppingCart.Totals = shoppingCart.Calculate();
            }
            else
            {
                // Use dummy totals
                shoppingCart.Totals = totals ?? new OrderTotals_V01
                {
                    AmountDue = 1000M,
                    BalanceAmount = 900M,
                    DiscountPercentage = 50M,
                    VolumePoints = 1000
                };
            }

            // Rules results
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();

            return shoppingCart;
        }

        /// <summary>
        /// Gets the basic shopping cart.
        /// </summary>
        /// <param name="distributorId">The distributor id.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="freightCode">The freight code.</param>
        /// <param name="wareHouseCode">The ware house code.</param>
        /// <param name="calculate">if set to <c>true</c> [calculate].</param>
        /// <param name="totals">The totals.</param>
        /// <param name="shoppingCartItem">The shopping cart item.</param>
        /// <param name="orderCType">Type of the order C.</param>
        /// <returns></returns>
        internal static MyHLShoppingCart GetBasicShoppingCart(string distributorId, string locale, string freightCode, string wareHouseCode, bool calculate, OrderTotals_V01 totals, List<DistributorShoppingCartItem> shoppingCartItem, OrderCategoryType orderCType)
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

            var cartItemList = new ShoppingCartItemList();
            cartItemList.AddRange(shoppingCartItem.Select(i => new ShoppingCartItem_V01
                {
                    ID = i.ID,
                    MinQuantity = i.MinQuantity,
                    PartialBackordered = i.PartialBackordered,
                    Quantity = i.Quantity,
                    SKU = i.SKU,
                    Updated = i.Updated
                }));

            var shoppingCart = new MyHLShoppingCart
                {
                    Locale = locale,
                    ShoppingCartItems = shoppingCartItem,
                    CartItems = cartItemList,
                    DistributorID = string.IsNullOrEmpty(distributorId) ? "webtest1" : distributorId,
                    FreightCode = freightCode,
                    CurrentItems = cartItemList,
                    DeliveryInfo = new ShippingInfo
                        {
                            FreightCode = freightCode,
                            WarehouseCode = wareHouseCode,
                            Option = DeliveryOptionType.Shipping,
                            Address = new ShippingAddress_V02
                                {
                                    FirstName = string.Empty,
                                    LastName = string.Empty,
                                    Recipient = string.Empty
                                }
                        },
                    CountryCode = locale.Substring(3),
                    OrderCategory = orderCType
                };

            // Calculate totals.
            if (calculate)
            {
                shoppingCart.Totals = shoppingCart.Calculate();
            }
            else
            {
                // Use dummy totals
                shoppingCart.Totals = totals ?? new OrderTotals_V01
                {
                    AmountDue = 1000M,
                    BalanceAmount = 900M,
                    DiscountPercentage = 50M,
                    VolumePoints = 1000
                };
            }

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
