using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.es_CL
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            try
            {
                if (order != null)
                {
                    Message message = new Message();
                    message.MessageType = "Perception";
                    order.Messages = order.Messages == null ? new MessageCollection() : order.Messages;

                    List<TaxIdentification> tinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
                    var CIID = (tinList.Find(t => t.IDType.Key == "CIID") != null);
                    var CIEW = (tinList.Find(t => t.IDType.Key == "CIWE") != null);
                    if (CIID && !CIEW)
                    {
                        message.MessageValue = "Perception";
                        order.Messages.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CL Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }
    }
}