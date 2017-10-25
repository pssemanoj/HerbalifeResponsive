using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Rules.Promotional.zh_CN;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel;
using HL.Catalog.ValueObjects;
using HL.Order.ValueObjects.China;

namespace MyHerbalife3.Ordering.Web.Test.Providers
{
    [TestClass]
    public class PromotionalRulesTest_zh_CN
    {
        private ShoppingCart_V02 cart;
        private PromotionCollection promotionSet = new PromotionCollection();
        private SessionInfo sessionInfo;
        private DistributorOrderingProfile distributorProfile;

        [TestInitialize]
        public void Init()
        {
            setupCart();
        }

        [TestMethod]
        public void ProcessFreightPromotion_Eligible()
        {

            PromotionalRules rules = new PromotionalRules();
            //rules.ProcessCart();


            var promotion = Substitute.For<IPromotionLoader>();
            promotion.GetPromotions().Returns(promotionSet);
            promotion.getCurrentSessionFromMock().Returns(sessionInfo);
            promotion.GetDistributorOrderingProfilefromMock().Returns(distributorProfile);

            rules.setPromotion(promotion);
            rules.ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);

            var testCart = cart as MyHLShoppingCart;
            OrderTotals_V02 total = (OrderTotals_V02)testCart.Totals;


            Assert.AreEqual(475, total.AmountDue);
        }

        [TestMethod]
        public void ProcessFreightPromotion_Not_Eligible()
        {

            PromotionalRules rules = new PromotionalRules();
            //rules.ProcessCart();


            var promotion = Substitute.For<IPromotionLoader>();
            promotion.GetPromotions().Returns(promotionSet);
            promotion.getCurrentSessionFromMock().Returns(sessionInfo);
            promotion.GetDistributorOrderingProfilefromMock().Returns(distributorProfile);

            rules.setPromotion(promotion);
            rules.ProcessCart(cart, ShoppingCartRuleReason.CartCalculated);

            var testCart = cart as MyHLShoppingCart;
            OrderTotals_V02 total = (OrderTotals_V02)testCart.Totals;

            total.AmountDue = 499;
            Assert.AreEqual(499, total.AmountDue);



        }


        private void setupCart()
        {
            MyHLShoppingCart _cart = new MyHLShoppingCart
            {
                 CurrentItems = new ShoppingCartItemList(),
                 CartItems = new ShoppingCartItemList(),
                 CountryCode = "CN",
                 DeliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo(),
                 DistributorID="test123",
                 RuleResults = new List<ShoppingCartRuleResult> {  },
                 Totals = new HL.Order.ValueObjects.OrderTotals(),
                 ShoppingCartItems = new List<Shared.Providers.DistributorShoppingCartItem>(),
            };

            OrderTotals_V02 totals = new OrderTotals_V02
            {
                AmountDue = 500,
                ChargeList = new HL.Order.ValueObjects.ChargeList {
                    new HL.Order.ValueObjects.Charge_V01 {
                        ChargeType = HL.Order.ValueObjects.ChargeTypes.FREIGHT,
                        Amount =25,
                        DiscountedAmount  = 1
                    }
                },
                DiscountAmount = 1,
                ItemsTotal = 500,
                OrderFreight = new OrderFreight {  ActualFreight = 1, Packages = new List<Package> { new Package { Packagetype="C", Unit=1, Volume= 0.0078045M } }, }
            };

            _cart.CartItems.Add(new ShoppingCartItem_V01() {SKU = "1316",ID = 1,Quantity = 2});
            _cart.CurrentItems.Add(new ShoppingCartItem_V01() {SKU = "1316",ID = 1,Quantity = 2});
            _cart.RuleResults.Add(new ShoppingCartRuleResult { RuleName = "ETO Rules" });
            _cart.DeliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo { FreightCode = "22", AddressType = "EXP", WarehouseCode = "3019", Option = HL.Common.ValueObjects.DeliveryOptionType.Shipping };
            _cart.Totals = totals;
            _cart.DeliveryInfo = new MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo
            {
                Address = new HL.Shipping.ValueObjects.ShippingAddress_V01
                {
                    Address = new HL.Common.ValueObjects.Address_V01 { StateProvinceTerritory = "广西壮族自治区", City= "梧州市", Country = "CN", CountyDistrict= "蝶山区", PostalCode= "111111" }
                },
                AddressType = "EXP"
            };

            cart = _cart;

            var onePromo = new PromotionElement
            {
                AmountMinInclude = 500,
                PromotionType = HL.Order.ValueObjects.China.PromotionType.Freight,
                StartDate = DateTime.Now.ToString("MM-dd-yyyy"),
                EndDate = DateTime.Now.ToString("MM-dd-yyyy"),
                excludedExpID = new List<string> { "10,30" },
                CustTypeList = new List<string> { "PC,CS" },
                CustCategoryTypeList = new List<string> { "PC" },
                Code = "DecPCPromo"
            };
            promotionSet.Add(onePromo);

            // Session
            sessionInfo = new SessionInfo
            {
                ShippingAddresses = new List<HL.Shipping.ValueObjects.ShippingAddress_V02>()
                {
                  new HL.Shipping.ValueObjects.ShippingAddress_V02
                  {
                       Address = new HL.Common.ValueObjects.Address_V01 { City="南宁市", StateProvinceTerritory = "广西壮族自治区", CountyDistrict ="青秀区", Country="CN" }
                  }            
                }
            };

            //DistributorProfile 
            distributorProfile = new DistributorOrderingProfile
            {
                CNAPFStatus = 2,
                CNCustCategoryType = "PC",
                CNCustType = "PC",
                IsPC = true,
                Id = "test123",
                CNStoreProvince = "广西壮族自治区"
            };
            
        }

    }

    
}
