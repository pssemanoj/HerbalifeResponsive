// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyOrderProvider.cs" company="Herbalife">
//   Herbalife 2012
// </copyright>
// <summary>
//   Used to store copy order methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>
    /// Copy order class.
    /// </summary>
    public class CopyOrderProvider
    {
        /// <summary>
        /// Copy a shopping cart from a exisiting order.
        /// </summary>
        /// <param name="shoppingCartID">Exisisting shopping cart.</param>
        /// <param name="distributorID">Actual distributor ID.</param>
        /// <param name="locale">Actual Locale.</param>
        /// <param name="SkuitemList">List of SKU item</param>
        /// <returns>New Shopping cart id.</returns>
        public static MyHLShoppingCart CopyShoppingCart(int shoppingCartID, string distributorID, string locale, SKU_V01ItemCollection SkuitemList = null)
        {
            try
            {
                
                //var myHLShoppingCart = ShoppingCartProvider.createShoppingCart(distributorID, locale);
                // create a new cart in catalog service
                MyHLShoppingCart copiedCart = ShoppingCartProvider.GetShoppingCartForCopy(distributorID, shoppingCartID, locale, 0, SkuitemList);
                if (copiedCart != null && copiedCart.ShippingAddressID != 0)
                {
                    IShippingProvider shippingProvider = ShippingProvider.GetShippingProvider(null);
                    if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)              
                    {
                     
                        List<DeliveryOption> shippingAddresses = shippingProvider.GetShippingAddresses(distributorID, locale).Where(s => s.HasAddressRestriction== true)
                                         .ToList(); 
                        if(shippingAddresses.Count>0)
                        {
                            copiedCart.ShippingAddressID = shippingAddresses.FirstOrDefault().ID;
                        }

                    }
                   
                    if (shippingProvider != null)
                    {
                        List<DeliveryOption> shippingAddresses = shippingProvider.GetShippingAddresses(distributorID, locale);
                        if (shippingAddresses == null || shippingAddresses != null &&
                            shippingAddresses.Find(s => s.ID == copiedCart.ShippingAddressID) == null)
                        {
                            string cacheKey = new ShippingProviderBase().getSessionShippingAddressKey(distributorID, locale);
                            HttpContext.Current.Session.Remove(cacheKey);
                        }
                    }
                }
                return copiedCart;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CopyOrderError error: DS {0}, locale{1}, original {2}, error: {3}", distributorID, locale, shoppingCartID, ex.ToString()));
            }
            return null;
        }

        /// <summary>
        /// Gets a shopping cart from an exisiting invoice.
        /// </summary>
        /// <param name="shoppingCartID">Exisisting shopping cart.</param>
        /// <param name="distributorID">Actual distributor ID.</param>
        /// <param name="locale">Actual Locale.</param>
        /// <returns>New Shopping cart id.</returns>
        public static MyHLShoppingCart CopyShoppingCart(string distributorID, string locale, long invoiceID)
        {
            try
            {
                 return ShoppingCartProvider.GetShoppingCartFromInvoice(distributorID, locale, invoiceID);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CopyInvoiceError error: DS {0}, locale{1}, invoice id {2}, error: {3}", distributorID, locale, invoiceID, ex.ToString()));
            }
            return null;
        }

        public static MyHLShoppingCart CopyShoppingCartFromMemberInvoice(string distributorID, string locale, int invoiceID)
        {
            try
            {
                return ShoppingCartProvider.GetShoppingCartFromMemberInvoice(distributorID, locale, invoiceID);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CopyInvoiceError error: DS {0}, locale{1}, invoice id {2}, error: {3}", distributorID, locale, invoiceID, ex.ToString()));
            }
            return null;
        }
    }
}