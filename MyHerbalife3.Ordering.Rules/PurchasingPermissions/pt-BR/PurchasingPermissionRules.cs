using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.pt_BR
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        public bool CanPurchase(string distributorID, string countryCode)
        {
            bool canPurchase = false;
            List<MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true);
            //Must have BRPF TinCode to purchase
            if (tins.Count > 0)
            {
                List<string> requiredTins = new List<string>(new string[] {"BRPF"});
                var tin = (from t in tins from r in requiredTins where t.IDType.Key == r select t).ToList();
                if (tin != null && tin.Count > 0)
                {
                    canPurchase = true;

                    //// Savin the CPF# in the session info
                    //if (HttpContext.Current.Session != null)
                    //{
                    //    var sessionInfo = SessionInfo.GetSessionInfo(distributor.Value.ID, Locale);
                    //    sessionInfo.BRPF = tin.FirstOrDefault().ID;
                    //}
                }
            }
            //}
            return canPurchase;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                try
                {

                    if (!CanPurchase(cart.DistributorID, Country))
                    {
                        Result.Result = RulesResult.Failure;
                        Result.AddMessage(
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_Rules", HLConfigManager.Platform), "CantPurchase").ToString());
                        cart.RuleResults.Add(Result);
                    }

                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Error while performing Add to Cart Rule for Brazilian distributor: {0}, Cart Id:{1}, \r\n{2}",
                            cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
                }
            }
            else if (reason == ShoppingCartRuleReason.CartRetrieved)
            {
                if (cart != null)
                {
                    MyHLShoppingCart shoppingCart = cart as MyHLShoppingCart;
                    if (shoppingCart != null && shoppingCart.DeliveryInfo != null)
                    {
                        ShippingInfo shippingInfo = shoppingCart.DeliveryInfo;
                        if (shippingInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                        {
                            if (shippingInfo.Address != null)
                            {
                                switch (shippingInfo.Address.Address.StateProvinceTerritory)
                                {
                                    case "AC":
                                        shippingInfo.WarehouseCode = "DO";
                                        shippingInfo.FreightCode = "RAC";
                                        break;
                                    case "AL":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RAL";
                                        break;
                                    case "AM":
                                        shippingInfo.WarehouseCode = "BM";
                                        shippingInfo.FreightCode = "RAM";
                                        break;
                                    case "AP":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "AAP";
                                        break;
                                    case "BA":
                                        shippingInfo.WarehouseCode = "43";
                                        if (!string.IsNullOrEmpty(shippingInfo.FreightCode))
                                        {
                                            if (shippingInfo.FreightCode != "ABA" &&
                                                shippingInfo.FreightCode != "RBA")
                                            {
                                                shippingInfo.FreightCode = "RBA";
                                            }
                                        }
                                        else
                                        {
                                            shippingInfo.FreightCode = "RBA";
                                        }
                                        break;
                                    case "CE":
                                        shippingInfo.WarehouseCode = "B0";
                                        shippingInfo.FreightCode = "RCE";
                                        break;
                                    case "DF":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RDF";
                                        break;
                                    case "ES":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RES";
                                        break;
                                    case "GO":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RGO";
                                        break;
                                    case "MA":
                                        if (string.IsNullOrEmpty(shippingInfo.FreightCode) ||
                                            (shippingInfo.FreightCode != "RMA" &&
                                            shippingInfo.FreightCode != "AM1"))
                                        {
                                            shippingInfo.WarehouseCode = "B4";
                                            shippingInfo.FreightCode = "RMA";
                                        }
                                        break;
                                    case "MG":
                                        shippingInfo.WarehouseCode = "BH";
                                        shippingInfo.FreightCode = "RMG";
                                        break;
                                    case "MS":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RMS";
                                        break;
                                    case "MT":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RMT";
                                        break;
                                    case "PA":
                                        if (!string.IsNullOrEmpty(shippingInfo.FreightCode))
                                        {
                                            if (shippingInfo.FreightCode == "RBE")
                                            {
                                                shippingInfo.WarehouseCode = "BE";
                                            }
                                            else if (shippingInfo.FreightCode == "APA")
                                            {
                                                shippingInfo.WarehouseCode = "43";
                                            }
                                            else
                                            {
                                                shippingInfo.WarehouseCode = "BE";
                                                shippingInfo.FreightCode = "RBE";
                                            }
                                        }
                                        else
                                        {
                                            shippingInfo.WarehouseCode = "BE";
                                            shippingInfo.FreightCode = "RBE";
                                        }
                                        break;
                                    case "PB":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RPB";
                                        break;
                                    case "PE":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RPE";
                                        break;
                                    case "PI":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RPI";
                                        break;
                                    case "PR":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RPR";
                                        break;
                                    case "RJ":
                                        shippingInfo.WarehouseCode = "BR";
                                        shippingInfo.FreightCode = "RRJ";
                                        break;
                                    case "RN":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RRN";
                                        break;
                                    case "RO":
                                        shippingInfo.WarehouseCode = "DO";
                                         shippingInfo.FreightCode = "RRO";
                                        break;
                                    case "RR":
                                        shippingInfo.WarehouseCode = "BM";
                                        shippingInfo.FreightCode = "RRR";
                                        break;
                                    case "RS":
                                        shippingInfo.WarehouseCode = "B2";
                                        shippingInfo.FreightCode = "RRS";
                                        break;
                                    case "SC":
                                        shippingInfo.WarehouseCode = "43";
                                        shippingInfo.FreightCode = "RSC";
                                        break;
                                    case "SE":
                                        shippingInfo.WarehouseCode = "B4";
                                        shippingInfo.FreightCode = "RSE";
                                        break;
                                    case "SP":
                                        shippingInfo.WarehouseCode = "DC";
                                        shippingInfo.FreightCode = "RSP";
                                        break;
                                    case "TO":
                                        shippingInfo.WarehouseCode = "43";
                                        if (!string.IsNullOrEmpty(shippingInfo.FreightCode))
                                        {
                                            if (shippingInfo.FreightCode != "ATO" &&
                                                shippingInfo.FreightCode != "RTO")
                                            {
                                                shippingInfo.FreightCode = "RTO";
                                            }
                                        }
                                        else
                                        {
                                            shippingInfo.FreightCode = "RTO";
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return Result;
        }
    }
}