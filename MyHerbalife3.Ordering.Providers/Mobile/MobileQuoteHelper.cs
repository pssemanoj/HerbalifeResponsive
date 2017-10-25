#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;



#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileQuoteHelper
    {
        private readonly MobileGetDisconinuedSkuProvider _getDisconinuedSkuProvider =
            new MobileGetDisconinuedSkuProvider();

        public MyHLShoppingCart PriceCart(ref OrderViewModel order, ref List<ValidationErrorViewModel> errors,
            bool supressRules = false, bool checkSkuAvalability = false, bool checkUnSupportedAddress = false)
        {
            try
            {
                var skuErrorModel = new List<SkuErrorModel>();

                Thread.CurrentThread.Name = "MyHLMobile";
                var country = order.Locale.Substring(3, 2);
                var shoppingCart = CreateShoppingCart(order);
                var shoppingcartItems = CreateShoppingCartItems(order, country);
                if (shoppingCart.ShoppingCartItems == null)
                {
                    shoppingCart.ShoppingCartItems = new List<DistributorShoppingCartItem>();
                }
                var isApfSkuInCart = IsApfSkuInCart(shoppingCart.ShoppingCartItems);
                ApfViewModel apf = null;
                if (isApfSkuInCart)
                {
                    apf = GetApf(shoppingCart.RuleResults);
                }
                ClearCart(shoppingCart, isApfSkuInCart, country);
                CheckWarehouseCode(shoppingCart, order);
                PopulateDonation(shoppingCart, order);
                Dictionary<string, SKU_V01> allSKUS = null;
                if (checkSkuAvalability)
                {
                    var warehouse = null != order.Shipping ? order.Shipping.WarehouseCode : string.Empty;
                    var skus = shoppingcartItems.Select(s => s.SKU);
                    if (null != skus && skus.Any())
                    {
                        allSKUS =
                            CatalogProvider.GetProductInfoCatalog(order.Locale, warehouse).AllSKUs;
                        var error = CheckProductAvailability(skus.ToList(), shoppingCart, order.Locale,
                            warehouse, allSKUS);
                        if (null != error)
                        {
                            errors.Add(error);
                            foreach (var sku in error.Skus)
                            {
                                shoppingcartItems.RemoveAll(s => s.SKU == sku.Sku);
                            }
                            if (!shoppingcartItems.Any())
                            {
                                if (skuErrorModel.Any())
                                {
                                    errors.Add(new ValidationErrorViewModel
                                    {
                                        Code = 101301,
                                        Reason = "Out of Stock",
                                        Skus = skuErrorModel
                                    });
                                }
                                return shoppingCart;
                            }
                        }

                        var inventoryErrors = new List<string>();

                        foreach (var item in shoppingcartItems)
                        {
                            if (APFDueProvider.IsAPFSku(item.SKU))
                            {
                                continue;
                            }
                            var availableQuantity = CheckInventory(item.Quantity, item.SKU, inventoryErrors,
                                shoppingCart,
                                null != order.Shipping ? order.Shipping.WarehouseCode : string.Empty, allSKUS);
                            if (availableQuantity > 0 && item.Quantity != availableQuantity)
                            {
                                item.Quantity = availableQuantity;
                                skuErrorModel.Add(new SkuErrorModel {MaxQuantity = availableQuantity, Sku = item.SKU});
                            }
                        }

                        
                        
                        if (skuErrorModel.Any())
                        {
                            errors.Add(new ValidationErrorViewModel
                            {
                                Code = 101301,
                                Reason = "Out Of Stock",
                                Skus = skuErrorModel
                            });
                        }
                    }
                }

                var profile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID,
                    order.CountryOfProcessing);
                
                if (order.Locale == "zh-CN" && checkUnSupportedAddress && null != order.Shipping &&
                    null != order.Shipping.Address &&
                    !string.IsNullOrEmpty(order.Shipping.DeliveryType) &&
                    order.Shipping.DeliveryType.ToUpper().Trim() == "SHIPPING")
                {
                    var errorModel = CheckUnsupportedAddress(order.Shipping.Address, new ShippingProvider_CN());
                    if (null != errorModel)
                    {
                        errors.Add(errorModel);
                    }
                }
               
                //Fix for SR placing for PC
                ValidateShoppingCartForPromotions(shoppingCart, order.MemberId, order.OrderMemberId, order.Locale);
                int ordermonth = 0;
                if (int.TryParse(GetOrderMonthString(order.CountryOfProcessing), out ordermonth))
                {
                    shoppingCart.OrderMonth = ordermonth;
                }
                
                shoppingCart.AddItemsToCart(shoppingcartItems, supressRules);
               
                var slowMovingSkuCount = 0;
                var slowMovingPromoMesaage = string.Empty;
                if (order.Locale == "zh-CN")
                {
                    var display = false;
                    slowMovingPromoMesaage = China.CatalogProvider.GetSlowMovingPopUp(shoppingCart, allSKUS,
                        out slowMovingSkuCount, out display);
                }

                //Fix for Dec PC Promo Freight Exemption
                var totals = shoppingCart.Totals as OrderTotals_V02;
                if (shoppingCart.ShoppingCartIsFreightExempted)
                {
                    Charge_V01 freightCharge = totals.ChargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                    totals.AmountDue -= freightCharge.Amount;
                    freightCharge.Amount = 0;
                    
                }

                order.Quote = MobileOrderProvider.ModelConverter.ConvertToOrderTotalsViewModel(shoppingCart.Totals);
                order.OrderItems =
                    MobileOrderProvider.ModelConverter.ConvertCartItemsToOrderItems(shoppingCart.CartItems);
                order.Quote.Promotions = GetPromotions(shoppingCart.RuleResults, order.OrderMemberId.ToUpper(), order.Locale,
                   order.Shipping.WarehouseCode, profile.CNCustomorProfileID.ToString(), shoppingCart, slowMovingSkuCount > 0 ? slowMovingPromoMesaage : string.Empty);

                if(checkSkuAvalability)
                {
                    var errormodelfopromotion = checkErrorMessageForPromotion(shoppingCart);
                    if (errormodelfopromotion != null && !string.IsNullOrEmpty(errormodelfopromotion.Message))
                    {
                        errors.Add(errormodelfopromotion);
                    }
                }

                //if (totals != null && totals.DiscountedItemsTotal > 0)
                //{
                    var errormodelPCFOPExceeded = checkErrorMessageForPCFOPExceeded(shoppingCart);
                    if (errormodelPCFOPExceeded != null && !string.IsNullOrEmpty(errormodelPCFOPExceeded.Message))
                    {
                        errors.Add(errormodelPCFOPExceeded);
                }
                //}
                
                if (totals != null && totals.IsFreeFreight)
                {
                    var promoViewModel = new PromotionViewModel
                    {
                        Action = string.Empty,
                        Title = "您符合促销规则的免费送货。",
                        Type = "MESSAGE",
                        Sku = string.Empty
                    };
                    order.Quote.Promotions.Add(promoViewModel);
                }
                VerifyCartItemsWithPromoItems(order.OrderItems, order.Quote.Promotions, order.Locale);

                if (order.Locale == "zh-CN" && profile.IsPC)
                {
                    order.Quote.Apf = null;
                }
                else
                {
                    order.Quote.Apf = apf ?? CheckApfStatus2(order.OrderMemberId, order.CountryOfProcessing);
                }
                order.Quote.Id = Guid.NewGuid();

                if (order.Locale == "zh-CN" && profile.IsPC)
                {
                    var errorModel = ValidatePcLearning(shoppingCart, order);
                    if (errorModel != null)
                    {
                        errors.Add(errorModel);
                    }
                }

                return shoppingCart;
            }
            catch (Exception ex)
            {

                LoggerHelper.Error(
                          string.Format("Exception in PriceCart method MobileQuoteProvider DistributorID:{0} ,MemberID:{1},OrderMemderID={2},{3}",
                                       order.CustomerId,order.MemberId,order.OrderMemberId, ex.Message));
               
                return null;
            }
        }
        private ValidationErrorViewModel checkErrorMessageForPromotion(MyHLShoppingCart shoppingcart)
        {

            if (shoppingcart.RuleResults != null && shoppingcart.RuleResults.Any())
            {
                foreach (
                    var ruleResult in
                        shoppingcart.RuleResults.Where(
                            ruleResult =>
                            null != ruleResult.Messages && ruleResult.Messages.Any() && ruleResult.Result == RulesResult.Feedback && ruleResult.RuleName == "Promotional Rules"))
                {
                    foreach (var message in ruleResult.Messages)
                    {
                        return new ValidationErrorViewModel()
                        {
                            Code = 101301,
                            Message = message,
                            Reason = "Out of stock promo sku"
                        };

                    }
                }
            }
            return new ValidationErrorViewModel();

        }
        
        private ValidationErrorViewModel checkErrorMessageForPCFOPExceeded(MyHLShoppingCart shoppingcart)
        {

            if (shoppingcart.RuleResults != null && shoppingcart.RuleResults.Any())
            {
                foreach (
                    var ruleResult in
                        shoppingcart.RuleResults.Where(
                            ruleResult =>
                            null != ruleResult.Messages && ruleResult.Messages.Any() && ruleResult.Result == RulesResult.Failure && ruleResult.RuleName == "PurchasingLimits Rules"))
                {
                    foreach (var message in ruleResult.Messages)
                    {
                        return new ValidationErrorViewModel()
                        {
                            Code = 110430,
                            Message = message,
                            Reason = "FOP Exceeded. Triggers when PC exceeds VP limit on first order."
                        };

                    }
                }
            }
            return new ValidationErrorViewModel();

        }
        
        private List<DiscontinuedSkuItemResponseViewModel> CheckDiscontinuedSku(string distributorId, string locale,
            List<ShoppingCartItem_V01> shoppingCart)
        {
            GetDiscontinuedSkuParam param = new GetDiscontinuedSkuParam
            {
                DistributorId = distributorId,
                Locale = locale,
                ShoppingCartItemToCheck = shoppingCart
            };
            return _getDisconinuedSkuProvider.GetDiscontinuedSkuRequest(param);
        }

        private void ValidateShoppingCartForPromotions(MyHLShoppingCart shoppingCart, string memberId,
            string orderMemberId, string locale)
        {
            if (locale == "zh-CN")
            {
                var originalMemberProfile = DistributorOrderingProfileProvider.GetProfile(memberId, "CN");
                if (null != originalMemberProfile && 
                    !string.IsNullOrEmpty(originalMemberProfile.CNCustCategoryType) &&
                    memberId != orderMemberId)
                {
                    shoppingCart.SrPlacingForPcOriginalMemberId = orderMemberId;
                    if(!Settings.GetRequiredAppSetting("AllowedTypesForPromo").Contains(originalMemberProfile.CNCustCategoryType))
                    {
                        shoppingCart.IgnorePromoSKUAddition = true;
                    }
                }
            }
        }
        private ValidationErrorViewModel ValidatePcLearning(MyHLShoppingCart shoppingCart,OrderViewModel order)
        {
            order.pcLearningPointEligibleOffSet = Convert.ToInt32(Math.Truncate(shoppingCart.PCLearningPoint));
            if (shoppingCart.PCLearningPoint < order.pcLearningPointOffSet)
            {
                return new ValidationErrorViewModel
                    {
                        Code = 101304,
                        Message = "您已超额使用您的学习积分，请调整后再使用",
                        Reason = "PC Learning Input Exceed"
                    };
            }
            if (order.pcLearningPointOffSet > order.Quote.ChargeFreightAmount)
            {
                return new ValidationErrorViewModel
                {
                    Code = 101304,
                    Message = "输入的学习积分兑换金额超过配送费，请调整后再使用",
                    Reason = "PC Learning Input Exceed"
                };
            }
          
            shoppingCart.pcLearningPointOffSet = order.pcLearningPointOffSet;
            var total = shoppingCart.Totals as OrderTotals_V02;//.AmountDue -= order.pcLearningPointOffSet;
            if (total != null)
            {
                order.Quote.AmountDue = total.AmountDue -= order.pcLearningPointOffSet;
            }
            return null;
        }
            private ValidationErrorViewModel CheckUnsupportedAddress(AddressViewModel address,
            ShippingProvider_CN shippingProvider)
        {
            if (null == address || null == shippingProvider)
            {
                return null;
            }
            var message = shippingProvider.GetUnsupportedAddress(address.StateProvinceTerritory, address.City,
                address.CountyDistrict);
            if (string.IsNullOrEmpty(message))
            {
                return null;
        }

            return new ValidationErrorViewModel
            {
                Code = 101304,
                Message = message,
                Reason = "Unsupported Shipping Address"
            };
        }
        
        public bool IsPCExpired(OrderRequestViewModel request)
        {
            bool IsthePCExpired = true;
            var data = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetPCCustomerIdByReferralId(request.Data.MemberId.Trim());
            if (data != null && data.Any())
            {
                foreach (var pcData in data)
                {
                   if(pcData.CustomerId.Trim() != request.Data.OrderMemberId.Trim())
                   {
                       IsthePCExpired = false;
                       break;
                   }
                }
                
            }

            return IsthePCExpired;
        }
        
        private void VerifyCartItemsWithPromoItems(List<OrderItemViewModel> orderItems,
             List<PromotionViewModel> promotions, string locale)
        {
            if (null != promotions && promotions.Any() && null != orderItems && orderItems.Any())
            {
                var items = new List<PromotionViewModel>();
                var itemAddedTypePromos = promotions.Where(s => s.Type == "ITEM-ADDED" && !string.IsNullOrEmpty(s.Sku));
                var nonMatachedItems = itemAddedTypePromos.Where(p => orderItems.All(c => c.Sku != p.Sku));
                var cartPromoItems = orderItems.Where(o => o.ItemType == "promotional");
                var nonMatchedCartItems = cartPromoItems.Where(c => itemAddedTypePromos.All(i => i.Sku != c.Sku));
                if (null != nonMatachedItems && nonMatachedItems.Any())
                {
                    items.AddRange(nonMatachedItems);
                }

                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        promotions.Remove(item);
                    }
                }

                if (null != nonMatchedCartItems && nonMatchedCartItems.Any())
                {
                    promotions.AddRange(nonMatchedCartItems.Select(cartItem => new PromotionViewModel
                    {
                        Sku = cartItem.Sku,
                        Type = "ITEM-ADDED",
                        Title = GetPromoSkusTitle(new List<string>() {cartItem.Sku}, locale, string.Empty)
                    }));
                }


                if (promotions.Any())
                {
                    var messageTypeItems = new List<PromotionViewModel>();
                    var messageBuilder = new StringBuilder();
                    var messageTypePromotions = promotions.Where(p => p.Type == "MESSAGE");
                    if (null != messageTypePromotions && messageTypePromotions.Any())
                    {
                        foreach (var messageType in messageTypePromotions)
                        {
                            messageBuilder.Append(messageType.Title);
                            messageBuilder.Append("\n");
                        }
                    }

                    if (messageTypePromotions.Any())
                    {
                        messageTypeItems.AddRange(messageTypePromotions);
                    }

                    if (messageTypeItems.Any())
                    {
                        foreach (var item in messageTypeItems)
                        {
                            promotions.Remove(item);
                        }
                        promotions.Add(new PromotionViewModel
                        {
                            Sku = string.Empty,
                            Type = "MESSAGE",
                            Title = messageBuilder.ToString()
                        });
                    }
                }
            }
            else if (null != orderItems && orderItems.Any() && orderItems.Any(o => o.ItemType == "promotional"))
            {
                var cartPromoItems = orderItems.Where(o => o.ItemType == "promotional");
                if (null == promotions)
                {
                    promotions = new List<PromotionViewModel>();
                }
                promotions.AddRange(cartPromoItems.Select(item => new PromotionViewModel
                {
                    Sku = item.Sku,
                    Type = "ITEM-ADDED",
                    Title = GetPromoSkusTitle(new List<string>() {item.Sku}, locale, string.Empty)
                }));
            }
        }


        public ShoppingCart_V05 GetShoppingCart(Guid orderId)
        {
            var request = new GetMobileOrderRequest_V01 {OrderId = orderId};
            return GetShoppingCart(request);
        }

        public ShoppingCart_V05 GetShoppingCart(string orderNumber, Guid id)
        {
            var request = new GetMobileOrderRequest_V01 {OrderNumber = id == Guid.Empty ? orderNumber : string.Empty};
            if (id != Guid.Empty)
            {
                request.OrderId = id;
            }
            return GetShoppingCart(request);
        }

        private ShoppingCart_V05 GetShoppingCart(GetMobileOrderRequest request)
        {
            var proxy = ServiceClientProvider.GetCatalogServiceProxy();
            try
            {
                var response = proxy.GetMobileOrder(new GetMobileOrderRequest1(request)).GetMobileOrderResult;
                if (null == response)
                {
                    return null;
                }
                var responseV01 = response as GetMobileOrderResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceProvider.CatalogSvc.ServiceResponseStatusType.Success &&
                    null != responseV01.ShoppingCart)
                {
                    return responseV01.ShoppingCart;
                }
                return null;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MobileOrderProvider GetShoppingCart {0}\n",
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

        public bool CheckIfOrderCompleted(Guid orderId)
        {
            var shoppingCart = GetShoppingCart(orderId);
            if (null != shoppingCart && null != shoppingCart.MobileOrderDetail 
                && !string.IsNullOrEmpty(shoppingCart.MobileOrderDetail.OrderStatus))
            {
                return string.Compare(shoppingCart.MobileOrderDetail.OrderStatus.Trim(), "Completed",
                    StringComparison.OrdinalIgnoreCase) == 0;

            }
            return false;
        }

        public bool CheckIfAnyOrderInProcessing(string memberId, string locale, ref string orderNumber,
            ref bool isSubmitted, string requestOrderNumber)
        {
            try
            {
                if (locale != "zh-CN") return false;
                var orders = OrdersProvider.GetOrdersInProcessing(memberId, locale);
                if (orders != null && orders.Any())
                {
                    if (!string.IsNullOrEmpty(requestOrderNumber))
                    {
                        return !orders.Any(
                            i =>
                                !string.IsNullOrEmpty(i.OrderId) &&
                                string.Equals(i.OrderId.Trim(), requestOrderNumber.Trim(),
                                    StringComparison.CurrentCultureIgnoreCase));
                    }
                    orderNumber = orders.FirstOrDefault().OrderId;
                    
                    return true;
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.Exception("General", exception);
                return false;
            }
            return false;
        }


        private static ApfViewModel CheckApfStatus2(string orderMemberId, string countryOfProcessing)
        {
            try
            {
                var showApfModule = APFDueProvider.ShouldShowAPFModule(orderMemberId, countryOfProcessing);
                if (showApfModule)
                {
                    return new ApfViewModel
                    {
                        ApfOptional = true,
                        Sku = APFDueProvider.GetAPFSku(orderMemberId)
                    };
                }
            }
            catch (Exception exception)
            {
                LoggerHelper.Exception("General", exception);
            }
            return null;
        }

        private static List<ShoppingCartItem_V01> CheckforPromo(MyHLShoppingCart shoppingCart,
            List<ShoppingCartItem_V01> shoppingcartItems, string country)
        {
            if (null == shoppingCart || null == shoppingCart.ShoppingCartItems || null == shoppingcartItems ||
                !shoppingCart.ShoppingCartItems.Any() || !shoppingcartItems.Any())
            {
                return shoppingcartItems;
            }
            var promoSkuToRemove = new List<string>();
            var promoItems = shoppingcartItems.Where(s => s.IsPromo || IsPromoSku(s.SKU, country));
            var cartPromoItems = shoppingCart.ShoppingCartItems.Where(e => e.IsPromo);
            if (null != promoItems && promoItems.Any() && null != cartPromoItems && cartPromoItems.Any())
            {
                promoSkuToRemove.AddRange(from item in promoItems
                    let cartPromoItem = cartPromoItems.Where(i => i.SKU == item.SKU)
                    where null != cartPromoItem && cartPromoItem.Any()
                    select item.SKU);
            }
            if (null == promoSkuToRemove || !promoSkuToRemove.Any()) return shoppingcartItems;
            foreach (var sku in promoSkuToRemove)
            {
                shoppingcartItems.RemoveAll(s => s.SKU == sku);
            }
            return shoppingcartItems;
        }

        private void CheckWarehouseCode(MyHLShoppingCart shoppingCart, OrderViewModel order)
        {
            if (null != shoppingCart && null != shoppingCart.DeliveryInfo &&
                string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode) && null != order.Shipping &&
                !string.IsNullOrEmpty(order.Shipping.DeliveryType) &&
                order.Shipping.DeliveryType.ToUpper() == "SHIPPING")
            {
                shoppingCart.DeliveryInfo.WarehouseCode = order.Shipping.WarehouseCode;
            }

            if (null != shoppingCart && null != shoppingCart.DeliveryInfo &&
                string.IsNullOrEmpty(shoppingCart.DeliveryInfo.FreightVariant) && null != order.Shipping &&
                !string.IsNullOrEmpty(order.Shipping.DeliveryType) &&
                order.Shipping.DeliveryType.ToUpper() == "SHIPPING")
            {
                shoppingCart.DeliveryInfo.FreightVariant = order.Shipping.FreightVariant;
                shoppingCart.DeliveryInfo.AddressType = order.Shipping.FreightVariant;
            }

            if (null != shoppingCart && null != shoppingCart.DeliveryInfo &&
                string.IsNullOrEmpty(shoppingCart.DeliveryInfo.RGNumber) && null != order.Shipping &&
                !string.IsNullOrEmpty(order.Shipping.RecipientIdentification) &&
                !string.IsNullOrEmpty(order.Shipping.DeliveryType) &&
                order.Shipping.DeliveryType.ToUpper() != "SHIPPING")
            {
                shoppingCart.DeliveryInfo.RGNumber = string.Format("{0}|{1}",
                    string.IsNullOrEmpty(order.Shipping.RecipientIdentificationType)
                        ? "Id"
                        : order.Shipping.RecipientIdentificationType, order.Shipping.RecipientIdentification);
            }
        }

        private static void ClearCart(MyHLShoppingCart shoppingCart, bool isApfSkuInCart, string country)
        {
            if (null != shoppingCart.CartItems && shoppingCart.CartItems.Any())
            {
                var skusToRemove =
                    shoppingCart.CartItems.Where(x => !APFDueProvider.IsAPFSku(x.SKU))
                        .Select(i => i.SKU)
                        .ToList();
                shoppingCart.DeleteItemsFromCart(skusToRemove);
            }

            shoppingCart.CartItems = isApfSkuInCart
                ? RemoveNonApfOrNonPromo(shoppingCart.CartItems, country)
                : new ShoppingCartItemList();
        }

        private static ShoppingCartItemList RemoveNonApfOrNonPromo(ShoppingCartItemList shoppingCartItems,
            string country)
        {
            try
            {
                if (APFDueProvider.containsOnlyAPFSku(shoppingCartItems))
                {
                    return shoppingCartItems;
                }
                shoppingCartItems.RemoveAll(s => !APFDueProvider.IsAPFSku(s.SKU));
                return shoppingCartItems;
            }
            catch (Exception exception)
            {
                LoggerHelper.Exception("General", exception);
                return shoppingCartItems;
            }
        }

        private static bool IsPromoSku(string sku, string country)
        {
            var catalogItem = CatalogProvider.GetCatalogItem(sku, country);
            if (null != catalogItem)
            {
                return catalogItem.ProductType == ServiceProvider.CatalogSvc.ProductType.PromoAccessory;
            }
            return false;
        }


        private static bool IsApfSkuInCart(List<DistributorShoppingCartItem> shoppingCartItems)
        {
            return APFDueProvider.IsAPFSkuPresent(shoppingCartItems);
        }

        private static ApfViewModel GetApf(IEnumerable<ShoppingCartRuleResult> ruleResults)
        {
            var results = ruleResults.Where(
                ruleResult =>
                    null != ruleResult.ApfRuleResponse && !string.IsNullOrEmpty(ruleResult.ApfRuleResponse.Message));
            if (null == results || !results.Any())
            {
                return null;
            }
            var apfResponse = results.FirstOrDefault();
            if (null != apfResponse && null != apfResponse.ApfRuleResponse)
            {
                return new ApfViewModel
                {
                    Action = apfResponse.ApfRuleResponse.Action.ToString(),
                    Message = apfResponse.ApfRuleResponse.Message,
                    Name = apfResponse.ApfRuleResponse.Name,
                    Sku = apfResponse.ApfRuleResponse.Sku,
                    Type = apfResponse.ApfRuleResponse.Type.ToString(),
                    ApfOptional = false
                };
            }
            return null;
        }

        private static void PopulateDonation(MyHLShoppingCart shoppingCart, OrderViewModel order)
        {
            if (null == order || null == order.Donations || !order.Donations.Any()) return;
            var orderTotals = new OrderTotals_V02();
            var donationAmount = order.Donations.Sum(s => s.Amount);
            orderTotals.Donation = donationAmount;
            shoppingCart.Totals = orderTotals;

            //Change the warehousecode for pure donations
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && shoppingCart != null && order != null &&
                (order.OrderItems == null || !order.OrderItems.Any()))
            {
                if (null != order.Donations && order.Donations.Count > 0 && shoppingCart.DeliveryInfo != null)
                {
                    MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
                    shippingInfo.WarehouseCode = "888";
                    order.Shipping.WarehouseCode = "888";
        }
                else if (null != order.Donations && order.Donations.Count > 0 && shoppingCart.DeliveryInfo == null)
                {
                    ShippingProvider_CN objCNShipping = new ShippingProvider_CN();
                    shoppingCart = objCNShipping.StandAloneAddressForDonation(shoppingCart);
                }

            }
        }

        private static List<PromotionViewModel> GetPromotions(IEnumerable<ShoppingCartRuleResult> ruleResults,
            string memberId, string locale, string warehouse, string customerid, MyHLShoppingCart shoppingCart,
            string slowMovingPromoMessage)
        {
            var promotions = new List<PromotionViewModel>();
            if (!string.IsNullOrEmpty(slowMovingPromoMessage))
            {
                var slowMovingPromotion = new PromotionViewModel
                {
                    Type = "MESSAGE",
                    Title = HTMLHelper.ToText(slowMovingPromoMessage.Replace("</li>", "\n").Replace("<br>", "\n")),
                };
                promotions.Add(slowMovingPromotion);
            }
            foreach (
                var ruleResult in
                    ruleResults.Where(
                        ruleResult =>
                            null != ruleResult.PromotionalRuleResponses && ruleResult.PromotionalRuleResponses.Any()))
            {
                foreach (var promotionResponse in ruleResult.PromotionalRuleResponses)
                {
                    PromotionViewModel promoViewModel = null;
                    if (null != promotionResponse && !string.IsNullOrEmpty(promotionResponse.Message) &&
                        promotionResponse.Message.Contains("EligibleForChinaPromotionalSKU"))
                    {
                        var messages = promotionResponse.Message.Split('@');
                        if (messages.Length > 0 && messages.Count() == 2)
                        {
                            var badgePromotions = GetPromoSelectableList(memberId, locale, warehouse, TypeOfPromotion.SelectItem, shoppingCart);
                            promotions.AddRange(badgePromotions);
                        }
                        if (null != promoViewModel)
                        {
                            promotions.Add(promoViewModel);
                        }
                    }
                    else if (promotionResponse.Skus != null && promotionResponse.Skus.Any())
                    {
                        foreach (var sku in promotionResponse.Skus)
                        {
                            promoViewModel = new PromotionViewModel
                            {
                                Action = promotionResponse.Action.ToString(),
                                Title = GetPromoSkusTitle(new List<string> {sku}, locale, promotionResponse.Message),
                                Type = GetPromotionTypeValue(promotionResponse.Type),
                                Sku = sku
                            };
                            if (null != promoViewModel)
                            {
                                promotions.Add(promoViewModel);
                            }
                        }
                    }
                    else
                    {
                        promoViewModel = new PromotionViewModel
                        {
                            Action = promotionResponse.Action.ToString(),
                            Title = GetPromoSkusTitle(new List<string> {""}, locale, promotionResponse.Message),
                            Type = GetPromotionTypeValue(promotionResponse.Type),
                            Sku = ""
                        };
                        if (null != promoViewModel)
                        {
                            promotions.Add(promoViewModel);
                        }
                    }
                }
            }

            PromotionInformation promo;
            List<CatalogItem> PcPromoOnly = new List<CatalogItem>();
            Dictionary<string, SKU_V01> _AllSKUS = null;

            promo = ChinaPromotionProvider.GetPCPromotion(customerid, memberId);

            // Check PC Promo
            if (promo != null && promo.promoelement != null)
            {
                var totals = shoppingCart.Totals as OrderTotals_V02;
                decimal currentMonthTotalDue = 0;
                if (promo.MonthlyInfo != null)
                {
                    if (promo.MonthlyInfo.Count > 0)
                        currentMonthTotalDue = promo.MonthlyInfo[0].Amount;
                }
                if (totals != null && totals.OrderFreight != null &&
                    totals.AmountDue + currentMonthTotalDue - totals.OrderFreight.FreightCharge >=
                    promo.promoelement.AmountMinInclude)
                {
                    if (promo.IsEligible)
                    {
                        if (promo.SKUList.Count > 0)
                        {
                            if (shoppingCart != null && shoppingCart.CartItems.Count > 0 &&
                                shoppingCart.DeliveryInfo != null &&
                                !string.IsNullOrEmpty(shoppingCart.DeliveryInfo.WarehouseCode))
                            {
                                _AllSKUS = CatalogProvider.GetAllSKU(locale, shoppingCart.DeliveryInfo.WarehouseCode);
                                SKU_V01 sku;
                                foreach (CatalogItem t in promo.SKUList)
                                {
                                    if (_AllSKUS != null && _AllSKUS.TryGetValue(t.SKU, out sku))
                                    {
                                        if (!ChinaPromotionProvider.GetPCPromoCode(t.SKU).Trim().Equals("PCPromo"))
                                        {
                                            continue;
                                        }
                                        if ((
                                            ShoppingCartProvider.CheckInventory(t as CatalogItem_V01, 1,
                                                shoppingCart.DeliveryInfo.WarehouseCode) > 0 &&
                                            (CatalogProvider.GetProductAvailability(sku,
                                                shoppingCart.DeliveryInfo.WarehouseCode) ==
                                             ProductAvailabilityType.Available)))
                                        {
                                            PcPromoOnly.Add(t);
                                        }
                                    }
                                }

                                var promoInCart =
                                    shoppingCart.CartItems.Select(c => c.SKU).Intersect(PcPromoOnly.Select(f => f.SKU));
                                if (promoInCart.Any())
                                {
                                    //btnAddToCart.Enabled = false;
                                    //divPromo.Visible = false;

                                    PcPromoOnly.Clear();
                                }
                                if (shoppingCart.CartItems.Count == 1)
                                {
                                    if (promoInCart.Any())
                                    {
                                        shoppingCart.DeleteItemsFromCart(promoInCart.ToList(), true);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var itemsInBoth =
                            shoppingCart.CartItems.Where(x => x.IsPromo)
                                .Select(c => c.SKU)
                                .Intersect(promo.SKUList.Select(f => f.SKU));
                        if (itemsInBoth.Any())
                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                    }
                }
                else
                {
                    var itemsInBoth =
                        shoppingCart.CartItems.Where(x => x.IsPromo)
                            .Select(c => c.SKU)
                            .Intersect(promo.SKUList.Select(f => f.SKU));
                    if (itemsInBoth.Any())
                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                }

                if (PcPromoOnly.Count > 0)
                {
                    string titlelist = string.Empty;
                    string skulist = string.Empty;
                    foreach (var item in PcPromoOnly)
                    {
                        titlelist += item.Description + ",";
                        skulist += item.SKU + ",";
                    }

                    titlelist = titlelist.Substring(0, titlelist.Length - 1);
                    skulist = skulist.Substring(0, skulist.Length - 1);

                    var promoViewModel = new PromotionViewModel
                    {
                        //Action = promotionResponse.Action.ToString(),
                        Title = titlelist,
                        Type = GetPromotionTypeValue(TypeOfPromotion.SelectItem),
                        Sku = skulist
                    };

                    promotions.Add(promoViewModel);
                }
            }


            return promotions;
        }


        private static string GetPromoSkusTitle(List<string> skus, string locale, string defaultValue)
        {
            try
            {
                Dictionary<string, SKU_V01> _AllSKUS = null;
                _AllSKUS = CatalogProvider.GetProductInfoCatalog(locale).AllSKUs;
                SKU_V01 sku = null;
                var titles =
                    (from skuValue in skus where _AllSKUS.TryGetValue(skuValue, out sku) select sku.Description).ToList();
                if (null != titles && titles.Any())
                {
                    return string.Join(",", titles.Select(k => k));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                return defaultValue;
            }
            return defaultValue;
        }

        private ValidationErrorViewModel CheckProductAvailability(IEnumerable<string> skus, MyHLShoppingCart cart,
            string locale,
            string warehouseCode, Dictionary<string, SKU_V01> _AllSKUS)
        {
            var errors = new ValidationErrorViewModel();

            var skuList = new List<SkuErrorModel>();
            foreach (var item in skus)
            {
                SKU_V01 sku;
                _AllSKUS.TryGetValue(item, out sku);
                if (sku == null)
                {
                    skuList.Add(new SkuErrorModel { MaxQuantity = 0, Sku = item });
                }
                if (null != sku && CatalogProvider.GetProductAvailability(sku, warehouseCode,
                    cart.DeliveryInfo != null ? (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), cart.DeliveryInfo.Option.ToString())
                     : ServiceProvider.CatalogSvc.DeliveryOptionType.Unknown) ==
                    ProductAvailabilityType.Unavailable)
                {
                    skuList.Add(new SkuErrorModel {MaxQuantity = 0, Sku = item});
                }
            }
            if (skuList.Any())
            {
                return new ValidationErrorViewModel
                {
                    Code = 101301,
                    Reason = "Out of stock",
                    Skus = skuList
                };
            }
            return null;
        }

        private int CheckInventory(int quantity, string skuValue, List<string> errors, MyHLShoppingCart cart,
            string warehouse, Dictionary<string, SKU_V01> allSKUS)
        {
            try
            {
                var newQuantity = quantity;
                var availQuantity = 0;
                SKU_V01 sku;
                allSKUS.TryGetValue(skuValue, out sku);
                HLRulesManager.Manager.CheckInventory(cart, newQuantity, sku, warehouse, ref availQuantity);
                var ruleResultMessages =
                    from r in cart.RuleResults
                    where
                        r.Result == RulesResult.Failure &&
                        (r.RuleName == "Back Order" || r.RuleName == "Inventory Rules")
                    select r.Messages[0];
                if (ruleResultMessages.Any())
                {
                    Array.ForEach(ruleResultMessages.ToArray(), a => errors.Add(ruleResultMessages.First()));
                }
                cart.RuleResults.Clear();
                return availQuantity;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("General", ex);
            }
            return 0;
        }


        private static PromotionViewModel GetPromoSelectableList(string memberId, int quantity, string locale,
            string warehouseCode, TypeOfPromotion promotionType)
        {
            PromotionInformation promo;
            var promoSkus = new Dictionary<string, string>();
            Dictionary<string, SKU_V01> _AllSKUS = null;
            if ((promo = ChinaPromotionProvider.GetChinaPromotion(memberId)) != null)
            {
                if (quantity > 0)
                {
                    if (promo.SKUList.Count > 0)
                    {
                        _AllSKUS = CatalogProvider.GetProductInfoCatalog(locale, warehouseCode).AllSKUs;
                        SKU_V01 sku;
                        foreach (var t in promo.SKUList)
                        {
                            if (_AllSKUS.TryGetValue(t.SKU, out sku))
                            {
                                if (!ChinaPromotionProvider.GetPCPromoCode(t.SKU).Trim().Equals("ChinaBadgePromo"))
                                    // SKUs are same for both DSChinaPromo and PCChinaPromo
                                {
                                    continue;
                                }
                                if ((ShoppingCartProvider.CheckInventory(t as CatalogItem_V01, 1, warehouseCode) > 0 &&
                                     (CatalogProvider.GetProductAvailability(sku,
                                         warehouseCode) == ProductAvailabilityType.Available)))
                                {
                                    if (!promoSkus.ContainsKey(t.SKU))
                                    {
                                        promoSkus.Add(t.SKU, t.Description);
                                    }
                                }
                            }
                        }
                        if (promoSkus.Any())
                        {
                            var promoModel = new PromotionViewModel
                            {
                                Sku = string.Join(",", promoSkus.Select(k => k.Key)),
                                Action = GetPromotionTypeValue(promotionType),
                                Type = GetPromotionTypeValue(promotionType),
                                Title = string.Join(",", promoSkus.Select(k => k.Value))
                            };
                            return promoModel;
                        }
                    }
                }
            }
            return null;
        }

        private static List<PromotionViewModel> GetPromoSelectableList(string memberId, string locale, string warehouseCode, TypeOfPromotion promotionType, MyHLShoppingCart shoppingCart)
        {
            var result = new List<PromotionViewModel>();
            var promo = ChinaPromotionProvider.GetChinaPromotion(memberId);
            ServiceProvider.OrderChinaSvc.BadgeDetails[] memberBadges = null;
            if (promo != null && ChinaPromotionProvider.IsEligibleForBadgePromotion(shoppingCart, "Mobile", memberId, out memberBadges))
            {
                var allSkus = CatalogProvider.GetProductInfoCatalog(locale, warehouseCode).AllSKUs;
                
                if(memberBadges != null && memberBadges.Length > 0)
                {
                    foreach (var catalogItem in promo.SKUList)
                    {
                        SKU_V01 skuItem = null;
                        if (ChinaPromotionProvider.GetPCPromoCode(catalogItem.SKU).Trim().Equals("ChinaBadgePromo") &&
                            allSkus.TryGetValue(catalogItem.SKU, out skuItem))
                        {
                            var findBadgeItem = memberBadges.FirstOrDefault(f => f.BadgeCode == skuItem.SKU);
                            if( findBadgeItem != null &&
                                ShoppingCartProvider.CheckInventory(catalogItem as CatalogItem_V01, findBadgeItem.Quantity, warehouseCode) > 0 &&
                                CatalogProvider.GetProductAvailability(skuItem, warehouseCode) == ProductAvailabilityType.Available)
                            {
                                if (!result.Any(r => r.Sku == catalogItem.SKU))
                                {
                                    result.Add(new PromotionViewModel
                                    {
                                        Sku = catalogItem.SKU,
                                        Action = GetPromotionTypeValue(promotionType),
                                        Type = GetPromotionTypeValue(promotionType),
                                        Title = findBadgeItem.BadegName,
                                        Code = "ChinaBadgePromo",
                                        Quantity = findBadgeItem.Quantity
                                    });
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static string GetPromoSkus(List<string> skus)
        {
            if (null == skus)
            {
                return string.Empty;
            }
            if (null != skus && skus.Any())
            {
                return string.Join(",", skus.ToArray());
            }
            return string.Empty;
        }

        private static string GetPromotionTypeValue(TypeOfPromotion promotionType)
        {
            var promotionTypeValue = string.Empty;
            switch (promotionType)
            {
                case TypeOfPromotion.None:
                    promotionTypeValue = "NONE";
                    break;
                case TypeOfPromotion.Message:
                    promotionTypeValue = "MESSAGE";
                    break;
                case TypeOfPromotion.FreeItem:
                    promotionTypeValue = "ITEM-ADDED";
                    break;
                case TypeOfPromotion.OptionalItem:
                    promotionTypeValue = "OPTIONAL-ITEM-ADDED";
                    break;
                case TypeOfPromotion.SelectItem:
                    promotionTypeValue = "PICK-ITEM-TO-ADD";
                    break;
                case TypeOfPromotion.Freight:
                    promotionTypeValue = "SHIPPING";
                    break;
                case TypeOfPromotion.Volume:
                    promotionTypeValue = "VOLUME";
                    break;
                case TypeOfPromotion.Special:
                    promotionTypeValue = "ITEM-ADDED";
                    break;
                case TypeOfPromotion.Other:
                    promotionTypeValue = "ITEM-ADDED";
                    break;
                case TypeOfPromotion.FirstOrder:
                    promotionTypeValue = "ITEM-ADDED";
                    break;
                default:
                    promotionTypeValue = string.Empty;
                    break;
            }
            return promotionTypeValue;
        }

        private static MyHLShoppingCart CreateShoppingCart(OrderViewModel order)
        {
            MyHLShoppingCart myHLShoppingCart = null;
            try
            {
                var countryCode = order.Locale.Substring(3, 2);
                var shippingProvider =
                    ShippingProvider.GetShippingProvider(countryCode);

                myHLShoppingCart = ShoppingCartProvider.GetShoppingCart(order, order.OrderMemberId, order.Locale, false,
                    true);
                if (order.Shipping != null)
                {
                    LoggerHelper.Error(string.Format("CNID Tracking Enter CreateShoppingCart Method :Customer:{0}, Order:{1},DeliveryType:{3},CNID:{2}", 
                        order.MemberId, order.OrderNumber, order.Shipping.RecipientIdentification,order.Shipping.DeliveryType));
                }
               
                if (myHLShoppingCart != null)
                {
                    myHLShoppingCart.EmailAddress = order.Shipping.Email;
                    if (order.Shipping != null && !string.IsNullOrEmpty(order.Shipping.DeliveryType))
                    {
                        switch (order.Shipping.DeliveryType.ToUpper().Trim())
                        {
                            case "SHIPPING":
                            {
                                LoadShippingDetails(myHLShoppingCart, order, shippingProvider);
                                break;
                            }
                            case "PICKUP":
                            {
                                LoadPickupDetails(order, myHLShoppingCart, DeliveryOptionType.Pickup);
                                break;
                            }

                            case "PICKUPFROMCOURIER":
                            {
                                LoadPickupDetails(order, myHLShoppingCart, DeliveryOptionType.PickupFromCourier);
                                break;
                            }
                        }
                    }
                    if (myHLShoppingCart.DeliveryInfo != null
                        && (!string.IsNullOrEmpty(myHLShoppingCart.DeliveryInfo.WarehouseCode)
                            && order.Locale == "zh-CN"))
                    {
                        China.CatalogProvider.LoadInventory(myHLShoppingCart.DeliveryInfo.WarehouseCode);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Error in MobileQuoteProvider createShoppingCart. error message:  {0}; \r\n Stack Info: {1}",
                        ex.GetBaseException().Message, ex.GetBaseException().StackTrace));
            }
            return myHLShoppingCart;
        }

        private static void LoadPickupDetails(OrderViewModel order, MyHLShoppingCart myHLShoppingCart,
            DeliveryOptionType deliveryOptionType)
        {
            var countryCode = order.Locale.Substring(3, 2);
            var shippingProvider =
                ShippingProvider.GetShippingProvider(countryCode);
            var deliveryOptionId = 0;
            myHLShoppingCart.SMSNotification = order.Shipping != null ? order.Shipping.Phone : string.Empty;
            if (order.Shipping != null)
            {
                var dilveryid = 0;
                if (order.Shipping.DeliveryOptionId > 0)
                {
                    dilveryid = order.Shipping.DeliveryOptionId;

                }
                else
                {
                    Int32.TryParse(order.Shipping.ShippingMethodId, out dilveryid);
                }
                LoggerHelper.Error(string.Format("CNID Tracking Enter LoadPickupDetails :Customer:{0}, Order:{1},DeliveryType:{3},CNID:{2},deliveryOptionId:{4}",
               order.MemberId, order.OrderNumber, order.Shipping.RecipientIdentification, order.Shipping.DeliveryType, dilveryid));
            }
            if (deliveryOptionType == DeliveryOptionType.PickupFromCourier)
            {
                var pickupLocationId = 0;

                if (order.Shipping.DeliveryOptionId == 0)
                {
                    Int32.TryParse(order.Shipping.ShippingMethodId, out pickupLocationId);
                }
                else
                {
                    deliveryOptionId = order.Shipping.DeliveryOptionId;
                }

                if (order.Shipping.DeliveryOptionId == 0)
                {
                    var pickupLocations = shippingProvider.GetPickupLocationsPreferences(order.OrderMemberId,
                        countryCode);
                    var matchedLocations = pickupLocations.Where(x => x.PickupLocationID == pickupLocationId);
                    if (null == matchedLocations || !matchedLocations.Any())
                    {
                        var pickupLocationName = string.Format("{0},{1}", pickupLocationId, order.Shipping.StoreName);
                        deliveryOptionId = shippingProvider.SavePickupLocationsPreferences(order.OrderMemberId, false,
                            pickupLocationId,
                            pickupLocationName,
                            string.Empty, countryCode, false);
                    }
                    else if (null != matchedLocations && matchedLocations.Any())
                    {
                        deliveryOptionId = matchedLocations.FirstOrDefault().ID;
                    }
                }
            }
            else
            {
                if (order.Shipping.DeliveryOptionId == 0)
                {
                    Int32.TryParse(order.Shipping.WarehouseCode, out deliveryOptionId);
                }
                else
                {
                    deliveryOptionId = order.Shipping.DeliveryOptionId;
                }
            }
            var shippingInfo = shippingProvider.GetShippingInfoFromID(order.OrderMemberId, order.Locale,
                (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), deliveryOptionType.ToString()),
                deliveryOptionId, 0);
            if (null != shippingInfo)
            {
                order.Shipping.DeliveryOptionId = deliveryOptionId;
                order.Shipping.Address =
                    MobileAddressProvider.ModelConverter.ConvertShippingAddressToAddressViewModel(shippingInfo.Address);
                order.Shipping.WarehouseCode = shippingInfo.WarehouseCode;
                order.Shipping.ShippingMethodId = !string.IsNullOrEmpty(shippingInfo.FreightCode) &&
                                                  shippingInfo.FreightCode != "0"
                    ? shippingInfo.FreightCode
                    : HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                order.Shipping.FreightVariant = deliveryOptionType == DeliveryOptionType.Pickup ? "SD" : string.IsNullOrEmpty(shippingInfo.AddressType) ? "PUCA" : shippingInfo.AddressType;
                shippingInfo.FreightVariant = order.Shipping.FreightVariant;
                shippingInfo.AddressType = order.Shipping.FreightVariant;
                 if (!string.IsNullOrEmpty(order.Shipping.RecipientIdentification) &&
                    null != myHLShoppingCart.DeliveryInfo)
                {
                    myHLShoppingCart.DeliveryInfo.RGNumber = string.Format("{0}|{1}",
                        string.IsNullOrEmpty(order.Shipping.RecipientIdentificationType)
                            ? "Id"
                            : order.Shipping.RecipientIdentificationType, order.Shipping.RecipientIdentification);
                }
            }else
            {
                if (order.Shipping != null)
                {
                    LoggerHelper.Error(string.Format("CNID Tracking shippingInfo Is null  :Customer:{0}, Order:{1},DeliveryType:{3},CNID:{2},deliveryOptionId:{4}",
         order.MemberId, order.OrderNumber, order.Shipping.RecipientIdentification, order.Shipping.DeliveryType, deliveryOptionId));
                }
     
            }
          
            myHLShoppingCart.UpdateShippingInfo(0, deliveryOptionId,
                (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), deliveryOptionType.ToString()));
        }

        private static bool IsValidFreightCodeAndWarehouse(IList<string> freightCodeAndWarehouse)
        {
            return (freightCodeAndWarehouse != null && freightCodeAndWarehouse.Count == 2)
                   && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[0])
                   && !string.IsNullOrWhiteSpace(freightCodeAndWarehouse[1])
                ;
        }

        private static void LoadShippingDetails(MyHLShoppingCart cart, OrderViewModel order,
            IShippingProvider shippingProvider)
        {
            cart.DeliveryOption = DeliveryOptionType.Shipping;
            cart.DeliveryOptionID = 0;
            cart.SMSNotification = order.Shipping != null ? order.Shipping.Phone : string.Empty;
            var shippingAddress = MobileOrderProvider.ModelConverter.GetShippingAddress(order.Shipping, shippingProvider,
                order.OrderMemberId, order.CustomerId, order.Locale);
            if (null == shippingAddress)
            {
                var shippingAddressId = MobileOrderProvider.ModelConverter.SaveShippingAddress(order.Shipping,
                    shippingProvider,
                    order.OrderMemberId, order.CustomerId, order.Locale);
                shippingAddress =
                    MobileOrderProvider.ModelConverter.ConvertShippingModelToShippingAddress(order.Shipping,
                        order.CustomerId);
                shippingAddress.ID = shippingAddressId;
            }

            var deliveryOption = new DeliveryOption(shippingAddress);
            var shippingInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo(ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping, deliveryOption);

            var freightCodeAndWarehouse = shippingProvider.GetFreightCodeAndWarehouse(deliveryOption);
            if (!IsValidFreightCodeAndWarehouse(freightCodeAndWarehouse))
            {
                freightCodeAndWarehouse =
                    shippingProvider.GetFreightCodeAndWarehouseForTaxRate(deliveryOption.Address);
            }

            cart.UpdateShippingInfo(shippingAddress.ID, 0, ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping);

            if (IsValidFreightCodeAndWarehouse(freightCodeAndWarehouse))
            {
                shippingInfo.WarehouseCode = freightCodeAndWarehouse.Any()
                    ? freightCodeAndWarehouse[1]
                    : shippingInfo.WarehouseCode;
                order.Shipping.WarehouseCode = shippingInfo.WarehouseCode;

                if (!string.IsNullOrEmpty(order.Shipping.ShippingMethodId))
                    //honor the freightcode passed from the client -- Express company code
                {
                    shippingInfo.FreightCode = order.Shipping.ShippingMethodId;
                }
                else
                {
                    shippingInfo.FreightCode = freightCodeAndWarehouse.Any()
                        ? freightCodeAndWarehouse[0]
                        : shippingInfo.FreightCode;

                    order.Shipping.ShippingMethodId = shippingInfo.FreightCode;
                }
                order.Shipping.FreightVariant = "EXP";
                shippingInfo.FreightVariant = "EXP";
                shippingInfo.AddressType = "EXP";
            }
            else
            {
                LoggerHelper.Error(string.Format("No warehouseCode for State {0} -> {1}",
                    shippingAddress.Address.StateProvinceTerritory, shippingAddress.Address.PostalCode));
            }
            cart.DeliveryInfo = shippingInfo;
            cart.ShippingAddressID = shippingAddress.ID;
        }

        private static List<ShoppingCartItem_V01> CreateShoppingCartItems(OrderViewModel order, string country)
        {
            List<ShoppingCartItem_V01> shoppingCartItem = new List<ShoppingCartItem_V01>();
            try
            {
            if (order != null && order.OrderItems != null)
            {

                    foreach (var item in order.OrderItems)
                        {
                        ShoppingCartItem_V01 i = new ShoppingCartItem_V01();
                        i.SKU = item.Sku;
                        i.Quantity = item.Quantity;
                        i.IsPromo = string.Equals("promotional", item.ItemType, StringComparison.OrdinalIgnoreCase);
                        shoppingCartItem.Add(i);
                    }
                    return shoppingCartItem;
        }
            else
            {
                    return new List<ShoppingCartItem_V01>();
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(string.Format("Error when creating the shopping cart order {0}, ex {1}",order,ex.StackTrace),"Error");
                return new List<ShoppingCartItem_V01>();
            }
        }

        private static string GetOrderMonthString(string countrycode)
        {
            DateTime dtOrderMonth = new OrderMonth(countrycode).CurrentOrderMonth;
            return dtOrderMonth.ToString("yyyyMM");
        }
    }
}