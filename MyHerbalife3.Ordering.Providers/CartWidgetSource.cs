#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using HL.Blocks.Caching.SimpleCache;
using HL.Blocks.CircuitBreaker;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShoppingCartSvc;
using OrderTotals_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01;
using ItemTotal_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ItemTotal_V01;
using ProductType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType;

#endregion

namespace MyHerbalife3.Ordering.Providers
{
    public class CartWidgetSource : ICartWidgetSource
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();

        public CartWidgetModel GetCartWidget(string id, string countryCode, string locale)
        {
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            if (string.IsNullOrEmpty(locale))
            {
                throw new ArgumentException("Locale is blank", "locale");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }

            var cacheKey = string.Format("MyHL3_SC_{0}_{1}", id, locale);
            var result = _cache.Retrieve(_ => GetCartWidgetFromSource(id, countryCode, locale), cacheKey,
                                         TimeSpan.FromMinutes(15));
            return result;
        }

        public bool SetCartWidgetCache(MyHLShoppingCart myHLShoppingCart)
        {
            var cacheKey = string.Format("MyHL3_SC_{0}_{1}", myHLShoppingCart.DistributorID, myHLShoppingCart.Locale);
            var result = _cache.Retrieve(_ => GetCartWidgetModelFromShoppingCart(myHLShoppingCart), cacheKey,
                                         TimeSpan.FromMinutes(15));
            return result != null;
        }

        private CartWidgetModel GetCartWidgetModelFromShoppingCart(MyHLShoppingCart myHLShoppingCart)
        {
            CartWidgetModel response = new CartWidgetModel();
            if (myHLShoppingCart != null)
            {
                if (myHLShoppingCart.Totals == null)
                {
                    myHLShoppingCart.Calculate(myHLShoppingCart.CartItems);
                }
                var total = myHLShoppingCart.Totals as OrderTotals_V01;

                if (total != null)
                {
                    if (myHLShoppingCart.Locale == "ko-KR")
                    {
                        decimal subtotal = 0m;
                        if (total.ItemTotalsList != null)
                        {
                            subtotal = total.ItemTotalsList.Sum(itemTotal => getDistributorPrice(itemTotal as ItemTotal_V01, total.DiscountPercentage, myHLShoppingCart.Locale));
                        }
                        response.Subtotal = subtotal;
                    }
                    else if (myHLShoppingCart.Locale.Equals("es-MX"))
                    {
                        response.Subtotal = OrderProvider.getPriceWithAllCharges(total);
                    }
                    else
                    {
                        response.Subtotal = total.DiscountedItemsTotal;
                    }

                    response.VolumePoints = total.VolumePoints;
                    response.DisplaySubtotal = getAmountString(response.Subtotal);
                }
                response.Id = myHLShoppingCart.ShoppingCartID;
                response.Quantity = myHLShoppingCart.CartItems.Sum(x => x.Quantity);
            }
            return response;
        }


        public static decimal getDistributorPrice(ItemTotal_V01 lineItem, decimal discount, string locale)
        {
            var prodType = ProductType.Product;
            var catalogItem = CatalogProvider.GetCatalogItem(lineItem.SKU, locale.Substring(3));
            if (catalogItem != null)
            {
                prodType = catalogItem.ProductType;
            }
            int marketingFundPercent = discount == 31 ? 10 : 5;
            var roundMode = MidpointRounding.AwayFromZero;
            decimal PH = prodType == ProductType.Literature ? 0 : Math.Round(lineItem.LinePrice * 7 / 100.0M, roundMode);
            decimal marketingFund = (lineItem.SKU == "4115" || lineItem.SKU == "4116")
                                        ? 0
                                        : Math.Round(lineItem.LinePrice * marketingFundPercent / 1000.0M, roundMode);
            decimal discountAmt = prodType == ProductType.Product
                                      ? Math.Round(lineItem.LinePrice * discount / 100.0M, roundMode)
                                      : 0;
            decimal discountedPrice = lineItem.LinePrice - discountAmt;
            decimal tax = Math.Round(((discountedPrice + PH + marketingFund) * 10) / 100.0M, roundMode);

            return discountedPrice + tax + marketingFund + PH;
        }

        public CartWidgetModel AddToCart(CartWidgetModel cartWidget, string id, string countryCode, string locale)
        {
            if (null == cartWidget)
            {
                throw new ArgumentException("cartWidget is null", "cartWidget");
            }
            if (string.IsNullOrEmpty(countryCode))
            {
                throw new ArgumentException("countryCode is blank", "countryCode");
            }

            if (string.IsNullOrEmpty(locale))
            {
                throw new ArgumentException("Locale is blank", "locale");
            }

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id is blank", "id");
            }

            var cartModel = GetCartWidget(id, countryCode, locale);
            if (null != cartModel)
            {
                cartWidget.Id = cartModel.Id;
                var proxy = ServiceClientProvider.GetShoppingCartServiceProxy();
                try
                {
                    var circuitBreaker =
                        CircuitBreakerFactory.GetFactory().GetCircuitBreaker<AddItemsToCartResponse_V01>();
                    var response =
                        circuitBreaker.Execute(() => proxy.AddItemsToCart(new AddItemsToCartRequest1(new AddItemsToCartRequest_V01
                            {
                                ShoppingCartID = cartWidget.Id,
                                OrderItems =
                                    new List<ServiceProvider.ShoppingCartSvc.OrderItem>
                                        {
                                            new ServiceProvider.ShoppingCartSvc.OrderItem {Quantity = cartWidget.Quantity, SKU = cartWidget.Sku}
                                        },
                                Platform = "MyHL",
                                DistributorId = id,
                                Locale = locale
                            }))).AddItemsToCartResult as AddItemsToCartResponse_V01;

                    if (response != null && response.Status == ServiceProvider.ShoppingCartSvc.ServiceResponseStatusType.Success)
                    {
                        ExpireShoppingCartCache(id, locale);
                        ClearShoppingCartFromSession(id, locale);
                        return new CartWidgetModel
                            {
                                Id = response.ShoppingCartID,
                                Quantity = response.TotalItems,
                                Subtotal = response.Subtotal,
                                DisplaySubtotal =
                                    String.Format("{0}{1}",
                                                  HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                                                  response.Subtotal)
                            };
                    }

                    LogErrorIfAny(response);

                    return null;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("Error", ex,
                                           "Errored out in AddToCart");
                    if (null != proxy)
                    {
                        proxy.Close();
                    }
                    throw;
                }
                finally
                {
                    if (null != proxy)
                    {
                        proxy.Close();
                    }
                }
            }
            return null;
        }

        private void ClearShoppingCartFromSession(string id, string locale)
        {
            if (null == HttpContext.Current || null == HttpContext.Current.Session) return;
            var sessionInfo = SessionInfo.GetSessionInfo(id, locale);
            if (null != sessionInfo)
            {
                sessionInfo.ShoppingCart = null;
            }
        }

        public void ExpireShoppingCartCache(string id, string locale)
        {
            _cache.Expire(typeof (CartWidgetModel), string.Format("MyHL3_SC_{0}_{1}", id, locale));
        }

        public CartWidgetModel GetCartWidgetFromSource(string id, string countryCode, string locale)
        {
            var proxy = ServiceClientProvider.GetShoppingCartServiceProxy();
            try
            {
                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetShoppingCartResponse_V01>();
                var response =
                    circuitBreaker.Execute(() => proxy.GetShoppingCart(new GetShoppingCartRequest1(new GetShoppingCartRequest_V01
                        {
                            Locale = locale,
                            DistributorId = id,
                            Platform = "MyHL"
                        })).GetShoppingCartResult as GetShoppingCartResponse_V01);

                if (response != null && response.Status == ServiceProvider.ShoppingCartSvc.ServiceResponseStatusType.Success)
                {
                    return GetCartWidgetModel(response);
                }
                LogErrorIfAny(response);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex,
                                       "Errored out in GetCartWidgetFromSource " + id + countryCode);
                if (null != proxy)
                {
                    proxy.Close();
                }
                throw;
            }
            finally
            {
                if (null != proxy)
                {
                    proxy.Close();
                }
            }
            
            return new CartWidgetModel();
        }

        private static void LogErrorIfAny(ServiceProvider.ShoppingCartSvc.ServiceResponseValue response)
        {
            if (response != null && response.Status != ServiceProvider.ShoppingCartSvc.ServiceResponseStatusType.Success &&
                null != response.ParameterErrorList &&
                response.ParameterErrorList.Any())
            {
                foreach (var error in response.ParameterErrorList)
                {
                    LoggerHelper.Error(
                        string.Format("CartWidgetSource(ShoppingCartservice) Error:{0} Name:{1} Value:{2}", error.Error,
                                      error.Name, error.Value));
                }
            }
        }

        private static CartWidgetModel GetCartWidgetModel(GetShoppingCartResponse_V01 shoppingCartResponse)
        {
            return new CartWidgetModel
                {
                    Id = shoppingCartResponse.ShoppingCartID,
                    Quantity = shoppingCartResponse.TotalItems,
                    Subtotal = shoppingCartResponse.Subtotal,
                    VolumePoints = shoppingCartResponse.VolumePoint,
                    DisplaySubtotal = getAmountString(shoppingCartResponse.Subtotal)
                };
        }


        internal static string getAmountString(decimal amount)
        {
            var symbol = GetSymbol();
            return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbolPosition ==
                   CheckoutConfiguration.CurrencySymbolLayout.Leading
                       ? (HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                              ? symbol + amount.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                              : symbol + string.Format(GetPriceFormat(amount), amount))
                       : string.Format(GetPriceFormat(amount), amount) + symbol;
        }

        internal static string GetSymbol()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration != null)
            {
                return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            }
            return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
        }

        internal static string GetPriceFormat(decimal number)
        {
            var PriceFormatString = string.Empty;
            if (HLConfigManager.Configurations.CheckoutConfiguration.UseCommaWithoutDecimalFormat)
            {
                if (HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal)
                {
                    PriceFormatString = number.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                }
                else
                {
                    if (number == 0)
                    {
                        return "0,000";
                    }

                    PriceFormatString = number.ToString("#,###", CultureInfo.GetCultureInfo("en-US"));
                }
            }
            else
            {
                PriceFormatString = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                                        ? "{0:N2}"
                                        : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
            }

            return PriceFormatString;
        }
    }
}