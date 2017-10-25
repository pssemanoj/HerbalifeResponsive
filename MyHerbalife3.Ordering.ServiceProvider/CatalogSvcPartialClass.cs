using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MyHerbalife3.Ordering.ServiceProvider.CatalogSvc
{
    public partial class ShoppingCartItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem"/> class.
        /// </summary>
        public ShoppingCartItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public ShoppingCartItem(int id)
        {
            ID = id;
        }
    }

    public partial class ShoppingCartItem_V01 : ShoppingCartItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem_V01"/> class.
        /// </summary>
        public ShoppingCartItem_V01()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem_V01"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="sku">
        /// The sku.
        /// </param>
        /// <param name="quantity">
        /// The quantity.
        /// </param>
        /// <param name="updated">
        /// The updated.
        /// </param>
        public ShoppingCartItem_V01(int id, string sku, int quantity, DateTime updated)
            : base(id)
        {
            SKU = sku;
            Quantity = quantity;
            Updated = updated;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShoppingCartItem_V01"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="sku">
        /// The sku.
        /// </param>
        /// <param name="quantity">
        /// The quantity.
        /// </param>
        /// <param name="updated">
        /// The updated.
        /// </param>
        public ShoppingCartItem_V01(int id, string sku, int quantity, DateTime updated, int minQuantity)
            : base(id)
        {
            SKU = sku;
            Quantity = quantity;
            Updated = updated;
            MinQuantity = minQuantity;
        }
    }

    public partial class GetProductInfoRequest_V01
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProductInfoRequest_V01"/> class. 
        /// Request constructor
        /// </summary>
        /// <param name="platform">
        /// Platform name for the product info
        /// </param>
        /// <param name="locale">
        /// The ISO locale code for the info requested.
        /// </param>
        public GetProductInfoRequest_V01(string platform, string locale)
        {
            Platform = platform;
            Locale = locale;
        }
    }

    public partial class GetCatalogRequest_V01
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetCatalogRequest_V01"/> class. 
        /// The get catalog request_ v 01.
        /// </summary>
        /// <param name="countryCode">
        /// The ISO country code for the catalog requested.
        /// </param>
        public GetCatalogRequest_V01(string countryCode)
        {
            CountryCode = countryCode;
        }
    }

    public partial class GetSizeChartsRequest_V01
    {
        /// <summary>
        /// Initializes a new instance of <see cref="GetSizeChartsRequest_V01"/> class.
        /// </summary>
        public GetSizeChartsRequest_V01(string locale)
        {
            Locale = locale;
        }
    }

    public partial class SKU_V01ItemCollection
    {
        public SKU_V01ItemCollection() { }
        public SKU_V01ItemCollection(Dictionary<string, SKU_V01> skuCollection)
            : base(skuCollection) { }
    }

    public partial class ShoppingCartRuleResult
    {
        public void AddMessage(string message)
        {
            if (this.Messages == null)
                this.Messages = new List<string>();
            this.Messages.Add(message);
        }
    }
}
