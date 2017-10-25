using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Rules.PurchasingLimits.Global;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using CountryType = HL.Common.ValueObjects.CountryType;

namespace MyHerbalife3.Ordering.Rules.PurchaseRestriction.Global
{
    public class PurchaseRestrictionRules : PurchaseRestrictionRuleBase, IPurchaseRestrictionRule
    {
        private readonly int WaitMinutes = Settings.GetRequiredAppSetting<int>("DistributorProfileReloadMinutes");

        /// <summary>
        /// determine if DS can purchase P type product
        /// </summary>
        /// <param name="tins"></param>
        /// <param name="CountryOfProcessing"></param>
        /// <param name="CountyCode"></param>
        /// <returns></returns>
        public override bool CanPurchasePType(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
            bool canPurchasePType = true;

            switch (CountyCode)
            {
                case "IN":
                    {
                        //1. Distributors or Foreign Distributors with “Dummy TIN No”, “INID“can purchase any category of products.  They will be limited to purchase up to 1000vp per month. Web site should display warning message “You have reached your purchasing limit of 1000 VP per month, please amend your order before checkout” and direct purchaser to shopping cart page and do not allow purchaser to submit the order if PPV of the added order plus current orders’ PPV is >1000PPV. (Logic will further discuss with INT Dev since HMS is only show warning message if over 1000/order but not stopping agent to continue). DS with a TIN Code of INID will be treated as a Foreign Distributor.
                        //2. For the distributors or Foreign Distributors that do not possess the local national ID (TIN Code: Not equal to INID, INTX, INPP, INRS, INNR or INAD) can only purchase Promotion, IBP and Literature items.(L and A)

                        canPurchasePType = (new[] { "INTX", "INPP", "INRS", "INNR", "INAD", "INID", "INRC", "INVR", "INDL" }.Intersect(from t in tins select t.IDType.Key).ToList().Count > 0);
                        
                        if (!canPurchasePType)
                        {
                            var dsProfile = DistributorOrderingProfileProvider.GetProfile(DistributorProfileModel.Id, CountyCode);
                            if (dsProfile != null && (!dsProfile.Refreshed || (DateTime.Now - dsProfile.ApplicationDate).TotalMinutes <= WaitMinutes))
                            {
                                tins = DistributorOrderingProfileProvider.GetTinList(DistributorProfileModel.Id, true, true);
                                canPurchasePType = (new[] { "INTX", "INPP", "INRS", "INNR", "INAD", "INID", "INRC", "INVR", "INDL" }.Intersect(from t in tins select t.IDType.Key).ToList().Count > 0);
                            }
                        }
                    }
                    break;
                case "MY":
                    {
                        // For the DS without local national ID (TIN Code: Not equal to MYID), they can only purchase IBP, Promotion, and Literature items.
                        canPurchasePType =  tins.Find(t => t.IDType.Key == "MYID") != null;
                    }
                    break;
                case "SG":
                    {
                        // For Foreign DS without local National ID they cannot purchase "P" type products. They can purchase "L" and "A" type products
                        canPurchasePType = false;

                        if (tins.Find(t => t.IDType.Key == "SNID") != null)
                            canPurchasePType = true;
                        else
                        {
                            var countryCodes = (new List<string>(CountryType.SG.HmsCountryCodes)).Union(new List<string>() { CountryType.SG.Key });
                            if (countryCodes.Contains(CountryOfProcessing))
                            {
                                canPurchasePType = true;
                            }
                        }
                        // DS with Dummy TIN No "S0000000S", can purchase any category of products up to 1100 vp per order
                        if (tins != null && tins.Find(t => t.ID == "S0000000S") != null)
                        {
                            canPurchasePType = true;
                        }
                    }
                    break;
                case "TH":
                    {
                        string dummyTin = "TH00000000000";
                        var isDummyTin = tins.Find(t => t.ID == dummyTin) != null;
                        var tid = tins.Find(t => t.IDType.Key == "THID");
                        //COP = Thai Tin = No TIN, Can place only L and A items
                        //COP Not Thai, No Tin- Can place L & A  item
                       
                        var countryCodes = (new List<string>(CountryType.TH.HmsCountryCodes)).Union(new List<string>() { CountryType.TH.Key });
                        var isCOPThai = countryCodes.Contains(CountryOfProcessing);
                        if ((isCOPThai && tid == null) || (!isCOPThai && tid == null && !isDummyTin))
                        {
                            canPurchasePType = false;
                        }
                    }
                    break;
            }
            return canPurchasePType;
        }

        private void setValues(PurchasingLimits_V01 l, PurchasingLimitRestrictionPeriod period, decimal maxValue)
        {
            l.RestrictionPeriod = period;
            l.maxVolumeLimit = l.RemainingVolume = maxValue;
            l.LimitsRestrictionType = LimitsRestrictionType.PurchasingLimits;
        }

        /// <summary>
        /// if DS has purchasing limits
        /// </summary>
        /// <param name="tins"></param>
        /// <param name="orderMonth"></param>
        /// <param name="distributorId"></param>
        public override void SetPurchaseRestriction(List<TaxIdentification> tins, int orderMonth, string distributorId, IPurchaseRestrictionManager manager)
        {
            //if (manager.PurchasingLimits == null)
            //    return;

            if (manager.ApplicableLimits == null)
                return;

            base.SetPurchaseRestriction(tins, orderMonth, distributorId, manager);

            PurchaseLimitType limitType = PurchaseLimitType.Volume;
            var limits = GetLimits(LimitsRestrictionType.PurchasingLimits,orderMonth,manager);
            if (limits == null)
            {
                return;
            }

            //if (!manager.CanPurchase)
           //     return;

            switch (this.Country)
            {
                case "BA":
                    {
                        //DS without BATX are restricted to 5000VPs
                        limitType = tins.Find(t => t.IDType.Key == "BATX") != null ? PurchaseLimitType.None : limitType;
                    }
                    break;
                case "BE":
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.Annually 
                            && tins.Any(t => t.IDType.Key == "BENO" || t.IDType.Key == "BEVT"))
                        {
                            limitType = PurchaseLimitType.None;
                        }
                    }
                    break;
                case "BY":
                    {
                        limitType = PurchaseLimitType.None;
                    }
                    break;
                case "CO":
                    {
                        if ((new List<string>(CountryType.CO.HmsCountryCodes)).Union(new List<string>() { CountryType.CO.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                            limitType = PurchaseLimitType.None;
                    }
                    break;
                //case "FR":
                //    {
                //        limitType = _IPurchaseRestrictionManager.OrderSubType != "F" ? PurchaseLimitType.None : limitType;
                //    }
                //    break;
                case "IN":
                    {
                        if (tins.Find(t => t.IDType.Key == "INID") == null && new[] { "INTX", "INPP", "INRS", "INNR", "INAD", "INRC", "INVR", "INDL" }.Intersect(from t in tins select t.IDType.Key).ToList().Count > 0)
                        {
                            limitType = PurchaseLimitType.None;
                        }

                        if (limitType != PurchaseLimitType.None)
                        {
                            var dsProfile = DistributorOrderingProfileProvider.GetProfile(DistributorProfileModel.Id, "IN");
                            if (dsProfile != null && (!dsProfile.Refreshed || (DateTime.Now - dsProfile.ApplicationDate).TotalMinutes <= WaitMinutes))
                            {
                                tins = DistributorOrderingProfileProvider.GetTinList(DistributorProfileModel.Id, true, true);
                                if (tins.Find(t => t.IDType.Key == "INID") == null && new[] { "INTX", "INPP", "INRS", "INNR", "INAD", "INRC", "INVR", "INDL" }.Intersect(from t in tins select t.IDType.Key).ToList().Count > 0)
                                {
                                    limitType = PurchaseLimitType.None;
                                }
                            }
                        }
                     }
                     break;
                case "HU":
                     {
                         if (tins.Find(p => p.IDType.Key == "HUVT") != null && tins.Find(p => p.IDType.Key == "HUBL" || p.IDType.Key == "HUPT") != null)
                         {
                             limitType = PurchaseLimitType.None;
                         }
                     }
                     break;
                case "KZ":
                     {
                         if ((new List<string>(CountryType.KZ.HmsCountryCodes)).Union(new List<string>() { CountryType.KZ.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                             limitType = (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "KZTX", "KZBL" }).Count() == 2) ? PurchaseLimitType.None : limitType;
                         else
                             limitType = PurchaseLimitType.None;
                     }
                     break;
                case "MK":
                     {
                         limitType = tins.Find(p => p.IDType.Key == "MKTX") != null ? PurchaseLimitType.None : limitType;
                     }
                     break;
                case "MN":
                     {
                         if ((new List<string>(CountryType.MN.HmsCountryCodes)).Union(new List<string>() { CountryType.MN.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                         {
                             limitType = tins.Find(p => p.IDType.Key == "MGTX") != null ? PurchaseLimitType.None : limitType;
                         }
                         else
                             limitType = PurchaseLimitType.Volume;
                     }
                     break;
                
                case "MY":
                    {
                        limitType = tins.Find(t => t.IDType.Key == "MYID") != null ? PurchaseLimitType.None : limitType;
                        if ( tins.Find(t => t.IDType.Key == "MYID" && t.ID == "MY00" ) != null)
                        {
                            //For the DS with ‘Dummy TIN No” (MYID = ‘MY00’), he/she can purchase any category of products up to 1000 Volume Points per month.
                            limitType = PurchaseLimitType.Volume;
                        }
                    }
                    break;

                case "PE":
                    {
                        if ((new List<string>(CountryType.PE.HmsCountryCodes)).Union(new List<string>() { CountryType.PE.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                            limitType = tins.Find(p => p.IDType.Key == "PETX") != null ? PurchaseLimitType.None : limitType;
                        else
                            limitType = (tins.Select(c => c.IDType.Key).Intersect(new[] { "PEID", "PETX" }).Count() == 2) ? PurchaseLimitType.None : limitType;

                    }
                    break;
                case "PF":
                    {
                        limitType = tins.Find(t => t.IDType.Key == "FPBL") != null ? PurchaseLimitType.None : limitType;

                        if (limitType == PurchaseLimitType.None)
                        {
                            // NOTE THAT NEED TO CHECK WH on ADD TO CART TOO
                            if (tins.Find(t => t.IDType.Key == "APP") != null)
                            {
                                limitType = PurchaseLimitType.Volume;
                            }
                        }
                    }
                    break;
                case "PH":
                    {
                        var countryCodes = (new List<string>(CountryType.PH.HmsCountryCodes)).Union(new List<string>() { CountryType.PH.Key });
                        if (!countryCodes.Contains(DistributorProfileModel.ProcessingCountryCode))
                        {
                            limitType = PurchaseLimitType.Volume;
                            setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1050.00m);
                        }
                    }
                    break;
                
                case "PY":
                    {
                        limitType = tins.Find(t => t.IDType.Key == "PYTX") != null ? PurchaseLimitType.None : limitType;
                    }
                    break;
                
               
                case "PL":
                    {
                        var countryCodes = (new List<string>(CountryType.PL.HmsCountryCodes)).Union(new List<string>() { CountryType.PL.Key });
                        //Foreign DS have no limit. "
                        if (!countryCodes.Contains(DistributorProfileModel.ProcessingCountryCode))
                        {
                            limitType = PurchaseLimitType.None;
                        }
                        else
                        {
                            limitType = tins.Find(p => p.IDType.Key == "REGN") != null && tins.Find(p => p.IDType.Key == "APP") == null ? PurchaseLimitType.None : limitType;
                        }
                    }
                    break;
                case "RO":
                    {
                        if (tins.Find(t => t.IDType.Key == "ROBL") != null)
                            limitType = PurchaseLimitType.None;
                        if (tins.Find(t => t.IDType.Key == "APP") != null)
                            limitType = PurchaseLimitType.Volume;
                    }
                    break;
                case "RS":
                    {
                        if (tins.Find(t => t.IDType.Key == "SRTX") != null)
                            limitType = PurchaseLimitType.None;
                    }
                    break;
                case "RU":
                    {
                        limitType = (tins != null && tins.Select(c => c.IDType.Key).Intersect(new[] { "RUTX", "RUBL" }).Count() == 2) ? PurchaseLimitType.None : limitType;
                    }
                    break;
                case "SG":
                    {
                        if (tins.Find(t => t.IDType.Key == "SNID") != null)
                            limitType = PurchaseLimitType.None;
                        else
                        {
                            if ((new List<string>(CountryType.SG.HmsCountryCodes)).Union(new List<string>() { CountryType.SG.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                            {
                                limitType = PurchaseLimitType.None;
                            }
                        }
                        // DS with Dummy TIN No "S0000000S", can purchase any category of products up to 1100 vp per order
                        if (tins != null && tins.Find(t => t.ID == "S0000000S") != null)
                        {
                            limitType = PurchaseLimitType.Volume;
                            setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1100.00m);
                        }
                    }
                    break;
                case "SI":
                    {
                        if (tins.Find(t => t.IDType.Key == "SITX") != null)
                            limitType = PurchaseLimitType.None;
                    }
                    break;
                
                case "TH":
                    
                    {
                        if (tins.Find(t => t.ID == "TH00000000000") != null) 
                        {
                            limitType = PurchaseLimitType.Volume;
                            setValues(limits, PurchasingLimitRestrictionPeriod.PerOrder, 1050.00m);
                        }
                    }
                    break;
                case "UA":
                    {
                        if (tins.Find(t => t.IDType.Key == "UABL") != null)
                            limitType = PurchaseLimitType.None;
                    }
                    break;
                case "UY":
                    {
                        limitType = tins.Find(t => t.IDType.Key == "UYTX") != null ? PurchaseLimitType.None : limitType;
                    }
                    break;
  
                case "VN":
                    {
                        if ((new List<string>(CountryType.VN.HmsCountryCodes)).Union(new List<string>() { CountryType.VN.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                        {
                            limitType = tins.Find(t => t.IDType.Key == "VNID") != null ? PurchaseLimitType.None : limitType;
                        }
                    }
                    break;
                case "BG":
                    {
                        var processingCountryCode = DistributorProfileModel.ProcessingCountryCode;

                        if (processingCountryCode == "BG")
                        {
                            var now = DateUtils.GetCurrentLocalTime("BG");
                            var bgTin = tins.Find(t => t.IDType.Key == "BGBL");
                            var cart = ShoppingCartProvider.GetShoppingCart(distributorId, Locale, true);
                            if (bgTin != null && bgTin.IDType != null && bgTin.IDType.ExpirationDate > now && cart != null && cart.SelectedDSSubType != "PC")
                            {
                                limitType = PurchaseLimitType.None;
                            }
                            else  //BG Members with NO BGBL have a 7500 VP per year
                            {
                                limitType = PurchaseLimitType.Volume;
                            }
                        }
                        else  //foreign members have a 7500 VP per year
                        {
                            limitType = PurchaseLimitType.Volume;
                        }
                    }
                    break;
                default:
                    if (!HLConfigManager.Configurations.DOConfiguration.GetPurchaseLimitsFromFusion)
                        limitType = PurchaseLimitType.None;
                    break;
            }

            if (limits != null)
            {
                if (limits.maxVolumeLimit == -1 && limits.MaxEarningsLimit == -1)
                {
                    limits.PurchaseLimitType = PurchaseLimitType.None;
                }
                else
                {
                    limits.PurchaseLimitType = limitType;
                }
                SetLimits(orderMonth, manager, limits);

            }
            
        }

        /// <summary>
        /// if DS can purchase on site
        /// </summary>
        /// <param name="tins"></param>
        /// <param name="CountryOfProcessing"></param>
        /// <param name="CountyCode"></param>
        /// <returns></returns>
        public override bool CanPurchase(List<TaxIdentification> tins, string CountryOfProcessing, string CountyCode)
        {
        
            bool canPurchase = true;

            //Check for override rule whether this DS is exempted.
            if (CatalogProvider.IsDistributorExempted(this.DistributorProfileModel.Id))
            {
                return true;
            }

            var countryCodes = new List<string>();
            switch (CountyCode)
            {
                case "SG":
                    {
                        countryCodes.AddRange(CountryType.SG.HmsCountryCodes);
                        countryCodes.Add(CountryType.SG.Key);
                        if (countryCodes.Contains(CountryOfProcessing))
                            canPurchase =  !(tins.Count == 0 || !tins.Any(t => t.IDType.Key.Equals("SNID")));
                    }
                    break;
                
                case "CL":
                    {
                        countryCodes.AddRange(CountryType.CL.HmsCountryCodes);
                        countryCodes.Add(CountryType.CL.Key);
                        if (countryCodes.Contains(CountryOfProcessing))
                        {
                            var tin = (from t in tins where t.IDType.Key == "CIID" select t).FirstOrDefault();
                            if (tin == null)
                            {
                                canPurchase = false;
                            }
                        }
                    }
                    break;
                //case "EC":
                //    {
                //        countryCodes.AddRange(CountryType.EC.HmsCountryCodes);
                //        countryCodes.Add(CountryType.EC.Key);
                //        if (countryCodes.Contains(CountryOfProcessing))
                //        {
                //            if (tins.Count > 0)
                //            {
                //                var tin = (from t in tins from r in (new[] { "ECID", "ECTX" }) where t.IDType.Key == r select t).ToList();
                //                if (tin == null || tin.Count == 0)
                //                {
                //                    canPurchase = false;
                //                }
                //            }
                //        }
                //    }
                //    break;
                case "VE":
                    {
                        countryCodes.AddRange(CountryType.VE.HmsCountryCodes);
                        countryCodes.Add(CountryType.VE.Key);
                        if (countryCodes.Contains(CountryOfProcessing))
                        {
                            canPurchase = false;
                            //VRIF TinCode required to purchase
                            foreach (TaxIdentification t in tins)
                            {
                                if (t.IDType.Key == "VRIF")
                                {
                                    canPurchase = true;
                                }
                            }
                        }
                        else if (CountryOfProcessing == "BR" || CountryOfProcessing == "CO")
                        {
                            //foreign DS, Not in whitelist, can't purchase
                            canPurchase = CatalogProvider.IsDistributorExempted(this.DistributorProfileModel.Id);
                        }
                    }
                    break;
                case "BE":
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType == PurchasingLimitRestrictionType.MarketingPlan)
                        {
                            countryCodes.AddRange(CountryType.BE.HmsCountryCodes);
                            countryCodes.Add(CountryType.BE.Key);
                            if (countryCodes.Contains(CountryOfProcessing))
                            {
                                if (tins.Count > 0)
                                {
                                    var beTins =
                                        (from t in tins
                                         from r in (new[] {"BEDI", "BENO", "BEVT"})
                                         where t.IDType.Key == r
                                         select t.IDType.Key).ToList();
                                    if (beTins.Contains("BEDI") && (beTins.Contains("BENO") || beTins.Contains("BEVT")))
                                    {
                                        canPurchase = false;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "KR":
                    {
                        // only KR distributor
                        if (!(new List<string>(CountryType.KR.HmsCountryCodes)).Union(new List<string>() { CountryType.KR.Key }).Contains(DistributorProfileModel.ProcessingCountryCode))
                        {
                            canPurchase = false;
                        }
                        else if (tins.Count > 0 && tins.Any(t => t.IDType.Key.Equals("KSID")))
                        {
                            var tin = tins.Find(t => t.IDType.Key.Equals("KSID"));
                            if (tin.IDType.ExpirationDate != null && tin.IDType.ExpirationDate < DateUtils.GetCurrentLocalTime("KR"))
                            {
                                canPurchase = false;
                            }
                        }
                    }
                    break;
                case "BR":
                    {
                        canPurchase = !(tins.Count == 0 || !tins.Any(t => t.IDType.Key.Equals("BRPF")));
                    }
                    break;
                case "PT":
                    {
                        canPurchase = !(tins.Count == 0 || !tins.Any(t => t.IDType.Key.Equals("POTX")));
                        if (!canPurchase)
                        {
                            //If they don't have the TinCode, they can only purchase if their mailing address is not in Portugal.
                            var mailingAddress = DistributorOrderingProfileProvider.GetAddress(ServiceProvider.OrderSvc.AddressType.Mailing, this.DistributorProfileModel.Id, CountyCode);
                            if (null != mailingAddress)
                            {
                                if (!string.IsNullOrEmpty(mailingAddress.Country) && mailingAddress.Country != "PT")
                                {
                                    canPurchase = true;
                                }
                            }
                        }
                    }
                    break;
                //case "TH":
                //    {
                //        countryCodes.AddRange(CountryType.TH.HmsCountryCodes);
                //        countryCodes.Add(CountryType.TH.Key);

                //        var tid = tins == null ? null : tins.Find(t => t.IDType.Key == "THID");

                //        if (countryCodes.Contains(CountryOfProcessing))
                //        {
                //            canPurchase = tid != null;
                //        }
                //        else
                //        {
                //            canPurchase = tins != null && tins.Find(p => p.ID == "TH00000000000") != null;
                //        }
                //    }
                //    break;
                case "VN":
                    {
                        canPurchase = false;
                        countryCodes.AddRange(CountryType.VN.HmsCountryCodes);
                        countryCodes.Add(CountryType.VN.Key);
                        if (countryCodes.Contains(CountryOfProcessing))
                        {
                            canPurchase = tins != null && tins.Find(p => p.IDType.Key == "VNID") != null;
                        }
                    }
                    break;
                case "MO":
                    {
                        countryCodes.AddRange(CountryType.MO.HmsCountryCodes);
                        countryCodes.Add(CountryType.MO.Key);
                        if (countryCodes.Contains(CountryOfProcessing))
                        {
                            canPurchase = tins != null && tins.Find(p => p.IDType.Key == "MCID") != null;
                        }
                    }
                    break;
            }

            return canPurchase;
        }
    }
}
