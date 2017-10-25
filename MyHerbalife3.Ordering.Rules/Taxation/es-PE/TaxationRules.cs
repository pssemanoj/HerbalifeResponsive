using System;
using System.Collections.Generic;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.es_PE
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {

        public bool isPEDs
        {
            get { return DistributorProfileModel.ProcessingCountryCode == "PE"; }
        }

        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            try
            {
                Message message = new Message();
                message.MessageType = "OrderType";
                message.MessageValue = "RSO";

                Message messageBoleta = new Message();
                messageBoleta.MessageType = "TypePerceptions";
                messageBoleta.MessageValue = "Boleta";

                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType))
                {
                    var sessionInfo = SessionInfo.GetSessionInfo(order.DistributorID, locale);
                    if (null != sessionInfo)
                    {
                        message.MessageValue = sessionInfo.IsEventTicketMode ? "ETO" : "RSO";
                    }
                    if ("RSO" == message.MessageValue)
                    {
                        List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);
                        TaxIdentification tid = null;
                        var isPeid= (tid = tins.Find(t => t.IDType.Key == "PEID"))!=null;
                        var isPetx= (tid = tins.Find(t => t.IDType.Key == "PETX"))!=null;
                        if ((isPEDs && isPetx && isPeid) || (isPEDs && isPetx))
                        {
                            messageBoleta.MessageValue = "Factura";
                        }
                    }
                }

                if (order.Messages == null)
                    order.Messages = new MessageCollection();
                order.Messages.Add(message);
                bool enable = Settings.GetRequiredAppSetting<bool>("PEperceptionsEnable", true);
                if(enable)
                order.Messages.Add(messageBoleta);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("PE Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }
    }
}