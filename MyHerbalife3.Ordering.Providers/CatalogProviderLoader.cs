using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

//This class is serve as a wrapper class for the existing CatalogProvider class.
//The main objective of this class is to provide a mockable interface for the unit testing purpose.

namespace MyHerbalife3.Ordering.Providers
{
    public class CatalogProviderLoader : ICatalogProviderLoader
    {
        public void CloseShoppingCart(int shoppingCartId, string distributorId, string orderNumber, DateTime orderDate)
        {
            Providers.CatalogProvider.CloseShoppingCart(shoppingCartId, distributorId, orderNumber, orderDate);
        }

        public Dictionary<string, SKU_V01> GetAllSKU(string locale)
        {
            return Providers.CatalogProvider.GetAllSKU(locale);
        }

        public Dictionary<string, SKU_V01> GetAllSKU(string locale, string warehouse)
        {
            return Providers.CatalogProvider.GetAllSKU(locale, warehouse);
        }

        public Catalog_V01 GetCatalog(string isoCountryCode)
        {
            return Providers.CatalogProvider.GetCatalog(isoCountryCode);
        }

        public CatalogItem_V01 GetCatalogItem(string sku, string isoCountryCode)
        {
            return Providers.CatalogProvider.GetCatalogItem(sku, isoCountryCode);
        }

        public CatalogItemList GetCatalogItems(List<string> skuList, string isoCountryCode)
        {
            return Providers.CatalogProvider.GetCatalogItems(skuList, isoCountryCode);
        }

        public ProductAvailabilityType GetProductAvailability(SKU_V01 sku, string warehouse, DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown, string freightCode = null)
        {
            return Providers.CatalogProvider.GetProductAvailability(sku, warehouse, deliveryOption, freightCode);
        }

        public void GetProductAvailability(List<SKU_V01> productInfoCatalog, string locale, string distributorID, string warehouse, DeliveryOptionType deliveryOption = DeliveryOptionType.Unknown)
        {
            Providers.CatalogProvider.GetProductAvailability(productInfoCatalog, locale, distributorID, warehouse, deliveryOption);
        }

        public ProductInfoCatalog_V01 GetProductInfoCatalog(string locale)
        {
            return Providers.CatalogProvider.GetProductInfoCatalog(locale);
        }

        public ProductInfoCatalog_V01 GetProductInfoCatalog(string locale, string warehouse)
        {
            return Providers.CatalogProvider.GetProductInfoCatalog(locale, warehouse);
        }

        public List<string> GetSKUByListFromService(string locale, string platformName, string searchTerm)
        {
            return Providers.CatalogProvider.GetSKUByListFromService(locale, platformName, searchTerm);
        }

        public List<string> GetSKUsByList(string locale, string platformName, string searchTerm)
        {
            return Providers.CatalogProvider.GetSKUsByList(locale, platformName, searchTerm);
        }

        public Dictionary<string, int> GetWhCodeAndQuantity(string sku, string countryCode, string currentWarehouse, int quantity, List<string> warehousesToSplit)
        {
            return Providers.CatalogProvider.GetWhCodeAndQuantity(sku, countryCode, currentWarehouse, quantity, warehousesToSplit);
        }

        public bool IsAllPreorderingProducts(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode)
        {
            return Providers.CatalogProvider.IsAllPreorderingProducts(shoppingCart, wareHousecode);
        }

        public bool IsDisplayable(Category_V02 category, string locale)
        {
            return Providers.CatalogProvider.IsDisplayable(category, locale);
        }

        public bool IsDistributorExempted(string DistributorId)
        {
            return Providers.CatalogProvider.IsDistributorExempted(DistributorId);
        }

        public bool IsPreordering(string sku, string wareHousecode)
        {
            return Providers.CatalogProvider.IsPreordering(sku, wareHousecode);
        }

        public bool IsPreordering(List<ShoppingCartItem_V01> shoppingCart, string wareHousecode)
        {
            return Providers.CatalogProvider.IsPreordering(shoppingCart, wareHousecode);
        }

        public bool IsPreordering(ShoppingCartItemList cart, string wareHousecode)
        {
            return Providers.CatalogProvider.IsPreordering(cart, wareHousecode);
        }
        
        public void IsPreordering(Ordering.Providers.MyHLShoppingCart cartItems, List<ShoppingCartItem_V01> currentitems, string wareHousecode, out bool preorderskus, out bool nonpreorderskus)
        {
            Providers.CatalogProvider.IsPreordering(cartItems, currentitems, wareHousecode, out preorderskus, out nonpreorderskus);
        }

        public GetSlowMovingSkuList LoadSlowMovingSkuInfo()
        {
          return  China.CatalogProvider.LoadSlowMovingSkuInfo();
        }
    }
}
