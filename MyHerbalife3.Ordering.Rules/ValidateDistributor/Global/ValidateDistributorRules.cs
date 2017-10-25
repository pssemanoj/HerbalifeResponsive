using System.Collections.Generic;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;

namespace MyHerbalife3.Ordering.Rules.ValidateDistributor.Global
{
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
    using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

    public class ValidateDistributorRules : MyHerbalifeRule, IValidateDistributorRule
    {
        public List<string> PerformValidateDistributorRules(string DistributorID)
        {
            List<string> errors = null;
            //OnlineDistributor ods =  DistributorProvider.GetDistributor(DistributorID);
            DistributorOrderingProfile ods = DistributorOrderingProfileProvider.GetProfile(DistributorID, Country);
            if (ods != null)
            {
                if (ods.CantBuy)
                {
                    return errors = new List<string> { string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "CantBuy").ToString()) };
                }
            }
            return errors;
        }
    }
}
