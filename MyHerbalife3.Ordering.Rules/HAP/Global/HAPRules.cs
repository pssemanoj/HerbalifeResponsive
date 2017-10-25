using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.HAP.Global
{
    class HAPRules : MyHerbalifeRule, IShoppingCartRule
    {
        #region const
        private const string RuleName = "HAP Rules";
        private const decimal MIN_VP = 100;
        private const decimal MAX_VP = 1000;
        #endregion

        Shared.Infrastructure.Interfaces.IGlobalContext globalContext;

        #region IShoppingCartRule interface implementation
        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            globalContext = (Shared.Infrastructure.Interfaces.IGlobalContext)HttpContext.Current.ApplicationInstance;
            var result = new List<ShoppingCartRuleResult>();
            if (null == cart.RuleResults)
            {
                cart.RuleResults = new List<ShoppingCartRuleResult>();
            }
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = RuleName;
            defaultResult.Result = RulesResult.Unknown;

            if (null != cart)
            {
                result.Add(PerformRules(cart, defaultResult, reason));
            }

            return result;
        }
        #endregion

        #region private methods
        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleResult result, ShoppingCartRuleReason reason)
        {
            try
            {
                if (cart != null && cart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
                {
                    switch (reason)
                    {
                        case ShoppingCartRuleReason.CartItemsBeingAdded:
                            result = CanAddProducts(ref cart, result);
                            break;
                        case ShoppingCartRuleReason.CartBeingPaid:
                            result = CanCheckout(ref cart, result);
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("MyHerbalife3.Ordering.Rules.HAP.Global - PermorfRules: {0}", ex.Message));
            }
            return result;
        }

        private ShoppingCartRuleResult CanAddProducts(ref ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {
            //i.	A China MB cannot setup HAP order for US or Canada
            if (DistributorProfileModel.ProcessingCountry == "CN")
            {
                result.Result = RulesResult.Errors;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "CantOrder") as string);
            }

            MyHLShoppingCart myCart = cart as MyHLShoppingCart;
            if (myCart.ItemsBeingAdded == null || myCart.ItemsBeingAdded.Count == 0)
                return result;
            if (myCart.DsType == null)
            {
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(cart.DistributorID, Country);
                myCart.DsType = DistributorType;
            }

            if (globalContext.CultureConfiguration.IsBifurcationEnabled)
            {
                if (myCart.DsType == Scheme.Member)
                {
                    myCart.HAPType = "01";
                }
                else
                {
                    var session = SessionInfo.GetSessionInfo(myCart.DistributorID, Locale);
                    if (session != null && session.HAPOrderType != null)
                    {
                        if (session.HAPOrderType == "Personal")
                        {
                            myCart.HAPType = "01";
                        }
                        else if (session.HAPOrderType == "RetailOrder")
                        {
                            myCart.HAPType = "02";
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(myCart.HAPType))
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "MissingHapType") as string);
                myCart.ItemsBeingAdded.Clear();
                return result;
            }

            if (myCart.HAPScheduleDay == 0)
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "MissingHapDate") as string);
                myCart.ItemsBeingAdded.Clear();
                return result;
            }

            //calculate VP
            var vp = getTotalvolumePoints(myCart, this.Country);

            // Generic HAP Order rules
            // HAP orders are allowed for Product type (“P”) items only
            if (!verifyProducttypes(myCart, this.Country))
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "HAPOrderAddingNonProducts") as string);
                myCart.ItemsBeingAdded.Clear();
            }
            // Validate is not expired
            if (isExpired(cart))
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    "HAPOrderExpired") as string);
                myCart.ItemsBeingAdded.Clear();
            }

            // Personal HAP Order
            // Personal HAP order is allowed a maximum of 1000 VP
            // There is a Maximum quantity of 6 for any individual selling SKU on personal consumption HAP orders

            //check if is a personal hap order with order subtype
            //01 = Personal / 02 = Resale
            if (myCart.HAPType == "01")
            {
                if (vp > MAX_VP)
                {
                    result.Result = RulesResult.Failure;
                    result.AddMessage(HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform), 
                        myCart.DsType == Scheme.Member 
                            ? "HAPOrderExceedVPForMB" 
                            : "HAPOrderExceedVP") as string);
                    myCart.ItemsBeingAdded.Clear();
                }
                if (myCart.ItemsBeingAdded != null && myCart.ItemsBeingAdded.Count > 0)
                {
                    foreach (var a in myCart.ItemsBeingAdded)
                    {
                        var itemInCard = cart.CartItems.Find(c => c.SKU == a.SKU);
                        if (globalContext.CultureConfiguration.IsBifurcationEnabled && myCart.DsType == Scheme.Member)
                        {
                            if (a.Quantity > 4 || (itemInCard != null && (itemInCard.Quantity + a.Quantity) > 4))
                            {

                                result.Result = RulesResult.Failure;
                                result.AddMessage(HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "HAPOrderExceedSKUForMember") as string);
                                myCart.ItemsBeingAdded.Clear();
                                break;
                            }

                        }
                        else if (a.Quantity > 6 || (itemInCard != null && (itemInCard.Quantity + a.Quantity) > 6))
                        {
                            result.Result = RulesResult.Failure;
                            result.AddMessage(HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                "HAPOrderExceedSKU") as string);
                            myCart.ItemsBeingAdded.Clear();
                            break;
                        }
                    }
                }
            }
            // Resale HAP Order
            //For HAP Resale, there is a minimum order restriction of 100 VP, but there is no maximum VP limit and no SKU quantity limitations
            return result;
        }

        private ShoppingCartRuleResult CanCheckout(ref ShoppingCart_V02 cart, ShoppingCartRuleResult result)
        {

            MyHLShoppingCart myCart = cart as MyHLShoppingCart;
            var vp = (myCart.Totals as OrderTotals_V01) != null ? (myCart.Totals as OrderTotals_V01).VolumePoints : getTotalvolumePoints(cart as MyHLShoppingCart, this.Country);

            //HAP order is allowed a minimum of 100 VP
            if (vp < MIN_VP)
            {
                result.Result = RulesResult.Failure;
                result.AddMessage(HttpContext.GetGlobalResourceObject(
                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                    myCart.DsType == Scheme.Member
                        ? "HAPOrderLessVPForMB"
                        : "HAPOrderLessVP") as string);
            }

            if (myCart.HAPType == "01")
            {
                if (vp > MAX_VP)
                {
                    result.Result = RulesResult.Failure;
                    result.AddMessage(HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                        myCart.DsType == Scheme.Member
                            ? "HAPOrderExceedVPForMB"
                            : "HAPOrderExceedVP") as string);
                }
                if (myCart.CartItems != null && myCart.CartItems.Count > 0)
                {
                    foreach (var a in myCart.CartItems)
                    {
                        if (globalContext.CultureConfiguration.IsBifurcationEnabled && myCart.DsType == Scheme.Member)
                        {
                            if (a.Quantity > 4)
                            {
                                result.Result = RulesResult.Failure;
                                result.AddMessage(HttpContext.GetGlobalResourceObject(
                                    string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                    "HAPOrderExceedSKUForMember") as string);

                                break;
                            }
                        }
                        else if (a.Quantity > 6)
                        {
                            result.Result = RulesResult.Failure;
                            result.AddMessage(HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                "HAPOrderExceedSKU") as string);
                            break;
                        }
                    }
                }
            }
            return result;
        }

        private decimal getTotalvolumePoints(MyHLShoppingCart cart, string countryCode)
        {
            decimal vp = 0.0M;
            vp = cart.VolumeInCart;
            if (cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
            {
                foreach (ShoppingCartItem_V01 CartItem in cart.ItemsBeingAdded)
                {
                    var item = CatalogProvider.GetCatalogItem(CartItem.SKU, countryCode);
                    if (item != null)
                    {
                        vp += (item.VolumePoints) * CartItem.Quantity;
                    }
                }
            }

            return vp;
        }

        private bool verifyProducttypes(MyHLShoppingCart cart, string countryCode)
        {
            //check all the skus in cart
            foreach (ShoppingCartItem_V01 CartItem in cart.CartItems)
            {
                var item = CatalogProvider.GetCatalogItem(CartItem.SKU, countryCode);
                if (item.ProductType != ServiceProvider.CatalogSvc.ProductType.Product)
                {
                    return false;
                }
            }
            //checking the sku to be added
            foreach (ShoppingCartItem_V01 CartItem in cart.ItemsBeingAdded)
            {
                string sku = CartItem.SKU;
                var item = CatalogProvider.GetCatalogItem(sku, countryCode);
                if (item.ProductType != ServiceProvider.CatalogSvc.ProductType.Product)
                {
                    return false;
                }
            }

            return true;
        }

        private bool isExpired(ShoppingCart_V02 cart)
        {
            //TODO: HAP Rule add logic to determine if is a Expired HAP order
            //TBD
            //getHapExpirationDate
            return false;
        }

        private void RemoveItemsAdded(ref ShoppingCart_V02 cart)
        {
            var hlCart = cart as MyHLShoppingCart;
            ShoppingCartItemList cartItems = new ShoppingCartItemList();
            cartItems.AddRange(hlCart.CartItems);

            foreach (ShoppingCartItem_V01 item in hlCart.CurrentItems)
            {
                if (cartItems.Exists(i => i.SKU == item.SKU && i.Quantity >= item.Quantity))
                {
                    cartItems.FirstOrDefault(i => i.SKU == item.SKU).Quantity -= item.Quantity;
                    if (cartItems.FirstOrDefault(i => i.SKU == item.SKU).Quantity < 1)
                        cartItems.Remove(cartItems.FirstOrDefault(i => i.SKU == item.SKU));
                }
            }

            // Clear Cart and Re Add initial items
            hlCart.DeleteItemsFromCart((from i in hlCart.CartItems select i.SKU).ToList<string>());
            hlCart.AddItemsToCart(cartItems, true);

            cart = hlCart;
        }
        #endregion
    }
}
