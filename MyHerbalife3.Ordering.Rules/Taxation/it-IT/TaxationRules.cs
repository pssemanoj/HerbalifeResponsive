using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.it_IT
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            if (null != order)
            {
                var limits = order.PurchasingLimits as PurchasingLimits_V01;
                if (null == limits)
                {
                    limits =
                        PurchasingLimitProvider.GetCurrentPurchasingLimits(order.DistributorID);
                    order.PurchasingLimits = limits;
                }
                if (null == limits)
                {
                    //Log an error here - can't tax this, it is invalid for IT if we don't have Limits created
                }
                else
                {
                    //Add suplemental items for Incaricato VAT and INPS calcs
                    limits.Items = new SupplementalItems();
                    limits.Items.Add("ConsignmentWitholdingRate", new List<decimal>(new[] {0.1794M}));
                    //Both A1 and B1
                    limits.Items.Add("FlatFreightRate", new List<decimal>(new[] {0.045M}));
                    if (!string.IsNullOrEmpty(limits.PurchaseSubType) && limits.PurchaseSubType.Equals("A1"))
                        //VAT Registered
                    {
                        DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(order.DistributorID, Country);
                        //Calcualte the INPS contribution for A1
                        if (distributorOrderingProfile.YTDEarnings > limits.MaxEarningsLimit &&
                            distributorOrderingProfile.YTDEarnings <=
                            HLConfigManager.Configurations.DOConfiguration.MaxTaxableEarnings)
                        {
                            var taxIds = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
                            if (taxIds.Count > 0 &&
                                taxIds.Where(
                                    p =>
                                    p.IDType.Key.Equals("IEVA") & p.IDType.Key.Equals("ITIN") &
                                    p.IDType.Key.Equals("ITSS")).Count() > 0)
                            {
                                limits.Items.Add("INPSContributionRate", new List<decimal>(new[] {0.0442M}));
                            }
                            else
                            {
                                limits.Items.Add("INPSContributionRate", new List<decimal>(new[] {0.0695M}));
                            }
                        }
                        else
                        {
                            limits.Items.Add("INPSContributionRate", new List<decimal>(new[] {0M}));
                        }
                        limits.Items.Add("VATReimbursementRate", new List<decimal>(new[] {0.20M}));
                    }
                }

                CheckforMultipleDuplicateLinkedSkus(order);
            }
        }

        private void CheckforMultipleDuplicateLinkedSkus(Order_V01 order)
        {
            var items = order.OrderItems;
            var promoskus = items.FindAll(p => p.SKU == "8153");
            if (null != promoskus && promoskus.Count > 1)
            {
                int totalQuantity = promoskus.Sum(s => s.Quantity);
                items.RemoveAll(p => p.SKU == "8153");
                items.Add(new OrderItem_V01("0", "8153", totalQuantity));
            }
        }
    }
}