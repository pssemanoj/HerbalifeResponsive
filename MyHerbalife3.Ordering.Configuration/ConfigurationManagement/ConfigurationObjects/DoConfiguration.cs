using System;
using System.Configuration;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class DOConfiguration : HLConfiguration
    {
        #region Construction

        public static DOConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "DO") as DOConfiguration;
        }

        #endregion Construction

        #region Config Properties

        [ConfigurationProperty("checkSKUExpirationDate", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CheckSKUExpirationDate
        {
            get { return (bool)this["checkSKUExpirationDate"]; }
            set { this["checkSKUExpirationDate"] = value; }
        }

        [ConfigurationProperty("expirationDateFormat", DefaultValue = "MMMM dd yyyy", IsRequired = false, IsKey = false)]
        public string ExpirationDateFormat
        {
            get { return (string)this["expirationDateFormat"]; }
            set { this["expirationDateFormat"] = value; }
        }

        [ConfigurationProperty("showBulletinBoard", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowBulletinBoard
        {
            get
            {
                bool showBulletinBoard = false;
                if (!bool.TryParse(this["showBulletinBoard"].ToString(), out showBulletinBoard))
                {
                    showBulletinBoard = false;
                }
                return showBulletinBoard;
            }
            set { this["showBulletinBoard"] = value; }
        }

        /// <summary>
        ///     this attribute specify if certain locale is allowed for DO
        /// </summary>
        [ConfigurationProperty("allowDO", DefaultValue = null)]
        public bool AllowDO
        {
            get
            {
                if ( !bool.Parse(Settings.GetRequiredAppSetting("OverrideDBCountryConfig")))
                {
                    var gdoConfig = HlCountryConfigurationProvider.GetCountryConfiguration(Locale);
                    if (gdoConfig != null)
                    {
                        return !gdoConfig.DisableDo;
                    }
                }
                try
                {
                    var value = this["allowDO"];
                    if (null != value)
                    {
                        return (bool)value;
                    }

                    value = HLConfigManager.DefaultConfiguration.DOConfiguration.AllowDO;
                    return (bool)value;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("Configuration", ex, "AllowDO");
                }

                return false;
            }
            set { this["allowDO"] = value; }
        }

        /// <summary>
        ///     this attribute specify if certain locale is allowed to retrieve TIN
        /// </summary>
        [ConfigurationProperty("retrieveTIN", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool RetrieveTIN
        {
            get { return (bool)this["retrieveTIN"]; }
            set { this["retrieveTIN"] = value; }
        }

        /// <summary>
        ///     indicate to show error when login fails
        /// </summary>
        [ConfigurationProperty("showErrorOnLogin", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool ShowErrorOnLogin
        {
            get { return (bool)this["showErrorOnLogin"]; }
            set { this["showErrorOnLogin"] = value; }
        }

        [ConfigurationProperty("isResponsive", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsResponsive
        {
            get { return (bool)this["isResponsive"]; }
            set { this["isResponsive"] = value; }
        }

        [ConfigurationProperty("deliveryTerms", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DeliveryTerms
        {
            get { return (bool)this["deliveryTerms"]; }
            set { this["deliveryTerms"] = value; }
        }

        [ConfigurationProperty("productDetailCntrl", IsRequired = false,
            DefaultValue = "~/Ordering/Controls/ProductDetailControl.ascx")]
        public string ProductDetailCntrl
        {
            get { return (string)this["productDetailCntrl"]; }
            set { this["productDetailCntrl"] = value; }
        }

        [ConfigurationProperty("allowAllDistributor", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowAllDistributor
        {
            get { return (bool)this["allowAllDistributor"]; }
            set { this["allowAllDistributor"] = value; }
        }

        [ConfigurationProperty("categoryPageSize", DefaultValue = 9, IsRequired = false, IsKey = false)]
        public int CategoryPageSize
        {
            get { return (int)this["categoryPageSize"]; }
            set { this["categoryPageSize"] = value; }
        }

        [ConfigurationProperty("orderMonthFormat", DefaultValue = "yyyyMM", IsRequired = false, IsKey = false)]
        public string OrderMonthFormat
        {
            get { return (string)this["orderMonthFormat"]; }
            set { this["orderMonthFormat"] = value; }
        }

        [ConfigurationProperty("orderMonthShortFormat", DefaultValue = "yyMM", IsRequired = false, IsKey = false)]
        public string OrderMonthShortFormat
        {
            get { return (string)this["orderMonthShortFormat"]; }
            set { this["orderMonthShortFormat"] = value; }
        }

        [ConfigurationProperty("orderMonthLongFormat", DefaultValue = "MMM yyyy", IsRequired = false, IsKey = false)]
        public string OrderMonthLongFormat
        {
            get { return (string)this["orderMonthLongFormat"]; }
            set { this["orderMonthLongFormat"] = value; }
        }

        [ConfigurationProperty("dualOrderMonthSort", DefaultValue = "Ascending", IsRequired = false, IsKey = false)]
        public string OrderMonthSort
        {
            get { return (string)this["dualOrderMonthSort"]; }
            set { this["dualOrderMonthSort"] = value; }
        }

        /// <summary>
        /// Indicates when the current culture is used to format the order month.
        /// </summary>
        [ConfigurationProperty("orderMonthFormatLocalProvider", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool OrderMonthFormatLocalProvider
        {
            get { return (bool)this["orderMonthFormatLocalProvider"]; }
            set { this["orderMonthFormatLocalProvider"] = value; }
        }

        [ConfigurationProperty("dupeCheckDaysInterval", DefaultValue = "1", IsRequired = false, IsKey = false)]
        public int DupeCheckDaysInterval
        {
            get { return (int)this["dupeCheckDaysInterval"]; }
            set { this["dupeCheckDaysInterval"] = value; }
        }

        [ConfigurationProperty("getPurchaseLimitsFromFusion", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool GetPurchaseLimitsFromFusion
        {
            get { return (bool)this["getPurchaseLimitsFromFusion"]; }
            set { this["getPurchaseLimitsFromFusion"] = value; }
        }
        [ConfigurationProperty("IsDemoVideo", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsDemoVideo
        {
            get { return (bool)this["IsDemoVideo"]; }
            set { this["IsDemoVideo"] = value; }
        }

        /// <summary>
        /// for FOP and OT : it is implied, only used for PL
        /// </summary>
        [ConfigurationProperty("purchasingLimitRestrictionPeriod", DefaultValue = PurchasingLimitRestrictionPeriod.Unknown,
           IsRequired = false, IsKey = false)]
        public PurchasingLimitRestrictionPeriod PurchasingLimitRestrictionPeriod
        {
            get { return (PurchasingLimitRestrictionPeriod)this["purchasingLimitRestrictionPeriod"]; }
            set { this["purchasingLimitRestrictionPeriod"] = value; }
        }        
        /// <summary>
        /// for FOP and OT : it is implied, only used for PL
        /// </summary>
        //[ConfigurationProperty("purchasingLimitRestrictionPeriod", DefaultValue = PurchasingLimitRestrictionPeriod.Unknown,
        //   IsRequired = false, IsKey = false)]
        //public PurchasingLimitRestrictionPeriod PurchasingLimitRestrictionPeriod
        //{
        //    get { return (PurchasingLimitRestrictionPeriod)this["purchasingLimitRestrictionPeriod"]; }
        //    set { this["purchasingLimitRestrictionPeriod"] = value; }
        //}


        /// <summary>
        /// FOP/OT threshold period in days
        /// </summary>
        [ConfigurationProperty("thresholdPeriod", DefaultValue = 10, IsRequired = false, IsKey = false)]
        public int ThresholdPeriod
        {
            get { return (int)this["thresholdPeriod"]; }
            set { this["thresholdPeriod"] = value; }
        }

        /// <summary>
        /// vp limits per order
        /// </summary>
        [ConfigurationProperty("volumeLimitsPerOrder", IsRequired = false, DefaultValue = "-1.00")]
        public decimal VolumeLimitsPerOrder
        {
            get { return (decimal)this["volumeLimitsPerOrder"]; }
            set { this["volumeLimitsPerOrder"] = value; }
        }

        /// <summary>
        /// vp limits per order
        /// </summary>
        [ConfigurationProperty("volumeLimitsAfterFirstOrderFOP", IsRequired = false, DefaultValue = "3999.99")]
        public decimal VolumeLimitsAfterFirstOrderFOP
        {
            get { return (decimal)this["volumeLimitsAfterFirstOrderFOP"]; }
            set { this["volumeLimitsAfterFirstOrderFOP"] = value; }
        }

        /// <summary>
        /// has additional limits
        /// </summary>
        [ConfigurationProperty("hasAdditionalLimits", IsRequired = false, DefaultValue = false)]
        public bool HasAdditionalLimits
        {
            get { return (bool)this["hasAdditionalLimits"]; }
            set { this["hasAdditionalLimits"] = value; }
        }

        [ConfigurationProperty("enforcesPurchaseLimits", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool EnforcesPurchaseLimits
        {
            get { return (bool)this["enforcesPurchaseLimits"]; }
            set { this["enforcesPurchaseLimits"] = value; }
        }

        [ConfigurationProperty("nonThresholdCountryRequiredPurchasingLimits", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool NonThresholdCountryRequiredPurchasingLimits
        {
            get { return (bool)this["nonThresholdCountryRequiredPurchasingLimits"]; }
            set { this["nonThresholdCountryRequiredPurchasingLimits"] = value; }
        }

        [ConfigurationProperty("purchasingLimitRestrictionType", DefaultValue = PurchasingLimitRestrictionType.Unknown,
            IsRequired = false, IsKey = false)]
        public PurchasingLimitRestrictionType PurchasingLimitRestrictionType
        {
            get { return (PurchasingLimitRestrictionType)this["purchasingLimitRestrictionType"]; }
            set { this["purchasingLimitRestrictionType"] = value; }
        }

        [ConfigurationProperty("allowNonProductsWhenPurchaseLimitsExceeded", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool AllowNonProductsWhenPurchaseLimitsExceeded
        {
            get { return (bool)this["allowNonProductsWhenPurchaseLimitsExceeded"]; }
            set { this["allowNonProductsWhenPurchaseLimitsExceeded"] = value; }
        }

        [ConfigurationProperty("disableAddressSortingInOrderPreferences", DefaultValue = false, IsRequired = false,
            IsKey = false)]
        public bool DisableAddressSortingInOrderPreferences
        {
            get { return (bool)this["disableAddressSortingInOrderPreferences"]; }
            set { this["disableAddressSortingInOrderPreferences"] = value; }
        }

        [ConfigurationProperty("usesTaxRules", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool UsesTaxRules
        {
            get { return (bool)this["usesTaxRules"]; }
            set { this["usesTaxRules"] = value; }
        }

        [ConfigurationProperty("usesDiscountRules", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool UsesDiscountRules
        {
            get { return (bool)this["usesDiscountRules"]; }
            set { this["usesDiscountRules"] = value; }
        }

        [ConfigurationProperty("usesOrderManagementRules", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool usesOrderManagementRules
        {
            get { return (bool)this["usesOrderManagementRules"]; }
            set { this["usesOrderManagementRules"] = value; }
        }

        [ConfigurationProperty("showTotalItems", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool ShowTotalItems
        {
            get { return (bool)this["showTotalItems"]; }
            set { this["showTotalItems"] = value; }
        }

        [ConfigurationProperty("maxTaxableEarnings")]
        public decimal MaxTaxableEarnings
        {
            get { return (decimal)this["maxTaxableEarnings"]; }
            set { this["maxTaxableEarnings"] = value; }
        }

        [ConfigurationProperty("allowEventPurchasing", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowEventPurchasing
        {
            get { return (bool)this["allowEventPurchasing"]; }
            set { this["allowEventPurchasing"] = value; }
        }

        [ConfigurationProperty("showOrderQuickViewForEventTicket", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool ShowOrderQuickViewForEventTicket
        {
            get { return (bool)this["showOrderQuickViewForEventTicket"]; }
            set { this["showOrderQuickViewForEventTicket"] = value; }
        }

        [ConfigurationProperty("eventId", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string EventId
        {
            get { return (string)this["eventId"]; }
            set { this["eventId"] = value; }
        }

        [ConfigurationProperty("skuToNoDisplayForNonQualifyMembers", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SkuToNoDisplayForNonQualifyMembers
        {
            get { return (string)this["skuToNoDisplayForNonQualifyMembers"]; }
            set { this["skuToNoDisplayForNonQualifyMembers"] = value; }
        }

        [ConfigurationProperty("allowShipping", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowShipping
        {
            get { return (bool)this["allowShipping"]; }
            set { this["allowShipping"] = value; }
        }

        [ConfigurationProperty("hasPickupPreference", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasPickupPreference
        {
            get { return (bool)this["hasPickupPreference"]; }
            set { this["hasPickupPreference"] = value; }
        }

        [ConfigurationProperty("hasPickupFromCourierPreference", DefaultValue = false, IsRequired = false, IsKey = false
            )]
        public bool HasPickupFromCourierPreference
        {
            get { return (bool)this["hasPickupFromCourierPreference"]; }
            set { this["hasPickupFromCourierPreference"] = value; }
        }
        [ConfigurationProperty("InvoiceInOrderConfrimation", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool InvoiceInOrderConfrimation
        {
            get { return (bool)this["InvoiceInOrderConfrimation"]; }
            set { this["InvoiceInOrderConfrimation"] = value; }
        }
        [ConfigurationProperty("MemberHasPhoneNumber", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool MemberHasPhoneNumber
        {
            get { return (bool)this["MemberHasPhoneNumber"]; }
            set { this["MemberHasPhoneNumber"] = value; }
        }

        [ConfigurationProperty("AddressOnInvoice", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AddressOnInvoice
        {
            get { return (bool)this["AddressOnInvoice"]; }
            set { this["AddressOnInvoice"] = value; }
        }
        
       [ConfigurationProperty("hasFAQ", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasFAQ
        {
            get { return (bool)this["hasFAQ"]; }
            set { this["hasFAQ"] = value; }
        }
        [ConfigurationProperty("HaveNewHFF", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HaveNewHFF
        {
            get { return (bool)this["HaveNewHFF"]; }
            set { this["HaveNewHFF"] = value; }
        }
        [ConfigurationProperty("hasOrderHistory", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasOrderHistory
        {
            get { return (bool)this["hasOrderHistory"]; }
            set { this["hasOrderHistory"] = value; }
        }

        [ConfigurationProperty("hasMyOrder", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasMyOrder
        {
            get { return (bool)this["hasMyOrder"]; }
            set { this["hasMyOrder"] = value; }
        }

        [ConfigurationProperty("panelConfiguration", DefaultValue = "//Ordering//GlobalDO.xml", IsRequired = false,
            IsKey = false)]
        public string PanelConfiguration
        {
            get { return (string)this["panelConfiguration"]; }
            set { this["panelConfiguration"] = value; }
        }

        /// <summary>
        /// HFF SKU. This property is deprecated, use HFFSkuList instead        
        /// </summary>
        [ConfigurationProperty("HFFHerbalifeSku", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string HFFHerbalifeSku
        {
            get { return (string)this["HFFHerbalifeSku"]; }
            set { this["HFFHerbalifeSku"] = value; }
        }

        /// <summary>
        /// This property is not being used anymore, use HFFSkuMaxQuantity instead
        /// </summary>
        [ConfigurationProperty("quantityBoxSizeHFF", DefaultValue = 5, IsRequired = false, IsKey = false)]
        public int QuantityBoxSizeHFF
        {
            get { return (int)this["quantityBoxSizeHFF"]; }
            set { this["quantityBoxSizeHFF"] = value; }
        }


        [ConfigurationProperty("canCancelDonation", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CanCancelDonation
        {
            get { return (bool)this["canCancelDonation"]; }
            set { this["canCancelDonation"] = value; }
        }

        /// <summary>
        /// allow donation without SKU number 
        /// </summary>
        [ConfigurationProperty("donationWithoutSKU", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DonationWithoutSKU
        {
            get { return (bool)this["donationWithoutSKU"]; }
            set { this["donationWithoutSKU"] = value; }
        }

        [ConfigurationProperty("HFFHerbalifeDefaultValue", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int HFFHerbalifeDefaultValue
        {
            get { return (int)this["HFFHerbalifeDefaultValue"]; }
            set { this["HFFHerbalifeDefaultValue"] = value; }
        }

        [ConfigurationProperty("allowHFF", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowHFF
        {
            get { return (bool)this["allowHFF"]; }
            set { this["allowHFF"] = value; }
        }

        [ConfigurationProperty("showHFFBox", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool ShowHFFBox
        {
            get { return (bool)this["showHFFBox"]; }
            set { this["showHFFBox"] = value; }
        }

        /// <summary>
        ///     To show the HFF in a modal popup.
        /// </summary>
        [ConfigurationProperty("allowHFFModal", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowHFFModal
        {
            get { return (bool)this["allowHFFModal"]; }
            set { this["allowHFFModal"] = value; }
        }

        /// <summary>
        ///     this is to show HFF link on menu-US
        /// </summary>
        [ConfigurationProperty("showHFFLinkOnMenu", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowHFFLinkOnMenu
        {
            get { return (bool)this["showHFFLinkOnMenu"]; }
            set { this["showHFFLinkOnMenu"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show HFF link on ETO].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show HFF link on ETO]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("showHFFLinkOnETO", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowHFFLinkOnETO
        {
            get { return (bool)this["showHFFLinkOnETO"]; }
            set { this["showHFFLinkOnETO"] = value; }
        }

        /// <summary>
        ///     The order type for the HFF standalone in modal.
        /// </summary>
        [ConfigurationProperty("HFFModalOrderType", DefaultValue = "APF", IsRequired = false, IsKey = false)]
        public string HFFModalOrderType
        {
            get { return (string)this["HFFModalOrderType"]; }
            set { this["HFFModalOrderType"] = value; }
        }

        /// <summary>
        ///     Indicates if there is a description for the unit to donate.
        /// </summary>
        [ConfigurationProperty("hasHFFUnitDescription", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasHFFUnitDescription
        {
            get { return (bool)this["hasHFFUnitDescription"]; }
            set { this["hasHFFUnitDescription"] = value; }
        }

        [ConfigurationProperty("changeOrderingLeftMenuMyHL3", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ChangeOrderingLeftMenuMyHL3
        {
            get { return (bool)this["changeOrderingLeftMenuMyHL3"]; }
            set { this["changeOrderingLeftMenuMyHL3"] = value; }
        }

        /// <summary>
        ///     The unid description to donate.
        /// </summary>
        [ConfigurationProperty("HFFUnitDescription", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string HFFUnitDescription
        {
            get { return (string)this["HFFUnitDescription"]; }
            set { this["HFFUnitDescription"] = value; }
        }

        /// <summary>
        ///     Indicates when a message will be displayed in CO2 page when the HFF sku is present in the shopping cart.
        /// </summary>
        [ConfigurationProperty("showHFFMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowHFFMessage
        {
            get { return (bool)this["showHFFMessage"]; }
            set { this["showHFFMessage"] = value; }
        }

        // PC/AD online ordering , order month is not editable
        [ConfigurationProperty("orderMonthEnabled", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool OrderMonthEnabled
        {
            get { return (bool)this["orderMonthEnabled"]; }
            set { this["orderMonthEnabled"] = value; }
        }

        [ConfigurationProperty("HFFUrl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string HFFUrl
        {
            get { return (string)this["HFFUrl"]; }
            set { this["HFFUrl"] = value; }
        }

        [ConfigurationProperty("allowTodaysMagazine", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowTodaysMagazine
        {
            get { return (bool)this["allowTodaysMagazine"]; }
            set { this["allowTodaysMagazine"] = value; }
        }

        [ConfigurationProperty("DisableProductSkuOnPrintpage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisableProductSkuOnPrintpage
        {
            get { return (bool)this["DisableProductSkuOnPrintpage"]; }
            set { this["DisableProductSkuOnPrintpage"] = value; }
        }


        [ConfigurationProperty("todayMagazineSku", DefaultValue = "S202", IsRequired = false, IsKey = false)]
        public string TodayMagazineSku
        {
            get { return (string)this["todayMagazineSku"]; }
            set { this["todayMagazineSku"] = value; }
        }

        [ConfigurationProperty("todayMagazineSecondarySku", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string TodayMagazineSecondarySku
        {
            get { return (string)this["todayMagazineSecondarySku"]; }
            set { this["todayMagazineSecondarySku"] = value; }
        }

        [ConfigurationProperty("todayMagazineMax", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int TodayMagazineMax
        {
            get { return (int)this["todayMagazineMax"]; }
            set { this["todayMagazineMax"] = value; }
        }
        [ConfigurationProperty("HFFValueOne", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int HFFValueOne
        {
            get { return (int)this["HFFValueOne"]; }
            set { this["HFFValueOne"] = value; }
        }
        [ConfigurationProperty("HFFValueTwo", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int HFFValueTwo
        {
            get { return (int)this["HFFValueTwo"]; }
            set { this["HFFValueTwo"] = value; }
        }

        [ConfigurationProperty("purchasingLimitsControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PurchasingLimitsControl
        {
            get { return (string)this["purchasingLimitsControl"]; }
            set { this["purchasingLimitsControl"] = value; }
        }

        [ConfigurationProperty("checkTodaysMagazineAvailability", DefaultValue = false, IsRequired = false,
            IsKey = false)]
        public bool CheckTodaysMagazineAvailability
        {
            get { return (bool)this["checkTodaysMagazineAvailability"]; }
            set { this["checkTodaysMagazineAvailability"] = value; }
        }

        /// <summary>
        /// Indicates when restriction per product type applies.
        /// </summary>
        [ConfigurationProperty("todayMagazineProdTypeRestricted", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool TodayMagazineProdTypeRestricted
        {
            get { return (bool)this["todayMagazineProdTypeRestricted"]; }
            set { this["todayMagazineProdTypeRestricted"] = value; }
        }

        [ConfigurationProperty("enableSearch", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool EnableSearch
        {
            get { return (bool)this["enableSearch"]; }
            set { this["enableSearch"] = value; }
        }

        [ConfigurationProperty("eventTicketUrl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string EventTicketUrl
        {
            get { return (string)this["eventTicketUrl"]; }
            set { this["eventTicketUrl"] = value; }
        }

        [ConfigurationProperty("eventTicketUrlTarget", DefaultValue = "_blank", IsRequired = false, IsKey = false)]
        public string EventTicketUrlTarget 
        {
            get { return (string)this["eventTicketUrlTarget"]; }
            set { this["eventTicketUrlTarget"] = value; }
        }

        [ConfigurationProperty("IBPSku", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string IBPSku
        {
            get { return (string)this["IBPSku"]; }
            set { this["IBPSku"] = value; }
        }

        [ConfigurationProperty("notToshowTodayMagazineInCart", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool NotToshowTodayMagazineInCart
        {
            get { return (bool)this["notToshowTodayMagazineInCart"]; }
            set { this["notToshowTodayMagazineInCart"] = value; }
        }

        /// <summary>
        /// Indicates the default option for Inventory where 0 = All inventory, 1 = Available inventory only.
        /// </summary>
        [ConfigurationProperty("inventoryViewDefault", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int InventoryViewDefault
        {
            get { return (int)this["inventoryViewDefault"]; }
            set { this["inventoryViewDefault"] = value; }
        }

        [ConfigurationProperty("resetInventoryViewDefaultAfterSumbitOrder", DefaultValue = false, IsRequired = false,
            IsKey = false)]
        public bool ResetInventoryViewDefaultAfterSumbitOrder
        {
            get { return (bool)this["resetInventoryViewDefaultAfterSumbitOrder"]; }
            set { this["resetInventoryViewDefaultAfterSumbitOrder"] = value; }
        }

        [ConfigurationProperty("allowCreateOrderFromInvoice", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowCreateOrderFromInvoice
        {
            get { return (bool)this["allowCreateOrderFromInvoice"]; }
            set { this["allowCreateOrderFromInvoice"] = value; }
        }

        [ConfigurationProperty("phoneSplit", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PhoneSplit
        {
            get { return (bool)this["phoneSplit"]; }
            set { this["phoneSplit"] = value; }
        }

        [ConfigurationProperty("showEventTicketMessaging")]
        public bool ShowEventTicketMessaging
        {
            get { return (bool)this["showEventTicketMessaging"]; }
            set { this["showEventTicketMessaging"] = value; }
        }

        [ConfigurationProperty("inMaintenance")]
        public bool InMaintenance
        {
            get { return (bool)this["inMaintenance"]; }
            set { this["inMaintenance"] = value; }
        }

        [ConfigurationProperty("landingPage", DefaultValue = "pricelist.aspx")]
        public string LandingPage
        {
            get { return (string)this["landingPage"]; }
            set { this["landingPage"] = value; }
        }

        [ConfigurationProperty("loadFAQMenuFromHTML", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool LoadFAQMenuFromHTML
        {
            get { return (bool)this["loadFAQMenuFromHTML"]; }
            set { this["loadFAQMenuFromHTML"] = value; }
        }

        [ConfigurationProperty("enforcesPurchasingPermissions", DefaultValue = false, IsRequired = false, IsKey = false)
        ]
        public bool EnforcesPurchasingPermissions
        {
            get { return (bool)this["enforcesPurchasingPermissions"]; }
            set { this["enforcesPurchasingPermissions"] = value; }
        }

        [ConfigurationProperty("ignoreInventory", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IgnoreInventory
        {
            get { return (bool)this["ignoreInventory"]; }
            set { this["ignoreInventory"] = value; }
        }

        [ConfigurationProperty("allowFreeDistributorOrdering", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowFreeDistributorOrdering
        {
            get { return (bool)this["allowFreeDistributorOrdering"]; }
            set { this["allowFreeDistributorOrdering"] = value; }
        }

        /// <summary>
        ///     Gets or sets the flag value to process saved carts.
        /// </summary>
        [ConfigurationProperty("allowSavedCarts", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AllowSavedCarts
        {
            get { return (bool)this["allowSavedCarts"]; }
            set { this["allowSavedCarts"] = value; }
        }

        [ConfigurationProperty("saveDSSubType", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SaveDSSubType
        {
            get { return (bool)this["saveDSSubType"]; }
            set { this["saveDSSubType"] = value; }
        }

        /// <summary>
        ///     Reason why Ordering is not alowed.
        /// </summary>
        [ConfigurationProperty("orderingUnavailableReason", DefaultValue = OrderingUnavailableReason.ScheduledDowntime, IsRequired = false, IsKey = false)]
        public OrderingUnavailableReason OrderingUnavailableReason
        {
            get
            {
                var gdoConfig = HlCountryConfigurationProvider.GetCountryConfiguration(Locale);
                if (gdoConfig != null && gdoConfig.DisableDo)
                {
                    return (OrderingUnavailableReason)gdoConfig.OrderingUnavailableReason;
                }

                return (OrderingUnavailableReason)this["orderingUnavailableReason"];
            }
            set { this["orderingUnavailableReason"] = value; }
        }

        /// <summary>
        ///     Countries may have deferred processing
        /// </summary>
        [ConfigurationProperty("hasDeferredProcessing", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasDeferredProcessing
        {
            get { return (bool)this["hasDeferredProcessing"]; }
            set { this["hasDeferredProcessing"] = value; }
        }

        [ConfigurationProperty("sendEmailUsingSubmitOrder", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SendEmailUsingSubmitOrder
        {
            get { return (bool)this["sendEmailUsingSubmitOrder"]; }
            set { this["sendEmailUsingSubmitOrder"] = value; }
        }

        [ConfigurationProperty("sendEmailUsingSubmitOrderForWire", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool SendEmailUsingSubmitOrderForWire
        {
            get { return (bool)this["sendEmailUsingSubmitOrderForWire"]; }
            set { this["sendEmailUsingSubmitOrderForWire"] = value; }
        }

        [ConfigurationProperty("pendingOrdersUsesPaymentGateway", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PendingOrdersUsesPaymentGateway
        {
            get { return (bool)this["pendingOrdersUsesPaymentGateway"]; }
            set { this["pendingOrdersUsesPaymentGateway"] = value; }
        }
        
        [ConfigurationProperty("taxRateSKU", DefaultValue = "3152", IsRequired = false, IsKey = false)]
        public string TaxRateSKU
        {
            get { return (string)this["taxRateSKU"]; }
            set { this["taxRateSKU"] = value; }
        }

        [ConfigurationProperty("extravaganzaCategoryName", DefaultValue = "Extravaganza", IsRequired = false, IsKey = false)]
        public string ExtravaganzaCategoryName
        {
            get { return (string)this["extravaganzaCategoryName"]; }
            set { this["extravaganzaCategoryName"] = value; }
        }

        /// <summary>
        ///     Gets or sets the flag value to display pending orders page.
        /// </summary>
        [ConfigurationProperty("allowPendingOrders", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowPendingOrders
        {
            get { return (bool)this["allowPendingOrders"]; }
            set { this["allowPendingOrders"] = value; }
        }

        [ConfigurationProperty("modifyTodaysMagazineQuantity", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ModifyTodaysMagazineQuantity
        {
            get { return (bool)this["modifyTodaysMagazineQuantity"]; }
            set { this["modifyTodaysMagazineQuantity"] = value; }
        }

        /// <summary>
        /// Indicates when the order month default value is not set.
        /// </summary>
        [ConfigurationProperty("preselectedDualOrderMonth", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool PreselectedDualOrderMonth
        {
            get { return (bool)this["preselectedDualOrderMonth"]; }
            set { this["preselectedDualOrderMonth"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [use gregorian calendar].
        /// </summary>
        /// <value>
        /// <c>true</c> if [use gregorian calendar]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("useGregorianCalendar", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool UseGregorianCalendar
        {
            get
            {
                return (bool)this["useGregorianCalendar"];
            }
            set
            {
                this["useGregorianCalendar"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the flag value to simulate freight charge.
        /// </summary>
        [ConfigurationProperty("allowFreightSimulation", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowFreightSimulation
        {
            get { return (bool)this["allowFreightSimulation"]; }
            set { this["allowFreightSimulation"] = value; }
        }        

        [ConfigurationProperty("calculateWithoutItems", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CalculateWithoutItems
        {
            get { return (bool)this["calculateWithoutItems"]; }
            set { this["calculateWithoutItems"] = value; }
        }

        /// <summary>
        /// Indicates the extra hours to extend the dual order month time configured.
        /// </summary>
        [ConfigurationProperty("extendDualOrderMonth", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int ExtendDualOrderMonth
        {
            get { return (int)this["extendDualOrderMonth"]; }
            set { this["extendDualOrderMonth"] = value; }
        }

        /// <summary>
        /// Get or sets the country where MLM is enabled
        /// </summary>
        [ConfigurationProperty("hasMLMCheck", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasMLMCheck
        {
            get
            {
                return (bool)this["hasMLMCheck"];
            }
            set
            {
                this["hasMLMCheck"] = value;
            }
        }

        /// <summary>
        /// Gets or set the parameter to allow zero pricing in event ticket order.
        /// </summary>
        [ConfigurationProperty("allowZeroPricingEventTicket", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowZeroPricingEventTicket
        {
            get
            {
                return (bool)this["allowZeroPricingEventTicket"];
            }
            set
            {
                this["allowZeroPricingEventTicket"] = value;
            }
        }

        /// <summary>
        /// is china mode
        /// </summary>
        [ConfigurationProperty("isChina", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsChina
        {
            get { return (bool)this["isChina"]; }
            set { this["isChina"] = value; }
        }
        /// <summary>
        /// Gets or sets the property to allow Create Invoice
        /// </summary>
        [ConfigurationProperty("allowToCreateInvoice", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowToCreateInvoice
        {
            get { return (bool)this["allowToCreateInvoice"]; }
            set { this["allowToCreateInvoice"] = value; } 
        }
        /// <summary>
        /// Gets or sets the property to show Cradit Card Message on Catalog page
        /// </summary>
        [ConfigurationProperty("allowToDispalyCCMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AllowToDispalyCCMessage
        {
            get { return (bool)this["allowToDispalyCCMessage"]; }
            set { this["allowToDispalyCCMessage"] = value; } 
        }
        [ConfigurationProperty("Invoice_EnableMyOrders", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool InvoiceEnableMyOrders
        {
            get { return (bool)this["Invoice_EnableMyOrders"]; }
            set { this["Invoice_EnableMyOrders"] = value; }
        }
        [ConfigurationProperty("InvoiceHasHMSCal", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool InvoiceHasHMSCal
        {
            get { return (bool)this["InvoiceHasHMSCal"]; }
            set { this["InvoiceHasHMSCal"] = value; }
        }
        [ConfigurationProperty("AddrerssVelidationInvoice", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool AddrerssVelidationInvoice
        {
            get { return (bool)this["AddrerssVelidationInvoice"]; }
            set { this["AddrerssVelidationInvoice"] = value; }
        }
        /// <summary>
        /// Show End of Month Timer
        /// </summary>
        [ConfigurationProperty("showEOFTimer", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool ShowEOFTimer
        {
            get { return (bool)this["showEOFTimer"]; }
            set { this["showEOFTimer"] = value; }
        }
        /// <summary>
        /// Indicates the number of days to show EOM Timer
        /// </summary>
        [ConfigurationProperty("eomCounterDisplayDays", DefaultValue = 4, IsRequired = false, IsKey = false)]
        public int EOMCounterDisplayDays
        {
            get { return (int)this["eomCounterDisplayDays"]; }
            set { this["eomCounterDisplayDays"] = value; }
        }
        /// <summary>
        /// Display sepereate Grid for Promo Sku
        /// </summary>
        [ConfigurationProperty("promoSkuGrid", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool PromoSkuGrid
        {
            get { return (bool)this["promoSkuGrid"]; }
            set { this["promoSkuGrid"] = value; }
        }


        // for US extravaganza launch
        [ConfigurationProperty("showPopupOnPage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowPopupOnPage
        {
            get { return (bool)this["showPopupOnPage"]; }
            set { this["showPopupOnPage"] = value; }
        }

        // for volume points display
        //[ConfigurationProperty("showVolumePoints", DefaultValue = false, IsRequired = false, IsKey = false)]
        //public bool ShowVolumePoints
        //{
        //    get { return (bool)this["showVolumePoints"]; }
        //    set { this["showVolumePoints"] = value; }
        //}

        /// <summary>
        /// Defines a category for rule management.
        /// </summary>
        [ConfigurationProperty("specialCategoryName", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SpecialCategoryName
        {
            get { return (string)this["specialCategoryName"]; }
            set { this["specialCategoryName"] = value; }
        }

        /// <summary>
        /// Defines if a changes for an event are valid or not.
        /// </summary>
        [ConfigurationProperty("isEventInProgress", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool IsEventInProgress
        {
            get { return (bool)this["isEventInProgress"]; }
            set { this["isEventInProgress"] = value; }
        }

        /// <summary>
        /// Defines the warehouses included when split order.
        /// </summary>
        [ConfigurationProperty("whCodesForSplit", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string WhCodesForSplit
        {
            get { return (string)this["whCodesForSplit"]; }
            set { this["whCodesForSplit"] = value; }
        }

        /// <summary>
        /// Defines the freight code list to be included when split order. If the list is empty all freight are included.
        /// </summary>
        [ConfigurationProperty("freightCodesForSplit", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string FreightCodesForSplit
        {
            get { return (string)this["freightCodesForSplit"]; }
            set { this["freightCodesForSplit"] = value; }
        }

        [ConfigurationProperty("hasDetailLink", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasDetailLink
        {
            get { return (bool)this["hasDetailLink"]; }
            set { this["hasDetailLink"] = value; }
        }
        [ConfigurationProperty("detailLinkAddress", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DetailLinkAddress
        {
            get { return (string)this["detailLinkAddress"]; }
            set { this["detailLinkAddress"] = value; }
        }

        [ConfigurationProperty("showDeletedAddressesMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowDeletedAddressesMessage
        {
            get
            {
                bool showDeletedAddressesMessage = false;
                if (!bool.TryParse(this["showDeletedAddressesMessage"].ToString(), out showDeletedAddressesMessage))
                {
                    showDeletedAddressesMessage = false;
                }
                return showDeletedAddressesMessage;
            }
            set { this["showDeletedAddressesMessage"] = value; }
        }

        #region KR
                
        [ConfigurationProperty("hasPayeeID", DefaultValue=false, IsRequired=false, IsKey=false)]
        public bool HasPayeeID 
        {
            get { return (bool)this["hasPayeeID"];  }
            set { this["hasPayeeID"] = value; }
        }

        #endregion

        [ConfigurationProperty("useTotalVolumeToReconciliation", IsRequired = false, DefaultValue = false)]
        public bool UseTotalVolumeToReconciliation
        {
            get { return (bool)this["useTotalVolumeToReconciliation"]; }
            set { this["useTotalVolumeToReconciliation"] = value; }
        }


        [ConfigurationProperty("allowHAP", IsRequired = false, DefaultValue = false)]
        public bool AllowHAP
        {
            get { return (bool)this["allowHAP"]; }
            set { this["allowHAP"] = value; }
        }

        [ConfigurationProperty("displayBackOrderEnhancements", IsRequired = false, DefaultValue = false)]
        public bool DisplayBackOrderEnhancements
        {
            get { return (bool)this["displayBackOrderEnhancements"]; }
            set { this["displayBackOrderEnhancements"] = value; }
        }

        /// <summary>
        /// Defines a category for Apparel menu item on left menu.
        /// </summary>
        [ConfigurationProperty("apparelCategoryName", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ApparelCategoryName
        {
            get { return (string)this["apparelCategoryName"]; }
            set { this["apparelCategoryName"] = value; }
        }

        /// <summary>
        /// List of HFF SKUs. This property has the purpose to replace HFFHerbalifeSku property which accepts only 1 SKU
        /// Values should be separated by coma Ex. 0141,0142
        /// First SKU in list is considered the main HFF SKU and the one taken in HFF module in COP1
        /// </summary>
        [ConfigurationProperty("HFFSkuList", DefaultValue = "", IsRequired = false, IsKey = false),
         TypeConverter(typeof(StringListConverter))]
        public List<string> HFFSkuList
        {
            get { return this["HFFSkuList"] as List<string>; }
            set { this["HFFSkuList"] = value; }
        }

        /// <summary>
        /// Max quantity allowed for HFF SKUs, applies to all SKUs included in HFFSkuList     
        /// </summary>
        [ConfigurationProperty("HFFSkuMaxQuantity", DefaultValue = 999, IsRequired = false, IsKey = false)]
        public int HFFSkuMaxQuantity
        {
            get { return (int)this["HFFSkuMaxQuantity"]; }
            set { this["HFFSkuMaxQuantity"] = value; }
        }


        /// <summary>
        /// Defines the format of date time to display.
        /// </summary>
        [ConfigurationProperty("dateTimeFormat", DefaultValue = "G", IsRequired = false, IsKey = false)]
        public string DateTimeFormat
        {
            get { return (string)this["dateTimeFormat"]; }
            set { this["dateTimeFormat"] = value; }
        }


        [ConfigurationProperty("showStaticPaymentInfo", IsRequired = false, DefaultValue = false)]
        public bool ShowStaticPaymentInfo
        {
            get { return (bool)this["showStaticPaymentInfo"]; }
            set { this["showStaticPaymentInfo"] = value; }
        }

        [ConfigurationProperty("displayNonResidentsDisclaimer", DefaultValue = false, IsRequired = false, IsKey = false )]
        public bool DisplayNonResidentsMessage
        {
            get { return (bool)this["displayNonResidentsDisclaimer"]; }
            set { this["displayNonResidentsDisclaimer"] = value; }
        }

        [ConfigurationProperty("displayLookUpPriceListGenerator", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool DisplayLookUpPriceListGenerator
        {
            get { return (bool)this["displayLookUpPriceListGenerator"]; }
            set { this["displayLookUpPriceListGenerator"] = value; }
        }

        [ConfigurationProperty("displayMissingTinMessage", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayMissingTinMessage
        {
            get { return (bool)this["displayMissingTinMessage"]; }
            set { this["displayMissingTinMessage"] = value; }
        }

        /// <summary>
        /// This flag is to identify if the country should add the javascripts for Adobe Target and Salesforce PI
        /// </summary>
        [ConfigurationProperty("addScriptsForRecommendations", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool AddScriptsForRecommendations
        {
            get { return (bool)this["addScriptsForRecommendations"]; }
            set { this["addScriptsForRecommendations"] = value; }
        }

        /// <summary>
        /// Defines the format of date time to display.
        /// </summary>
        [ConfigurationProperty("salesforceCollectOrderItemFormat", DefaultValue = "{{ \"item\" : \"{0}_{1}\", \"quantity\": \"{2}\" , \"price\" : \"{3}\" , \"unique_id\" : \"{0}_{1}\" }}", IsRequired = false, IsKey = false)]
        public string SalesforceCollectOrderItemFormat
        {
            get { return (string)this["salesforceCollectOrderItemFormat"]; }
            set { this["salesforceCollectOrderItemFormat"] = value; }
        }

        [ConfigurationProperty("displayBifurcationKeys", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplayBifurcationKeys
        {
            get { return (bool)this["displayBifurcationKeys"]; }
            set { this["displayBifurcationKeys"] = value; }
        }

        [ConfigurationProperty("redirectToShop", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool RedirectToShop
        {
            get { return (bool)this["redirectToShop"]; }
            set { this["redirectToShop"] = value; }
        }

        [ConfigurationProperty("shopUrlForMB", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShopUrlForMB
        {
            get { return (string)this["shopUrlForMB"]; }
            set { this["shopUrlForMB"] = value; }
        }

        [ConfigurationProperty("shopUrlForDS", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShopUrlForDS
        {
            get { return (string)this["shopUrlForDS"]; }
            set { this["shopUrlForDS"] = value; }
        }

        [ConfigurationProperty("shopUrlForMBInvoice", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShopUrlForMBInvoice
        {
            get { return (string)this["shopUrlForMBInvoice"]; }
            set { this["shopUrlForMBInvoice"] = value; }
        }

        [ConfigurationProperty("shopUrlForDSInvoice", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShopUrlForDSInvoice
        {
            get { return (string)this["shopUrlForDSInvoice"]; }
            set { this["shopUrlForDSInvoice"] = value; }
        }

        [ConfigurationProperty("skuForCurrentEvent", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string SkuForCurrentEvent
        {
            get { return (string)this["skuForCurrentEvent"]; }
            set { this["skuForCurrentEvent"] = value; }
        }

        [ConfigurationProperty("showFreightChrageonCOP1", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ShowFreightChrageonCOP1
        {
            get { return (bool)this["showFreightChrageonCOP1"]; }
            set { this["showFreightChrageonCOP1"] = value; }
        }

        #region Price List Generator settings
        /// <summary>
        /// Default sales tax in price list generator page
        /// </summary>
        [ConfigurationProperty("plgDefaultTax", IsRequired = false, DefaultValue = "0.0")]
        public decimal PLGDefaultTax
        {
            get { return (decimal)this["plgDefaultTax"]; }
            set { this["plgDefaultTax"] = value; }
        }

        /// <summary>
        /// Default shipping and handling in price list generator page
        /// </summary>
        [ConfigurationProperty("plgDefaultSH", IsRequired = false, DefaultValue = "0.0")]
        public decimal PLGDefaultSH
        {
            get { return (decimal)this["plgDefaultSH"]; }
            set { this["plgDefaultSH"] = value; }
        }
        #endregion

        #endregion Config Properties

        #region Private methods

        private string GetFallbackErrorMessage(string Property)
        {
            return string.Format("Property {0} not found in DOConfig for culture {1}. Falling back to Default", Property,
                                 Thread.CurrentThread.CurrentCulture);
        }

        private string GetDatabaseFallbackErrorMessage(string Property)
        {
            return string.Format("Database call failed to fetch country configuration. Falling back to Default", Property,
                                 Thread.CurrentThread.CurrentCulture);
        }

        private string GetFallbackFailedErrorMessage(string property)
        {
            return string.Format("Property {0} was not found in DOConfig for Default configuration", property);
        }

        [ConfigurationProperty("displaySyndicatedWidget", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool DisplaySyndicatedWidget
        {
            get { return (bool)this["displaySyndicatedWidget"]; }
            set { this["displaySyndicatedWidget"] = value; }
        }

        #endregion Private methods
    }
}
