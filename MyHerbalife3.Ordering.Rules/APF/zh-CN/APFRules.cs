using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;


namespace MyHerbalife3.Ordering.Rules.APF.zh_CN
{
    public class APFRules : MyHerbalifeRule, IShoppingCartRule
    {
        #region Events

        #endregion

        #region Consts


        private const string IS_EVENT_TICKET_SESSION_KEY = "Is_Event_Ticket";
        private const string RuleName = "APF Rules";

        #endregion

        #region IShoppingCartRule interface implementation

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            defaultResult.ApfRuleResponse = new ApfRuleResponse();

            if (null != cart)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }

            return result;
        }

        #endregion

        #region Private methods

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleResult result,
                                                    ShoppingCartRuleReason reason)
        {
            if (null != cart)
            {
                var myhlCart = cart as MyHLShoppingCart;

                string level = "DS";

                if (DistributorProfileModel != null)
                {
                    level = DistributorProfileModel.TypeCode.ToUpper();
                }
                else
                {
                    level = GetMemberLevelFromDistributorProfile(cart.DistributorID);
                }
                if (null != myhlCart)
                {
                    //Do the rules
                    if (reason == ShoppingCartRuleReason.CartCreated || reason == ShoppingCartRuleReason.CartRetrieved)
                    {
                        if (myhlCart.OrderCategory == OrderCategoryType.ETO) //No APFs allowed in ETO cart 
                        {
                            result.Result = RulesResult.Success;
                            return result;
                        }
                        return CartRetrievedRuleHandler(cart, result, level);
                    }
                    else if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
                    {
                        if (myhlCart.OrderCategory == OrderCategoryType.ETO) //No APFs allowed in ETO cart 
                        {
                            result.Result = RulesResult.Success;
                            return result;
                        }
                        return CartItemBeingAddedRuleHandler(cart, result, level);
                    }
                    else if (reason == ShoppingCartRuleReason.CartItemsBeingRemoved)
                    {
                        if (myhlCart.OrderCategory == OrderCategoryType.ETO) //No APFs allowed in ETO cart 
                        {
                            result.Result = RulesResult.Success;
                            return result;
                        }
                        return CartItemBeingDeletedRuleHandler(cart, result, level);
                    }
                    else if (reason == ShoppingCartRuleReason.CartItemsAdded)
                    {
                        if (myhlCart.OrderCategory == OrderCategoryType.ETO) //No APFs allowed in ETO cart 
                        {
                            result.Result = RulesResult.Success;
                            return result;
                        }
                        return CartItemAddedRuleHandler(cart, result, level);
                    }
                }
            }
            return result;
        }

        private static string GetMemberLevelFromDistributorProfile(string distributorId)
        {
            return DistributorProfile(distributorId).TypeCode;
        }

        private bool IsAPFExempt(DistributorOrderingProfile distributorProfile, decimal currentOrderPoint)
        {
            bool isVPExceeded = false;
            if (distributorProfile == null) return false;

            //var datenow = DateTime.Now; // new DateTime(2015, 9, 17, 12, 0, 0); //(DateTime.Now).Date
            //if (!orderingProfile.ApfDueDate.Equals(datenow.Date)) return false;

            var totalVolum = distributorProfile.DistributorVolumes.Find(v => v.VolumeDate.Month == DateTime.UtcNow.Month);

            if (totalVolum != null && (totalVolum.Volume + currentOrderPoint) >= 200) isVPExceeded = true;

            return isVPExceeded;
        }

        /// <summary>Distributor is APF Due, add APF to the cart</summary>
        /// <param name="distributorID"></param>
        /// <param name="result"></param>
        /// <param name="cacheKey"></param>
        /// <param name="locale"></param>
        /// <param name="cartHasItems"></param>
        /// <param name="ruleResult"></param>
        private void DoApfDue(string distributorID,
                              ShoppingCart_V01 result,
                              string cacheKey,
                              string locale,
                              bool cartHasItems,
                              ShoppingCartRuleResult ruleResult,
                              bool justEntered)
        {
            var cart = result as MyHLShoppingCart;
            if (cart == null)
            {
                return;
            }

            try
            {
                string level;
                if (DistributorProfileModel != null)
                {
                    level = DistributorProfileModel.TypeCode.ToUpper();
                }
                else
                {
                    level = GetMemberLevelFromDistributorProfile(cart.DistributorID);
                }

                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID,
                                                                                               cart.CountryCode);
                if ((distributorOrderingProfile.HardCashOnly &&
                     !HLConfigManager.Configurations.PaymentsConfiguration.AllowWireForHardCash))
                {
                    return;
                }
                var apfItems = new List<ShoppingCartItem_V01>();
                if (cart.CartItems != null && cart.CartItems.Count > 0)
                {
                    //Stash off all non-APF items - to be re-added if appropriate
                    var nonApfItems =
                        (from c in cart.CartItems where APFDueProvider.IsAPFSku(c.SKU.Trim()) == false select c)
                            .ToList<ShoppingCartItem_V01>();
                    apfItems =
                        (from c in cart.CartItems where APFDueProvider.IsAPFSku(c.SKU.Trim()) select c)
                            .ToList<ShoppingCartItem_V01>();
                    if (nonApfItems.Count > 0 ||
                        HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                    {
                        // Clear the cart
                        cart.DeleteItemsFromCart(null, true);
                        //if (APFDueProvider.CanEditAPFOrder(distributorID, locale, level))
                        //{
                        //Global rule - they can always edit the cart ie add remove products at least
                        var list =
                            CatalogProvider.GetCatalogItems((from p in nonApfItems select p.SKU).ToList(), Country);
                        var products =
                            (from c in list where c.Value.ProductType == ProductType.Product select c.Value.SKU).ToList();
                        var nonproducts =
                            (from c in list where c.Value.ProductType != ProductType.Product select c.Value.SKU).ToList();
                        if (!HLConfigManager.Configurations.APFConfiguration.AllowNonProductItemsWithStandaloneAPF)
                        //We don't allow non product items alone on an apf order
                        {
                            if (products.Count == 0)
                            {
                                if (nonproducts.Count > 0)
                                {
                                    ruleResult.Result = RulesResult.Success;
                                    ruleResult.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "NonProductItemsRemovedForStandaloneAPF") as string);
                                    cart.RuleResults.Add(ruleResult);
                                }
                            }
                            else
                            {
                                cart.AddItemsToCart(nonApfItems, true);
                            }
                        }
                        else
                        {
                            if (!HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
                            {
                                cart.AddItemsToCart(nonApfItems, true);
                            }
                        }
                    }
                }
                else if (null != cart && null != cart.RuleResults)
                {
                    var rules =
                        (from rule in cart.RuleResults
                         where rule.RuleName == RuleName && rule.Result == RulesResult.Failure
                         select rule);
                    if (null != rules && rules.Count() > 0)
                    {
                        cart.RuleResults.Remove(rules.First());
                    }
                }

                //Add the APF in
                var apfSku = new List<ShoppingCartItem_V01>();
                var sku = APFDueProvider.GetAPFSku();
                //apfSku.Add(new ShoppingCartItem_V01(0, sku, 1, DateTime.Now));
                bool hasPreOrdering = false;

                //if (apfItems.Count > 0) cart.removeItem(sku);
                var datenow = DateTime.Now; // new DateTime(2015, 9, 17, 12, 0, 0); //(DateTime.Now).Date
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && distributorOrderingProfile.CNAPFStatus == 2)
                {
                    if (!APFDueProvider.IsAPFExemptOn200VP(distributorOrderingProfile, cart.VolumeInCart))
                        apfSku.Add(new ShoppingCartItem_V01(0, sku, 1, DateTime.Now));
                    else
                        cart.removeItem(sku);
                }
                else
                    apfSku.Add(new ShoppingCartItem_V01(0, sku, 1, DateTime.Now)); ;

                #region !cart.APFEdited
                if (!cart.APFEdited)
                {
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        if (cart.CartItems != null && cart.CartItems.Count > 0 && cart.DeliveryInfo != null)
                        {
                            hasPreOrdering = CatalogProvider.IsPreordering(cart.CartItems, cart.DeliveryInfo.WarehouseCode);
                        }

                        if (!hasPreOrdering)
                        {
                            if (cart.CurrentItems != null && cart.CurrentItems.Count > 0 && cart.DeliveryInfo != null)
                            {
                                hasPreOrdering = CatalogProvider.IsPreordering(cart.CurrentItems, cart.DeliveryInfo.WarehouseCode);
                            }
                        }
                    }

                    if (!hasPreOrdering)
                    {
                        if (apfSku.Count > 0)
                            apfSku[0].Quantity = CalcQuantity(distributorOrderingProfile.ApfDueDate);

                        if (apfSku.Count > 0 && cart.CartItems.Exists(c => c.SKU == apfSku[0].SKU))
                        {
                            var apf =
                                (from a in cart.CartItems where a.SKU == apfSku[0].SKU select a).First();
                            cart.DeleteItemsFromCart(
                                (from a in cart.CartItems where a.SKU == apfSku[0].SKU select a.SKU).ToList(), true);
                        }
                        if (cart.CartItems.Count == 0) //This is now a Standalone APF
                        {
                            SetAPFDeliveryOption(cart);
                        }

                        if (apfSku.Count > 0) cart.AddItemsToCart(apfSku, true);

                        if (justEntered && apfSku.Count > 0)
                        {
                            ruleResult.AddMessage(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "APFDueAdded") as string);
                            ruleResult.Result = RulesResult.Success;

                            SetApfRuleResponse(ruleResult, ApfAction.None, sku, "ApfRule", TypeOfApf.CantDSRemoveAPF,
                                "APFDueAdded");

                            cart.RuleResults.Add(ruleResult);
                        }
                        else
                        {
                            foreach (ShoppingCartRuleResult r in cart.RuleResults)
                            {
                                if (r.RuleName == RuleName)
                                {
                                    r.Messages.Clear();
                                    r.AddMessage(string.Empty);
                                }
                            }
                        }
                    }
                }
                #endregion
                //else
                //{
                //    if (APFDueProvider.CanRemoveAPF(distributorID, locale, level))
                //    {
                //        cart.AddItemsToCart(apfSku, true);
                //    }
                //}
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("doAPFDue DS:{0} ERR:{1}", distributorID, ex));
            }
        }

        private void SetApfRuleResponse(ShoppingCartRuleResult ruleResult, ApfAction action, string sku, string name, TypeOfApf apfType, string message)
        {
            ruleResult.ApfRuleResponse.Action = action;
            ruleResult.ApfRuleResponse.Sku = sku;
            ruleResult.ApfRuleResponse.Name = name;
            ruleResult.ApfRuleResponse.Type = apfType;
            ruleResult.ApfRuleResponse.Message = message;
        }

        //   private void SetApfRuleResponse()

        #region Event methods

        private ShoppingCartRuleResult CartRetrievedRuleHandler(ShoppingCart_V01 cart,
                                                                ShoppingCartRuleResult result,
                                                                string level)
        {
            string key = string.Format("{0}_{1}_{2}", "JustEntered", cart.DistributorID, Locale);

            bool justEntered = (Session != null && ((null != Session[key]))) ? (bool)Session[key] : true;
            if (Session != null) Session[key] = null;

            string cacheKey = string.Empty; ////ShoppingCartProvider.GetCacheKey(distributor.Value.ID, Locale);
            bool reloadAPFs = (null != cart && null != cart.CartItems) &&
                              APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale) &&
                              !APFDueProvider.IsAPFSkuPresent(cart.CartItems);
            if (null == cart || null == cart.CartItems || cart.CartItems.Count == 0 || reloadAPFs ||
                APFDueProvider.IsAPFSkuPresent(cart.CartItems))
            {
                try
                {
                    var hlCart = cart as MyHLShoppingCart;
                    if (null != hlCart)
                    {
                        var isPlacing = !String.IsNullOrWhiteSpace(hlCart.SrPlacingForPcOriginalMemberId);
                        if (!isPlacing && APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale))
                        {
                            if (justEntered || !cart.APFEdited)
                            {
                                DoApfDue(cart.DistributorID, cart, cacheKey, Locale, reloadAPFs, result, justEntered);
                            }
                        }
                        else
                        {
                            if (APFDueProvider.IsAPFSkuPresent(cart.CartItems))
                            {
                                if (isPlacing ||
                                    !hlCart.APFEdited ||
                                    !APFDueProvider.CanEditAPFOrder(hlCart.DistributorID, Locale, level))
                                {
                                    var skuList = APFDueProvider.GetAPFSkuList();
                                    if (skuList != null && skuList.Count > 0)
                                    {

                                        var items =
                                                 (from s in skuList
                                                  from c in cart.CartItems
                                                  where s == c.SKU
                                                  select c.SKU).ToList();

                                        if (null != items && items.Count > 0)
                                        {
                                            hlCart.DeleteItemsFromCart(items);
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("APFRules.ProcessAPF DS:{0} locale:{2} ERR:{1}", cart.DistributorID,
                                      ex, Locale));
                }
            }

            return result;
        }

        private ShoppingCartRuleResult CartItemAddedRuleHandler(ShoppingCart_V01 cart, ShoppingCartRuleResult result, string level)
        {

            var distributorId = cart.DistributorID;
            var isAPFDueandNotPaid = APFDueProvider.IsAPFDueAndNotPaid(distributorId, Locale);

            if (isAPFDueandNotPaid && HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                string cacheKey = string.Empty;
                bool reloadAPFs = (null != cart && null != cart.CartItems) && isAPFDueandNotPaid &&
                                  !APFDueProvider.IsAPFSkuPresent(cart.CartItems);
                cart.APFEdited = false;
                DoApfDue(cart.DistributorID, cart, cacheKey, Locale, reloadAPFs, result, true);
            }

            try
            {
                var hlCart = cart as MyHLShoppingCart;
                if (null != hlCart)
                {
                    var isPlacing = !String.IsNullOrWhiteSpace(hlCart.SrPlacingForPcOriginalMemberId);
                    if (isPlacing)
                    {
                        var skuList = APFDueProvider.GetAPFSkuList();
                        if (skuList != null && skuList.Count > 0)
                        {

                            var items =
                                        (from s in skuList
                                            from c in cart.CartItems
                                            where s == c.SKU
                                            select c.SKU).ToList();

                            if (null != items && items.Count > 0)
                            {
                                hlCart.DeleteItemsFromCart(items, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("APFRules.CartItemAddedRuleHandler DS:{0} locale:{2} ERR:{1}", cart.DistributorID,
                                  ex, Locale));
            }

            return result;
        }

        private ShoppingCartRuleResult CartItemBeingAddedRuleHandler(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult result,
                                                                     string level)
        {
            var distributorId = cart.DistributorID;
            var isAPFDueandNotPaid = APFDueProvider.IsAPFDueAndNotPaid(distributorId, Locale);

            if (isAPFDueandNotPaid && HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                        "CompleteAPFPurchase") as string);
                var sku = APFDueProvider.GetAPFSku();
                SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                            "CompleteAPFPurchase");

                cart.RuleResults.Add(result);
                return result;
            }
            else if (isAPFDueandNotPaid && HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                string cacheKey = string.Empty;
                bool reloadAPFs = (null != cart && null != cart.CartItems) && isAPFDueandNotPaid &&
                                  !APFDueProvider.IsAPFSkuPresent(cart.CartItems);

                DoApfDue(cart.DistributorID, cart, cacheKey, Locale, reloadAPFs, result, true);
            }

            if ((cart as MyHLShoppingCart).OrderCategory == OrderCategoryType.ETO) //No APFs allowed in ETO cart 
            {
                result.Result = RulesResult.Success;
                return result;
            }

            //cart.CurrentItems[0] contains the current item being added
            //because the provider only adds one at a time, we just need to return a single error, but aggregate to the cart errors for the UI
            if (APFDueProvider.IsAPFSku(cart.CurrentItems[0].SKU))
            {
                if (APFDueProvider.CanRemoveAPF(distributorId, Locale, level))
                {
                    if (APFDueProvider.IsAPFSku(cart.CurrentItems[0].SKU))
                    {
                        var originaItem =
                            (from c in cart.CartItems where c.SKU == cart.CurrentItems[0].SKU select c).ToList();
                        int previousQuantity = 0;
                        if (null != originaItem && originaItem.Count > 0)
                        {
                            previousQuantity = originaItem[0].Quantity;
                        }
                        if (APFDueProvider.IsAPFDueAndNotPaid(distributorId, Locale))
                        {
                            int requiredQuantity = APFDueProvider.APFQuantityDue(distributorId, Locale);
                            if (cart.CurrentItems[0].Quantity + previousQuantity <= requiredQuantity)
                            {
                                if (level == "SP")
                                {
                                    cart.CurrentItems[0].Quantity = requiredQuantity - previousQuantity;
                                    result.Result = RulesResult.Recalc;
                                    result.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "RequiredAPFLeftInCart") as string);
                                    var sku = APFDueProvider.GetAPFSku();
                                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.CantDSRemoveAPF,
                                                "RequiredAPFLeftInCart");
                                    cart.RuleResults.Add(result);
                                    cart.APFEdited = false;
                                    return result;
                                }
                                else if (level == "DS" || level == "FM" || level == "SC" || level == "SQ")
                                {
                                    result.Result = RulesResult.Recalc;
                                    result.AddMessage(string.Empty);
                                    cart.RuleResults.Add(result);
                                    cart.APFEdited = true;
                                    return result;
                                }
                                else
                                {
                                    result.Result = RulesResult.Recalc;
                                    result.AddMessage(string.Empty);
                                    cart.RuleResults.Add(result);
                                    cart.APFEdited = true;
                                    return result;
                                }
                            }
                            else if (cart.CurrentItems[0].Quantity + previousQuantity > requiredQuantity)
                            {
                                cart.CurrentItems[0].Quantity = requiredQuantity - previousQuantity;
                                result.Result = RulesResult.Recalc;
                                result.AddMessage(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "RequiredAPFLeftInCart") as string);
                                var sku = APFDueProvider.GetAPFSku();
                                SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.CantDSRemoveAPF,
                                            "RequiredAPFLeftInCart");
                                cart.RuleResults.Add(result);
                                cart.APFEdited = false;
                                return result;
                            }
                        }
                        else
                        {
                            if (cart.CurrentItems[0].Quantity + previousQuantity > 1)
                            {
                                if (APFDueProvider.CanAddAPF(distributorId))
                                {
                                    result.Result = RulesResult.Failure;
                                    result.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "CanOnlyPrepayOneAPF") as string);
                                    var sku = APFDueProvider.GetAPFSku();
                                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.CantDSRemoveAPF,
                                                "CanOnlyPrepayOneAPF");
                                    cart.RuleResults.Add(result);
                                    cart.APFEdited = true;
                                    return result;
                                }
                            }
                        }
                    }
                }

                if (!APFDueProvider.CanAddAPF(distributorId))
                {
                    result.Result = RulesResult.Failure;
                    result.AddMessage(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CannotAddAPFSku") as string);
                    var sku = APFDueProvider.GetAPFSku();
                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.CannotAddAPFSku,
                                "CannotAddAPFSku");

                    cart.RuleResults.Add(result);
                    return result;
                }
                else
                {
                    string apfSku = APFDueProvider.GetAPFSku();
                    if (cart.CurrentItems[0].SKU == apfSku)
                    {
                        cart.APFEdited = true;
                    }
                    else
                    {
                        result.Result = RulesResult.Failure;
                        result.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "SkuNotValid") as string);
                        var sku = APFDueProvider.GetAPFSku();
                        SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.CannotAddAPFSku,
                                    "SkuNotValid");
                        cart.RuleResults.Add(result);
                        return result;
                    }
                }
            }

            if (APFDueProvider.IsAPFSkuPresent(cart.CartItems))
            {
                if (!APFDueProvider.CanEditAPFOrder(cart.DistributorID, Thread.CurrentThread.CurrentCulture.Name, level))
                {
                    result.Result = RulesResult.Failure;
                    result.AddMessage(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CompleteAPFPurchase") as
                        string);
                    var sku = APFDueProvider.GetAPFSku();
                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                "CompleteAPFPurchase");
                    cart.RuleResults.Add(result);
                    return result;
                }
                else
                {
                    if (APFDueProvider.containsOnlyAPFSku(cart.CartItems))
                    {
                        CatalogItem item = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                        if (null != item)
                        {
                            if (!HLConfigManager.Configurations.APFConfiguration.AllowNonProductItemsWithStandaloneAPF)
                            {
                                if (item.ProductType != ProductType.Product)
                                {
                                    result.Result = RulesResult.Failure;
                                    result.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "NoNonProductToStandaloneAPF") as string);

                                    var sku = APFDueProvider.GetAPFSku();
                                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.AllowNonProductItemsWithStandaloneAPF,
                                                "NoNonProductToStandaloneAPF");

                                    cart.RuleResults.Add(result);
                                    return result;
                                }
                            }
                            if (item.SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku ||
                                item.SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                            {
                                result.Result = RulesResult.Failure;
                                result.AddMessage(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "NoTodayMagazineWithStandalonAPF") as string);
                                var sku = APFDueProvider.GetAPFSku();
                                SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                            "NoTodayMagazineWithStandalonAPF");

                                cart.RuleResults.Add(result);
                                return result;
                            }

                            //To the standalone APF, DS trying to add product then change the Freight code & warehouse code.
                            else if (item.ProductType == ProductType.Product ||
                                     item.ProductType == ProductType.Literature ||
                                     item.ProductType == ProductType.PromoAccessory)
                            {
                                SetNonStandAloneAPFDeliveryOption(cart as MyHLShoppingCart);
                            }
                        }
                    }
                }
            }

            //Can add
            if (APFDueProvider.IsAPFSku(cart.CurrentItems[0].SKU))
            {
                if (cart.CurrentItems[0].Quantity < 0)
                {
                    var originaItem =
                        (from c in cart.CartItems where c.SKU == cart.CurrentItems[0].SKU select c).ToList();
                    int previousQuantity = originaItem[0].Quantity;
                    if (APFDueProvider.IsAPFDueAndNotPaid(distributorId, Locale))
                    {
                        int requiredQuantity = APFDueProvider.APFQuantityDue(distributorId, Locale);
                        if (level == "SP" || !HLConfigManager.Configurations.APFConfiguration.AllowDSRemoveAPFWhenDue)
                        {
                            if (cart.CurrentItems[0].Quantity + previousQuantity < requiredQuantity)
                            {
                                cart.CurrentItems[0].Quantity = (previousQuantity - requiredQuantity) * -1;
                                result.Result = RulesResult.Recalc;
                                result.AddMessage(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "RequiredAPFLeftInCart") as string);
                                var sku = APFDueProvider.GetAPFSku();
                                SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                            "RequiredAPFLeftInCart");

                                cart.RuleResults.Add(result);
                                cart.APFEdited = false;
                                return result;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private ShoppingCartRuleResult CartItemBeingDeletedRuleHandler(ShoppingCart_V01 cart,
                                                                       ShoppingCartRuleResult result,
                                                                       string level)
        {
            if (APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale) &&
                HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(
                    HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                        "CompleteAPFPurchase") as string);
                var sku = APFDueProvider.GetAPFSku();
                SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                            "CompleteAPFPurchase");

                cart.RuleResults.Add(result);
                return result;
            }

            //Deleting is a little different than Inserting, because delete handles multiple skus while insert only does one
            //so we can use the cart.CurrentItems to control this. Anything the rule says to not delete, we remove from the CurrentItems
            //Delete post-rule will only delete what is left in there
            bool isStandaloneAPF = APFDueProvider.hasOnlyAPFSku(cart.CartItems, Locale);
            var toRemove = new List<ShoppingCartItem_V01>();
            var hlCart = cart as MyHLShoppingCart;
            if (null != hlCart)
            {
                foreach (ShoppingCartItem_V01 item in cart.CurrentItems)
                {
                    if (APFDueProvider.IsAPFSku(item.SKU))
                    {
                        if (
                            !APFDueProvider.CanRemoveAPF(cart.DistributorID, Thread.CurrentThread.CurrentCulture.Name,
                                                         level))
                        {
                            result.Result = RulesResult.Failure;
                            result.AddMessage(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CannotRemoveAPFSku")
                                as string);
                            var sku = APFDueProvider.GetAPFSku();
                            SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                        "CannotRemoveAPFSku");

                            cart.RuleResults.Add(result);
                        }
                        else
                        {
                            if (APFDueProvider.IsAPFDueAndNotPaid(cart.DistributorID, Locale))
                            {
                                var originalItem =
                                    (from c in cart.CartItems where c.SKU == cart.CurrentItems[0].SKU select c).ToList();
                                int requiredQuantity = APFDueProvider.APFQuantityDue(cart.DistributorID, Locale);
                                if (level == "SP")
                                {
                                    item.Quantity = requiredQuantity;
                                    result.Result = RulesResult.Recalc;
                                    result.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "RequiredAPFLeftInCart") as string);
                                    var sku = APFDueProvider.GetAPFSku();
                                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                                "RequiredAPFLeftInCart");
                                    cart.RuleResults.Add(result);
                                    //toRemove.Add(item);
                                    cart.APFEdited = false;
                                    return result;
                                }

                                //Add for ChinaDO
                                if (HLConfigManager.Configurations.APFConfiguration.CantDSRemoveAPF)
                                {
                                    result.Result = RulesResult.Recalc;
                                    result.AddMessage(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "RequiredAPFLeftInCart") as string);
                                    var sku = APFDueProvider.GetAPFSku();
                                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                                "RequiredAPFLeftInCart");
                                    cart.RuleResults.Add(result);
                                    //toRemove.Add(item);
                                    cart.APFEdited = false;
                                    return result;
                                }
                            }
                            cart.APFEdited = true;
                            toRemove.Add(item); // added 
                        }
                    }
                    else
                    {
                        toRemove.Add(item); // added 
                    }
                }
                //if (toRemove.Count > 0)
                //{
                //    cart.CurrentItems.Remove(toRemove[0]);
                //}
                var remainingItems = (from c in cart.CartItems
                                      where !toRemove.Any(x => x.SKU == c.SKU)
                                      select c).ToList();

                //List<ShoppingCartItem_V01> remainingItems = (from c in cart.CartItems where c.SKU != cart.CurrentItems[0].SKU select c).ToList();
                var mags = CartHasOnlyTodaysMagazine(remainingItems);
                if (mags.Count > 0)
                {
                    foreach (ShoppingCartItem_V01 item in mags)
                    {
                        (cart as MyHLShoppingCart).DeleteTodayMagazine(item.SKU);
                        remainingItems.Remove(item);
                        if (!cart.CurrentItems.Exists(c => c.SKU == item.SKU))
                        {
                            cart.CurrentItems.Add(item);
                        }
                    }
                    result.Result = RulesResult.Failure;
                    result.AddMessage(
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                            "NoTodayMagazineWithStandalonAPF") as string);
                    var sku = APFDueProvider.GetAPFSku();
                    SetApfRuleResponse(result, ApfAction.None, sku, "ApfRule", TypeOfApf.StandaloneAPFOnlyAllowed,
                                "NoTodayMagazineWithStandalonAPF");
                    cart.RuleResults.Add(result);
                }

                // added
                if (remainingItems.Count > 0)
                {
                    var list = new ShoppingCartItemList();
                    list.AddRange(remainingItems);
                    isStandaloneAPF = APFDueProvider.hasOnlyAPFSku(list, Locale);
                    if (isStandaloneAPF)
                    {
                        SetAPFDeliveryOption(hlCart);
                    }
                    else
                    {
                        if (APFDueProvider.IsAPFSkuPresent(cart.CurrentItems))
                        {
                            SetNonStandAloneAPFDeliveryOption(hlCart);
                        }
                    }
                }
                else
                {
                    SetNonStandAloneAPFDeliveryOption(hlCart);
                }
            }
            //if((from c in cart.CurrentItems where APFDueProvider.IsAPFSku(c.SKU) select c).Count() >0)
            //{  
            //    if (isStandaloneAPF)
            //    {
            //        SetAPFDeliveryOption(hlCart);
            //    }
            //}

            return result;
        }

        #endregion

        #region helper methods

        private static int CalcQuantity(DateTime apfDue)
        {
            var now = DateTime.Now;
            var ts = now.Subtract(apfDue);
            return ((ts.Days / 365) + 1);
        }

        private ShoppingCartItem_V01 GetApfSku(List<ShoppingCartItem_V01> cartItems, string locale, string level)
        {
            ShoppingCartItem_V01 sku = null;
            var SKUs = APFDueProvider.GetAPFSkuList();
            if (SKUs != null && SKUs.Count == 2)
            {
                // if sku does not exist in the cart, add it
                // sku 1 is SP
                //string apfSKU = level == "SP" ? SKUs[1] : SKUs[0];
                string apfSKU = string.Empty;
                if (level == "SP")
                {
                    apfSKU = HLConfigManager.Configurations.APFConfiguration.SupervisorSku;
                }
                else
                {
                    apfSKU = HLConfigManager.Configurations.APFConfiguration.DistributorSku;
                }

                sku = new ShoppingCartItem_V01(0, apfSKU, 1, DateTime.Now);
            }
            return sku;
        }

        private void SetNonStandAloneAPFDeliveryOption(MyHLShoppingCart cart)
        {
            cart.CheckShippingForNonStandAloneAPF();
        }

        private void SetAPFDeliveryOption(MyHLShoppingCart cart)
        {
            cart.CheckAPFShipping();
        }

        private List<ShoppingCartItem_V01> CartHasOnlyTodaysMagazine(List<ShoppingCartItem_V01> items)
        {
            bool hasOnlyMagazine = false;
            var mags = new List<ShoppingCartItem_V01>();
            string primarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku;
            string secondarySku = HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku;
            int numMags = (from c in items where c.SKU == primarySku || c.SKU == secondarySku select c).Count();
            if (numMags > 0)
            {
                if ((from i in items where APFDueProvider.GetAPFSkuList().Contains(i.SKU) select i).Count() > 0)
                {
                    if (items.Count == numMags + 1)
                    {
                        hasOnlyMagazine = true;
                    }
                }
            }

            if (hasOnlyMagazine)
            {
                mags.AddRange((from i in items where i.SKU == primarySku || i.SKU == secondarySku select i).ToList());
            }

            return mags;
        }

        #endregion

        #endregion
    }
}
