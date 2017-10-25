using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Providers.FOP
{
    public class PurchaseRestrictionProvider
    {
        public static List<string> OrderThresholdCountries = DistributorOrderingProfileProvider.MpeThresholdCountries;
        public static List<string> NonFOPCountries = new List<string>(Settings.GetRequiredAppSetting("NonFOPCountries").Split(new char[] { ',' }).Union(OrderThresholdCountries));

        public static IPurchaseRestrictionManager PurchaseRestrictionManager(string id)
        {
            IPurchaseRestrictionManagerFactory purchaseRestrictionManagerFactory = new PurchaseRestrictionManagerFactory();
            return purchaseRestrictionManagerFactory.GetPurchaseRestrictionManager(id);
        }

        public static bool HasPurchaseRestriction(string distributorId, string COP, string level)
        {
            if (HLConfigManager.Configurations.DOConfiguration.GetPurchaseLimitsFromFusion) return true; // this applies to purchasing limits
            if (HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionPeriod == PurchasingLimitRestrictionPeriod.PerOrder)
                return true;
            if (level == "DS" )
            {
                if (OrderThresholdCountries.Contains(COP) || !NonFOPCountries.Contains(COP))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// called from UI
        /// </summary>
        /// <param name="distributorID"></param>
        /// <param name="TIN"></param>
        /// <returns></returns>
        public static PurchasingLimits_V01 GetPurchasingLimits(int orderMonth, string distributorID, string orderSubType)
        {
            try
            {
                IPurchaseRestrictionManager  IPurchaseRestrictionManager = PurchaseRestrictionManager(distributorID);
                HLRulesManager.Manager.SetOrderSubType(orderSubType, distributorID);

                var purchasingLimits = IPurchaseRestrictionManager.GetPurchasingLimits(orderMonth);
                return purchasingLimits != null ? purchasingLimits : new PurchasingLimits_V01();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Error GetPurchasingLimits for distributor: {0}\r\n{1}", distributorID,
                                  ex.Message));
            }
            return null;
        }

        public static void ReconcileAfterPurchase(MyHLShoppingCart shoppingCart, string distributorId, string countryCode)
        {
            try
            {
                IPurchaseRestrictionManager IPurchaseRestrictionManager = PurchaseRestrictionManager(distributorId);
                IPurchaseRestrictionManager.ReconcileAfterPurchase(shoppingCart, distributorId, countryCode);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Error ReconcileAfterPurchase for distributor: {0}\r\n{1}", distributorId,
                                  ex.Message));
            }
        }
        /// <summary>
        /// setValues
        /// </summary>
        /// <param name="l"></param>
        /// <param name="period"></param>
        /// <param name="maxValue"></param>
        public static void SetValues(PurchasingLimits_V01 l, PurchasingLimitRestrictionPeriod period, PurchaseLimitType limitType)
        {
            l.RestrictionPeriod = period;
            l.PurchaseLimitType = limitType;
        }


        public static bool IsBlackoutPeriod(string countryCode)
        {
            //if (countryCode == "IT")
            //{
            //    //After 5:30 pm and before 9:00AM M - F and all weekend
            //    var now = HL.Common.Utilities.DateUtils.GetCurrentLocalTime(countryCode);
            //    var cutoffStart = new DateTime(now.Year, now.Month, now.Day, 17, 29, 59);
            //    var cutoffEnd = new DateTime(now.Year, now.Month, now.Day, 8, 59, 59);
            //    if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            //    {
            //        return true;
            //    }
            //    if (now > cutoffStart || now < cutoffEnd)
            //    {
            //        return true;
            //    }
            //}
            return false;
        }


         public static int GetThresholdPeriod(string COP)
        {
            var configSet = HLConfigManager.GetConfigurationByCountry(COP);
            if ( configSet != null )
            {
                return configSet.DOConfiguration.ThresholdPeriod;
            }
            return 10;
        }

         public static decimal GetVolumeLimitsAfterFirstOrderFOP(string COP)
        {
            var configSet = HLConfigManager.GetConfigurationByCountry(COP);
            if (configSet != null)
            {
                return configSet.DOConfiguration.VolumeLimitsAfterFirstOrderFOP;
            }
            return 3999.99M;
        }

        public static bool RequireTraining(string distributorID, string locale, string countryCode)
        {
            var session = SessionInfo.GetSessionInfo(distributorID, locale);
            string trainingCode = HLConfigManager.Configurations.ShoppingCartConfiguration.TrainingCode;
            if (session.DsTrainings == null)
                session.DsTrainings = DistributorOrderingProfileProvider.GetTrainingList(distributorID, countryCode);

            if (session.DsTrainings != null && session.DsTrainings.Count > 0 && session.DsTrainings.Exists(t => t.TrainingCode == trainingCode && !t.TrainingFlag))
            {
                return true;
            }
            return false;
        }

    }
}
