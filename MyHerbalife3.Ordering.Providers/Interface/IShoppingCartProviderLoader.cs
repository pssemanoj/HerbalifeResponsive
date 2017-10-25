using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers.Interface
{
    public interface IShoppingCartProviderLoader
    {
        OrderTotals GetQuote(Order order, OrderTotals total, bool calcFreight);
        ShoppingCart GetShoppingCartFromV02(string memberId, string locale, int shoppingCartId);
        MyHLShoppingCart GetShoppingCart2(string distributorID, string locale);
        MyHLShoppingCart GetShoppingCart(string distributorID, string locale, int shoppingCartID);

        MyHLShoppingCart GetShoppingCartForCopy(string distributorID,
                                                int shoppingCartID,
                                                string locale,
                                                int newShoppingCartID,
                                                SKU_V01ItemCollection SkuitemList = null);

        MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool noupdateDeliveryInfoToDB = false);

        MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool suppressRules,
                                         bool noupdateDeliveryInfoToDB = false);

        void ResetInternetShoppingCartsCache(string distributorID, string locale);

        List<MyHLShoppingCart> GetInternetShoppingCarts(string distributorID,
                                                        string locale,
                                                        int index,
                                                        int maxLength,
                                                        bool indexChanged);

        List<MyHLShoppingCart> GetInternetShoppingCartsFromService(string distributorId,
                                                                   string locale,
                                                                   int index,
                                                                   int maxLength);

        List<MyHLShoppingCart> GetCarts(string distributorID,
                                        string locale,
                                        bool reLoad = false,
                                        bool updateSession = true);

        bool DeleteCartFromCache(string distributorID, string locale, int shoppingCartID);

        void DeleteItemsFromSession(string distributorID, string locale);

        Order AddLinkedSKU(Order _order, string locale, string countryCode, string warehouse);

        int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse);

        int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse, string freightCode,
                           ref bool isSplitted);
        
        int CheckForBackOrderableOverage(SKU_V01 sku,
                                         int quantity,
                                         string warehouse,
                                         DeliveryOptionType type);

        MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                    string distributorID,
                                                    string locale);

        string GetDSFraudResxKey(DRFraudStatusType fraudStatus);

        void CheckDSFraud(MyHLShoppingCart ShoppingCart);

        MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                    string distributorID,
                                                    string locale,
                                                    bool ignoreRules);

        MyHLShoppingCart GetBasicShoppingCartFromService(int shoppingCartId,
                                                         string distributorID,
                                                         string locale);

        MyHLShoppingCart GetShoppingCartForCopyFromService(int shoppingCartId,
                                          string distributorID,
                                          string locale,
                                          int newShoppingCartID,
                                          SKU_V01ItemCollection SkuitemList);

        MyHLShoppingCart GetShoppingCartFromService(string distributorID,
                                                    string locale,
                                                    string customerOrderNumber,
                                                    bool updateSession = true, bool noUpdateDeliveryInfoToDB = false);

        MyHLShoppingCart createShoppingCart(string distributorID, string locale);

        bool CartExists(string distributorID, string locale, string cartName);

        MyHLShoppingCart InsertShoppingCart(string distributorID,
                                            string locale,
                                            ServiceProvider.CatalogSvc.OrderCategoryType orderCategory,
                                            ShippingInfo shippingInfo,
                                            bool isDraft,
                                            string cartName);

        MyHLShoppingCart InsertShoppingCart(string distributorID,
                                            string locale,
                                            ServiceProvider.CatalogSvc.OrderCategoryType orderCategory,
                                            Shipping.ShippingInfo shippingInfo,
                                            CustomerOrderDetail customerOrderDetail,
                                            bool isDraft,
                                            string draftName);

        bool InsertShoppingCartItem(string distributorID, ShoppingCartItem_V01 item, int shoppingCartID);

        bool InsertShoppingCartItemList(string distributorID, List<ShoppingCartItem_V01> items, int shoppingCartId);

        MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart);

        MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart,
                                            string orderXML,
                                            string orderNumber,
                                            DateTime orderDate);

        string getTodayMagazineDeletedSessionKey(string distributorID, string localc, int shoppingCartID);

        bool TodayMagazineDeleted(string distributorID, string localc, int shoppingCartID);

        bool DeleteOldShoppingCartForCustomerOrder(string distributorID, string customerOrderID);

        bool DeleteShoppingCart(MyHLShoppingCart shoppingCart, List<string> skus);

        List<DualMonthPair> GetDualOrderMonthDates(DateTime fromDate,
                                                   DateTime toDate,
                                                   string isoCountryCode);

        DateTime GetEOMDate(DateTime date, string isoCountryCode);

        List<DualMonthPair> GetDualOrderMonth(string orderMonthValue, string countryCode);

        List<DualMonthPair> GetDualOrderMonthDatesFromService(DateTime fromDate,
                                                              DateTime toDate,
                                                              string isoCountryCode);

        List<ServiceProvider.CatalogSvc.ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                 ShoppingCartItem_V01 item,
                                                 ShoppingCartRuleReason reason);

        List<ServiceProvider.CatalogSvc.ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                 List<ShoppingCartItem_V01> items,
                                                 ShoppingCartRuleReason reason);

        MyHLShoppingCart GetNewUnsavedShoppingCart(string distributorID, string locale);

        string ValidateCartItem(string sku,
                                int quantity,
                                out int newQuantity,
                                string locale,
                                string warehouseCode);

        MyHLShoppingCart GetShoppingCartFromInvoice(string distributorID, string locale, long invoiceID);

        MyHLShoppingCart GetShoppingCartFromMemberInvoice(string distributorID, string locale, int invoiceID);

        EligibleDistributorInfo_V01 GetEligibleForPromo(string distributorId, string locale);

        EligibleDistributorInfo_V01 SaveEligibleForPromo(EligibleDistributorInfo_V01 promoInfo, int shoppingCartId,
                                                         string orderNumber,
                                                         ref int updShoppingCartId, ref string updOrderNumber);

        EligibleDistributorInfo_V01 SaveEligibleForPromoToFromService(EligibleDistributorInfo_V01 promoInfo,
                                                                      int shoppingCartId, string orderNumber,
                                                                      ref int updShoppingCartId,
                                                                      ref string updOrderNumber);

        bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart);

        bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart, bool isHidden);

        void CheckSplitOrder(MyHLShoppingCart shoppingCart);
        List<ShoppingCartItem_V01> GetDiscontinuededSku(MyHLShoppingCart originalUnfilteredCopyCart);

        List<ShoppingCartItem_V01> GetDiscontinuededSku(int shoppingCartId, string distributorId,
            string locale, int newShoppingCartId, SKU_V01ItemCollection skuitemList = null);
    }
}
