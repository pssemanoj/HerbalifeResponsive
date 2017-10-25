using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
            Result.Result = RulesResult.Success;

            if (null == cart)
            {
                LoggerHelper.Error(
                    string.Format("{0} cart is null {1}, MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules.", Locale, cart.DistributorID));
                Result.Result = RulesResult.Failure;
                return Result;
            }
            if (Settings.GetRequiredAppSetting<bool>("EnableJPSKURestriction", false))
            {
                string orderMonth = SKULimitationProvider.GetOrderMonthString();
                if (reason == ShoppingCartRuleReason.CartItemsBeingAdded && cart.ItemsBeingAdded != null && cart.ItemsBeingAdded.Count > 0)
                {
                    var allLimits = SKULimitationProvider.SKUPurchaseRestrictionInfo(cart.DistributorID, orderMonth);
                    if (allLimits != null)
                    {
                        Result = HandleCartItemsBeingAdded(cart, Result, allLimits);
                        if (Result.Result == RulesResult.Failure)
                            return Result;
                    }
                }
                else if (reason == ShoppingCartRuleReason.CartBeingPaid)
                {
                    var allLimits = SKULimitationProvider.SKUPurchaseRestrictionInfo(cart.DistributorID, orderMonth);
                    if (allLimits != null)
                    {
                        Result = HandleCartBeingPaid(cart, Result, allLimits);
                        if (Result.Result == RulesResult.Failure)
                            return Result;
                    }
                }
                else if (reason == ShoppingCartRuleReason.CartClosed) // order placed
                {
                    var allLimits = SKULimitationProvider.SKUPurchaseRestrictionInfo(cart.DistributorID, cart.OrderMonth.ToString());
                    if (allLimits != null)
                    {
                        Result = HandleCartClosed(cart, Result, allLimits);
                        SKULimitationProvider.SetSKUPurchaseRestrictionInfo(cart.DistributorID, cart.OrderMonth.ToString(), allLimits);
                    }
                }
            }
            base.PerformRules(cart, reason, Result);
            return Result;
        }
        public string groupFormulaOne = "FORMULA1";
        private string defaultError1()
        {
            var error1 = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "ExceededSKULimitPerMonth") ??
               "SKU {0} exceeded the number of quantity {1} per volume month. Remaining quantity limit is {2}.";
            return error1 as string;
        }
        private string defaultError2()
        {
            var error2 = HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform), "CombinedFormulaOneQuantityExceed") ??
            "The combined quantities of Formula-1 {0} exceeds purchase limit {1}.  You have remaining of quantity {2} for current volume month.";
            return error2 as string;
        }
        public ShoppingCartRuleResult HandleCartBeingPaid(MyHLShoppingCart cart, ShoppingCartRuleResult Result, List<PurchaseRestrictionInfo> allLimits)
        {
            var error1 = defaultError1();
            var error2 = defaultError2();

            List<string> errorList = new List<string>();
            List<string> itemsToRemove = new List<string>();
            // non formula one
            var nonFormulaOneLimits = allLimits.Where(f => !f.Group.Equals(groupFormulaOne));
            if (nonFormulaOneLimits != null && nonFormulaOneLimits.Count() > 0)
            {
                var itemsCannotAdd = (from a in nonFormulaOneLimits.First().SKUInfoList
                                      from b in cart.CartItems
                                      where a.SKU == b.SKU && b.Quantity > a.QuantityAllow
                                      select new { SKU = b, Qty = a.QuantityAllow }
                                     );
                // set error message
                errorList.AddRange(from c in itemsCannotAdd
                                   select string.Format(error1.ToString(), c.SKU.SKU, 100, c.Qty));
            }

            // formula one
            var formulaOneLimits = allLimits.Where(l => l.Group.Equals(groupFormulaOne));
            if (formulaOneLimits != null && formulaOneLimits.Count() > 0 )
            {
                var formulaOneSKUs = formulaOneLimits.First().SKUInfoList;

                var formulaOneToAdd = (from a in formulaOneSKUs
                                       from b in cart.CartItems
                                       where a.SKU == b.SKU
                                       select new { SKU = b, Qty = a.QuantityAllow }
                                     );

                // exceed formula-1 group limit 
                if (formulaOneToAdd.Sum(f => f.SKU.Quantity) > formulaOneSKUs.First().QuantityAllow)
                {
                    errorList.Add(
                        string.Format(
                        error2.ToString(),
                        string.Join("と", formulaOneToAdd.Select(x => x.SKU.SKU)),
                        200,
                        formulaOneSKUs.First().QuantityAllow)
                    );
                    itemsToRemove.AddRange(from c in formulaOneToAdd
                                           select c.SKU.SKU);
                }
            }
            errorList = errorList.Distinct().ToList();
            if (errorList.Count > 0)
            {
                Result.Result = RulesResult.Failure;
                Result.RuleName = "PurchaseRestriction Rules";
                Array.ForEach(errorList.ToArray(), a => Result.AddMessage(a));
                cart.RuleResults.Add(Result);
                cart.ItemsBeingAdded.RemoveAll(s => itemsToRemove.Select(a => a).Contains(s.SKU));
            }
            return Result;
        }
        public ShoppingCartRuleResult HandleCartItemsBeingAdded(MyHLShoppingCart cart, ShoppingCartRuleResult Result, List<PurchaseRestrictionInfo> allLimits)
        {
            var error1 = defaultError1();
            var error2 = defaultError2();

            Func<ShoppingCartItemList, ShoppingCartItem_V01, int> sumQuantity =
                   delegate(ShoppingCartItemList itemsInCart, ShoppingCartItem_V01 itemToAdd)
                   {
                       return itemsInCart.Find(i => i.SKU.Equals(itemToAdd.SKU)) == null ? itemToAdd.Quantity : itemsInCart.Find(i => i.SKU.Equals(itemToAdd.SKU)).Quantity + itemToAdd.Quantity;
                   };

            // sum quantity in cart and quantity to add for every sku being added
            ShoppingCartItemList newItemsToAdd = new ShoppingCartItemList();
            Array.ForEach(cart.ItemsBeingAdded.ToArray(), a => newItemsToAdd.Add(new ShoppingCartItem_V01 { SKU = a.SKU, Quantity = sumQuantity(cart.CartItems, a) }));

            List<string> errorList = new List<string>();
            List<string> itemsToRemove = new List<string>();
            // non formula one
            var nonFormulaOneLimits = allLimits.Where(f => !f.Group.Equals(groupFormulaOne));
            if (nonFormulaOneLimits != null && nonFormulaOneLimits.Count() > 0)
            {
                var itemsCannotAdd = (from a in nonFormulaOneLimits.First().SKUInfoList
                                      from b in newItemsToAdd
                                      where a.SKU == b.SKU  && b.Quantity > a.QuantityAllow
                                      select new { SKU = b, Qty = a.QuantityAllow }
                                     );
                // set error message
                errorList.AddRange(from c in itemsCannotAdd 
                                   select string.Format(error1.ToString(),c.SKU.SKU,100,c.Qty) );
                // remove items can't be added
                itemsToRemove.AddRange(from c in itemsCannotAdd 
                                        select c.SKU.SKU);
                newItemsToAdd.RemoveAll(s => itemsCannotAdd.Select(a => a.SKU.SKU).Contains(s.SKU));

            }

            if (newItemsToAdd.Count() > 0)
            {
                // formula one
                var formulaOneLimits = allLimits.Where(l => l.Group.Equals(groupFormulaOne));
                if (formulaOneLimits != null && formulaOneLimits.Count() > 0 )
                {
                    var formulaOneSKUs = formulaOneLimits.First().SKUInfoList;

                    var formulaOneToAdd = (from a in formulaOneSKUs
                                           from b in newItemsToAdd
                                           where a.SKU == b.SKU
                                           select new { SKU = b, Qty = a.QuantityAllow }
                                         );
                    var qtyFormulaOneInCartNotBeingAdded =
                           (from a in cart.CartItems
                            from b in formulaOneSKUs
                            where a.SKU == b.SKU
                            select a).Except
                                (from c in formulaOneToAdd
                                 from d in cart.CartItems
                                 where c.SKU.SKU == d.SKU
                                 select d).ToList().Sum(x=>x.Quantity);

                    int formulaOneSKUsAllowQty = formulaOneSKUs.Count() > 0 ? formulaOneSKUs.First().QuantityAllow : 0;
                    // exceed formula-1 group limit 
                    if (formulaOneToAdd.Sum(f => f.SKU.Quantity) + qtyFormulaOneInCartNotBeingAdded > formulaOneSKUs.First().QuantityAllow)
                    {
                        errorList.Add(
                            string.Format(
                            error2.ToString(),
                            string.Join("と", formulaOneToAdd.Select(x => x.SKU.SKU)), 
                            200,
                            formulaOneSKUs.First().QuantityAllow)
                        );
                        itemsToRemove.AddRange(from c in formulaOneToAdd
                                               select c.SKU.SKU);
                    }
                }
            }
            errorList = errorList.Distinct().ToList();
            if (errorList.Count > 0)
            {
                Result.Result = RulesResult.Failure;
                Result.RuleName = "PurchaseRestriction Rules";
                Array.ForEach(errorList.ToArray(), a =>Result.AddMessage(a));
                cart.RuleResults.Add(Result);
                cart.ItemsBeingAdded.RemoveAll(s => itemsToRemove.Select(a => a).Contains(s.SKU));
            }
            return Result;
        }

        public ShoppingCartRuleResult HandleCartClosed(MyHLShoppingCart cart, ShoppingCartRuleResult Result, List<PurchaseRestrictionInfo> allLimits)
        {
            // non formula one
            var nonFormulaOneLimits = allLimits.Where(f => !f.Group.Equals(groupFormulaOne));
            if (nonFormulaOneLimits != null && nonFormulaOneLimits.Count() > 0)
            {
                var nonFormulaOne = nonFormulaOneLimits.First();
                if (nonFormulaOne.SKUInfoList != null && nonFormulaOne.SKUInfoList.Count() > 0)
                {
                    Func<PurchaseRestrictionSKUInfo, ShoppingCartItemList, int> deductQuantity = delegate(PurchaseRestrictionSKUInfo limitInfo, ShoppingCartItemList itemsInCart)
                    {
                        var item = itemsInCart.Find(q => q.SKU.Equals(limitInfo.SKU));
                        if (item != null)
                            limitInfo.QuantityAllow -= item.Quantity;
                        return limitInfo.QuantityAllow;
                    };
                    Array.ForEach(nonFormulaOne.SKUInfoList.ToArray(), b => b.QuantityAllow = deductQuantity(b, cart.CartItems));
                }
            }
            // formula one
            var formulaOneLimits = allLimits.Where(l => l.Group.Equals(groupFormulaOne));
            if (formulaOneLimits != null && formulaOneLimits.Count() > 0 && formulaOneLimits.First().SKUInfoList != null)
            {
                var formulaOneSKUs = formulaOneLimits.First().SKUInfoList;

                var qtyFormulaOneInCartAdded =
                      (from a in cart.CartItems
                       from b in formulaOneSKUs
                       where a.SKU == b.SKU
                       select a).ToList().Sum(x => x.Quantity);
                if (qtyFormulaOneInCartAdded > 0)
                {
                    foreach (var fone in formulaOneSKUs)
                    {
                        fone.QuantityAllow -= qtyFormulaOneInCartAdded;
                    }
                }
            }
            return Result;
        }
    }
}
