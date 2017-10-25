using HL.Common.Configuration;
using HL.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider;

namespace MyHerbalife3.Ordering.Providers.FOP
{
    public class PurchaseRestrictionManager : IPurchaseRestrictionManager
    {
        private readonly string _distributorId;
        private readonly string _distributorLevel;
        private readonly string _processingCountry;
        private string _currentLoggedInCountry;
        private string _locale;
        private List<PurchasingLimits> _applicableLimits;

        #region IPurchaseRestrictionManager

        /// <summary>
        /// when there is no limits in cache or when cache expires.
        /// </summary>
        /// <param name="distributorId"></param>
        /// <param name="currentLoggedInCountry"></param>
        /// <param name="distributorLevel"></param>
        /// <param name="processingCountry"></param>
        /// <param name="locale"></param>
        public PurchaseRestrictionManager(string distributorId, string currentLoggedInCountry, string distributorLevel,
                                      string processingCountry, string locale, List<TaxIdentification> tins, string orderSubType)
        {
            _distributorId = distributorId;
            _currentLoggedInCountry = currentLoggedInCountry;
            _distributorLevel = distributorLevel;
            _processingCountry = processingCountry;
            _locale = locale;

            Tins = tins == null ? new List<TaxIdentification>() : tins;
            OrderSubType = orderSubType;
            CanPurchase = CanPurchasePType = true;

            LoadLimits();
            SetPurchasingLimits();
            // when cache expires, call rules to set parameters 
            // SetPurchaseRestriction();

        }

        public Dictionary<int, PurchasingLimits_V01> PurchasingLimits { get; set; }

        public List<PurchasingLimits> ApplicableLimits { get { return this._applicableLimits; } set { this._applicableLimits = value; } }
        public bool CanPurchasePType { get; set; }

        public bool ExtendRestrictionErrorMessage { get; set; }
        public bool CanPurchase { get; set; }
        public List<TaxIdentification> Tins { get; set; }
        public string OrderSubType { get; set; }

        /// <summary>
        /// SetPurchaseRestriction
        /// </summary>
        public void SetPurchaseRestriction()
        {
            int orderMonth = GetOrderMonth();
            if (orderMonth != 0)
            {
                HLRulesManager.Manager.SetPurchaseRestriction(Tins, orderMonth, _distributorId, this);
            }
        }

        public string DistributorID
        {
            get
            {
                return _distributorId;
            }
        }
        public string LoginCountry
        {
            get
            {
                return _currentLoggedInCountry;
            }
        }

        /// <summary>
        /// GetPurchasingLimits
        /// </summary>
        /// <param name="orderMonth"></param>
        /// <returns></returns>
        public PurchasingLimits_V01 GetPurchasingLimits(int orderMonth)
        {
            PurchasingLimits_V01 limit = null;
            if (PurchasingLimits != null)
            {
                PurchasingLimits.TryGetValue(orderMonth, out limit);
            }
            return limit;
        }

        /// <summary>
        /// after order submitted, update DB
        /// </summary>
        /// <param name="limits"></param>
        /// <param name="orderMonth"></param>
        public void UpdatePurchasingLimits(PurchasingLimits_V01 limits)
        {
            WritePurchasingLimitsToDB(new List<PurchasingLimits> { limits });
            SaveToCache();
        }

        public void ReloadPurchasingLimits(int orderMonth)
        {
            DistributorPurchasingLimitsSourceType source = GetDistributorPurchasingLimitsSource(_currentLoggedInCountry, _distributorId);
            if (source == DistributorPurchasingLimitsSourceType.HMS)
            {
                var HMSLimits = LoadPurchasingLimitsFromDB(DistributorPurchasingLimitsSourceType.HMS);
                if (HMSLimits.Any())
                {
                    _applicableLimits = HMSLimits;
                    WritePurchasingLimitsToDB(HMSLimits);
                    SaveToCache();
                }
            }
        }

        public bool PurchasingLimitsAreExceeded(string distributorId, MyHLShoppingCart cart)
        {
            bool exceeded = false;
            if (cart.OrderCategory == OrderCategoryType.HSO)
                return exceeded;
            if (CanPurchase)
            {
                // PurchasingLimits_V01 limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(distributorId);
                PurchasingLimits_V01 limits = GetPurchasingLimits(intOrderMonth(OrderMonth.GetCurrentOrderMonth()));
                if (limits == null || cart == null)
                    return exceeded;
                
                if (null != cart.Totals && cart.OrderCategory == OrderCategoryType.ETO)
                    return exceeded;

                if (null != limits && limits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits && limits.PurchaseLimitType != PurchaseLimitType.None)
                {

                    switch (limits.PurchaseLimitType)
                    {
                        case PurchaseLimitType.Earnings:
                            {
                                exceeded = limits.NextMaxEarningsLimit > -1 && ((limits.RemainingEarnings - cart.ProductEarningsInCart) < 0);
                                break;
                            }
                        case PurchaseLimitType.Volume:
                            {
                                exceeded = limits.maxVolumeLimit > -1 && ((limits.RemainingVolume - cart.ProductVolumeInCart) < 0);
                                break;
                            }
                        case PurchaseLimitType.DiscountedRetail:
                            {
                                exceeded = limits.maxVolumeLimit > -1 && ((limits.RemainingVolume - cart.ProductDiscountedRetailInCart) < 0);
                                break;
                            }
                        case PurchaseLimitType.TotalPaid:
                            {
                                if (null != cart.Totals)
                                {
                                    if (APFDueProvider.containsOnlyAPFSku(cart.ShoppingCartItems))
                                    {
                                        return exceeded;
                                    }

                                    exceeded = limits.maxVolumeLimit > -1 && ((limits.RemainingVolume - (cart.Totals as OrderTotals_V01).AmountDue) < 0);
                                }
                                break;
                            }
                    }

                }
                if (null != limits && limits.LimitsRestrictionType != LimitsRestrictionType.PurchasingLimits)
                {
                    exceeded = ((limits.RemainingVolume - cart.VolumeInCart) < 0);
                    if (exceeded)
                    {
                        cart.RuleResults = new List<ShoppingCartRuleResult>();
                        ShoppingCartRuleResult Result = new ShoppingCartRuleResult();
                        Result.RuleName = "PurchaseRestriction Rules";
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(
                                                        string.Format("{0}_Rules", HLConfigManager.Platform),
                                                        "FOPVolumePointExceedsOnCheckout").ToString(), 1100, PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(_processingCountry), PurchaseRestrictionProvider.GetThresholdPeriod(_processingCountry), cart.VolumeInCart - limits.RemainingVolume));
                        cart.RuleResults.Add(Result);
                    }
                }
                else if (exceeded)
                {
                    cart.RuleResults = new List<ShoppingCartRuleResult>();
                    ShoppingCartRuleResult Result = new ShoppingCartRuleResult();
                    Result.RuleName = "PurchaseRestriction Rules";
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_Rules", HLConfigManager.Platform),
                                                    "RemainingVolumePointExceeds").ToString(), (cart.VolumeInCart - limits.RemainingVolume)));
                    cart.RuleResults.Add(Result);
                }
            }
            else  //member can't purchase
            {
                if ((cart.Totals as OrderTotals_V01).AmountDue > 0) //can't purchase and there are items in cart
                {
                    exceeded = true;
                    cart.RuleResults = new List<ShoppingCartRuleResult>();
                    ShoppingCartRuleResult Result = new ShoppingCartRuleResult();
                    Result.RuleName = "PurchaseRestriction Rules";
                    Result.Result = RulesResult.Failure;
                    Result.AddMessage(string.Format(HttpContext.GetGlobalResourceObject(
                                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                    "CantBuy").ToString(), 1100, cart.DistributorID));
                    cart.RuleResults.Add(Result);
                }
            }
            return exceeded;
        }

        public void ReconcileAfterPurchaseForeLearning(MyHLShoppingCart shoppingCart)
        {
            foreach (var l in this.PurchasingLimits.Keys)
            {
                var currentLimits = PurchasingLimits[l];
                if (currentLimits.LimitsRestrictionType == LimitsRestrictionType.FOP)
                {
                    currentLimits.maxVolumeLimit = HLConfigManager.Configurations.ShoppingCartConfiguration.eLearningMaxPPV;
                    currentLimits.RemainingVolume -= currentLimits.maxVolumeLimit - shoppingCart.VolumeInCart;
                    if (!hasValidDate(currentLimits.FirstOrderDate) && shoppingCart.VolumeInCart > 0) // this is just to indicate first order just placed
                    {
                        currentLimits.FirstOrderDate = HL.Common.Utilities.DateUtils.ConvertToCurrentLocalTime(DateTime.Now, this._currentLoggedInCountry);
                        currentLimits.Expiry = AbsoluteEnd(currentLimits.FirstOrderDate.AddDays(currentLimits.ThresholdPeriod == 0 ? PurchaseRestrictionProvider.GetThresholdPeriod(_processingCountry) - 1 : currentLimits.ThresholdPeriod)); /// one for now
                    }
                }
                else if (currentLimits.LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits)
                {
                    currentLimits.RemainingVolume -= shoppingCart.ProductVolumeInCart;
                }
                UpdatePurchasingLimits(currentLimits);
            }
        }

        public void ReconcileAfterPurchase(MyHLShoppingCart shoppingCart, string distributorId, string countryCode)
        {
            if (null != shoppingCart)
            {
                if (HLConfigManager.Configurations.ShoppingCartConfiguration.CheckELearning)
                {
                    if (PurchaseRestrictionProvider.RequireTraining(distributorId, shoppingCart.Locale, countryCode))
                    {
                        ReconcileAfterPurchaseForeLearning(shoppingCart);
                        return;
                    }
                }

                var currentLimits = GetPurchasingLimits(shoppingCart.OrderMonth);

                if (null != currentLimits)
                {
                    // orderMonth : 1408
                    if (currentLimits.LimitsRestrictionType == LimitsRestrictionType.FOP || currentLimits.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold)
                    {
                        bool placingFirstOrder = false;
                        DateTime currentDateTime = HL.Common.Utilities.DateUtils.ConvertToCurrentLocalTime(DateTime.Now, this._currentLoggedInCountry);
                        currentLimits.RemainingVolume -= shoppingCart.VolumeInCart;
                        if (!hasValidDate(currentLimits.FirstOrderDate) && shoppingCart.VolumeInCart > 0) // this is just to indicate first order just placed
                        {
                            placingFirstOrder = true;
                            currentLimits.maxVolumeLimit = PurchaseRestrictionProvider.GetVolumeLimitsAfterFirstOrderFOP(_processingCountry);
                            currentLimits.RemainingVolume = currentLimits.maxVolumeLimit - shoppingCart.VolumeInCart;
                            //currentLimits.FirstOrderDate = currentDateTime = HL.Common.Utilities.DateUtils.ConvertToCurrentLocalTime(DateTime.Now, this._currentLoggedInCountry);
                            currentLimits.Expiry = AbsoluteEnd(currentLimits.FirstOrderDate.AddDays(currentLimits.ThresholdPeriod == 0 ? PurchaseRestrictionProvider.GetThresholdPeriod(_processingCountry) - 1 : currentLimits.ThresholdPeriod)); /// one for now
                        }
                        if (PurchasingLimits != null)
                        {
                            foreach (var p in PurchasingLimits)
                            {
                                if (p.Value.LimitsRestrictionType == LimitsRestrictionType.FOP)
                                {
                                    p.Value.RemainingVolume = currentLimits.RemainingVolume;
                                    if (placingFirstOrder)
                                    {
                                        p.Value.FirstOrderDate = currentDateTime;
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        switch (currentLimits.PurchaseLimitType)
                        {
                            case PurchaseLimitType.Earnings:
                                {
                                    currentLimits.RemainingEarnings -= shoppingCart.ProductEarningsInCart;
                                    break;
                                }
                            case PurchaseLimitType.Volume:
                                {
                                    if (currentLimits.RestrictionPeriod != PurchasingLimitRestrictionPeriod.PerOrder)
                                    {
                                        currentLimits.RemainingVolume -= HLConfigManager.Configurations.DOConfiguration.UseTotalVolumeToReconciliation ? shoppingCart.ProductPromoVolumeInCart : shoppingCart.ProductVolumeInCart;
                                    }
                                    break;
                                }
                            case PurchaseLimitType.DiscountedRetail:
                                {
                                    currentLimits.RemainingVolume -= shoppingCart.ProductDiscountedRetailInCart;
                                    break;
                                }
                            case PurchaseLimitType.TotalPaid:
                                {
                                    currentLimits.RemainingVolume -= (shoppingCart.Totals as OrderTotals_V01).AmountDue;
                                    break;
                                }
                        }
                    }

                    UpdatePurchasingLimits(currentLimits);
                   // ExpireCache();
                }
            }
            else
            {
                LoggerHelper.Error(
                    string.Format(
                        "PurchaseRestrictionManager.ReconcileAfterPurchase was passed a null cart and couldn't update the cached limits for Distributor: {0}, Country: {1}",
                        distributorId, countryCode));
            }
        }

        #endregion IPurchaseRestrictionManager

        #region public method
        public List<PurchasingLimits> GetApplicableLimits()
        {
            return this._applicableLimits;
        }
        #endregion public method

        #region private method
        private void SaveToCache()
        {
            var purchasingLimitKey = PurchaseRestrictionManagerFactory.GetCacheKey(_distributorId, _currentLoggedInCountry);
            CacheFactory.Create().Add(this, purchasingLimitKey, TimeSpan.FromMinutes(Settings.GetRequiredAppSetting(
                                                                                      "PurchasingLimitsCacheMinutes",
                                                                                      30)));
        }

        private void ExpireCache()
        {
            CacheFactory.Create().Expire(typeof(PurchaseRestrictionManager), PurchaseRestrictionManagerFactory.GetCacheKey(_distributorId, _currentLoggedInCountry));
            string cacheKey = string.Format("{0}_{1}_{2}", "FOP_PLSOURCE", _currentLoggedInCountry, _distributorId);
            string runtimeCache = HttpRuntime.Cache[cacheKey] as string;
            if (runtimeCache != null)
                HttpRuntime.Cache.Remove(cacheKey);
        }

        private int intOrderMonth(int orderMonth)
        {
            // 1408 + 200000 = 201408
            return (orderMonth + 200000);
        }

        /// <summary>
        /// query fusion if there is outstanding orders
        /// </summary>
        /// <param name="country"></param>
        /// <param name="distributorId"></param>
        /// <returns></returns>
        private DistributorPurchasingLimitsSourceType GetDistributorPurchasingLimitsSource(string country, string distributorId)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                return DistributorPurchasingLimitsSourceType.HMS;

            if (PurchaseRestrictionProvider.IsBlackoutPeriod(country))
            {
                return DistributorPurchasingLimitsSourceType.InternetOrdering;
            }

            var source = DistributorPurchasingLimitsSourceType.Unknown;
            string cacheKey = string.Format("{0}_{1}_{2}", "FOP_PLSOURCE", country, distributorId);
            string useFusion = HttpRuntime.Cache[cacheKey] as string;

            if (useFusion == null)
            {
                if (!string.IsNullOrEmpty(distributorId) && !string.IsNullOrEmpty(country))
                {
                    try
                    {
                        var proxy = ServiceClientProvider.GetOrderServiceProxy();
                        var request =
                            new GetDistributorPurchasingLimitsSourceRequest_V01();
                        request.DistributorID = distributorId;
                        request.CountryCode = country;
                        // RS exception in country for request
                        if (country.Equals("RS"))
                        {
                            request.CountryCode = HL.Common.ValueObjects.CountryType.RS.HmsCountryCodes.FirstOrDefault();
                        }

                        var response = proxy.GetDistributorPurchasingLimitsSource(new GetDistributorPurchasingLimitsSourceRequest(request)).GetDistributorPurchasingLimitsSourceResult as GetDistributorPurchasingLimitsSourceResponse_V01;
                        if (response.Status != MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                        {
                            LoggerHelper.Error(
                                string.Format("GetDistributorPurchasingLimitsSource failed for Distributor {0}, Country {1}, Status{2}",
                                              distributorId, country, response.Status));
                        }
                        if (null != response)
                        {
                            source = response.Source;
                            useFusion = source == DistributorPurchasingLimitsSourceType.HMS ? Boolean.TrueString : Boolean.FalseString;
                            HttpRuntime.Cache.Insert(cacheKey, useFusion, null, DateTime.Now.AddMinutes(15), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "Error retrieving OutstandingOrders Status from BPEL service for: DS:{0} - Country:{1}, {2}",
                                distributorId, country, ex));
                    }
                }
            }

            return source = string.IsNullOrEmpty(useFusion) ? DistributorPurchasingLimitsSourceType.InternetOrdering : (useFusion.Equals(Boolean.TrueString) ? DistributorPurchasingLimitsSourceType.HMS : DistributorPurchasingLimitsSourceType.InternetOrdering);
        }

        private bool hasValidDate(DateTime datetime)
        {
            return (datetime < DateTime.Now.AddYears(1));
        }
        /// <summary>
        /// to check if go ahead and load from fusion, BE VERY CAREFUL NOT TO CALL FUSION UNNECESSARILY 
        /// </summary>
        /// <param name="processingCountry"></param>
        /// <param name="distributorId"></param>
        /// <returns></returns>
        private bool checkSQLPurchasingLimits(string processingCountry, string level)
        {
            bool bCheckFusion = false;

            // current login country requires purchasing limits from fusion
            // GR, MY, PH, PE, PY, UY, VE, FR, PF, HR, BA, HU, IT, MK, MN, PL, BY, KZ, SI, RS, UA
            if (HLConfigManager.Configurations.DOConfiguration.GetPurchaseLimitsFromFusion)
            {
                return bCheckFusion = true;
            }

            // OT and FOP should be mutully exclusive!

            var applicableLimits = LoadPurchasingLimitsFromDB(DistributorPurchasingLimitsSourceType.InternetOrdering);

            // if DS's COP is from order threshold country
            // RU,TR,AM,GE,UA,BY,KZ,MN,IN,RO,IL,MO,HK,VN,TW,CN,
            if (PurchaseRestrictionProvider.OrderThresholdCountries.Contains(processingCountry))
            {
                if (applicableLimits != null && applicableLimits.Count > 0) // if  records in SQL
                {
                    var OT = applicableLimits.Where(l => ((PurchasingLimits_V01)l).LimitsRestrictionType == LimitsRestrictionType.OrderThreshold);
                    bCheckFusion = (OT.Any() && (OT.First() as PurchasingLimits_V01).Completed) ? false : true; // OT completed, return false
                    if (bCheckFusion)
                    {
                        bCheckFusion = level == "DS"; // OT only applicale for DS
                    }
                }
                else
                {
                    return bCheckFusion = level == "DS";
                }// no records in SQL
            }

            // if DS's COP is from FOP country
            // All except OT country and whose in NonFOPCountries
            if (!PurchaseRestrictionProvider.NonFOPCountries.Contains(processingCountry))
            {
                if (applicableLimits != null && applicableLimits.Count > 0) // if  records in SQL
                {
                    var FOP = applicableLimits.Where(l => ((PurchasingLimits_V01)l).LimitsRestrictionType == LimitsRestrictionType.FOP);
                    bCheckFusion = (FOP.Any() && (FOP.First() as PurchasingLimits_V01).Completed) ? false : true; // FOP completed, return false
                    if (bCheckFusion)
                    {
                        bCheckFusion = level == "DS"; // FOP only applicale for DS
                    }
                }
                else
                    return bCheckFusion = level == "DS";
            }

            // AR, SG, TH - limits per order, no need Fusion access

            return bCheckFusion;
        }
        /// <summary>
        /// check InternetOrdering DB first, if DS's limits not met, check HMS -- to reduce calls
        /// </summary>
        private void LoadLimits()
        {
            try
            {
                bool bCheckFusion = checkSQLPurchasingLimits(_processingCountry, _distributorLevel);
                if (bCheckFusion)
                {
                    this._applicableLimits = null;

                    DistributorPurchasingLimitsSourceType source = GetDistributorPurchasingLimitsSource(_currentLoggedInCountry, _distributorId);
                    if (source == DistributorPurchasingLimitsSourceType.HMS)
                    {
                        this._applicableLimits = LoadPurchasingLimitsFromDB(DistributorPurchasingLimitsSourceType.HMS);
                    }
                    if (this._applicableLimits == null)
                    {
                        this._applicableLimits = LoadPurchasingLimitsFromDB(source = DistributorPurchasingLimitsSourceType.InternetOrdering);
                    }

                    if (this._applicableLimits != null)
                    {
                        foreach (var al in _applicableLimits)
                        {
                            if ((al as PurchasingLimits_V01).RemainingVolume < -1)
                                (al as PurchasingLimits_V01).RemainingVolume = 0;
                        }
                    }

                    // if no SQL data to start with?
                    if (source == DistributorPurchasingLimitsSourceType.InternetOrdering)
                    {
                        if(_applicableLimits == null || _applicableLimits.Count() == 0 || (HLConfigManager.Configurations.DOConfiguration.GetPurchaseLimitsFromFusion && _applicableLimits.Where(l => (l as PurchasingLimits_V01).LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits).Count() == 1 ))
                        {
                            this._applicableLimits = LoadPurchasingLimitsFromDB(DistributorPurchasingLimitsSourceType.HMS);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HL.Common.Logging.LoggerHelper.Error(
                    string.Format(
                        "Error LoadLimits in PurchaseRestrictionManager for: DS:{0} - Country:{1}, {2}", _distributorId, _currentLoggedInCountry, ex));
            }
        }

        /// <summary>
        /// load limits from Internet or HMS, return two months limits
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private List<PurchasingLimits> LoadPurchasingLimitsFromDB(DistributorPurchasingLimitsSourceType source)
        {
            List<PurchasingLimits> limits = new List<PurchasingLimits>();
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var currentLocalTime = HL.Common.Utilities.DateUtils.GetCurrentLocalTime(_currentLoggedInCountry);

                    var request = new GetDistributorPurchasingLimitsRequest_V02
                    {
                        DistributorID = _distributorId,
                        CountryCode = _currentLoggedInCountry,
                        Year = currentLocalTime.Year,
                        Month = currentLocalTime.Month,
                        Source = source,
                        LimitPeriod = HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionPeriod, // this is applicable only to purchasing limits
                    };

                    var response =
                        proxy.GetDistributorPurchasingLimits(new GetDistributorPurchasingLimitsRequest(request)).GetDistributorPurchasingLimitsResult as GetDistributorPurchasingLimitsResponse_V02;
                    if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                    {
                        limits = response.PurchasingLimits;
                    }
                }
                catch (Exception ex)
                {
                    HL.Common.Logging.LoggerHelper.Exception("System.Exception", new Exception(
                                                                   string.Format(
                                                                       "Error LoadPurchasingLimitsFromDB from Order service for: DS:{0} - Country:{1}, {2}",
                                                                       _distributorId, _currentLoggedInCountry, ex)));
                }
            }
            return limits;
        }

        /// <summary>
        /// WritePurchasingLimitsToDB
        /// </summary>
        /// <param name="limits"></param>
        private void WritePurchasingLimitsToDB(List<PurchasingLimits> limits)
        {
            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    var currentLocalTime = HL.Common.Utilities.DateUtils.GetCurrentLocalTime(_currentLoggedInCountry);

                    foreach (PurchasingLimits l in limits)
                    {
                        PurchasingLimits_V01 lV01 = l as PurchasingLimits_V01;
                        PurchasingLimitRestrictionPeriod purchasingLimitRestrictionPeriod = HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionPeriod;
                        if (lV01.LimitsRestrictionType == LimitsRestrictionType.FOP || lV01.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold)
                            purchasingLimitRestrictionPeriod = PurchasingLimitRestrictionPeriod.OneTime;
                        //else if (lV01.LimitsRestrictionType ==  HL.Order.ValueObjects.LimitsRestrictionType.OrderThreshold)
                        //    purchasingLimitRestrictionPeriod = PurchasingLimitRestrictionPeriod.Monthly;

                        var request = new SetPurchasingLimitsRequest_V02
                        {
                            DistributorId = _distributorId,
                            Country = _currentLoggedInCountry,
                            OrderMonth = string.Format("{0}{1}", lV01.Year, lV01.Month),
                            PurchasingLimits = lV01,
                            RestrictionPeriod = purchasingLimitRestrictionPeriod
                        };

                        var response =
                            proxy.SetPurchasingLimits(new SetPurchasingLimitsRequest1(request)).SetPurchasingLimitsResult as SetPurchasingLimitsResponse_V02;
                        if (response != null && response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                        {
                            HL.Common.Logging.LoggerHelper.Info("WritePurchasingLimitsToDB update successful");
                        }
                    }
                }
                catch (Exception ex)
                {
                    HL.Common.Logging.LoggerHelper.Exception("System.Exception", new Exception(
                                                                   string.Format(
                                                                       "Error WritePurchasingLimitsToDB for: DS:{0} - Country:{1}, {2}",
                                                                       _distributorId, _currentLoggedInCountry, ex)));
                }
            }
        }

        /// <summary>
        /// get order month
        /// </summary>
        /// <returns></returns>
        private int GetOrderMonth()
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return 0;
            var sessionInfo = SessionInfo.GetSessionInfo(_distributorId, _locale);
            if (sessionInfo != null)
            {
                if (sessionInfo.ShoppingCart != null)
                {
                    return sessionInfo.ShoppingCart.OrderMonth;
                }
            }
            return 0;
        }


        private int getIntOrderMonth(DateTime date)
        {
            return date.Year * 100 + date.Month;
        }

        private int getIntOrderMonth(int year, int month)
        {
            return year * 100 + month;
        }

        private void createEmptyLimits()
        {
            DateTime localDatetime = HL.Common.Utilities.DateUtils.GetCurrentLocalTime(this._currentLoggedInCountry);
            DateTime previousMonth = localDatetime.AddMonths(-1);
            var limit = PurchasingLimits[getIntOrderMonth(localDatetime)] = new PurchasingLimits_V01 { LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits, PurchaseLimitType = PurchaseLimitType.None, Year = localDatetime.Year, Month = localDatetime.Month, maxVolumeLimit = -1, MaxEarningsLimit = -1, RemainingVolume = -1, RemainingEarnings = -1 };
            var limit2 = PurchasingLimits[getIntOrderMonth(previousMonth)] = new PurchasingLimits_V01 { LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits, PurchaseLimitType = PurchaseLimitType.None, Year = previousMonth.Year, Month = previousMonth.Month, maxVolumeLimit = -1, MaxEarningsLimit = -1, RemainingVolume = -1, RemainingEarnings = -1 };
            if (this._applicableLimits == null || _applicableLimits.Count == 0)
            {
                _applicableLimits = new List<PurchasingLimits>();
                _applicableLimits.Add(limit);
                _applicableLimits.Add(limit2);
            }

        }

        private DateTime AbsoluteStart(DateTime dateTime)
        {
            return dateTime.Date;
        }

        /// <summary>
        /// Gets the 11:59:59 instance of a DateTime
        /// </summary>
        private DateTime AbsoluteEnd(DateTime dateTime)
        {
            return AbsoluteStart(dateTime).AddDays(1).AddTicks(-1);
        }

        private void checkLimitExpiry(PurchasingLimits_V01 limit, DateTime localDatetime)
        {
            if (limit.LimitsRestrictionType == LimitsRestrictionType.FOP ||
                limit.LimitsRestrictionType == LimitsRestrictionType.OrderThreshold)
            {
                if (limit.maxVolumeLimit == -1)
                {
                    limit.Completed = true;
                    limit.maxVolumeLimit = -1; // no limit
                    limit.RestrictionPeriod = PurchasingLimitRestrictionPeriod.OneTime;
                    WritePurchasingLimitsToDB(new List<PurchasingLimits> { limit });
                }
                else if (hasValidDate(limit.FirstOrderDate) && !limit.Completed)
                {
                    DateTime firstOrderDatetimeLocal = new DateTime(limit.FirstOrderDate.Year, limit.FirstOrderDate.Month, limit.FirstOrderDate.Day);
                    DateTime expiry = firstOrderDatetimeLocal.AddDays(limit.ThresholdPeriod == 0 ? PurchaseRestrictionProvider.GetThresholdPeriod(this._processingCountry) : limit.ThresholdPeriod);
                    localDatetime = new DateTime(localDatetime.Year, localDatetime.Month, localDatetime.Day);
                    if (expiry <= localDatetime)
                    {
                        limit.Completed = true;
                        limit.maxVolumeLimit = -1; // no limit
                        limit.Expiry = expiry;
                        limit.RestrictionPeriod = PurchasingLimitRestrictionPeriod.OneTime;
                        WritePurchasingLimitsToDB(new List<PurchasingLimits> { limit });
                    }
                }
            }
        }

        private void copyPurchasingLimits(PurchasingLimits_V01 source, PurchasingLimits_V01 target)
        {
            var copy = source.Clone();
        }


        private void SetPurchasingLimits()
        {
            PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();
            try
            {
                if (_applicableLimits != null && _applicableLimits.Count > 0)
                {
                    // order by Month
                    DateTime localDatetime = HL.Common.Utilities.DateUtils.GetCurrentLocalTime(this._currentLoggedInCountry);
                    var FOP = _applicableLimits.Find(l => ((PurchasingLimits_V01)l).LimitsRestrictionType == LimitsRestrictionType.FOP) as PurchasingLimits_V01;
                    if (FOP != null)
                        checkLimitExpiry(FOP, localDatetime);

                    DateTime previousMonth = localDatetime.AddMonths(-1);
                    int currentOrderMonth = getIntOrderMonth(localDatetime);
                    int prevOrderMonth = getIntOrderMonth(previousMonth);

                    HLRulesManager.Manager.SetPurchaseRestriction(Tins, currentOrderMonth, _distributorId, this); // call rules to set PurchaseLimitType
                    HLRulesManager.Manager.SetPurchaseRestriction(Tins, prevOrderMonth, _distributorId, this); // call rules to set PurchaseLimitType
                }
                else
                {
                    createEmptyLimits();
                    var keys = new List<int>();
                    keys.AddRange(PurchasingLimits.Keys);
                    foreach (var pl in keys)
                    {
                        HLRulesManager.Manager.SetPurchaseRestriction(Tins, pl, _distributorId, this); // call rules to set can purchase
                    }
                }
                if (!HLConfigManager.Configurations.DOConfiguration.HasAdditionalLimits)
                   _applicableLimits = null;
            }
            catch (Exception)
            {
                createEmptyLimits();
            }
        }
        #endregion private method
    }
}
