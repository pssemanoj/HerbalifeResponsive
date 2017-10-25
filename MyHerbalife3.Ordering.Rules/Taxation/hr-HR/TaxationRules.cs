using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.hr_HR
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            try
            {
                List<TaxIdentification> tinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);

                order.Messages = new MessageCollection();
                TaxIdentification id = null;
                if (tinList == null || (tinList != null && (id=tinList.Find(t => t.IDType.Key.Equals("HRBL"))) != null)
                    || (tinList != null && (id=tinList.Find(t => t.IDType.Key.Equals("HRRP"))) != null))
                {
                    order.Messages = addTinMessage(order.Messages,id);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("HR Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }

        private MessageCollection addTinMessage(MessageCollection messageCollection, TaxIdentification id)
        {
            Message message = new Message();
            message.MessageType = "TinCode";
            message.MessageValue = id.IDType.Key;
            messageCollection.Add(message);
            return messageCollection;
        }

    }
}