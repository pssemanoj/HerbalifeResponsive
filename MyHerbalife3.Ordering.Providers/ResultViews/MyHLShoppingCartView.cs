// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyHLShoppingCartView.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to pass a hl shopping cart data from server to client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.Providers.Shipping;
using System.Web.Caching;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    /// Used to pass data from server to client.
    /// </summary>
    public class MyHLShoppingCartView
    {
        private IChinaInterface _chinaOrderProxy;
        private ICatalogProviderLoader _catalogProviderLoader;
        private IOrderChinaProviderLoader _orderChinaProviderLoader;
        public const string YearMonthDayFormat = "yyyy-MM-dd";

        public static string OrderListCaheKey = "OrderListView_";
        #region data properties

        /// <summary>
        /// Cart Id.
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Draft name.
        /// </summary>
        public string DraftName { get; set; }

        /// <summary>
        /// Order Number.
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        /// Last updated date.
        /// </summary>
        public string Date { get; set; }

        public DateTime DateTimeForOrder { get; set; }


        /// <summary>
        /// Recipient.
        /// </summary>
        public DateTime LastUpdatedDate { get; set; }

        /// <summary>
        /// Address.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Address.
        /// </summary>
        public ServiceProvider.ShippingSvc.Address AddressValue { get; set; }

        /// <summary>
        /// Recipient.
        /// </summary>
        public string Recipient { get; set; }

        /// <summary>
        /// Cart items.
        /// </summary>
        public List<MyHLProductItemView> CartItems { get; set; }

        public bool IsPaymentPending { get; set; }

        /// <summary>
        /// TotalAmount.
        /// </summary>
        public string TotalAmount { get; set; }

        /// <summary>
        /// VolumnPoints.
        /// </summary>
        public string VolumePoints { get; set; }

        /// <summary>
        /// TotalAmount.
        /// </summary>
        public decimal TotalAmountValue { get; set; }

        /// <summary>
        /// VolumnPoints.
        /// </summary>
        public decimal VolumePointsValue { get; set; }

        public decimal FreightCharges { get; set; }
        public decimal ProductTaxTotal { get; set; }
        public decimal DonationAmount { get; set; }
        public decimal OnBehalfDonationAmount { get; set; }
        public decimal SelfDonationAmount { get; set; }
        public string OnBehalfDonationName { get; set; }
        public string OnBehalfDonationContact { get; set; }
        public string SelfDonationMemberId { get; set; }
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string OrderMonth { get; set; }

        /// <summary>
        /// ChannelInfo.
        /// </summary>
        public string ChannelInfo { get; set; }

        /// <summary>
        /// StoreInfo.
        /// </summary>
        public string StoreInfo { get; set; }

        /// <summary>
        /// Warehouse Code.
        /// </summary>
        public string WarehouseCode { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string OrderStatus { get; set; }

        /// <summary>
        /// IsCopyEnabled.
        /// </summary>
        public bool IsCopyEnabled { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderAltPhone { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderPhone { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderEmail { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderInstruction { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderRecipient { get; set; }

        /// <summary>
        /// OrderMonth.
        /// </summary>
        public string PendingOrderRGNumber { get; set; }

        public int OrderHeaderId { get; set; }

        public string DistributorID { get; set; }

        /// <summary>
        /// Has Feed Back.
        /// </summary>
        public bool HasFeedBack { get; set; }

        /// <summary>
        /// non-GDO is considered historical data.
        /// </summary>
        public string CreatedBy { get; set; }

        public string Phone { get; set; }

        public string ShippingMethodId { get; set; }

        /// <summary>
        /// Set this to true (manually) when CreatedBY != GDO
        /// </summary>
        public bool IsHistoricalData { get; set; }

        /// <summary>
        /// Contains event type item?
        /// </summary>
        public bool HasEventItem { get; set; }

        #endregion

        /// <summary>
        /// 未知
        /// </summary>
        public const string CN_Status_Unknown = "未知";

        /// <summary>
        /// Initializes a new instance of the MyHLShoppingCartView class.
        /// </summary>
        public MyHLShoppingCartView()
        {
        }

        public MyHLShoppingCartView(IChinaInterface chinaOrderProxy,
                                       ICatalogProviderLoader catalogProviderLoader = null,
                                       IOrderChinaProviderLoader orderChinaProviderLoader = null)
        {
            _chinaOrderProxy = chinaOrderProxy;
            _catalogProviderLoader = catalogProviderLoader;
            _orderChinaProviderLoader = orderChinaProviderLoader;
        }

        private static void changeFormatCalendarToGregorian()
        {
            //if (System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar.Equals(new System.Globalization.ThaiBuddhistCalendar()))
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(CultureInfo.CurrentCulture.Name);
            System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
        }

        /// <summary>
        /// Wrapp the given hl shopping cart to view.
        /// </summary>
        /// <param name="hlCarts">List of shopping cart.</param>
        /// <returns>Shopping cart views.</returns>
        public static List<MyHLShoppingCartView> WrappToShoppingCartViewList(List<MyHLShoppingCart> hlCarts, string Locale, string filterExpressions, string sortExpressions, bool savedCartsMode)
        {
            var config = HLConfigManager.Configurations.DOConfiguration;
            if (config.UseGregorianCalendar)
            {
                changeFormatCalendarToGregorian();
            }
            // Initializing the MyHLShoppingCartView list result.
            var cartViewList = new List<MyHLShoppingCartView>();
            if (hlCarts.Count > 1 && hlCarts[0].ShoppingCartID == hlCarts[1].ShoppingCartID && hlCarts[0].IsSavedCart)
            {
                // Leave the cart with the right date in list
                hlCarts.RemoveAt(0);
            }
            foreach (MyHLShoppingCart cart in hlCarts)
            {
                if (!cartViewList.Select(i => i.ID).Contains(cart.ShoppingCartID.ToString()) && cart.OrderCategory != ServiceProvider.CatalogSvc.OrderCategoryType.APF)
                {
                    var cartView = new MyHLShoppingCartView();

                    // Direct properties.
                    cartView.ID = cart.ShoppingCartID.ToString();
                    cartView.OrderNumber = cart.OrderNumber ?? string.Empty;
                    cartView.DraftName = cart.CartName ?? string.Empty;

                    if (savedCartsMode)
                    {
                        cartView.Date = cart.LastUpdated.ToString("d", CultureInfo.CurrentCulture);
                        cartView.LastUpdatedDate = cart.LastUpdated;
                    }
                    else
                    {
                        cartView.Date = cart.OrderDate.ToString("d", CultureInfo.CurrentCulture);
                        cartView.LastUpdatedDate = cart.OrderDate;
                    }

                    var deliveryOpt = cart.DeliveryOption;

                    // Address formatting
                    if (savedCartsMode)
                    {
                        cart.LoadShippingInfo(cart.DeliveryOptionID, cart.ShippingAddressID, cart.DeliveryOption, true);
                    }
                    else
                    {
                        if (cart.DeliveryInfo == null || cart.DeliveryInfo.Address == null)
                        {
                            cart.GetShippingInfoForCopyOrder(cart.DistributorID, cart.Locale, cart.ShoppingCartID, deliveryOpt, true);
                        }
                    }

                    cartView.Recipient = cart.DeliveryInfo != null && cart.DeliveryInfo.Address != null ?
                        cart.DeliveryInfo.Address.Recipient ?? string.Empty : string.Empty;
                    if (cart.DeliveryInfo != null && cart.DeliveryInfo.Address != null)
                    {
                        cartView.Address =
                            ShippingProvider.GetShippingProvider(Locale.Substring(3, 2)).FormatShippingAddress(
                                new DeliveryOption(cart.DeliveryInfo.Address),
                                cart.DeliveryInfo.Option, cart.DeliveryInfo.Description, false) ?? string.Empty;
                        cartView.AddressValue = cart.DeliveryInfo.Address.Address;
                    }

                    if (!savedCartsMode && deliveryOpt == ServiceProvider.CatalogSvc.DeliveryOptionType.Pickup && cart.DeliveryInfo != null && cart.DeliveryInfo.Address != null)
                    {
                        cartView.Address = string.Concat(cart.DeliveryInfo.Address.Alias, "</br>", cartView.Address);
                        cartView.AddressValue = cart.DeliveryInfo.Address.Address;
                    }

                    cartViewList.Add(cartView);

                    // Getting car items
                    var cartItems = new List<MyHLProductItemView>();

                    if (cart.CartItems != null)
                    {
                        cart.CartItems.ForEach(
                            item =>
                            cartItems.Add(new MyHLProductItemView() { Quantity = item.Quantity, SKU = item.SKU }));

                        // Getting descriptions
                        if (savedCartsMode)
                        {
                            cart.GetShoppingCartForDisplay(false, true);
                        }

                        if (cart.ShoppingCartItems != null && cart.ShoppingCartItems.Count() > 0)
                        {
                            foreach (MyHLProductItemView t in cartItems)
                            {
                                if (t != null)
                                {
                                    var item = cart.ShoppingCartItems.Where(x => x.SKU == t.SKU);
                                    if (item.Count() > 0)
                                    {
                                        t.Description = item.FirstOrDefault().Description;
                                    }
                                }
                            }
                        }
                    }

                    cartView.CartItems = cartItems;
                }
            }

            // Search
            if (!string.IsNullOrEmpty(filterExpressions))
            {
                cartViewList = Search(cartViewList, filterExpressions);
            }
            // Sort
            if (!string.IsNullOrEmpty(sortExpressions))
            {
                cartViewList = SortBy(cartViewList, sortExpressions);
            }

            return cartViewList;
        }

        private static decimal GetFreightCharges(ChargeList chargeList)
        {
            var chargeV01s =
                (from ite in chargeList
                 where ite as Charge_V01 != null && (ite as Charge_V01).ChargeType == ChargeTypes.FREIGHT
                 select ite as Charge_V01);
            if (null != chargeV01s && chargeV01s.Any())
            {
                return chargeV01s.FirstOrDefault().Amount;
            }
            return decimal.Zero;
        }

        /// <summary>
        /// digging OrderItems details
        /// </summary>
        /// <param name="orderView"></param>
        private void LoadPreOrderDetails(MyHLShoppingCartView orderView)
        {
            if (_orderChinaProviderLoader == null)
                _orderChinaProviderLoader = new OrderChinaProviderLoader();

            var orderDetails = _orderChinaProviderLoader.GetPreOrderDetail(orderView.DistributorID, orderView.OrderHeaderId);
            var shippingInfo = (ShippingInfo_V01)orderDetails.Shipment;
            if (shippingInfo != null && shippingInfo.Address != null)
            {
                orderView.Recipient = shippingInfo.Recipient;
                orderView.Address = shippingInfo.Address.Line1 + shippingInfo.Address.City + shippingInfo.Address.CountyDistrict;
                orderView.AddressValue = ObjectMappingHelper.Instance.GetToShipping(shippingInfo.Address);
            }

            var items = orderDetails.OrderItems;
            if (items != null)
            {
                var cartItems = (from OrderItem_V02 item in items
                                 select
                                     new MyHLProductItemView()
                                     {
                                         Quantity = item.Quantity,
                                         SKU = item.SKU,
                                         Description = item.Description,
                                         RetailPrice = item.RetailPrice
                                     }).ToList();

                orderView.CartItems = cartItems;
            }
        }

        #region StatusDictionary
        static Dictionary<string, string> StatusDictionary = new Dictionary<string, string>
        {
            // PaymentGatewayRecordStatusType
            {"Unknown"          , "待付款"},    //OrderStatusFilterType.Payment_Pending
            {"CancelledByUser"  , "取消订单"},  //OrderStatusFilterType.Cancel_Order
            {"OrderSubmitted"   , "支付成功"},  //OrderStatusFilterType.Complete
            {"Approved"         , "支付成功"},  //OrderStatusFilterType.Complete
            {"Declined"         , "支付失败"},  //OrderStatusFilterType.Payment_Failed

            //PaymentPendingOrderPaymentStatusType
            {"CNCancelled"  , "取消订单"}, //OrderStatusFilterType.Cancel_Order
            {"CNPending"    , "待付款"},   //OrderStatusFilterType.Payment_Pending
        };
        #endregion

        /// <summary>
        /// Sort by a expression.
        /// </summary>
        /// <param name="cartViewList">Cart view list.</param>
        /// <param name="sortExpressions">Sort expression.</param>
        /// <returns>Sorted list.</returns>
        public static List<MyHLShoppingCartView> SortBy(List<MyHLShoppingCartView> cartViewList, string sortExpressions)
        {
            if (!string.IsNullOrEmpty(sortExpressions))
            {
                switch (sortExpressions)
                {
                    case "ca":
                        return cartViewList.OrderBy(item => item.DraftName).ToList();
                    case "cz":
                        return cartViewList.OrderByDescending(item => item.DraftName).ToList();
                    case "sa":
                        return cartViewList.OrderBy(item => item.Recipient).ToList();
                    case "sz":
                        return cartViewList.OrderByDescending(item => item.Recipient).ToList();
                    case "da":
                        return cartViewList.OrderByDescending(item => item.LastUpdatedDate).ToList();
                    case "dz":
                        return cartViewList.OrderBy(item => item.LastUpdatedDate).ToList();
                }
            }
            return cartViewList;
        }

        public static List<MyHLShoppingCartView> ChinaDoSortBy(List<MyHLShoppingCartView> cartViewList, string sortExpressions)
        {
            if (!string.IsNullOrEmpty(sortExpressions))
            {
                switch (sortExpressions)
                {
                    case "va":
                        return cartViewList.OrderBy(item => item.VolumePoints).ToList();
                    case "vd":
                        return cartViewList.OrderByDescending(item => item.VolumePoints).ToList();
                    case "oa":
                        return cartViewList.OrderBy(item => item.ChannelInfo).ToList();
                    case "od":
                        return cartViewList.OrderByDescending(item => item.ChannelInfo).ToList();
                    case "pa":
                        return cartViewList.OrderBy(item => item.StoreInfo).ToList();
                    case "pd":
                        return cartViewList.OrderByDescending(item => item.StoreInfo).ToList();
                    case "ma":
                        return cartViewList.OrderBy(item => item.OrderMonth).ToList();
                    case "md":
                        return cartViewList.OrderByDescending(item => item.OrderMonth).ToList();
                    case "osa":
                        return cartViewList.OrderBy(item => item.OrderStatus).ToList();
                    case "osd":
                        return cartViewList.OrderByDescending(item => item.OrderStatus).ToList();
                    case "aa":
                        return cartViewList.OrderBy(item => item.TotalAmount).ToList();
                    case "ad":
                        return cartViewList.OrderByDescending(item => item.TotalAmount).ToList();
                    case "sa":
                        return cartViewList.OrderBy(item => item.Recipient).ToList();
                    case "sz":
                        return cartViewList.OrderByDescending(item => item.Recipient).ToList();
                    case "da":
                        return cartViewList.OrderBy(item => item.Date).ToList();
                    case "dz":
                        return cartViewList.OrderByDescending(item => item.Date).ToList();
                }
            }
            return cartViewList;
        }

        /// <summary>
        /// Search by expression.
        /// </summary>
        /// <param name="cartViewList">Cart view list.</param>
        /// <param name="filterExpressions"></param>
        /// <returns>Result list.</returns>
        public static List<MyHLShoppingCartView> Search(List<MyHLShoppingCartView> cartViewList, string filterExpressions)
        {
            if (string.IsNullOrEmpty(filterExpressions))
            {
                return cartViewList;
            }

            var result = new List<MyHLShoppingCartView>();

            filterExpressions = filterExpressions.ToUpper().Trim();
            foreach (MyHLShoppingCartView t in cartViewList)
            {
                if ((t.DraftName != null && t.DraftName.ToUpper().Contains(filterExpressions)) // Searching by Cart Name
                    || (t.Recipient != null && t.Recipient.ToUpper().Contains(filterExpressions)) // Searching by Recipient
                    || (t.Address != null && t.Address.ToUpper().Contains(filterExpressions)) // Searching by Address
                    || (t.OrderNumber.Contains(filterExpressions)) // Searching by Order Number
                    || (t.Date.Contains(filterExpressions)) // Searching by Date
                    )
                {
                    result.Add(t);
                }
                else
                {
                    if (t.CartItems.Any(cartItem => cartItem.SKU.Contains(filterExpressions))) // Searching by SKU
                    {
                        result.Add(t);
                    }
                    else if (t.CartItems.Any(cartItem => cartItem.Description.ToUpper().Contains(filterExpressions))) // Searching by Description
                    {
                        result.Add(t);
                    }
                }
            }
            return result;
        }

        public List<MyHLShoppingCartView> GetPreOrders(string distributorId, int customerProfileID, string countryCode, DateTime? startOrderDate, DateTime? endOrderDate, MyHerbalife3.Ordering.Providers.China.PreOrderStatusFilterType statusFilter, string filterExpressions, string sortExpressions)
        {
            List<MyHLShoppingCartView> ret = new List<MyHLShoppingCartView>();

            #region date range...
            bool isStartDateDefined = (startOrderDate != null);
            bool isEndDateDefined = (endOrderDate != null);

            var sDate = !isStartDateDefined ? DateTime.Now.AddMonths(-1) : startOrderDate.Value;
            var eDate = !isEndDateDefined ? DateTime.Now.AddDays(2) : endOrderDate.Value;

            // if only one of them is undefined... adjust the other accordingly
            if ((!isStartDateDefined || !isEndDateDefined) && (isStartDateDefined != isEndDateDefined))
            {
                if (!isStartDateDefined) sDate = eDate.AddMonths(-1);
                if (!isEndDateDefined) eDate = sDate.AddMonths(1);
            }
            #endregion

            List<OnlineOrder> orderHeaderList = null;

            #region OrderHeader

            List<PreOrderImportStatusType> ordStsList = new List<PreOrderImportStatusType>();

            #region OrderStatusList
            switch (statusFilter)
            {
                case PreOrderStatusFilterType.Cancel_PreOrder:
                    ordStsList.Add(PreOrderImportStatusType.Cancel);
                    break;

                case PreOrderStatusFilterType.Hold_PreOrder:
                    ordStsList.Add(PreOrderImportStatusType.Hold);
                    break;

                case PreOrderStatusFilterType.ReadyToSubmit_PreOrder:
                    ordStsList.Add(PreOrderImportStatusType.ReadyToSubmit);
                    break;
            }
            #endregion

            if (ordStsList != null) // ordStsList.Count == 0 means all, so need to query
            {
                var req = new GetPreOrdersRequest_V01
                {
                    CustomerProfileID = customerProfileID,
                    OrderFilter = new PreOrdersFilter
                    {
                        DistributorId = distributorId,
                        StartDate = sDate,
                        EndDate = eDate,
                        OrderStatusList = ordStsList
                    },
                };

                if (_chinaOrderProxy == null)
                    _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

                var rsp = _chinaOrderProxy.GetPreOrders(new GetPreOrdersRequest1(req)).GetPreOrdersResult as GetPreOrdersResponse_V01;
                if (Helper.Instance.ValidateResponse(rsp))
                {
                    if (!IsChina)
                        orderHeaderList = rsp.Orders.Where(h => h.OrderCategory == OrderCategoryType.RSO).ToList();
                    else
                        orderHeaderList = rsp.Orders;
                }
                else
                    LoggerHelper.Error(string.Format("MyHLShoppingCartView.GetPreOrders() Error. Unsuccessful result from web service ChinaOrderSVC.GetPreOrders. Data: DistributorId={0}", distributorId));
            }

            #endregion

            #region orderHeaderList
            if (Helper.Instance.HasData(orderHeaderList))
            {
                foreach (var order in orderHeaderList)
                {
                    var priceInfo = order.Pricing as OrderTotals_V01;
                    var priceInfo_V02 = order.Pricing as OrderTotals_V02;
                    if (priceInfo == null) continue;
                    if (priceInfo.ItemsTotal <= 0) continue;

                    var orderView = new MyHLShoppingCartView
                    {
                        ID = order.ShoppingCartID.ToString(),
                        OrderNumber = order.OrderID ?? string.Empty,
                        Date = order.ReceivedDate.AddHours(8).ToString("d", CultureInfo.CurrentCulture),
                        DateTimeForOrder = order.ReceivedDate.AddHours(8),
                        TotalAmount = priceInfo.AmountDue.ToString("##.##"),
                        VolumePoints = priceInfo.VolumePoints.ToString("00.00"),

                        TotalAmountValue = priceInfo.AmountDue,
                        VolumePointsValue = priceInfo.VolumePoints,

                        ProductTaxTotal = priceInfo_V02 != null ? priceInfo_V02.ProductTaxTotal : decimal.Zero,
                        DonationAmount = priceInfo_V02 != null ? priceInfo_V02.Donation : decimal.Zero,
                        DiscountAmount = priceInfo_V02 != null ? priceInfo_V02.DiscountAmount : decimal.Zero,
                        FreightCharges = priceInfo_V02 != null && null != priceInfo_V02.ChargeList && priceInfo_V02.ChargeList.Any() ? GetFreightCharges(priceInfo_V02.ChargeList) : decimal.Zero,

                        OrderStatus = order.Status,
                        StoreInfo = order.StoreInfo,
                        ChannelInfo = order.ChannelInfo,
                        OrderMonth = order.OrderMonth,
                        OrderHeaderId = order.OrderHeaderID,
                        DistributorID = order.DistributorID,
                        HasFeedBack = order.HasFeedBack,
                        CreatedBy = order.CreatedBy,
                    };
                    orderView.TotalAmount = (priceInfo.AmountDue + orderView.FreightCharges).ToString("00");

                    LoadPreOrderDetails(orderView);

                    orderView.Address = ((ShippingInfo_V01)order.Shipment).Address.StateProvinceTerritory + ((ShippingInfo_V01)order.Shipment).Address.CountyDistrict + ((ShippingInfo_V01)order.Shipment).Address.City + ((ShippingInfo_V01)order.Shipment).Address.Line1 + ((ShippingInfo_V01)order.Shipment).Address.PostalCode;
                    orderView.AddressValue = ObjectMappingHelper.Instance.GetToShipping(((ShippingInfo_V01)order.Shipment).Address);
                    orderView.Recipient = ((ShippingInfo_V01)order.Shipment).Recipient;
                    orderView.Phone = ((ShippingInfo_V01)order.Shipment).Phone;
                    orderView.WarehouseCode = ((ShippingInfo_V01)order.Shipment).WarehouseCode;
                    orderView.ShippingMethodId = ((ShippingInfo_V01)order.Shipment).ShippingMethodID;

                    var createdBy = order.CreatedBy;
                    if (!string.IsNullOrWhiteSpace(createdBy))
                        orderView.IsHistoricalData = (createdBy.Trim().ToUpper() != "GDO");

                    ret.Add(orderView);
                }
            }
            #endregion

            // Search
            if (!string.IsNullOrWhiteSpace(filterExpressions))
                ret = Search(ret, filterExpressions);

            // Sort
            if (string.IsNullOrWhiteSpace(sortExpressions))
                ret = ret.OrderByDescending(item => item.OrderNumber).ToList();
            else
                ret = ChinaDoSortBy(ret, sortExpressions);

            return ret;
        }

        static bool IsAllVirtualOrder(MyHLShoppingCartView orderView, List<string> virtualProduct)
        {
            bool isAllVirtual = false;

            if (virtualProduct == null || virtualProduct.Count == 0) return false;

            if (orderView.CartItems != null && orderView.CartItems.Count > 0)
            {
                var virtualItems = (from a in orderView.CartItems
                                    where virtualProduct.Any(vp => vp == a.SKU)
                                    select a).ToList();

                if (virtualItems.Count > 0 && (virtualItems.Count == orderView.CartItems.Count))
                {
                    isAllVirtual = true;
                }
            }

            if (isAllVirtual || (orderView.DonationAmount > 0 && (orderView.CartItems == null || orderView.CartItems.Count == 0)))
                return true;
            else
                return false;
        }

        private List<MyHLShoppingCartView> SearchFromCachedList(string distributorId, MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType statusFilter, string filterExpressions, string sortExpressions, DateTime startOrderDate, DateTime endOrderDate, string orderNumber)
        {
            List<MyHLShoppingCartView> result = null;

            string cacheKey = string.Format(OrderListCaheKey + distributorId + "CN" + statusFilter + startOrderDate.ToString(YearMonthDayFormat) + endOrderDate.ToString(YearMonthDayFormat)); //search from general key first.

            if (Settings.GetRequiredAppSetting("GetOrderinfoCache", "true") == "true")
            {
                result = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCartView>;
                if (result != null)
                {
                    // Search
                    if (!string.IsNullOrWhiteSpace(filterExpressions))
                        result = Search(result, filterExpressions);

                    // Order Number
                    if (!string.IsNullOrWhiteSpace(orderNumber))
                        result = Search(result, orderNumber);

                    // Sort
                    if (string.IsNullOrWhiteSpace(sortExpressions))
                        result = result.OrderByDescending(item => item.DateTimeForOrder).ToList();
                    else
                        result = ChinaDoSortBy(result, sortExpressions);
                }
                else
                {
                    cacheKey = string.Format(OrderListCaheKey + distributorId + "CN" + statusFilter + filterExpressions + sortExpressions + startOrderDate.ToString(YearMonthDayFormat) + endOrderDate.ToString(YearMonthDayFormat) + orderNumber);
                    result = HttpRuntime.Cache[cacheKey] as List<MyHLShoppingCartView>;
                }
            }

            return result;
        }

        public List<MyHLShoppingCartView> GetOrdersWithDetail(string distributorId, int customerProfileID, string countryCode, DateTime startOrderDate, DateTime endOrderDate, MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType statusFilter, string filterExpressions, string sortExpressions, bool isNonGDOOrder = false, bool isPreOrdering = false, string orderNumber = null)
        {
            List<MyHLShoppingCartView> result;
            List<OnlineOrder> orderListing = null;
            List<OrderStatusType> ordStsList = new List<OrderStatusType>();
            List<PaymentGatewayRecordStatusType> pgrStsList = new List<PaymentGatewayRecordStatusType>();
            List<PaymentPendingOrderPaymentStatusType> ppoStsList = new List<PaymentPendingOrderPaymentStatusType>();

            if (Settings.GetRequiredAppSetting("GetOrderinfoCache", "true") == "true")
            {
                result = SearchFromCachedList(distributorId, statusFilter, filterExpressions, sortExpressions, startOrderDate, endOrderDate, orderNumber);

                if (result != null)
                {
                    return result;
                }
            }

            result = new List<MyHLShoppingCartView>();

            switch (statusFilter)
            {
                case OrderStatusFilterType.Cancel_Order:
                    ordStsList.Add(OrderStatusType.Cancel_Order);
                    break;

                case OrderStatusFilterType.Complete:
                    ordStsList.Add(OrderStatusType.Complete);
                    break;

                case OrderStatusFilterType.In_Progress:
                    ordStsList.Add(OrderStatusType.In_Progress);
                    break;

                case OrderStatusFilterType.NTS_Printed:
                    ordStsList.Add(OrderStatusType.NTS_Printed);
                    break;

                case OrderStatusFilterType.To_Be_Assign:
                    ordStsList.Add(OrderStatusType.To_Be_Assign);
                    break;

                case OrderStatusFilterType.Payment_Failed:
                case OrderStatusFilterType.Payment_Pending:
                    ordStsList = null;
                    break;
            }

            switch (statusFilter)
            {
                case OrderStatusFilterType.Payment_Pending:
                    pgrStsList.Add(PaymentGatewayRecordStatusType.Unknown);
                    break;

                case OrderStatusFilterType.Cancel_Order:
                    pgrStsList.Add(PaymentGatewayRecordStatusType.CancelledByUser);
                    break;

                case OrderStatusFilterType.Payment_Failed:
                    pgrStsList.Add(PaymentGatewayRecordStatusType.Declined);
                    break;

                case OrderStatusFilterType.All:
                    pgrStsList.Add(PaymentGatewayRecordStatusType.Unknown);
                    pgrStsList.Add(PaymentGatewayRecordStatusType.CancelledByUser);
                    pgrStsList.Add(PaymentGatewayRecordStatusType.Declined);
                    break;
            }

            switch (statusFilter)
            {
                case OrderStatusFilterType.Cancel_Order:
                    ppoStsList.Add(PaymentPendingOrderPaymentStatusType.CNCancelled);
                    break;

                case OrderStatusFilterType.Payment_Pending:
                    ppoStsList.Add(PaymentPendingOrderPaymentStatusType.CNPending);
                    break;

                case OrderStatusFilterType.All:
                    ppoStsList.Add(PaymentPendingOrderPaymentStatusType.CNCancelled);
                    ppoStsList.Add(PaymentPendingOrderPaymentStatusType.CNPending);
                    break;
            }

            var req = new GetOrdersRequest_V02
            {
                CountryCode = countryCode,
                CustomerProfileID = customerProfileID,
                OrderFilter = new OrdersFilter
                {
                    DistributorId = distributorId,
                    StartDate = startOrderDate,
                    EndDate = endOrderDate,
                    OrderStatusList = ordStsList,
                    IsNonGDOOrders = isNonGDOOrder,
                    OrderNumber = orderNumber,
                },
                OrderingType = isPreOrdering ? OrderingType.PreOrder : OrderingType.RSO,
                PaymentGatewayRecordStatusList = pgrStsList,
                PaymentPendingOrderPaymentStatusList = ppoStsList,
            };

            if (_chinaOrderProxy == null)
                _chinaOrderProxy = ServiceClientProvider.GetChinaOrderServiceProxy();

            var rsp = _chinaOrderProxy.GetOrdersWithDetail(new GetOrdersWithDetailRequest(req)).GetOrdersWithDetailResult as GetOrdersResponse_V01;
            if (Helper.Instance.ValidateResponse(rsp))
            {
                orderListing = rsp.Orders;
            }
            else
                LoggerHelper.Error(string.Format("MyHLShoppingCartView.GetOrdersWithDetail() Error. Unsuccessful result from web service ChinaOrderSVC.GetOrdersWithDetail. Data: DistributorId={0}", distributorId));

            bool copyEnabled = true;
            bool hasUnknownOrder = false;
            bool pendingPayment = false;
            List<MyHLProductItemView> cartItems;

            if (Helper.Instance.HasData(orderListing))
            {
                var unknownOrder = (from a in orderListing where a.Status == "未知" select a).FirstOrDefault();

                //dont display feedback, if any order is pending
                if (unknownOrder != null)
                {
                    hasUnknownOrder = true;
                }

                var virtualProduct = GetVirtualProduct();

                foreach (var order in orderListing)
                {
                    var priceInfo = order.Pricing as OrderTotals_V01;
                    var priceInfo_V02 = order.Pricing as OrderTotals_V02;
                    if (priceInfo == null) continue;

                    switch (order.Status)
                    {
                        case "未知": //unknown order
                            copyEnabled = false;
                            pendingPayment = true;
                            break;
                        default:
                            pendingPayment = false;
                            copyEnabled = !hasUnknownOrder;
                            break;
                    }

                    var orderView = new MyHLShoppingCartView
                    {
                        ID = order.ShoppingCartID.ToString(),
                        OrderNumber = order.OrderID ?? string.Empty,
                        Date = order.ReceivedDate.ToString("d", CultureInfo.CurrentCulture),
                        DateTimeForOrder = order.ReceivedDate,
                        VolumePoints = priceInfo.VolumePoints.ToString("00.00"),

                        TotalAmountValue = priceInfo.AmountDue,
                        VolumePointsValue = priceInfo.VolumePoints,

                        ProductTaxTotal = priceInfo_V02 != null ? priceInfo_V02.ProductTaxTotal : decimal.Zero,
                        DonationAmount = priceInfo_V02 != null ? priceInfo_V02.Donation : decimal.Zero,
                        SelfDonationAmount = priceInfo_V02 != null ? priceInfo_V02.SelfDonationAmount : decimal.Zero,
                        OnBehalfDonationAmount = priceInfo_V02 != null ? priceInfo_V02.OnBehalfDonationAmount : decimal.Zero,
                        OnBehalfDonationContact = priceInfo_V02 != null ? priceInfo_V02.OnBehalfDonationContact : string.Empty,
                        SelfDonationMemberId = priceInfo_V02 != null ? priceInfo_V02.SelfDonationMemberId : string.Empty,
                        OnBehalfDonationName = priceInfo_V02 != null ? priceInfo_V02.OnBehalfDonationName : string.Empty,
                        DiscountAmount = priceInfo_V02 != null ? priceInfo_V02.DiscountAmount : decimal.Zero,
                        FreightCharges = priceInfo_V02 != null && null != priceInfo_V02.ChargeList && priceInfo_V02.ChargeList.Any() ? GetFreightCharges(priceInfo_V02.ChargeList) : decimal.Zero,

                        OrderStatus = order.Status,
                        StoreInfo = order.StoreInfo,
                        WarehouseCode = ((order.Shipment as ShippingInfo_V01) == null) ? "" : (order.Shipment as ShippingInfo_V01).WarehouseCode,
                        ChannelInfo = order.ChannelInfo,
                        OrderMonth = order.OrderMonth,
                        IsCopyEnabled = order.ShoppingCartID > 0 ? copyEnabled : false, //If ShoppingCartID is 0 then CopyEnabled is set to false
                        OrderHeaderId = order.OrderHeaderID,
                        DistributorID = distributorId,
                        HasFeedBack = (order.HasFeedBack && !hasUnknownOrder),
                        CreatedBy = order.CreatedBy,
                        IsPaymentPending = pendingPayment,
                    };

                    var shippingAddress = order.Shipment as ShippingInfo_V01;

                    if (shippingAddress != null)
                    {
                        if (shippingAddress.Address != null)
                            orderView.Address = shippingAddress.Address.StateProvinceTerritory + shippingAddress.Address.CountyDistrict + shippingAddress.Address.City + shippingAddress.Address.Line1 + shippingAddress.Address.PostalCode;
                        orderView.AddressValue = ObjectMappingHelper.Instance.GetToShipping(shippingAddress.Address);
                        orderView.Recipient = shippingAddress.Recipient;
                        orderView.Phone = shippingAddress.Phone;
                        orderView.ShippingMethodId = shippingAddress.ShippingMethodID;
                    }

                    orderView.TotalAmount = (priceInfo.AmountDue + orderView.FreightCharges).ToString("00");

                    var createdBy = order.CreatedBy;
                    if (!string.IsNullOrWhiteSpace(createdBy))
                        orderView.IsHistoricalData = (createdBy.Trim().ToUpper() != "GDO");

                    cartItems = new List<MyHLProductItemView>();

                    if (order.OrderItems != null && order.OrderItems.Count > 0)
                    {
                        foreach (var itm in order.OrderItems)
                        {
                            if (string.IsNullOrEmpty(itm.SKU))
                                continue;

                            if (itm.SKU.EndsWith("||"))
                            {
                                itm.SKU = itm.SKU.Replace("||", ""); //a workaround to indicate this is preordering.
                                orderView.IsCopyEnabled = false;
                            }

                            if (itm is OnlineOrderItem)
                            {
                                var orderItem = itm as OnlineOrderItem;

                                if (orderItem != null)
                                {
                                    cartItems.Add(new MyHLProductItemView()
                                    {
                                        Quantity = orderItem.Quantity,
                                        SKU = string.IsNullOrEmpty(orderItem.SKU) ? "" : orderItem.SKU.Trim(),
                                        Description = orderItem.Description,
                                        RetailPrice = orderItem.RetailPrice
                                    });
                                }
                            }
                            else if (itm is OrderItem_V02)
                            {
                                var orderItem = itm as OrderItem_V02;

                                if (orderItem != null)
                                {
                                    cartItems.Add(new MyHLProductItemView()
                                    {
                                        Quantity = orderItem.Quantity,
                                        SKU = string.IsNullOrEmpty(orderItem.SKU) ? "" : orderItem.SKU.Trim(),
                                        Description = orderItem.Description,
                                        RetailPrice = orderItem.RetailPrice
                                    });
                                }
                            }
                        }
                    }

                    orderView.CartItems = cartItems;

                    if (IsChina)
                    {
                        var isAllVirtualOrder = IsAllVirtualOrder(orderView, virtualProduct);
                        orderView.IsCopyEnabled = isAllVirtualOrder ? false : orderView.IsCopyEnabled;

                        if (isAllVirtualOrder)
                        {
                            orderView.HasFeedBack = true; //Set this true so that the feedback button will not showing.
                        }
                    }

                    result.Add(orderView);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(filterExpressions))
                result = Search(result, filterExpressions);

            // Order Number
            if (!string.IsNullOrWhiteSpace(orderNumber))
                result = Search(result, orderNumber);

            // Sort
            if (string.IsNullOrWhiteSpace(sortExpressions))
                result = result.OrderByDescending(item => item.DateTimeForOrder).ToList();
            else
                result = ChinaDoSortBy(result, sortExpressions);

            if (Settings.GetRequiredAppSetting("GetOrderinfoCache", "true") == "true")
            {
                string cacheKey = string.Format(OrderListCaheKey + distributorId + "CN" + statusFilter + filterExpressions + sortExpressions + startOrderDate.ToString(YearMonthDayFormat) + endOrderDate.ToString(YearMonthDayFormat) + orderNumber);

                HttpRuntime.Cache.Insert(cacheKey,
                                             result,
                                             null,
                                             DateTime.Now.AddMinutes(Convert.ToDouble(Settings.GetRequiredAppSetting("GetOrderinfoCacheTimeout"))),
                                             Cache.NoSlidingExpiration,
                                             CacheItemPriority.Low,
                                             null);
            }

            return result;
        }

        protected bool IsChina
        {
            get
            {
                return HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
        }

        public List<string> GetVirtualProduct()
        {
            List<string> result = new List<string>();

            if (_catalogProviderLoader == null)
                _catalogProviderLoader = new CatalogProviderLoader();

            var allProduct = _catalogProviderLoader.GetCatalog("CN");
            result = (from c in allProduct.Items where c.Value.IsInventory == false select c.Key).ToList();

            return result;
        }
    }
}