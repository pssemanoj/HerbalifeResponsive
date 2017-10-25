#region

using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.ViewModel;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileOrderSummaryProvider : IMobileOrderSummaryProvider
    {
        public OrderSummaryResponseViewModel GetOrderList(OrderSummaryRequestViewModel request)
        {
            #region validation

            if (request == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(request.Locale) || string.IsNullOrWhiteSpace(request.MemberId))
            {
                return null;
            }

            #endregion

            var response = new OrderSummaryResponseViewModel
            {
                Pending = null,
                Completed = null,
                Failed = null
            };

            if (request.Limit == 0)
            {
                //Set default to 5
                request.Limit = 5;
            }
            var result = GetOrderList(request.MemberId, request.Locale, request.From, request.To);
            response = ModelConverter.ConvertMyHLSCListToOrderSummaryResponse(result, request.Limit, request.Locale);
            return response;
        }

        public List<OrderViewModel> GetOrders(OrderSearchParameter request)
        {
            List<OrderViewModel> result = null;

            #region validation

            if (string.IsNullOrWhiteSpace(request.Locale) || string.IsNullOrWhiteSpace(request.MemberId))
            {
                return result;
            }

            #endregion

            var countryCode = request.Locale.Substring(3, 2);
            var localNow = DateUtils.GetCurrentLocalTime(countryCode);

            var defaultStartDate = localNow.AddMonths(-12);
            var defaultEndDate = localNow;

            DateTime startDate = request.From ?? defaultStartDate;
            DateTime endDate = request.To ?? defaultEndDate;

            if (!string.IsNullOrEmpty(request.OrderNumber))
            {
                var orderViewmodelsByOrderNumber = new List<OrderViewModel>();
                var orders = GetOrdersByOrderNumber(request.MemberId, request.Locale, request.OrderNumber, startDate,
                    endDate);
                var addressGuid = Guid.Empty;
                var country = string.Empty;
                if (null != orders && orders.Any())
                {
                    if (orders.Count == 1)
                    {
                        var mobileQuoteHelper = new MobileQuoteHelper();
                        var shoppingCart = mobileQuoteHelper.GetShoppingCart(request.OrderNumber, Guid.Empty);
                        if (null != shoppingCart && shoppingCart.DeliveryOption == DeliveryOptionType.Shipping)
                        {
                            var addressId = shoppingCart.ShippingAddressID;
                            if (addressId > 0)
                            {
                                var shippingProvider =
                                    ShippingProvider.GetShippingProvider(countryCode);
                                if (null != shippingProvider)
                                {
                                    var address = shippingProvider.GetShippingAddressFromAddressGuidOrId(Guid.Empty,
                                        addressId);
                                    addressGuid = null != address ? address.AddressId : Guid.Empty;
                                    country = null != address && null != address.Address
                                        ? address.Address.Country
                                        : string.Empty;
                                }
                            }
                        }
                    }
                    orderViewmodelsByOrderNumber.AddRange(
                        orders.Select(
                            item => ModelConverter.ConvertShoppingCartViewToOrderViewModel(item, request.Locale)));
                    if (addressGuid != Guid.Empty && null != orderViewmodelsByOrderNumber &&
                        orderViewmodelsByOrderNumber.Any() && orderViewmodelsByOrderNumber.Count == 1
                        && null != orderViewmodelsByOrderNumber[0].Shipping &&
                        null != orderViewmodelsByOrderNumber[0].Shipping.Address)
                    {
                        orderViewmodelsByOrderNumber[0].Shipping.Address.CloudId = addressGuid;
                        orderViewmodelsByOrderNumber[0].Shipping.Address.Country = country;
                        if (request.Locale == "zh-CN")
                        {
                            orderViewmodelsByOrderNumber[0].Shipping.FreightVariant =
                                GetFreightVariant(orderViewmodelsByOrderNumber[0].Shipping.DeliveryType);
                        }
                    }
                }
                return orderViewmodelsByOrderNumber;
            }
            List<MyHLShoppingCartView> orderList;
            var shoppingCartView = new MyHLShoppingCartView();
            var customerProfileId =
                DistributorOrderingProfileProvider.GetProfile(request.MemberId, countryCode).CNCustomorProfileID;

            //var proposedStartDate = DateTime.Now.AddMonths(-6);
            //DateTime startDate = new DateTime(proposedStartDate.Year, proposedStartDate.Month, proposedStartDate.Day); //ensure the time is the start of the day, which is 00:00:00
            //DateTime endDate = DateTime.Now;

            orderList = shoppingCartView.GetOrdersWithDetail(request.MemberId, customerProfileId, request.Locale,
                startDate, endDate, OrderStatusFilterType.All, "", "");

            if (orderList != null)
            {
                if (request.PageSize > 0 && orderList.Any())
                {
                    var paged = orderList.Skip((request.PageNumber)*request.PageSize).Take(request.PageSize);
                    if (null != paged && paged.Any())
                    {
                        orderList = paged.ToList();
                    }
                    else
                    {
                        orderList = new List<MyHLShoppingCartView>();
                    }
                }

                result = new List<OrderViewModel>();

                foreach (var itm in orderList)
                {
                    var newItem = ModelConverter.ConvertShoppingCartViewToOrderViewModel(itm, request.Locale);
                    result.Add(newItem);
                }
            }

            return result;
        }

        private string GetFreightVariant(string deliveryType)
        {
            if (string.IsNullOrEmpty(deliveryType))
            {
                return string.Empty;
            }

            switch (deliveryType.ToUpper().Trim())
            {
                case "SHIPPING":
                    return "EXP";
                case "PICKUP":
                    return "SD";
                case "PICKUPFROMCOURIER":
                    return "PUCA";
            }
            return string.Empty;
        }


        private List<OrderViewModel> GetOrderViewModelsFromDictionary(
            Dictionary<int, List<MyHLShoppingCartView>> orderList, string locale)
        {
            var orderViewModels = new List<OrderViewModel>();
            if (null == orderList && !orderList.Any())
            {
                return orderViewModels;
            }

            foreach (var orders in orderList.Values.Where(orders => null != orders && orders.Any()))
            {
                orderViewModels.AddRange(
                    orders.Select(
                        shoppingCartView =>
                            ModelConverter.ConvertShoppingCartViewToOrderViewModel(shoppingCartView, locale)));
            }
            return orderViewModels;
        }

        internal class ModelConverter
        {
            public static List<OrderItemViewModel> ConvertToOrderItemViewModel(List<MyHLProductItemView> request)
            {
                List<OrderItemViewModel> response = null;
                if (request != null)
                {
                    response = request.Select(item => new OrderItemViewModel
                    {
                        Quantity = item.Quantity,
                        Sku = item.SKU
                    }).ToList();
                }
                return response;
            }

            public static OrderViewModel ConvertShoppingCartViewToOrderViewModel(MyHLShoppingCartView request,
                string locale)
            {
                DateTime date;
                DateTime.TryParse(request.Date, out date);
                var countryCode = locale.Substring(3);

                var item = new OrderViewModel
                {
                    OrderItems = ConvertToOrderItemViewModel(request.CartItems),
                    OrderMonth = request.OrderMonth,
                    CategoryType = string.Empty,
                    Shipping = PopulateShipping(request, locale),
                    Donations = PopulateDonation(request),
                    Quote = PopulateQuote(request),
                    CreatedDate = DateUtils.GetGMTDateTime(countryCode, request.DateTimeForOrder),
                    LastUpdatedDate = DateUtils.GetGMTDateTime(countryCode, request.DateTimeForOrder),
                    MemberId = request.DistributorID,
                    CustomerId = request.DistributorID,
                    Id = Guid.Empty,
                    Locale = string.Empty,
                    Status = TranslateStatus(request.OrderStatus),
                    StatusForDisplay = request.OrderStatus,
                    OrderNumber = request.OrderNumber,
                    CountryOfProcessing = countryCode,
                    CopyEnabled = request.IsCopyEnabled
                };

                return item;
            }

            private static List<DonationViewModel> PopulateDonation(MyHLShoppingCartView request)
            {
                List<DonationViewModel> result;
                if (request.DonationAmount > 0)
                {
                    result = new List<DonationViewModel>();
                    DonationViewModel donation;

                    if(request.OnBehalfDonationAmount > 0)
                    {
                        donation = new DonationViewModel();
                        donation.Amount = request.OnBehalfDonationAmount;
                        donation.Name = request.OnBehalfDonationName;
                        donation.PhoneNumber = request.OnBehalfDonationContact;
                        donation.Type = "OnBehalf";
                        result.Add(donation);
                    }

                    if(request.SelfDonationAmount > 0)
                    {
                        donation = new DonationViewModel();
                        donation.Amount = request.SelfDonationAmount;
                        donation.Name = request.SelfDonationMemberId;
                        donation.Type = "Self";
                        result.Add(donation);
                    }

                    return result;
                }
                return null;
            }

            private static OrderTotalsViewModel PopulateQuote(MyHLShoppingCartView request)
            {
                var quote = new OrderTotalsViewModel
                {
                    AmountDue = request.TotalAmountValue + request.FreightCharges,
                    VolumePoints = request.VolumePointsValue,
                    ProductTaxTotal = request.ProductTaxTotal,
                    ChargeFreightAmount = request.FreightCharges,
                    DiscountAmount = request.DiscountAmount,
                    LineItems = PopulateLineItems(request.CartItems)
                };
                return quote;
            }

            private static decimal GetRetailPrice(string sku, int quantity, decimal unitPrice)
            {
                var SkuPrice = CatalogProvider.GetCatalogItem(sku, "CN");
                if (SkuPrice != null)
                {
                    return SkuPrice.ListPrice*quantity;
                }

                return unitPrice;
            }

            private static List<ItemTotalsListViewModel> PopulateLineItems(IEnumerable<MyHLProductItemView> cartItems)
            {
                return cartItems.Select(item => new ItemTotalsListViewModel
                {
                    RetailPrice = GetRetailPrice(item.SKU, item.Quantity, item.RetailPrice),
                    Sku = item.SKU,
                    Quantity = item.Quantity
                }).ToList();
            }

            private static string TranslateStatus(string status)
            {
                var translatedStatus = string.Empty;
                switch (status)
                {
                    case "待付款":
                    case "僵持":
                    case "未付":
                        translatedStatus = "Pending";
                        break;
                    case "取消订单":
                    case "支付失败":
                        translatedStatus = "Failed";
                        break;
                    case "处理中":
                    case "待配送":
                    case "NTS打印":
                    case "完成":
                    case "后备订单":
                    case "批准后备订单":
                    case "箱数确认":
                    case "整单欠货":
                    case "待发运":
                    case "已发货":
                    case "自提到货确认":
                    case "订单发运中":
                    case "3PL入库":
                    case "3PL自提到货":
                    case "3PL逾期返回总仓":
                    case "3PL拒收":
                    case "3PL逾期进入总仓":
                    case "3PL部分到货":
                    case "3PL配货中":
                    case "3PL面单打印":
                    case "3PL部分提货":
                        translatedStatus = "Completed";
                        break;
                }
                return translatedStatus;
            }

            private static ShippingViewModel PopulateShipping(MyHLShoppingCartView request, string locale)
            {
                var shipping = new ShippingViewModel
                {
                    Address = new AddressViewModel
                    {
                        City = request.AddressValue.City,
                        StateProvinceTerritory = request.AddressValue.StateProvinceTerritory,
                        CountyDistrict = request.AddressValue.CountyDistrict,
                        Line1 = request.AddressValue.Line1,
                        Line2 = request.AddressValue.Line2,
                        Line3 = request.AddressValue.Line3,
                        Line4 = request.AddressValue.Line4,
                        Country = request.AddressValue.Country,
                        PostalCode = request.AddressValue.PostalCode
                    },
                    Recipient = request.Recipient,
                    StoreName = request.StoreInfo,
                    Phone = request.Phone,
                    WarehouseCode = request.WarehouseCode,
                    ShippingMethodId = request.ShippingMethodId
                };


                //Code to add Delivery Option

                var cartfromsql =
                    ShoppingCartProvider.GetShoppingCartFromV02(request.DistributorID, locale, int.Parse(request.ID)) as
                        ShoppingCart_V02;
                if (cartfromsql != null)
                {
                    shipping.DeliveryType = cartfromsql.DeliveryOption.ToString();
                    shipping.DeliveryOptionId = cartfromsql.DeliveryOptionID;
                }

                return shipping;
            }


            public static OrderSummaryResponseViewModel ConvertMyHLSCListToOrderSummaryResponse(
                List<MyHLShoppingCartView> request, int limit, string locale)
            {
                if (request != null)
                {
                    var response = new OrderSummaryResponseViewModel();
                    var Pending = new List<OrderViewModel>();
                    var Failed = new List<OrderViewModel>();
                    var Completed = new List<OrderViewModel>();

                    foreach (var item in request)
                    {
                        switch (item.OrderStatus)
                        {
                            case "待付款":
                                Pending.Add(ConvertShoppingCartViewToOrderViewModel(item, locale));
                                break;

                            case "取消订单":
                            case "支付失败":
                                Failed.Add(ConvertShoppingCartViewToOrderViewModel(item, locale));
                                break;


                            case "完成":
                            case "支付成功":
                            case "待配送":
                            case "配货中":
                            case "已出库":
                                Completed.Add(ConvertShoppingCartViewToOrderViewModel(item, locale));
                                break;
                        }
                    }
                    response.Pending = null != Pending && Pending.Any() ? Pending.Take(limit).ToList() : Pending;
                    response.Failed = null != Failed && Failed.Any() ? Failed.Take(limit).ToList() : Failed;
                    response.Completed = null != Completed && Completed.Any()
                        ? Completed.Take(limit).ToList()
                        : Completed;
                    return response;
                }

                return null;
            }
        }

        #region privateMethods

        private List<MyHLShoppingCartView> GetOrdersByOrderNumber(string memberId, string locale, string orderNumber,
            DateTime startDate, DateTime endDate)
        {
            var shoppingCartViews = new List<MyHLShoppingCartView>();

            if (!string.IsNullOrEmpty(orderNumber))
            {
                var customerProfileId =
                    DistributorOrderingProfileProvider.GetProfile(memberId, locale.Substring(3, 2)).CNCustomorProfileID;
                var shoppingCartView = new MyHLShoppingCartView();

                var proposedStartDate = DateTime.Now.AddMonths(-12);
                //DateTime startDate = new DateTime(proposedStartDate.Year, proposedStartDate.Month, proposedStartDate.Day); //ensure the time is the start of the day, which is 00:00:00
                //DateTime endDate = DateTime.Now;

                shoppingCartViews = shoppingCartView.GetOrdersWithDetail(memberId, customerProfileId, locale, startDate,
                    endDate, OrderStatusFilterType.All, "", "", false, false, orderNumber);

                if (shoppingCartViews.Count > 0)
                {
                    return shoppingCartViews.FindAll(oi => oi.OrderNumber == orderNumber);
                }
            }

            return shoppingCartViews;
        }

        private List<MyHLShoppingCartView> GetOrderList(string memberId, string locale, DateTime? from, DateTime? to,
            bool check3Months = false)
        {
            var shoppingCartView = new MyHLShoppingCartView();
            var customerProfileId =
                DistributorOrderingProfileProvider.GetProfile(memberId, locale.Substring(3, 2)).CNCustomorProfileID;
            var filterExpressions = string.Empty;
            var sortExpressions = string.Empty;
            var countryCode = locale.Substring(3, 2);
            var localNow = DateUtils.GetCurrentLocalTime(countryCode);

            if (check3Months)
            {
                from = localNow.AddMonths(-3);
                to = localNow;
            }
            else
            {
                if (!from.HasValue)
                {
                    if (to.HasValue)
                    {
                        from = to.Value.AddMonths(-3);
                    }
                    else
                    {
                        from = localNow.AddMonths(-3);
                    }
                }

                if (!to.HasValue)
                {
                    if (from.HasValue)
                    {
                        to = from.Value.AddMonths(3);
                    }
                    else
                    {
                        to = localNow;
                    }
                }
            }

            var result = shoppingCartView.GetOrdersWithDetail(memberId, customerProfileId, locale, from.Value, to.Value,
                OrderStatusFilterType.All, "", "");
            return result;
        }

        #endregion
    }
}