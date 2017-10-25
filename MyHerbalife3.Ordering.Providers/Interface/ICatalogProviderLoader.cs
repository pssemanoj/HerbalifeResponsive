using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface ICatalogProviderLoader
    {
        Catalog_V01 GetCatalog(string isoCountryCode);

        CatalogItem_V01 GetCatalogItem(string sku, string isoCountryCode);

        CatalogItemList GetCatalogItems(List<string> skuList, string isoCountryCode);

        ProductInfoCatalog_V01 GetProductInfoCatalog(string locale);

        ProductAvailabilityType GetProductAvailability(SKU_V01 sku, string warehouse, DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown, string freightCode = null);
        GetSlowMovingSkuList LoadSlowMovingSkuInfo();

        bool IsPreordering(ShoppingCartItemList cart, string wareHousecode);

        bool IsPreordering(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode);

        void IsPreordering(MyHerbalife3.Ordering.Providers.MyHLShoppingCart cartItems, List<ShoppingCartItem_V01> currentitems,
                                         string wareHousecode, out bool preorderskus, out bool nonpreorderskus);

        bool IsPreordering(string sku, string wareHousecode);

        bool IsAllPreorderingProducts(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode);

        void GetProductAvailability(
            List<SKU_V01> productInfoCatalog,
            string locale,
            string distributorID,
            string warehouse,
            DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown);

        ProductInfoCatalog_V01 GetProductInfoCatalog(string locale, string warehouse);

        Dictionary<string, SKU_V01> GetAllSKU(string locale);

        Dictionary<string, SKU_V01> GetAllSKU(string locale, string warehouse);

        List<string> GetSKUsByList(string locale, string platformName, string searchTerm);

        List<string> GetSKUByListFromService(string locale, string platformName, string searchTerm);

        bool IsDistributorExempted(string DistributorId);

        Dictionary<string, int> GetWhCodeAndQuantity(string sku, string countryCode, string currentWarehouse, int quantity, List<string> warehousesToSplit);

        bool IsDisplayable(Category_V02 category, string locale);

        void CloseShoppingCart(int shoppingCartId, string distributorId, string orderNumber, DateTime orderDate);

    }
}
