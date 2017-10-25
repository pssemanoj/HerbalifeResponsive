using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IEventTicketProviderLoader
    {
        Dictionary<int, SKU_V01>  ValidateEventList(Dictionary<int, SKU_V01> eventProductList,
                                                               Dictionary<int, SKU_V01> allreadypurchasedEventTicktList,
                                                               int limit, ref ShoppingCartRuleResult resul);

        Dictionary<int, SKU_V01> LoadSkuPurchasedCount(Dictionary<int, SKU_V01> eventProductList, string distributorId, string countrycode);
    }
}
