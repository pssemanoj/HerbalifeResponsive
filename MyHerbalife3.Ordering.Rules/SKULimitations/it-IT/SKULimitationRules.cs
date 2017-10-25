using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.SKULimitations.it_IT
{
    public class SKULimitationsRules : MyHerbalifeRule, IShoppingCartRule
    {
        private readonly Dictionary<string, List<string>> _IBPAllowedSubTypes = new Dictionary<string, List<string>>();

        public SKULimitationsRules()
        {
            _IBPAllowedSubTypes.Add("5564", new List<string> { "A1", "B1", "C", "D", "D2", "E" });
            _IBPAllowedSubTypes.Add("5565", new List<string> { "C" });
            _IBPAllowedSubTypes.Add("5451", new List<string>() { "A1", "B1", "C", "D", "D2", "E" });
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            var result = new List<ShoppingCartRuleResult>();
            var defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "SkuLimitation Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult ShippingTemperatureControlled(ShoppingCart_V01 cart,
                                                                     ShoppingCartRuleResult Result)
        {
            var SKUIds = new List<string> { "Q638", "Q639", "Q640", "Q641" };
            if (!SKUIds.Any(s => s == cart.CurrentItems[0].SKU))
            {
                return Result;
            }
            try
            {
                var x = (from a in APFDueProvider.GetAPFSkuList()
                         from b in cart.CartItems
                         from c in SKUIds
                         where
                             b.SKU != c && b.SKU != a &&
                             b.SKU != HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                         select b);
                if (x.Count() > 0)
                {
                    Result.Result = RulesResult.Feedback;
                    Result.AddMessage(
                        string.Format(
                            "Il codice {0} non può essere ordinato insieme ad altri prodotti. Se hai già inserito altri prodotti nel carrello, rimuovili prima di completare il tuo ordine. La mancata rimozione dal carrello degli altri prodotti comporterà la cancellazione e l'eventuale rimborso dell'ordine stesso",
                            cart.CurrentItems[0].SKU));
                    cart.RuleResults.Add(Result);
                }
            }
            catch
            {
            }
            return Result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (cart == null)
            {
                return Result;
            }

            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                string sku = cart.CurrentItems[0].SKU.Trim();
                var allowed = new List<string>();
                if (_IBPAllowedSubTypes.TryGetValue(sku, out allowed))
                {
                    string orderSubType = cart.OrderSubType;
                    if (string.IsNullOrEmpty(cart.OrderSubType))
                    {
                        var purchasingLimits =
                            PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);
                        if (null != purchasingLimits)
                        {
                            orderSubType = purchasingLimits.PurchaseSubType;
                        }
                    }
                    if (!allowed.Contains(orderSubType))
                    {
                        Result.Result = RulesResult.Failure;
                        var errorMessage =
                            HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),
                                                                "SKU" + sku + "Limitations") ??
                            string.Format("SKU {0} Limitations", sku);

                        Result.AddMessage(string.Format(errorMessage.ToString(), cart.CurrentItems[0].SKU));
                        cart.RuleResults.Add(Result);
                    }
                }

                if (cart.CurrentItems[0].SKU == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku)
                {
                    var catItems =
                        CatalogProvider.GetCatalogItems(
                            (from c in cart.CartItems select c.SKU.Trim()).ToList<string>(), Country);
                    if (catItems != null)
                    {
                        if (!catItems.Any(c => c.Value.ProductType == ProductType.Product))
                        {
                            Result.Result = RulesResult.Failure;
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "AddProductFirst")
                                               .ToString(), cart.CurrentItems[0].SKU));
                            cart.RuleResults.Add(Result);
                        }
                    }
                }
            }
            else if (reason == ShoppingCartRuleReason.CartItemsAdded)
            {
                var tempShoppingCartList = new ShoppingCartItemList();
                tempShoppingCartList.AddRange(from c in cart.CartItems
                                              select new ShoppingCartItem_V01(c.ID, c.SKU, c.Quantity, c.Updated));
                var item = tempShoppingCartList.Where(i => i.SKU == cart.CurrentItems[0].SKU);
                if (null != item && item.Count() > 0)
                {
                    var itemFirst = item.First();
                    itemFirst.Quantity += cart.CurrentItems[0].Quantity;
                }
                else
                {
                    tempShoppingCartList.Add(cart.CurrentItems[0]);
                }
            }
            else if (reason == ShoppingCartRuleReason.CartCalculated)
            {
                string orderSubType = cart.OrderSubType;
                if (string.IsNullOrEmpty(cart.OrderSubType))
                {
                    var purchasingLimits =
                        PurchasingLimitProvider.GetCurrentPurchasingLimits(cart.DistributorID);
                    if (null != purchasingLimits)
                    {
                        orderSubType = purchasingLimits.PurchaseSubType;
                    }
                }

                var myhlCart = cart as MyHLShoppingCart;
                if (myhlCart != null)
                {
                    var found =
                        (from a in _IBPAllowedSubTypes.Keys.ToList()
                         from s in myhlCart.ShoppingCartItems
                         where a == s.SKU
                         select a);
                    if (found.Count() > 0)
                    {
                        var skuToDelete = new List<string>();
                        foreach (string sku in found)
                        {
                            var allowed = new List<string>();
                            if (_IBPAllowedSubTypes.TryGetValue(sku, out allowed))
                            {
                                if (!allowed.Contains(orderSubType))
                                {
                                    skuToDelete.Add(sku);

                                }
                            }
                        }
                        if (skuToDelete.Count() > 0)
                        {
                            myhlCart.DeleteItemsFromCart(skuToDelete);
                            foreach (string sku in skuToDelete)
                            {
                                Result.Result = RulesResult.Failure;
                                Result.AddMessage(
                                    string.Format(
                                        HttpContext.GetGlobalResourceObject(
                                            string.Format("{0}_Rules", HLConfigManager.Platform),
                                            "SKU" + sku + "Limitations").ToString(), sku));
                                cart.RuleResults.Add(Result);
                            }
                        }
                    }
                }
            }

            return Result;
        }
    }
}