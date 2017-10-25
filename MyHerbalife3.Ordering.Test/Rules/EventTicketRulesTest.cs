

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyHerbalife3.Ordering.Test.Rules
{
    using MyHerbalife3.Ordering.Providers.RulesManagement;
    using MyHerbalife3.Ordering.Test.Helpers;
    using MyHerbalife3.Shared.Providers;
    using System.Collections.Generic;
    using System.Web;
    using HL.Common.ValueObjects;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    /// <summary>
    ///This is a test class for EventTicketRulesTest and is intended
    ///to contain all EventTicketRulesTest Unit Tests
    ///</summary>
    [TestClass]
    public class EventTicketRulesTest
    {

        #region Public Methods and Operators

        [TestMethod]

        public void ProcessCartTest()
        {
            const string DsId = "090245813";
            const string Local = "en-US";
            var testSettings = new OrderingTestSettings(Local, DsId);
            var target = new Ordering.Rules.EventTicket.Global.EventTicketRules();
            var distributor =
                          OnlineDistributorHelper.GetOnlineDistributor(testSettings.Distributor);
            HttpRuntime.Cache.Insert("DISTR_" + testSettings.Distributor, distributor);
            var cart = MyHLShoppingCartGenerator.GetBasicShoppingCart(
               DsId, Local, "", "", false, null, new List<DistributorShoppingCartItem>
                {
                    ShoppingCartItemHelper.GetCatalogItems(1, 1, "D077", Local)
                    
                }, OrderCategoryType.RSO);

            cart.ShoppingCartID = 1;
            var result = target.ProcessCart(cart, ShoppingCartRuleReason.CartItemsBeingAdded);
            if (result.Count > 0 && result[0].Messages.Count > 0)
            {
                Assert.AreEqual((string) result[0].Messages[0],
                 "The sku D077 is not valid.");

            }
            else
                Assert.Fail("Distributor can not add the Sku D077 when oreder catagory is Non Event Ticket Ordering");

        }

        #endregion

    }
}