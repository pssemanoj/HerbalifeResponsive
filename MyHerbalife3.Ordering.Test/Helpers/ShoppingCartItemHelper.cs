// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShoppingCartItemHelper.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Helper class for shopping cart items.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using HL.Catalog.ValueObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Shared.Providers;

namespace MyHerbalife3.Ordering.Test.Helpers
{
    /// <summary>
    ///     Helper class for shopping cart items.
    /// </summary>
    internal static class ShoppingCartItemHelper
    {
        /// <summary>
        ///     Gets a cart item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="qty">The quantity.</param>
        /// <param name="sku">The SKU.</param>
        /// <returns>Shopping cart item object.</returns>
        public static ShoppingCartItem_V01 GetCartItem(int id, int qty, string sku)
        {
            return GetCartItem(id, 1, false, qty, sku, DateTime.Now);
        }

        /// <summary>
        ///     Gets a cart item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="minQty">The minimum quantity.</param>
        /// <param name="partialBackordered">
        ///     if set to <c>true</c> [partial backordered].
        /// </param>
        /// <param name="qty">The quantity.</param>
        /// <param name="sku">The SKU.</param>
        /// <param name="updated">The updated.</param>
        /// <returns>Shopping cart item object.</returns>
        public static ShoppingCartItem_V01 GetCartItem(int id, int minQty, bool partialBackordered, int qty, string sku,
                                                       DateTime updated)
        {
            return new ShoppingCartItem_V01
            {
                ID = id,
                MinQuantity = minQty,
                PartialBackordered = partialBackordered,
                Quantity = qty,
                SKU = sku,
                Updated = updated
            };
        }

        /// <summary>
        ///     Gets a cart item.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="qty">The quantity.</param>
        /// <param name="sku">The SKU.</param>
        /// <returns>Shopping cart item object.</returns>
        public static DistributorShoppingCartItem GetShoppingCartItem(int id, int qty, string sku)
        {
            return new DistributorShoppingCartItem
            {
                ID = id,
                MinQuantity = 1,
                PartialBackordered = false,
                Quantity = qty,
                SKU = sku,
                Updated = DateTime.Now,
            };
        }

        public static DistributorShoppingCartItem GetCatalogItems(int id, int qty, string sku, string countrylocal)
        {
            CatalogItem_V01 catalog = CatalogProvider.GetCatalogItem(sku, countrylocal.Substring(3));
            if (catalog == null)
            {
                return new DistributorShoppingCartItem
                {
                    ID = id,
                    MinQuantity = 1,
                    PartialBackordered = false,
                    Quantity = qty,
                    SKU = sku,
                    Updated = DateTime.Now,
                };
            }
            else
            {
                return new DistributorShoppingCartItem
                {
                    ID = id,
                    MinQuantity = 1,
                    PartialBackordered = false,
                    Quantity = qty,
                    SKU = sku,
                    Updated = DateTime.Now,
                    CatalogItem = catalog,
                    Description = catalog.Description

                };
            }
        }

        /// <summary>
        /// Creates a list up to 12 DistributorShoppingCartItems with the parameters given
        /// </summary>
        /// <param name="locale">Locale</param>
        /// <param name="skus">Array of skus to be taken</param>
        /// <param name="lines">Number of lines to be added</param>
        /// <param name="qty">Quantity for each line</param>
        /// <returns>List of DistributorShoppingCartItem</returns>
        public static List<DistributorShoppingCartItem> GetDistributorShoppingCartItemList(string locale,
                                                                                           List<string> skus, int lines,
                                                                                           int qty)
        {
            var distributorShoppingCartItemList = new List<DistributorShoppingCartItem>();
            var linesToAdd = (skus.Count < lines) ? skus.Count : lines;
            for (int i = 0; i < linesToAdd; i++)
            {
                distributorShoppingCartItemList.Add(ShoppingCartItemHelper.GetCatalogItems(1, qty, skus[i], locale));
            }
            return distributorShoppingCartItemList;
        }
    }
}
