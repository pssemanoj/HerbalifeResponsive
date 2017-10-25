using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IShoppingCartIntegrityRule
    {
        bool DuplicateSKU(ShoppingCart_V01 shoppingCart);
        bool InvalidQuantity(ShoppingCart_V01 shoppingCart);
    }
}
