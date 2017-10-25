using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;

namespace MyHerbalife3.Ordering.Providers.RulesManagement
{
    public class RuleSet
    {
        public List<IShoppingCartRule> IShoppingCartRuleModules = new List<IShoppingCartRule>();
        public List<IShoppingCartManagementRule> IShoppingCartManagementRuleModules = new List<IShoppingCartManagementRule>();
        public List<IPurchasingLimitsRule> IPurchasingLimitsRuleModules = new List<IPurchasingLimitsRule>();
        public List<IPurchaseRestrictionRule> IPurchaseRestrictionRuleModules = new List<IPurchaseRestrictionRule>();
        public List<IInventoryRule> IInventoryRuleModules = new List<IInventoryRule>();
        public List<ITaxationRule> ITaxationRuleModules = new List<ITaxationRule>();
        public List<IShoppingCartIntegrityRule> IShoppingCartIntegrityRuleModules = new List<IShoppingCartIntegrityRule>();
        public List<IPurchasingPermissionRule> IPurchasingPermissionRuleModules = new List<IPurchasingPermissionRule>();
        public List<IDiscountRule> IDiscountRuleModules = new List<IDiscountRule>();
        public List<IHFFRule> IHFFRuleModules = new List<IHFFRule>();
        public List<IValidateDistributorRule> IValidateDistributorRuleModules = new List<IValidateDistributorRule>();
        public List<ISavedCartManagementRule> ISavedCartManagementRuleModules = new List<ISavedCartManagementRule>();
        public List<IOrderManagementRule> IOrderManagementRuleModule = new List<IOrderManagementRule>();
        public List<IPromoRule> IPromoRuleModule = new List<IPromoRule>(); 
    }
}
