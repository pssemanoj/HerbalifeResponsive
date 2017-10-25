using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers;
using HL.Common.ValueObjects;
using MyHerbalife3.Shared.ViewModel;
using System.Web.Security;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;


namespace MyHerbalife3.Ordering.Providers.FraudControl
{
    public static class FraudControlProvider
    {
        public static bool IsSubjectToFraudCheck(string locale, MyHLShoppingCart cart, string subTypeCode, DateTime applicationDate, MyHLConfiguration gdoConfig)
        {
            if (gdoConfig == null || gdoConfig.FControlSettings == null)
                return false;
            if (cart != null && gdoConfig != null)
            {
                var fControlConfig = gdoConfig.FControlSettings;
                if (fControlConfig != null)
                {
                    // check if ETO order is subject to fraud check
                    if (!fControlConfig.ETOIncluded && cart.OrderCategory == OrderCategoryType.ETO)
                    {
                        return false;
                    }
                    // check if number of sku in order is subject to fraud check
                    if (fControlConfig.NumberOfItem > 0 && cart.CartItems != null && cart.CartItems.Count() > 0)
                    {
                        if (cart.CartItems.Any(i => i.Quantity > fControlConfig.NumberOfItem))
                            return true;
                    }

                    // check if pick up order is subject to fraud check
                    var shipment = cart.DeliveryInfo;
                    if (shipment!=null)
                    {
                        bool bFraudCheck = !(fControlConfig.PickupOrderIncluded==false && shipment.Option == DeliveryOptionType.Pickup);
                        if (!bFraudCheck)
                            return bFraudCheck;

                        // check zip code
                        if (fControlConfig.Zipcodes != null && shipment.Option == DeliveryOptionType.Shipping)
                        {
                            if (shipment.Address != null && shipment.Address.Address != null)
                            {
                                if (fControlConfig.Zipcodes.Contains(shipment.Address.Address.PostalCode))
                                {
                                    return true;
                                }
                            }
                        }

                        var states = fControlConfig.Statecodes;
                        if (states != null && shipment.Option == DeliveryOptionType.Shipping && shipment.Address != null && shipment.Address.Address != null)
                        {
                            if (states.Where(s => s.Equals(shipment.Address.Address.StateProvinceTerritory)).Any())
                            {
                                return false;
                            }
                        }
                    }
                    
                    // check DS subtype
                    var levels = fControlConfig.DistributorLevels;
                    if (levels != null)
                    {
                        if (!levels.Where(l => l.ToString().Replace("_", "").Equals(subTypeCode)).Any())
                        {
                            return false;
                        }
                    }

                    //  check total volume point and amount due
                    if (cart.Totals!=null)
                    {
                        var orderTotals = cart.Totals as OrderTotals_V01;
                        if (orderTotals!=null)
                        {
                            if (orderTotals.VolumePoints < fControlConfig.VolumePoint || orderTotals.AmountDue < fControlConfig.AmountDue)
                                return false;
                        }
                    }

                    if (shipment != null)
                    {
                        // freight code
                        if (fControlConfig.FreightCodes != null)
                        {
                            if (fControlConfig.FreightCodes.Contains(shipment.FreightCode))
                            {
                                return false;
                            }
                        }
                    }
                    // check application date
                    DateTime currentLocalDatetime = HL.Common.Utilities.DateUtils.ConvertToLocalDateTime(DateTime.Now, cart.CountryCode);
                    if (currentLocalDatetime.Subtract(applicationDate).TotalDays >= fControlConfig.DayApplication)
                    {
                        return false;
                    }
                    
                }
            }
            return true;
        }
    }
}
