﻿using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement.RuleInterfaces;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Rules.PurchasingPermissions.ro_RO
{
    public class PurchasingPermissionRules : MyHerbalifeRule, IPurchasingPermissionRule, IShoppingCartRule
    {
        private const decimal MaxVolPoints = 500.00m;

        public bool CanPurchase(string distributorID, string countryCode)
        {
            return true;
        }

        public List<ShoppingCartRuleResult> ProcessCart(ShoppingCart_V02 cart, ShoppingCartRuleReason reason)
        {
            List<ShoppingCartRuleResult> result = new List<ShoppingCartRuleResult>();
            ShoppingCartRuleResult defaultResult = new ShoppingCartRuleResult();
            defaultResult.RuleName = "PurchasingPermission Rules";
            defaultResult.Result = RulesResult.Unknown;
            result.Add(PerformRules(cart, reason, defaultResult));
            return result;
        }

        private ShoppingCartRuleResult PerformRules(ShoppingCart_V02 cart,
                                                    ShoppingCartRuleReason reason,
                                                    ShoppingCartRuleResult Result)
        {
            if (reason == ShoppingCartRuleReason.CartItemsBeingAdded)
            {
                if (HasActiveAppTinCode(cart.DistributorID) && IsShippingWithInRomania(cart))
                {
                    try
                    {
                        var cartVolume = 0m;
                        var currentItem = CatalogProvider.GetCatalogItem(cart.CurrentItems[0].SKU, Country);
                        var myCart = cart as MyHLShoppingCart;
                        if (myCart != null && !string.IsNullOrEmpty(myCart.VolumeInCart.ToString()))
                        {
                            cartVolume = myCart.VolumeInCart;
                        }
                        var newVolumePoints = currentItem.VolumePoints*cart.CurrentItems[0].Quantity;

                        if (cartVolume + newVolumePoints > MaxVolPoints)
                        {
                            Result.AddMessage(
                                string.Format(
                                    HttpContext.GetGlobalResourceObject(
                                        string.Format("{0}_Rules", HLConfigManager.Platform), "VolumePointExceeds")
                                               .ToString(), cart.CurrentItems[0].SKU));
                            Result.Result = RulesResult.Failure;
                            cart.RuleResults.Add(Result);
                            return Result;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(
                            string.Format(
                                "Error while performing Add to Cart Rule for Venezuala distributor: {0}, Cart Id:{1}, \r\n{2}",
                                cart.DistributorID, cart.ShoppingCartID, ex.ToString()));
                    }
                }
            }
            

            return Result;
        }

        private bool HasActiveAppTinCode(string distributorID)
        {
            bool hasActiveAppTinCode = false;
            List<TaxIdentification> tins = DistributorOrderingProfileProvider.GetTinList(distributorID, true); 
            
            var now = DateUtils.GetCurrentLocalTime("RO");
            foreach (TaxIdentification taxId in tins)
            {
                if (taxId.IDType != null && (taxId.IDType.Key == "APP" && taxId.IDType.ExpirationDate > now))
                {
                    hasActiveAppTinCode = true;
                    break;
                }
            }


            return hasActiveAppTinCode;
        }

        private bool IsShippingWithInRomania(ShoppingCart_V02 cart)
        {
            bool isShippedWithInRomania = false;
            var myCart = cart as MyHLShoppingCart;
            if (myCart.DeliveryInfo.Address.Address.Country == "RO")
            {
                isShippedWithInRomania = true;
            }
            return isShippedWithInRomania;
        }
    }
}