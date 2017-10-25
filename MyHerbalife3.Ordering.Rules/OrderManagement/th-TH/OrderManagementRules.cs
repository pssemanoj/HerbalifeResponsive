using System;

namespace MyHerbalife3.Ordering.Rules.OrderManagement.th_TH
{
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
    using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

    public class OrderManagementRules : MyHerbalifeRule, IOrderManagementRule
    {
        public void PerformOrderManagementRules(ShoppingCart_V02 cart, Order_V01 order, string locale, OrderManagementRuleReason reason)
        {
            if (System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar != null)
            {
                //System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();

                if (order.OrderMonth != null)
                {
                    var oM = order.OrderMonth;
                    var gregorianYear = DateTime.Now.Year.ToString().Substring(2, 2);
                    //If not contain the GregorianYear
                    if (oM.Contains(gregorianYear))
                    {
                        oM = gregorianYear + oM.Substring(2);
                        order.OrderMonth = oM;
                    }
                }
            }
        }
    }
}
