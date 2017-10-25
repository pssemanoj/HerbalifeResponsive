// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PriceListItemView.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//   Used to pass a hl price list item from server to client.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Providers.ResultViews
{
    using System.Xml.Serialization;

    /// <summary>
    /// Used to pass a hl price list item from server to client.
    /// </summary>
    public class PriceListItemView
    {
        /// <summary>
        /// Gets or sets the SKU.
        /// </summary>
        /// <value>
        /// The SKU.
        /// </value>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the subcategory.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public string SubCategory { get; set; }

        /// <summary>
        /// Gets or sets the volume points.
        /// </summary>
        /// <value>
        /// The volume points.
        /// </value>
        public decimal VolumePoints { get; set; }

        /// <summary>
        /// Gets or sets the earnbase.
        /// </summary>
        /// <value>
        /// The earnbase.
        /// </value>
        public string EarnBase { get; set; }

        /// <summary>
        /// Gets or sets the retail price.
        /// </summary>
        /// <value>
        /// The retail price.
        /// </value>
        public string RetailPrice { get; set; }

        /// <summary>
        /// Gets or sets the customer retail price.
        /// </summary>
        /// <value>
        /// The customer retail price.
        /// </value>
        public string CustomerRetailPrice { get; set; }

        /// <summary>
        /// Gets or sets the distributor cost.
        /// </summary>
        /// <value>
        /// The distributor cost.
        /// </value>
        public string DistributorDiscountCost { get; set; }

        /// <summary>
        /// Gets or sets the distributor loaded cost.
        /// </summary>
        /// <value>
        /// The distributor loaded cost.
        /// </value>
        public string DistributorLoadedCost { get; set; }

        /// <summary>
        /// Gets or sets the customer price.
        /// </summary>
        /// <value>
        /// The customer price.
        /// </value>
        public string CustomerPrice { get; set; }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the short description.
        /// </summary>
        /// <value>
        /// The short description.
        /// </value>
        public string ShortDescription { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tax exempt.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tax exempt; otherwise, <c>false</c>.
        /// </value>
        public bool IsTaxExempt { set; get; }

        /// <summary>
        /// Gets or sets tha tax rate.
        /// </summary>
        public string TaxRate { get; set; }

        /// <summary>
        /// Gets or sets the earnbase.
        /// </summary>
        /// <value>
        /// The earnbase.
        /// </value>
        private readonly decimal _earnBase;

        /// <summary>
        /// Gets or sets the retail price.
        /// </summary>
        /// <value>
        /// The retail price.
        /// </value>
        private readonly decimal _retailPrice;

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceListItemView"/> class.
        /// </summary>
        public PriceListItemView()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceListItemView" /> class.
        /// </summary>
        /// <param name="earnBase">The earn base.</param>
        /// <param name="retailPrice">The retail price.</param>
        public PriceListItemView(decimal earnBase, decimal retailPrice)
        {
            this._earnBase = earnBase;
            this._retailPrice = retailPrice;
        }

        /// <summary>
        /// Calculate distributor cost.
        /// </summary>
        /// <param name="distributorCost">The distributor cost.</param>
        /// <param name="salesTax">The sales tax.</param>
        /// <param name="shippingAndHandling">The shipping and handling.</param>
        public decimal GetDistributorLoadedCost(decimal distributorCost, decimal salesTax, decimal shippingAndHandling)
        {
            var val = this._retailPrice - (this._earnBase*distributorCost);

            if (this.IsTaxExempt)
            {
                salesTax = 0M;
            }

            val += ((salesTax + shippingAndHandling)*this._retailPrice);

            return val;
        }

        /// <summary>
        /// Calculate customer price.
        /// </summary>
        /// <param name="customerDiscount">The customer discount.</param>
        /// <param name="salesTax">The sales tax.</param>
        /// <param name="shippingAndHandling">The shipping and handling.</param>
        public decimal GetCustomerPrice(decimal customerDiscount, decimal salesTax, decimal shippingAndHandling)
        {
            var val = this._retailPrice - (this._retailPrice*customerDiscount);

            if (this.IsTaxExempt)
            {
                salesTax = 0M;
            }
            
            val += ((salesTax + shippingAndHandling)*this._retailPrice);

            return val;
        }

        /// <summary>
        /// Gets the customer retail price.
        /// </summary>
        /// <param name="salesTax">The sales tax.</param>
        /// <param name="shippingAndHandling">The shipping and handling.</param>
        /// <returns></returns>
        public decimal GetCustomerRetailPrice(decimal salesTax, decimal shippingAndHandling)
        {
            var val = this._retailPrice;

            if (this.IsTaxExempt)
            {
                salesTax = 0M;
            }

            val += ((salesTax + shippingAndHandling) * this._retailPrice);
            
            return val;
        }

        /// <summary>
        /// Gets the distributor discount cost.
        /// </summary>
        /// <param name="distributorCost">The distributor cost.</param>
        /// <returns></returns>
        public decimal GetDistributorDiscountCost(decimal distributorCost)
        {
            return this._retailPrice - (this._earnBase*distributorCost);
        }

        /// <summary>
        /// Gets the tax rato to be displayed for the item.
        /// </summary>
        /// <param name="salesTax">The initial tax.</param>
        /// <param name="products">The product dictionary.</param>
        /// <returns></returns>
        public decimal GetTaxRate(decimal salesTax, Dictionary<string,decimal> products)
        {
            if (this.IsTaxExempt)
                return 0M;

            if (salesTax != 0 || products == null)
                return salesTax;

            var taxRate = salesTax;
            if (products.TryGetValue(this.Sku, out taxRate))
                return taxRate/100;
            return salesTax;
        }
    }
}