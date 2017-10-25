using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.Test.Providers;
using MyHerbalife3.Ordering.Rules.PurchaseRestriction;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Test.Rules
{
    [TestClass]
    public class JPSKUPurchaseRestrictionUnitTest
    {
        [TestMethod]
        public void SKU0106_OverLimit_TestMethod1()
        {
            var cartItems = new ShoppingCartItemList();
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 1 });
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0106", Quantity = 6 }); //NFO

            var cartItemsBeingAdded = new ShoppingCartItemList();
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0106", Quantity = 4 });

            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("staff", "ja-JP", "XX", "XX",
                cartItems, OrderCategoryType.RSO);

            cart.ItemsBeingAdded = cartItemsBeingAdded;

            MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules rules = new MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules();
            rules.HandleCartItemsBeingAdded(cart, new ShoppingCartRuleResult(), MockPurchaseRestrictionInfo());
        }

        [TestMethod]
        public void SKU0106_OverLimit_TestMethod2()
        {
            var cartItems = new ShoppingCartItemList();

            var cartItemsBeingAdded = new ShoppingCartItemList();
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0106", Quantity = 20 });

            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("staff", "ja-JP", "30", "SAG",
                cartItems, OrderCategoryType.RSO);

            cart.ItemsBeingAdded = cartItemsBeingAdded;

            MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules rules = new MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules();
            rules.HandleCartItemsBeingAdded(cart, new ShoppingCartRuleResult(), MockPurchaseRestrictionInfo());
        }

        [TestMethod]
        public void ExceedGroupLimit_TestMethod()
        {
            var cartItems = new ShoppingCartItemList();
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 9 }); // FO
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0118", Quantity = 6 }); // FO

            var cartItemsBeingAdded = new ShoppingCartItemList();
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 1 });
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0118", Quantity = 89 });
            
            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("staff", "ja-JP", "30", "SAG",
                cartItems, OrderCategoryType.RSO);

            cart.ItemsBeingAdded = cartItemsBeingAdded;

            MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules rules = new MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules();
            rules.HandleCartItemsBeingAdded(cart, new ShoppingCartRuleResult(), MockPurchaseRestrictionInfo());
        }

        [TestMethod]
        public void ExceedGroupLimitAndIndivualLimit_TestMethod()
        {
            var cartItems = new ShoppingCartItemList();
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 9 });

            var cartItemsBeingAdded = new ShoppingCartItemList();
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 1 }); // FO
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0118", Quantity = 91 }); // FO
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "1158", Quantity = 11 }); // NFO

            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("staff", "ja-JP", "30", "SAG",
                cartItems, OrderCategoryType.RSO);

            cart.ItemsBeingAdded = cartItemsBeingAdded;

            MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules rules = new MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules();
            rules.HandleCartItemsBeingAdded(cart, new ShoppingCartRuleResult(), MockPurchaseRestrictionInfo());
        }

        public void ALlSKUGood_TestMethod()
        {
            var cartItems = new ShoppingCartItemList();
            cartItems.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 1 });

            var cartItemsBeingAdded = new ShoppingCartItemList();
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0120", Quantity = 1 }); // FO
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "0118", Quantity = 1 }); // FO
            cartItemsBeingAdded.Add(new ShoppingCartItem_V01 { SKU = "1158", Quantity = 1 }); // NFO

            MyHLShoppingCart cart =
                MyHLShoppingCartGenerator.GetBasicShoppingCart("staff", "ja-JP", "30", "SAG",
                cartItems, OrderCategoryType.RSO);

            cart.ItemsBeingAdded = cartItemsBeingAdded;

            MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules rules = new MyHerbalife3.Ordering.Rules.PurchaseRestriction.JP.PurchaseRestrictionRules();
            rules.HandleCartItemsBeingAdded(cart, new ShoppingCartRuleResult(), MockPurchaseRestrictionInfo());
        }

        private List<PurchaseRestrictionInfo> MockPurchaseRestrictionInfo()
        {
            var limits = new List<PurchaseRestrictionInfo>();
            var p1 = new PurchaseRestrictionInfo { Group = "FormulaOne", MaxQuantity = 100 };
            var skus = new List<PurchaseRestrictionSKUInfo>();
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "0120", QuantityAllow = 10 });
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "0118", QuantityAllow = 90 });
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "0139", QuantityAllow = 25 });
            p1.SKUInfoList = skus;
            limits.Add(p1);

            var p2 = new PurchaseRestrictionInfo { Group = "NonFormulaOne", MaxQuantity = 100 };
            skus = new List<PurchaseRestrictionSKUInfo>();
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "1158", QuantityAllow = 10 });
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "0242", QuantityAllow = 34 });
            skus.Add(new PurchaseRestrictionSKUInfo { SKU = "0106", QuantityAllow = 8 });
            p2.SKUInfoList = skus;
            limits.Add(p2);

            return limits;
        }
    }
}
