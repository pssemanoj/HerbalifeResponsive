// -----------------------------------------------------------------------
// <copyright file="HFFRule.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    /// <summary>
    /// Inteface for the HFF rules.
    /// </summary>
    public interface IHFFRule
    {
        bool CanDonate(ShoppingCart_V02 cart);
    }
}
