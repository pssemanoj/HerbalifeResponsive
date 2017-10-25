
using System.Linq;
using HL.Catalog.ValueObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Test.Helpers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.Providers;
using providers = MyHerbalife3.Ordering.Providers.RulesManagement;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.SessionState;
    using HL.Common.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading;
    using System.Globalization;

    [TestClass]
    public class PromotionalRulesTests
    {
        #region Constants and Fields

        private const string PromoSku = "U320";

        private const string PromoU688 = "U688";

        #endregion

        // private string promoSkuU659 = "U659";

        #region Public Methods and Operators

        [TestInitialize]
        public void Initialize()
        {
            var httpRequest = new HttpRequest(
                AppDomain.CurrentDomain.BaseDirectory, "http://www.local.myherbalife.com/Default.aspx", "");
            var stringWriter = new StringWriter();
            var httpResponce = new HttpResponse(stringWriter);

            var httpContext = new HttpContext(httpRequest, httpResponce);

            var sessionContainer = new HttpSessionStateContainer(
                "id",
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(),
                10,
                true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc,
                false);

            httpContext.Items["AspSession"] =
                typeof(HttpSessionState).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    CallingConventions.Standard,
                    new[] { typeof(HttpSessionStateContainer) },
                    null).Invoke(new object[] { sessionContainer });

            HttpContext.Current = httpContext;
        }

        [TestMethod]
        public void ProcessCartTest_Italy()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "it-IT";

            // Getting a shopping cart for IT.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.it_IT.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartCreated);
            
            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Created");
        }

        [TestMethod]
        public void ProcessCartTest_Italy_CartItemsAdded()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "it-IT";

            // Getting a shopping cart for IT.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.it_IT.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsAdded);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Added");
        }

        [TestMethod]
        public void ProcessCartTest_Italy_CartItemsBeingAdded()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "it-IT";

            // Getting a shopping cart for IT.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
               OrderCategoryType.RSO,
               DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.it_IT.PromotionalRules();

            shoppingCart.CurrentItems = new ShoppingCartItemList();
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            shoppingCart.CurrentItems.Add(
                new ShoppingCartItem_V01(1, PromoSku, 10, DateTime.Now));

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Added");
        }

        [TestMethod]
        public void ProcessCartTest_Italy_CartItemsBeingRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "it-IT";

            // Getting a shopping cart for IT.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.it_IT.PromotionalRules();

            shoppingCart.CurrentItems = new ShoppingCartItemList();
            shoppingCart.CurrentItems.Add(
                new ShoppingCartItem_V01(1, PromoU688, 10, DateTime.Now));

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Italy_CartItemsRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "it-IT";
            // Getting a shopping cart for IT.
            var shoppingCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(Distributor, Locale, "FREIGHT", null, false, null, new List<DistributorShoppingCartItem>
                    {
                        ShoppingCartItemHelper.GetCatalogItems(1, 10, PromoU688, Locale)
                    }, OrderCategoryType.RSO);
            var ruleEngine = new Ordering.Rules.Promotional.it_IT.PromotionalRules();
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Russia()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "ru-RU";

            // Getting a shopping cart for RU.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.ru_RU.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartCreated);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Created");
        }

        [TestMethod]
        public void ProcessCartTest_Russia_CartItemsAddedOrRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "ru-RU";

            // Getting a shopping cart for RU.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.ru_RU.PromotionalRules();

            shoppingCart.CurrentItems = new ShoppingCartItemList();
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            shoppingCart.CurrentItems.Add(
                new ShoppingCartItem_V01(1, PromoSku, 10, DateTime.Now));

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Added Or Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Russia_CartItemsBeingAdded()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "ru-RU";

            // Getting a shopping cart for RU.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.ru_RU.PromotionalRules();

            shoppingCart.CurrentItems = new ShoppingCartItemList();
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            shoppingCart.CurrentItems.Add(
                new ShoppingCartItem_V01(1, PromoSku, 10, DateTime.Now));

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Added");
        }

        [TestMethod]
        public void ProcessCartTest_Russia_CartItemsBeingRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "ru-RU";

            // Getting a shopping cart for RU.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.ru_RU.PromotionalRules();

            shoppingCart.CurrentItems = new ShoppingCartItemList();
            shoppingCart.RuleResults = new List<ShoppingCartRuleResult>();
            shoppingCart.CurrentItems.Add(
                new ShoppingCartItem_V01(1, PromoSku, 10, DateTime.Now));

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Turky_CartItemsBeingAdded()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "tr-TR";

            // Getting a shopping cart for TR.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.tr_TR.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingAdded);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Turky_CartItemsBeingRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "tr-TR";

            // Getting a shopping cart for TR.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.tr_TR.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsBeingRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        [TestMethod]
        public void ProcessCartTest_Turky_CartItemsRemoved()
        {
            // Used constants.
            const string Distributor = "25657863";
            const string Locale = "tr-TR";

            // Getting a shopping cart for TR.
            var shoppingCart = MyHLShoppingCartGenerator.GetShoppingCart(
                12345,
                DateTime.Now,
                Locale,
                1,
                OrderCategoryType.RSO,
                DeliveryOptionType.Pickup,
                5,
                Distributor,
                "FREIGHT",
                "OrderType");
            var ruleEngine = new Ordering.Rules.Promotional.tr_TR.PromotionalRules();

            // Getting a result.
            var result = ruleEngine.ProcessCart(
                shoppingCart, ShoppingCartRuleReason.CartItemsRemoved);

            // Asserts.
            Assert.IsNotNull(result, "Cart Processed as Cart Items Being Removed");
        }

        #region IT Aloe Promotional Rule

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsAdded_ElegibleToPromo()
        {
            var settings = new OrderingTestSettings("it-IT", "1111111111");
            var skus = new List<string> { "0141", "2561" };
            var promoAloeSku = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalSku;
            var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 1),
                OrderCategoryType.RSO);
            MyHLShoppingCartGenerator.PrepareAddToCart(itCart, ShoppingCartItemHelper.GetCartItem(1, 10, "2561"));

            var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();
            target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsAdded);
            var ruleResult = itCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
            {
                Assert.IsNull(itCart.CartItems.FirstOrDefault(c => c.SKU.Equals(promoAloeSku)), "There is not promo sku in catalog items");
                return;
            }
            Assert.AreEqual(true, ruleResult != null && ruleResult.Result == RulesResult.Success);
        }

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsAdded_AdjustingPromo()
        //{
        //    var settings = new OrderingTestSettings("it-IT", "1111111111");
        //    var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();

        //    var skus = new List<string> { "0118", "2561", target.PromotionalSku };
        //    var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 10),
        //        OrderCategoryType.RSO);
        //    MyHLShoppingCartGenerator.PrepareAddToCart(itCart, ShoppingCartItemHelper.GetCartItem(1, 10, "2562"));

        //    target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsAdded);
        //    var ruleResult = itCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
        //    if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
        //    {
        //        //Assert.IsNull(itCart.CartItems.FirstOrDefault(c => c.SKU.Equals(target.PromotionalSKU)), "There is not promo sku in catalog items");
        //        return;
        //    }

        //    //Assert.AreEqual(true, ruleResult != null && ruleResult.Result == HL.RulesResult.Success);
        //}

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsRemoved_AdjustingPromo()
        //{
        //    var settings = new OrderingTestSettings("it-IT", "1111111111");
        //    var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();

        //    var skus = new List<string> { "0141", "2561", target.PromotionalSku };
        //    var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 1),
        //        OrderCategoryType.RSO);

        //    target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsRemoved);
        //    var ruleResult = itCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
        //    if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
        //    {
        //        //Assert.IsNull(itCart.CartItems.FirstOrDefault(c => c.SKU.Equals(target.PromotionalSKU)), "There is not promo sku in catalog items");
        //        return;
        //    }

        //    //Assert.AreEqual(true, ruleResult != null && ruleResult.Result == HL.RulesResult.Success);
        //}

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsBeingRemoved_PromoSku()
        {
            var settings = new OrderingTestSettings("it-IT", "1111111111");
            var promoAloeSku = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalSku;
            var skus = new List<string> { "0118", "2561", "2562", promoAloeSku };
            var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 1),
                OrderCategoryType.RSO);
            MyHLShoppingCartGenerator.PrepareAddToCart(itCart, ShoppingCartItemHelper.GetCartItem(1, 1, promoAloeSku));

            var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();
            target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsBeingRemoved);
            Assert.IsNotNull(itCart.CartItems.Any(c => c.SKU.Equals(promoAloeSku)));
        }

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsBeingAdded_PromoSku()
        //{
        //    var settings = new OrderingTestSettings("it-IT", "1111111111");
        //    var promoAloeSku = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalSku = "0141";
        //    var skus = new List<string> { "2561", "2562" };
        //    var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 1),
        //        OrderCategoryType.RSO);

        //    var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();
        //    MyHLShoppingCartGenerator.PrepareAddToCart(itCart, ShoppingCartItemHelper.GetCartItem(1, 10, target.PromotionalSku));

        //    target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsBeingAdded);
        //    Assert.AreEqual(true, itCart.CurrentItems.Count == 0);
        //}

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsRemoved_PromoNotStarted()
        {
            var settings = new OrderingTestSettings("it-IT", "1111111111");
            var promoAloeSku = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalSku = "X946";
            var beginDate = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalBeginDate = "2013-07-01";
            var skus = new List<string> { "2561", "2562" };
            var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 10),
                OrderCategoryType.RSO);

            var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();
            target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsRemoved);
            var ruleResult = itCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
            {
                Assert.IsNull(itCart.CartItems.FirstOrDefault(c => c.SKU.Equals(promoAloeSku)), "There is not promo sku in catalog items");
                return;
            }
            MyHLShoppingCartGenerator.PrepareAddToCart(itCart, ShoppingCartItemHelper.GetCartItem(1, 1, "2562"));
            Assert.AreEqual(true, ruleResult != null && ruleResult.Messages.Any(m => m.Equals("PromoOutOfDate")));
        }

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_IT_Aloe_CartItemsRemoved_PromoEneded()
        {
            var settings = new OrderingTestSettings("it-IT", "1111111111");
            var promoAloeSku = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalSku = "0141";
            var endDate = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalEndDate = "2013-06-26";

            var skus = new List<string> { "2561", "2562" };
            var itCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
                ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 10),
                OrderCategoryType.RSO);

            var target = new Ordering.Rules.Promotional.it_IT.PromotionalRulesAloe();
            target.ProcessCart(itCart, ShoppingCartRuleReason.CartItemsRemoved);
            var ruleResult = itCart.RuleResults.FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
            {
                Assert.IsNull(itCart.CartItems.FirstOrDefault(c => c.SKU.Equals(promoAloeSku)), "There is not promo sku in catalog items");
                return;
            }
            Assert.AreEqual(true, ruleResult != null && ruleResult.Messages.Any(m => m.Equals("PromoOutOfDate")));
        }

        #endregion

        #region HK Freight Code Promotional Rule

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_HK_CartItemsAdded_WithPromo()
        //{
        //    var settings = new OrderingTestSettings("zh-HK", "1111111111");
        //    var skus = new List<string> { "0118", "0119", "1029" };
        //    var promoFreightCode = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalFreightCode;
        //    var promoRequiredVolume = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalRequiredVolumePoints;
        //    var hkCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 20),
        //        OrderCategoryType.RSO);
        //    MyHLShoppingCartGenerator.PrepareAddToCart(hkCart, ShoppingCartItemHelper.GetCartItem(1, 1, "0118"));

        //    var target = new Ordering.Rules.Promotional.zh_HK.PromotionalRules();
        //    target.ProcessCart(hkCart, ShoppingCartRuleReason.CartItemsAdded);
        //    if(!string.IsNullOrEmpty(promoFreightCode))
        //    Assert.AreEqual(true, hkCart.VolumeInCart >= promoRequiredVolume && hkCart.DeliveryInfo.FreightCode.Equals(promoFreightCode));
        //    else
        //      Assert.AreEqual("",promoFreightCode,"Might be Promofrightcode is not configured.");
        //}

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_HK_CartItemsRemoved_WithPromo()
        //{
        //    var settings = new OrderingTestSettings("zh-HK", "1111111111");
        //    var skus = new List<string> { "0118", "0119", "1029", "3516" };
        //    var promoFreightCode = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalFreightCode;
        //    var promoRequiredVolume = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalRequiredVolumePoints;
        //    var hkCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 20),
        //        OrderCategoryType.RSO);

        //    var target = new Ordering.Rules.Promotional.zh_HK.PromotionalRules();
        //    target.ProcessCart(hkCart, ShoppingCartRuleReason.CartItemsRemoved);
        //    if (!string.IsNullOrEmpty(promoFreightCode))
        //    Assert.AreEqual(true, hkCart.VolumeInCart >= promoRequiredVolume && hkCart.DeliveryInfo.FreightCode.Equals(promoFreightCode));
        //    else
        //        Assert.AreEqual("", promoFreightCode, "Might be Promofrightcode is not configured.");
        //}

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_HK_CartItemsAdded_WithoutPromo()
        //{
        //    var settings = new OrderingTestSettings("zh-HK", "1111111111");
        //    var skus = new List<string> { "0118", "0119", "1029" };
        //    var promoFreightCode = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalFreightCode;
        //    var promoRequiredVolume = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalRequiredVolumePoints;
        //    var hkCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 10),
        //        OrderCategoryType.RSO);
        //    MyHLShoppingCartGenerator.PrepareAddToCart(hkCart, ShoppingCartItemHelper.GetCartItem(1, 1, "0118"));

        //    var target = new Ordering.Rules.Promotional.zh_HK.PromotionalRules();
        //    target.ProcessCart(hkCart, ShoppingCartRuleReason.CartItemsAdded);
        //    if (!string.IsNullOrEmpty(promoFreightCode))
        //    Assert.AreEqual(true, hkCart.VolumeInCart < promoRequiredVolume && !hkCart.DeliveryInfo.FreightCode.Equals(promoFreightCode));
        //    else
        //        Assert.AreEqual("", promoFreightCode, "Might be Promofrightcode is not configured.");
        //}

        //[TestMethod]
        //public void PromotionalRules_ProcessCartTest_HK_CartItemsRemoved_WithoutPromo()
        //{
        //    var settings = new OrderingTestSettings("zh-HK", "1111111111");
        //    var skus = new List<string> { "0118", "0119", "1029", "3516" };
        //    var promoFreightCode = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalFreightCode;
        //    var promoRequiredVolume = HLConfigManager.CurrentPlatformConfigs[settings.Locale].ShoppingCartConfiguration.PromotionalRequiredVolumePoints;
        //    var hkCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(settings.Distributor, settings.Locale, null, null, false, null,
        //        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(settings.Locale, skus, skus.Count, 5),
        //        OrderCategoryType.RSO);

        //    var target = new Ordering.Rules.Promotional.zh_HK.PromotionalRules();
        //    target.ProcessCart(hkCart, ShoppingCartRuleReason.CartItemsRemoved);
        //    if (!string.IsNullOrEmpty(promoFreightCode))
        //    Assert.AreEqual(false, hkCart.VolumeInCart < promoRequiredVolume && !hkCart.DeliveryInfo.FreightCode.Equals(promoFreightCode));
        //    else
        //        Assert.AreEqual("", promoFreightCode, "Might be Promofrightcode is not configured.");
        //}

        #endregion

        #region SG Promotional Rule

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_SG_NotStarted()
        {
            var locale = "en-SG";
            var distributorId = "1111111111";
            HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalBeginDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            var skus = new List<string> { "2561", "2562" };
            var sgCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorId, locale, null, null, false, null,
                                                                        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(locale, skus, skus.Count, 1),
                                                                        OrderCategoryType.RSO);
            var target = new Ordering.Rules.Promotional.en_SG.PromotionalRules();
            MyHLShoppingCartGenerator.PrepareAddToCart(sgCart, ShoppingCartItemHelper.GetCartItem(1, 1, "2561"));
            var ruleResult = target.ProcessCart(sgCart, ShoppingCartRuleReason.CartItemsAdded).FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            Assert.AreEqual(true, ruleResult != null && ruleResult.Result == RulesResult.Unknown);
        }

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_SG_PromoEnded()
        {
            var locale = "en-SG";
            var distributorId = "1111111111";
            HLConfigManager.Configurations.ShoppingCartConfiguration.PromotionalEndDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var skus = new List<string> { "2561", "2562" };
            var sgCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorId, locale, null, null, false, null,
                                                                        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(locale, skus, skus.Count, 1),
                                                                        OrderCategoryType.RSO);
            var target = new Ordering.Rules.Promotional.en_SG.PromotionalRules();
            MyHLShoppingCartGenerator.PrepareAddToCart(sgCart, ShoppingCartItemHelper.GetCartItem(1, 1, "2561"));
            var ruleResult = target.ProcessCart(sgCart, ShoppingCartRuleReason.CartItemsAdded).FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            Assert.AreEqual(true, ruleResult != null && ruleResult.Result == RulesResult.Unknown);
        }

        [TestMethod]
        public void PromotionalRules_ProcessCartTest_SG_CartItemsAdded_AddingPromoZ479()
        {
            var locale = "en-SG";
            var distributorId = "1111111111";
            var skus = new List<string> { "0141", "Z478" };
            var promoSku = "Z479";
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(locale);
            var sgCart = MyHLShoppingCartGenerator.GetBasicShoppingCart(distributorId, locale, null, null, false, null,
                                                                        ShoppingCartItemHelper.GetDistributorShoppingCartItemList(locale, skus, skus.Count, 1),
                                                                        OrderCategoryType.RSO);
            var target = new Ordering.Rules.Promotional.en_SG.PromotionalRules();
            var ruleResult = target.ProcessCart(sgCart, ShoppingCartRuleReason.CartItemsAdded).FirstOrDefault(r => r.RuleName.Equals("Promotional Rules"));
            if (ruleResult != null && ruleResult.Result == RulesResult.Feedback)
            {
                var message = string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "NoPromoSku").ToString(), "SG", "Z479");
                Assert.AreEqual(true, ruleResult != null && ruleResult.Messages.Any(m => m.Equals(message)));
                return;
            }
            Assert.AreEqual(true, ruleResult != null && ruleResult.Result == RulesResult.Success);
        }

        #endregion

        #endregion
    }
}