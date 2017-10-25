using System;
using System.Collections.Generic;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.Taxation.fr_FR
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules(Order_V01 order, string locale)
        {
            try
            {
                //Social Security Fee only applies to those Distributors without RCS TIN code in HMS 
                //AND postcode range between 01000 UP TO 974XX AND with HMS Country of Mailing = FR - France OR = FG - FRENCH GUIANA OR = RE - REUNION OR = MB - MARTINIQUE OR = GP - GUADELOUPE 

                List<TaxIdentification> tinList = DistributorOrderingProfileProvider.GetTinList(order.DistributorID, true);

                order.Messages = new MessageCollection();
                // non-RCS, no charge
                if (tinList == null || (tinList != null && tinList.Find(t => t.IDType.Key.Equals("RCS")) == null))
                {
                    ShippingInfo_V01 shippingInfo = order.Shipment as ShippingInfo_V01;
                    if (shippingInfo != null && shippingInfo.Address != null)
                    {
                        int postCode = 0;
                        if (Int32.TryParse(shippingInfo.Address.PostalCode, out postCode))
                        {
                            // no charge
                            if (postCode >= 1000 && postCode <= 97499)
                            {
                                if (validateCountryOfResidence(DistributorProfileModel.ResidenceCountry))
                                {
                                    order.Messages = addTinMessage(order.Messages);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("FR Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }

        private MessageCollection addTinMessage(MessageCollection messageCollection)
        {
            Message message = new Message();
            message.MessageType = "TinCode";
            message.MessageValue = "ApplySS";
            //if (messageCollection == null)
            //  messageCollection = new MessageCollection();
            messageCollection.Add(message);
            return messageCollection;
        }

        private bool validateCountryOfResidence(string countryCode)
        {
            if (countryCode == "FR" || countryCode == "GF" || countryCode == "RE"
                || countryCode == "MQ" || countryCode == "GP")
            {
                return true;
            }
            return false;
        }
    }
}