using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class ShoppingCartProviderLoader : IShoppingCartProviderLoader
    {
        public ShoppingCartProviderLoader(ICatalogProviderLoader proxy, ICatalogInterface invalidCart)
        {
            ShoppingCartProvider.CatalogProviderLoader = proxy;
            ShoppingCartProvider.CatalogProxyInterface = invalidCart;
        }
        public ShoppingCartProviderLoader(ICatalogProviderLoader proxy)
        {
            ShoppingCartProvider.CatalogProviderLoader = proxy;
        }

        public ShoppingCartProviderLoader() { }

        public OrderTotals GetQuote(Order order, OrderTotals total, bool calcFreight)
        {
            return Providers.ShoppingCartProvider.GetQuote(order, total, calcFreight);
        }
        public ShoppingCart GetShoppingCartFromV02(string memberId, string locale, int shoppingCartId)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromV02(memberId, locale, shoppingCartId);
        }
        public MyHLShoppingCart GetShoppingCart2(string distributorID, string locale)
        {
            return Providers.ShoppingCartProvider.GetShoppingCart2(distributorID, locale);
        }
        public MyHLShoppingCart GetShoppingCart(string distributorID, string locale, int shoppingCartID)
        {
            return Providers.ShoppingCartProvider.GetShoppingCart(distributorID, locale, shoppingCartID);
        }

        public MyHLShoppingCart GetShoppingCartForCopy(string distributorID,
                                                 int shoppingCartID,
                                                 string locale,
                                                 int newShoppingCartID,
                                                 SKU_V01ItemCollection SkuitemList = null)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartForCopy(distributorID,
                                          shoppingCartID,
                                          locale,
                                          newShoppingCartID,
                                          SkuitemList);

        }

        public MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool noupdateDeliveryInfoToDB = false)
        {
            return Providers.ShoppingCartProvider.GetShoppingCart(distributorID, locale, noupdateDeliveryInfoToDB);
        }

        public MyHLShoppingCart GetShoppingCart(string distributorID, string locale, bool suppressRules,
                                          bool noupdateDeliveryInfoToDB = false)
        {
            return Providers.ShoppingCartProvider.GetShoppingCart(distributorID, locale, suppressRules, noupdateDeliveryInfoToDB);
        }

        public void ResetInternetShoppingCartsCache(string distributorID, string locale)
        {
            Providers.ShoppingCartProvider.ResetInternetShoppingCartsCache(distributorID, locale);
        }

        public List<MyHLShoppingCart> GetInternetShoppingCarts(string distributorID,
                                                         string locale,
                                                         int index,
                                                         int maxLength,
                                                         bool indexChanged)
        {
            return Providers.ShoppingCartProvider.GetInternetShoppingCarts(distributorID,
                                            locale,
                                            index,
                                            maxLength,
                                            indexChanged);
        }

        public List<MyHLShoppingCart> GetInternetShoppingCartsFromService(string distributorId,
                                                                    string locale,
                                                                    int index,
                                                                    int maxLength)
        {
            return Providers.ShoppingCartProvider.GetInternetShoppingCartsFromService(distributorId,
                                                       locale,
                                                       index,
                                                       maxLength);
        }

        public List<MyHLShoppingCart> GetCarts(string distributorID,
                                         string locale,
                                         bool reLoad = false,
                                         bool updateSession = true)
        {
            return Providers.ShoppingCartProvider.GetCarts(distributorID, locale, reLoad, updateSession);
        }

        public bool DeleteCartFromCache(string distributorID, string locale, int shoppingCartID)
        {
            return Providers.ShoppingCartProvider.DeleteCartFromCache(distributorID, locale, shoppingCartID);
        }

        public void DeleteItemsFromSession(string distributorID, string locale)
        {
            Providers.ShoppingCartProvider.DeleteItemsFromSession(distributorID, locale);
        }

        public Order AddLinkedSKU(Order _order, string locale, string countryCode, string warehouse)
        {
            return Providers.ShoppingCartProvider.AddLinkedSKU(_order, locale, countryCode, warehouse);
        }

        public int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse)
        {
            return Providers.ShoppingCartProvider.CheckInventory(cItem, quantity, warehouse);
        }

        public int CheckInventory(CatalogItem_V01 cItem, int quantity, string warehouse, string freightCode,
                            ref bool isSplitted)
        {
            return Providers.ShoppingCartProvider.CheckInventory(cItem, quantity, warehouse, freightCode, ref isSplitted);
        }

        public int CheckForBackOrderableOverage(SKU_V01 sku,
                                  int quantity,
                                  string warehouse,
                                  DeliveryOptionType type)
        {
            return Providers.ShoppingCartProvider.CheckForBackOrderableOverage(sku, quantity, warehouse, type);
        }

        public MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                     string distributorID,
                                                     string locale)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromService(shoppingCartId, distributorID, locale);
        }

        public string GetDSFraudResxKey(DRFraudStatusType fraudStatus)
        {
            return Providers.ShoppingCartProvider.GetDSFraudResxKey(fraudStatus);
        }

        public void CheckDSFraud(MyHLShoppingCart ShoppingCart)
        {
            Providers.ShoppingCartProvider.CheckDSFraud(ShoppingCart);
        }

        public MyHLShoppingCart GetShoppingCartFromService(int shoppingCartId,
                                                     string distributorID,
                                                     string locale,
                                                     bool ignoreRules)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromService(shoppingCartId, distributorID, locale, ignoreRules);
        }

        public MyHLShoppingCart GetBasicShoppingCartFromService(int shoppingCartId,
                                                          string distributorID,
                                                          string locale)
        {
            return Providers.ShoppingCartProvider.GetBasicShoppingCartFromService(shoppingCartId, distributorID, locale);
        }

        public MyHLShoppingCart GetShoppingCartForCopyFromService(int shoppingCartId,
                                                            string distributorID,
                                                            string locale,
                                                            int newShoppingCartID,
                                                            SKU_V01ItemCollection SkuitemList)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartForCopyFromService(shoppingCartId, distributorID, locale, newShoppingCartID,
                                                     SkuitemList);
        }

        public MyHLShoppingCart GetShoppingCartFromService(string distributorID,
                                                     string locale,
                                                     string customerOrderNumber,
                                                     bool updateSession = true, bool noUpdateDeliveryInfoToDB = false)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromService(distributorID, locale, customerOrderNumber, updateSession,
                                              noUpdateDeliveryInfoToDB);
        }

        public MyHLShoppingCart createShoppingCart(string distributorID, string locale)
        {
            return Providers.ShoppingCartProvider.createShoppingCart(distributorID, locale);
        }
        public bool CartExists(string distributorID, string locale, string cartName)
        {
            return Providers.ShoppingCartProvider.CartExists(distributorID, locale, cartName);
        }

        public MyHLShoppingCart InsertShoppingCart(string distributorID,
                                             string locale,
                                             ServiceProvider.CatalogSvc.OrderCategoryType orderCategory,
                                             ShippingInfo shippingInfo,
                                             bool isDraft,
                                             string cartName)
        {
            return Providers.ShoppingCartProvider.InsertShoppingCart(distributorID, locale, orderCategory, shippingInfo, isDraft, cartName);
        }

        public MyHLShoppingCart InsertShoppingCart(string distributorID,
                                             string locale,
                                             ServiceProvider.CatalogSvc.OrderCategoryType orderCategory,
                                             Shipping.ShippingInfo shippingInfo,
                                             CustomerOrderDetail customerOrderDetail,
                                             bool isDraft,
                                             string draftName)
        {
            return Providers.ShoppingCartProvider.InsertShoppingCart(distributorID, locale, orderCategory, shippingInfo, customerOrderDetail, isDraft,
                                      draftName);
        }

        public bool InsertShoppingCartItem(string distributorID, ShoppingCartItem_V01 item, int shoppingCartID)
        {
            return Providers.ShoppingCartProvider.InsertShoppingCartItem(distributorID, item, shoppingCartID);

        }

        public bool InsertShoppingCartItemList(string distributorID, List<ShoppingCartItem_V01> items, int shoppingCartId)
        {
            return Providers.ShoppingCartProvider.InsertShoppingCartItemList(distributorID, items, shoppingCartId);
        }

        public MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart)
        {
            return Providers.ShoppingCartProvider.UpdateShoppingCart(shoppingCart);
        }

        public MyHLShoppingCart UpdateShoppingCart(MyHLShoppingCart shoppingCart,
                                             string orderXML,
                                             string orderNumber,
                                             DateTime orderDate)
        {
            return Providers.ShoppingCartProvider.UpdateShoppingCart(shoppingCart,
                                             orderXML,
                                             orderNumber,
                                             orderDate);
        }

        public string getTodayMagazineDeletedSessionKey(string distributorID, string localc, int shoppingCartID)
        {
            return Providers.ShoppingCartProvider.getTodayMagazineDeletedSessionKey(distributorID, localc, shoppingCartID);
        }

        public bool TodayMagazineDeleted(string distributorID, string localc, int shoppingCartID)
        {
            return Providers.ShoppingCartProvider.TodayMagazineDeleted(distributorID, localc, shoppingCartID);
        }

        public bool DeleteOldShoppingCartForCustomerOrder(string distributorID, string customerOrderID)
        {
            return Providers.ShoppingCartProvider.DeleteOldShoppingCartForCustomerOrder(distributorID, customerOrderID);
        }

        public bool DeleteShoppingCart(MyHLShoppingCart shoppingCart, List<string> skus)
        {
            return Providers.ShoppingCartProvider.DeleteShoppingCart(shoppingCart, skus);
        }

        public List<DualMonthPair> GetDualOrderMonthDates(DateTime fromDate,
                                                   DateTime toDate,
                                                   string isoCountryCode)
        {
            return Providers.ShoppingCartProvider.GetDualOrderMonthDates(fromDate,
                                          toDate,
                                          isoCountryCode);
        }

        public DateTime GetEOMDate(DateTime date, string isoCountryCode)
        {
            return Providers.ShoppingCartProvider.GetEOMDate(date, isoCountryCode);
        }

        public List<DualMonthPair> GetDualOrderMonth(string orderMonthValue, string countryCode)
        {
            return Providers.ShoppingCartProvider.GetDualOrderMonth(orderMonthValue, countryCode);
        }

        public List<DualMonthPair> GetDualOrderMonthDatesFromService(DateTime fromDate,
                                                              DateTime toDate,
                                                              string isoCountryCode)
        {
            return Providers.ShoppingCartProvider.GetDualOrderMonthDatesFromService(fromDate,
                                                     toDate,
                                                     isoCountryCode);
        }

        public List<ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                 ShoppingCartItem_V01 item,
                                                 ShoppingCartRuleReason reason)
        {
            return Providers.ShoppingCartProvider.processCart(cart,
                               item,
                               reason);
        }

        public List<ShoppingCartRuleResult> processCart(MyHLShoppingCart cart,
                                                 List<ShoppingCartItem_V01> items,
                                                 ShoppingCartRuleReason reason)
        {
            return Providers.ShoppingCartProvider.processCart(cart,
                               items,
                               reason);
        }

        public MyHLShoppingCart GetNewUnsavedShoppingCart(string distributorID, string locale)
        {
            return Providers.ShoppingCartProvider.GetNewUnsavedShoppingCart(distributorID, locale);
        }

        public string ValidateCartItem(string sku,
                                        int quantity,
                                        out int newQuantity,
                                        string locale,
                                        string warehouseCode)
        {
            return Providers.ShoppingCartProvider.ValidateCartItem(sku,
                                    quantity,
                                    out newQuantity,
                                    locale,
                                    warehouseCode);
        }

        public MyHLShoppingCart GetShoppingCartFromInvoice(string distributorID, string locale, long invoiceID)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromInvoice(distributorID, locale, invoiceID);
        }

        public MyHLShoppingCart GetShoppingCartFromMemberInvoice(string distributorID, string locale, int invoiceID)
        {
            return Providers.ShoppingCartProvider.GetShoppingCartFromMemberInvoice(distributorID, locale, invoiceID);
        }

        public EligibleDistributorInfo_V01 GetEligibleForPromo(string distributorId, string locale)
        {
            return Providers.ShoppingCartProvider.GetEligibleForPromo(distributorId, locale);

        }

        public EligibleDistributorInfo_V01 SaveEligibleForPromo(EligibleDistributorInfo_V01 promoInfo, int shoppingCartId,
                                                         string orderNumber,
                                                         ref int updShoppingCartId, ref string updOrderNumber)
        {
            return Providers.ShoppingCartProvider.SaveEligibleForPromo(promoInfo, shoppingCartId,
                                                           orderNumber,
                                                          ref updShoppingCartId, ref updOrderNumber);
        }

        public EligibleDistributorInfo_V01 SaveEligibleForPromoToFromService(EligibleDistributorInfo_V01 promoInfo,
                                                                      int shoppingCartId, string orderNumber,
                                                                      ref int updShoppingCartId,
                                                                      ref string updOrderNumber)
        {
            return Providers.ShoppingCartProvider.SaveEligibleForPromoToFromService(promoInfo,
                                                     shoppingCartId, orderNumber,
                                                     ref updShoppingCartId,
                                                     ref updOrderNumber);
        }

        public bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart)
        {
            return Providers.ShoppingCartProvider.IsEnabledPaymentType(paymentType, shoppingCart);

        }

        public bool IsEnabledPaymentType(string paymentType, MyHLShoppingCart shoppingCart, bool isHidden)
        {
            return Providers.ShoppingCartProvider.IsEnabledPaymentType(paymentType, shoppingCart, isHidden);
        }

        public void CheckSplitOrder(MyHLShoppingCart shoppingCart)
        {
            Providers.ShoppingCartProvider.CheckSplitOrder(shoppingCart);
        }

        public List<ShoppingCartItem_V01> GetDiscontinuededSku(MyHLShoppingCart originalUnfilteredCopyCart)
        {
            return ShoppingCartProvider.GetDiscontinuededSku(originalUnfilteredCopyCart);
        }

        public List<ShoppingCartItem_V01> GetDiscontinuededSku(int shoppingCartId, string distributorId, string locale, int newShoppingCartId,
            SKU_V01ItemCollection skuitemList = null)
        {
            return ShoppingCartProvider.GetDiscontinuededSku(shoppingCartId, distributorId, locale, newShoppingCartId,
                skuitemList);
        }
    }
}
