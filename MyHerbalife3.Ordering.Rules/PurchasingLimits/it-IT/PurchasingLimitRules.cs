using System;
using System.Collections.Generic;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.it_IT
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase, IShoppingCartRule, IPurchasingLimitsRule
    {
        private readonly List<string> ExemptSubTypes = new List<string>(new[] {"C"});
        private readonly List<string> ResaleSubTypes = new List<string>(new[] {"A1", "B1"});
        private readonly List<string> RetailSubTypes = new List<string>(new[] {"A2", "B2", "D", "D2", "E"});

        public override Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN)
        {
            var currentLimits = base.GetPurchasingLimits(distributorId, TIN);
            var theLimits = currentLimits[PurchasingLimitProvider.GetOrderMonth()];

            //conditional massage of subtype
            if (TIN.Equals("D") || TIN.Equals("ETO"))
            {
                TIN = "D2";
            }

            theLimits.PurchaseSubType = TIN;

            if (RetailSubTypes.Contains(theLimits.PurchaseSubType))
            {
                theLimits.PurchaseType = OrderPurchaseType.PersonalConsumption;
                theLimits.PurchaseLimitType = PurchaseLimitType.Volume;
            }
            else if (ResaleSubTypes.Contains(theLimits.PurchaseSubType))
            {
                theLimits.PurchaseType = OrderPurchaseType.Consignment;
                theLimits.PurchaseLimitType = PurchaseLimitType.Earnings;
            }
            else
            {
                if (!ExemptSubTypes.Contains(theLimits.PurchaseSubType))
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Unknown Distributor Subtype of \"{0}\" encountered in PurchasingLimitRules for it-IT, Distributor: {1}",
                            theLimits.PurchaseSubType, distributorId));
                }
            }

            return currentLimits;
        }

        /// <summary>Determine whether the current time is in a non-NTS period</summary>
        /// <returns></returns>
        protected override bool IsBlackoutPeriod()
        {
            //After 5:30 pm and before 9:00AM M - F and all weekend
            var now = DateUtils.GetCurrentLocalTime(Country);
            var cutoffStart = new DateTime(now.Year, now.Month, now.Day, 17, 29, 59);
            var cutoffEnd = new DateTime(now.Year, now.Month, now.Day, 8, 59, 59);
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }
            if (now > cutoffStart || now < cutoffEnd)
            {
                return true;
            }
            return false;
        }

        protected override ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart, ShoppingCartRuleReason reason,
                                                               ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                var myhlCart = cart as MyHLShoppingCart;

                if (null == myhlCart)
                {
                    LoggerHelper.Error(
                        string.Format("{0} myhlCart is null {1}", Locale, cart.DistributorID));
                    Result.Result = RulesResult.Failure;
                    return Result;
                }
                // to set purchasing limit type
                var theLimits = GetPurchasingLimits(cart.DistributorID, myhlCart.SelectedDSSubType ?? string.Empty);
                var currentLimits = theLimits[PurchasingLimitProvider.GetOrderMonth()];
                if (currentLimits.PurchaseLimitType == PurchaseLimitType.Earnings)
                {
                    var itemsToCalc = new List<ShoppingCartItem_V01>();
                    itemsToCalc.AddRange(myhlCart.CartItems);
                    if (myhlCart.CurrentItems != null && myhlCart.CurrentItems.Count > 0)
                        itemsToCalc.Add(myhlCart.CurrentItems[0]);
                    OrderTotals_V01 orderTotals = myhlCart.Calculate(itemsToCalc) as OrderTotals_V01;
                    if (orderTotals != null && orderTotals.DiscountPercentage != 0.0M)
                    {
                        myhlCart.SetDiscountForLimits(orderTotals.DiscountPercentage);
                        myhlCart.Totals = orderTotals;
                    }
                }
            }
            return base.PerformRules(cart, reason, Result);
        }
    }
}