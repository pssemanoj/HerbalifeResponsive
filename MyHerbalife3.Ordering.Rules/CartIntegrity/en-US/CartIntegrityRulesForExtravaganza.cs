using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Rules.CartIntegrity.en_US
{
    public class CartIntegrityRulesForExtravaganza : MyHerbalifeRule, IShoppingCartRule
    {
        private const string RuleName = "CartIntegrity Rules";
        private const string ExtravaganzaSkusCacheKeyUS = "ExtravaganzaSkus_US";
        private const int USExtravaganzaCacheMinutes = 60 * 1;
        private List<string> NamCountries= new List<string>(){"PR","CA","JM", "US"};
        /// <summary>
        /// Gets a value indicating whether this instance is us DS.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is us DS; otherwise, <c>false</c>.
        /// </value>
        public bool isUsDS
        {
            get { return DistributorProfileModel.ProcessingCountryCode == "US"; }
        }

        public bool isNamDS
        {
            get { return NamCountries.Contains(DistributorProfileModel.ProcessingCountryCode); }
        }

        /// <summary>
        /// Gets the extravaganza skus.
        /// </summary>
        /// <value>
        /// The extravaganza skus.
        /// </value>
        public List<string> extravaganzaSkus
        {
            get
            {
                var allProducts = HttpRuntime.Cache[ExtravaganzaSkusCacheKeyUS] as List<string>;
                if (null == allProducts)
                {
                    allProducts = new List<string>();
                    if (null != HLConfigManager.Configurations.DOConfiguration.ExtravaganzaCategoryName)
                    {
                        var prodictinfocatalog = CatalogProvider.GetProductInfoCatalog(Locale);
                        Category_V02 cat =
                            prodictinfocatalog.RootCategories.FirstOrDefault(x => x.DisplayName.Contains(HLConfigManager.Configurations.DOConfiguration.ExtravaganzaCategoryName.ToString()));
                        if (null != cat)
                        {
                            allProducts = getSkus(cat);
                            if (allProducts != null && allProducts.Count > 0)
                            {
                                HttpRuntime.Cache.Insert(ExtravaganzaSkusCacheKeyUS, allProducts, null,
                                                         DateTime.Now.AddMinutes(USExtravaganzaCacheMinutes),
                                                         Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                            }
                        }
                    }
                }
                return allProducts;
            }
        }

        private List<string> getSkus(Category_V02 category)
        {
            List<string> Lkus= new List<string>();
            if(category.Products != null)
            {
                var listin = category.Products;
                foreach (var productInfoV02 in category.Products)
                {
                    Lkus.AddRange(productInfoV02.SKUs.Select(x => x.SKU));
                }
            }
            if (category.SubCategories != null && category.SubCategories.Count() >= 1)
            {
                foreach (var subcat in category.SubCategories)
                {
                    Lkus.AddRange(getSkus(subcat));   
                }
            }
            return Lkus;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult result)
        {
            var shoppingCart = cart as MyHLShoppingCart;

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (extravaganzaSkus.Contains(cart.CurrentItems[0].SKU))
                {
                    //only for NAM DS can buy Extravaganza Items
                    if (!isNamDS)
                    {
                        if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                            && shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                        {
                            var message =
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "NoPickupSelectedAllowed")
                                               .ToString());
                            result.AddMessage(message);
                            result.Result = RulesResult.Failure;
                            RemoveAllItems(shoppingCart);
                            return result;
                        }
                        if (extravaganzaSkus.Contains(cart.CurrentItems[0].SKU))
                        {
                            RemoveExtravaganzaItems(shoppingCart);
                            var message =
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CantPurchase")
                                               .ToString());
                            result.AddMessage(message);
                            result.Result = RulesResult.Failure;
                            return result;
                        }
                    } //end for non NAM DS

                    //non standalone, only shipping
                    if (isUsDS)
                    {
                        var extravaganzaItemsinCart = from item in shoppingCart.CartItems
                                                      where extravaganzaSkus.Contains(item.SKU)
                                                      select item;
                        if (extravaganzaItemsinCart.Any())
                        {
                            if (shoppingCart.DeliveryInfo.Option != DeliveryOptionType.Shipping)
                            {
                                //RemoveExtravaganzaItems(shoppingCart);
                                var message =
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "OnlyShippingAllowed")
                                                   .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                                return result;
                            }
                        }
                    }

                    //non US DS ans NAM ds PU allowed only standalone order
                    if (isNamDS && !isUsDS)
                    {
                        if (shoppingCart.DeliveryInfo.Option != DeliveryOptionType.Pickup)
                        {
                            RemoveExtravaganzaItems(shoppingCart);

                            var message = string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OnlyPickUpAllowed")
                                           .ToString());
                            result.AddMessage(message);
                            result.Result = RulesResult.Failure;
                        }
                        else
                        {
                            //check if PU for extravaganza is selected
                            if (!shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                            {
                                RemoveExtravaganzaItems(shoppingCart);
                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "OnlyPickUpAllowed")
                                               .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }
                        }
                        if (!extravaganzaSkus.Contains(cart.CurrentItems[0].SKU))
                        {
                            //DS will be able to place order for products and event merchandise – apparel. 
                            //These will be standalone anc cannot be combined with regular product orders
                            //if (!cart.CartItems.Any(f => extravaganzaSkus.Contains(f.SKU)) && cart.CartItems.Count > 0)
                            {
                                var message =
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                            "ExtravaganzaStandaloneOrder")
                                                   .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }
                        }
                    } //End for non US ds ans NAM ds
                }
                //if they want to add items != extravaganza
                else
                {
                    //non US DS ans NAM ds PU allowed only standalone order
                    if (isNamDS && !isUsDS)
                    {
                        var extravaganzaItemsinCart = from item in shoppingCart.CartItems
                                                      where extravaganzaSkus.Contains(item.SKU)
                                                      select item;
                        if (extravaganzaItemsinCart.Any())
                        {

                            if (!extravaganzaSkus.Contains(cart.CurrentItems[0].SKU))
                            {
                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "ExtravaganzaStandaloneOrder")
                                               .ToString()); //"no shipping para extravaganza";
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }

                            if (shoppingCart.DeliveryInfo.Option != DeliveryOptionType.Pickup)
                            {
                                RemoveExtravaganzaItems(shoppingCart);

                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OnlyPickUpAllowed")
                                               .ToString()); //"no shipping para extravaganza";
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }
                            else if (!shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                            {
                                RemoveExtravaganzaItems(shoppingCart);
                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "OnlyPickUpAllowed")
                                               .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }
                        }
                       else
                        {
                            //check if PU for extravaganza is selected
                            if (shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                            {
                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "OnlyPickUpAllowed")
                                               .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Failure;
                            }
                        }
                        
                    } //End for non US ds ans NAM ds
                }
            }

            if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (null != shoppingCart.DeliveryInfo)
                {
                    if (!isNamDS && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup &&
                        shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                    {
                        var message =
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "NoPickupSelectedAllowed")
                                           .ToString());
                        result.AddMessage(message);
                        result.Result = RulesResult.Failure;
                        RemoveAllItems(shoppingCart);
                        return result;
                    }
                    var extravaganzaItemsinCart = from item in shoppingCart.CartItems
                                                  where extravaganzaSkus.Contains(item.SKU)
                                                  select item;
                    if (isUsDS && shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup && extravaganzaItemsinCart.Any())
                    {
                        //RemoveExtravaganzaItems(shoppingCart);

                    }
                }
            }

            if (reason == ShoppingCartRuleReason.CartWarehouseCodeChanged)
            {
                //is not nam ds only valid to not select extravaganza pick up
                if (!isNamDS)
                {
                    if (shoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup
                        && shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                    {
                        var message =
                            string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "NoPickupSelectedAllowed")
                                           .ToString());
                        result.AddMessage(message);
                        result.Result = RulesResult.Failure;
                        RemoveAllItems(shoppingCart);
                        return result;
                    }
                }
                //US ds valid select only shipping
                if (isUsDS )
                {
                    var extravaganzaItemsinCart = from item in shoppingCart.CartItems
                                  where extravaganzaSkus.Contains(item.SKU)
                                  select item;
                    if (extravaganzaItemsinCart.Any())
                    {
                        if (shoppingCart.DeliveryInfo.Option != DeliveryOptionType.Shipping)
                        {
                            RemoveExtravaganzaItems(shoppingCart);
                            var message =
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "OnlyShippingAllowed")
                                               .ToString());
                            result.AddMessage(message);
                            result.Result = RulesResult.Failure;
                            return result;
                        }
                    }
                }
                //non usds and nam ds check only pu allowed
                //non US DS ans NAM ds PU allowed only standalone order
                if (isNamDS && !isUsDS)
                {
                    //check if the cart contain extravaganza skus
                    var evItemsInCart = from item in shoppingCart.CartItems
                                  where extravaganzaSkus.Contains(item.SKU)
                                  select item;

                    if (evItemsInCart.Any())
                    {
                        if (shoppingCart.DeliveryInfo.Option != DeliveryOptionType.Pickup)
                        {
                            RemoveExtravaganzaItems(shoppingCart);

                            var message = string.Format(
                                HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OnlyPickUpAllowed")
                                           .ToString()); //"no shipping para extravaganza";
                            result.AddMessage(message);
                            result.Result = RulesResult.Failure;
                        }
                        else
                        {
                            //check if PU for extravaganza is selected
                            if (!shoppingCart.DeliveryInfo.Description.Contains("Extravaganza"))
                            {
                                RemoveExtravaganzaItems(shoppingCart);
                                var message = string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                        "OnlyPickUpAllowed")
                                               .ToString());
                                result.AddMessage(message);
                                result.Result = RulesResult.Feedback;
                            }
                        }
                    }
                }//End for non US ds ans NAM ds
            }
            return result;
        }
        
        private void RemoveExtravaganzaItems(MyHLShoppingCart shoppingCart)
        {
            var evItemsToRemove = from item in shoppingCart.CartItems
                                  where extravaganzaSkus.Contains(item.SKU)
                                  select item;

            if (evItemsToRemove.Any())
            {
                var notValidSkus = evItemsToRemove.Select(s => s.SKU).ToList();
                shoppingCart.DeleteItemsFromCart(notValidSkus, true);
            }
        }

        private void RemoveAllItems(MyHLShoppingCart shoppingCart)
        {
            var evItemsToRemove = from item in shoppingCart.CartItems
                                  select item;

            if (evItemsToRemove.Any())
            {
                var notValidSkus = evItemsToRemove.Select(s => s.SKU).ToList();
                shoppingCart.DeleteItemsFromCart(notValidSkus, true);
            }
        }
    }
}