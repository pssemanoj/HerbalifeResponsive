using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using System.Xml;
using System.Text;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using Charge = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge;
using Charge_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Charge_V01;
using ChargeTypes = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ChargeTypes;
using OrderTotals_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V01;
using OrderTotals_V02 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.OrderTotals_V02;

namespace MyHerbalife3.Ordering.Rules.Promotional.zh_CN
{
    public class PromotionalRules : MyHerbalifeRule, IShoppingCartRule, IPromoRule
    {
        private const string FreeProductText = "赠品";
        private const string CustType_PC = "PC";
        private SessionInfo CurrentSession = null;

        const string PromotionDateFormat_V00 = "MM-dd-yyyy";

        /// <summary>
        /// Promotional LowerPriceDelivery
        /// </summary>
        public const string Promotional_RuleName_LowerPriceDelivery = "Promotional LowerPriceDelivery";

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown,
                PromotionalRuleResponses = new List<PromotionalRuleResponse>()
            };
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated ||
                    reason == ShoppingCartRuleReason.CartRetrieved || reason == ShoppingCartRuleReason.CartClosed ||
                    reason == ShoppingCartRuleReason.CartItemsRemoved || reason == ShoppingCartRuleReason.CartCalculated)
            {
                try
                {
                    PromotionCollection promotions = null;
                    if (reason == ShoppingCartRuleReason.CartCreated)
                    {
                        if ((promotions = getPromotion()) == null) ChinaPromotionProvider.LoadPromoConfig();
                    }
                    promotions = getPromotion();
                    if (preorderingSku(cart))
                    {
                        return Result;
                    }
                    if (promotions != null)
                    {
                        #region reset RuleResults
                        var ruleResults = cart.RuleResults;
                        if ((ruleResults != null) && (ruleResults.Count > 0))
                        {
                            #region reset/remove Promotional_RuleName_LowerPriceDelivery results
                            var lowerPriceDeliveryRslts = ruleResults.Where(x => x.RuleName == Promotional_RuleName_LowerPriceDelivery).ToList();
                            lowerPriceDeliveryRslts.ForEach(x => cart.RuleResults.Remove(x));
                            #endregion

                            #region reset PromotionalRuleResponses

                            if (null != lowerPriceDeliveryRslts && lowerPriceDeliveryRslts.Any())
                            {
                                lowerPriceDeliveryRslts.ForEach(
                                    x => x.PromotionalRuleResponses = new List<PromotionalRuleResponse>());
                            }

                            #endregion
                        }
                        #endregion

                        processPromotions(promotions, cart, reason, Result);
                    }
                }
                catch (Exception ex)
                {

                    LoggerHelper.Error(
                        string.Format(
                            "Error in China Promotionalrule. DistributorID  : {0} ,Details :{1}", cart.DistributorID, ex.Message + ex.StackTrace));
                }

            }

            return Result;
        }
        private bool preorderingSku(ShoppingCart_V02 cart)
        {
            bool val;
            var myCart = cart as MyHLShoppingCart;

            if (myCart.DeliveryInfo == null || string.IsNullOrEmpty(myCart.DeliveryInfo.WarehouseCode))
                return false;

            val = CatalogProvider.IsPreordering(myCart.CartItems, myCart.DeliveryInfo.WarehouseCode);
            if (val)
            {
                return true;
            }

            val = CatalogProvider.IsPreordering(myCart.CurrentItems, myCart.DeliveryInfo.WarehouseCode);
            if (val)
            {
                return true;
            }
            return false;
        }
        private void processPromotions(PromotionCollection promotions, ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            try
            {
                var shoppingCart = cart as MyHLShoppingCart;
                if (shoppingCart == null)
                    return;

                var dpm = DistributorProfileModel;
                CurrentSession = (dpm != null)
                    ? SessionInfo.GetSessionInfo(dpm.Id, Locale)
                    : SessionInfo.GetSessionInfo(cart.DistributorID, Locale);

                // for some unknown reason, event mode shouldn't come into promo engine (issue only happened in afarm on 05/Feb/2015). in case it does, kick it out.
                if (null != CurrentSession && CurrentSession.IsEventTicketMode) return;

                if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated)
                {
                    var varFirstPromo =
                        promotions.Where(
                            p => (p.PromotionType & PromotionType.SR_FirstOrder) == PromotionType.SR_FirstOrder ||
                                 (p.PromotionType & PromotionType.PC_FirstOrder) == PromotionType.PC_FirstOrder ||
                                 (p.PromotionType & PromotionType.PC_TO_SR_FirstOrder) ==
                                 PromotionType.PC_TO_SR_FirstOrder);
                    if (varFirstPromo.Any())
                    {
                        List<PromoSKUInfo> promoInfo = new List<PromoSKUInfo>();
                        foreach (var promoSkuInfo in varFirstPromo)
                        {
                            FreeSKUCollection collFreeSKU = promoSkuInfo.FreeSKUList;
                            FreeSKU freeSKU = collFreeSKU.First();
                            promoInfo.Add(new PromoSKUInfo
                            {
                                SKU = freeSKU.SKU,
                                Type = promoSkuInfo.PromotionType,
                                PromotionCode = promoSkuInfo.Code,
                            });
                            var itemsInBoth =
                                          cart.CartItems
                                              .Select(c => c.SKU)
                                              .Intersect(promoSkuInfo.FreeSKUList.Select(f => f.SKU));
                            bool CheckSRFirstOrder = promoSkuInfo.PromotionType == PromotionType.SR_FirstOrder;
                            if (itemsInBoth != null && itemsInBoth.Any() && !CheckSRFirstOrder)
                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                        }

                        DistributorOrderingProfile distributorOrderingProfile = null;

                        if (null != CurrentSession && CurrentSession.IsReplacedPcOrder &&
                            CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                        {
                            distributorOrderingProfile = CurrentSession.ReplacedPcDistributorOrderingProfile;
                            shoppingCart.SrPlacingForPcOriginalMemberId =
                                CurrentSession.ReplacedPcDistributorOrderingProfile.Id;
                        }
                        else
                        {
                            distributorOrderingProfile =
                                DistributorOrderingProfileProvider.GetProfile(cart.DistributorID,
                                    "CN");
                        }

                        List<PromotionType> promoTypes =
                            ChinaPromotionProvider.IsFirstOrder(distributorOrderingProfile.Id);
                        if (promoTypes == null)
                        {
                            shoppingCart.FirstOrderPromotionTypes = new List<PromotionType>();
                        }
                        else
                        {
                            shoppingCart.FirstOrderPromotionTypes = promoTypes;
                        }

                        foreach (var promotype in shoppingCart.FirstOrderPromotionTypes)
                        {
                            if (promotype != PromotionType.None)
                            {
                                var varSku = promoInfo.ToList().Find(p => p.Type == promotype);
                                if (varSku != null)
                                {
                                    if (shoppingCart.CartItems.Count > 0)
                                    {
                                        processFirstOrderPromotion(cart, reason, varSku.SKU, Result);
                                    }

                                }
                            }
                        }

                    }
                }
                if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                {
                    if (shoppingCart.CartItems.Count > 0)
                    {


                        var varFirstPromo =
                            promotions.Where(
                                p => (p.PromotionType & PromotionType.SR_FirstOrder) == PromotionType.SR_FirstOrder ||
                                     (p.PromotionType & PromotionType.PC_FirstOrder) == PromotionType.PC_FirstOrder ||
                                     (p.PromotionType & PromotionType.PC_TO_SR_FirstOrder) ==
                                     PromotionType.PC_TO_SR_FirstOrder);
                        if (varFirstPromo.Any())
                        {
                            List<PromoSKUInfo> promoInfo = new List<PromoSKUInfo>();
                            foreach (var promoSkuInfo in varFirstPromo)
                            {
                                FreeSKUCollection collFreeSKU = promoSkuInfo.FreeSKUList;
                                FreeSKU freeSKU = collFreeSKU.First();
                                if (freeSKU != null)
                                    promoInfo.Add(new PromoSKUInfo
                                    {
                                        SKU = freeSKU.SKU,
                                        Type = promoSkuInfo.PromotionType,
                                    });
                            }

                            if (null != shoppingCart.FirstOrderPromotionTypes)
                            {
                                foreach (var promotype in shoppingCart.FirstOrderPromotionTypes)
                                {
                                    if (promotype != PromotionType.None)
                                    {
                                        var varSku = promoInfo.ToList().Find(p => p.Type == promotype);
                                        if (varSku != null)
                                        {
                                            var shoppingCartItemV01 =
                                                cart.CartItems.FirstOrDefault(x => x.SKU.Trim() == varSku.SKU.Trim());
                                            var skus = new List<string>();
                                            if (shoppingCartItemV01 != null)
                                            {
                                                var itemsInBoth =
                                                    shoppingCartItemV01.SKU;
                                                skus.Add(itemsInBoth);
                                                if (itemsInBoth.Any())
                                                {
                                                    var others =
                                                        shoppingCart.CartItems.Where(s => !s.IsPromo)
                                                            .Select(c => c.SKU)
                                                            .Except(skus)
                                                            .Except(APFDueProvider.GetAPFSkuList());
                                                    if (itemsInBoth.Count() == shoppingCart.CartItems.Count ||
                                                        others.Count() == 0)
                                                    {
                                                        shoppingCart.DeleteItemsFromCart(skus.ToList());
                                                    }
                                                }

                                            }

                                        }
                                    }
                                }
                            }

                        }
                    }
                    //To Do: Remove SR Promo Phase2 SKU if all items deleting and its in cart..
                }
                foreach (var promo in promotions)
                {
                    #region check CustType or CustCategoryType

                    bool chkCustType = (promo.CustTypeList != null) && (promo.CustTypeList.Count > 0);
                    bool chkCustCtgyType = (promo.CustCategoryTypeList != null) &&
                                           (promo.CustCategoryTypeList.Count > 0);

                    if (chkCustType || chkCustCtgyType)
                    {
                        DistributorOrderingProfile distributorOrderingProfile =
                            (null != CurrentSession && CurrentSession.IsReplacedPcOrder &&
                             CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                                ? CurrentSession.ReplacedPcDistributorOrderingProfile
                                : DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, this.Country);

                        int rankPassed = 0;
                        if (chkCustType)
                        {
                            if (promo.CustTypeList.Contains(distributorOrderingProfile.CNCustType))
                                rankPassed++;
                            else
                                rankPassed--;
                        }

                        if (chkCustCtgyType)
                        {
                            if (promo.CustCategoryTypeList.Contains(distributorOrderingProfile.CNCustCategoryType))
                                rankPassed++;
                            else
                                rankPassed--;
                        }

                        if (rankPassed < 0) continue; // it will be negative if none of the ranking check passed, skip.
                    }

                    #endregion

                    if ((promo.PromotionType & PromotionType.Order) == PromotionType.Order)
                    {
                        processOrderPromotion(promo, cart, reason, Result);
                    }

                    else if ((promo.PromotionType & PromotionType.Freight) == PromotionType.Freight &&
                             reason == ShoppingCartRuleReason.CartCalculated
                             && !IsPromoTypeFlagOn(promo.PromotionType, PromotionType.LowerPriceDelivery))
                    {
                        processFreightPromotion(promo, cart, reason, Result);
                    }
                    else if ((promo.PromotionType & PromotionType.Volume) == PromotionType.Volume)
                    {
                        processVolumePromotion(promo, cart, reason, Result);
                    }
                    else if ((promo.PromotionType & PromotionType.Special) == PromotionType.Special)
                    {
                        processSpecialPromotion(promo, cart, reason, Result);
                    }
                    else if (IsPromoTypeFlagOn(promo.PromotionType, PromotionType.LowerPriceDelivery)
                             &&
                             ((reason == ShoppingCartRuleReason.CartRetrieved) ||
                              (reason == ShoppingCartRuleReason.CartCalculated)
                              || (reason == ShoppingCartRuleReason.CartItemsAdded))
                        // to overcome changes by changeset 81218
                        )
                    {
                        processFreightPromotion(promo, cart, reason, Result, true);
                    }
                    else if (promo.PromotionType == PromotionType.Other)
                    {
                        processOtherPromotion(promo, cart, reason, Result);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error orrcured in China processPromotions");
            }
        }
        private void processFirstOrderPromotion(ShoppingCart_V02 cart, ShoppingCartRuleReason reason, string SKU, ShoppingCartRuleResult Result, string promotionCode = null)
        {
            try
            {
                if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated)
                {
                    var shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart != null)
                    {
                        if (shoppingCart.CartItems.Find(x => x.SKU == SKU.Trim()) == null)
                        {
                            if (
                                validateSKU(SKU,
                                    shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result) &&
                                !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                            {
                                var itemsToAdd = new List<ShoppingCartItem_V01>
                                {
                                    new ShoppingCartItem_V01
                                    {
                                        Quantity = 1,
                                        SKU = SKU,
                                        Updated = DateTime.Now,
                                        IsPromo = true,
                                    }
                                };
                                shoppingCart.AddItemsToCart(itemsToAdd, true);
                                Array.ForEach(
                                    shoppingCart.ShoppingCartItems.TakeWhile(x => x.SKU == SKU)
                                        .ToArray(), a => a.Flavor = FreeProductText);

                                //ShoppingCartRuleResult scrr = new ShoppingCartRuleResult();
                                SetPromotionalRuleResponses_V02(Result, promotionCode, TypeOfPromotion.FirstOrder,
                                    PromotionAction.ItemAddedToCart, itemsToAdd);
                                if (cart.RuleResults == null) cart.RuleResults = new List<ShoppingCartRuleResult>();
                                //cart.RuleResults.Add(scrr);


                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, processFirstOrderPromotion");
            }

        }

        private void processOrderPromotion(PromotionElement promotion, ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            try
            {
                var shoppingCart = cart as MyHLShoppingCart;
                if (shoppingCart == null)
                    return;

                if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated ||
                    reason == ShoppingCartRuleReason.CartRetrieved)
                {
                    if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                    {
                        if (shoppingCart.CartItems.Count > 0)
                        {
                            #region compare the amount or volume rules if setup

                            PromoEx promoEx = new PromoEx(promotion);
                            if (promoEx.IsConfigured)
                            {
                                var totals = shoppingCart.Totals as OrderTotals_V02;
                                if (totals == null || totals.OrderFreight == null)
                                    return;

                                if (!promoEx.Validate(totals)) return;
                            }

                            #endregion
                            if (promotion.YearlyPromo)
                            {

                                if (!shoppingCart.HasYearlyPromoTaken)
                                {
                                    DistributorOrderingProfile profile = null;
                                    if (CurrentSession != null && CurrentSession.IsReplacedPcOrder &&
                                        CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                                    {


                                        profile = CurrentSession.ReplacedPcDistributorOrderingProfile;
                                        shoppingCart.SrPlacingForPcOriginalMemberId =
                                                       CurrentSession.ReplacedPcDistributorOrderingProfile.Id;
                                    }
                                    else
                                    {
                                        profile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, Country);
                                    }


                                    var EligibleYearlyPromo = MyHerbalife3.Ordering.Providers.ChinaPromotionProvider.CheckEligibleYearlyPromo(profile.CNCustomorProfileID);

                                    if (EligibleYearlyPromo.IsEgibility)
                                    {
                                        foreach (
                                     var sku in
                                         promotion.FreeSKUList.Where(
                                    sku => (shoppingCart.CartItems.Find(x => x.SKU == sku.SKU.Trim()) == null)))
                                        {
                                            if (!validateSKU(sku.SKU,
                                                shoppingCart.DeliveryInfo == null
                                                    ? "300"
                                                    : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result)) continue;
                                            var allSkus = CatalogProvider.GetAllSKU(Locale, shoppingCart.DeliveryInfo == null
                                                    ? "300"
                                                    : shoppingCart.DeliveryInfo.WarehouseCode);
                                            if (allSkus.Keys.Contains(sku.SKU))
                                            {
                                                if (CurrentSession != null)
                                                {
                                                    CurrentSession.HasPromoSku = true;
                                                }
                                            }
                                        }

                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion, shoppingCart, Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                .ToArray(), a => a.Flavor = FreeProductText);

                                        if (shoppingCart.ShoppingCartItems.Exists(x => itemsToAdd.Exists(i => i.SKU == x.SKU)))
                                        {
                                            shoppingCart.HasYearlyPromoTaken = true;

                                        }
                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }
                                    else
                                    {
                                        shoppingCart.HasYearlyPromoTaken = true;
                                    }
                                }
                            }
                            else if (promotion.Code == "MAPPromotion")
                            {
                                  if (CurrentSession != null && CurrentSession.IsReplacedPcOrder )
                                    {
                                        var itemsInBoth =
                                                        cart.CartItems.Where(x => x.IsPromo)
                                                            .Select(c => c.SKU)
                                                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                    }
                                    else
                                    {
                                        
                                        var itemsInBoth =
                                                       cart.CartItems.Where(x => x.IsPromo)
                                                           .Select(c => c.SKU)
                                                           .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion, shoppingCart,
                                                              Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                .ToArray(), a => a.Flavor = FreeProductText);

                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }
                              
                            }
                            else
                            {
                                foreach (
                                    var sku in
                                        promotion.FreeSKUList.Where(
                                            sku => (shoppingCart.CartItems.Find(x => x.SKU == sku.SKU.Trim()) == null)))
                                {
                                    if (!validateSKU(sku.SKU,
                                        shoppingCart.DeliveryInfo == null
                                            ? "300"
                                                : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result)) continue;
                                    var allSkus = CatalogProvider.GetAllSKU(Locale, shoppingCart.DeliveryInfo == null
                                            ? "300"
                                            : shoppingCart.DeliveryInfo.WarehouseCode);
                                    if (allSkus.Keys.Contains(sku.SKU))
                                    {
                                        if (CurrentSession != null)
                                        {
                                            CurrentSession.HasPromoSku = true;
                                        }
                                    }
                                }
                                List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion, shoppingCart, Result);
                                shoppingCart.AddItemsToCart(itemsToAdd, true);
                                Array.ForEach(
                                    shoppingCart.ShoppingCartItems.TakeWhile(x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                        .ToArray(), a => a.Flavor = FreeProductText);

                                SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                            }
                        }
                    }
                }
                else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                {
                    if (promotion.FreeSKUList != null)
                    {
                        if (shoppingCart.CartItems.Count > 0)
                        {
                            // exist both in shopping cart and free sku list
                            var itemsInBoth =
                                shoppingCart.CartItems.Select(c => c.SKU)
                                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                            // what are in cart are free sku
                            if (itemsInBoth.Count() > 0)
                            {
                                // the rest of sku which is not free sku and not APF
                                var others =
                                    shoppingCart.CartItems.Where(s => !s.IsPromo)
                                        .Select(c => c.SKU)
                                        .Except(itemsInBoth)
                                        .Except(APFDueProvider.GetAPFSkuList());
                                if (itemsInBoth.Count() == shoppingCart.CartItems.Count || others.Count() == 0)
                                {
                                    shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList());
                                    shoppingCart.HasYearlyPromoTaken = false;
                                    if (CurrentSession != null && CurrentSession.surveyDetails != null &&
                                        cart.CartItems.Find(
                                            x => x.SKU == CurrentSession.surveyDetails.SurveySKU.Trim()) != null &&
                                        !CurrentSession.surveyDetails.SurveyCompleted)
                                    {
                                        shoppingCart.DeleteItemsFromCart(new List<string>()
                                        {
                                            CurrentSession.surveyDetails.SurveySKU.Trim()

                                        });
                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, processOrderPromotion");
            }
        }

        private void SetPromotionalRuleResponses_FreeSku(ShoppingCartRuleResult ruleResult, string promotionCode,
            List<ShoppingCartItem_V01> items)
        {
            SetPromotionalRuleResponses(TypeOfPromotion.FreeItem.ToString(), items, ruleResult,
                PromotionAction.ItemAddedToCart, TypeOfPromotion.FreeItem, promotionCode);
        }

        private void SetPromotionalRuleResponses_FreeListSku(ShoppingCartRuleResult ruleResult, string promotionCode, string message)
        {
            try
            {
                if (null == ruleResult) return;

                if (ruleResult.Result == RulesResult.Unknown) ruleResult.Result = RulesResult.Feedback;

                var response = new PromotionalRuleResponse
                {
                    Action = PromotionAction.ItemAddedToCart,
                    Message = message,
                    Name = TypeOfPromotion.SelectItem.ToString(),
                    Type = TypeOfPromotion.SelectItem,
                };

                if (null == ruleResult.PromotionalRuleResponses)
                {
                    ruleResult.PromotionalRuleResponses = new List<PromotionalRuleResponse>();
                }
                ruleResult.PromotionalRuleResponses.Add(response);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, SetPromotionalRuleResponses_FreeListSku");
            }
        }

        private void SetPromotionalRuleResponses_V02(ShoppingCartRuleResult ruleResult, string promotionCode,
            TypeOfPromotion promotionType, PromotionAction action
            , List<ShoppingCartItem_V01> items)
        {
            SetPromotionalRuleResponses(promotionType.ToString(), items, ruleResult, action, promotionType,
                promotionCode);
        }

        private void SetPromotionalRuleResponses(string promoSubTypeName, List<ShoppingCartItem_V01> items,
            ShoppingCartRuleResult ruleResult, PromotionAction action, TypeOfPromotion promotionType, string message)
        {
            try
            {
                if (null == ruleResult) return;

                if (ruleResult.Result == RulesResult.Unknown) ruleResult.Result = RulesResult.Feedback;

                if (null == items || !items.Any()) return;
                var response = new PromotionalRuleResponse
                {
                    Action = action,
                    Message = string.Format("{0} {1}", promoSubTypeName, message),
                    Name = promoSubTypeName,
                    Type = promotionType,
                    Skus = new List<string>()
                };

                foreach (var item in items)
                {
                    response.Skus.Add(item.SKU);
                }

                if (null == ruleResult.PromotionalRuleResponses)
                {
                    ruleResult.PromotionalRuleResponses = new List<PromotionalRuleResponse>();
                }
                ruleResult.PromotionalRuleResponses.Add(response);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, SetPromotionalRuleResponses");
            }
        }

        private void getMinMax(PromotionElement promotion, out decimal valueMin, out decimal valueMax)
        {
            valueMax = decimal.MaxValue;
            valueMin = decimal.MinValue;

            if ((promotion.PromotionType & PromotionType.Volume) == PromotionType.Volume)
            {
                if (promotion.VolumeMax > -1M && promotion.VolumeMaxInclude == -1M)
                {
                    valueMax = promotion.VolumeMax - 1;
                }
                if (promotion.VolumeMin > -1M && promotion.VolumeMinInclude == -1M)
                {
                    valueMin = promotion.VolumeMin - 1;
                }
                if (promotion.VolumeMaxInclude > -1M && promotion.VolumeMax == -1M)
                {
                    valueMax = promotion.VolumeMaxInclude;
                }
                if (promotion.VolumeMinInclude > -1M && promotion.VolumeMin == -1M)
                {
                    valueMin = promotion.VolumeMinInclude;
                }
            }
            else if ((promotion.PromotionType & PromotionType.AmountDue) == PromotionType.AmountDue)
            {
                if (promotion.AmountMax > -1M && promotion.AmountMaxInclude == -1M)
                {
                    valueMax = promotion.AmountMax - 1;
                }
                if (promotion.AmountMin > -1M && promotion.AmountMinInclude == -1M)
                {
                    valueMin = promotion.AmountMin - 1;
                }
                if (promotion.AmountMaxInclude > -1M && promotion.AmountMax == -1M)
                {
                    valueMax = promotion.AmountMaxInclude;
                }
                if (promotion.AmountMinInclude > -1M && promotion.AmountMin == -1M)
                {
                    valueMin = promotion.AmountMinInclude;
                }
            }
        }

        /// <summary>
        /// processFreightPromotion
        /// </summary>
        /// <param name="promotion"></param>
        /// <param name="cart"></param>
        /// <param name="reason"></param>
        /// <param name="Result"></param>
        /// <param name="isLowerPriceDelivery"></param>
        private void processFreightPromotion(PromotionElement promotion, ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult result, bool isLowerPriceDelivery = false)
        {
            try
            {
                //if (reason == ShoppingCartRuleReason.CartFreightCodeChanging || reason == ShoppingCartRuleReason.CartCreated)
                if ((reason == ShoppingCartRuleReason.CartCalculated)
                    || (isLowerPriceDelivery && ((reason == ShoppingCartRuleReason.CartRetrieved)
                                                 || (reason == ShoppingCartRuleReason.CartItemsAdded))
                        // to overcome changes by changeset 81218
                        )
                    )
                {
                    var shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart == null)
                        return;
                    var totals = shoppingCart.Totals as OrderTotals_V02;
                    if (totals == null || totals.OrderFreight == null)
                        return;

                    DistributorOrderingProfile profile = null;
                    if (CurrentSession != null && CurrentSession.IsReplacedPcOrder &&
                        CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                    {
                        //not applicable for Dec pc promo
                        if (promotion.PromotionType == PromotionType.Freight && promotion.ShippedToProvince.Count == 0 &&
                            promotion.DSStoreProvince.Count == 0) return;

                        profile = CurrentSession.ReplacedPcDistributorOrderingProfile;
                    }
                    else
                    {
                        profile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, Country);
                    }

                    if (profile == null)
                        return;

                    bool isDeliveryTypeMatch = false;
                    var deliveryInfo = shoppingCart.DeliveryInfo;
                    if (deliveryInfo != null)
                    {
                        isDeliveryTypeMatch = (deliveryInfo.AddressType == "EXP"); // defaultDSChinaPromo

                        if (isLowerPriceDelivery)
                        {
                            var deliveryTypeList = promotion.DeliveryTypeList;
                            if ((deliveryTypeList != null) && (deliveryTypeList.Count > 0))
                            {
                                isDeliveryTypeMatch = deliveryTypeList.Contains(deliveryInfo.AddressType);
                            }
                        }
                    }

                    if (isDeliveryTypeMatch)
                    {
                        bool isValid = true;
                        string DSStoreProvince = profile.CNStoreProvince != null
                            ? profile.CNStoreProvince.Trim()
                            : string.Empty;
                        if ((promotion.PromotionType & PromotionType.DSProvince) == PromotionType.DSProvince)
                        {
                            if (!string.IsNullOrEmpty(DSStoreProvince))
                            {
                                if (!promotion.DSStoreProvince.Contains(DSStoreProvince))
                                    isValid = false;
                            }
                        }
                        if ((promotion.PromotionType & PromotionType.ShiptoProvince) == PromotionType.ShiptoProvince)
                        {
                            if (shoppingCart.DeliveryInfo.Address != null)
                            {
                                if (!promotion.ShippedToProvince.Contains(
                                    shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory.Trim()))
                                    isValid = false;
                            }
                        }

                        if ((promotion.PromotionType & PromotionType.DSProvince) == PromotionType.DSProvince &&
                            (promotion.PromotionType & PromotionType.ShiptoProvince) == PromotionType.ShiptoProvince)
                        {
                            if (shoppingCart.DeliveryInfo.Address == null ||
                                !DSStoreProvince.Equals(
                                    shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory.Trim()))
                                isValid = false;
                        }

                        if (isValid)
                        {
                            if (!isLowerPriceDelivery) // below is original implementation for non-LowerPriceDelivery
                            {
                                decimal freightDiscountAmount = decimal.Zero;
                                decimal decMax = decimal.MaxValue;
                                decimal decMin = decimal.MinValue;
                                getMinMax(promotion, out decMin, out decMax);
                                if (((promotion.PromotionType & PromotionType.Volume) == PromotionType.Volume) &&
                                    (totals.VolumePoints <= decMax && totals.VolumePoints >= decMin)
                                    ||
                                    ((promotion.PromotionType & PromotionType.AmountDue) == PromotionType.AmountDue) &&
                                    (totals.AmountDue <= decMax && totals.AmountDue >= decMin))
                                {
                                    if (totals.OrderFreight.ActualFreight > promotion.MaxFreight)
                                    {
                                        freightDiscountAmount = totals.OrderFreight.ActualFreight - promotion.MaxFreight;
                                        totals.OrderFreight.ActualFreight = promotion.MaxFreight;
                                        Charge_V01 freightCharge =
                                            totals.ChargeList.Find(
                                                delegate (Charge p)
                                                {
                                                    return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT;
                                                }) as
                                                Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                                        freightCharge.Amount = totals.OrderFreight.ActualFreight;
                                        totals.AmountDue -= freightDiscountAmount;
                                        totals.ProductRetailAmount -= freightDiscountAmount;
                                        // AmountDue in OrderHeader table
                                        //totals.MiscAmount += freightDiscountAmount;
                                        // totals.DiscountAmount += freightDiscountAmount;

                                        SetPromotionalRuleResponses_V02(result, promotion.Code, TypeOfPromotion.Freight,
                                            PromotionAction.FreightDiscount, null);
                                    }
                                }
                                else if (promotion.PromotionType == PromotionType.Freight &&
                                         promotion.ShippedToProvince.Count == 0 && promotion.DSStoreProvince.Count == 0)
                                {
                                    PromoEx promoEx = new PromoEx(promotion);
                                    var tot = totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation;
                                    if (promoEx.ValidateTotalDue(tot))
                                    {
                                        List<string> excludedExpID = promotion.excludedExpID;
                                        Charge_V01 freightCharge =
                                            totals.ChargeList.Find(
                                                delegate (Charge p)
                                                {
                                                    return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT;
                                                }) as
                                                Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);

                                        if (
                                            DSStoreProvince ==
                                            shoppingCart.DeliveryInfo.Address.Address.StateProvinceTerritory.Trim()
                                            &&
                                            !(excludedExpID.Count > 0 &&
                                              excludedExpID.Contains(shoppingCart.FreightCode))
                                            )
                                        {
                                            totals.AmountDue -= freightCharge.Amount;
                                            freightCharge.Amount = 0;
                                            if (CurrentSession != null)
                                            {
                                                CurrentSession.IsFreightExempted = true;
                                            }
                                            shoppingCart.ShoppingCartIsFreightExempted = true;
                                        }
                                        else
                                        {
                                            if (CurrentSession != null)
                                            {
                                                CurrentSession.IsFreightExempted = false;
                                            }
                                            shoppingCart.ShoppingCartIsFreightExempted = false;
                                        }
                                    }
                                    else
                                    {
                                        if (CurrentSession != null)
                                        {
                                            CurrentSession.IsFreightExempted = false;
                                        }
                                        shoppingCart.ShoppingCartIsFreightExempted = false;
                                    }
                                }
                            }
                            else
                            {
                                #region isLowerPriceDelivery

                                PromoEx promoEx = new PromoEx(promotion);
                                if (promoEx.Validate(totals))
                                {
                                    ShoppingCartRuleResult ruleRslt = new ShoppingCartRuleResult
                                    {
                                        Result = RulesResult.Success,
                                        RuleName = Promotional_RuleName_LowerPriceDelivery,
                                    };

                                    ruleRslt.AddMessage(string.Format("Code:{0}", promotion.Code));
                                    cart.RuleResults.Add(ruleRslt);

                                    if (reason == ShoppingCartRuleReason.CartCalculated)
                                    {
                                        // only zero-ize the freight charge if lower-price-delivery company is selected

                                        int doId = 0;
                                        if (Helper.Instance.HasData(cart.LowerPriceDeliveryIdList)
                                            && int.TryParse(cart.FreightCode, out doId) &&
                                            cart.LowerPriceDeliveryIdList.Contains(doId))
                                        {
                                            Charge_V01 freightCharge =
                                                totals.ChargeList.Find(
                                                    delegate (Charge p)
                                                    {
                                                        return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT;
                                                    }) as
                                                    Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                                            var freightDiscountAmount = freightCharge.Amount;
                                            freightCharge.Amount = 0;
                                            totals.AmountDue -= freightDiscountAmount;

                                            SetPromotionalRuleResponses_V02(result, promotion.Code,
                                                TypeOfPromotion.Freight, PromotionAction.FreeFreightCharge, null);
                                        }
                                    }

                                }

                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, processFreightPromotion");
            }
        }

        #region PromoEx

        /// <summary>
        /// To manage the amount and volume comparison logic.
        /// </summary>
        class PromoEx
        {
            public decimal? AmountMin { get; set; }
            public decimal? AmountMinInclude { get; set; }
            public decimal? AmountMax { get; set; }
            public decimal? AmountMaxInclude { get; set; }

            public decimal? VolumeMin { get; set; }
            public decimal? VolumeMinInclude { get; set; }
            public decimal? VolumeMax { get; set; }
            public decimal? VolumeMaxInclude { get; set; }

            /// <summary>
            /// Return false if no rules of amount/rule setup.
            /// </summary>
            public bool IsConfigured { get; set; }

            const decimal DefaultVal = -1;

            public PromoEx(PromotionElement promo)
            {
                if (promo.AmountMin != DefaultVal) { AmountMin = promo.AmountMin; IsConfigured = true; }
                if (promo.AmountMax != DefaultVal) { AmountMax = promo.AmountMax; IsConfigured = true; }
                if (promo.AmountMinInclude != DefaultVal) { AmountMinInclude = promo.AmountMinInclude; IsConfigured = true; }
                if (promo.AmountMaxInclude != DefaultVal) { AmountMaxInclude = promo.AmountMaxInclude; IsConfigured = true; }

                if (promo.VolumeMin != DefaultVal) { VolumeMin = promo.VolumeMin; IsConfigured = true; }
                if (promo.VolumeMax != DefaultVal) { VolumeMax = promo.VolumeMax; IsConfigured = true; }
                if (promo.VolumeMinInclude != DefaultVal) { VolumeMinInclude = promo.VolumeMinInclude; IsConfigured = true; }
                if (promo.VolumeMaxInclude != DefaultVal) { VolumeMaxInclude = promo.VolumeMaxInclude; IsConfigured = true; }
            }

            public bool Validate(OrderTotals_V02 compare)
            {
                var amt = compare.AmountDue;
                if (this.AmountMax.HasValue && (amt >= this.AmountMax.Value)) return false;
                if (this.AmountMaxInclude.HasValue && (amt > this.AmountMaxInclude.Value)) return false;

                if (this.AmountMin.HasValue && (amt <= this.AmountMin.Value)) return false;
                if (this.AmountMinInclude.HasValue && (amt < this.AmountMinInclude.Value)) return false;

                var vol = compare.VolumePoints;
                if (this.VolumeMax.HasValue && (vol >= this.VolumeMax.Value)) return false;
                if (this.VolumeMaxInclude.HasValue && (vol > this.VolumeMaxInclude.Value)) return false;

                if (this.VolumeMin.HasValue && (vol <= this.VolumeMin.Value)) return false;
                if (this.VolumeMinInclude.HasValue && (vol < this.VolumeMinInclude.Value)) return false;

                return true;
            }

            public bool ValidateTotalDue(decimal AmountDue)
            {
                var amt = AmountDue;
                if (this.AmountMax.HasValue && (amt >= this.AmountMax.Value)) return false;
                if (this.AmountMaxInclude.HasValue && (amt > this.AmountMaxInclude.Value)) return false;

                if (this.AmountMin.HasValue && (amt <= this.AmountMin.Value)) return false;
                if (this.AmountMinInclude.HasValue && (amt < this.AmountMinInclude.Value)) return false;

                return true;
            }
        }
        #endregion

        List<ShoppingCartItem_V01> getItemsToAdd(PromotionElement promotion, MyHLShoppingCart shoppingCart, ShoppingCartRuleResult Result)
        {
            List<ShoppingCartItem_V01> itemsToAdd = new List<ShoppingCartItem_V01>();
            var currentsession = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, Locale);
            foreach (var sku in promotion.FreeSKUList)
            {
                if (promotion.Code.Equals("PCPromoSurvey"))
                {
                    if ((shoppingCart.CartItems.Find(x => x.SKU == sku.SKU.Trim()) == null) &&
                        ChinaPromotionProvider.IsEligibleForPCPromoSurveyFreeSku(shoppingCart.DistributorID, Country, Convert.ToDateTime(promotion.StartDate), Convert.ToDateTime(promotion.EndDate), sku.SKU, promotion.Code))
                    {
                        if (validateSKU(sku.SKU, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result))
                        {
                            var allSkus = CatalogProvider.GetAllSKU(Locale, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode);
                            if (allSkus.Keys.Contains(sku.SKU))
                            {
                                itemsToAdd.Add(new ShoppingCartItem_V01
                                {
                                    Quantity = sku.Quantity,
                                    SKU = sku.SKU,
                                    Updated = DateTime.Now,
                                    IsPromo = true,
                                });
                            }
                            else
                            {
                                var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                                string message =
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "NoPromoSku").ToString(), country.DisplayName, sku.SKU);
                                LoggerHelper.Error(message);
                                Result.Result = RulesResult.Failure;
                                Result.RuleName = "Promotional Rules";
                                Result.AddMessage(string.Format(message, country.DisplayName, sku.SKU));
                                shoppingCart.RuleResults.Add(Result);
                            }
                        }
                    }
                }
                else
                {
                    if (shoppingCart.CartItems.Find(x => x.SKU == sku.SKU.Trim()) == null)
                    {
                        if (validateSKU(sku.SKU, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result))
                        {
                            var allSkus = CatalogProvider.GetAllSKU(Locale, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode);
                            if (allSkus.Keys.Contains(sku.SKU))
                            {
                                itemsToAdd.Add(new ShoppingCartItem_V01
                                {
                                    Quantity = sku.Quantity,
                                    SKU = sku.SKU,
                                    Updated = DateTime.Now,
                                    IsPromo = true,
                                });

                            }
                            else
                            {
                                var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                                string message =
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "NoPromoSku").ToString(), country.DisplayName, sku.SKU);
                                LoggerHelper.Error(message);
                                Result.Result = RulesResult.Failure;
                                Result.RuleName = "Promotional Rules";
                                Result.AddMessage(string.Format(message, country.DisplayName, sku.SKU));
                                shoppingCart.RuleResults.Add(Result);
                            }

                        }
                    }
                }

            }
            return itemsToAdd;
        }

        List<ShoppingCartItem_V01> getItemsToAdd(FreeSKUCollection freeSkuCollection, MyHLShoppingCart shoppingCart, ShoppingCartRuleResult Result)
        {
            List<ShoppingCartItem_V01> itemsToAdd = new List<ShoppingCartItem_V01>();
            var currentsession = SessionInfo.GetSessionInfo(shoppingCart.DistributorID, Locale);
            foreach (var sku in freeSkuCollection)
            {


                if (shoppingCart.CartItems.Find(x => x.SKU == sku.SKU.Trim()) == null)
                {
                    if (validateSKU(sku.SKU, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode, shoppingCart, Result))
                    {
                        var allSkus = CatalogProvider.GetAllSKU(Locale, shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode);
                        if (allSkus.Keys.Contains(sku.SKU))
                        {
                            itemsToAdd.Add(new ShoppingCartItem_V01
                            {
                                Quantity = sku.Quantity,
                                SKU = sku.SKU,
                                Updated = DateTime.Now,
                                IsPromo = true,
                            });

                        }
                        else
                        {
                            var country = new RegionInfo(new CultureInfo(Locale, false).LCID);
                            string message =
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "NoPromoSku").ToString(), country.DisplayName, sku.SKU);
                            LoggerHelper.Error(message);
                            Result.Result = RulesResult.Failure;
                            Result.RuleName = "Promotional Rules";
                            Result.AddMessage(string.Format(message, country.DisplayName, sku.SKU));
                            shoppingCart.RuleResults.Add(Result);
                        }

                    }
                }


            }
            return itemsToAdd;
        }

        private void processVolumePromotion(PromotionElement promotion, ShoppingCart_V02 cart,
            ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            try
            {
                if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated)
                {

                    if (promotion.FreeSKUList != null)
                    {
                        var shoppingCart = cart as MyHLShoppingCart;
                        if (shoppingCart == null)
                            return;
                        var totals = shoppingCart.Totals as OrderTotals_V02;
                        if (totals == null)
                            return;
                        decimal decMax = decimal.MaxValue;
                        decimal decMin = decimal.MinValue;
                        var temppromotion = (PromotionElement)promotion.Clone();

                        if ((temppromotion.SelectableSKUList == null) || (temppromotion.SelectableSKUList.Count == 0))
                        {

                            getMinMax(temppromotion, out decMin, out decMax);
                            if (totals.VolumePoints <= decMax && totals.VolumePoints >= decMin)
                            {
                                if (shoppingCart.CartItems.Count > 0 &&
                                    !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                                {
                                    if (!promotion.HasIncrementaldegree)
                                    {
                                        if (promotion.Code == "SRChinaPromo")
                                        {
                                            if (!CurrentSession.IsReplacedPcOrder && ChinaPromotionProvider.IsEligibleForSRPromotion(shoppingCart, HLConfigManager.Platform))
                                            {
                                                var message = HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                                    "EligibleForChinaPromotionalSKU");
                                                Result.Result = RulesResult.Failure;
                                                Result.AddMessage(message.ToString());
                                                SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                                    "EligibleForChinaPromotionalSKU@1");
                                                cart.RuleResults.Add(Result);
                                            }
                                        }
                                        else
                                        {
                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion, shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                    .ToArray(), a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                        }
                                    }
                                    else
                                    {
                                        if (promotion.HasIncrementaldegree)
                                        {
                                            if (totals.VolumePoints >= temppromotion.VolumeMinInclude)
                                            {
                                                if (promotion.Code == "DSChinaPromo")
                                                {
                                                    var Quantity = (int)totals.VolumePoints /
                                                                   temppromotion.VolumeMinInclude;
                                                    if (CurrentSession != null)
                                                    {
                                                        CurrentSession.ChinaPromoSKUQuantity = (int)Quantity;
                                                    }
                                                    var message = HttpContext.GetGlobalResourceObject(
                                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                                        "EligibleForChinaPromotionalSKU");
                                                    Result.Result = RulesResult.Failure;
                                                    Result.AddMessage(message.ToString());
                                                    SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                                        "EligibleForChinaPromotionalSKU" +
                                                        "@" + (int)Quantity);
                                                    cart.RuleResults.Add(Result);
                                                }


                                                else
                                                {
                                                    var itemsInBoth =
                                                        cart.CartItems.Where(x => x.IsPromo)
                                                            .Select(c => c.SKU)
                                                            .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));
                                                    if (itemsInBoth.Any())
                                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                                    //var quantity = (int) totals.VolumePoints/temppromotion.VolumeMinInclude;
                                                    //var freeSkuCollection = new FreeSKUCollection();
                                                    //freeSkuCollection.AddRange(
                                                    //    promotion.FreeSKUList.Select(
                                                    //        sku => new FreeSKU {Quantity = (int) quantity, SKU = sku.SKU}));
                                                    var freeSkuCollection = CheckDSPromo(promotion, cart);
                                                    var itemsToAdd = getItemsToAdd(freeSkuCollection, shoppingCart,
                                                        Result);
                                                    shoppingCart.AddItemsToCart(itemsToAdd, true);
                                                    Array.ForEach(
                                                        shoppingCart.ShoppingCartItems.TakeWhile(
                                                            x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                            .ToArray(), a => a.Flavor = FreeProductText);

                                                    SetPromotionalRuleResponses_FreeSku(Result, promotion.Code,
                                                        itemsToAdd);
                                                }

                                            }
                                            else
                                            {
                                                var itemsInBoth =
                                                    cart.CartItems.Where(x => x.IsPromo)
                                                        .Select(c => c.SKU)
                                                        .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));
                                                if (itemsInBoth.Any())
                                                    shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                            }
                                        }
                                    }

                                }
                            }
                            else
                            {
                                if (CurrentSession != null)
                                {
                                    CurrentSession.ChinaPromoSKUQuantity = 0;
                                }
                                var itemsInBoth =
                                    cart.CartItems.Where(x => x.IsPromo)
                                        .Select(c => c.SKU)
                                        .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                if (itemsInBoth.Any())
                                    shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                            }
                        }
                        else
                        {
                            #region

                            if (promotion.HasIncrementaldegree)
                            {

                                if (promotion.Code == "DSNovPromo")
                                {
                                    if (totals.VolumePoints >= temppromotion.VolumeMinInclude)
                                    {
                                        var itemsInBoth =
                                            cart.CartItems.Where(x => x.IsPromo)
                                                .Select(c => c.SKU)
                                                .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));

                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                        var freeSkuCollection = CheckDsNovPromo(promotion, cart);

                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                            shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                            a => a.Flavor = FreeProductText);

                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }
                                    else
                                    {
                                        var itemsInBoth =
                                            cart.CartItems.Where(x => x.IsPromo)
                                                .Select(c => c.SKU)
                                                .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));

                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                        var freeSkuCollection = new FreeSKUCollection();
                                        var count = (from selectablesku in promotion.SelectableSKUList
                                                     from cartitem in shoppingCart.CartItems
                                                     where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                     select cartitem.Quantity).Sum();
                                        var itemsToAdd = new List<ShoppingCartItem_V01>();
                                        if (count > 0)
                                        {
                                            foreach (var sku in promotion.FreeSKUListForSelectableSku)
                                            {
                                                count = count * sku.Quantity;
                                                var freesku = new FreeSKU { Quantity = count, SKU = sku.SKU };
                                                freeSkuCollection.Add(freesku);

                                            }
                                            itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                a => a.Flavor = FreeProductText);
                                        }


                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }


                                }

                                else
                                {

                                    if (totals.VolumePoints >= temppromotion.VolumeMinInclude)
                                    {
                                        // First deleting all existing promoskus for PDM Borsch flavor Feb 2015 [蛋白营养粉（罗宋汤口味）上市促销信息(userstory :146058)
                                        var itemsInBoth =
                                            cart.CartItems.Where(x => x.IsPromo)
                                                .Select(c => c.SKU)
                                                .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));
                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                        var freeSkuCollection = DSPromo(promotion, cart);

                                        //   temppromotion.FreeSKUList = freeSkuCollection;
                                        //    List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(temppromotion, shoppingCart,Result);
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                            shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                .ToArray(), a => a.Flavor = FreeProductText);

                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }

                                    else
                                    {
                                        //deleting all existing promoskus for PDM Borsch flavor Feb 2015 [蛋白营养粉（罗宋汤口味）上市促销信息(userstory :146058)
                                        var itemsInBoth =
                                            cart.CartItems.Where(x => x.IsPromo)
                                                .Select(c => c.SKU)
                                                .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                    }


                                }
                            }

                            #endregion

                            else
                            {
                                if (totals.VolumePoints > promotion.VolumeMinInclude)
                                {
                                    if (shoppingCart.CartItems.Count > 0 &&
                                        !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                                    {
                                        var count = (from selectablesku in promotion.SelectableSKUList
                                                     from cartitem in shoppingCart.CartItems
                                                     where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                     select cartitem.Quantity).Sum();

                                        if (count > 1)
                                        {
                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                        }
                                        else
                                        {
                                            var itemsInBoth =
                                                cart.CartItems.Where(x => x.IsPromo)
                                                    .Select(c => c.SKU)
                                                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                            if (itemsInBoth.Any())
                                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                        }
                                    }
                                }
                                else
                                {
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                }
                            }

                        }
                    }

                }
                else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                {

                    var shoppingCart = cart as MyHLShoppingCart;
                    var totals = shoppingCart.Totals as OrderTotals_V02;
                    if (totals == null)
                    {
                        return;
                    }

                    if (totals.VolumePoints > promotion.VolumeMinInclude)
                    {
                        if (shoppingCart.CartItems.Count > 0 &&
                            !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                        {
                            if (!promotion.HasIncrementaldegree)
                            {
                                var count = (from selectablesku in promotion.SelectableSKUList
                                             from cartitem in shoppingCart.CartItems
                                             where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                             select cartitem.Quantity).Sum();
                                if (count < 2)
                                {
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                }
                            }
                            else if (promotion.HasIncrementaldegree)
                            {
                                var temppromotion = (PromotionElement)promotion.Clone();
                                if (promotion.Code == "DSChinaPromo")
                                {
                                    var Quantity = (int)totals.VolumePoints / temppromotion.VolumeMinInclude;
                                    if (CurrentSession != null)
                                    {
                                        CurrentSession.ChinaPromoSKUQuantity = (int)Quantity;
                                    }

                                    //var message = HttpContext.GetGlobalResourceObject(
                                    //                string.Format("{0}_Rules", HLConfigManager.Platform), "EligibleForChinaPromotionalSKU");
                                    //Result.Result = RulesResult.Failure;
                                    //SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                    //                    "EligibleForChinaPromotionalSKU" + "@" + (int)Quantity);
                                    //Result.AddMessage(message.ToString());
                                    //cart.RuleResults.Add(Result);
                                }
                                else
                                {
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                    if ((temppromotion.SelectableSKUList == null) ||
                                        (temppromotion.SelectableSKUList.Count == 0))
                                    {

                                        var quantity = (int)totals.VolumePoints / temppromotion.VolumeMinInclude;
                                        var freeSkuCollection = new FreeSKUCollection();
                                        freeSkuCollection.AddRange(
                                            promotion.FreeSKUList.Select(
                                                sku => new FreeSKU { Quantity = (int)quantity, SKU = sku.SKU }));
                                        var itemsToAdd = getItemsToAdd(freeSkuCollection, shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                .ToArray(), a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    }
                                    else
                                    {
                                        if (promotion.Code == "DSNovPromo")
                                        {
                                            //  if (totals.VolumePoints >= temppromotion.VolumeMinInclude)
                                            //  {
                                            itemsInBoth =
                                                cart.CartItems.Where(x => x.IsPromo)
                                                    .Select(c => c.SKU)
                                                    .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));

                                            if (itemsInBoth.Any())
                                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                            var Q = totals.VolumePoints / temppromotion.VolumeMinInclude;
                                            var v = (from selectablesku in promotion.SelectableSKUList
                                                     from cartitem in shoppingCart.CartItems
                                                     where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                     select cartitem.Quantity).Sum();
                                            var freeSkuCollection = new FreeSKUCollection();
                                            if (Q > 0.0m)
                                            {
                                                freeSkuCollection.AddRange(
                                                    promotion.FreeSKUListForVolume.Select(
                                                        sku => new FreeSKU { Quantity = (int)Q, SKU = sku.SKU }));
                                            }
                                            if (v > 0)
                                            {
                                                foreach (var selectablesku in promotion.FreeSKUListForSelectableSku)
                                                {
                                                    freeSkuCollection.AddRange(
                                                        promotion.FreeSKUListForSelectableSku.Select(
                                                            sku =>
                                                                new FreeSKU
                                                                {
                                                                    Quantity = v * selectablesku.Quantity,
                                                                    SKU = sku.SKU
                                                                }));
                                                }
                                            }

                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                        }
                                        else
                                        {

                                            if (totals.VolumePoints >= temppromotion.VolumeMinInclude)
                                            {
                                                // First deleting all existing promoskus for PDM Borsch flavor Feb 2015 [蛋白营养粉（罗宋汤口味）上市促销信息(userstory :146058)
                                                var freeSkuCollection = DSPromo(promotion, cart);

                                                //   temppromotion.FreeSKUList = freeSkuCollection;
                                                //    List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(temppromotion, shoppingCart,Result);
                                                List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                    shoppingCart,
                                                    Result);
                                                shoppingCart.AddItemsToCart(itemsToAdd, true);
                                                Array.ForEach(
                                                    shoppingCart.ShoppingCartItems.TakeWhile(
                                                        x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                        .ToArray(), a => a.Flavor = FreeProductText);

                                                SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                            }
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {

                        if (promotion.Code == "DSNovPromo")
                        {
                            var itemsInBoth =
                                cart.CartItems.Where(x => x.IsPromo)
                                    .Select(c => c.SKU)
                                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));

                            if (itemsInBoth.Any())
                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                            var freeSkuCollection = new FreeSKUCollection();
                            FreeSKU freesku;
                            var count = (from selectablesku in promotion.SelectableSKUList
                                         from cartitem in shoppingCart.CartItems
                                         where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                         select cartitem.Quantity).Sum();
                            var itemsToAdd = new List<ShoppingCartItem_V01>();
                            if (count > 0)
                            {
                                foreach (var sku in promotion.FreeSKUListForSelectableSku)
                                {
                                    count = count * sku.Quantity;
                                    freesku = new FreeSKU();
                                    freesku.Quantity = count;
                                    freesku.SKU = sku.SKU;
                                    freeSkuCollection.Add(freesku);

                                }
                                itemsToAdd = getItemsToAdd(freeSkuCollection,
                                    shoppingCart,
                                    Result);
                                shoppingCart.AddItemsToCart(itemsToAdd, true);
                                Array.ForEach(
                                    shoppingCart.ShoppingCartItems.TakeWhile(
                                        x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                    a => a.Flavor = FreeProductText);
                            }

                        }
                        else
                        {
                            if (CurrentSession != null)
                            {
                                CurrentSession.ChinaPromoSKUQuantity = 0;
                            }
                            var itemsInBoth =
                                cart.CartItems.Where(x => x.IsPromo)
                                    .Select(c => c.SKU)
                                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                            if (itemsInBoth.Any())
                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "PromotionalRule - China, ProcessVolumePromotion");
            }
        }

        public FreeSKUCollection CheckDsNovPromo(PromotionElement promotion, ShoppingCart_V02 cart)
        {
            var shoppingCart = cart as MyHLShoppingCart;
            var totals = shoppingCart.Totals as OrderTotals_V02;
            //bug 225881 null reference was thrown here
            if (totals != null && cart != null && cart.CartItems != null && promotion.SelectableSKUList != null && promotion.FreeSKUListForVolume != null && promotion.FreeSKUListForSelectableSku != null)
                try
                {
                    if (totals.VolumePoints >= promotion.VolumeMinInclude)
                    {
                        var Q = totals.VolumePoints / promotion.VolumeMinInclude;
                        var v = (from selectablesku in promotion.SelectableSKUList
                                 from cartitem in shoppingCart.CartItems
                                 where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                 select cartitem.Quantity).Sum();

                        var freeSkuCollection = new FreeSKUCollection();
                        if (Q > 0.0m)
                        {
                            freeSkuCollection.AddRange(
                                promotion.FreeSKUListForVolume.Select(sku => new FreeSKU { Quantity = (int)Q, SKU = sku.SKU }));
                        }
                        if (v > 0)
                        {
                            foreach (var selectablesku in promotion.FreeSKUListForSelectableSku)
                            {
                                freeSkuCollection.AddRange(
                                    promotion.FreeSKUListForSelectableSku.Select(
                                        sku => new FreeSKU { Quantity = v * selectablesku.Quantity, SKU = sku.SKU }));
                            }

                        }
                        return freeSkuCollection;

                    }
                    return new FreeSKUCollection();
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error FreeSKUCollection CheckDsNovPromo: is cart null?:{0},is promotion null:{1},is total null{2},is cart Items Null{3}, StackTrace{4}, error message{5}, full ex{6}",
                            (shoppingCart == null) ? "null" : cart.DistributorID, (promotion == null) ? "null" : promotion.Code,
                            (totals == null) ? "null" : totals.QuoteID,
                            (shoppingCart != null && shoppingCart.CartItems == null) ? "null" : shoppingCart.CartItems.Count.ToString(), ex.StackTrace, ex.Message, ex));
                }
            return new FreeSKUCollection();
        }
        public FreeSKUCollection CheckPcPromo(PromotionElement promotion, ShoppingCart_V01 cart)
        {
            var temppromotion = (PromotionElement)promotion.Clone();
            var ShoppingCart = cart as MyHLShoppingCart;
            var Total = ShoppingCart.Totals as OrderTotals_V02;

            var Amount = (int)(Total.AmountDue - Total.OrderFreight.FreightCharge - Total.Donation);
            if (Amount >= temppromotion.AmountMinInclude)
            {
                var freeSkuCollection = new FreeSKUCollection();
                var quantity = (int)(Amount /
                                      temppromotion.AmountMinInclude);
                freeSkuCollection.AddRange(
                    promotion.FreeSKUList.Select(
                        sku => new FreeSKU { Quantity = (int)quantity, SKU = sku.SKU }));

                return freeSkuCollection;

            }
            return new FreeSKUCollection();

        }
        public FreeSKUCollection CheckDSPromo(PromotionElement promotion, ShoppingCart_V01 cart)
        {
            var temppromotion = (PromotionElement)promotion.Clone();
            var ShoppingCart = cart as MyHLShoppingCart;
            var Total = ShoppingCart.Totals as OrderTotals_V02;
            if (Total.VolumePoints >= temppromotion.VolumeMinInclude)
            {
                var quantity = (int)Total.VolumePoints / temppromotion.VolumeMinInclude;
                var freeSkuCollection = new FreeSKUCollection();
                freeSkuCollection.AddRange(
                    promotion.FreeSKUList.Select(
                        sku => new FreeSKU { Quantity = (int)quantity, SKU = sku.SKU }));
                return freeSkuCollection;
            }
            return new FreeSKUCollection();
        }
        public FreeSKUCollection DSPromo(PromotionElement promotion, ShoppingCart_V01 cart)
        {

            var temppromotion = (PromotionElement)promotion.Clone();
            var ShoppingCart = cart as MyHLShoppingCart;
            var Total = ShoppingCart.Totals as OrderTotals_V02;
            var count = (from selectablesku in promotion.SelectableSKUList
                         from cartitem in ShoppingCart.CartItems
                         where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                         select cartitem.Quantity).Sum();

            //then adding those deleted promoskus as per the logic 
            var freeSkuCollection = new FreeSKUCollection();
            FreeSKU freesku;
            foreach (var skus in promotion.SelectableSKUList)
            {
                var v = (int)Total.VolumePoints / temppromotion.VolumeMinInclude;
                var s = count / skus.Quantity;
                if (s <= 0)
                {
                    return freeSkuCollection;

                }
                var val = 0;
                val = v < s ? (int)v : (int)s;


                foreach (var sku in promotion.FreeSKUList)
                {
                    val = val * sku.Quantity;
                    freesku = new FreeSKU();
                    freesku.Quantity = val;
                    freesku.SKU = sku.SKU;
                    freeSkuCollection.Add(freesku);
                }


            }
            return freeSkuCollection;


        }

        public FreeSKUCollection QtyToAdd(PromotionElement Promotion, ShoppingCart_V01 cart)
        {
            var temppromotion = (PromotionElement)Promotion.Clone();
            var ShoppingCart = cart as MyHLShoppingCart;

            var freeSkuCollection = new FreeSKUCollection();

            var skucountquery = from selectablesku in Promotion.SelectableSKUList
                                from cartitem in ShoppingCart.CartItems
                                where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                select cartitem;
            skucountquery.GroupBy(item => item.SKU).ToList().ForEach(skulist =>
            {
                var skucount = skulist.Sum(item => item.Quantity);
                if (skucount > 0)
                {
                    var quantity = 1;
                    if (Promotion.SelectableSKUList.Count > 0)
                    {
                        var tablesku = Promotion.SelectableSKUList.First(f => f.SKU == skulist.Key);
                        if (tablesku.Quantity != quantity)
                        {
                            quantity = tablesku.Quantity;
                        }
                    }
                    foreach (var sku in Promotion.FreeSKUList)
                    {
                        double tempcount = skucount / quantity;
                        var count = (int)Math.Floor(tempcount);
                        var freesku = new FreeSKU();
                        freesku.Quantity = count * sku.Quantity;
                        freesku.SKU = sku.SKU;
                        freeSkuCollection.Add(freesku);
                    }
                }

            });
            return freeSkuCollection;
        }

        public FreeSKUCollection PCPromo(PromotionElement promotion, ShoppingCart_V01 cart)
        {
            var temppromotion = (PromotionElement)promotion.Clone();
            var ShoppingCart = cart as MyHLShoppingCart;
            var Total = ShoppingCart.Totals as OrderTotals_V02;


            var count = (from selectablesku in promotion.SelectableSKUList
                         from cartitem in ShoppingCart.CartItems
                         where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                         select cartitem.Quantity).Sum();
            //then adding those deleted promoskus as per the logic 
            var freeSkuCollection = new FreeSKUCollection();
            FreeSKU freesku;
            foreach (var skus in promotion.SelectableSKUList)
            {
                var v =
                    (int)
                        (Total.AmountDue - Total.OrderFreight.FreightCharge -
                         Total.Donation) /
                    temppromotion.AmountMinInclude;
                var s = count / skus.Quantity;
                if (s <= 0)
                {
                    return freeSkuCollection;

                }
                var val = 0;
                val = v < s ? (int)v : (int)s;


                foreach (var sku in promotion.FreeSKUList)
                {
                    val = val * sku.Quantity;
                    freesku = new FreeSKU();
                    freesku.Quantity = val;
                    freesku.SKU = sku.SKU;
                    freeSkuCollection.Add(freesku);
                }

            }
            return freeSkuCollection;
        }

        public void processSpecialPromotion(PromotionElement promotion, ShoppingCart_V02 cart, ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            try
            {
                #region commented Old Special Promo 
                //var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");
                //object message;
                ////var tempDistributorId = cart.DistributorID;
                ////if (CurrentSession.IsReplacedPcOrder && CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                ////{
                ////    tempDistributorId = CurrentSession.ReplacedPcDistributorOrderingProfile.Id;
                ////}
                //if (reason == ShoppingCartRuleReason.CartCreated)
                //{
                //    DateTime startDateTime = convertDateTime(promotion.StartDate);
                //    DateTime endDateTime = convertDateTime(promotion.EndDate);
                //    var isEligible =
                //        ChinaPromotionProvider.IsEligible(distributorOrderingProfile.CNCustomorProfileID.ToString(),
                //            startDateTime, endDateTime,
                //            promotion.FreeSKUList.Select(f => f.SKU).ToList()
                //            , promotion.AmountMinInclude, promotion.NumOfMonth, promotion.OnlineOrderOnly);

                //    var shoppingCart = cart as MyHLShoppingCart;
                //    Array.ForEach(promotion.SelectableSKUList.ToArray(),
                //        a =>
                //            validateSKU(a.SKU.Trim(),
                //                shoppingCart.DeliveryInfo == null ? "300" : shoppingCart.DeliveryInfo.WarehouseCode,shoppingCart,Result));
                //    if (!isEligible.IsEligible)
                //    {

                //        if (shoppingCart != null && shoppingCart.Totals != null)
                //        {
                //            PromotionInformation pInfo =
                //                ChinaPromotionProvider.GetPCPromotion(
                //                    distributorOrderingProfile.CNCustomorProfileID.ToString(),
                //                    shoppingCart.DistributorID);
                //            if (pInfo != null && !pInfo.GotPromoSKU)
                //            {
                //                //  decimal currentMonthAmount = ChinaPromotionProvider.GetCurrentMonthAmount(pInfo.MonthlyInfo);
                //                int numOfMonthOver = ChinaPromotionProvider.GetNumberMonthQualified(pInfo.MonthlyInfo,
                //                    (shoppingCart.Totals as OrderTotals_V01).AmountDue, promotion.AmountMinInclude);
                //                if (numOfMonthOver >= promotion.NumOfMonth
                //                    &&
                //                    ((shoppingCart.Totals as OrderTotals_V01).AmountDue) >= promotion.AmountMinInclude)
                //                {
                //                    ChinaPromotionProvider.SetEligible(shoppingCart.DistributorID, true);
                //                }
                //            }
                //            var itemsInBoth =
                //                cart.CartItems.Where(x => x.IsPromo)
                //                    .Select(c => c.SKU)
                //                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                //            // what are in cart are free sku
                //            if (itemsInBoth.Count() > 0)
                //            {
                //                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                //            }
                //        }
                //    }
                //}
                //else if (reason == ShoppingCartRuleReason.CartItemsAdded)
                //{
                //    //if (CurrentSession.IsReplacedPcOrder && CurrentSession.ReplacedPcDistributorOrderingProfile != null && CurrentSession.ReplacedPcDistributorOrderingProfile.IsPC)
                //    //{
                //    //    DateTime startDateTime = convertDateTime(promotion.StartDate);
                //    //    ChinaPromotionProvider.IsEligible(tempDistributorId, startDateTime,
                //    //                                      promotion.FreeSKUList.Select(f => f.SKU).ToList()
                //    //                                      , promotion.AmountMinInclude, promotion.NumOfMonth, promotion.OnlineOrderOnly);
                //    //}


                //    var shoppingCart = cart as MyHLShoppingCart;
                //    if (shoppingCart != null && shoppingCart.Totals != null)
                //    {

                //        PromotionInformation pInfo =
                //            ChinaPromotionProvider.GetPCPromotion(
                //                distributorOrderingProfile.CNCustomorProfileID.ToString(), shoppingCart.DistributorID);
                //        if (pInfo == null)
                //            return;
                //        decimal currentMonthAmount = ChinaPromotionProvider.GetCurrentMonthAmount(pInfo.MonthlyInfo);
                //        if (!ChinaPromotionProvider.IsEligible(shoppingCart.DistributorID))
                //        {
                //            if (promotion.Code.Equals("PCPromoSurvey"))
                //            {
                //                if (((shoppingCart.Totals as OrderTotals_V01).ProductRetailAmount) >=
                //                    promotion.AmountMinInclude)
                //                {
                //                    if (promotion.FreeSKUList != null &&
                //                        !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                //                    {
                //                        if (shoppingCart.CartItems.Count > 0)
                //                        {
                //                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                //                                shoppingCart, Result);
                //                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                //                            Array.ForEach(
                //                                shoppingCart.ShoppingCartItems.TakeWhile(
                //                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                //                                a => a.Flavor = FreeProductText);
                //                            SetPromotionalRuleResponses_V02(Result, promotion.Code,
                //                                TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                //                        }
                //                    }
                //                }
                //                else if (((shoppingCart.Totals as OrderTotals_V01).ProductRetailAmount) <
                //                         promotion.AmountMinInclude)
                //                {
                //                    var itemsInBoth =
                //                        cart.CartItems.Where(x => x.IsPromo)
                //                            .Select(c => c.SKU)
                //                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                //                    // what are in cart are free sku
                //                    if (itemsInBoth.Count() > 0)
                //                    {
                //                        message = HttpContext.GetGlobalResourceObject(
                //                            string.Format("{0}_Rules", HLConfigManager.Platform),
                //                            "PromotionalSKURemoved") ?? "{0}{1}{2}";
                //                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                //                        foreach (var item in itemsInBoth)
                //                        {
                //                            message = string.Format(message.ToString(), item,
                //                                promotion.AmountMinInclude, "RMB");
                //                            Result.Result = RulesResult.Failure;
                //                        Result.AddMessage(message.ToString());
                //                        }
                //                    }
                //                    cart.RuleResults.Add(Result);
                //                }


                //            }

                //            else if (!pInfo.GotPromoSKU &&
                //                     ChinaPromotionProvider.GetNumberMonthQualified(pInfo.MonthlyInfo,
                //                         (shoppingCart.Totals as OrderTotals_V01).AmountDue, promotion.AmountMinInclude) >=
                //                     promotion.NumOfMonth
                //                     &&
                //                     ((shoppingCart.Totals as OrderTotals_V01).AmountDue) >= promotion.AmountMinInclude)
                //            {
                //                ChinaPromotionProvider.SetEligible(shoppingCart.DistributorID, true);
                //                message = HttpContext.GetGlobalResourceObject(
                //                    string.Format("{0}_Rules", HLConfigManager.Platform), "EligibleForPromotionalSKU") ??
                //                          "{0}{1}";
                //                message = string.Format(message.ToString(), promotion.AmountMinInclude, "RMB");
                //                Result.Result = RulesResult.Failure;
                //            Result.AddMessage(message.ToString());
                //                cart.RuleResults.Add(Result);
                //            }



                //        }
                //        else
                //        {
                //            if (
                //                ChinaPromotionProvider.GetNumberMonthQualified(pInfo.MonthlyInfo,
                //                    (shoppingCart.Totals as OrderTotals_V01).AmountDue, promotion.AmountMinInclude) <
                //                promotion.NumOfMonth
                //                || ((shoppingCart.Totals as OrderTotals_V01).AmountDue) < promotion.AmountMinInclude)
                //            {
                //                ChinaPromotionProvider.SetEligible(shoppingCart.DistributorID, false);
                //                var itemsInBoth =
                //                    cart.CartItems.Where(x => x.IsPromo)
                //                        .Select(c => c.SKU)
                //                        .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                //                // what are in cart are free sku
                //                if (itemsInBoth.Count() > 0)
                //                {
                //                    message = HttpContext.GetGlobalResourceObject(
                //                        string.Format("{0}_Rules", HLConfigManager.Platform), "PromotionalSKURemoved") ??
                //                              "{0}{1}{2}";
                //                    shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                //                    foreach (var item in itemsInBoth)
                //                    {
                //                        message = string.Format(message.ToString(), item, promotion.AmountMinInclude,
                //                            "RMB");
                //                        Result.Result = RulesResult.Failure;
                //                    Result.AddMessage(message.ToString());
                //                    }
                //                }
                //                cart.RuleResults.Add(Result);
                //            }
                //            else if (((shoppingCart.Totals as OrderTotals_V01).ProductRetailAmount) >=
                //                     promotion.AmountMinInclude && promotion.Code.Equals("PCPromoSurvey"))
                //            {
                //                if (promotion.FreeSKUList != null &&
                //                    !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                //                {
                //                    if (shoppingCart.CartItems.Count > 0)
                //                    {
                //                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                //                            shoppingCart, Result);
                //                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                //                        Array.ForEach(
                //                            shoppingCart.ShoppingCartItems.TakeWhile(
                //                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                //                            a => a.Flavor = FreeProductText);
                //                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                //                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                //                    }
                //                }
                //            }

                //        }
                //    }
                //}
                //else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                //{
                //    if (ChinaPromotionProvider.IsEligible(cart.DistributorID))
                //    {
                //        var shoppingCart = cart as MyHLShoppingCart;
                //        if (shoppingCart.Totals != null)
                //        {
                //            PromotionInformation pInfo =
                //                ChinaPromotionProvider.GetPCPromotion(
                //                    distributorOrderingProfile.CNCustomorProfileID.ToString(),
                //                    shoppingCart.DistributorID);
                //            if (pInfo != null)
                //            {
                //                decimal currentMonthAmount =
                //                    ChinaPromotionProvider.GetCurrentMonthAmount(pInfo.MonthlyInfo);
                //                int numOfMonthOver = ChinaPromotionProvider.GetNumberMonthQualified(pInfo.MonthlyInfo,
                //                    (shoppingCart.Totals as OrderTotals_V01).AmountDue, promotion.AmountMinInclude);
                //                if (numOfMonthOver < promotion.NumOfMonth ||
                //                    ((shoppingCart.Totals as OrderTotals_V01).AmountDue) < promotion.AmountMinInclude)
                //                {
                //                    ChinaPromotionProvider.SetEligible(shoppingCart.DistributorID, false);
                //                    var itemsInBoth =
                //                        cart.CartItems.Where(x => x.IsPromo)
                //                            .Select(c => c.SKU)
                //                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                //                    // what are in cart are free sku
                //                    if (itemsInBoth.Count() > 0)
                //                    {
                //                        message = HttpContext.GetGlobalResourceObject(
                //                            string.Format("{0}_Rules", HLConfigManager.Platform),
                //                            "PromotionalSKURemoved") ?? "{0}{1}{2}";
                //                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                //                        foreach (var item in itemsInBoth)
                //                        {
                //                            message = string.Format(message.ToString(), item, promotion.AmountMinInclude,
                //                                "RMB");
                //                            Result.Result = RulesResult.Failure;
                //                        Result.AddMessage(message.ToString());
                //                        }
                //                    }
                //                    Result.Result = RulesResult.Failure;
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        var shoppingCart = cart as MyHLShoppingCart;
                //        if (shoppingCart.Totals != null)
                //        {
                //            PromotionInformation pInfo =
                //                ChinaPromotionProvider.GetPCPromotion(
                //                    distributorOrderingProfile.CNCustomorProfileID.ToString(),
                //                    shoppingCart.DistributorID);
                //            if (pInfo != null)
                //            {
                //                if (((shoppingCart.Totals as OrderTotals_V01).ProductRetailAmount) <
                //                    promotion.AmountMinInclude && promotion.Code.Equals("PCPromoSurvey"))
                //                {
                //                    ChinaPromotionProvider.SetEligible(shoppingCart.DistributorID, false);
                //                    var itemsInBoth =
                //                        cart.CartItems.Where(x => x.IsPromo)
                //                            .Select(c => c.SKU)
                //                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                //                    // what are in cart are free sku
                //                    if (itemsInBoth.Count() > 0)
                //                    {
                //                        message = HttpContext.GetGlobalResourceObject(
                //                            string.Format("{0}_Rules", HLConfigManager.Platform),
                //                            "PromotionalSKURemoved") ?? "{0}{1}{2}";
                //                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                //                        foreach (var item in itemsInBoth)
                //                        {
                //                            message = string.Format(message.ToString(), item, promotion.AmountMinInclude,
                //                                "RMB");
                //                            Result.Result = RulesResult.Failure;
                //                        Result.AddMessage(message.ToString());
                //                        }
                //                    }
                //                    Result.Result = RulesResult.Failure;
                //                }
                //            }
                //        }
                //    }

                //}
                //else if (reason == ShoppingCartRuleReason.CartClosed)
                //{
                //    ClosePromo(promotion, cart as MyHLShoppingCart, Result);
                //    ChinaPromotionProvider.SetEligible(cart.DistributorID, false);
                //    ChinaPromotionProvider.ReloadPCPromotion(cart.DistributorID);
                //    processSpecialPromotion(promotion, cart, ShoppingCartRuleReason.CartCreated,
                //        new ShoppingCartRuleResult());
                //}
                #endregion
                if (reason == ShoppingCartRuleReason.CartCreated || reason == ShoppingCartRuleReason.CartItemsAdded)
                {
                    var shoppingcart = cart as MyHLShoppingCart;
                    var freeSkuCollection = new FreeSKUCollection();
                    if (promotion.Code == "SRGrowingPromo" && !CurrentSession.IsReplacedPcOrder)
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            if (shoppingcart.CartItems.Count > 0)
                            {

                                var IsEligibleForSRQGrowingPromotion = ChinaPromotionProvider.GetSRQGrowingPromoFromService(shoppingcart, HLConfigManager.Platform);
                                if (IsEligibleForSRQGrowingPromotion != null && IsEligibleForSRQGrowingPromotion.Skus != null && IsEligibleForSRQGrowingPromotion.PromotionCode != null && IsEligibleForSRQGrowingPromotion.Quantity > 0)
                                {
                                    var itemsInBoth =
                                    shoppingcart.CartItems.Where(x => x.IsPromo)
                                      .Select(c => c.SKU)
                                       .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingcart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                    if (promotion.FreeSKUList.Any(f => f.SKU == IsEligibleForSRQGrowingPromotion.Skus))
                                    {
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                       shoppingcart, Result);
                                        shoppingcart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                                       shoppingcart.ShoppingCartItems.TakeWhile(
                                                           x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                       a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }
                                }
                            }



                        }
                    }
                    else if (promotion.Code == "SRExcellentPromo" && !CurrentSession.IsReplacedPcOrder)
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            if (shoppingcart.CartItems.Count > 0)
                            {

                                var GetSRQExcellentPromoFromService = ChinaPromotionProvider.GetSRQExcellentPromoFromService(shoppingcart, HLConfigManager.Platform);
                                if (GetSRQExcellentPromoFromService != null && GetSRQExcellentPromoFromService.Skus != null && GetSRQExcellentPromoFromService.PromotionCode != null && GetSRQExcellentPromoFromService.Quantity > 0)
                                {
                                    var itemsInBoth =
                                      shoppingcart.CartItems.Where(x => x.IsPromo)
                                          .Select(c => c.SKU)
                                          .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingcart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                    if (promotion.FreeSKUList.Any(f => f.SKU == GetSRQExcellentPromoFromService.Skus))
                                    {
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                       shoppingcart, Result);
                                        shoppingcart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                                       shoppingcart.ShoppingCartItems.TakeWhile(
                                                           x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                       a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }



                                }
                            }
                        }
                    }
                    else if (promotion.Code == "MonthlyFirstList")
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            var memberId = String.IsNullOrEmpty(shoppingcart.SrPlacingForPcOriginalMemberId) ?
                                  shoppingcart.DistributorID :
                                  shoppingcart.SrPlacingForPcOriginalMemberId;
                             DeleteNotBrochureSKUInCart(memberId, shoppingcart, promotion);
                            if (ChinaPromotionProvider.IsEligibleForBrochurePromotion(shoppingcart, HLConfigManager.Platform, memberId))
                            {
                                shoppingcart.HasBrochurePromotion = true;
                                if (shoppingcart.CartItems.Count > 0)
                                {
                                    List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                     shoppingcart, Result);
                                    SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                    shoppingcart.AddItemsToCart(itemsToAdd, true);
                                    cart.RuleResults.Add(Result);

                                }
                            }
                            else
                            {
                                shoppingcart.HasBrochurePromotion = false;
                            }

                        }
                    }

                    else if (promotion.Code == "ChinaBadgePromo")
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            if (shoppingcart.CartItems.Count > 0)
                            {
                                var memberId = String.IsNullOrEmpty(shoppingcart.SrPlacingForPcOriginalMemberId) ?
                                    shoppingcart.DistributorID :
                                    shoppingcart.SrPlacingForPcOriginalMemberId;

                                if (ChinaPromotionProvider.IsEligibleForBadgePromotion(shoppingcart, HLConfigManager.Platform, memberId))
                                {
                                    var message = HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "EligibleForChinaPromotionalSKU");
                                    Result.Result = RulesResult.Failure;
                                    Result.AddMessage(message.ToString());
                                    SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                        "EligibleForChinaPromotionalSKU@1");
                                    cart.RuleResults.Add(Result);

                                    //DeleteNotMyBadgeInCart(memberId, shoppingcart, promotion);
                                }

                                DeleteNotMyBadgeInCart(memberId, shoppingcart, promotion);
                            }
                        }
                    }
                    else if(promotion.Code == PromotionCode.NewSrPromotion)
                    {
                        if (promotion.FreeSKUList != null &&
                            promotion.FreeSKUList.Any() &&
                            shoppingcart != null &&
                            shoppingcart.CartItems != null &&
                            shoppingcart.CartItems.Count > 0 &&
                            !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            GetNewSRPromotionResponse_V01 myNewSrPromotion = null;
                            var bothItems = from item in shoppingcart.CartItems
                                                    join freelist in promotion.FreeSKUList
                                                    on item.SKU equals freelist.SKU into bothTable
                                                    from bothItem in bothTable
                                                    select item.SKU;
                            //if (bothItems.Any())
                            //    shoppingcart.DeleteItemsFromCart(bothItems.ToList(), true);
                            var isplacing = !String.IsNullOrWhiteSpace(shoppingcart.SrPlacingForPcOriginalMemberId) || CurrentSession.IsReplacedPcOrder;
                            if (ChinaPromotionProvider.IsEligibleForNewSRPromotion(shoppingcart, HLConfigManager.Platform) &&
                                !isplacing)
                            {
                                var cacheKey = String.Format("GetNewSRPromotionDetail_{0}", shoppingcart.DistributorID);
                                myNewSrPromotion = HttpRuntime.Cache[cacheKey] as GetNewSRPromotionResponse_V01;
                                if (myNewSrPromotion != null && !String.IsNullOrWhiteSpace(myNewSrPromotion.Skus))
                                {
                                    var myNewSrPromoSkus = myNewSrPromotion.Skus.Split(',');
                                    var addItemSkus = myNewSrPromoSkus.Except(bothItems);
                                    if (addItemSkus.Any())
                                    {
                                        var addItems = getItemsToAdd(promotion, shoppingcart, Result);
                                        if (addItems.Any())
                                            shoppingcart.AddItemsToCart(addItems, true);
                                        Array.ForEach(shoppingcart.ShoppingCartItems.TakeWhile(
                                                               x => addItems.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                           a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, addItems);
                                    }
                                }
                            }

                            DeleteNotMyNewSrPromotionInCart(shoppingcart, promotion, myNewSrPromotion, bothItems);
                        }
                    }
                    else
                    {
                        var itemsInBoth =
                                      shoppingcart.CartItems.Where(x => x.IsPromo)
                                          .Select(c => c.SKU)
                                          .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                        if (itemsInBoth.Any())
                            shoppingcart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                    }
                }
                if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                {

                    var shoppingcart = cart as MyHLShoppingCart;
                    var freeSkuCollection = new FreeSKUCollection();
                    if (promotion.Code == "SRGrowingPromo" && !CurrentSession.IsReplacedPcOrder)
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            if (shoppingcart.CartItems.Count > 0)
                            {
                                var itemsInBoth =
                           shoppingcart.CartItems.Where(x => x.IsPromo)
                               .Select(c => c.SKU)
                               .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                if (itemsInBoth.Any())
                                    shoppingcart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                var IsEligibleForSRQGrowingPromotion = ChinaPromotionProvider.GetSRQGrowingPromoFromService(shoppingcart, HLConfigManager.Platform);
                                if (IsEligibleForSRQGrowingPromotion != null && IsEligibleForSRQGrowingPromotion.Skus != null && IsEligibleForSRQGrowingPromotion.PromotionCode != null && IsEligibleForSRQGrowingPromotion.Quantity > 0)
                                {

                                    if (promotion.FreeSKUList.Any(f => f.SKU == IsEligibleForSRQGrowingPromotion.Skus))
                                    {
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                       shoppingcart, Result);
                                        shoppingcart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                                       shoppingcart.ShoppingCartItems.TakeWhile(
                                                           x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                       a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }
                                }
                            }



                        }
                    }
                    else if (promotion.Code == "SRExcellentPromo" && !CurrentSession.IsReplacedPcOrder)
                    {
                        if (promotion.FreeSKUList != null && !APFDueProvider.hasOnlyAPFSku(shoppingcart.CartItems, Locale))
                        {
                            if (shoppingcart.CartItems.Count > 0)
                            {
                                var itemsInBoth =
                                       shoppingcart.CartItems.Where(x => x.IsPromo)
                                           .Select(c => c.SKU)
                                           .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                if (itemsInBoth.Any())
                                    shoppingcart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                var GetSRQExcellentPromoFromService = ChinaPromotionProvider.GetSRQExcellentPromoFromService(shoppingcart, HLConfigManager.Platform);
                                if (GetSRQExcellentPromoFromService != null && GetSRQExcellentPromoFromService.Skus != null && GetSRQExcellentPromoFromService.PromotionCode != null && GetSRQExcellentPromoFromService.Quantity > 0)
                                {


                                    if (promotion.FreeSKUList.Any(f => f.SKU == GetSRQExcellentPromoFromService.Skus))
                                    {
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion,
                                                       shoppingcart, Result);
                                        shoppingcart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                                       shoppingcart.ShoppingCartItems.TakeWhile(
                                                           x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                       a => a.Flavor = FreeProductText);
                                        SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                            TypeOfPromotion.Special, PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }



                                }
                            }
                        }
                    }else if (promotion.Code == "MonthlyFirstList")
                    {
                        var memberId = String.IsNullOrEmpty(shoppingcart.SrPlacingForPcOriginalMemberId) ?
                                shoppingcart.DistributorID :
                                shoppingcart.SrPlacingForPcOriginalMemberId;
                        DeleteNotBrochureSKUInCart(memberId, shoppingcart, promotion);
                   
                    }
                    else if(promotion.Code == PromotionCode.NewSrPromotion)
                    {
                        ClearNewSrPromotionIfNotProduct(promotion, shoppingcart);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error orrcured in China processSpecialPromotion" +
                                                               "");
            }
        }


        private void DeleteNotBrochureSKUInCart(string memberId, MyHLShoppingCart shoppingcart, PromotionElement promotion)
        {
            if (promotion != null && promotion.FreeSKUList.Count > 0)
            {
                var removelist = from cartitems in shoppingcart.CartItems
                                 from bsku in promotion.FreeSKUList
                                 where cartitems.SKU == bsku.SKU
                                 select cartitems.SKU;
                if (removelist.Any())
                    shoppingcart.DeleteItemsFromCart(removelist.ToList(), true);
                shoppingcart.HasBrochurePromotion = false;
            }
        }

        private void DeleteNotMyNewSrPromotionInCart(MyHLShoppingCart shoppingcart, PromotionElement promotion, GetNewSRPromotionResponse_V01 myNewSrPromo, IEnumerable<string> newSrPromosInCartSkus)
        {
            
            if (!CurrentSession.IsReplacedPcOrder && myNewSrPromo != null)
            {
                var myNewSrPromoSkus = myNewSrPromo.Skus.Split(',');
                var notMyPromoSkus = newSrPromosInCartSkus.Except(myNewSrPromoSkus);
                if (notMyPromoSkus.Any())
                    shoppingcart.DeleteItemsFromCart(notMyPromoSkus.ToList(), true);
            }
            else
            {
                shoppingcart.DeleteItemsFromCart(newSrPromosInCartSkus.ToList(), true);
            }
        }

        private void ClearNewSrPromotionIfNotProduct(PromotionElement promotion, MyHLShoppingCart shoppingcart)
        {
            if (promotion.FreeSKUList != null && shoppingcart.CartItems != null && shoppingcart.CartItems.Count > 0)
            {
                var others = shoppingcart.CartItems
                    .Where(s => !s.IsPromo)
                    .Select(c => c.SKU).Except(APFDueProvider.GetAPFSkuList());

                if (!others.Any())
                {
                    var removeList = promotion.FreeSKUList.Select(s => s.SKU).ToList();
                    shoppingcart.DeleteItemsFromCart(removeList, true);
                }
            }
        }
        /// <summary>
        /// 1.change ds to pc, clean all badge promo list
        /// 2.change pc to ds, clean all badge promo list
        /// 3.clean all badge promo, because every one has different badge and quantity
        /// </summary>
        /// <param name="shoppingcart"></param>
        private void DeleteNotMyBadgeInCart(string memberId, MyHLShoppingCart shoppingcart, PromotionElement promotion)
        {
            var cacheKey = string.Format("GetBadgePromoDetail_{0}", memberId);
            var myBadgeResponse = HttpRuntime.Cache[cacheKey] as GetBadgePinResponse_V01;

            var badgesInCart = from cartitems in shoppingcart.CartItems
                               from skulist in promotion.FreeSKUList
                               where cartitems.SKU == skulist.SKU
                               select cartitems;

            if (myBadgeResponse != null && myBadgeResponse.BadgeDetails != null && myBadgeResponse.BadgeDetails.Length > 0)
            {
                var mybadges = myBadgeResponse.BadgeDetails.Select(s => new
                {
                    Sku = s.BadgeCode,
                    Quantity = s.Quantity
                });

                var removelist = from cartitem in badgesInCart
                                 join mybadge in mybadges
                                 on cartitem.SKU equals mybadge.Sku
                                 into intersect
                                 from item in intersect.DefaultIfEmpty()
                                 where item == null || cartitem.Quantity != item.Quantity
                                 select cartitem.SKU;

                if(removelist.Any())
                    shoppingcart.DeleteItemsFromCart(removelist.ToList(), true);
            }
            else
            {
                var removelist = badgesInCart.Select(s => s.SKU).ToList();
                if(removelist.Any())
                    shoppingcart.DeleteItemsFromCart(removelist, true);
            }
        }

        private void ClosePromo(PromotionElement promotion, MyHLShoppingCart shoppingCart, ShoppingCartRuleResult result)
        {
            var tempDistributorId = shoppingCart.DistributorID;
            //if (CurrentSession.IsReplacedPcOrder && CurrentSession.ReplacedPcDistributorOrderingProfile != null)
            //{
            //    tempDistributorId = CurrentSession.ReplacedPcDistributorOrderingProfile.Id;
            //}
            foreach (var pSKU in promotion.FreeSKUList)
            {
                if (!shoppingCart.CartItems.Any(c => c.SKU == pSKU.SKU && c.IsPromo))
                    continue;
                EligibleDistributorInfo_V01 info = new EligibleDistributorInfo_V01();
                info.DistributorId = tempDistributorId;
                info.Sku = pSKU.SKU;
                info.Quantity = 1;
                info.IsDisable = false;
                info.Locale = "zh-CN";

                if (ChinaPromotionProvider.SaveEligibleForPromoToFromService(info, shoppingCart.ShoppingCartID,
                                                                       shoppingCart.OrderNumber))
                {
                    var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(shoppingCart.DistributorID, "CN");
                    PromotionInformation pInfo = ChinaPromotionProvider.GetPCPromotion(distributorOrderingProfile.CNCustomorProfileID.ToString(), shoppingCart.DistributorID);
                    if (pInfo != null)
                    {
                        pInfo.GotPromoSKU = true;
                    }
                }

            }
        }

        private DateTime convertDateTime(string datetime)
        {
            const string format = "MM-dd-yyyy";
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime dt;
                if (DateTime.TryParseExact(datetime, format, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out dt))
                {
                    return dt;
                }
            }
            return DateTime.MaxValue;
        }

        private object PromotionCollectionCacheMedia
        {
            // In web-api mode, HttpContext.Current.Session will be null, so use cache instead
            get
            {
                if (HttpContext.Current != null && HttpContext.Current.Session == null)
                    return HttpContext.Current.Cache[sessionKey];

                return HttpContext.Current.Session[sessionKey];
            }
            set
            {
                if (HttpContext.Current != null && HttpContext.Current.Session == null)
                {
                    var chk = HttpContext.Current.Cache[sessionKey];
                    if (chk != null)
                    {
                        HttpContext.Current.Cache[sessionKey] = value;
                        return;
                    }

                    int afterMinutes = 60;

                    var cm = HL.Common.Configuration.Settings.GetRequiredAppSetting("PromotionsCacheReloadAfterMinutes_Mobile");
                    if (cm != null) int.TryParse(cm, out afterMinutes);

                    DateTime absoluteExpiration = DateTime.Now.AddMinutes(afterMinutes);

                    HttpContext.Current.Cache.Add(sessionKey, value, null
                        , absoluteExpiration
                        , System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Normal, null);

                    return;
                }

                HttpContext.Current.Session[sessionKey] = value;
            }
        }

        public const string sessionKey = "PromotionInfo";

        private PromotionCollection loadPromotion()
        {
            if (HL.Common.Configuration.Settings.GetRequiredAppSetting("LoadPromotionsFromDb") == "1")
            {
                var DBinfo = PromotionCollectionCacheMedia as PromotionCollection;
                if (DBinfo == null)
                {
                    return loadPromotionFromDb();

                }

            }
            else
            {
                #region load from config file - old implementation

                var info = PromotionCollectionCacheMedia as PromotionCollection;
                if (info == null)
                {
                    info = PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform, "zh-CN");
                    PromotionCollectionCacheMedia = info;
                }
                return GetPromotion();
            }

            return PromotionCollectionCacheMedia as PromotionCollection;
            #endregion


        }
        private PromotionCollection GetPromotion()
        {
            var promotion = PromotionCollectionCacheMedia as PromotionCollection;

            PromotionCollection col = new PromotionCollection();


            if (promotion != null)
            {
                DateTime currentDateTime = DateUtils.GetCurrentLocalTime("CN").Date;

                foreach (var promoItem in promotion)
                {
                    DateTime startDateTime = convertDateTime(promoItem.StartDate);
                    DateTime endDateTime = convertDateTime(promoItem.EndDate);
                    if (startDateTime != DateTime.MaxValue && endDateTime != DateTime.MaxValue)
                    {
                        if (startDateTime <= currentDateTime && endDateTime >= currentDateTime)
                        {
                            col.Add(promoItem);
                        }
                    }


                }


            }
            return col;

        }

        public PromotionCollection loadPromotionFromDb()
        {
            var promotion = MyHerbalife3.Ordering.Providers.ChinaPromotionProvider.GetEffectivePromotionList(Locale, DateTime.Now);
            string config = promotion.Promotion_Info;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(config);
            XmlNodeList publicationsNodeList = xmlDoc.SelectNodes("Promotion");
            XmlNodeList child = publicationsNodeList[0].SelectNodes("Promo");

            PromotionCollection ret = new PromotionCollection();
            foreach (XmlElement y in child)
            {
                PromotionElement pe = new PromotionElement();
                if (y.Attributes["code"] != null) pe.Code = y.Attributes["code"].Value;
                if (y.Attributes["startDate"] != null) pe.StartDate = y.Attributes["startDate"].Value;
                if (y.Attributes["endDate"] != null) pe.EndDate = y.Attributes["endDate"].Value;
                if (y.Attributes["promotionType"] != null) pe.PromotionType = ConvertToPromotionType(y.Attributes["promotionType"].Value);
                if (y.Attributes["custTypeList"] != null) pe.CustTypeList = (y.Attributes["custTypeList"].Value.Split(',').ToList());
                if (y.Attributes["custCategoryTypeList"] != null) pe.CustCategoryTypeList = (y.Attributes["custCategoryTypeList"].Value.Split(',').ToList());
                if (y.Attributes["amountMinInclude"] != null) pe.AmountMinInclude = Convert.ToDecimal(y.Attributes["amountMinInclude"].Value);
                if (y.Attributes["deliveryTypeList"] != null) pe.DeliveryTypeList = (y.Attributes["deliveryTypeList"].Value.Split(',').ToList());
                if (y.Attributes["excludedExpID"] != null) pe.excludedExpID = (y.Attributes["excludedExpID"].Value.Split(',').ToList());
                if (y.Attributes["hasIncrementaldegree"] != null) pe.HasIncrementaldegree = Convert.ToBoolean(y.Attributes["hasIncrementaldegree"].Value);
                if (y.Attributes["volumeMinInclude"] != null) pe.VolumeMinInclude = Convert.ToDecimal(y.Attributes["volumeMinInclude"].Value);
                if (y.Attributes["freeSKUList"] != null) pe.FreeSKUList = ConvertToFreeSKUCollection1(y.Attributes["freeSKUList"].Value);
                if (y.Attributes["selectableSKUList"] != null) pe.SelectableSKUList = ConvertToFreeSKUCollection1(y.Attributes["selectableSKUList"].Value);
                if (y.Attributes["freeSKUListForSelectableSku"] != null) pe.FreeSKUListForSelectableSku = ConvertToFreeSKUCollection1(y.Attributes["freeSKUListForSelectableSku"].Value);
                if (y.Attributes["onlineOrderOnly"] != null) pe.OnlineOrderOnly = Convert.ToBoolean(y.Attributes["onlineOrderOnly"].Value);
                if (y.Attributes["maxFreight"] != null) pe.MaxFreight = Convert.ToDecimal(y.Attributes["maxFreight"].Value);
                if (y.Attributes["dsStoreProvince"] != null) pe.DSStoreProvince = (y.Attributes["dsStoreProvince"].Value.Split(',').ToList());
                if (y.Attributes["shippedToProvince"] != null) pe.ShippedToProvince = (y.Attributes["shippedToProvince"].Value.Split(',').ToList());
                if (y.Attributes["amountMaxInclude"] != null) pe.AmountMaxInclude = Convert.ToDecimal(y.Attributes["amountMinInclude"].Value);
                if (y.Attributes["volumeMaxInclude"] != null) pe.VolumeMaxInclude = Convert.ToDecimal(y.Attributes["volumeMaxInclude"].Value);
                if (y.Attributes["amountMax"] != null) pe.AmountMax = Convert.ToDecimal(y.Attributes["amountMax"].Value);
                if (y.Attributes["amountMin"] != null) pe.AmountMin = Convert.ToDecimal(y.Attributes["amountMin"].Value);
                if (y.Attributes["volumeMax"] != null) pe.VolumeMax = Convert.ToDecimal(y.Attributes["volumeMax"].Value);
                if (y.Attributes["volumeMin"] != null) pe.VolumeMin = Convert.ToDecimal(y.Attributes["volumeMin"].Value);
                if (y.Attributes["YearlyPromo"] != null) pe.YearlyPromo = Convert.ToBoolean(y.Attributes["YearlyPromo"].Value);

                ret.Add(pe);
                PromotionCollectionCacheMedia = ret;
            }
            return ret;
        }



        //foreach (var ep in rslt)
        //{
        //    PromotionElement pe = new PromotionElement();

        //    if (ep.AmountMax != null) pe.AmountMax = ep.AmountMax.Value;
        //    if (ep.AmountMaxInclude != null) pe.AmountMaxInclude = ep.AmountMaxInclude.Value;
        //    if (ep.AmountMin != null) pe.AmountMin = ep.AmountMin.Value;
        //    if (ep.AmountMinInclude != null) pe.AmountMinInclude = ep.AmountMinInclude.Value;
        //    pe.Code = ep.Code;
        //    if (ep.CustTypeList != null) pe.CustTypeList = ep.CustTypeList;
        //    if (ep.CustCategoryTypeList != null) pe.CustCategoryTypeList = ep.CustCategoryTypeList;
        //    if (ep.DeliveryTypeList != null) pe.DeliveryTypeList = ep.DeliveryTypeList;
        //    if (ep.DSStoreProvince != null) pe.DSStoreProvince = ep.DSStoreProvince;
        //    pe.EndDate = ep.EndDate.ToString(PromotionDateFormat_V00);
        //    if (ep.FreeSKUList != null) pe.FreeSKUList = ConvertToFreeSKUCollection(ep.FreeSKUList);
        //    if (ep.MaxFreight != null) pe.MaxFreight = ep.MaxFreight.Value;
        //    if (ep.NumOfMonth != null) pe.NumOfMonth = ep.NumOfMonth.Value;
        //    pe.OnlineOrderOnly = ep.OnlineOrderOnly;
        //    pe.PromotionType = ConvertToPromotionType(ep.PromotionType);
        //    if (ep.SelectableSKUList != null) pe.SelectableSKUList = ConvertToFreeSKUCollection(ep.SelectableSKUList);
        //    if (ep.ShippedToProvince != null) pe.ShippedToProvince = ep.ShippedToProvince;
        //    pe.StartDate = ep.StartDate.ToString(PromotionDateFormat_V00);
        //    if (ep.VolumeMax != null) pe.VolumeMax = ep.VolumeMax.Value;
        //    if (ep.VolumeMaxInclude != null) pe.VolumeMaxInclude = ep.VolumeMaxInclude.Value;
        //    if (ep.VolumeMin != null) pe.VolumeMin = ep.VolumeMin.Value;
        //    if (ep.VolumeMinInclude != null) pe.VolumeMinInclude = ep.VolumeMinInclude.Value;

        //    //if (ep.CustCategoryType != null) pe.CustCategoryType = ep.CustCategoryType;
        //    //if (ep.CustType != null) pe.CustType = ep.CustType;
        //    //pe.ForPC = ep.ForPC;

        //    ret.Add(pe);
        //}

        //PromotionCollectionCacheMedia = ret;

        //return ret;

        FreeSKUCollection ConvertToFreeSKUCollection1(string value)
        {

            var ret = new FreeSKUCollection();
            string freeSKUs = value as string;
            if (!string.IsNullOrEmpty(freeSKUs))
            {
                string[] skuQtyArr = freeSKUs.Split(',');
                foreach (var l in skuQtyArr)
                {
                    string[] skuQty = l.Split('|');
                    if (skuQty.Length != 0)
                    {
                        ret.Add(new FreeSKU { Quantity = int.Parse(skuQty[1]), SKU = skuQty[0] });
                    }
                }
            }

            return ret;
        }
        FreeSKUCollection ConvertToFreeSKUCollection(List<ProductSetting> list)
        {
            if (list == null) return null;

            FreeSKUCollection ret = new FreeSKUCollection();
            list.ForEach(x => ret.Add(new FreeSKU { SKU = x.SKU, Quantity = x.Quantity }));

            return ret;
        }

        PromotionType ConvertToPromotionType(string val)
        {
            var ptList = val.Split(',').ToList();

            PromotionType ret = PromotionType.None;

            var vals = Enum.GetValues(typeof(PromotionType));
            foreach (var v in vals)
            {
                var ptStr = v.ToString();
                PromotionType pt = PromotionType.None;
                if (!Enum.TryParse<PromotionType>(ptStr, out pt)) continue;

                if (ptList.Contains(ptStr)) ret = ret | pt;
            }

            return ret;
        }

        /// <summary>
        /// get promotion from config or session
        /// </summary>
        /// <returns></returns>
        private PromotionCollection getPromotion()
        {
            // return PromotionCollectionCacheMedia as PromotionCollection;
            return ChinaPromotionProvider.LoadPromoConfig();
        }

        public bool validateSKU(string sku, string storeID, MyHLShoppingCart shoppingCart, ShoppingCartRuleResult Result)
        {
            if (shoppingCart != null && shoppingCart.RuleResults != null)
            {
                bool valid = false;
                var ProductInfoCatalog = CatalogProvider.GetProductInfoCatalog(Locale, storeID);
                SKU_V01 sku_v01 = null;
                Providers.China.CatalogProvider.LoadInventory(storeID);
                CatalogItem_V01 catalogItem = CatalogProvider.GetCatalogItem(sku, "CN");
                string messages =
                    string.Format(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_Rules", HLConfigManager.Platform),
                            "PromoSkuInventory").ToString(), "", "");
                if (catalogItem != null)
                {
                    if (catalogItem.InventoryList == null)
                    {

                        var ruleResultMessages = HLConfigManager.Platform == "MyHL" ?
                            shoppingCart.RuleResults.Where(
                                r =>
                                r.Result == RulesResult.Failure && r.Messages != null && r.Messages.Count > 0 &&
                                r.RuleName == "Promotional Rules") : shoppingCart.RuleResults.Where(
                                r =>
                                r.Result == RulesResult.Feedback && r.Messages != null && r.Messages.Count > 0 &&
                                r.RuleName == "Promotional Rules");

                        if (ruleResultMessages != null)
                        {

                            if (!ruleResultMessages.Any(x => x.Messages.Contains(messages)))
                            {
                                if (HLConfigManager.Platform == "MyHL")
                                {
                                    Result.Result = RulesResult.Failure;
                                    Result.RuleName = "Promotional Rules";
                                    Result.AddMessage(messages);
                                    shoppingCart.RuleResults.Add(Result);
                                }
                                else
                                {
                                    Result.Result = RulesResult.Feedback;
                                    Result.RuleName = "Promotional Rules";
                                    Result.AddMessage(messages);
                                    shoppingCart.RuleResults.Add(Result);
                                }

                            }

                        }


                        return false;
                    }
                    WarehouseInventory inv = null;
                    if (!catalogItem.InventoryList.TryGetValue(storeID, out inv))
                    {

                        var ruleResultMessages = HLConfigManager.Platform == "MyHL" ?
                             shoppingCart.RuleResults.Where(
                                 r =>
                                 r.Result == RulesResult.Failure && r.Messages != null && r.Messages.Count > 0 &&
                                 r.RuleName == "Promotional Rules") : shoppingCart.RuleResults.Where(
                                 r =>
                                 r.Result == RulesResult.Feedback && r.Messages != null && r.Messages.Count > 0 &&
                                 r.RuleName == "Promotional Rules");
                        if (ruleResultMessages != null)
                        {

                            if (!ruleResultMessages.Any(x => x.Messages.Contains(messages)))
                            {
                                if (HLConfigManager.Platform == "MyHL")
                                {
                                    Result.Result = RulesResult.Failure;
                                    Result.RuleName = "Promotional Rules";
                                    Result.AddMessage(messages);
                                    shoppingCart.RuleResults.Add(Result);
                                }
                                else
                                {
                                    Result.Result = RulesResult.Feedback;
                                    Result.RuleName = "Promotional Rules";
                                    Result.AddMessage(messages);
                                    shoppingCart.RuleResults.Add(Result);
                                }

                            }

                        }

                        return false;
                    }
                    bool hasInventory = (inv as WarehouseInventory_V01).QuantityAvailable > 0;
                    bool isBlocked = false;
                    if (catalogItem.InventoryList != null)
                    {
                        isBlocked = (inv as WarehouseInventory_V01).IsBlocked;
                    }
                    if (!ProductInfoCatalog.AllSKUs.TryGetValue(sku, out sku_v01))
                    {
                        if (hasInventory)
                        {
                            sku_v01 = new SKU_V01
                            {
                                Description = catalogItem.Description,
                                SKU = sku,
                                ProductAvailability = ProductAvailabilityType.Available,
                                CatalogItem = catalogItem,
                                IsPurchasable = false,
                            };
                            ProductInfoCatalog.AllSKUs.Add(sku, sku_v01);
                        }
                    }
                    valid = hasInventory && !isBlocked;
                }
                if (!valid)
                {

                    var ruleResultMessages = HLConfigManager.Platform == "MyHL" ?
                              shoppingCart.RuleResults.Where(
                                  r =>
                                  r.Result == RulesResult.Failure && r.Messages != null && r.Messages.Count > 0 &&
                                  r.RuleName == "Promotional Rules") : shoppingCart.RuleResults.Where(
                                  r =>
                                  r.Result == RulesResult.Feedback && r.Messages != null && r.Messages.Count > 0 &&
                                  r.RuleName == "Promotional Rules");
                    if (ruleResultMessages != null)
                    {

                        if (!ruleResultMessages.Any(x => x.Messages.Contains(messages)))
                        {
                            if (HLConfigManager.Platform == "MyHL")
                            {
                                Result.Result = RulesResult.Failure;
                                Result.RuleName = "Promotional Rules";
                                Result.AddMessage(messages);
                                shoppingCart.RuleResults.Add(Result);
                            }
                            else
                            {
                                Result.Result = RulesResult.Feedback;
                                Result.RuleName = "Promotional Rules";
                                Result.AddMessage(messages);
                                shoppingCart.RuleResults.Add(Result);
                            }

                            //else
                            //{
                            //    var SRPromo2Sku = HL.Common.Configuration.Settings.GetRequiredAppSetting("ChinaSRPromoPhase2", string.Empty).Split('|');
                            //    var itemsInBoth =
                            //                        shoppingCart.CartItems.Select(c => c.SKU)
                            //                            .Intersect(SRPromo2Sku, StringComparer.OrdinalIgnoreCase);
                            //    if (itemsInBoth.Any())
                            //    {
                            //        string message2 = HttpContext.GetGlobalResourceObject(
                            //           string.Format("{0}_Rules", HLConfigManager.Platform),
                            //           "SRPromotionPhase2").ToString();

                            //        Result.Result = RulesResult.Feedback;
                            //        Result.RuleName = "Promotional Rules";
                            //        if (Result.Messages == null)
                            //            Result.AddMessage(message2);
                            //        else if (Result.Messages.Where(s => s == messages).ToString().Length <=0)
                            //            Result.AddMessage(message2);
                            //        shoppingCart.RuleResults.Add(Result);
                            //    }
                            //}

                        }

                    }

                }


                return valid;
            }
            return false;
        }

        private bool IsPromoTypeFlagOn(PromotionType promoType, PromotionType check)
        {
            return ((promoType & check) == check);
        }

        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason)
        {
            #region
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult
            {
                RuleName = "Promotional Rules",
                Result = RulesResult.Unknown
            };
            result.Add(defaultResult);

            var hlCart = cart as MyHLShoppingCart;
            if (cart == null || hlCart == null)
            {
                return result;
            }

            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                // If promo was cancelled previously then return
                CurrentSession = SessionInfo.GetSessionInfo(DistributorProfileModel.Id, Locale);
                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");

                //if (CurrentSession.IsReplacedPcOrder &&
                //    CurrentSession.ReplacedPcDistributorOrderingProfile != null)
                //{
                //    distributorOrderingProfile = CurrentSession.ReplacedPcDistributorOrderingProfile;
                //}
                //else
                //{
                //    distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID,
                //                                                  "CN");
                //}
                if (distributorOrderingProfile.IsPC)
                {
                    if (Session["SurveyCancelled"] != null && Convert.ToBoolean(Session["SurveyCancelled"]))
                        return result;
                    PromotionCollection promotions = null;

                    if ((getPromotion()) == null)
                        ChinaPromotionProvider.LoadPromoConfig();
                    promotions = getPromotion();
                    if (promotions != null)
                    {
                        // Check for the promo when Memeber comes to shopping cart page
                        var promotion = promotions.FirstOrDefault(p => p.Code.Equals("PCPromoSurvey"));
                        if (reason == ShoppingCartRuleReason.CartRetrieved && promotion != null)
                        {
                            if (hlCart.Totals != null)
                            {

                                if ((((hlCart.Totals as OrderTotals_V01).ProductRetailAmount) >= promotion.AmountMinInclude) &&
                                    promotion.Code.Equals("PCPromoSurvey"))
                                {
                                    if (promotion.FreeSKUList != null &&
                                        !APFDueProvider.hasOnlyAPFSku(hlCart.CartItems, Locale) &&
                                        hlCart.CartItems.Count > 0)
                                    {
                                        defaultResult.Result = RulesResult.Feedback;
                                        result.Add(defaultResult);
                                        cart.RuleResults.Add(defaultResult);
                                    }
                                }
                            }

                        }
                    }
                }


                // Get the promo configuration

            }
            return result;
            #endregion

        }

        private void processOtherPromotion(PromotionElement promotion, ShoppingCart_V02 cart,
            ShoppingCartRuleReason reason, ShoppingCartRuleResult Result)
        {
            try
            {
                if (promotion.Code == "PCPromo") return;

                if (reason == ShoppingCartRuleReason.CartItemsAdded || reason == ShoppingCartRuleReason.CartCreated)
                {
                    var shoppingCart = cart as MyHLShoppingCart;                       
                    if (promotion.AmountMinInclude == 0 || promotion.VolumeMinInclude == 0)
                    {
                        var itemsInBoth =
                                         cart.CartItems.Where(x => x.IsPromo)
                                             .Select(c => c.SKU)
                                             .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                        if (itemsInBoth.Any())
                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                        var freeSkuCollection = QtyToAdd(promotion, cart);
                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                           shoppingCart,
                           Result);
                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                        Array.ForEach(
                            shoppingCart.ShoppingCartItems.TakeWhile(
                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                            a => a.Flavor = FreeProductText);

                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                    }
                    else if (promotion.FreeSKUList != null)
                    {

                        if (shoppingCart == null)
                            return;
                        var totals = shoppingCart.Totals as ServiceProvider.OrderSvc.OrderTotals_V02;
                        if (totals == null)
                            return;

                        if (totals.OrderFreight != null &&
                            totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation >=
                            promotion.AmountMinInclude)
                        {
                            if (!promotion.HasIncrementaldegree)
                            {
                                if (shoppingCart.CartItems.Count > 0 &&
                                    !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                                {
                                    var count = (from selectablesku in promotion.SelectableSKUList
                                                 from cartitem in shoppingCart.CartItems
                                                 where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                 select cartitem.Quantity).Sum();

                                    if (count > 1)
                                    {
                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(promotion, shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                            a => a.Flavor = FreeProductText);

                                        SetPromotionalRuleResponses_V02(Result, promotion.Code, TypeOfPromotion.Other,
                                            PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }
                                    else
                                    {
                                        var itemsInBoth =
                                            cart.CartItems.Where(x => x.IsPromo)
                                                .Select(c => c.SKU)
                                                .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                        if (itemsInBoth.Any())
                                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                    }
                                }
                            }
                            else
                            {

                                var temppromotion = (PromotionElement)promotion.Clone();
                                if (promotion.Code == "PCChinaPromo")
                                {
                                    var Quantity =
                                        (int)(totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation) /
                                        temppromotion.AmountMinInclude;
                                    if (CurrentSession != null)
                                    {
                                        CurrentSession.ChinaPromoSKUQuantity = (int)Quantity;
                                    }
                                    var message = HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "EligibleForChinaPromotionalSKU");
                                    Result.Result = RulesResult.Failure;
                                    Result.AddMessage(message.ToString());
                                    SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                        "EligibleForChinaPromotionalSKU" + "@" +
                                        (int)Quantity);
                                    cart.RuleResults.Add(Result);
                                }
                                else
                                {

                                    // First deleting all existing promoskus for PDM Borsch flavor Feb 2015 [蛋白营养粉（罗宋汤口味）上市促销信息(userstory :146058)
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                    if ((temppromotion.SelectableSKUList == null) ||
                                        (temppromotion.SelectableSKUList.Count == 0))
                                    {
                                        // if (CurrentSession != null && CurrentSession.IsReplacedPcOrder != true && !shoppingCart.IgnorePromoSKUAddition)
                                        //{
                                        //var quantity =
                                        //    (int) (totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation)/
                                        //    temppromotion.AmountMinInclude;
                                        //freeSkuCollection.AddRange(
                                        //    promotion.FreeSKUList.Select(
                                        //        sku => new FreeSKU {Quantity = (int) quantity, SKU = sku.SKU}));
                                        var freeSkuCollection = CheckPcPromo(promotion, cart);

                                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                            shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                            a => a.Flavor = FreeProductText);
                                        //}
                                    }
                                    else
                                    {

                                        if (promotion.Code == "PCNovPromo")
                                        {
                                            var freeSkuCollection = new FreeSKUCollection();

                                            var Q =
                                                (int)
                                                    (totals.AmountDue - totals.OrderFreight.FreightCharge -
                                                     totals.Donation) /
                                                temppromotion.AmountMinInclude;
                                            var v = (from selectablesku in promotion.SelectableSKUList
                                                     from cartitem in shoppingCart.CartItems
                                                     where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                     select cartitem.Quantity).Sum();

                                            if (Q > 0.0m)
                                            {
                                                freeSkuCollection.AddRange(
                                                    promotion.FreeSKUListForVolume.Select(
                                                        sku => new FreeSKU { Quantity = (int)Q, SKU = sku.SKU }));
                                            }
                                            if (v > 0)
                                            {
                                                foreach (var selectablesku in promotion.FreeSKUListForSelectableSku)
                                                {
                                                    freeSkuCollection.AddRange(
                                                        promotion.FreeSKUListForSelectableSku.Select(
                                                            sku =>
                                                                new FreeSKU
                                                                {
                                                                    Quantity = v * selectablesku.Quantity,
                                                                    SKU = sku.SKU
                                                                }));
                                                }
                                            }

                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                        }
                                        else
                                        {
                                            var freeSkuCollection = PCPromo(promotion, cart);
                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                    .ToArray(), a => a.Flavor = FreeProductText);
                                            SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                                TypeOfPromotion.Other,
                                                PromotionAction.ItemAddedToCart, itemsToAdd);
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            if (CurrentSession != null)
                            {
                                CurrentSession.ChinaPromoSKUQuantity = 0;
                            }
                            var itemsInBoth =
                                cart.CartItems.Where(x => x.IsPromo)
                                    .Select(c => c.SKU)
                                    .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                            if (itemsInBoth.Any())
                                shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                            if (promotion.Code == "PCNovPromo")
                            {
                                var freeSkuCollection = new FreeSKUCollection();
                                var count = (from selectablesku in promotion.SelectableSKUList
                                             from cartitem in shoppingCart.CartItems
                                             where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                             select cartitem.Quantity).Sum();
                                if (count > 0)
                                {
                                    foreach (var sku in promotion.FreeSKUListForSelectableSku)
                                    {
                                        count = count * sku.Quantity;
                                        var freesku = new FreeSKU { Quantity = count, SKU = sku.SKU };
                                        freeSkuCollection.Add(freesku);

                                    }
                                    List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                        shoppingCart,
                                        Result);
                                    shoppingCart.AddItemsToCart(itemsToAdd, true);
                                    Array.ForEach(
                                        shoppingCart.ShoppingCartItems.TakeWhile(
                                            x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                        a => a.Flavor = FreeProductText);
                                }
                            }


                        }

                    }

                }
                else if (reason == ShoppingCartRuleReason.CartItemsRemoved)
                {

                    var shoppingCart = cart as MyHLShoppingCart;
                    var totals = shoppingCart.Totals as OrderTotals_V02;
                    if (totals == null)
                    {
                        return;
                    }

                    if (totals.OrderFreight != null &&
                        totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation >=
                        promotion.AmountMinInclude)
                    {
                        if (shoppingCart.CartItems.Count > 0 &&
                            !APFDueProvider.hasOnlyAPFSku(shoppingCart.CartItems, Locale))
                        {
                            if (!promotion.HasIncrementaldegree)
                            {
                                var count = (from selectablesku in promotion.SelectableSKUList
                                             from cartitem in shoppingCart.CartItems
                                             where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                             select cartitem.Quantity).Sum();
                                if (count < 2)
                                {
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                                }
                            }
                            else if (promotion.HasIncrementaldegree)
                            {
                                var temppromotion = (PromotionElement)promotion.Clone();
                                if (promotion.Code == "PCChinaPromo")
                                {
                                    var Quantity = (int)totals.AmountDue / temppromotion.AmountMinInclude;
                                    if (CurrentSession != null)
                                    {
                                        CurrentSession.ChinaPromoSKUQuantity = (int)Quantity;
                                    }
                                    //var message = HttpContext.GetGlobalResourceObject(
                                    //                 string.Format("{0}_Rules", HLConfigManager.Platform), "EligibleForChinaPromotionalSKU");
                                    //Result.Result = RulesResult.Failure;
                                    //Result.AddMessage(message.ToString());
                                    //SetPromotionalRuleResponses_FreeListSku(Result, promotion.Code,
                                    //                    "EligibleForChinaPromotionalSKU" + "@" + (int)Quantity);
                                    //cart.RuleResults.Add(Result);
                                }
                                else
                                {
                                    var itemsInBoth =
                                        cart.CartItems.Where(x => x.IsPromo)
                                            .Select(c => c.SKU)
                                            .Intersect(temppromotion.FreeSKUList.Select(f => f.SKU));
                                    if (itemsInBoth.Any())
                                        shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

                                    if ((temppromotion.SelectableSKUList == null) ||
                                        (temppromotion.SelectableSKUList.Count == 0))
                                    {
                                        var freeSkuCollection = new FreeSKUCollection();
                                        var quantity = (int)((totals.AmountDue - totals.OrderFreight.FreightCharge - totals.Donation) / temppromotion.AmountMinInclude);
                                        freeSkuCollection.AddRange(
                                            promotion.FreeSKUList.Select(
                                                sku => new FreeSKU { Quantity = (int)quantity, SKU = sku.SKU }));
                                        var itemsToAdd = getItemsToAdd(freeSkuCollection, shoppingCart,
                                            Result);
                                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                                        Array.ForEach(
                                            shoppingCart.ShoppingCartItems.TakeWhile(
                                                x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                .ToArray(), a => a.Flavor = FreeProductText);

                                        SetPromotionalRuleResponses_V02(Result, promotion.Code, TypeOfPromotion.Other,
                                            PromotionAction.ItemAddedToCart, itemsToAdd);
                                    }
                                    else
                                    {
                                        if (promotion.Code == "PCNovPromo")
                                        {
                                            var freeSkuCollection = new FreeSKUCollection();
                                            var Q =
                                                (int)
                                                    (totals.AmountDue - totals.OrderFreight.FreightCharge -
                                                     totals.Donation) /
                                                temppromotion.AmountMinInclude;
                                            var v = (from selectablesku in promotion.SelectableSKUList
                                                     from cartitem in shoppingCart.CartItems
                                                     where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                                     select cartitem.Quantity).Sum();

                                            if (Q > 0.0m)
                                            {
                                                freeSkuCollection.AddRange(
                                                    promotion.FreeSKUListForVolume.Select(
                                                        sku => new FreeSKU { Quantity = (int)Q, SKU = sku.SKU }));
                                            }
                                            if (v > 0)
                                            {
                                                foreach (var selectablesku in promotion.FreeSKUListForSelectableSku)
                                                {
                                                    freeSkuCollection.AddRange(
                                                        promotion.FreeSKUListForSelectableSku.Select(
                                                            sku =>
                                                                new FreeSKU
                                                                {
                                                                    Quantity = v * selectablesku.Quantity,
                                                                    SKU = sku.SKU
                                                                }));
                                                }
                                            }
                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart,
                                                Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                                                a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                                        }
                                        else
                                        {
                                            var freeSkuCollection = PCPromo(promotion, cart);
                                            List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                                                shoppingCart, Result);
                                            shoppingCart.AddItemsToCart(itemsToAdd, true);
                                            Array.ForEach(
                                                shoppingCart.ShoppingCartItems.TakeWhile(
                                                    x => itemsToAdd.Exists(i => i.SKU == x.SKU))
                                                    .ToArray(), a => a.Flavor = FreeProductText);

                                            SetPromotionalRuleResponses_V02(Result, promotion.Code,
                                                TypeOfPromotion.Other, PromotionAction.ItemAddedToCart, itemsToAdd);
                                        }
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
                        if (CurrentSession != null)
                        {
                            CurrentSession.ChinaPromoSKUQuantity = 0;
                        }
                        var itemsInBoth =
                            cart.CartItems.Where(x => x.IsPromo)
                                .Select(c => c.SKU)
                                .Intersect(promotion.FreeSKUList.Select(f => f.SKU));
                        if (itemsInBoth.Any())
                            shoppingCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);
                    }
                    if (promotion.Code == "PCNovPromo")
                    {

                        var v = (from selectablesku in promotion.SelectableSKUList
                                 from cartitem in shoppingCart.CartItems
                                 where cartitem.SKU.Equals(selectablesku.SKU.Trim())
                                 select cartitem.Quantity).Sum();

                        var freeSkuCollection = new FreeSKUCollection();
                        freeSkuCollection.AddRange(
                            promotion.FreeSKUListForSelectableSku.Select(
                                sku => new FreeSKU { Quantity = v * sku.Quantity, SKU = sku.SKU }));

                        List<ShoppingCartItem_V01> itemsToAdd = getItemsToAdd(freeSkuCollection,
                            shoppingCart,
                            Result);
                        shoppingCart.AddItemsToCart(itemsToAdd, true);
                        Array.ForEach(
                            shoppingCart.ShoppingCartItems.TakeWhile(
                                x => itemsToAdd.Exists(i => i.SKU == x.SKU)).ToArray(),
                            a => a.Flavor = FreeProductText);

                        SetPromotionalRuleResponses_FreeSku(Result, promotion.Code, itemsToAdd);
                    }

                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error orrcured in China processOtherPromotion");
            }
        }
        //private void AddSRPromoPhase2(ShoppingCart_V02 cart)
        //{
        //    var myCart = cart as MyHLShoppingCart;
        //    Dictionary<string, SKU_V01> _AllSKUS = CatalogProvider.GetAllSKU(Locale);
        //    string[] skuList = new string[0];

        //    if (myCart != null && myCart.CartItems.Any())
        //    {
        //        if (myCart.DeliveryInfo == null || string.IsNullOrEmpty(myCart.DeliveryInfo.WarehouseCode))
        //        {
        //            return;
        //        }
        //        if (CatalogProvider.IsPreordering(myCart.CartItems, myCart.DeliveryInfo.WarehouseCode))
        //        {
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        return;
        //    }


        //    if (ChinaPromotionProvider.IsEligibleForSRQGrowingPromotion(myCart, HLConfigManager.Platform))
        //    {
        //        var cacheKey = string.Format("GetSRPromoQGrowingDetail_{0}", myCart.DistributorID);
        //        var results = HttpRuntime.Cache[cacheKey] as GetSRPromotionResponse_V01;
        //        skuList = results.Skus.Split(',').ToArray();
        //        if (results != null && !string.IsNullOrWhiteSpace(results.Skus))
        //        {
        //            if (skuList.Count() > 0)
        //            {
        //                if (myCart != null && myCart.CartItems.Count > 0)
        //                {
        //                    SKU_V01 sku;
        //                    if (_AllSKUS.TryGetValue(skuList[0], out sku))
        //                    {
        //                        if ((ShoppingCartProvider.CheckInventory(sku.CatalogItem, 1,
        //                                                                 myCart.DeliveryInfo.WarehouseCode) > 0 &&
        //                             (CatalogProvider.GetProductAvailability(sku,
        //                                                                     myCart.DeliveryInfo.WarehouseCode) == ProductAvailabilityType.Available)))
        //                        {

        //                            //var myShoppingCart = (Page as ProductsBase).ShoppingCart;

        //                            var promoInCart = myCart.CartItems.Where(c => c.SKU == skuList[0]).Select(c => c.SKU);
        //                            if (myCart.CartItems.Count > 0)
        //                            {
        //                                if (promoInCart.Any())
        //                                {
        //                                    myCart.DeleteItemsFromCart(promoInCart.ToList(), true);
        //                                }
        //                                var itemsToAddg = new List<ShoppingCartItem_V01>
        //                                {
        //                                    new ShoppingCartItem_V01
        //                                    {
        //                                        Quantity = 1,
        //                                        SKU = skuList[0],
        //                                        Updated = DateTime.Now,
        //                                        IsPromo = true,
        //                                    }
        //                                };
        //                                //myCart.ShoppingCartIsFreightExempted = true;
        //                                myCart.AddItemsToCart(itemsToAddg, true);
        //                                //ShoppingCartChanged(this, new ShoppingCartEventArgs(myCart));
        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //        }
        //        skuList = new string[0];
        //    }
        //    else if (ChinaPromotionProvider.IsEligibleForSRQExcellentPromotion(myCart, HLConfigManager.Platform))
        //    {
        //        var cacheKey1 = string.Format("GetSRPromoQExcellentDetail_{0}", myCart.DistributorID);
        //        var results1 = HttpRuntime.Cache[cacheKey1] as GetSRPromotionResponse_V01;
        //        skuList = results1.Skus.Split(',').ToArray();
        //        if (results1 != null && !string.IsNullOrWhiteSpace(results1.Skus))
        //        {
        //            if (skuList.Count() > 0)
        //            {
        //                if (myCart != null && myCart.CartItems.Count > 0)
        //                {

        //                    SKU_V01 sku;
        //                    if (_AllSKUS.TryGetValue(skuList[0], out sku))
        //                    {
        //                        if ((ShoppingCartProvider.CheckInventory(sku.CatalogItem, 1,
        //                                                                 myCart.DeliveryInfo.WarehouseCode) > 0 &&
        //                             (CatalogProvider.GetProductAvailability(sku,
        //                                                                     myCart.DeliveryInfo.WarehouseCode) == ProductAvailabilityType.Available)))
        //                        {

        //                            //var myShoppingCart = (Page as ProductsBase).ShoppingCart;

        //                            var promoInCart = myCart.CartItems.Where(c => c.SKU == skuList[0]).Select(c => c.SKU);
        //                            if (myCart.CartItems.Count > 0)
        //                            {
        //                                if (promoInCart.Any())
        //                                {
        //                                    myCart.DeleteItemsFromCart(promoInCart.ToList(), true);
        //                                }
        //                                var itemsToAddE = new List<ShoppingCartItem_V01>
        //                                {
        //                                    new ShoppingCartItem_V01
        //                                    {
        //                                        Quantity = 1,
        //                                        SKU = skuList[0],
        //                                        Updated = DateTime.Now,
        //                                        IsPromo = true,
        //                                    }
        //                                };
        //                                //myCart.ShoppingCartIsFreightExempted = true;
        //                                myCart.AddItemsToCart(itemsToAddE, true);

        //                                //ShoppingCartChanged(this, new ShoppingCartEventArgs(myCart));
        //                            }
        //                        }

        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {

        //            var itemsInBoth =
        //                          myCart.CartItems.Where(x => x.IsPromo)
        //                              .Select(c => c.SKU)
        //                              .Intersect(skuList, StringComparer.OrdinalIgnoreCase);
        //            if (itemsInBoth.Any())
        //                myCart.DeleteItemsFromCart(itemsInBoth.ToList(), true);

        //    }
        //}
    }

}


