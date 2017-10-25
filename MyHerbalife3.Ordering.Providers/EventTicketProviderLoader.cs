using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public class EventTicketProviderLoader : IEventTicketProviderLoader
    {
        
        public Dictionary<int, SKU_V01> ValidateEventList(Dictionary<int, SKU_V01> eventProductList,
                                                          Dictionary<int, SKU_V01> allreadypurchasedEventTicktList,
                                                          int limit, ref ShoppingCartRuleResult result)
        {
            var eligibleEventProductList = new Dictionary<int, SKU_V01>();
            foreach (var item in eventProductList)
            {
                if (item.Key < limit)
                {
                    if (allreadypurchasedEventTicktList.Any())
                    {
                        var item1 = item;
                        var val = allreadypurchasedEventTicktList.FirstOrDefault(z => z.Value.SKU == item1.Value.SKU);
                        var AvilQuantity = limit - val.Key;
                        if (val.Value != null)
                        {
                            if (val.Key == limit)
                            {
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventProductAlreadyBeenPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                            }
                            else if (AvilQuantity < item.Key)
                            {
                                eligibleEventProductList.Add(AvilQuantity, item.Value);
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventMaxPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                            }
                            else if (val.Key < limit)
                            {

                                eligibleEventProductList.Add(item.Key, item.Value);
                                if (AvilQuantity < item.Key)
                                {
                                    result.Result = RulesResult.Failure;
                                    var msg = GetRulesResourceString("EventMaxPurchased");
                                    msg = string.Format(msg, val.Value.SKU);
                                    result.AddMessage(msg);
                                }
                            }
                            else 
                            {
                                eligibleEventProductList.Add(item.Key, item.Value);
                            }
                        }
                        else
                            eligibleEventProductList.Add(item.Key, item.Value);
                    }
                    else
                    {
                        eligibleEventProductList.Add(item.Key, item.Value);
                    }
                }
                else if (item.Key == limit)
                {
                    if (allreadypurchasedEventTicktList.Any())
                    {
                        var item1 = item;
                        var val = allreadypurchasedEventTicktList.FirstOrDefault(z => z.Value.SKU == item1.Value.SKU);
                        if (val.Value != null)
                        {
                            if (val.Key == limit)
                            {
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventProductAlreadyBeenPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                            }
                            else if (limit > val.Key)
                            {
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventMaxPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                                eligibleEventProductList.Add(item.Key - val.Key, item.Value);
                            }
                        }
                        else
                            eligibleEventProductList.Add(item.Key, item.Value);
                    }
                    else
                    {
                        eligibleEventProductList.Add(item.Key, item.Value);
                    }
                }
                else if (item.Key > limit)
                {

                    if (allreadypurchasedEventTicktList.Any())
                    {
                        var item1 = item;
                        var val = allreadypurchasedEventTicktList.FirstOrDefault(z => z.Value.SKU == item1.Value.SKU);
                        if (val.Value != null)
                        {
                            if (val.Key == limit)
                            {
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventProductAlreadyBeenPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                            }
                            else if (val.Key < limit)
                            {
                                result.Result = RulesResult.Failure;
                                var msg = GetRulesResourceString("EventMaxPurchased");
                                msg = string.Format(msg, val.Value.SKU);
                                result.AddMessage(msg);
                                eligibleEventProductList.Add(limit - val.Key, item.Value);
                            }
                        }
                        else
                        {
                            result.Result = RulesResult.Failure;
                            var msg = GetRulesResourceString("EventMaxPurchased");
                            msg = string.Format(msg, item.Value.SKU);
                            result.AddMessage(msg);
                            eligibleEventProductList.Add(limit, item.Value);
                        }

                    }
                    else
                    {
                        result.Result = RulesResult.Failure;
                        var msg = GetRulesResourceString("EventMaxPurchased");
                        msg = string.Format(msg, item.Value.SKU);
                        result.AddMessage(msg);
                        eligibleEventProductList.Add(limit, item.Value);
                    }
                }

            }
            return eligibleEventProductList;
        }

        public Dictionary<int, SKU_V01> LoadSkuPurchasedCount(Dictionary<int, SKU_V01> eventProductList, string distributorId, string countrycode)
        {
            var purchasedEventticketSkuList = new Dictionary<int, SKU_V01>();
            List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased> skuList = eventProductList.Select(x => new MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.SkuOrderedAndPurchased
            {
                SKU = (x.Value.SKU != null && x.Value.SKU.Trim() != "" ) ? x.Value.SKU.Trim() : x.Value.CatalogItem.StockingSKU.Trim(),
                Category = "ETO",
            }).ToList();

            var rslt = Providers.China.OrderProvider.GetSkuOrderedAndPurchased(countrycode, distributorId, null, null, skuList);
            if (rslt != null)
            {
                foreach (var r in eventProductList)
                {
                    //  SkuOrderedAndPurchased r1 = r;
                    var mList = rslt.Where(x => x.SKU.Trim() == ((r.Value.SKU != null && r.Value.SKU.Trim() != "") ? r.Value.SKU.Trim() : r.Value.CatalogItem.StockingSKU.Trim()));
                    foreach (var m in mList)
                    {
                        purchasedEventticketSkuList.Add(m.QuantityPurchased, r.Value);
                    }
                }
            }

            return purchasedEventticketSkuList;
        }

        protected string GetRulesResourceString(string key, string defaultValue = null)
        {
            var clsKey = string.Format("{0}_Rules", HLConfigManager.Platform);
            var value = HttpContext.GetGlobalResourceObject(clsKey, key);

            if (value == null || !(value is string))
            {
                HL.Common.Logging.LoggerHelper.Warn(string.Format("Missing {0} resource object. Key: {1}", clsKey, key));
                return string.Empty;
            }

            return value as string;
        }
    }
}
