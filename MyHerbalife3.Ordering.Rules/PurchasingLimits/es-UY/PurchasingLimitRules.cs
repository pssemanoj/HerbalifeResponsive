using MyHerbalife3.Ordering.Providers;

namespace MyHerbalife3.Ordering.Rules.PurchasingLimits.es_UY
{
    public class PurchasingLimitRules : PurchasingLimitRuleBase
    {
        #region Public Methods and Operators

        public override bool DistributorIsExemptFromPurchasingLimits(string distributorId)
        {
            bool hasUytxTinCode = false;
            var tins = DistributorOrderingProfileProvider.GetTinList(distributorId, true);
            if (tins != null)
            {
                var uytx = tins.Find(p => p.IDType.Key == "UYTX");
                if (uytx != null)
                {
                    hasUytxTinCode = true;
                }
            }

            return hasUytxTinCode;
        }

        #endregion
    }
}