using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.FOP;
using TaxIdentification = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    /// <summary>Interface for Purchasing Limit Rules</summary>
    public interface IPurchasingLimitsRule
    {
        Dictionary<int, PurchasingLimits_V01> GetPurchasingLimits(string distributorId, string TIN);
        bool PurchasingLimitsAreExceeded(string distributorId);
        bool DistributorIsExemptFromPurchasingLimits(string distributorId);
    }

    public interface IPurchasingPermissionRule
    {
        bool CanPurchase(string distributorID, string countryCode);
    }

    // FOP
    public interface IPurchaseRestrictionRule
    {
        bool PurchasingLimitsAreExceeded(int orderMonth, string distributorId);
        void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager);
        void SetOrderSubType(string orderSubType, string distributorId);
        List<ShoppingCartRuleResult> ProcessCart(ShoppingCartRuleReason reason , MyHLShoppingCart cart);
    }
}