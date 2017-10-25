using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits
{
    public class PurchasingLimitRuleBase : MyHerbalifeRule, IShoppingCartRule, IPurchasingLimitsRule
    {
        //protected OrderMonth _orderMonth ;

        public virtual Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            try
            {
                //Fetch these records from Web Service

                var purchasingLimitManager = PurchasingLimitManager(distributorId);
                if (PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId))
                {
                    purchasingLimitManager.PurchasingLimitsRestriction = PurchasingLimitRestrictionType.MarketingPlan;
                }
                int ordermonth = GetOrderMonth();

                purchasingLimitManager.SetPurchasingLimits(ordermonth);
                var currentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
                //Get the current Limits if the exist
                var storedLimits = PurchasingLimitProvider.GetPurchasingLimitsFromStore(Country,
                                                                                        distributorId);
                //Get the saved limits for the DS and country

                PurchasingLimits_V01 theLimits = null;
                bool shouldUpdateStore = false;
                if (null != currentLimits && null != storedLimits && storedLimits.Id > 0) //Decide if we use the stored limits
                {
                    //if (null == currentLimits)
                    //{
                    //    currentLimits = storedLimits;
                    //}
                    storedLimits.MaxEarningsLimit = currentLimits.MaxEarningsLimit;
                    storedLimits.maxVolumeLimit = currentLimits.maxVolumeLimit;

                    if (IsBlackoutPeriod() || storedLimits.OutstandingOrders > 0 ||
                        PurchasingLimitProvider.GetDistributorPurchasingLimitsSource(Country, distributorId) ==
                        DistributorPurchasingLimitsSourceType.InternetOrdering)
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType ==
                            PurchasingLimitRestrictionType.Quarterly)
                        {
                            var lastCutoff = GetLastQuarterlyCutoff();
                            if (storedLimits.LastRead < lastCutoff)
                            {
                                storedLimits = currentLimits;
                                if (null != storedLimits)
                                {
                                    PurchasingLimitProvider.UpdatePurchasingLimits(storedLimits, distributorId, true);
                                }
                                theLimits = storedLimits;
                            }
                        }
                        else
                        {
                            theLimits = storedLimits;
                            PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId);
                        }
                    }
                    else
                    {
                        theLimits = currentLimits;
                        shouldUpdateStore = true;
                    }
                }
                else
                {
                    theLimits = currentLimits;
                }

                if (null == theLimits) //We're bare and need the DS
                {
                    theLimits = new PurchasingLimits_V01();
                    var newLimits =
                        purchasingLimitManager.ReloadPurchasingLimits(ordermonth) as
                        PurchasingLimits_V01;

                    if (newLimits != null)
                    {
                        if (null != currentLimits) //if We're already init'ed resolve against current refreshed DS
                        {
                            if (currentLimits.RemainingVolume > newLimits.RemainingVolume)
                                currentLimits.RemainingVolume = newLimits.RemainingVolume;
                            if (currentLimits.RemainingEarnings > newLimits.RemainingEarnings)
                                currentLimits.RemainingEarnings = newLimits.RemainingEarnings;
                            currentLimits.MaxEarningsLimit = newLimits.MaxEarningsLimit;
                            currentLimits.maxVolumeLimit = newLimits.maxVolumeLimit;
                            theLimits = currentLimits;
                            theLimits.LastRead = DateTime.UtcNow;
                        }
                        else
                        {
                            //Probably first time in - refresh from DS.
                            theLimits.RemainingVolume = newLimits.RemainingVolume;
                            theLimits.RemainingEarnings = newLimits.RemainingEarnings;
                            theLimits.MaxEarningsLimit = newLimits.MaxEarningsLimit;
                            theLimits.maxVolumeLimit = newLimits.maxVolumeLimit;
                            theLimits.LastRead = DateTime.UtcNow;
                        }
                    }
                }

                if (null == storedLimits || shouldUpdateStore)
                {
                    PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId, true);
                }
                else
                {
                    PurchasingLimitProvider.UpdatePurchasingLimits(theLimits, distributorId,
                                                                   ordermonth);
                }
                var theCurrentLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
                var limitsType = PurchaseLimitType.Volume;

                if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType ==
                    PurchasingLimitRestrictionType.MarketingPlan && theCurrentLimits.maxVolumeLimit < 0)
                {
                    limitsType = PurchaseLimitType.None;
                }

                if (DistributorIsExemptFromPurchasingLimits(distributorId) &&
                    PurchasingLimitManager(distributorId).PurchasingLimitsRestriction !=
                    PurchasingLimitRestrictionType.MarketingPlan)
                {
                    limitsType = PurchaseLimitType.None;
                }
                else
                {
                    limitsType = PurchaseLimitType.Volume;
                }
                purchasingLimitManager.PurchasingLimits.Values.AsQueryable()
                                      .ToList()
                                      .ForEach(pl => pl.PurchaseLimitType = limitsType);

                return purchasingLimitManager.PurchasingLimits;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Messages:{0},StackTrace:{1},Locale:{2},DistributorId{3},Purchasing Months:{4}",
                        ex.Message, ex.StackTrace, Locale, distributorId,GetOrderMonth()));
                return null;
            }
        }

        public virtual int GetOrderMonth()
        {
            return PurchasingLimitProvider.GetOrderMonth();
        }
        public virtual bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            return !PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId);
        }

        public virtual bool PurchasingLimitsAreExceeded(string distributorId)
        {
            bool exceeded = false;
            if (!PurchasingLimitProvider.RequirePurchasingLimits(distributorId, Country))
                return exceeded;

            var cart = ShoppingCartProvider.GetShoppingCart(distributorId, Locale, true);

            PurchasingLimits_V01 limits = null;
            if (HLConfigManager.Configurations.DOConfiguration.SaveDSSubType)
            {
                limits = PurchasingLimitProvider.GetPurchasingLimits(distributorId, cart.SelectedDSSubType);
            }
            else
            {
                limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
            }
            if (null != cart && null != cart.Totals && cart.OrderCategory != ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
            {
                bool restrictedByMarketingPlan = PurchasingLimitProvider.IsRestrictedByMarketingPlan(distributorId);
                if ((null != limits && (limits.PurchaseLimitType != PurchaseLimitType.None) ||
                     restrictedByMarketingPlan))
                {
                    if (limits.PurchaseType != OrderPurchaseType.Consignment ||
                        (limits.PurchaseType == OrderPurchaseType.Consignment && cart.OrderSubType == "B1"))
                    {
                        if (restrictedByMarketingPlan)
                        {
                            limits.PurchaseLimitType = (limits.maxVolumeLimit == -1)
                                                           ? PurchaseLimitType.None
                                                           : PurchaseLimitType.Volume;
                        }
                        else
                        {
                            limits.PurchaseLimitType = (limits.maxVolumeLimit == -1)
                                                           ? PurchaseLimitType.None
                                                           : limits.PurchaseLimitType;
                        }

                        switch (limits.PurchaseLimitType)
                        {
                            case PurchaseLimitType.Earnings:
                                {
                                    exceeded = ((limits.RemainingEarnings - cart.ProductEarningsInCart) < 0);
                                    break;
                                }
                            case PurchaseLimitType.Volume:
                                {
                                    exceeded = ((limits.RemainingVolume - cart.VolumeInCart) < 0);
                                    break;
                                }
                            case PurchaseLimitType.DiscountedRetail:
                                {
                                    exceeded = ((limits.RemainingVolume - cart.ProductDiscountedRetailInCart) < 0);
                                    break;
                                }
                            case PurchaseLimitType.TotalPaid:
                                {
                                    if (null != cart.Totals)
                                    {
                                        exceeded = ((limits.RemainingVolume - (cart.Totals as OrderTotals_V01).AmountDue) < 0);
                                    }
                                    break;
                                }
                        }
                    }
                }
                //else if (ods.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan)
                //{
                //    exceeded = ((limits.RemainingVolume - cart.VolumeInCart) < 0);
                //}
            }

            return exceeded;
        }

        public ShoppingCartRuleResult checkVolumeLimits(MyHLShoppingCart cart, ShoppingCartRuleResult Result, string Locale, string Country)
        {
            decimal DistributorRemainingVolumePoints = 0;
            decimal NewVolumePoints = 0;

            IPurchasingLimitManagerFactory purchasingLimitManagerFactory = new PurchasingLimitManagerFactory();
            var purchasingLimitManager = purchasingLimitManagerFactory.GetPurchasingLimitManager(cart.DistributorID);

            var purchasingLimits =
                PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);

            if (null == purchasingLimits)
            {
                LoggerHelper.Error(
                    string.Format("{0} PurchasingLimits could not be retrieved for distributor in checkVolumeLimits {1}", Locale,
                                  cart.DistributorID));
                Result.Result = RulesResult.Failure;
                return Result;
            }

            purchasingLimitManager.SetPurchasingLimits(purchasingLimits);

            DistributorRemainingVolumePoints = purchasingLimits.RemainingVolume;

            var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
            if (currentItem != null)
            {
                if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                {
                    PurchasingLimitProvider.GetPurchasingLimits(cart.DistributorID, "ETO");
                }
                else
                {
                    NewVolumePoints = currentItem.VolumePoints * cart.CurrentItems[0].Quantity;
                }
            }

            if (NewVolumePoints > 0)
            {
                if (purchasingLimits.maxVolumeLimit == -1)
                {
                    return Result;
                }
                decimal cartVolume = cart.VolumeInCart;

                if (DistributorRemainingVolumePoints - (cartVolume + NewVolumePoints) < 0)
                {
                    Result.Result = RulesResult.Failure;
                    var orderMonth = new OrderMonth(Country);
                    var msg = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform) ?? string.Empty, "VolumePointExceedsOnOrderMonth") as string;
                    msg = string.Format(msg,orderMonth.CurrentOrderMonth.ToString("MM-yyyy"), DistributorRemainingVolumePoints);
                    Result.AddMessage(msg);
                    var globalResourceObject =
                                   HttpContext.GetGlobalResourceObject(
                                       string.Format("{0}_Rules", HLConfigManager.Platform),
                                       "DisgardCommonMessage");
                    if (globalResourceObject != null)
                        Result.AddMessage(globalResourceObject.ToString());
                    cart.RuleResults.Add(Result);
                }
            }
            return Result;
        }
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingLimits Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        /// <summary>
        ///     The IShoppingCart Rule Interface implementation
        /// </summary>
        /// <param name="cart">The current Shopping Cart</param>
        /// <param name="reason">The Rule invoke Reason</param>
        /// <param name="Result">The Rule Results collection</param>
        /// <returns>The cumulative rule results - including the results of this iteration</returns>
        protected virtual ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var purchasingLimitManager = PurchasingLimitManager(cart.DistributorID);
                decimal DistributorRemainingVolumePoints = 0;
                decimal DistributorRemainingDiscountedRetail = 0;
                //decimal CartVolumePoints = 0;
                decimal NewVolumePoints = 0;

                decimal DistributorRemainingEarnings = 0;
                //decimal CartEarnings = 0;
                decimal NewEarnings = 0;

                decimal Discount = 0;

                var myhlCart = cart as MyHLShoppingCart;

                if (null == myhlCart)
                {
                    LoggerHelper.Error(
                        string.Format("{0} myhlCart is null {1}", Locale, cart.DistributorID));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                PurchasingLimits_V01 PurchasingLimits =
                    PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);

                purchasingLimitManager.SetPurchasingLimits(PurchasingLimits);


                if (null == PurchasingLimits)
                {
                    LoggerHelper.Error(
                        string.Format("{0} PurchasingLimits could not be retrieved for distributor {1}", Locale,
                                      cart.DistributorID));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }

                DistributorRemainingVolumePoints =
                    DistributorRemainingDiscountedRetail = PurchasingLimits.RemainingVolume;
                DistributorRemainingEarnings = PurchasingLimits.RemainingEarnings;

                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID,
                                                                                               Country);
                Discount = Convert.ToDecimal(distributorOrderingProfile.StaticDiscount);


                if (myhlCart.Totals != null && (myhlCart.Totals as OrderTotals_V01).DiscountPercentage != 0.0M)
                {
                    Discount = (myhlCart.Totals as OrderTotals_V01).DiscountPercentage;
                }

                myhlCart.SetDiscountForLimits(Discount);

                var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                if (currentItem != null)
                {
                    if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.EventTicket)
                    {
                        PurchasingLimitProvider.GetPurchasingLimits(cart.DistributorID, "ETO");
                    }
                    else
                    {
                        NewVolumePoints = currentItem.VolumePoints*cart.CurrentItems[0].Quantity;
                    }
                }

                if (NewVolumePoints > 0 || PurchasingLimits.PurchaseLimitType == PurchaseLimitType.Earnings || 
                    purchasingLimitManager.PurchasingLimitsRestriction == PurchasingLimitRestrictionType.MarketingPlan)
                {
                    if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.Volume)
                    {
                        if (PurchasingLimits.maxVolumeLimit == -1)
                        {
                            return Result;
                        }
                        decimal cartVolume = (cart as MyHLShoppingCart).VolumeInCart;

                        if (DistributorRemainingVolumePoints - (cartVolume + NewVolumePoints) < 0)
                        {
                            Result.Result = RulesResult.Failure;
                            if (purchasingLimitManager.PurchasingLimitsRestriction ==
                                PurchasingLimitRestrictionType.MarketingPlan)
                                //MPE Thresholds
                            {
                                if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsThresholdByIncreasingQuantity").ToString(),
                                            cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                                }
                                else
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsThreshold").ToString(), cart.CurrentItems[0].SKU));
                                }
                            }
                            else
                            {
                                if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsByIncreasingQuantity").ToString(),
                                            cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                                }
                                else
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceeds").ToString(), cart.CurrentItems[0].SKU));
                                }
                            }
                            cart.RuleResults.Add(Result);
                        }
                    }
                    else if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.DiscountedRetail)
                    {
                        var myHlCart = cart as MyHLShoppingCart;
                        var calcTheseItems = new List<ShoppingCartItem_V01>(cart.CartItems);
                        var existingItem = cart.CartItems.Find(ci => ci.SKU == cart.CurrentItems[0].SKU);
                        if (null != existingItem)
                        {
                            existingItem.Quantity += cart.CurrentItems[0].Quantity;
                        }
                        else
                        {
                            calcTheseItems.Add(new ShoppingCartItem_V01(0, cart.CurrentItems[0].SKU,
                                                                        cart.CurrentItems[0].Quantity, DateTime.Now));
                        }
                        var totals = myhlCart.Calculate(calcTheseItems) as OrderTotals_V01;
                        if (null != existingItem)
                        {
                            existingItem.Quantity -= cart.CurrentItems[0].Quantity;
                        }
                        if (null == totals)
                        {
                            var message =
                                string.Format(
                                    "Purchasing Limits DiscountedRetail calculation failed due to Order Totals returning a null for Distributor {0}",
                                    cart.DistributorID);
                            LoggerHelper.Error(message);
                            throw new ApplicationException(message);
                        }
                        decimal newDiscountedRetail = 0.0M;
       
                        decimal dsicount = totals.DiscountPercentage;

                        var skus = (from s in calcTheseItems where s.SKU != null select s.SKU).ToList();
                        var allItems = CatalogProvider.GetCatalogItems(skus, Country);
                        if (null != allItems && allItems.Count > 0)
                        {
                            newDiscountedRetail = (from t in totals.ItemTotalsList
                                                   from c in allItems.Values
                                                   where
                                                       (c as CatalogItem_V01).ProductType == ServiceProvider.CatalogSvc.ProductType.Product &&
                                                       c.SKU == (t as ItemTotal_V01).SKU
                                                   select (t as ItemTotal_V01).DiscountedPrice).Sum();
                        }

                        myhlCart.SetDiscountForLimits(totals.DiscountPercentage);

                        if (DistributorRemainingDiscountedRetail - newDiscountedRetail < 0)
                        {
                            Result.Result = RulesResult.Failure;
                            if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "DiscountedRetailExceedsByIncreasingQuantity").ToString(),
                                        cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                            }
                            else
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "DiscountedRetailExceeds").ToString(), cart.CurrentItems[0].SKU));
                            }
                            cart.RuleResults.Add(Result);
                        }
                    }
                    else if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.Earnings &&
                             cart.OrderSubType == "B1")
                    {
                        NewEarnings += (currentItem.EarnBase) * cart.CurrentItems[0].Quantity * Discount / 100;

                        if (DistributorRemainingEarnings -
                            ((cart as MyHLShoppingCart).ProductEarningsInCart + NewEarnings) < 0)
                        {
                            Result.Result = RulesResult.Failure;
                            if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "EarningExceedsByIncreasingQuantity").ToString(),
                                        cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                            }
                            else
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform), "EarningExceeds")
                                                   .ToString(), cart.CurrentItems[0].SKU));
                            }
                            cart.RuleResults.Add(Result);
                        }
                    }
                    else if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.ProductCategory)
                    {
                        if (currentItem.ProductType == ServiceProvider.CatalogSvc.ProductType.Product)
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                        "PurchaseLimitTypeProductCategory").ToString(), cart.CurrentItems[0].SKU));
                            cart.RuleResults.Add(Result);
                        }
                        else
                        {
                            if (DistributorRemainingVolumePoints -
                                ((cart as MyHLShoppingCart).VolumeInCart + NewVolumePoints) < 0)
                            {
                                Result.Result = RulesResult.Failure;
                                if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsThresholdByIncreasingQuantity").ToString(),
                                            cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                                }
                                else
                                {
                                    Result.AddMessage(
                                        string.Format(
                                            HttpContext.GetGlobalResourceObject(
                                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                                "VolumePointExceedsThreshold").ToString(), cart.CurrentItems[0].SKU));
                                }
                            }
                        }
                    }
                    else if (PurchasingLimits.PurchaseLimitType == PurchaseLimitType.TotalPaid)
                    {
                        Result.Result = RulesResult.Success;

                        var myHlCart = cart as MyHLShoppingCart;
                        var calcTheseItems = new List<ShoppingCartItem_V01>();
                        calcTheseItems.AddRange(from i in cart.CartItems
                                                where !APFDueProvider.IsAPFSku(i.SKU)
                                                select
                                                    new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                             i.MinQuantity));

                        var existingItem =
                            calcTheseItems.Find(ci => ci.SKU == cart.CurrentItems[0].SKU);
                        if (null != existingItem)
                        {
                            existingItem.Quantity += cart.CurrentItems[0].Quantity;
                        }
                        else
                        {
                            calcTheseItems.Add(new ShoppingCartItem_V01(0, cart.CurrentItems[0].SKU,
                                                                        cart.CurrentItems[0].Quantity, DateTime.Now));
                        }

                        // remove A and L type
                        var allItems =
                            CatalogProvider.GetCatalogItems(
                                (from s in calcTheseItems where s.SKU != null select s.SKU).ToList(), Country);
                        if (null != allItems && allItems.Count > 0)
                        {
                            var skuExcluded = (from c in allItems.Values
                                               where (c as CatalogItem_V01).ProductType != ServiceProvider.CatalogSvc.ProductType.Product
                                               select c.SKU);
                            calcTheseItems.RemoveAll(s => skuExcluded.Contains(s.SKU));
                        }

                        var totals = myhlCart.Calculate(calcTheseItems);
                        if (null == totals)
                        {
                            var message =
                                string.Format(
                                    "Purchasing Limits DiscountedRetail calculation failed due to Order Totals returning a null for Distributor {0}",
                                    cart.DistributorID);
                            LoggerHelper.Error(message);
                            throw new ApplicationException(message);
                        }
                        if (DistributorRemainingVolumePoints - (totals as OrderTotals_V01).AmountDue < 0)
                        {
                            Result.Result = RulesResult.Failure;
                            if (cart.CartItems.Exists(item => item.SKU == cart.CurrentItems[0].SKU))
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "DiscountedRetailExceedsByIncreasingQuantity").ToString(),
                                        cart.CurrentItems[0].SKU, cart.CurrentItems[0].Quantity));
                            }
                            else
                            {
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "DiscountedRetailExceeds").ToString(), cart.CurrentItems[0].SKU));
                            }
                            cart.RuleResults.Add(Result);
                        }
                    }
                    else
                    {
                        Result.Result = RulesResult.Success;
                    }
                }
            }

            return Result;
        }

        /// <summary>Determine whether the current time is in a non-NTS period</summary>
        /// <returns></returns>
        protected virtual bool IsBlackoutPeriod()
        {
            return false;
        }

        private DateTime GetLastQuarterlyCutoff()
        {
            int currentOrderMonth = PurchasingLimitProvider.GetOrderMonth();
            var lastCutoff = DateTime.MinValue;
            int month = int.Parse(currentOrderMonth.ToString().Substring(4));
            int year = int.Parse(currentOrderMonth.ToString().Substring(0, 4));
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    {
                        lastCutoff = new DateTime(year - 1, 12, 31, 23, 59, 59);
                        break;
                    }
                case 4:
                case 5:
                case 6:
                    {
                        lastCutoff = new DateTime(year, 3, 31, 23, 59, 59);
                        break;
                    }
                case 7:
                case 8:
                case 9:
                    {
                        lastCutoff = new DateTime(year, 6, 30, 23, 59, 59);
                        break;
                    }
                case 10:
                case 11:
                case 12:
                    {
                        lastCutoff = new DateTime(year, 9, 30, 23, 59, 59);
                        break;
                    }
            }

            return lastCutoff;
        }

        public IPurchasingLimitManager PurchasingLimitManager(string id)
        {
            IPurchasingLimitManagerFactory purchasingLimitManagerFactory = new PurchasingLimitManagerFactory();
            return purchasingLimitManagerFactory.GetPurchasingLimitManager(id);
        }
    }
}
