using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.ViewModel;
using System.Web.Security;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Providers
{
    public class ServerRulesManager : IShoppingCartRule, IPurchasingLimitsRule, IPurchaseRestrictionRule, IInventoryRule, ITaxationRule,
                                      IShoppingCartIntegrityRule, IPurchasingPermissionRule, IShoppingCartManagementRule,
                                      IDiscountRule, IHFFRule, ISavedCartManagementRule, IOrderManagementRule, IPromoRule
    {
        //TBD - replace all these try catches with a TryGet

        private const string _AssemblyName = "MyHerbalife3.Ordering.Rules.dll";

        #region Fields

        private readonly Dictionary<string, Dictionary<string, RuleSet>> _platformRuleSets =
            new Dictionary<string, Dictionary<string, RuleSet>>();

        #endregion Fields

        #region Singleton Implementation

        private static readonly ServerRulesManager _ServerRulesManager = new ServerRulesManager();

        static ServerRulesManager()
        {
        }

        public static ServerRulesManager Instance
        {
            get { return _ServerRulesManager; }
        }

        #endregion Singleton Implementation

        #region Construction

        private ServerRulesManager()
        {
            var context = HttpContext.Current;

            try
            {
                string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
                var assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
                var varTypes = (from type in assembly.GetTypes()
                                where type.IsClass
                                select type).ToList<Type>();
                bool FOPEnabled = Settings.GetRequiredAppSetting<bool>("FOPEnabled", false);
                var platformConfigs = ConfigManager.Instance.AllPlatformConfigs;
                foreach (KeyValuePair<string, Dictionary<string, ConfigurationSet>> platform in platformConfigs)
                {
                    if (!_platformRuleSets.ContainsKey(platform.Key))
                    {
                        _platformRuleSets.Add(platform.Key, new Dictionary<string, RuleSet>());
                    }

                    // walk thru each locale in this platform
                    foreach (ConfigurationSet cSet in platform.Value.Values)
                    {
                        // for each locale, create a RuleSet
                        var ruleSet = new RuleSet();
                        var e = cSet.RulesConfiguration.RulesModules.GetEnumerator();
                        while (e.MoveNext())
                        {                            
                            var varObjs = varTypes.Where(t => t.FullName.Equals(e.Current.ClassName)).ToList();
                            if (varObjs.Any())
                            {
                                var module = e.Current;
                                var varInterface =
                                    varObjs.SelectMany(
                                        v =>
                                        v.GetInterfaces()
                                         .Where(i => String.CompareOrdinal(i.FullName, module.RuleInterface) == 0)
                                         .Select(i => new
                                             {
                                                 cls = v,
                                                 clsInterface = i
                                             }
                                            )).ToList();

                                if (varInterface.Any())
                                {
                                    var Interface = varInterface.First().clsInterface;
                                    try
                                    {
                                        var ruleClass = Activator.CreateInstance(varInterface.First().cls);
                                        (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
                                        (ruleClass as MyHerbalifeRule).CheckIntegrityOnError = module.CheckIntegrityOnError;
                                        if (Interface == typeof (IShoppingCartRule)) //To Be Genericized
                                        {
                                            string name = ruleClass.ToString();
                                            if (name.Contains(".PurchasingLimits.") || name.Contains(".PurchasingPermissions."))
                                            {
                                                if (!FOPEnabled)
                                                    ruleSet.IShoppingCartRuleModules.Add(ruleClass as IShoppingCartRule);
                                            }
                                            else
                                                ruleSet.IShoppingCartRuleModules.Add(ruleClass as IShoppingCartRule);
                                        }
                                        else if (Interface == typeof (IShoppingCartManagementRule))
                                        {
                                            ruleSet.IShoppingCartManagementRuleModules.Insert(module.ProcessOrder,
                                                                                              ruleClass as
                                                                                              IShoppingCartManagementRule);
                                        }
                                        else if (Interface == typeof (IPurchasingLimitsRule))
                                        {
                                            if (!FOPEnabled)
                                            {
                                                ruleSet.IPurchasingLimitsRuleModules.Insert(module.ProcessOrder,
                                                                                            ruleClass as
                                                                                            IPurchasingLimitsRule);
                                            }
                                        }
                                        else if (Interface == typeof(IPurchaseRestrictionRule))
                                        {
                                            ruleSet.IPurchaseRestrictionRuleModules.Insert(module.ProcessOrder,
                                                                                        ruleClass as
                                                                                        IPurchaseRestrictionRule);
                                        }
                                        else if (Interface == typeof (IInventoryRule))
                                        {
                                            ruleSet.IInventoryRuleModules.Insert(module.ProcessOrder,
                                                                                 ruleClass as IInventoryRule);
                                        }
                                        else if (Interface == typeof (ITaxationRule))
                                        {
                                            ruleSet.ITaxationRuleModules.Insert(module.ProcessOrder,
                                                                                ruleClass as ITaxationRule);
                                        }
                                        else if (Interface == typeof (IDiscountRule))
                                        {
                                            ruleSet.IDiscountRuleModules.Insert(module.ProcessOrder,
                                                                                ruleClass as IDiscountRule);
                                        }
                                        else if (Interface == typeof (IShoppingCartIntegrityRule))
                                        {
                                            ruleSet.IShoppingCartIntegrityRuleModules.Insert(module.ProcessOrder,
                                                                                             ruleClass as
                                                                                             IShoppingCartIntegrityRule);
                                        }
                                        else if (Interface == typeof (IPurchasingPermissionRule))
                                        {
                                            if (!FOPEnabled)
                                            {
                                                ruleSet.IPurchasingPermissionRuleModules.Insert(module.ProcessOrder,
                                                                                                ruleClass as
                                                                                                IPurchasingPermissionRule);
                                            }
                                        }
                                        else if (Interface == typeof (IHFFRule))
                                        {
                                            ruleSet.IHFFRuleModules.Insert(module.ProcessOrder, ruleClass as IHFFRule);
                                        }
                                        else if (Interface == typeof(IValidateDistributorRule))
                                        {
                                            ruleSet.IValidateDistributorRuleModules.Insert(module.ProcessOrder, ruleClass as IValidateDistributorRule);
                                        }
                                        else if (Interface == typeof(ISavedCartManagementRule))
                                        {
                                            ruleSet.ISavedCartManagementRuleModules.Insert(module.ProcessOrder, ruleClass as ISavedCartManagementRule);
                                        }
                                        else if (Interface == typeof(IOrderManagementRule))
                                        {
                                            ruleSet.IOrderManagementRuleModule.Insert(module.ProcessOrder, ruleClass as IOrderManagementRule);
                                        }
                                        else if (Interface == typeof(IPromoRule))
                                        {
                                            ruleSet.IPromoRuleModule.Insert(module.ProcessOrder, ruleClass as IPromoRule);
                                        }
                                       
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggerHelper.Error(
                                            string.Format(
                                                "Could not activate Rule for Locale: {0} - {1}. The Error Message is: \r\n{2}",
                                                cSet.Locale, varInterface.First().cls, ex.Message));
                                    }
                                }
                            }
                        }
                        _platformRuleSets[platform.Key].Add(cSet.Locale, ruleSet);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName,
                                  ex.Message));
            }
        }

        //private ServerRulesManager()
        //{
        //    //Get list of all RuleProcessors from configs
        //    //Instantiate collections of processors by locale
        //    //to get started - we're just doing a well-known interface
        //    Dictionary<string, Dictionary<string, ConfigurationSet>> platformConfigs = ConfigManager.Instance.AllPlatformConfigs;
        //    foreach (KeyValuePair<string, Dictionary<string, ConfigurationSet>> platform in platformConfigs)
        //    {
        //        if (!_platformRuleSets.ContainsKey(platform.Key))
        //        {
        //            _platformRuleSets.Add(platform.Key, new Dictionary<string, RuleSet>());
        //        }

        //        foreach (KeyValuePair<string, ConfigurationSet> configurationSet in platform.Value)
        //        {
        //            List<IShoppingCartRule> cartRules = new List<IShoppingCartRule>();
        //            List<IShoppingCartManagementRule> cartManagementRules = new List<IShoppingCartManagementRule>();
        //            List<IPurchasingLimitsRule> purchaseLimitRules = new List<IPurchasingLimitsRule>();
        //            List<IInventoryRule> inventoryRules = new List<IInventoryRule>();
        //            List<IShippingSourceRule> shippingSourceRules = new List<IShippingSourceRule>();
        //            List<ITaxationRule> taxationRules = new List<ITaxationRule>();
        //            List<IDiscountRule> discountRules = new List<IDiscountRule>();
        //            List<IInvoiceOptionsRule> invoiceOptionsRules = new List<IInvoiceOptionsRule>();
        //            List<IShoppingCartIntegrityRule> shippingCartInterityRules = new List<IShoppingCartIntegrityRule>();
        //            List<IPurchasingPermissionRule> purchasingPermissionRules = new List<IPurchasingPermissionRule>();
        //            List<IHFFRule> hFFRules = new List<IHFFRule>();

        //            if (null != configurationSet.Value.RulesConfiguration && configurationSet.Value.RulesConfiguration.RulesModules.Count > 0)
        //            {
        //                cartRules = GetShoppingCartRules(configurationSet.Value.RulesConfiguration);
        //                cartManagementRules = GetShoppingCartManagementRules(configurationSet.Value.RulesConfiguration);
        //                purchaseLimitRules = GetPurchasingLimitsRules(configurationSet.Value.RulesConfiguration);
        //                inventoryRules = GetInventoryRules(configurationSet.Value.RulesConfiguration);
        //                shippingSourceRules = GetShippingSourceRules(configurationSet.Value.RulesConfiguration);
        //                taxationRules = GetTaxationRules(configurationSet.Value.RulesConfiguration);
        //                discountRules = GetDiscountRules(configurationSet.Value.RulesConfiguration);
        //                invoiceOptionsRules = GetInvoiceOptions(configurationSet.Value.RulesConfiguration);
        //                shippingCartInterityRules = GetShoppingCartIntegrityRules(configurationSet.Value.RulesConfiguration);
        //                purchasingPermissionRules = GetPurchasingPermissionRules(configurationSet.Value.RulesConfiguration);
        //                hFFRules = GetHFFRules(configurationSet.Value.RulesConfiguration);
        //            }

        //            RuleSet ruleSet = new RuleSet();
        //            ruleSet.IShoppingCartRuleModules.AddRange(cartRules);
        //            ruleSet.IShoppingCartManagementRuleModules.AddRange(cartManagementRules);
        //            ruleSet.IPurchasingLimitsRuleModules.AddRange(purchaseLimitRules);
        //            ruleSet.IInventoryRuleModules.AddRange(inventoryRules);
        //            ruleSet.IShippingSourceRuleModules.AddRange(shippingSourceRules);
        //            ruleSet.ITaxationRuleModules.AddRange(taxationRules);
        //            ruleSet.IDiscountRuleModules.AddRange(discountRules);
        //            ruleSet.IInvoiceOptionsRuleModules.AddRange(invoiceOptionsRules);
        //            ruleSet.IShoppingCartIntegrityRuleModules.AddRange(shippingCartInterityRules);
        //            ruleSet.IPurchasingPermissionRuleModules.AddRange(purchasingPermissionRules);
        //            ruleSet.IHFFRuleModules.AddRange(hFFRules);

        //            _platformRuleSets[platform.Key].Add(configurationSet.Key, ruleSet);
        //        }
        //    }
        //}

        #endregion Construction

        #region Rule Invoking methods

        public string PerformDiscountRangeRules(ShoppingCart_V02 cart, string locale, decimal dsDiscount)
        {
            try
            {
                foreach (IDiscountRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IDiscountRuleModules)
                {
                    return rule.PerformDiscountRangeRules(cart, locale, dsDiscount);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PerformDiscountRangeRules fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
            return null;
        }

        public void PerformDiscountRules(ShoppingCart_V02 cart, Order_V01 order, string locale, ShoppingCartRuleReason reason)
        {
            //string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                foreach (IDiscountRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IDiscountRuleModules)
                {
                    rule.PerformDiscountRules(cart, order, locale, reason);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PerformDiscountRules fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
        }

        public void ProcessCatalogItemsForInventory(string locale, ShoppingCart_V02 shoppingCart, List<SKU_V01> itemList)
        {
            try
            {
                foreach (
                    IInventoryRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IInventoryRuleModules)
                {
                    rule.ProcessCatalogItemsForInventory(locale, shoppingCart, itemList);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("ProcessCatalogItemsForInventory fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
        }


        public void CheckInventory(ShoppingCart_V02 shoppingCart, int quantity, SKU_V01 sku, string warehouse, ref int availQuantity)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                foreach (
                    IInventoryRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IInventoryRuleModules)
                {
                    rule.CheckInventory(shoppingCart, quantity, sku, warehouse, ref availQuantity);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CheckInventory fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
        }

        public void PerformBackorderRules(ShoppingCart_V02 cart, CatalogItem item)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process BackOrder Rules for Locale {1}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            try
            {
                foreach (
                    IInventoryRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IInventoryRuleModules)
                {
                    rule.PerformBackorderRules(cart, item);
                    //break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PerformBackorderRules fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
        }

        public Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            Dictionary<int, PurchasingLimits_V01> result = null;
            var listofpurchasinglimit = _platformRuleSets[HLConfigManager.Platform][locale].IPurchasingLimitsRuleModules;
            if (listofpurchasinglimit != null)
            {
                try
                {
                    foreach (
                        IPurchasingLimitsRule rule in listofpurchasinglimit
                           )
                    {
                        result = rule.GetPurchasingLimits(distributorId, TIN);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetPurchasingLimits fails: {0} {1} {2}. The Error Message is: \r\n{3}, IsListOfPurchasingLimit null:{4},Stack Trace{5}",
                        HLConfigManager.Platform, locale, distributorId, ex.Message,listofpurchasinglimit,ex.StackTrace));
                }
            }
            return result;
        }

        // FOP
        public void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            try
            {
                foreach (
                    IPurchaseRestrictionRule rule in
                        _platformRuleSets[HLConfigManager.Platform][Thread.CurrentThread.CurrentCulture.ToString()].IPurchaseRestrictionRuleModules)
                {
                    rule.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("SetPurchasRestriction fails: {0} {1} {2}. The Error Message is: \r\n{3}",
                                  HLConfigManager.Platform, distributorId, orderMonth.ToString(), ex.Message));
            }
        }

        public void SetOrderSubType(string orderSubType, string distributorId)
        {
            try
            {
                foreach (
                    IPurchaseRestrictionRule rule in
                        _platformRuleSets[HLConfigManager.Platform][Thread.CurrentThread.CurrentCulture.ToString()].IPurchaseRestrictionRuleModules)
                {
                    rule.SetOrderSubType(orderSubType, distributorId);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("SetOrderSubType fails: {0} {1} {2}. The Error Message is: \r\n{3}",
                                  HLConfigManager.Platform, distributorId, orderSubType, ex.Message));
            }
        }

        public bool PurchasingLimitsAreExceeded(string distributorId)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            bool result = false;
            try
            {
                foreach (
                    IPurchasingLimitsRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IPurchasingLimitsRuleModules)
                {
                    result = rule.PurchasingLimitsAreExceeded(distributorId);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PurchasingLimitsAreExceeded fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }
            return result;
        }

        public bool PurchasingLimitsAreExceeded(int orderMonth, string distributorId)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            bool result = false;
            try
            {
                foreach (
                    IPurchaseRestrictionRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IPurchaseRestrictionRuleModules)
                {
                    result = rule.PurchasingLimitsAreExceeded(orderMonth, distributorId);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PurchasingLimitsAreExceeded fails: {0} {1} {3}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message, orderMonth.ToString()));
            }
            return result;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCartRuleReason reason, MyHLShoppingCart cart)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process PurchaseRestriction Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            string name = string.Empty;
            //Process all ICartRule rules for this locale
            try
            {
                foreach (
                    IPurchaseRestrictionRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IPurchaseRestrictionRuleModules)
                {
                    name = rule.ToString();
                    results.AddRange(rule.ProcessCart(reason, cart));

                    if ((rule as MyHerbalifeRule).DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
                if (results.Any(rr => rr.Result == RulesResult.Failure))
                {
                    // Get integrity rules to run on error
                    foreach (IShoppingCartRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                    {
                        if ((rule as MyHerbalifeRule).CheckIntegrityOnError)
                        {
                            results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartRuleFailed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while processing PurchaseRestriction Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, name));
            }

            return results;
        }

        public bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            bool result = false;
            try
            {
                foreach (
                    IPurchasingLimitsRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IPurchasingLimitsRuleModules)
                {
                    result = rule.DistributorIsExemptFromPurchasingLimits(distributorId);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "DistributorIsExemptFromPurchasingLimits fails: {0} {1} {2}. The Error Message is: \r\n{3}",
                        HLConfigManager.Platform, locale, distributorId, ex.Message));
            }
            return result;
        }

        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool result = false;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                foreach (
                    IPurchasingPermissionRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IPurchasingPermissionRuleModules)
                {
                    //Check for override rule whether this DS is exempted.
                    if (CatalogProvider.IsDistributorExempted(distributorID))
                    {
                        result = true;
                    }
                    else
                    {
                        result = rule.CanPurchase(distributorID, countryCode);
                    }

                    
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CanPurchase check fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }

            return result;
        }

        public List<string> PerformValidateDistributorRules(string DistributorID)
        {
            List<string> result = null;
            string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                foreach (IValidateDistributorRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IValidateDistributorRuleModules)
                {
                    result = rule.PerformValidateDistributorRules(DistributorID);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("PerformValidateDistributorRules check fails: {0} {1} {2}. The Error Message is: \r\n{2}", HLConfigManager.Platform, locale, DistributorID, ex.Message));
            }

            return result;
        }

        public List<ShoppingCartRuleResult> ProcessSavedCartManagementRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            List<ShoppingCartRuleResult> results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage = string.Format("Cannot process Saved Cart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            try
            {
                foreach (ISavedCartManagementRule rule in _platformRuleSets[HLConfigManager.Platform][locale].ISavedCartManagementRuleModules)
                {
                    results.AddRange(rule.ProcessSavedCartManagementRules(cart, reason));
                    if ((rule as MyHerbalifeRule).DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
                if (results.Any(rr => rr.Result == RulesResult.Failure))
                {
                    // Get integrity rules to run on error
                    foreach (IShoppingCartRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                    {
                        if ((rule as MyHerbalifeRule).CheckIntegrityOnError)
                        {
                            results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartRuleFailed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("An exception occurred while processing SavedCart Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}", cart.DistributorID, locale, cart.ShoppingCartID, ex.Message));
            }

            return results;
        }

        /// <summary>
        /// Performs the order management rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="order">The order.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="reason">The Reason</param>
        public void PerformOrderManagementRules(ShoppingCart_V02 cart, Order_V01 order, string locale, OrderManagementRuleReason reason)
        {
            try
            {
                foreach (IOrderManagementRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IOrderManagementRuleModule)
                {
                    rule.PerformOrderManagementRules(cart, order, locale, reason);

                    if (reason == OrderManagementRuleReason.OrderBeingSubmitted)
                        break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("PerformOrderManagementRules fails: {0} {1}. The Error Message is: \r\n{2}", HLConfigManager.Platform, locale, ex.Message));
            }
        }

        /// <summary>
        /// Performs the promo process rules.
        /// </summary>
        /// <param name="cart">The cart.</param>
        /// <param name="skus">List of skus.</param>
        /// <param name="reason">The rule reason.</param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessPromoInCart(ShoppingCart_V02 cart, List<string> skus, ShoppingCartRuleReason reason)
        {
            string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage = string.Format("Cannot process Promo In Cart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            try
            {
                foreach (IPromoRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IPromoRuleModule)
                {
                    results.AddRange(rule.ProcessPromoInCart(cart, skus, reason));
                    if ((rule as MyHerbalifeRule).DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
                if (results.Any(rr => rr.Result == RulesResult.Failure))
                {
                    // Get integrity rules to run on error
                    foreach (IShoppingCartRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                    {
                        if ((rule as MyHerbalifeRule).CheckIntegrityOnError)
                        {
                            results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartRuleFailed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("An exception occurred while processing Promo In Cart Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}", cart.DistributorID, locale, cart.ShoppingCartID, ex.Message));
            }
            return results;
        }

        public bool DuplicateSKU(ShoppingCart_V01 cart)
        {
            bool result = false;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (null == cart)
            {
                string errorMessage =
                    string.Format(
                        "Cannot process ShoppingCart Integrity Rules for Locale {1}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            try
            {
                foreach (
                    IShoppingCartIntegrityRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartIntegrityRuleModules)
                {
                    result = rule.DuplicateSKU(cart);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("DuplicateSKU check fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }

            return result;
        }

        public bool InvalidQuantity(ShoppingCart_V01 cart)
        {
            bool result = false;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            if (null == cart)
            {
                string errorMessage =
                    string.Format(
                        "Cannot process ShoppingCart Integrity Rules for Locale {1}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            try
            {
                foreach (
                    IShoppingCartIntegrityRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartIntegrityRuleModules)
                {
                    result = rule.InvalidQuantity(cart);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("InvalidQuantity check fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }

            return result;
        }

        public List<ShoppingCartRuleResult> ProcessCartManagementRules(ShoppingCart_V02 cart)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process Cart Management Rules for Locale {0}. The ShoppingCart is null",
                                  locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }

            //Process all ICartRule rules for this locale
            try
            {
                foreach (
                    IShoppingCartManagementRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartManagementRuleModules)
                {
                    results.AddRange(rule.ProcessCartManagementRules(cart));
                    if ((rule as MyHerbalifeRule).DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
                if (results.Any(rr => rr.Result == RulesResult.Failure))
                {
                    // Get integrity rules to run on error
                    foreach (IShoppingCartRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                    {
                        if ((rule as MyHerbalifeRule).CheckIntegrityOnError)
                        {
                            results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartRuleFailed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("ProcessCartManagementRules fails: {0} {1}. The Error Message is: \r\n{2}",
                                  HLConfigManager.Platform, locale, ex.Message));
            }

            return results;
        }

        /// <summary>Runs the shopping Cart through all ICartRule processors</summary>
        /// <param name="cart">The current Shopping Cart</param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process ShoppingCart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            bool FOPEnabled = (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false));
            string name = string.Empty;
            //Process all ICartRule rules for this locale
            try
            {
                foreach (
                    IShoppingCartRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                {
                    name = rule.ToString();
                    if (name.Contains(".PurchasingLimits.") || name.Contains(".PurchasingPermissions."))
                    {
                        if (!FOPEnabled) // skip when switch to FOP
                            results.AddRange(rule.ProcessCart(cart, reason));
                    }
                    else if (name.Contains(".Promotional") || name.Contains(".PurchaseRestrictionRules"))
                    {
                        var isHAP = HLConfigManager.Configurations.DOConfiguration.AllowHAP &&
                                    cart.OrderCategory == OrderCategoryType.HSO;
                        if (!isHAP)
                        {
                            results.AddRange(rule.ProcessCart(cart, reason));
                        }
                    }
                    else
                    {
                        results.AddRange(rule.ProcessCart(cart, reason));
                    }
                    if ((rule as MyHerbalifeRule).DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
                if (results.Any(rr => rr.Result == RulesResult.Failure))
                {
                    // Get integrity rules to run on error
                    foreach (IShoppingCartRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                    {
                        var myHerbalifeRule = rule as MyHerbalifeRule;
                        if (myHerbalifeRule != null && myHerbalifeRule.CheckIntegrityOnError)
                        {
                            results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartRuleFailed));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while processing ShoppingCart Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, name));
            }

            return results;
        }

        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            //string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (member != null) order.DistributorID = member.Value.Id;
            try
            {
                foreach (ITaxationRule rule in _platformRuleSets[HLConfigManager.Platform][locale].ITaxationRuleModules)
                {
                    rule.PerformTaxationRules(order, locale);
                    break;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PerformTaxationRules fails: {0} {1} {2}. The Error Message is: \r\n{3}",
                                  HLConfigManager.Platform, locale, order.DistributorID, ex.Message));
            }
        }

        //public string GetTinValue(OnlineDistributor distributor, string tinID)
        //{
        //    string result = string.Empty;
        //    string locale = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
        //    try
        //    {
        //        foreach (IPurchasingPermissionRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IPurchasingPermissionRuleModules)
        //        {
        //            result = (rule as MyHerbalifeRule).GetTinValue(distributor, tinID);
        //            break;
        //        }
        //    }
        //    catch { }

        //    return result;
        //}

        #endregion Rule Invoking methods

        #region Private methods

        //private List<IShoppingCartRule> GetShoppingCartRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IShoppingCartRule> cartRules = new List<IShoppingCartRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IShoppingCartRule))	//To Be Genericized
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    cartRules.Insert(module.ProcessOrder, ruleClass as IShoppingCartRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return cartRules;
        //}
        //private List<IShoppingCartManagementRule> GetShoppingCartManagementRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IShoppingCartManagementRule> cartManagementRules = new List<IShoppingCartManagementRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IShoppingCartManagementRule))	//To Be Genericized
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    cartManagementRules.Insert(module.ProcessOrder, ruleClass as IShoppingCartManagementRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return cartManagementRules;
        //}
        //private List<IPurchasingLimitsRule> GetPurchasingLimitsRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IPurchasingLimitsRule> purchaseLimitRules = new List<IPurchasingLimitsRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IPurchasingLimitsRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    purchaseLimitRules.Insert(module.ProcessOrder, ruleClass as IPurchasingLimitsRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return purchaseLimitRules;
        //}
        //private List<IInventoryRule> GetInventoryRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IInventoryRule> inventoryRules = new List<IInventoryRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IInventoryRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    inventoryRules.Insert(module.ProcessOrder, ruleClass as IInventoryRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return inventoryRules;
        //}
        //private List<IShippingSourceRule> GetShippingSourceRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IShippingSourceRule> shippingSourceRules = new List<IShippingSourceRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IShippingSourceRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    shippingSourceRules.Insert(module.ProcessOrder, ruleClass as IShippingSourceRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return shippingSourceRules;
        //}
        //private List<ITaxationRule> GetTaxationRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<ITaxationRule> taxationRules = new List<ITaxationRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(ITaxationRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    taxationRules.Insert(module.ProcessOrder, ruleClass as ITaxationRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return taxationRules;
        //}
        //private List<IDiscountRule> GetDiscountRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IDiscountRule> taxationRules = new List<IDiscountRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IDiscountRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    taxationRules.Insert(module.ProcessOrder, ruleClass as IDiscountRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return taxationRules;
        //}
        //private List<IInvoiceOptionsRule> GetInvoiceOptions(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IInvoiceOptionsRule> invoiceOptionsRules = new List<IInvoiceOptionsRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IInvoiceOptionsRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    invoiceOptionsRules.Insert(module.ProcessOrder, ruleClass as IInvoiceOptionsRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return invoiceOptionsRules;
        //}
        //private List<IShoppingCartIntegrityRule> GetShoppingCartIntegrityRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IShoppingCartIntegrityRule> shoppingCartIntegrityRules = new List<IShoppingCartIntegrityRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IShoppingCartIntegrityRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    shoppingCartIntegrityRules.Insert(module.ProcessOrder, ruleClass as IShoppingCartIntegrityRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return shoppingCartIntegrityRules;
        //}
        //private List<IPurchasingPermissionRule> GetPurchasingPermissionRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IPurchasingPermissionRule> purchasingPermissionRules = new List<IPurchasingPermissionRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IPurchasingPermissionRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    purchasingPermissionRules.Insert(module.ProcessOrder, ruleClass as IPurchasingPermissionRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return purchasingPermissionRules;
        //}
        //private List<IHFFRule> GetHFFRules(RulesConfiguration config)
        //{
        //    HttpContext context = HttpContext.Current;
        //    string configPath = Path.Combine(context.Request.PhysicalApplicationPath, "bin");
        //    List<IHFFRule> hFFRules = new List<IHFFRule>();
        //    foreach (RulesModule module in config.RulesModules)
        //    {
        //        try
        //        {
        //            Assembly assembly = Assembly.LoadFrom(Path.Combine(configPath, _AssemblyName));
        //            foreach (Type type in assembly.GetTypes())
        //            {
        //                if (type.IsClass == true)
        //                {
        //                    if (type.FullName.Equals(module.ClassName))
        //                    {
        //                        foreach (Type Interface in type.GetInterfaces())
        //                        {
        //                            if (string.Compare(Interface.FullName, module.RuleInterface) == 0)
        //                            {
        //                                if (Interface == typeof(IHFFRule))
        //                                {
        //                                    object ruleClass = Activator.CreateInstance(type);
        //                                    (ruleClass as MyHerbalifeRule).DiscontinueOnError = module.DiscontinueOnError;
        //                                    hFFRules.Insert(module.ProcessOrder, ruleClass as IHFFRule);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            LoggerHelper.Error(string.Format("Could not load the Rules assembly: {0}. The Error Message is: \r\n{1}", _AssemblyName, ex.Message), "ServerRulesManager");
        //        }
        //    }
        //    return hFFRules;
        //}

        #endregion Private methods

        #region IHFFRule implementation

        /// <summary>Load up any rule classes from their listed assemblies in the RulesConfiguration</summary>
        /// <returns>A list of IShoppingCartRules implementors</returns>
        /// <remarks>
        ///     This is only doing IShoppingCartRules for now due to time constraints
        ///     The next iteration will genericise the handling of rules interfaces for multiple interfaces
        /// </remarks>
        /// <summary>Load up any rule classes from their listed assemblies in the RulesConfiguration</summary>
        /// <returns>A list of IShoppingCartRules implementors</returns>
        /// <remarks>
        ///     This is only doing IShoppingCartRules for now due to time constraints
        ///     The next iteration will genericise the handling of rules interfaces for multiple interfaces
        /// </remarks>
        /// <summary>
        ///     Indicates when the HFF module must to be shown.
        /// </summary>
        /// <param name="cart">The shopping cart.</param>
        /// <returns>If the HFF module must to be shown.</returns>
        public virtual bool CanDonate(ShoppingCart_V02 cart)
        {
            bool result = HLConfigManager.Configurations.DOConfiguration.AllowHFF;
            string locale = Thread.CurrentThread.CurrentCulture.ToString();
            try
            {
                foreach (IHFFRule rule in _platformRuleSets[HLConfigManager.Platform][locale].IHFFRuleModules)
                {
                    result = rule.CanDonate(cart);
                    break;
                }
            }
            catch
            {
            }

            return result;
        }

        public RuleSet GetRuleSet(string platform, string locale)
        {
            if (_platformRuleSets.ContainsKey(platform) && _platformRuleSets[platform].ContainsKey(locale))
            {
                return _platformRuleSets[platform][locale];
            }
            return null;
        }

        #endregion

        public List<ShoppingCartRuleResult> ValidateAPF(MyHLShoppingCart cart, string locale)
        {
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process ShoppingCart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            string name = string.Empty;
            try
            {
                foreach (
                    IShoppingCartRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                {
                    name = rule.ToString();
                    if (name.Contains(".APF."))
                    {
                        bool bCartWasEdited = cart.APFEdited;
                        cart.APFEdited = false;
                        var oldCartItemCount = cart.CartItems.Count();
                        results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartCreated)); // this will remove non-APF if APF is due and not paid
                        cart.APFEdited = bCartWasEdited;
                        if(oldCartItemCount > cart.CartItems.Count()) // some sku got removed
                        {
                            ShoppingCartRuleResult result = new ShoppingCartRuleResult();
                            result.Result = RulesResult.Failure;
                            result.RuleName = "APF Rules";
                            result.AddMessage(
                                HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                                                    "CompleteAPFPurchase") as string);
                            cart.RuleResults.Add(result);
                            return results;
                        }
                    }
                    var myHerbalifeRule = rule as MyHerbalifeRule;
                    if (myHerbalifeRule != null && myHerbalifeRule.DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while processing ShoppingCart Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, name));
            }

            return results;
        }

        public List<ShoppingCartRuleResult> ValidateSKULimitation(MyHLShoppingCart cart, string locale, List<ShoppingCartItem_V01> skus)
        {
            string name = string.Empty;
            try
            {
                foreach (
                    IShoppingCartRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                {
                    name = rule.ToString();
                    if (name.Contains(".SKULimitations.Global."))
                    {
                        foreach (var s in skus)
                        {
                            var results = new List<ShoppingCartRuleResult>();
                            cart.CurrentItems = new ShoppingCartItemList();
                            cart.CurrentItems.Add(s);
                            results = rule.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
                            cart.RuleResults.AddRange(results);
                            cart.CurrentItems.Remove(s);
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while ValidateSKULimitation for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, name));
            }
            return cart.RuleResults;
        }

        public List<ShoppingCartRuleResult> ValidateHAPRules(MyHLShoppingCart cart, string locale)
        {
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process ShoppingCart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            string name = string.Empty;
            try
            {
                foreach (
                    IShoppingCartRule rule in
                        _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules)
                {
                    name = rule.ToString();
                    if (name.Contains(".HAP."))
                    {
                        results.AddRange(rule.ProcessCart(cart, ShoppingCartRuleReason.CartBeingPaid));
                        cart.RuleResults.AddRange(results);
                    }
                    var myHerbalifeRule = rule as MyHerbalifeRule;
                    if (myHerbalifeRule != null && myHerbalifeRule.DiscontinueOnError)
                    {
                        if (results.Any(rr => rr.Result == RulesResult.Failure))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while processing ShoppingCart Rules for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, name));
            }

            return results;
        }
        public List<ShoppingCartRuleResult> ValidateSKUForHAP(MyHLShoppingCart cart, string locale, List<ShoppingCartItem_V01> itemsToAdd)
        {
            var results = new List<ShoppingCartRuleResult>();
            if (null == cart)
            {
                string errorMessage =
                    string.Format("Cannot process ShoppingCart Rules for Locale {0}. The ShoppingCart is null", locale);
                LoggerHelper.Error(errorMessage);
                throw new ApplicationException(errorMessage);
            }
            string name = string.Empty;
            try
            {
                var rules = _platformRuleSets[HLConfigManager.Platform][locale].IShoppingCartRuleModules.Where(n => n.ToString().Contains(".HAP."));
                if (rules.Any())
                {
                    foreach(var r in rules)
                    {
                        cart.ItemsBeingAdded = new ShoppingCartItemList();
                        cart.ItemsBeingAdded.AddRange(itemsToAdd);
                        results.AddRange(r.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded));
                        cart.RuleResults.AddRange(results);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "An exception occurred in {4} while ValidateSKUForHAP for Distributor {0}, Locale {1}, Cart Id {2}, Exception details {3}",
                        cart.DistributorID, locale, cart.ShoppingCartID, ex.Message, "Hap Rules"));
            }

            return results;
        }
    }
}