#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
using System.Xml;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Utilities;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ViewModel;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using Order = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Order;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using ShippingAddress_V02 = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V02;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderCategoryType;
using System.Xml.Serialization;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileOrderProvider : IMobileOrderProvider
    {
        private readonly ISimpleCache _cache = CacheFactory.Create();
        private readonly MobileQuoteHelper _mobileQuoteHelper;
        private readonly MobileWechatHelper _mobileWechatHelper;

        public MobileOrderProvider(MobileQuoteHelper mobileQuoteHelper, MobileWechatHelper mobileWechatHelper)
        {
            _mobileQuoteHelper = mobileQuoteHelper;
            _mobileWechatHelper = mobileWechatHelper;
        }

        public List<OrderViewModel> GetOrders(OrderSearchParameter searchParameter)
        {
            if (null == searchParameter)
            {
                return null;
            }

            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                if (searchParameter.PageSize == 0 && searchParameter.PageNumber == 0)
                {
                    searchParameter.PageNumber = 1;
                    searchParameter.PageSize = 15;
                }
                var response =
                    proxy.GetMobileOrders(new GetMobileOrdersRequest1(new GetMobileOrdersRequest_V01
                    {
                        Filter = ModelConverter.PopulateMobileOrderSearchParamter(searchParameter)
                    })).GetMobileOrdersResult;
                if (null == response)
                {
                    return null;
                }
                var orderViewModels = new List<OrderViewModel>();
                var responseV01 = response as GetMobileOrdersResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success &&
                    null != responseV01.ShoppingCarts && responseV01.ShoppingCarts.Any())
                {
                    orderViewModels.AddRange(
                        responseV01.ShoppingCarts.Select(
                            ModelConverter.ConvertShoppingCart_V05ToOrderViewModel));
                }
                if (null != orderViewModels && orderViewModels.Any())
                {
                    orderViewModels = orderViewModels.OrderByDescending(o => o.LastUpdatedDate).ToList();
                }
                return orderViewModels;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider GetShoppingCart: {0}\n",
                    ex.Message));
                return null;
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
        }

        public OrderViewModel GetOrder(Guid orderId, string memberId, string locale, bool suppressRules = false)
        {
            var shoppingCart = ShoppingCartProvider.GetShoppingCart(memberId, locale, suppressRules, true);
            if (null == shoppingCart)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider GetOrder: {0}\n", orderId));
                return null;
            }
            var orderViewModel = ModelConverter.ConvertMyHLShoppingCartToOrderViewModel(shoppingCart, orderId);
            orderViewModel.Status = "Open";
            var orderNumber = string.Empty;
            var isOrderSubmitted = false;
            _mobileQuoteHelper.CheckIfAnyOrderInProcessing(memberId, locale, ref orderNumber, ref isOrderSubmitted, orderViewModel.OrderNumber);
            if (!string.IsNullOrEmpty(orderNumber))
            {
                orderViewModel.OrderNumber = orderNumber;
                orderViewModel.Status = "Unknown";
            }

            return orderViewModel;
        }


        public OrderViewModel Save(OrderViewModel orderViewModel, ref List<ValidationErrorViewModel> errors)
        {
            if (null == orderViewModel)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider Save: {0}\n", "input orderViewModel is null"));
                return null;
            }
            errors = new List<ValidationErrorViewModel>();
            if (_mobileQuoteHelper.CheckIfOrderCompleted(orderViewModel.Id))
            {
                errors.Add(
                    new ValidationErrorViewModel
                    {
                        Code = 109999,
                        Reason = string.Format("order for guid {0} already completed", orderViewModel.Id)
                    });
                return orderViewModel;
            }

            // var currentOrder = GetOrder(orderViewModel.Id, orderViewModel.OrderMemberId,
            //   orderViewModel.Locale, true);
            //if (null != currentOrder.LastUpdatedDate && null != orderViewModel.LastUpdatedDate &&
            //    currentOrder.LastUpdatedDate > orderViewModel.LastUpdatedDate)
            //{
            //    return currentOrder;
            //}

            var currentCart = _mobileQuoteHelper.PriceCart(ref orderViewModel, ref errors, true);
            try
            {
                if (null == currentCart)
                {
                    return null;
                }

                var updatedDate = SaveMobileOrderResponse(orderViewModel, currentCart.ShoppingCartID, "Open", false);
                if (null != updatedDate)
                {
                    orderViewModel.LastUpdatedDate = updatedDate;
                }
                else
                {
                    var savedOrder = GetOrder(orderViewModel.Id, orderViewModel.OrderMemberId,
                        orderViewModel.Locale, true);
                    orderViewModel.LastUpdatedDate = savedOrder != null && savedOrder.LastUpdatedDate != null
                        ? savedOrder.LastUpdatedDate
                        : DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider SaveMobileOrder: {0}\n",
                    ex.Message));
                return null;
            }
            return orderViewModel;
        }

        public OrderViewModel Submit(OrderViewModel orderViewModel, ref List<ValidationErrorViewModel> errors,
            Guid authToken, string loginDistributorId = null)
        {
            try
            {
                if (null == errors)
                {
                    errors = new List<ValidationErrorViewModel>();
                }

                //Fix for Order Month null check.
                if (string.IsNullOrEmpty(orderViewModel.OrderMonth))
                {
                    DateTime createDateTime = DateUtils.GetCurrentLocalTime(orderViewModel.Locale.Substring(3, 2));
                    orderViewModel.OrderMonth = createDateTime.ToString("yyMM", CultureInfo.InvariantCulture);
                }

                var shoppingCart = _mobileQuoteHelper.GetShoppingCart(orderViewModel.Id);
                if (null != shoppingCart)
                {
                    var completedOrderNumber = string.Empty;
                    OrderViewModel completedOrder = null;
                    if (CheckOrderStatus(shoppingCart, ref completedOrderNumber, ref completedOrder))
                    {
                        LoggerHelper.Error(string.Format("MobileOrderProvider Submit Order {0}\n", "Duplicate Order"));
                        if (null != completedOrder)
                        {
                            if (string.IsNullOrEmpty(completedOrder.OrderMonth))
                            {
                                completedOrder.OrderMonth = orderViewModel.OrderMonth;
                            }
                            completedOrder.LastUpdatedDate = completedOrder.LastUpdatedDate ?? DateTime.UtcNow;
                            completedOrder.Status = "Completed";
                            completedOrder.OrderNumber = completedOrderNumber;
                        }
                        return completedOrder;
                    }
                }
                var myHlShoppingCart = _mobileQuoteHelper.PriceCart(ref orderViewModel, ref errors);
                myHlShoppingCart.GreetingMsg = orderViewModel.Shipping.Greeting;
                if (null != errors && errors.Any())
                {
                    var errormsg = errors.FirstOrDefault(x => x.Code == 101301); //  out of stock promotional sku
                    if (errormsg != null)
                    {
                        errors.Remove(errormsg);
                    }
                    else
                    {
                        return orderViewModel;
                    }
                }
                var orderTotalsV01 = myHlShoppingCart.Totals as OrderTotals_V01;
                var orderTotalsV02 = myHlShoppingCart.Totals as OrderTotals_V02;

                decimal amount;
                var order = ModelConverter.ConvertOrderViewModelToOrderV01(orderViewModel, ref errors, orderTotalsV02,
                    orderTotalsV01,
                    out amount);
                var generatedOrderNumber = orderViewModel.Payments[0] is WechatPaymentViewModel || orderViewModel.Payments[0] is QuickPayPaymentViewModel
                    ? orderViewModel.OrderNumber
                    : GetOrderNumber(amount, myHlShoppingCart);
                var isPayd = false;
                var isUnknownPaymentStatus = false;
                var profileLoader = new DistributorLoader();
                var profile = profileLoader.Load(orderViewModel.MemberId, orderViewModel.CountryOfProcessing);
                var payments = order.Payments;

                if (orderViewModel.Payments.OfType<WechatPaymentViewModel>().Any())
                {
                    var status = _mobileWechatHelper.QueryWechatOrder(generatedOrderNumber);
                    if (!status)
                    {
                        errors.Add(new ValidationErrorViewModel
                        {
                            Code = 120303,
                            Reason = "Wechat payment failed"
                        });
                        return orderViewModel;
                    }
                }
                UpdateDonationforCNSMS(orderViewModel, myHlShoppingCart, orderTotalsV02);

                var error = string.Empty;
                var _failedCards = new List<FailedCardInfo>();
                string orderNumber = null;
                string paymentStatus = string.Empty;
           
                var countryCode = orderViewModel.Locale.Substring(3, 2);

                var btOrder = OrderProvider.CreateOrder(order, myHlShoppingCart, countryCode, null, "Mobile");
                if (null != profile)
                {
                    var name = null != profile.Localname
                        ? string.Format("{0} {1}", profile.Localname.First, profile.Localname.Last)
                        : string.Empty;
                    name = !string.IsNullOrEmpty(name) ? name.Trim() : name;
                    var phone = DistributorOrderingProfileProvider.GetPhoneNumberForCN(orderViewModel.MemberId);
                    phone = !string.IsNullOrEmpty(phone) ? phone.Trim() : phone;

                    try
                    {
                        if (orderViewModel.Payments.OfType<QuickPayPaymentViewModel>().Any())
                        {
                            var quickPayPaymentViewModel = orderViewModel.Payments[0] as QuickPayPaymentViewModel;
                            var invalidMobilePin = string.IsNullOrEmpty(quickPayPaymentViewModel.MobilePin) || (quickPayPaymentViewModel.MobilePin.Length != 6);
                            if (invalidMobilePin)
                            {
                                errors.Add(new ValidationErrorViewModel
                                {
                                    Code = 120304,
                                    Reason = "Quick Pay - Invalid mobile pin"
                                });
                                return orderViewModel;
                            }

                            var quickPayPaymentTypes = payments[0] as CreditPayment_V01;
                            var cn99BillQuickPayProvider = new CN_99BillQuickPayProvider();
                            var quickPayResponse = cn99BillQuickPayProvider.Submit(orderViewModel.OrderNumber, amount, loginDistributorId ?? orderViewModel.MemberId, quickPayPaymentTypes);

                            isUnknownPaymentStatus = cn99BillQuickPayProvider.IsUnknownPaymentStatus;

                            if (!string.IsNullOrEmpty(cn99BillQuickPayProvider.LastErrorMessage))
                            {
                                errors.Add(new ValidationErrorViewModel
                                {
                                    Code = 126000,
                                    Reason = cn99BillQuickPayProvider.LastErrorMessage
                                });

                                return orderViewModel;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(quickPayResponse))
                                {
                                    errors.Add(new ValidationErrorViewModel
                                    {
                                        Code = 120305,
                                        Reason = "Quick Pay - Submission fails"
                                    });
                                    return orderViewModel;
                                }
                                else
                                {
                                    var shoppingcart = ShoppingCartProvider.GetShoppingCart(orderViewModel.MemberId, orderViewModel.Locale, true, false);
                                    if (shoppingcart.HastakenSrPromotion)
                                         ChinaPromotionProvider.LockSRPromotion(shoppingcart, orderViewModel.OrderNumber);
                                    if (shoppingcart.HastakenSrPromotionGrowing)
                                         ChinaPromotionProvider.LockSRQGrowingPromotion(shoppingcart, orderViewModel.OrderNumber);
                                    if (shoppingcart.HastakenSrPromotionExcelnt)
                                        ChinaPromotionProvider.LockSRQExcellentPromotion(shoppingcart, orderViewModel.OrderNumber);
                                    if (shoppingcart.HastakenBadgePromotion)
                                        ChinaPromotionProvider.LockBadgePromotion(shoppingcart, orderViewModel.OrderNumber);
                                    if (shoppingcart.HastakenNewSrpromotion)
                                        ChinaPromotionProvider.LockNewSRPromotion(shoppingcart, orderViewModel.OrderNumber);
                                    if (shoppingcart.HasBrochurePromotion)
                                        ChinaPromotionProvider.LockBrochurePromotion(shoppingcart, orderViewModel.OrderNumber);
                                    isPayd = true;

                                    string[] parts = quickPayResponse.Split(',');
                                    string specialResponse = "";
                                    if (parts.Length == 6)
                                    {
                                        specialResponse = string.Format("{0},{1}", parts[3], parts[4]);
                                        isPayd = parts[1] == "1";
                                    }

                                    if (order.Payments != null && (order.Payments).Count > 0)
                                    {
                                        var orderPayment = order.Payments[0] as CreditPayment_V01;
                                        if (orderPayment != null)
                                        {
                                            var btPayment = ((Order)btOrder).Payments[0];
                                            btPayment.PaymentCode = orderPayment.TransactionType = "CC"; //Use back 'CC' as Payment Code for BT handling.
                                            string[] bankInfo = specialResponse.Split(',');
                                            if (bankInfo.Length == 2)
                                            {
                                                orderPayment.Card.IssuingBankID = bankInfo[0]; // this is PayChannel
                                                orderPayment.AuthorizationMerchantAccount = bankInfo[1]; // this is PaymentID
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (payments.OfType<CreditPayment_V01>().Any())
                        {
                            var creditCardNumber = ((Order)btOrder).Payments[0].AccountNumber;
                            var cvv = ((Order)btOrder).Payments[0].CVV;

                            ((Order)btOrder).Payments[0].AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                            ((Order)btOrder).Payments[0].CVV = "123";

                            ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                            ((order.Payments[0]) as CreditPayment_V01).Card.CVV = "123";

                            var holder = OrderProvider.GetSerializedOrderHolder(btOrder, order, myHlShoppingCart,
                                authToken == Guid.Empty ? Guid.NewGuid() : authToken);
                           
                            var orderData = OrderSerializer.SerializeOrder(holder);

                            var paymentId = OrderProvider.InsertPaymentGatewayRecord(generatedOrderNumber,
                                orderViewModel.MemberId, "CN_99BillPaymentGateway",
                                orderData, orderViewModel.Locale);

                            ((Order)btOrder).Payments[0].AccountNumber = creditCardNumber;
                            ((Order)btOrder).Payments[0].CVV = cvv;

                            ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = creditCardNumber;
                            ((order.Payments[0]) as CreditPayment_V01).Card.CVV = cvv;

                            foreach (CreditPayment_V01 payment in payments)
                            {
                                isPayd = CN_99BillPaymentGatewayInvoker.PostCNPForMobile(myHlShoppingCart, payment,
                                    orderViewModel.MemberId, name, amount, generatedOrderNumber,
                                    orderViewModel.MemberId, phone);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new ValidationErrorViewModel
                        {
                            Code = 120303,
                            Reason = "99BillCNP card Authorization failed"
                        });
                        LoggerHelper.Exception("General", ex, "99BillCNP card Authorization failed");
                        isPayd = false;
                    }
                }

                if (isPayd || (null != payments && payments.Any() && payments.OfType<WirePayment_V01>().Any()))
                {
                    if (!payments.OfType<WirePayment_V01>().Any())
                    {
                        ((Order)btOrder).OrderID = generatedOrderNumber;
                    }
                    else
                    {
                        if (orderViewModel.Payments.OfType<WechatPaymentViewModel>().Any())
                        {
                            ((Order)btOrder).OrderID = generatedOrderNumber;
                            ((Order)btOrder).Payments[0].NumberOfInstallments = 1;
                            ((Order)btOrder).Payments[0].NameOnAccount = "China User";
                            ((Order)btOrder).Payments[0].Currency = "RMB";
                        }
                        else
                        {
                            ((Order)btOrder).OrderID = string.Empty;
                        }
                    }
               
                 orderNumber = OrderProvider.ImportOrder(btOrder, out error, out _failedCards,
                        myHlShoppingCart.ShoppingCartID);

                    ShoppingCartProvider.UpdateShoppingCart(myHlShoppingCart, null, orderNumber,
                        ((Order)btOrder).ReceivedDate);

                    //updating the payment gateway record
                    OrderProvider.UpdatePaymentGatewayRecord(orderNumber, "", PaymentGatewayLogEntryType.OrderSubmitted,
                        PaymentGatewayRecordStatusType.OrderSubmitted);

                    UpdateApfDateIfRequired(myHlShoppingCart, orderViewModel.OrderMemberId,
                        orderViewModel.CountryOfProcessing);

                    if (countryCode == "CN")
                    {
                        paymentStatus = ((Order)btOrder).PaymentStatus;
                    }
                }
                else
                {
                    if(isUnknownPaymentStatus)
                    {
                        errors.Add(new ValidationErrorViewModel
                        {
                            Code = 120306,
                            Reason = "Payment under processing"
                        });
                    }
                    else
                    {
                        errors.Add(new ValidationErrorViewModel
                        {
                            Code = 120303,
                            Reason = "99BillCNP card Authorization failed"
                        });
                    }
                }

                //check failed cards and import error
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(new ValidationErrorViewModel
                    {
                        Code = 100412,
                        Reason = string.Format("Submit order Error: {0}", error)
                    });
                }
                if (_failedCards != null && _failedCards.Count > 0)
                {
                    errors.AddRange(_failedCards.Select(card => new ValidationErrorViewModel
                    {
                        Code = 120422,
                        Reason =
                            string.Format("Failed card info: {0}, {1}, {2}", card.CardNumber, card.CardType, card.Amount)
                    }));
                }

                if(isUnknownPaymentStatus)
                {
                    orderViewModel.Status = "Unknown";
                }
                else
                {
                    orderViewModel.Status = !string.IsNullOrEmpty(orderNumber) ? "Completed" : "Failed";
                }
                orderViewModel.OrderNumber = orderNumber;

                if (orderViewModel.Status == "Completed" && orderViewModel.OrderItems.Count == 0 &&
                    null != orderViewModel.Quote && orderViewModel.CountryOfProcessing == "CN")
                {
                    orderViewModel.Quote.AmountDue = amount;
                }

                if (orderViewModel.Status == "Failed" && !string.IsNullOrEmpty(error))
                {
                    LoggerHelper.Error(string.Format("MobileOrderProvider Submit: {0}-{1} failed", orderViewModel.Id,
                        error));
                }
                var response = SaveMobileOrderResponse(orderViewModel, myHlShoppingCart.ShoppingCartID,
                    orderViewModel.Status, true);
                if (null != response && response.HasValue)
                {
                    orderViewModel.LastUpdatedDate = response;
                }
                else
                {
                    orderViewModel.LastUpdatedDate = orderViewModel.LastUpdatedDate ?? DateTime.UtcNow;
                }

                if (orderViewModel.CountryOfProcessing == "CN")
                {
                    if (!string.IsNullOrEmpty(paymentStatus) && paymentStatus.Contains("Pending"))
                        orderViewModel.Status = "Pending";
                    switch (orderViewModel.Status)
                    {
                        case "Completed":
                            orderViewModel.StatusForDisplay = "订单完成";
                            break;
                        case "Pending":
                            orderViewModel.StatusForDisplay = "待配送";
                            break;
                        case "Unknown":
                            orderViewModel.StatusForDisplay = "未知";
                            break;
                        default:
                            orderViewModel.StatusForDisplay = "失败";
                            break;
                    }
                }

                return orderViewModel;
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationErrorViewModel
                {
                    Code = 100416,
                    Reason = string.Format("MobileOrderProvider Submit failed: {0}", ex.Message)
                });
                LoggerHelper.Exception("General", ex, "MobileOrderProvider Submit failed");
                return orderViewModel;
            }
        }


        public bool DeleteOrder(Guid orderId)
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var response =
                    proxy.DeleteMobileOrderDetail(new DeleteMobileOrderDetailRequest(new DeleteMobileOrderRequest_V01 { OrderId = orderId })).DeleteMobileOrderDetailResult;
                if (null == response)
                {
                    return false;
                }
                var responseV01 = response as DeleteMobileOrderResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success &&
                    responseV01.Success)
                {
                    return responseV01.Success;
                }
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider DeleteMobileOrder: {0}\n",
                    ex.Message));
                return false;
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
        }

        public OrderViewModel CancelOrder(string memberId, string orderNumber, string locale, string paymentMethodName,
            ref bool isApproved, Guid id)
        {
            var cartId = 0;
            MyHLShoppingCart cart = null;
            var orderViewModel = GetOrderDetails(
                orderNumber, memberId, locale, ref cartId, ref cart, id);
            if (null != orderViewModel && !string.IsNullOrEmpty(memberId) && cartId > 0)
            {
                var paymentResponse = PaymentGatewayInvoker.CheckOrderStatus(paymentMethodName, orderNumber);
                if (paymentResponse != null && paymentResponse.IsApproved && null != cart)
                {
                    DoSubmitOrder(locale, paymentResponse, orderViewModel, cart);
                    isApproved = true;
                    return null;
                }

                if (paymentResponse != null && paymentResponse.Status == PaymentGatewayRecordStatusType.Unknown)
                {
                    if (paymentMethodName == "WechatPayment")
                    {
                        var paymentStatus = _mobileWechatHelper.QueryWechatOrder(orderNumber);
                        if (paymentStatus)
                        {
                            DoSubmitOrder(locale, paymentResponse, orderViewModel, cart);
                            OrderProvider.UpdatePaymentGatewayRecord(orderNumber, "",
                            PaymentGatewayLogEntryType.OrderSubmitted,
                            PaymentGatewayRecordStatusType
                                .OrderSubmitted);
                            isApproved = true;
                            return null;
                        }
                    }
                    if (!string.IsNullOrEmpty(orderNumber))
                    {
                        OrderProvider.UpdatePaymentGatewayRecord(orderNumber, "Pending Order Declined by DS",
                            PaymentGatewayLogEntryType.Request, PaymentGatewayRecordStatusType.Declined);
                    }
                }
                using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
                {
                    try
                    {
                        var response =
                            proxy.DeleteShoppingCart(new DeleteShoppingCartRequest1(new DeleteShoppingCartRequest_V01
                            {
                                ShoppingCartID = cartId,
                                Distributor = memberId,
                                SKUList = null
                            })).DeleteShoppingCartResult
                                as DeleteShoppingCartResponse_V01;

                        // Check response status.
                        if (response == null || response.Status != ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success)
                        {
                            LoggerHelper.Error(
                                    "DeleteShoppingCartResponse indicates an error. Status: " +
                                    (response == null ? "null" : response.Status.ToString()));
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                        string.Format("DeclineShoppingCart error: DS {0}, shoppingCartID{1}, error: {2}",
                                memberId, cartId,
                                ex));
                    }
                }


            }
            return orderViewModel;
        }

        private static void DoSubmitOrder(string locale, PaymentGatewayResponse paymentResponse, OrderViewModel orderViewModel,
            MyHLShoppingCart cart)
        {
            try
            {
                (new AsyncSubmitOrderProvider()).DoSubmitOrder(paymentResponse, locale.Substring(3, 2),
                    orderViewModel.OrderMonth, cart);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
            }
        }

        public OrderViewModel GetOrderByOrderNumber(string orderNumber, string memberId, string locale, ref int cartId)
        {
            MyHLShoppingCart cart = null;
            return GetOrderDetails(orderNumber, memberId, locale, ref cartId, ref cart, Guid.Empty);
        }

        private void UpdateApfDateIfRequired(MyHLShoppingCart myHlShoppingCart, string memberId, string countryCode)
        {
            try
            {
                if (myHlShoppingCart != null && myHlShoppingCart.CartItems != null &&
                    APFDueProvider.IsAPFSkuPresent(myHlShoppingCart.CartItems))
                {
                    var orderingProfile = DistributorOrderingProfileProvider.GetProfile(memberId, countryCode);
                    if (orderingProfile != null)
                    {
                        var payedApf = APFDueProvider.APFQuantityInCart(myHlShoppingCart);
                        var currentDueDate = orderingProfile.ApfDueDate;
                        var newDueDate = currentDueDate + new TimeSpan(payedApf * 365, 0, 0, 0);
                        orderingProfile.ApfDueDate = newDueDate;
                        APFDueProvider.UpdateAPFDuePaid(memberId, newDueDate);
                        DistributorOrderingProfileProvider.ClearDistributorCache(memberId, "GetBasicDistributorResponse_V01");
                    }
                }

                var distributorOrderingProfileCacheKey = string.Format("DO_DS_{0}", memberId);
                _cache.Expire(typeof(DistributorOrderingProfile), distributorOrderingProfileCacheKey);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
            }
        }


        private OrderViewModel GetOrderDetails(string orderNumber, string memberId, string locale, ref int cartId,
            ref MyHLShoppingCart myhlCart, Guid id)
        {
            var cart = _mobileQuoteHelper.GetShoppingCart(orderNumber, id);
            if (null != cart)
            {
                if (cart.DistributorID != memberId)
                {
                    return null;
                }
                cartId = cart.ShoppingCartID;
                try
                {
                    myhlCart = new MyHLShoppingCart(cart);
                    myhlCart.GetShoppingCartForDisplay(true);
                    myhlCart.LoadShippingInfo(cart.DeliveryOptionID, cart.ShippingAddressID,
                        cart.DeliveryOption, cart.OrderCategory,
                        cart.FreightCode, false);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                            cart.DistributorID, cart.ShoppingCartID, cart.DeliveryOptionID,
                            cart.ShippingAddressID, cart.DeliveryOption, ex.Message));
                }

                return ModelConverter.ConvertShoppingCart_V05ToOrderViewModel(cart);
            }
            var pendingOrder = GetPendingOrder(memberId, locale, orderNumber);
            if (null == pendingOrder) return null;
            if (pendingOrder.ShoppingCartId <= 0) return null;
            cartId = pendingOrder.ShoppingCartId;
            myhlCart = ShoppingCartProvider.GetShoppingCart(memberId, locale, pendingOrder.ShoppingCartId);
            if (null == myhlCart)
            {
                myhlCart = GetShoppingCartFromV02(memberId, locale, pendingOrder.ShoppingCartId);
                if (null == myhlCart) return null;
            }

            var orderViewModel = ModelConverter.ConvertMyHLShoppingCartToOrderViewModel(myhlCart, Guid.Empty);
            if (null != orderViewModel && null == orderViewModel.Quote && locale == "zh-CN" &&
                null != pendingOrder.Order)
            {
                orderViewModel.CountryOfProcessing = "CN";
                var chinaOrder = pendingOrder.Order as OnlineOrder;
                if (null != chinaOrder && null != chinaOrder.Pricing)
                {
                    orderViewModel.Quote = ModelConverter.ConvertToOrderTotalsViewModel(chinaOrder.Pricing);
                }
            }

            orderViewModel.Status = "Unknown";
            orderViewModel.OrderNumber = pendingOrder.OrderId;
            return orderViewModel;
        }

        public MyHLShoppingCart GetShoppingCartFromV02(string memberId, string locale, int shoppingCartId)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new GetShoppingCartRequest_V02()
                    {
                        DistributorID = memberId,
                        Locale = locale,
                        OrderCategory = ServiceProvider.CatalogSvc.OrderCategoryType.RSO
                    };
                    request.ShoppingCartID = shoppingCartId;
                    var response =
                        proxy.GetShoppingCart(new GetShoppingCartRequest1(request)).GetShoppingCartResult as GetShoppingCartResponse_V02;
                    if (null != response && null != response.ShoppingCart)
                    {
                        var cart = new MyHLShoppingCart(response.ShoppingCart);
                        cart.RuleResults = new List<ShoppingCartRuleResult>();
                        cart.GetShoppingCartForDisplay(true);
                        try
                        {
                            cart.LoadShippingInfo(cart.DeliveryOptionID, cart.ShippingAddressID,
                                cart.DeliveryOption, cart.OrderCategory,
                                cart.FreightCode, cart.IsSavedCart);
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Error(
                                string.Format(
                                    "Couldn't load Shipping Info from saved cart info for DS: {0}, Cart ID: {1}, DeliveryOptionID: {2}, ShippingAddressID: {3}, DeliveryOption: {4}. Error is: {5}",
                                    cart.DistributorID, cart.ShoppingCartID, cart.DeliveryOptionID,
                                    cart.ShippingAddressID, cart.DeliveryOption, ex.Message));
                        }
                        return cart;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("General", ex);
                }
            }
            return null;
        }

        private
            PendingOrder GetPendingOrder(string memberId, string locale, string orderNumber)
        {
            try
            {
                if (locale != "zh-CN") return null;
                var orders = OrdersProvider.GetOrdersInProcessing(memberId, locale);
                if (orders != null && orders.Any())
                {
                    var pendingOrders = orders.Where(p => p.OrderId == orderNumber);
                    if (null != pendingOrders && pendingOrders.Any())
                    {
                        return pendingOrders.FirstOrDefault();
                    }
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.Exception("General", exception);
                return null;
            }
            return null;
        }


        private static string GetShippingInstruction(string countryOfProcessing, bool isShipping)
        {
            if (countryOfProcessing == "CN" && isShipping)
            {
                return "工作日、双休日与假日均可送货";
            }
            return string.Empty;
        }



        private static DateTime? SaveMobileOrderResponse(OrderViewModel order, int shoppingCartId, string status,
            bool closeCart)
        {
            using (var proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new InsertMobileOrderDetailRequest_V01();

                    var cardNumber = string.Empty;
                    var cvv = string.Empty;

                    if (null != order && null != order.Payments && order.Payments.Any())
                    {
                        var cardPayment = order.Payments[0] as CardPaymentViewModel;
                        if (null != cardPayment && null != cardPayment.Card)
                        {
                            cardNumber = cardPayment.Card.AccountNumber;
                            cvv = cardPayment.Card.Cvv;
                            cardPayment.Card.AccountNumber = string.Empty;
                            cardPayment.Card.Cvv = string.Empty;
                        }
                    }
                    var mobileOrderDetail =
                        ModelConverter.ConvertOrderViewModelToMobileOrderDetail(order,
                            shoppingCartId);
                    mobileOrderDetail.AddressId = order.Shipping != null && order.Shipping.Address != null
                        ? order.Shipping.Address.CloudId
                        : Guid.Empty;
                    mobileOrderDetail.CustomerId = order.CustomerId;
                    mobileOrderDetail.OrderStatus = status;
                    request.CloseCart = closeCart;
                    request.MobileOrderDetail = mobileOrderDetail;
                    var response = proxy.InsertMobileOrderDetail(new InsertMobileOrderDetailRequest1(request)).InsertMobileOrderDetailResult;
                    // Check response for error.
                    if (response == null || response.Status != ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success)
                    {
                        LoggerHelper.Error(string.Format("SaveMobileOrderResponse error Order: {0} Message: {1}",
                            order.Id, response));
                    }
                    if (null != order && null != order.Payments && order.Payments.Any())
                    {
                        var cardPayment = order.Payments[0] as CardPaymentViewModel;
                        if (null != cardPayment && null != cardPayment.Card)
                        {
                            cardPayment.Card.AccountNumber = cardNumber;
                            cardPayment.Card.Cvv = cvv;
                        }
                    }
                    var responsV01 = response as InsertMobileOrderDetailResponse_V01;
                    if (null != responsV01)
                    {
                        return responsV01.CartUpdatedDate;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("SaveMobileOrderResponse Exception Order: {0} Message: {1}",
                        order.Id, ex));
                    return null;
                }
            }
        }


        private static string getItemDescription(string sku, string country)
        {
            try
            {
                var catalogItem = CatalogProvider.GetCatalogItem(sku, country);
                return catalogItem.Description;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("MobileOrderProvider getItemDescription exception: {0}, getting details for {1} \n",
                        ex.Message, sku));
                return string.Empty;
            }
        }

        private static bool CheckOrderStatus(ShoppingCart_V05 shoppingCart, ref string orderNumber,
            ref OrderViewModel completedOrder)
        {
            var status = null != shoppingCart.MobileOrderDetail &&
                         !string.IsNullOrEmpty(shoppingCart.MobileOrderDetail.OrderStatus) &&
                         shoppingCart.MobileOrderDetail.OrderStatus.Trim().ToUpper() == "COMPLETED";
            if (status)
            {
                completedOrder = !string.IsNullOrEmpty(shoppingCart.MobileOrderDetail.OrderJson)
                    ? ModelConverter.DeserializeObject<OrderViewModel>(shoppingCart.MobileOrderDetail.OrderJson,
                        new[]
                        {
                            typeof (CreditCardViewModel), typeof (WirePaymentViewModel), typeof (WechatPaymentViewModel),
                            typeof (CardPaymentViewModel), typeof (QuickPayPaymentViewModel)
                        })
                    : null;
                if (null != completedOrder)
                {
                    orderNumber = completedOrder.OrderNumber;
                }
            }
            return status;
        }

        private MyHLShoppingCart convertToMyHLShoppingCart(ShoppingCart_V05 shoppingCart)
        {
            var myHlShoppingCart = new MyHLShoppingCart(shoppingCart);
            return myHlShoppingCart;
        }

        protected string GetOrderNumber(decimal amount, MyHLShoppingCart order)
        {
            var request = new GenerateOrderNumberRequest_V01
            {
                Amount = amount,
                Country = order.CountryCode,
                DistributorID = order.DistributorID
            };
            var response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                return response.OrderID;
            }
            return string.Empty;
        }
        //Updating Donation object for china mobile order to insert donation record in CNSMS.Donation table
        private void UpdateDonationforCNSMS(OrderViewModel request, MyHLShoppingCart shoppingCart, OrderTotals_V02 orderTotals)
        {
            if (shoppingCart != null && request != null)
            {
                orderTotals.Donation = decimal.Zero;
                shoppingCart.BehalfOfSelfAmount = decimal.Zero;
                shoppingCart.BehalfOfSelfAmount = decimal.Zero;
                if (null != request.Donations && request.Donations.Count > 0)
                {
                    foreach (var item in request.Donations)
                    {
                        if (!string.IsNullOrEmpty(item.Type))
                        {
                            if (item.Type.ToUpper() == "HFF")
                            {
                                shoppingCart.BehalfOfSelfAmount += item.Amount;
                                shoppingCart.BehalfDonationName = item.Name;
                                shoppingCart.BehalfOfContactNumber = item.PhoneNumber;
                                shoppingCart.BehalfOfMemberId = request.MemberId;
                                orderTotals.Donation += item.Amount;
                            }
                            if (item.Type.ToUpper() == "HFF_ON_BEHALF")
                            {
                                shoppingCart.BehalfOfAmount += item.Amount;
                                shoppingCart.BehalfDonationName = item.Name;
                                shoppingCart.BehalfOfContactNumber = item.PhoneNumber;
                                shoppingCart.BehalfOfMemberId = request.MemberId;
                                orderTotals.Donation += item.Amount;
                            }
                        }
                        else
                        {
                            shoppingCart.BehalfOfSelfAmount += item.Amount;
                            shoppingCart.BehalfDonationName = item.Name;
                            if (!string.IsNullOrEmpty(item.PhoneNumber))
                                shoppingCart.BehalfOfContactNumber = item.PhoneNumber;
                            else
                                shoppingCart.BehalfOfContactNumber = string.Empty;
                            shoppingCart.BehalfOfMemberId = request.MemberId;
                            orderTotals.Donation += item.Amount;
                            return;
                        }
                    }
                }
            }

        }

        internal class ModelConverter
        {
            private static void PopulateOrderPayment(OrderViewModel orderViewModel, Order_V01 order, decimal amount,
           ref List<ValidationErrorViewModel> errors)
            {
                try
                {
                    if (orderViewModel.Payments[0] is QuickPayPaymentViewModel)
                    {
                        var quickpayPayment = orderViewModel.Payments[0] as QuickPayPaymentViewModel;
                        if (quickpayPayment != null)
                        {
                            if (orderViewModel.CountryOfProcessing == "CN" &&
                           string.IsNullOrEmpty(quickpayPayment.AuthorizationMerchantAccount))
                            {
                                quickpayPayment.AuthorizationMerchantAccount = quickpayPayment.Card.IssuerAssociation;
                            }

                            order.Payments = new PaymentCollection
                            {
                                new CreditPayment_V01
                                    {
                                        Address =
                                            quickpayPayment.Address != null
                                                ? ModelConverter.ConvertAddressViewModelToAddress(
                                                    quickpayPayment.Address)
                                                : null,
                                        Amount =
                                            amount,
                                        ApsDistributorID = quickpayPayment.ApsDistributorId,
                                        Card = new QuickPayPayment
                                            {
                                                AccountNumber = CryptographicProvider.Encrypt(quickpayPayment.Card.AccountNumber),
                                                BillingAddress =
                                                    quickpayPayment.Address != null
                                                        ? ModelConverter.ConvertAddressViewModelToAddress(
                                                            quickpayPayment.Address)
                                                        : null,
                                                CVV = !string.IsNullOrEmpty(quickpayPayment.Card.Cvv) ? CryptographicProvider.Encrypt(quickpayPayment.Card.Cvv) : quickpayPayment.Card.Cvv,
                                                Expiration = quickpayPayment.Card.Expiration,
                                                IssuingBankID = quickpayPayment.Card.IssuingBankId,
                                                NameOnCard = quickpayPayment.Card.NameOnCard,
                                                CardHolderId = quickpayPayment.CardHolderId,
                                                CardHolderType = quickpayPayment.CardHolderType,
                                                MobilePhoneNumber = quickpayPayment.MobilePhoneNumber,
                                                IsDebitCard = quickpayPayment.IsDebitCard,
                                                BindCard = quickpayPayment.BindCard,
                                                MobilePin = quickpayPayment.MobilePin,
                                                Token = quickpayPayment.Token,
                                                StorablePAN = quickpayPayment.StorablePAN,
                                            },
                                        TransactionType = quickpayPayment.TransactionType,
                                        Currency = quickpayPayment.Currency,
                                        AuthorizationMerchantAccount = quickpayPayment.AuthorizationMerchantAccount,
                                        PaymentOptions = new PaymentOptions_V01
                                            {
                                                NumberOfInstallments = 1
                                            }
                                    }
                            };
                        }

                    }
                    else if (orderViewModel.Payments[0] is WirePaymentViewModel)
                    {
                        var wirePayment = orderViewModel.Payments[0] as WirePaymentViewModel;
                        order.Payments = new PaymentCollection
                        {
                            new WirePayment_V01
                            {
                                Address =
                                    wirePayment.Address != null
                                        ? ModelConverter.ConvertAddressViewModelToAddress(wirePayment.Address)
                                        : null,
                                Amount =
                                    amount,
                                ApsDistributorID = wirePayment.ApsDistributorId,
                                TransactionType = wirePayment.TransactionType,
                                Currency = wirePayment.Currency,
                                PaymentCode = wirePayment.PaymentCode
                            }
                        };
                    }
                    //veer
                    else if (orderViewModel.Payments[0] is CardPaymentViewModel)
                    {
                        var creditCardPayment = orderViewModel.Payments[0] as CardPaymentViewModel;
                        if (orderViewModel.CountryOfProcessing == "CN" &&
                            string.IsNullOrEmpty(creditCardPayment.AuthorizationMerchantAccount))
                        {
                            creditCardPayment.AuthorizationMerchantAccount = creditCardPayment.Card.IssuerAssociation;
                        }

                        order.Payments = new PaymentCollection
                        {
                            new CreditPayment_V01
                            {
                                Address =
                                    creditCardPayment.Address != null
                                        ? ModelConverter.ConvertAddressViewModelToAddress(creditCardPayment.Address)
                                        : null,
                                Amount =
                                    amount,
                                ApsDistributorID = creditCardPayment.ApsDistributorId,
                                Card = new CreditCard
                                {
                                    AccountNumber = creditCardPayment.Card.AccountNumber,
                                    BillingAddress =
                                        creditCardPayment.Address != null
                                            ? ModelConverter.ConvertAddressViewModelToAddress(creditCardPayment.Address)
                                            : null,
                                    CVV = creditCardPayment.Card.Cvv,
                                    Expiration = creditCardPayment.Card.Expiration,
                                    IssuingBankID = creditCardPayment.Card.IssuingBankId,
                                    NameOnCard = creditCardPayment.Card.NameOnCard
                                },
                                TransactionType = creditCardPayment.TransactionType,
                                Currency = creditCardPayment.Currency,
                                AuthorizationMerchantAccount = creditCardPayment.AuthorizationMerchantAccount,
                                PaymentOptions = new PaymentOptions_V01
                                {
                                    NumberOfInstallments = 1
                                }
                            }
                        };
                    }
                    //veer
                    else if (orderViewModel.Payments[0] is WechatPaymentViewModel)
                    {
                        var wirePayment = orderViewModel.Payments[0] as WechatPaymentViewModel;
                        order.Payments = new PaymentCollection
                        {
                            new WirePayment_V01
                            {
                                Address =
                                    wirePayment.Address != null
                                        ? ModelConverter.ConvertAddressViewModelToAddress(wirePayment.Address)
                                        : null,
                                Amount =
                                    amount,
                                ApsDistributorID = wirePayment.ApsDistributorId,
                                TransactionType = wirePayment.TransactionType,
                                Currency = wirePayment.Currency,
                                PaymentCode = wirePayment.PaymentCode
                            }
                        };
                    }
                    else
                    {
                        errors.Add(new ValidationErrorViewModel { Code = 100412, Reason = "No Payment Information" });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationErrorViewModel
                    {
                        Code = 100412,
                        Reason = string.Format("No Payment Information, {0}", ex.Message)
                    });
                }
            }
            private static void PopulateOrderShipment(OrderViewModel orderViewModel, Order_V01 order)
            {
                if (null != orderViewModel.Shipping)
                {
                    order.Shipment = new ShippingInfo_V01
                    {
                        Address = ConvertAddressViewModelToAddress(orderViewModel.Shipping.Address),
                        WarehouseCode = orderViewModel.Shipping.WarehouseCode,
                        FreightVariant = orderViewModel.Shipping.FreightVariant,
                        ShippingMethodID =
                            !string.IsNullOrEmpty(orderViewModel.Shipping.ShippingMethodId) &&
                            orderViewModel.Shipping.ShippingMethodId != "0"
                                ? orderViewModel.Shipping.ShippingMethodId
                                : HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode,
                        Recipient = orderViewModel.Shipping.Recipient,
                        Phone = orderViewModel.Shipping.Phone
                    };
                }
            }

            private static void PopulateOrderHandlingInfo(OrderViewModel orderViewModel, Order_V01 order)
            {
                if (null != orderViewModel.HandlingInfo)
                {
                    order.Handling = new HandlingInfo_V01
                    {
                        PickupName = orderViewModel.Shipping.Recipient,
                        IncludeInvoice = (InvoiceHandlingType)
                            Enum.Parse(typeof(InvoiceHandlingType), orderViewModel.HandlingInfo.InvoiceHandlingType),
                        ShippingInstructions = !string.IsNullOrEmpty(orderViewModel.HandlingInfo.ShippingInstructions)
                            ? orderViewModel.HandlingInfo.ShippingInstructions
                            : GetShippingInstruction(orderViewModel.CountryOfProcessing,
                                null != orderViewModel.Shipping &&
                                !string.IsNullOrEmpty(orderViewModel.Shipping.DeliveryType) &&
                                orderViewModel.Shipping.DeliveryType.ToUpper() == "SHIPPING")
                    };
                }
            }

            private static OrderItems PopulateOrderItems(IEnumerable<OrderItemViewModel> orderItems,
                OrderTotals_V01 totals,
                string country)
            {
                var items = new OrderItems();
                if ("CN" == country)
                {
                    items.AddRange((from orderItem in orderItems
                                    let itemTotals = totals.ItemTotalsList.Where(t => ((ItemTotal_V01)t).SKU == orderItem.Sku)
                                    let itemTotal = itemTotals.FirstOrDefault() as ItemTotal_V01
                                    select new OnlineOrderItem
                                    {
                                        SKU = orderItem.Sku,
                                        VolumePoint = itemTotal != null ? itemTotal.VolumePoints : decimal.Zero,
                                        Quantity = orderItem.Quantity,
                                        Description = getItemDescription(orderItem.Sku, country)
                                    }).Cast<OrderItem>());
                }
                else
                {
                    items.AddRange((from orderItem in orderItems
                                    let itemTotals = totals.ItemTotalsList.Where(t => ((ItemTotal_V01)t).SKU == orderItem.Sku)
                                    let itemTotal = itemTotals.FirstOrDefault() as ItemTotal_V01
                                    select new OrderItem
                                    {
                                        SKU = orderItem.Sku,
                                        Quantity = orderItem.Quantity
                                    }).Cast<OrderItem>());
                }
                return items;
            }

            public static Order_V01 ConvertOrderViewModelToOrderV01(OrderViewModel orderViewModel,
                ref List<ValidationErrorViewModel> errors,
                OrderTotals_V02 orderTotalsV02, OrderTotals_V01 orderTotalsV01, out decimal amount)
            {
                Order_V01 order = null;
                order = orderViewModel.CountryOfProcessing == "CN"
                    ? new OnlineOrder()
                    : new Order_V01();

                order.DistributorID = orderViewModel.OrderMemberId;
                order.CountryOfProcessing = orderViewModel.CountryOfProcessing;
                order.OrderCategory =
                    string.IsNullOrEmpty(orderViewModel.CategoryType)
                        ? OrderCategoryType.RSO
                        : (OrderCategoryType)
                            Enum.Parse(typeof(OrderCategoryType), orderViewModel.CategoryType);
                order.OrderMonth = string.IsNullOrEmpty(orderViewModel.OrderMonth)
                    ? DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM")
                    : orderViewModel.OrderMonth; //Fixing 1504 null order month
                order.Pricing = orderViewModel.CountryOfProcessing == "CN" ? orderTotalsV02 : orderTotalsV01;
                order.OrderItems =
                    PopulateOrderItems(orderViewModel.OrderItems, orderTotalsV01, orderViewModel.CountryOfProcessing);
                order.ReceivedDate = DateUtils.GetCurrentLocalTime(orderViewModel.CountryOfProcessing);

                amount = 0;

                if (orderViewModel.OrderItems.Count > 0)
                {
                    amount = orderViewModel.CountryOfProcessing == "CN"
                        ? orderTotalsV02.AmountDue
                        : orderTotalsV01.AmountDue;
                }
                else
                {
                    amount = orderViewModel.CountryOfProcessing == "CN"
                        ? orderTotalsV02.Donation
                        : orderTotalsV01.AmountDue;
                }

                PopulateOrderShipment(orderViewModel, order);
                PopulateOrderPayment(orderViewModel, order, amount, ref errors);
                PopulateOrderHandlingInfo(orderViewModel, order);
                return order;
            }

            public static OrderViewModel ConvertMyHLShoppingCartToOrderViewModel(MyHLShoppingCart shoppingCart,
                Guid orderId)
            {
                var orderViewModel = new OrderViewModel
                {
                    Id = orderId,
                    CategoryType = shoppingCart.OrderCategory.ToString(),
                    Locale = shoppingCart.Locale,
                    OrderItems = (null != shoppingCart.CartItems) ? PopulateOrderItems(shoppingCart.CartItems) : null,
                    MemberId = shoppingCart.DistributorID,
                    OrderNumber = shoppingCart.OrderNumber,
                    Shipping =
                        shoppingCart.DeliveryInfo != null
                            ? PopulateShipping(shoppingCart, shoppingCart.DistributorID, shoppingCart.Locale)
                            : null,
                    CountryOfProcessing = shoppingCart.CountryCode,
                    LastUpdatedDate = shoppingCart.LastUpdatedUtc
                };
                return orderViewModel;
            }

            public static OrderViewModel ConvertShoppingCart_V05ToOrderViewModel(ShoppingCart_V05 shoppingCart)
            {
                var orderViewModel = new OrderViewModel
                {
                    CategoryType = shoppingCart.OrderCategory.ToString(),
                    Locale = shoppingCart.Locale,
                    CountryOfProcessing = shoppingCart.Locale.Substring(3, 2),
                    OrderItems = (null != shoppingCart.CartItems) ? PopulateOrderItems(shoppingCart.CartItems) : null,
                    MemberId = shoppingCart.DistributorID,
                    OrderNumber = shoppingCart.OrderNumber,
                    Id = shoppingCart.MobileOrderDetail.OrderId,
                    Shipping = PopulateShipping(shoppingCart),
                    LastUpdatedDate = shoppingCart.LastUpdatedUtc
                };
                orderViewModel = PopulateOrderViewModelFromMobileOrderDetail(orderViewModel,
                    shoppingCart.MobileOrderDetail);
                return orderViewModel;
            }

            private static OrderViewModel PopulateOrderViewModelFromMobileOrderDetail(OrderViewModel order,
                MobileOrderDetail mobileOrderDetail)
            {
                order.Id = mobileOrderDetail.OrderId;
                order.CustomerId = mobileOrderDetail.CustomerId;
                order.Status = mobileOrderDetail.OrderStatus;
                order.LastUpdatedDate = mobileOrderDetail.OrderModifiedDate;

                if (!string.IsNullOrEmpty(mobileOrderDetail.QuoteJson))
                {
                    order.Quote = Deserialize<OrderTotalsViewModel>(mobileOrderDetail.QuoteJson);
                }


                if (!string.IsNullOrEmpty(mobileOrderDetail.OrderJson))
                {
                    var submittedOrder = Deserialize<OrderViewModel>(mobileOrderDetail.OrderJson,
                        new[]
                        {
                            typeof (CreditCardViewModel), typeof (WirePaymentViewModel), typeof (WechatPaymentViewModel),
                            typeof (CardPaymentViewModel), typeof (QuickPayPaymentViewModel)
                        });

                    if (submittedOrder != null)
                    {
                        order.OrderMonth = submittedOrder.OrderMonth;
                        if (order.Shipping != null && submittedOrder.Shipping != null)
                        {
                            order.Shipping.Recipient = submittedOrder.Shipping.Recipient;
                            order.Shipping.Phone = submittedOrder.Shipping.Phone;
                        }
                    }
                }


                return order;
            }

            public static MobileOrderDetail ConvertOrderViewModelToMobileOrderDetail(OrderViewModel order,
                int shoppingCartId)
            {
                var mobileOrderDetail = new MobileOrderDetail
                {
                    Address = new MobileShoppingCartAddress
                    {
                        AddressId = order.Shipping.Address != null ? order.Shipping.Address.CloudId : Guid.Empty,
                        Phone = order.Shipping.Phone,
                        Recipient = order.Shipping.Recipient,
                        Address = order.Shipping.Address != null
                            ? new ServiceProvider.CatalogSvc.Address_V01
                            {
                                City = order.Shipping.Address.City,
                                Country = order.Shipping.Address.Country,
                                CountyDistrict = order.Shipping.Address.CountyDistrict,
                                Line1 = order.Shipping.Address.Line1,
                                Line2 = order.Shipping.Address.Line2,
                                Line3 = order.Shipping.Address.Line3,
                                Line4 = order.Shipping.Address.Line4,
                                StateProvinceTerritory = order.Shipping.Address.StateProvinceTerritory,
                                PostalCode = order.Shipping.Address.PostalCode
                            }
                            : null
                    },
                    AddressId = order.Shipping.Address != null ? order.Shipping.Address.CloudId : Guid.Empty,
                    CustomerId = order.CustomerId,
                    OrderId = order.Id,
                    OrderStatus = order.Status,
                    QuoteId = order.Quote != null ? order.Quote.Id : Guid.Empty,
                    ShoppingCartId = shoppingCartId,
                    QuoteJson = order.Quote != null ? Serialize(order.Quote) : null,
                    OrderJson = SerializeObject(order, new[] { typeof(CreditCardViewModel), typeof(WirePaymentViewModel), typeof(CardPaymentViewModel), typeof(WechatPaymentViewModel), typeof(QuickPayPaymentViewModel) })

                };
                return mobileOrderDetail;
            }

            public static string SerializeObject<T>(T quote, Type[] knownTypes)
            {
                StringWriter orderObject = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(orderObject);
                DataContractSerializer serializer = new DataContractSerializer(typeof(T), knownTypes);
                serializer.WriteObject(xmlTextWriter, quote);
                return orderObject.ToString();
            }

            public static ShoppingCart_V05 ConertOrderViewModelToShoppingCart_V05(OrderViewModel order)
            {
                var shoppingCart = new ShoppingCart_V05();
                try
                {
                    var countryCode = order.Locale.Substring(3, 2);
                    var shippingProvider =
                        ShippingProvider.GetShippingProvider(countryCode);

                    shoppingCart.OrderNumber = order.OrderNumber;
                    shoppingCart.OrderCategory =
                        (ServiceProvider.CatalogSvc.OrderCategoryType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.OrderCategoryType), order.CategoryType);
                    shoppingCart.Locale = order.Locale;
                    shoppingCart.DistributorID = order.OrderMemberId;

                    shoppingCart.CartItems = ConvertOrderItemsToShoppingCartItems(order.OrderItems, 0);


                    if (order.Shipping != null && !string.IsNullOrEmpty(order.Shipping.DeliveryType))
                    {
                        switch (order.Shipping.DeliveryType.ToUpper().Trim())
                        {
                            case "SHIPPING":
                                {
                                    PopulateShippingDetails(shoppingCart, order, shippingProvider);
                                    break;
                                }

                            case "PICKUP":
                                {
                                    PopulatePickupDetails(shoppingCart, order, DeliveryOptionType.Pickup);
                                    break;
                                }

                            case "PICKUPFROMCOURIER":
                                {
                                    PopulatePickupDetails(shoppingCart, order, DeliveryOptionType.PickupFromCourier);
                                    break;
                                }
                        }
                    }

                    shoppingCart.MobileOrderDetail = ConvertOrderViewModelToMobileOrderDetail(order, 0);
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("ConertOrderViewModelToShoppingCart_V05 - MobileOrder {0}",
                            ex.Message));
                }
                return shoppingCart;
            }

            private static void PopulatePickupDetails(ShoppingCart_V05 shoppingCart, OrderViewModel order,
                DeliveryOptionType deliveryOptionType)
            {
                var deliveryOptionId = 0;
                if (order.Shipping.DeliveryOptionId == 0)
                {
                    Int32.TryParse(order.Shipping.WarehouseCode, out deliveryOptionId);
                }
                else
                {
                    deliveryOptionId = order.Shipping.DeliveryOptionId;
                }
                shoppingCart.DeliveryOption = deliveryOptionType;
                shoppingCart.DeliveryOptionID = deliveryOptionId;
                shoppingCart.ShippingAddressID = 0;
            }

            private static void PopulateShippingDetails(ShoppingCart_V05 cart, OrderViewModel order,
                IShippingProvider shippingProvider)
            {
                cart.DeliveryOption = DeliveryOptionType.Shipping;
                cart.DeliveryOptionID = 0;

                var shippingAddress = GetShippingAddress(order.Shipping,
                    shippingProvider,
                    order.OrderMemberId, order.CustomerId, order.Locale);
                if (null == shippingAddress)
                {
                    var shippingAddressId = SaveShippingAddress(order.Shipping,
                        shippingProvider,
                        order.OrderMemberId, order.CustomerId, order.Locale);
                    shippingAddress =
                        ConvertShippingModelToShippingAddress(order.Shipping,
                            order.CustomerId);
                    shippingAddress.ID = shippingAddressId;
                }

                var deliveryOption = new DeliveryOption(shippingAddress);
                var freightCodeAndWarehouse = shippingProvider.GetFreightCodeAndWarehouse(deliveryOption);
                if ((freightCodeAndWarehouse.Length == 2)
                    && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[0])
                    && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[1]))
                {
                    freightCodeAndWarehouse =
                        shippingProvider.GetFreightCodeAndWarehouseForTaxRate(deliveryOption.Address);
                }

                cart.DeliveryOption = DeliveryOptionType.Shipping;
                cart.DeliveryOptionID = 0;
                cart.ShippingAddressID = shippingAddress.ID;

                if ((freightCodeAndWarehouse.Length == 2)
                    && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[0])
                    && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[1]))
                {
                    cart.FreightCode = freightCodeAndWarehouse[0];
                }
            }

            private static ShoppingCartItemList ConvertOrderItemsToShoppingCartItems(
                IEnumerable<OrderItemViewModel> orderItems, int shoppingCartId)
            {
                var shoppingCartItems = new ShoppingCartItemList();
                shoppingCartItems.AddRange(orderItems.Select(item => new ShoppingCartItem_V01
                {
                    ID = shoppingCartId,
                    Quantity = item.Quantity,
                    SKU = item.Sku
                }));
                return shoppingCartItems;
            }

            public static OrderTotalsViewModel ConvertToOrderTotalsViewModel(OrderTotals totals)
            {
                var result = new OrderTotalsViewModel();

                var totalsV01 = totals as OrderTotals_V01;
                var totalsV02 = totals as OrderTotals_V02;
                result.AmountDue = totalsV01 != null ? totalsV01.AmountDue : 0;

                if (totalsV01 != null)
                {
                    result.BalanceAmount = totalsV01.BalanceAmount;
                    if (totalsV01 != null && totalsV01.ChargeList != null)
                    {
                        result.Charges = (from chargeV01 in totalsV01.ChargeList.OfType<Charge_V01>()
                                          select new ChargeViewModel
                                          {
                                              Amount = chargeV01.Amount,
                                              ChargeType = chargeV01.ChargeType.ToString(),
                                              TaxAmount = chargeV01.TaxAmount
                                          }).ToList();
                        result.ChargeFreightAmount = GetChargeAmount("Freight", result.Charges);
                        result.ChargePHAmount = GetChargeAmount("PH", result.Charges);
                    }
                    result.DiscountAmount = totalsV02 != null ? totalsV02.DiscountAmount : 0;
                    result.CreatedDate = DateTime.Now;
                    result.DiscountPercentage = totalsV01.DiscountPercentage;
                    result.DiscountType = totalsV01.DiscountType;
                    result.DiscountedItemsTotal = totalsV01.DiscountedItemsTotal;
                    result.ExciseTax = totalsV01.ExciseTax;
                    result.IcmsTax = totalsV01.IcmsTax;
                    result.IpiTax = totalsV01.IpiTax;
                    result.ItemsTotal = totalsV01.ItemsTotal;
                    result.LineItems = new List<ItemTotalsListViewModel>();
                    if (totalsV01.ItemTotalsList != null)
                        foreach (var itemTotal in totalsV01.ItemTotalsList)
                        {
                            var total = (ItemTotal_V01)itemTotal;
                            var chargeList = new List<ChargeViewModel>();
                            if (total.ChargeList != null)
                            {
                                var chargeList_V01s =
                                    (from ite in total.ChargeList
                                     where ite as Charge_V01 != null
                                     select ite as Charge_V01);
                                chargeList = (from x in chargeList_V01s
                                              select new ChargeViewModel
                                              {
                                                  Amount = x.Amount,
                                                  ChargeType = x.ChargeType.ToString(),
                                                  TaxAmount = x.TaxAmount
                                              }).ToList();
                            }
                            var item = new ItemTotalsListViewModel
                            {
                                Sku = total.SKU,
                                Quantity = total.Quantity,
                                LinePrice = total.LinePrice,
                                RetailPrice = total.RetailPrice,
                                DiscountedPrice = total.DiscountedPrice,
                                DiscountedItemPrice = total.DiscountedItemPrice,
                                LineTax = total.LineTax,
                                EarnBase = total.EarnBase,
                                VolumePoints = total.VolumePoints,
                                ChargeList = chargeList,
                                TaxableAmount = total.TaxableAmount,
                                AfterDiscountTax = total.AfterDiscountTax,
                                Discount = total.Discount,
                                ProductType = total.ProductType.ToString()
                            };

                            result.LineItems.Add(item);
                        }
                    result.LiteratureRetailAmount = totalsV01.LiteratureRetailAmount;
                    result.MarketingFundAmount = totalsV01.MarketingFundAmount;
                    result.MiscAmount = totalsV01.MiscAmount;
                    result.ModifiedDate = DateTime.Now;
                    result.PricingServerName = totalsV01.PricingServerName;
                    result.ProductRetailAmount = totalsV01.ProductRetailAmount;
                    result.ProductTaxTotal = totalsV01.ProductTaxTotal;
                    result.PromotionRetailAmount = totalsV01.PromotionRetailAmount;
                    result.TaxAmount = totalsV01.TaxAmount;
                    result.TaxableAmountTotal = totalsV01.TaxableAmountTotal;
                    result.TotalEarnBase = totalsV01.TotalEarnBase;
                    result.TotalItemDiscount = totalsV01.TotalItemDiscount;
                    result.VolumePoints = totalsV01.VolumePoints;
                }

                return result;
            }

            private static decimal GetChargeAmount(string value, List<ChargeViewModel> charges)
            {
                if (null != charges && charges.Any())
                {
                    var charge =
                        charges.Where(
                            c => !string.IsNullOrEmpty(c.ChargeType) && c.ChargeType.ToUpper() == value.ToUpper());
                    if (null != charge && charge.Any())
                    {
                        return charge.FirstOrDefault().Amount;
                    }
                }
                return 0;
            }

            public static OrderTotals_V01 ConvertOrderTotalsViewModelToWcfOrderTotals(
                OrderTotalsViewModel quoteViewModel)
            {
                var orderTotals = new OrderTotals_V01
                {
                    AmountDue = quoteViewModel.AmountDue,
                    BalanceAmount = quoteViewModel.BalanceAmount,
                    DiscountPercentage = quoteViewModel.DiscountPercentage,
                    DiscountType = quoteViewModel.DiscountType,
                    DiscountedItemsTotal = quoteViewModel.DiscountedItemsTotal,
                    ExciseTax = quoteViewModel.ExciseTax,
                    IcmsTax = quoteViewModel.IcmsTax,
                    ItemsTotal = quoteViewModel.ItemsTotal,
                    IpiTax = quoteViewModel.IpiTax,
                    LiteratureRetailAmount = quoteViewModel.LiteratureRetailAmount,
                    MarketingFundAmount = quoteViewModel.MarketingFundAmount,
                    MiscAmount = quoteViewModel.MiscAmount,
                    PricingServerName = quoteViewModel.PricingServerName,
                    ProductRetailAmount = quoteViewModel.ProductRetailAmount,
                    ProductTaxTotal = quoteViewModel.ProductTaxTotal,
                    PromotionRetailAmount = quoteViewModel.PromotionRetailAmount,
                    QuoteID = quoteViewModel.Id.ToString(),
                    TaxAmount = quoteViewModel.TaxAmount,
                    TaxableAmountTotal = quoteViewModel.TaxableAmountTotal,
                    TotalEarnBase = quoteViewModel.TotalEarnBase,
                    TotalItemDiscount = quoteViewModel.TotalItemDiscount,
                    VolumePoints = quoteViewModel.VolumePoints,
                    ChargeList = GetChargeList(quoteViewModel.Charges),
                    ItemTotalsList = GetItemTotalsList(quoteViewModel.LineItems)
                };
                return orderTotals;
            }

            public static OrderTotals_V02 ConvertOrderTotalsViewModelToWcfOrderTotalsV02(
                OrderTotalsViewModel quoteViewModel, List<DonationViewModel> donation)
            {
                var orderTotals = new OrderTotals_V02
                {
                    AmountDue = quoteViewModel.AmountDue,
                    BalanceAmount = quoteViewModel.BalanceAmount,
                    DiscountPercentage = quoteViewModel.DiscountPercentage,
                    DiscountType = quoteViewModel.DiscountType,
                    DiscountedItemsTotal = quoteViewModel.DiscountedItemsTotal,
                    ExciseTax = quoteViewModel.ExciseTax,
                    IcmsTax = quoteViewModel.IcmsTax,
                    ItemsTotal = quoteViewModel.ItemsTotal,
                    IpiTax = quoteViewModel.IpiTax,
                    LiteratureRetailAmount = quoteViewModel.LiteratureRetailAmount,
                    MarketingFundAmount = quoteViewModel.MarketingFundAmount,
                    MiscAmount = quoteViewModel.MiscAmount,
                    PricingServerName = quoteViewModel.PricingServerName,
                    ProductRetailAmount = quoteViewModel.ProductRetailAmount,
                    ProductTaxTotal = quoteViewModel.ProductTaxTotal,
                    PromotionRetailAmount = quoteViewModel.PromotionRetailAmount,
                    QuoteID = quoteViewModel.Id.ToString(),
                    TaxAmount = quoteViewModel.TaxAmount,
                    TaxableAmountTotal = quoteViewModel.TaxableAmountTotal,
                    TotalEarnBase = quoteViewModel.TotalEarnBase,
                    TotalItemDiscount = quoteViewModel.TotalItemDiscount,
                    VolumePoints = quoteViewModel.VolumePoints,
                    ChargeList = GetChargeList(quoteViewModel.Charges),
                    ItemTotalsList = GetItemTotalsList(quoteViewModel.LineItems),
                    DiscountAmount = quoteViewModel.DiscountAmount,
                    Donation = null != donation && donation.Any() ? donation.FirstOrDefault().Amount : 0
                };
                return orderTotals;
            }

            private static ItemTotalsList GetItemTotalsList(IEnumerable<ItemTotalsListViewModel> lineItems)
            {
                var itemTotals = new ItemTotalsList();
                itemTotals.AddRange(lineItems.Select(item => new ItemTotal_V01
                {
                    SKU = item.Sku,
                    Discount = item.Discount,
                    AfterDiscountTax = item.AfterDiscountTax,
                    DiscountedItemPrice = item.DiscountedItemPrice,
                    DiscountedPrice = item.DiscountedPrice,
                    EarnBase = item.EarnBase,
                    LinePrice = item.LinePrice,
                    LineTax = item.LineTax,
                    ProductType = (ServiceProvider.OrderSvc.ProductType)Enum.Parse(typeof(ServiceProvider.OrderSvc.ProductType), item.ProductType, true),
                    Quantity = item.Quantity,
                    RetailPrice = item.RetailPrice,
                    TaxableAmount = item.TaxableAmount,
                    VolumePoints = item.VolumePoints
                }));
                return itemTotals;
            }


            private static ChargeList GetChargeList(IEnumerable<ChargeViewModel> charges)
            {
                var chargeList = new ChargeList();
                chargeList.AddRange(charges.Select(c => new Charge_V01
                {
                    Amount = c.Amount,
                    ChargeType = (ChargeTypes)Enum.Parse(typeof(ChargeTypes), c.ChargeType, true),
                    TaxAmount = c.TaxAmount
                }));
                return chargeList;
            }

            private static List<OrderItemViewModel> PopulateOrderItems(
                IEnumerable<ShoppingCartItem_V01> shoppingCartItemList)
            {
                return shoppingCartItemList.Select(item => new OrderItemViewModel
                {
                    Quantity = item.Quantity,
                    Sku = item.SKU,
                    ItemType = GetSkuType(item)
                }).ToList();
            }

            private static ShippingViewModel PopulateShipping(ShoppingCart_V05 shoppingCart)
            {
                var shippingModel = new ShippingViewModel
                {
                    Address = shoppingCart.MobileOrderDetail != null
                        ? new AddressViewModel
                        {
                            CloudId = shoppingCart.MobileOrderDetail.AddressId,
                            City = shoppingCart.MobileOrderDetail.Address.Address.City,
                            Country = shoppingCart.MobileOrderDetail.Address.Address.Country,
                            CountyDistrict = shoppingCart.MobileOrderDetail.Address.Address.CountyDistrict,
                            Line1 = shoppingCart.MobileOrderDetail.Address.Address.Line1,
                            Line2 = shoppingCart.MobileOrderDetail.Address.Address.Line2,
                            Line3 = shoppingCart.MobileOrderDetail.Address.Address.Line3,
                            Line4 = shoppingCart.MobileOrderDetail.Address.Address.Line4,
                            PostalCode = shoppingCart.MobileOrderDetail.Address.Address.PostalCode,
                            StateProvinceTerritory =
                                shoppingCart.MobileOrderDetail.Address.Address.StateProvinceTerritory
                        }
                        : null,
                    Carrier = shoppingCart.FreightCode,
                    DeliveryType = shoppingCart.DeliveryOption.ToString().ToUpper(),
                    DeliveryOptionId = shoppingCart.DeliveryOptionID,
                    ShippingMethodId = shoppingCart.FreightCode
                };

                if ((null == shippingModel.Address || string.IsNullOrEmpty(shippingModel.Address.Line1)) &&
                    shoppingCart.ShippingAddressID > 0)
                {
                    var countryCode = !string.IsNullOrEmpty(shoppingCart.Locale)
                        ? shoppingCart.Locale.Substring(3, 2)
                        : string.Empty;
                    if (!string.IsNullOrEmpty(countryCode))
                    {
                        var shippingProvider = ShippingProvider.GetShippingProvider(countryCode);

                        var shippingAddress =
                            shippingProvider.GetShippingAddressFromAddressGuidOrId(Guid.Empty,
                                shoppingCart.ShippingAddressID);
                        if (null != shippingAddress && shippingAddress.Address != null)
                        {
                            shippingModel.Address = new AddressViewModel
                            {
                                City = shippingAddress.Address.City,
                                Country = shippingAddress.Address.Country,
                                CountyDistrict = shippingAddress.Address.CountyDistrict,
                                Line1 = shippingAddress.Address.Line1,
                                Line2 = shippingAddress.Address.Line2,
                                Line3 = shippingAddress.Address.Line3,
                                Line4 = shippingAddress.Address.Line4,
                                PostalCode = shippingAddress.Address.PostalCode,
                                StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory,
                                CloudId = shippingAddress.AddressId,
                                PersonCloudId = shippingAddress.CustomerId
                            };
                        }
                    }
                }
                return shippingModel;
            }

            private static ShippingViewModel PopulateShipping(MyHLShoppingCart shoppingCart, string memberId,
                string locale)
            {
                return ConvertShippingInfoToShippingViewModel(shoppingCart.DeliveryInfo,
                    shoppingCart.DeliveryOption, shoppingCart.DeliveryOptionID, memberId, locale);
            }

            private static ShippingViewModel ConvertShippingInfoToShippingViewModel(ShippingInfo deliveryInfo,
                DeliveryOptionType deliveryOption, int deliveryOptionId, string memberId, string locale)
            {
                var country = locale.Substring(3, 2);
                var shippingViewModel = new ShippingViewModel
                {
                    DeliveryType = deliveryOption.ToString(),
                    DeliveryOptionId = deliveryOptionId,
                    WarehouseCode = deliveryInfo.WarehouseCode,
                    FreightVariant = country == "CN" ? deliveryInfo.AddressType : deliveryInfo.FreightVariant,
                    ShippingMethodId = deliveryInfo.FreightCode,
                    Address =
                        deliveryInfo.Address != null && deliveryInfo.Address.Address != null
                            ? new AddressViewModel
                            {
                                City = deliveryInfo.Address.Address.City,
                                Country = deliveryInfo.Address.Address.Country,
                                CountyDistrict = deliveryInfo.Address.Address.CountyDistrict,
                                Line1 = deliveryInfo.Address.Address.Line1,
                                Line2 = deliveryInfo.Address.Address.Line2,
                                Line3 = deliveryInfo.Address.Address.Line3,
                                Line4 = deliveryInfo.Address.Address.Line4,
                                PostalCode = deliveryInfo.Address.Address.PostalCode,
                                StateProvinceTerritory = deliveryInfo.Address.Address.StateProvinceTerritory
                            }
                            : null,
                    Recipient = deliveryInfo.Address != null ? deliveryInfo.Address.Recipient : string.Empty,
                    Phone = deliveryInfo.Address != null ? deliveryInfo.Address.Phone : string.Empty
                };
                var countryCode = locale.Substring(3, 2);
                var shippingProvider =
                    ShippingProvider.GetShippingProvider(countryCode);

                if (null != shippingViewModel)
                {
                    var shippingAddressId = deliveryInfo.Address != null ? deliveryInfo.Address.ID : 0;
                    if (deliveryOption == DeliveryOptionType.Shipping)
                    {
                        if (shippingAddressId > 0)
                        {
                            var shippingAddress =
                                shippingProvider.GetShippingAddressFromAddressGuidOrId(Guid.Empty,
                                    shippingAddressId);
                            if (null != shippingAddress && shippingAddress.Address != null)
                            {
                                shippingViewModel.Address = new AddressViewModel
                                {
                                    City = shippingAddress.Address.City,
                                    Country = shippingAddress.Address.Country,
                                    CountyDistrict = shippingAddress.Address.CountyDistrict,
                                    Line1 = shippingAddress.Address.Line1,
                                    Line2 = shippingAddress.Address.Line2,
                                    Line3 = shippingAddress.Address.Line3,
                                    Line4 = shippingAddress.Address.Line4,
                                    PostalCode = shippingAddress.Address.PostalCode,
                                    StateProvinceTerritory = shippingAddress.Address.StateProvinceTerritory,
                                    CloudId = shippingAddress.AddressId,
                                    PersonCloudId = shippingAddress.CustomerId,
                                    CustomShippingMethods =
                                        MobileAddressProvider.ModelConverter.GetCustomShippingMethods(memberId,
                                            shippingAddress, locale)
                                };
                            }
                        }
                    }
                    else
                    {
                        var shippDeliveryOption = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), deliveryOption.ToString());
                        var shippingInfo = shippingProvider.GetShippingInfoFromID(memberId, locale,
                            shippDeliveryOption,
                            deliveryOptionId, shippingAddressId);
                        if (null != shippingInfo && shippingInfo.Address != null && shippingInfo.Address.Address != null)
                        {
                            shippingViewModel.Address = new AddressViewModel
                            {
                                City = shippingInfo.Address.Address.City,
                                Country = shippingInfo.Address.Address.Country,
                                CountyDistrict = shippingInfo.Address.Address.CountyDistrict,
                                Line1 = shippingInfo.Address.Address.Line1,
                                Line2 = shippingInfo.Address.Address.Line2,
                                Line3 = shippingInfo.Address.Address.Line3,
                                Line4 = shippingInfo.Address.Address.Line4,
                                PostalCode = shippingInfo.Address.Address.PostalCode,
                                StateProvinceTerritory = shippingInfo.Address.Address.StateProvinceTerritory
                            };
                            shippingViewModel.StoreName = deliveryOption == DeliveryOptionType.PickupFromCourier
                                ? shippingInfo.Name
                                : shippingInfo.Description;
                        }
                    }
                }
                return shippingViewModel;
            }

            public static ShippingAddress_V02 ConvertShippingModelToShippingAddress(ShippingViewModel shippingViewModel,
                string customerId)
            {
                return new ShippingAddress_V02
                {
                    Address = new ServiceProvider.ShippingSvc.Address_V01
                    {
                        City = shippingViewModel.Address.City,
                        Country = shippingViewModel.Address.Country,
                        CountyDistrict = shippingViewModel.Address.CountyDistrict,
                        Line1 = shippingViewModel.Address.Line1,
                        Line2 = shippingViewModel.Address.Line2,
                        Line3 = shippingViewModel.Address.Line3,
                        Line4 = shippingViewModel.Address.Line4,
                        PostalCode = shippingViewModel.Address.PostalCode,
                        StateProvinceTerritory = shippingViewModel.Address.StateProvinceTerritory
                    },
                    AddressId = shippingViewModel.Address != null ? shippingViewModel.Address.CloudId : Guid.Empty,
                    Phone = shippingViewModel.Phone,
                    Recipient = shippingViewModel.Recipient,
                    CustomerId = customerId
                };
            }

            public static ShippingAddress_V02 GetShippingAddress(ShippingViewModel shipping,
                IShippingProvider shippingProvider,
                string memberId, string customerId, string locale)
            {
                //Bug 239907:China Splunk: Mobile.MobileOrderProvider.ModelConverter.GetShippingAddress please Check the object before you use it!!
                if (shipping == null || shipping.Address == null)
                    return null;
                var shippingAddress =
                    shippingProvider.GetShippingAddressFromAddressGuidOrId(shipping.Address.CloudId, 0);
                if (null != shippingAddress && shippingAddress.ID > 0)
                {
                    return shippingAddress;
                }
                return null;
            }

            public static int SaveShippingAddress(ShippingViewModel shipping, IShippingProvider shippingProvider,
                string memberId, string customerId, string locale)
            {
                var shippingInfo = ConvertShippingModelToShippingAddress(shipping,
                    customerId);
                return shippingProvider.SaveShippingAddress(memberId, locale, shippingInfo, false, true, false);
            }

            public static int GetDeliveryOptionIdForPickup(IShippingProvider shippingProvider, string countryCode,
                string locale, ShippingAddress_V02 address,
                DeliveryOptionType deliveryOptionType = DeliveryOptionType.Pickup)
            {
                var shippDeliveryOption = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), deliveryOptionType.ToString());
                var pickupList = shippingProvider.GetDeliveryOptions(shippDeliveryOption,
                    shippingProvider.GetDefaultAddress()) ??
                                 shippingProvider.GetDeliveryOptionsListForPickup(countryCode, locale,
                                     address);

                var pickupOption = pickupList.FirstOrDefault(x => x.Address.Line1 == address.Address.Line1);
                return pickupOption.Id;
            }

            public static MobileOrderSearchParameter PopulateMobileOrderSearchParamter(
                OrderSearchParameter searchParameter)
            {
                return new MobileOrderSearchParameter
                {
                    Locale = searchParameter.Locale,
                    MemberId = searchParameter.MemberId,
                    OrderStatus = searchParameter.Status,
                    PageNumber = searchParameter.PageNumber,
                    PageSize = searchParameter.PageSize,
                    From = searchParameter.From,
                    To = searchParameter.To
                };
            }

            public static string Serialize<T>(T configuration)
            {
                var serializer = new DataContractJsonSerializer(configuration.GetType());
                var stream = new MemoryStream();
                serializer.WriteObject(stream, configuration);
                var result = Encoding.UTF8.GetString(stream.GetBuffer().Where(b => b != '\0').ToArray());
                stream.Close();
                return result;
            }

            public static string Serialize<T>(T configuration, Type[] knownTypes)
            {
                var serializer = new DataContractJsonSerializer(configuration.GetType(), knownTypes);
                var stream = new MemoryStream();
                serializer.WriteObject(stream, configuration);
                var result = Encoding.UTF8.GetString(stream.GetBuffer().Where(b => b != '\0').ToArray());
                stream.Close();
                return result;
            }

            public static T Deserialize<T>(string serializedConfiguration, Type[] knownTypes)
            {
                var serializer = new DataContractJsonSerializer(typeof(T), knownTypes);
                var buffer = Encoding.Unicode.GetBytes(serializedConfiguration);
                var stream = new MemoryStream(buffer);
                var result = (T)serializer.ReadObject(stream);
                stream.Close();
                return result;
            }
            public static T DeserializeObject<T>(string serializedConfiguration, Type[] knownTypes)
            {
                var serializer = new DataContractSerializer(typeof(T), knownTypes);
                var buffer = Encoding.UTF8.GetBytes(serializedConfiguration);
                var stream = new MemoryStream(buffer);
                var result = (T)serializer.ReadObject(stream);
                stream.Close();
                return result;
            }

            public static T Deserialize<T>(string serializedConfiguration)
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    var buffer = Encoding.Unicode.GetBytes(serializedConfiguration);
                    var stream = new MemoryStream(buffer);
                    var result = (T)serializer.ReadObject(stream);
                    stream.Close();
                    return result;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("General", ex);
                    return default(T);
                }
            }

            public static List<OrderItemViewModel> ConvertCartItemsToOrderItems(ShoppingCartItemList cartItems)
            {
                return cartItems.Select(item => new OrderItemViewModel
                {
                    Quantity = item.Quantity,
                    Sku = item.SKU,
                    ItemType = GetSkuType(item)
                }).ToList();
            }

            private static string GetSkuType(ShoppingCartItem_V01 item)
            {
                if (item.IsPromo)
                {
                    return "promotional";
                }

                return APFDueProvider.IsAPFSku(item.SKU) ? "apf" : "regular";
            }


            public static ServiceProvider.OrderSvc.Address_V01 ConvertAddressViewModelToAddress(AddressViewModel address)
            {
                if (null == address)
                {
                    return null;
                }
                return new ServiceProvider.OrderSvc.Address_V01
                {
                    City = address.City,
                    StateProvinceTerritory = address.StateProvinceTerritory,
                    Country = address.Country,
                    CountyDistrict = address.CountyDistrict,
                    Line1 = address.Line1,
                    PostalCode = address.PostalCode,
                    Line2 = address.Line2,
                    Line3 = address.Line3,
                    Line4 = address.Line4
                };
            }
        }
    }
}