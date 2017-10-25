using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Rules.PurchaseRestriction;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.GB
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase
    {
        public override void SetLimits(int orderMonth, IPurchaseRestrictionManager manager, PurchasingLimits_V01 limits)
        {
            if (manager.PurchasingLimits == null)
                manager.PurchasingLimits = new Dictionary<int, PurchasingLimits_V01>();

            var FOPlimits = GetFOPLimits(manager);
            if (FOPlimits != null && FOPlimits.maxVolumeLimit != -1 && !FOPlimits.Completed)
            {
                manager.PurchasingLimits[orderMonth] = FOPlimits;
                manager.PurchasingLimits[orderMonth].PurchaseLimitType = PurchaseLimitType.Volume;
            }
            else
            {
                manager.PurchasingLimits[orderMonth] = limits;
                manager.PurchasingLimits[orderMonth].PurchaseLimitType = PurchaseLimitType.DiscountedRetail;
            }
        }

        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits, orderMonth, manager);
            if (limits == null)
                return;
            SetLimits(orderMonth, manager, limits);
        }

        private string getBeginDate(string distributorId, string countryCode)
        {
            var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorId, countryCode);
            var endDate = distributorOrderingProfile.ApplicationDate;
            return endDate.ToShortDateString();
        }
        private string getEndDate(string distributorId, string countryCode)
        {
            var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(distributorId, countryCode);
            var endDate = distributorOrderingProfile.ApplicationDate.AddDays(7);
            return endDate.ToShortDateString();
        }

        private ShoppingCartRuleResult reportError(MyHLShoppingCart cart, ShoppingCartRuleResult Result, decimal amountToRemove, decimal maxLimits)
        {
            Result.Result = RulesResult.Failure;
            /// This order will put the Member over 7 Day Cooling Off Purchase Limit.  It is recommended to remove &REMOVE_GBP or more from the order to continue or cancel the order. The Cooling Off Period will end on: <DATE>
            Result.AddMessage(
                string.Format(
                    HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_Rules", HLConfigManager.Platform),
                        "LimitsThisOrderExceededWithinDays").ToString(), amountToRemove, getEndDate(cart.DistributorID, cart.CountryCode)));
            cart.RuleResults.Add(Result);
            return Result;
        }

        protected override ShoppingCartRuleResult PerformRules(MyHLShoppingCart cart,
                                                              ShoppingCartRuleReason reason,
                                                              ShoppingCartRuleResult Result)
        {
             Result.Result = RulesResult.Success;

            if (cart == null)
                return Result;

            Result = base.PerformRules(cart, reason, Result);
            if (Result.Result == RulesResult.Failure)
            {
                return Result;
            }

            var manager = GetPurchaseRestrictionManager(cart.DistributorID);
            if (manager == null)
                return Result;

            var currentLimits = manager.ApplicableLimits == null ? null : manager.ApplicableLimits.Where(x => (x as PurchasingLimits_V01).LimitsRestrictionType == LimitsRestrictionType.PurchasingLimits && (x as PurchasingLimits_V01).maxVolumeLimit > -1);
            if (currentLimits == null || currentLimits.Count() == 0 )
                return Result;
            var limits = currentLimits.First() as PurchasingLimits_V01;

            if (limits != null && limits.maxVolumeLimit > -1 )
            {
                if (!cart.OnCheckout )
                {
                    if (limits.RemainingVolume > decimal.Zero)
                    {
                        Result.Result = RulesResult.Failure;
                        ///New Members are subject to a 7 day cooling off period.  During this time Members are limited to £163.92.  Your cooling off period began on <date1> and ends on <date2>
                        Result.AddMessage(
                        string.Format(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform),
                                "LimitsInfoWithinDays").ToString(), limits.maxVolumeLimit, getBeginDate(cart.DistributorID, cart.CountryCode), getEndDate(cart.DistributorID, cart.CountryCode)));

                    }
                    else
                    {
                        Result.Result = RulesResult.Failure;
                        /// Member is subject to 7 Day Cooling Off Purchase Limit of  £163.92 and has already reached the limit. Member can only purchase items after the Cooling Off Period has been completed. The Cooling Off Period will end on: <DATE>
                        Result.AddMessage(
                       string.Format(
                           HttpContext.GetGlobalResourceObject(
                               string.Format("{0}_Rules", HLConfigManager.Platform),
                               "LimitsExceededWithinDays").ToString(), limits.maxVolumeLimit, getEndDate(cart.DistributorID, cart.CountryCode)));
                        return Result;
                    }
                }
                var calcTheseItems = new List<ShoppingCartItem_V01>();
                calcTheseItems.AddRange(from i in cart.CartItems
                                        where !APFDueProvider.IsAPFSku(i.SKU)
                                        select
                                            new ShoppingCartItem_V01(i.ID, i.SKU, i.Quantity, i.Updated,
                                                                     i.MinQuantity));

                foreach (var item in cart.ItemsBeingAdded)
                {
                    var existingItem =
                        calcTheseItems.Find(ci => ci.SKU == item.SKU);
                    if (null != existingItem)
                    {
                        existingItem.Quantity += item.Quantity;
                    }
                    else
                    {
                        calcTheseItems.Add(new ShoppingCartItem_V01(0, item.SKU, item.Quantity, DateTime.Now));
                    }
               
                }
                var totals = cart.Calculate(calcTheseItems, false) as OrderTotals_V01;
                if (null == totals)
                {
                    var message =
                        string.Format(
                            "Purchasing Limits DiscountedRetail calculation failed due to Order Totals returning a null for Distributor {0}",
                            cart.DistributorID);
                    LoggerHelper.Error(message);
                    throw new ApplicationException(message);
                }
                decimal discountedRetailInCart = totals.ItemTotalsList.Sum(x => (x as ItemTotal_V01).DiscountedPrice);
                if (limits.RemainingVolume - discountedRetailInCart < 0)
                {
                    Result = reportError(cart, Result, discountedRetailInCart - limits.RemainingVolume, limits.maxVolumeLimit);
                    cart.ItemsBeingAdded.Clear();
                }
            }

            return Result;

        }
    }
}
