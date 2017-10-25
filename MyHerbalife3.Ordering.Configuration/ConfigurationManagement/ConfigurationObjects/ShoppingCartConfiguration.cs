using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class ShoppingCartConfiguration : HLConfiguration
    {
        #region Consts

        private const string SHOPPING_CART_INFO = "Shopping_Cart_";

        #endregion

        #region Construction

        public static ShoppingCartConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "ShoppingCart") as ShoppingCartConfiguration;
        }

        #endregion

        #region Static methods

        public static string GetShoppingCartInfoKey(string distributorID, string locale)
        {
            return SHOPPING_CART_INFO + distributorID + "_" + locale;
        }

        #endregion

        #region Config Properties

        /// <summary>
        ///     Max quantity can enter
        /// </summary>
        [ConfigurationProperty("maxQuantity", DefaultValue = 999, IsRequired = false, IsKey = false)]
        public int MaxQuantity
        {
            get { return (int) this["maxQuantity"]; }
            set { this["maxQuantity"] = value; }
        }

        /// <summary>
        ///     Max mini cart items
        /// </summary>
        [ConfigurationProperty("maxMiniCartItem", DefaultValue = 5, IsRequired = false, IsKey = false)]
        public int MaxMiniCartItem
        {
            get { return (int) this["maxMiniCartItem"]; }
            set { this["maxMiniCartItem"] = value; }
        }

        /// <summary>
        ///     Volume points limit
        /// </summary>
        /// <summary>
        ///     Earnings limit
        /// </summary>
        /// <summary>
        ///     default warehouse
        /// </summary>
        [ConfigurationProperty("defaultWarehouse", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DefaultWarehouse
        {
            get { return (string) this["defaultWarehouse"]; }
            set { this["defaultWarehouse"] = value; }
        }

        /// <summary>
        ///     default freight
        /// </summary>
        [ConfigurationProperty("defaultFreightCode", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DefaultFreightCode
        {
            get { return (string) this["defaultFreightCode"]; }
            set { this["defaultFreightCode"] = value; }
        }

        /// <summary>
        ///     default freight code for HAP orders
        /// </summary>
        [ConfigurationProperty("defaultFreightCodeForHAP", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DefaultFreightCodeForHAP
        {
            get { return (string)this["defaultFreightCodeForHAP"]; }
            set { this["defaultFreightCodeForHAP"] = value; }
        }

        [ConfigurationProperty("allowBackorder", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowBackorder
        {
            get { return (bool) this["allowBackorder"]; }
            set { this["allowBackorder"] = value; }
        }

        [ConfigurationProperty("allowBackorderForPickup", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowBackorderForPickup
        {
            get { return (bool) this["allowBackorderForPickup"]; }
            set { this["allowBackorderForPickup"] = value; }
        }

        [ConfigurationProperty("allowBackorderForPickupFromCourier", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowBackorderForPickupFromCourier
        {
            get { return (bool)this["allowBackorderForPickupFromCourier"]; }
            set { this["allowBackorderForPickupFromCourier"] = value; }
        }

        [ConfigurationProperty("allowBackorderForPickupAllTypes", DefaultValue = false, IsRequired = false,
            IsKey = false)]
        public bool AllowBackorderForPickupAllTypes
        {
            get { return (bool) this["allowBackorderForPickupAllTypes"]; }
            set { this["allowBackorderForPickupAllTypes"] = value; }
        }

        [ConfigurationProperty("allowBackorderInventorySKUOnly", DefaultValue = true, IsRequired = false, IsKey = false)
        ]
        public bool AllowBackorderInventorySKUOnly
        {
            get { return (bool) this["allowBackorderInventorySKUOnly"]; }
            set { this["allowBackorderInventorySKUOnly"] = value; }
        }

        /// <summary>
        /// Defines the flag to allow add promotional items to the cart for back order.
        /// </summary>
        [ConfigurationProperty("allowBackorderForPromoType", DefaultValue = false, IsRequired = false, IsKey = false)
]
        public bool AllowBackorderForPromoType
        {
            get { return (bool)this["allowBackorderForPromoType"]; }
            set { this["allowBackorderForPromoType"] = value; }
        }

        /// <summary>
        /// Defines the flag to allow add ONLY promotional items to the cart for back order.
        /// </summary>
        [ConfigurationProperty("allowBackorderForPromoTypeOnly", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool allowBackorderForPromoTypeOnly
        {
            get { return (bool)this["allowBackorderForPromoTypeOnly"]; }
            set { this["allowBackorderForPromoTypeOnly"] = value; }
        }

        [ConfigurationProperty("promotionalWarehouse", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalWarehouse
        {
            get { return (string)this["promotionalWarehouse"]; }
            set { this["promotionalWarehouse"] = value; }
        }

        [ConfigurationProperty("supportLinkedSKU", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool SupportLinkedSKU
        {
            get { return (bool) this["supportLinkedSKU"]; }
            set { this["supportLinkedSKU"] = value; }
        }

        [ConfigurationProperty("priceToShow", DefaultValue = "AmountDue", IsRequired = false, IsKey = false)]
        public string PriceToShow
        {
            get { return (string) this["priceToShow"]; }
            set { this["priceToShow"] = value; }
        }

        [ConfigurationProperty("displayDiscount", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayDiscount
        {
            get { return (bool) this["displayDiscount"]; }
            set { this["displayDiscount"] = value; }
        }

        [ConfigurationProperty("showEarnBaseCurrencySymbol", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowEarnBaseCurrencySymbol
        {
            get { return (bool) this["showEarnBaseCurrencySymbol"]; }
            set { this["showEarnBaseCurrencySymbol"] = value; }
        }

        [ConfigurationProperty("displayRetailPrice", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool DisplayRetailPrice
        {
            get { return (bool) this["displayRetailPrice"]; }
            set { this["displayRetailPrice"] = value; }
        }

        [ConfigurationProperty("displayRetailPriceForLiterature", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayRetailPriceForLiterature
        {
            get { return (bool)this["displayRetailPriceForLiterature"]; }
            set { this["displayRetailPriceForLiterature"] = value; }
        }

        [ConfigurationProperty("displayEarnBase", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayEarnBase
        {
            get { return (bool) this["displayEarnBase"]; }
            set { this["displayEarnBase"] = value; }
        }

        [ConfigurationProperty("yourPriceDisplayDecimal", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool YourPriceDisplayDecimal
        {
            get { return (bool) this["yourPriceDisplayDecimal"]; }
            set { this["yourPriceDisplayDecimal"] = value; }
        }

        [ConfigurationProperty("displayMessageForBackorder", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool DisplayMessageForBackorder
        {
            get { return (bool) this["displayMessageForBackorder"]; }
            set { this["displayMessageForBackorder"] = value; }
        }

        /// <summary>
        ///     Indicates when the volume points amount is visible in CO page (columns in order summary)
        ///     and columns to display in pricelist in ETO order
        /// </summary>
        [ConfigurationProperty("hasVolumePoints", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasVolumePoints
        {
            get { return (bool) this["hasVolumePoints"]; }
            set { this["hasVolumePoints"] = value; }
        }

        /// <summary>
        ///     Indicates when the Discounted Price for Event Tickets is visible in PriceList page (columns in order summary)
        ///     and columns to display in pricelist in ETO order
        /// </summary>
        [ConfigurationProperty("hasDiscountedPriceForEventTicket", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool HasDiscountedPriceForEventTicket
        {
            get { return (bool) this["hasDiscountedPriceForEventTicket"]; }
            set { this["hasDiscountedPriceForEventTicket"] = value; }
        }

        /// <summary>
        ///     Indicates when the Discounted Price for Event Tickets is visible in PriceList page (columns in order summary)
        ///     and columns to display in pricelist in ETO order
        /// </summary>
        [ConfigurationProperty("displayProductDetailsColumn", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool DisplayProductDetailsColumn
        {
            get { return (bool) this["displayProductDetailsColumn"]; }
            set { this["displayProductDetailsColumn"] = value; }
        }

        /// <summary>
        ///     Indicates when the Volume Points for Event Tickets is visible in PriceList page (columns in order summary)
        ///     and columns to display in pricelist in ETO order
        /// </summary>
        [ConfigurationProperty("displayVolumePointsForEventTicket", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool DisplayVolumePointsForEventTicket
        {
            get { return (bool) this["displayVolumePointsForEventTicket"]; }
            set { this["displayVolumePointsForEventTicket"] = value; }
        }

        /// <summary>
        /// Defines the promo Sku
        /// </summary>
        [ConfigurationProperty("promotionalSku", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalSku
        {
            get { return (string)this["promotionalSku"]; }
            set { this["promotionalSku"] = value; }
        }

        /// <summary>
        /// Defines the required Skus to apply in a promo
        /// </summary>
        [ConfigurationProperty("promotionalRequiredSkus", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalRequiredSku
        {
            get { return (string)this["promotionalRequiredSkus"]; }
            set { this["promotionalRequiredSkus"] = value; }
        }

        /// <summary>
        /// Defines the required volume points to apply in a promo
        /// </summary>
        [ConfigurationProperty("promotionalRequiredVolumePoints", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int PromotionalRequiredVolumePoints
        {
            get { return (int)this["promotionalRequiredVolumePoints"]; }
            set { this["promotionalRequiredVolumePoints"] = value; }
        }

        /// <summary>
        /// Indicates a freight code assigned by a promo
        /// </summary>
        [ConfigurationProperty("promotionalFreightCode", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalFreightCode
        {
            get { return (string)this["promotionalFreightCode"]; }
            set { this["promotionalFreightCode"] = value; }
        }

        /// <summary>
        /// Indicates the date when the promotion begins
        /// </summary>
        [ConfigurationProperty("promotionalBeginDate", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalBeginDate
        {
            get { return (string)this["promotionalBeginDate"]; }
            set { this["promotionalBeginDate"] = value; }
        }

        /// <summary>
        /// Indicates the date when the promotion ends
        /// </summary>
        [ConfigurationProperty("promotionalEndDate", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PromotionalEndDate
        {
            get { return (string)this["promotionalEndDate"]; }
            set { this["promotionalEndDate"] = value; }
        }

        /// <summary>
        /// Defines the required IBPAllowed SKUs and their subtypes
        /// </summary>
        [ConfigurationProperty("ibpAllowedSKUWithSubTypes", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string IbpAllowedSKUWithSubTypes
        {
            get { return (string)this["ibpAllowedSKUWithSubTypes"]; }
            set { this["ibpAllowedSKUWithSubTypes"] = value; }
        }

        /// <summary>
        /// Defines if a popup should be displayed to notify the DS is eligible to promo.
        /// </summary>
        [ConfigurationProperty("notifyPromoFromService", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool NotifyPromoFromService
        {
            get
            {
                return (bool)this["notifyPromoFromService"];
            }
            set
            {
                this["notifyPromoFromService"] = value;
            }
        }

        /// <summary>
        ///     Max mini cart items
        /// </summary>
        [ConfigurationProperty("quantityBoxSize", DefaultValue = 3, IsRequired = false, IsKey = false)]
        public int QuantityBoxSize
        {
            get { return (int)this["quantityBoxSize"]; }
            set { this["quantityBoxSize"] = value; }
        }

        /// <summary>
        /// SKUQuantityLimitPeriod by day
        /// </summary>
        [ConfigurationProperty("skuQuantityLimitPeriodByDay", DefaultValue = 1, IsRequired = false, IsKey = false)]
        public int SKUQuantityLimitPeriodByDay
        {
            get { return (int)this["skuQuantityLimitPeriodByDay"]; }
            set { this["skuQuantityLimitPeriodByDay"] = value; }
        }

        /// <summary>
        /// display the PopUp if Promo apply
        /// </summary>
        [ConfigurationProperty("displayPromoPopUp", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayPromoPopUp
        {
            get { return (bool)this["displayPromoPopUp"]; }
            set { this["displayPromoPopUp"] = value; }
        }

        #region SKU Limitation properties
                
        /// <summary>
        /// Max line items in cart
        /// </summary>
        [ConfigurationProperty("maxLineItemsInCart", DefaultValue = 200, IsRequired = false, IsKey = false)]
        public int MaxLineItemsInCart
        {
            get { return (int)this["maxLineItemsInCart"]; }
            set { this["maxLineItemsInCart"] = value; }
        }
                
        /// <summary>
        /// Max line items in cart for foreign Member
        /// </summary>
        [ConfigurationProperty("maxLineItemsInCartForeignMember", DefaultValue = 200, IsRequired = false, IsKey = false)]
        public int MaxLineItemsInCartForeignMember 
        {
            get { return (int)this["maxLineItemsInCartForeignMember"]; }
            set { this["maxLineItemsInCartForeignMember"] = value; }
        }

        /// <summary>
        /// Display the error message for MaxQuantity on all ETO skus
        /// </summary>
        [ConfigurationProperty("displayETOMaxQuantity", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayETOMaxQuantityMessage
        {
            get { return (bool)this["displayETOMaxQuantity"]; }
            set { this["displayETOMaxQuantity"] = value; }
        }
        
        #endregion

        #region Standalone products

        /// <summary>
        /// Defines the skus that cannot be convined with others only between them
        /// </summary>
        [ConfigurationProperty("standaloneSkus", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string StandaloneSkus
        {
            get { return (string)this["standaloneSkus"]; }
            set { this["StandaloneSkus"] = value; }
        }

        /// <summary>
        /// Defines the skus that cannot be convined with any other, they are standalone
        /// </summary>
        [ConfigurationProperty("isolatedSkus", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string IsolatedSkus
        {
            get { return (string)this["isolatedSkus"]; }
            set { this["isolatedSkus"] = value; }
        }

        #endregion

        /// <summary>
        /// Validate if the MB already took the eLearning training
        /// </summary>
        [ConfigurationProperty("checkELearning", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CheckELearning
        {
            get { return (bool)this["checkELearning"]; }
            set { this["checkELearning"] = value; }
        }

        /// <summary>
        /// Training code to validate (eLearning)
        /// </summary>
        [ConfigurationProperty("trainingCode", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string TrainingCode
        {
            get { return (string)this["trainingCode"]; }
            set { this["trainingCode"] = value; }
        }

        /// <summary>
        /// Max PPV allowed for buy if eLearning is not completed
        /// </summary>
        [ConfigurationProperty("eLearningMaxPPV", DefaultValue = "0.0", IsRequired = false, IsKey = false)]
        
        public decimal eLearningMaxPPV
        {
            get { return (decimal)this["eLearningMaxPPV"]; }
            set { this["eLearningMaxPPV"] = value; }
        }
        [ConfigurationProperty("herbalifePickUPFreightCode", DefaultValue = "", IsRequired = false, IsKey = false)]
        
        public string HerbalifePickUPFreightCode
        {
            get { return (string) this["herbalifePickUPFreightCode"]; }
            set { this["herbalifePickUPFreightCode"] = value; }
        }
        #endregion
    }
}