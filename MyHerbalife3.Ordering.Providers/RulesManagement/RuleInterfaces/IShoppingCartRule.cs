using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
	/// <summary>Interface for Cart Processor Rules</summary>
	public interface IShoppingCartRule 
	{
		List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason);
	}
}
