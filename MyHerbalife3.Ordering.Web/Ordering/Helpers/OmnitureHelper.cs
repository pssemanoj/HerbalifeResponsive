using HL.Common.Configuration;
using HL.Common.Cryptography;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Shared.Analytics.Providers;
using MyHerbalife3.Shared.Analytics.ValueObjects;
using MyHerbalife3.Shared.Infrastructure.Helpers;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.UI.Extensions;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Web.Ordering.Helpers
{
    public class OmnitureHelper
    {
        private static readonly string EncriptionKey = Settings.GetRequiredAppSetting("EncriptionKey", "");
        private const string CartItemListScriptName = "AnalyticsCartItems";
        private const string CommonItemsScriptName = "AnalyticsCommonItems";
        private const string ProductDetailScriptName = "AnalyticsProductDetails";
        private const string AddedCartItemScriptName = "AnalyticsAddedCartItems";

        public static void RegisterOmnitureAnalyticsScript(Page currentPage, IGlobalContext globalContext)
        {
            try
            {
                if (globalContext == null || globalContext.CultureConfiguration == null)
                {
                    throw new ArgumentException("Global Context is null"); // nameof has been removed to fix mobile build
                }
                if (globalContext.CultureConfiguration == null)
                {
                    throw new ArgumentException("Culture Configuration is null"); // nameof has been removed to fix mobile build
                }

                string browseScheme = globalContext.CurrentExperience.BrowseScheme.ToString();
                AppendCommonItems(currentPage.Title, null, currentPage.Request, currentPage.User.Identity, globalContext.CurrentDistributor, browseScheme);
                var writer = new StringWriter();
                writer.WriteLine(string.Format("<!-- {0} -->", CommonItemsScriptName));
                AnalyticsProvider.Render(writer);
                currentPage.ClientScript.RegisterClientScriptBlock(currentPage.GetType(), CommonItemsScriptName, writer.GetStringBuilder().ToString(), false);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error setting Omniture Analytics variables. ");
            }
        }

        public static void AppendCommonItems(string pageTitle, string omnitureEventName, HttpRequest request, IIdentity identity, DistributorProfileModel distributorProfileModel, string browseScheme)
        {
            var isAuthenticated = identity.IsAuthenticated;
            var distributorId = isAuthenticated ? identity.Name : null;
            AnalyticsProvider.Set(AnalyticsFact.Id, distributorId);
            AnalyticsProvider.Set(AnalyticsFact.EncId, Encryption.Encrypt(distributorId, EncriptionKey));
            if (!string.IsNullOrEmpty(omnitureEventName))
            {
                AnalyticsProvider.Set(AnalyticsFact.OmnitureEventName, omnitureEventName);
            }
            AnalyticsProvider.Set(AnalyticsFact.IsLoggedIn, isAuthenticated);
            if (isAuthenticated)
            {
                var roles = Roles.GetRolesForUser();
                AnalyticsProvider.Set(AnalyticsFact.Roles, roles);
                var profile = distributorProfileModel;
                if (profile != null)
                {
                    AnalyticsProvider.Set(AnalyticsFact.IsPresidentsTeam, profile.IsPresidentTeam);
                    AnalyticsProvider.Set(AnalyticsFact.IsChairmanClub, profile.IsChairmanClubMember);
                    AnalyticsProvider.Set(AnalyticsFact.IsTabTeam, !string.IsNullOrWhiteSpace(profile.TabTeamType));
                    AnalyticsProvider.Set(AnalyticsFact.SubtypeCode, profile.SubTypeCode);
                    AnalyticsProvider.Set(AnalyticsFact.EncSubtypeCode, Encryption.Encrypt(profile.SubTypeCode, EncriptionKey));
                    AnalyticsProvider.Set(AnalyticsFact.ProcessingCountryCode, profile.ProcessingCountryCode);
                    AnalyticsProvider.Set(AnalyticsFact.Scheme, profile.Scheme);
                }
            }
            AnalyticsProvider.Set(AnalyticsFact.BrowseScheme, browseScheme);
            var dsSiteMap = new SiteMapDataSource();
            SiteMapHelper.SetCombinedSitemap(dsSiteMap);
            var currentNode = dsSiteMap.Provider.CurrentNode;
            AnalyticsProvider.Set(AnalyticsFact.Title, currentNode == null ? pageTitle : currentNode.Title);
            string searchKeyword = request.QueryString[RequestExtension.SEARCH_TERMS_KEY] ?? "";
            AnalyticsProvider.Set(AnalyticsFact.SearchTerms, searchKeyword.ToLower());
        }


        /// <summary>
        ///     Registers script emmitting newly added cart items
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="cartItems"></param>
        /// <param name="addedItems"></param>
        public static void RegisterOmnitureAddCartScript(Page currentPage, List<DistributorShoppingCartItem> cartItems, List<ShoppingCartItem_V01> addedItems)
        {
            try
            {
                if (addedItems == null || cartItems == null)
                {
                    return;
                }

                var items = new List<object>();
                foreach (var addedItem in addedItems.Join(
                        cartItems,
                        sci => sci.SKU,
                        dsci => dsci.SKU,
                        (s1, s2) => s1))
                {
                    var analyticsCartItem = CreateAnalyticsCartItem(addedItem.SKU, addedItem.Quantity, 0, null, 0, 0);
                    items.Add(analyticsCartItem);
                }

                AnalyticsProvider.Set(AnalyticsFact.AddedToCart, items);
                var writer = new StringWriter();
                writer.WriteLine(string.Format("<!-- {0} -->", AddedCartItemScriptName));
                AnalyticsProvider.Render(writer);
                currentPage.ClientScript.RegisterStartupScript(currentPage.GetType(), AddedCartItemScriptName, writer.GetStringBuilder().ToString());
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error setting Omniture Analytics variables Added To Cart:");
            }
        }

        public static void RegisterOmnitureCartScript(Page currentPage, OrderTotals_V01 totals, List<DistributorShoppingCartItem> shoppingCartItems, string eventState)
        {
            try
            {
                AppendCartItems(totals, shoppingCartItems, eventState);
                var writer = new StringWriter();
                writer.WriteLine(string.Format("<!-- {0} -->", CartItemListScriptName));
                AnalyticsProvider.Render(writer);
                currentPage.ClientScript.RegisterClientScriptBlock(currentPage.GetType(), CartItemListScriptName, writer.GetStringBuilder().ToString(), false);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error setting Omniture Analytics variables:");
            }
        }

        /// <summary>
        ///     Registers all client side script necessary for Omniture Analytics
        ///     tracking of the page.
        /// </summary>
        /// <param name="currentPage">
        ///     A reference to the page on which
        ///     the script should be included.
        /// </param>
        /// <param name="product"></param>
        /// <param name="skus"></param>
        /// <param name="category"></param>
        /// <param name="localizationProvider"></param>
        public static void RegisterOmnitureProductsScript(Page currentPage, string product, List<SKU_V01> skus, string category)
        {
            try
            {
                var productDetail = new
                {
                    Name = EnsureString(product),
                    Skus = skus.Select(s => EnsureString(s.SKU)),
                    Category = category,
                };

                AnalyticsProvider.Set(AnalyticsFact.ProductDetail, productDetail);
                var writer = new StringWriter();
                writer.WriteLine(string.Format("<!-- {0} -->", ProductDetailScriptName));
                AnalyticsProvider.Render(writer);
                currentPage.ClientScript.RegisterClientScriptBlock(currentPage.GetType(), ProductDetailScriptName, writer.GetStringBuilder().ToString(), false);
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Error setting Omniture Analytics variables: Product Details");
            }
        }

        internal static void AppendCartItems(OrderTotals_V01 totals, List<DistributorShoppingCartItem> cartItems, string eventState)
        {
            if (totals == null || totals.ItemTotalsList == null)
            {
                return;
            }

            var items = new List<object>();
            foreach (var lineTotal in totals.ItemTotalsList.Cast<ItemTotal_V01>())
            {
                var cartItem = cartItems.FirstOrDefault(n => n.SKU == lineTotal.SKU);
                if (cartItem == null)
                {
                    continue;
                }

                var fee = FindCharge("FREIGHT", lineTotal.ChargeList);
                var freight = fee == null ? 0m : fee.Amount;
                var analyticsItem = CreateAnalyticsCartItem(lineTotal.SKU, lineTotal.Quantity, lineTotal.DiscountedPrice, cartItem.Description, lineTotal.LineTax, freight);
                items.Add(analyticsItem);
            }
            AnalyticsProvider.Set(AnalyticsFact.PricedCart, items);
            string orderId = eventState.Replace("purchase|", "");
            AnalyticsProvider.Set(AnalyticsFact.OrderId, orderId);
        }

        internal static dynamic CreateAnalyticsCartItem(string sku, int quantity, decimal discountedPrice, string description, decimal lineTax, decimal freight)
        {
            var result = new
            {
                Sku = sku,
                Quantity = quantity,
                DiscountedPrice = discountedPrice,
                Description = description,
                ItemTax = lineTax,
                Freight = freight,
            };

            return result;
        }

        /// <summary>
        ///     1) encode on server-side and decode on client side. Pluse do any other common transfromations such as tolower.
        ///     2) on clinet use decodeURIComponent which supersedes the deprecated escaep
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        private static string EnsureString(string txt)
        {
            return Regex.Replace(txt, "[^\\w ]", "", RegexOptions.Compiled);
        }

        private static Charge_V01 FindCharge(string type, IEnumerable<Charge> chargeList)
        {
            return chargeList == null
                       ? null
                       : chargeList.Cast<Charge_V01>().FirstOrDefault(charge => charge.Type == type);
        }
    }
}

