using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces
{
    public interface IValidateDistributorRule
    {
        List<string> PerformValidateDistributorRules(string DistributorID);
    }
}
