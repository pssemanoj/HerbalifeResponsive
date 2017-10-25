using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ItemTotalsListViewModel
    {
        /// <summary>
        /// Gets or sets the SKU.
        /// </summary>
        /// <value>The SKU.</value>
        public string Sku { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>The quantity.</value>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the total price for this line item.
        /// </summary>
        /// <value>The total price applicable to this line item - sum of all pricing components.</value>
        public decimal LinePrice { get; set; }

        /// <summary>
        /// Gets or sets the retail price = CatalogItem.RetailPrice x Quantity
        /// </summary>
        /// <value>The retail price for this line item.</value>
        public decimal RetailPrice { get; set; }

        /// <summary>
        /// Gets or sets the discounted price = CatalogItem.RetailPrice x Discount x Quantity
        /// </summary>
        /// <value>The discounted price for this line item.</value>
        public decimal DiscountedPrice { get; set; }

        /// <summary>
        /// Gets or sets the discounted price for a single item.
        /// </summary>
        /// <value>The discounted price for this item.</value>
        public decimal DiscountedItemPrice { get; set; }

        ///// <summary>
        ///// Gets or sets the packing and handling: Allocated via rule driven BL.
        ///// </summary>
        ///// <value>The packing handling for this line item.</value>
        //[DataMember(IsRequired = true)]
        //[XmlElement(IsNullable = false)]
        ////public double PackingHandling { get; set; }
        //public double PackageAndHandling { get; set; }

        ///// <summary>
        ///// Gets or sets the freight: Allocated via rule driven BL.
        ///// </summary>
        ///// <value>The freight for this line item.</value>
        //[DataMember(IsRequired = true)]
        //[XmlElement(IsNullable = false)]
        //public double Freight { get; set; }

        /// <summary>
        /// Gets or sets the total tax.
        /// </summary>
        /// <value>The total tax on this line item.</value>
        public decimal LineTax { get; set; }

        /// <summary>
        /// Gets or sets the earn base.
        /// </summary>
        /// <value>The earn base on this line item.</value>
        public decimal EarnBase { get; set; }

        /// <summary>
        /// Volume Points
        /// </summary>
        public decimal VolumePoints { get; set; }

        /// <summary>
        /// Represent Line Item Charges
        /// </summary>
        [JsonIgnore]
        public List<ChargeViewModel> ChargeList { get; set; }

        /// <summary>
        /// Taxable amount for the line item total.
        /// </summary>
        public decimal TaxableAmount { get; set; }


        /// <summary>
        /// get discount tax
        /// </summary>
        public decimal AfterDiscountTax { get; set; }

        /// <summary>
        /// get discount amount
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// product type
        /// </summary>
        public string ProductType { get; set; }

    }
}