using MyHerbalife3.Shared.ViewModel.CatalogSvc;
namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IFTMRule
    {
        int GetFTMMaxQuantity(ShoppingCart_V02 cart);
    }
}
