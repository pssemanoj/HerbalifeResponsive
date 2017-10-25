using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Rules.Taxation.es_CO
{
    public class TaxationRules : MyHerbalifeRule, ITaxationRule
    {
        public void PerformTaxationRules( Order_V01 order, string locale)
        {
            const string SanAndres = "SAN ANDRES";
            const string Amazonas = "AMAZONAS";
            string contributorClass = "NATIONAL";
  
            if (order.Messages == null)
                order.Messages = new MessageCollection();
            try
            {
                var legalAddress = DistributorOrderingProfileProvider.GetAddress(AddressType.PermanentLegal, order.DistributorID, Country);
                                        
                Address shippingAddress = (order.Shipment as ShippingInfo_V01).Address;
                MyHLShoppingCart cart = ShoppingCartProvider.GetShoppingCart(order.DistributorID, "es-CO");

                if (legalAddress != null && shippingAddress != null)
                {
                    string legalAddressState = legalAddress.StateProvinceTerritory.ToUpper();
                    string shippingAddressState = shippingAddress.StateProvinceTerritory.ToUpper();
                    if (legalAddressState == SanAndres || legalAddressState == Amazonas)
                    {
                        if (shippingAddressState == SanAndres || shippingAddressState == Amazonas)
                        {
                            contributorClass = "NATIONAL EXEMPT";
                        }
                    }
                }

                Message message = new Message();
                message.MessageType = "ContributorClass";
                message.MessageValue = contributorClass;
                order.Messages.Add(message);

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("CO Taxation Rules failed for Distributor {0}. Exception details \r\n{1}",
                                  order.DistributorID, ex.Message));
            }
        }
    }
}