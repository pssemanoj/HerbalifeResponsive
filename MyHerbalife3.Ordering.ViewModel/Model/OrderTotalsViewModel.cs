#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class OrderTotalsViewModel
    {
        public Guid Id { get; set; }

        /// DiscountedPriceTotal + PackingHandlingTotal + FreightTotal + ProductTaxTotal + FreightTaxTotal + PackingHandlingTaxTotal
        public decimal AmountDue { get; set; }

        public decimal BalanceAmount { get; set; }

        [JsonIgnore]
        public List<ChargeViewModel> Charges { get; set; }

        public decimal DiscountPercentage { get; set; }

        /// <summary>
        ///     Type of discount returned from HMS.
        /// </summary>
        public string DiscountType { get; set; }

        public decimal DiscountedItemsTotal { get; set; }

        public decimal DiscountAmount{ get; set; }

        /// <summary>
        ///     Gets or sets the ICMS tax for Brazil
        /// </summary>
        public decimal IcmsTax { get; set; }

        /// <summary>
        ///     Gets or sets the IPI tax for Brazil
        /// </summary>
        public decimal IpiTax { get; set; }

        // public ItemTotalsList ItemTotalsList { get; set; }

        /// <summary>
        ///     Represent Item Totals
        /// </summary>
        /// <summary>
        ///     Gets or sets the retail price total - the sum cost of the items only, before any discounts.
        /// </summary>
        /// <value>The retail price total.</value>
        public decimal ItemsTotal { get; set; }

        /// <summary>
        ///     Literature Retail Amount
        /// </summary>
        [JsonIgnore]
        public decimal LiteratureRetailAmount { get; set; }

        /// <summary>
        ///     Marketing Fund Amount
        /// </summary>
        [JsonIgnore]
        public decimal MarketingFundAmount { get; set; }

        /// <summary>
        ///     Miscellaneous Amount
        /// </summary>
        public decimal MiscAmount { get; set; }

        /// <summary>
        ///     Product Retail Amount
        /// </summary>
        public decimal ProductRetailAmount { get; set; }

        /// <summary>
        ///     Gets or sets the product tax total.
        /// </summary>
        /// <value>The portion of tax emanating from tax on products themselves.</value>
        public decimal ProductTaxTotal { get; set; }

        /// <summary>
        ///     Promotion Retail Amount
        /// </summary>
        public decimal PromotionRetailAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total tax amount.
        /// </summary>
        /// <value>The tax total: Sum total of all tax items: Products, Packing &amp; Handling, Freight.</value>
        /// <remarks>
        ///     =
        ///     ProductTaxTotal + FreightTaxTotal + PackingHandlingTaxTotal
        /// </remarks>
        public decimal TaxAmount { get; set; }

        /// <summary>
        ///     Gets or sets the taxable amount total.
        /// </summary>
        /// <value>The basis for tax on the whole order: DiscounterdPriceTotal + FreightTotal + PackingHandlingTotal.  .</value>
        public decimal TaxableAmountTotal { get; set; }

        /// <summary>
        ///     Gets or sets the discount price - the sum of the dicount on each item.
        /// </summary>
        /// <value>The total discount.</value>
        public decimal TotalItemDiscount { get; set; }

        public decimal VolumePoints { get; set; }

        public string PricingServerName { get; set; }

        public decimal TotalEarnBase { get; set; }

        public decimal ChargeFreightAmount { get; set; }

        public decimal ChargePHAmount { get; set; }

        /// <summary>
        ///     gets or sets the total excise tax value for MX
        /// </summary>
        public decimal ExciseTax { get; set; }

        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? CreatedDate { get; set; }
        [JsonConverter(typeof(CustomDateTimeConverter))]
        public DateTime? ModifiedDate { get; set; }

        public List<ItemTotalsListViewModel> LineItems { get; set; }

        public List<PromotionViewModel> Promotions { get; set; }

        public ApfViewModel Apf { get; set; }
    }
}