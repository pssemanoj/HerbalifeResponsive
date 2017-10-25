using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class CheckoutConfiguration : HLConfiguration
    {
        #region enums

        public enum CurrencySymbolLayout
        {
            Leading,
            Trailing
        }

        #endregion enums

        #region Consts

        private const string ORDER_MONTH_KEY = "OrderMonth";
        private const string PREVIOUS_MONTH_SELECTED_KEY = "PreviousMonthSelected";

        #endregion Consts

        #region Construction

        public static CheckoutConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "Checkout") as CheckoutConfiguration;
        }

        #endregion Construction

        #region Static methods

        public static string GetOrderMonthSessionKey()
        {
            return ORDER_MONTH_KEY;
        }

        public static string GetPreviouosMonthSelectedSessionKey(string distributorID)
        {
            return PREVIOUS_MONTH_SELECTED_KEY + distributorID;
        }

        #endregion Static methods

        #region Config Properties

        [ConfigurationProperty("overrideHMSCalc", DefaultValue = false, IsRequired = false)]
        public bool OverrideHMSCalc
        {
            get { return (bool) this["overrideHMSCalc"]; }
            set { this["overrideHMSCalc"] = value; }
        }

        /// <summary>
        ///     Country's currency name
        /// </summary>
        [ConfigurationProperty("currency", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string Currency
        {
            get { return (string) this["currency"]; }
            set { this["currency"] = value; }
        }

        /// <summary>
        ///     Symbol
        /// </summary>
        [ConfigurationProperty("useConfigCurrencySymbol", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool UseConfigCurrencySymbol
        {
            get { return (bool) this["useConfigCurrencySymbol"]; }
            set { this["useConfigCurrencySymbol"] = value; }
        }

        /// <summary>
        ///     Symbol
        /// </summary>
        [ConfigurationProperty("hideFreightCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HideFreightCharges
        {
            get { return (bool) this["hideFreightCharges"]; }
            set { this["hideFreightCharges"] = value; }
        }

        /// <summary>
        ///     Symbol
        /// </summary>
        [ConfigurationProperty("currencySymbol", DefaultValue = "$", IsRequired = true, IsKey = false)]
        public string CurrencySymbol
        {
            get
            {
                if (UseConfigCurrencySymbol)
                    return (string) this["currencySymbol"];

                return CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
            }
            set { this["currencySymbol"] = value; }
        }

        [ConfigurationProperty("currencySymbolPosition", DefaultValue = CurrencySymbolLayout.Leading, IsRequired = false
            , IsKey = false)]
        public CurrencySymbolLayout CurrencySymbolPosition
        {
            get { return (CurrencySymbolLayout) this["currencySymbolPosition"]; }
            set { this["currencySymbolPosition"] = value; }
        }

        /// <summary>
        ///     is there space between first and last name for recipent
        /// </summary>
        /// <summary>
        ///     If use HMS calc when check out
        /// </summary>
        [ConfigurationProperty("useHMSCalc", DefaultValue = true, IsRequired = false)]
        public bool UseHMSCalc
        {
            get
            {
                if (HLConfigManager.DefaultConfiguration.CheckoutConfiguration.OverrideHMSCalc)
                {
                    return false;
                }

                return (bool) this["useHMSCalc"];
            }
            set { this["useHMSCalc"] = value; }
        }

        /// <summary>
        ///     If a DO allows additional Shipto countries - get them here
        /// </summary>
        [ConfigurationProperty("shipToCountries", IsRequired = true)]
        public string ShipToCountries
        {
            get { return this["shipToCountries"] as string; }
            set { this["shipToCountries"] = value; }
        }

        /// <summary>
        ///     If use Sliding Scale
        /// </summary>
        [ConfigurationProperty("useSlidingScale", DefaultValue = true, IsRequired = false)]
        public bool UseSlidingScale
        {
            get { return (bool) this["useSlidingScale"]; }
            set { this["useSlidingScale"] = value; }
        }

        /// <summary>
        ///     Require Email
        /// </summary>
        [ConfigurationProperty("requireEmail", DefaultValue = true, IsRequired = false)]
        public bool RequireEmail
        {
            get { return (bool) this["requireEmail"]; }
            set { this["requireEmail"] = value; }
        }

        /// <summary>
        ///     Require SMS - Korea
        /// </summary>
        [ConfigurationProperty("requireSMS", DefaultValue = false, IsRequired = false)]
        public bool RequireSMS
        {
            get { return (bool) this["requireSMS"]; }
            set { this["requireSMS"] = value; }
        }

        /// <summary>
        ///     Regular expression to validate area code mobile
        /// </summary>
        [ConfigurationProperty("areaCodeMobileRegExp", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string AreaCodeMobileRegExp
        {
            get { return this["areaCodeMobileRegExp"] as string; }
            set { this["areaCodeMobileRegExp"] = value; }
        }

        /// <summary>
        ///     Regular expression to validate mobile number
        /// </summary>
        [ConfigurationProperty("mobileNumberRegExp", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string MobileNumberRegExp
        {
            get { return this["mobileNumberRegExp"] as string; }
            set { this["mobileNumberRegExp"] = value; }
        }

        /// <summary>
        ///     Require Email
        /// </summary>
        [ConfigurationProperty("initialShippingAddressFromHMS", DefaultValue = false, IsRequired = false)]
        public bool InitialShippingAddressFromHMS
        {
            get { return (bool) this["initialShippingAddressFromHMS"]; }
            set { this["initialShippingAddressFromHMS"] = value; }
        }

        /// <summary>
        ///     Require Email
        /// </summary>
        [ConfigurationProperty("shippingAddressRequiredForPickupOrder", DefaultValue = false, IsRequired = false)]
        public bool ShippingAddressRequiredForPickupOrder
        {
            get { return (bool) this["shippingAddressRequiredForPickupOrder"]; }
            set { this["shippingAddressRequiredForPickupOrder"] = value; }
        }

        [ConfigurationProperty("eventTicketOrderType", DefaultValue = "ETO", IsRequired = false)]
        public string EventTicketOrderType
        {
            get { return (string) this["eventTicketOrderType"]; }
            set { this["eventTicketOrderType"] = value; }
        }

        [ConfigurationProperty("eventTicketFreightCode", DefaultValue = "NOF", IsRequired = false)]
        public string EventTicketFreightCode
        {
            get { return (string) this["eventTicketFreightCode"]; }
            set { this["eventTicketFreightCode"] = value; }
        }

        [ConfigurationProperty("eventTicketWarehouseCode", DefaultValue = "", IsRequired = false)]
        public string EventTicketWarehouseCode
        {
            get { return (string) this["eventTicketWarehouseCode"]; }
            set { this["eventTicketWarehouseCode"] = value; }
        }

        [ConfigurationProperty("checkoutOptionsControl",
            DefaultValue = "~/Ordering/Controls/Checkout/CheckOutOptions.ascx", IsRequired = false, IsKey = false)]
        public string CheckoutOptionsControl
        {
            get { return (string) this["checkoutOptionsControl"]; }
            set { this["checkoutOptionsControl"] = value; }
        }

        [ConfigurationProperty("checkoutTotalsMiniControl",
            DefaultValue = "~/Ordering/Controls/Checkout/CheckoutTotalsMini.ascx", IsRequired = false, IsKey = false)]
        public string CheckoutTotalsMiniControl
        {
            get { return (string) this["checkoutTotalsMiniControl"]; }
            set { this["checkoutTotalsMiniControl"] = value; }
        }

        [ConfigurationProperty("checkoutTotalsDetailedControl",
            DefaultValue = "~/Ordering/Controls/Checkout/CheckoutTotalsDetailed.ascx", IsRequired = false, IsKey = false
            )]
        public string CheckoutTotalsDetailedControl
        {
            get { return (string) this["checkoutTotalsDetailedControl"]; }
            set { this["checkoutTotalsDetailedControl"] = value; }
        }

        [ConfigurationProperty("checkoutOrderSummary",
            DefaultValue = "~/Ordering/Controls/Checkout/CheckoutOrderSummary.ascx", IsRequired = false, IsKey = false)]
        public string CheckoutOrderSummary
        {
            get { return (string)this["checkoutOrderSummary"]; }
            set { this["checkoutOrderSummary"] = value; }
        }

        [ConfigurationProperty("deliveryMethodControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DeliveryMethodControl
        {
            get { return (string) this["deliveryMethodControl"]; }
            set { this["deliveryMethodControl"] = value; }
        }

        [ConfigurationProperty("invoiceOptionsControl",
            DefaultValue = "~/Ordering/Controls/Checkout/InvoiceOptions.ascx", IsRequired = false, IsKey = false)]
        public string InvoiceOptionsControl
        {
            get { return (string) this["invoiceOptionsControl"]; }
            set { this["invoiceOptionsControl"] = value; }
        }

        /// <summary>
        ///     To set the default invoice option
        /// </summary>
        [ConfigurationProperty("defaultInvoiceOption", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DefaultInvoiceOption
        {
            get { return (string) this["defaultInvoiceOption"]; }
            set { this["defaultInvoiceOption"] = value; }
        }

        /// <summary>
        /// To display the invoice control when no options
        /// </summary>
        [ConfigurationProperty("alwaysDisplayInvoiceOption", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool AlwaysDisplayInvoiceOption
        {
            get { return (bool)this["alwaysDisplayInvoiceOption"]; }
            set { this["alwaysDisplayInvoiceOption"] = value; }
        }

        //[ConfigurationProperty("emailOptionsControl", DefaultValue = "~/Ordering/Controls/Checkout/EmailOptions.ascx",
        //    IsRequired = false, IsKey = false)]
        //public string EmailOptionsControl
        //{
        //    get { return (string) this["emailOptionsControl"]; }
        //    set { this["emailOptionsControl"] = value; }
        //}

        [ConfigurationProperty("savePickupPreferences", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool SavePickupPreferences
        {
            get { return (bool) this["savePickupPreferences"]; }
            set { this["savePickupPreferences"] = value; }
        }

        [ConfigurationProperty("savePickupFromCourierPreferences", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool SavePickupFromCourierPreferences
        {
            get { return (bool) this["savePickupFromCourierPreferences"]; }
            set { this["savePickupFromCourierPreferences"] = value; }
        }

        [ConfigurationProperty("convertAmountDueOnImport", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ConvertAmountDueOnImport
        {
            get { return (bool)this["convertAmountDueOnImport"]; }
            set { this["convertAmountDueOnImport"] = value; }
        }

        [ConfigurationProperty("convertAmountDue", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ConvertAmountDue
        {
            get { return (bool) this["convertAmountDue"]; }
            set { this["convertAmountDue"] = value; }
        }

        [ConfigurationProperty("convertCurrencyFrom", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ConvertCurrencyFrom
        {
            get { return (string) this["convertCurrencyFrom"]; }
            set { this["convertCurrencyFrom"] = value; }
        }

        [ConfigurationProperty("convertCurrencyTo", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ConvertCurrencyTo
        {
            get { return (string) this["convertCurrencyTo"]; }
            set { this["convertCurrencyTo"] = value; }
        }

        [ConfigurationProperty("hasOtherCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasOtherCharges
        {
            get { return (bool) this["hasOtherCharges"]; }
            set { this["hasOtherCharges"] = value; }
        }

        [ConfigurationProperty("hasLogisticCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasLogisticCharges
        {
            get { return (bool) this["hasLogisticCharges"]; }
            set { this["hasLogisticCharges"] = value; }
        }

        [ConfigurationProperty("hasPickupCharge", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasPickupCharge
        {
            get { return (bool) this["hasPickupCharge"]; }
            set { this["hasPickupCharge"] = value; }
        }
        
        [ConfigurationProperty("hasEarnBase", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasEarnBase
        {
            get { return (bool) this["hasEarnBase"]; }
            set { this["hasEarnBase"] = value; }
        }

        [ConfigurationProperty("hasEarnBaseBySku", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasEarnBaseBySku
        {
            get { return (bool)this["hasEarnBaseBySku"]; }
            set { this["hasEarnBaseBySku"] = value; }
        }

        [ConfigurationProperty("hasRetailPrice", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasRetailPrice
        {
            get { return (bool) this["hasRetailPrice"]; }
            set { this["hasRetailPrice"] = value; }
        }

        /// <summary>
        /// Indicates when Retail Price is displayed in ETO order type.
        /// </summary>
        [ConfigurationProperty("hasRetailPriceForETO", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasRetailPriceForETO
        {
            get { return (bool)this["hasRetailPriceForETO"]; }
            set { this["hasRetailPriceForETO"] = value; }
        }

        [ConfigurationProperty("hideRetailPriceOnCOP1", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HideRetailPriceOnCOP1
        {
            get { return (bool) this["hideRetailPriceOnCOP1"]; }
            set { this["hideRetailPriceOnCOP1"] = value; }
        }

        [ConfigurationProperty("hideYourPriceOnCOP1", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HideYourPriceOnCOP1
        {
            get { return (bool) this["hideYourPriceOnCOP1"]; }
            set { this["hideYourPriceOnCOP1"] = value; }
        }

        /// <summary>
        ///     To show the earn base in the total summary only even hasEarnBase is false
        /// </summary>
        [ConfigurationProperty("hasSummaryEarnBase", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasSummaryEarnBase
        {
            get { return (bool) this["hasSummaryEarnBase"]; }
            set { this["hasSummaryEarnBase"] = value; }
        }

        [ConfigurationProperty("hasTaxVat", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasTaxVat
        {
            get { return (bool) this["hasTaxVat"]; }
            set { this["hasTaxVat"] = value; }
        }

        [ConfigurationProperty("hasTax", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasTax
        {
            get { return (bool) this["hasTax"]; }
            set { this["hasTax"] = value; }
        }

        [ConfigurationProperty("hasLogistics", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasLogistics
        {
            get { return (bool) this["hasLogistics"]; }
            set { this["hasLogistics"] = value; }
        }

        [ConfigurationProperty("useUSPricesFormat", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool UseUSPricesFormat
        {
            get { return (bool) this["useUSPricesFormat"]; }
            set { this["useUSPricesFormat"] = value; }
        }

        [ConfigurationProperty("useCommaWithoutDecimalFormat", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool UseCommaWithoutDecimalFormat
        {
            get { return (bool)this["useCommaWithoutDecimalFormat"]; }
            set { this["useCommaWithoutDecimalFormat"] = value; }
        }

        [ConfigurationProperty("hasLocalTax", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasLocalTax
        {
            get { return (bool) this["hasLocalTax"]; }
            set { this["hasLocalTax"] = value; }
        }

        [ConfigurationProperty("hasTotalDiscount", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasTotalDiscount
        {
            get { return (bool) this["hasTotalDiscount"]; }
            set { this["hasTotalDiscount"] = value; }
        }

        [ConfigurationProperty("hasTotalTaxable", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasTotalTaxable
        {
            get { return (bool) this["hasTotalTaxable"]; }
            set { this["hasTotalTaxable"] = value; }
        }

        [ConfigurationProperty("hasTaxPercentage", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasTaxPercentage
        {
            get { return (bool) this["hasTaxPercentage"]; }
            set { this["hasTaxPercentage"] = value; }
        }

        [ConfigurationProperty("taxPercentage", DefaultValue = "18", IsRequired = false, IsKey = false)]
        public string TaxPercentage
        {
            get { return (string) this["taxPercentage"]; }
            set { this["taxPercentage"] = value; }
        }

        [ConfigurationProperty("hasYourPrice", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasYourPrice
        {
            get { return (bool) this["hasYourPrice"]; }
            set { this["hasYourPrice"] = value; }
        }

        [ConfigurationProperty("hasSubTotal", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasSubTotal
        {
            get { return (bool) this["hasSubTotal"]; }
            set { this["hasSubTotal"] = value; }
        }

        [ConfigurationProperty("hasSubTotalOnTotalsDetailed", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasSubTotalOnTotalsDetailed
        {
            get { return (bool) this["hasSubTotalOnTotalsDetailed"]; }
            set { this["hasSubTotalOnTotalsDetailed"] = value; }
        }

        [ConfigurationProperty("hasDiscountAmount", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasDiscountAmount
        {
            get { return (bool)this["hasDiscountAmount"]; }
            set { this["hasDiscountAmount"] = value; }
        }

        [ConfigurationProperty("calculateSubtotal", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool CalculateSubtotal
        {
            get { return (bool) this["calculateSubtotal"]; }
            set { this["calculateSubtotal"] = value; }
        }

        [ConfigurationProperty("displaySubTotalOnMinicart", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool DisplaySubTotalOnMinicart
        {
            get { return (bool) this["displaySubTotalOnMinicart"]; }
            set { this["displaySubTotalOnMinicart"] = value; }
        }

        /// <summary>
        ///     To hide PH and shipping on ETO
        /// </summary>
        [ConfigurationProperty("hidePHShippingForETO", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HidePHShippingForETO
        {
            get { return (bool) this["hidePHShippingForETO"]; }
            set { this["hidePHShippingForETO"] = value; }
        }

        /// <summary>
        /// Indicates to hide the shipping charges.
        /// </summary>
        [ConfigurationProperty("hideShippingCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HideShippingCharges
        {
            get { return (bool)this["hideShippingCharges"]; }
            set { this["hideShippingCharges"] = value; }
        }

        /// <summary>
        ///     To show VolumePoints if hidePHShippingForETO
        /// </summary>
        [ConfigurationProperty("showVolumePoinsForETO", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowVolumePoinsForETO
        {
            get { return (bool) this["showVolumePoinsForETO"]; }
            set { this["showVolumePoinsForETO"] = value; }
        }

        [ConfigurationProperty("showDisocuntTotal", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowDisocuntTotal
        {
            get { return (bool)this["showDisocuntTotal"]; }
            set { this["showDisocuntTotal"] = value; }
        }

        [ConfigurationProperty("yourPriceWithAllCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool YourPriceWithAllCharges
        {
            get { return (bool) this["yourPriceWithAllCharges"]; }
            set { this["yourPriceWithAllCharges"] = value; }
        }

        [ConfigurationProperty("hasVolumePoints", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasVolumePoints
        {
            get { return (bool) this["hasVolumePoints"]; }
            set { this["hasVolumePoints"] = value; }
        }

        [ConfigurationProperty("hasOrderMonthVolumePoints", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasOrderMonthVolumePoints
        {
            get { return (bool)this["hasOrderMonthVolumePoints"]; }
            set { this["hasOrderMonthVolumePoints"] = value; }
        }

        [ConfigurationProperty("getShippingInstructionsFromProvider", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool GetShippingInstructionsFromProvider
        {
            get { return (bool) this["getShippingInstructionsFromProvider"]; }
            set { this["getShippingInstructionsFromProvider"] = value; }
        }

        [ConfigurationProperty("modifyRecipientName", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ModifyRecipientName
        {
            get { return (bool) this["modifyRecipientName"]; }
            set { this["modifyRecipientName"] = value; }
        }

        [ConfigurationProperty("displayWireReferenceNumber", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayWireReferenceNumber
        {
            get { return (bool) this["displayWireReferenceNumber"]; }
            set { this["displayWireReferenceNumber"] = value; }
        }

        [ConfigurationProperty("populateHMSPrimaryEMail", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool PopulateHMSPrimaryEMail
        {
            get { return (bool) this["populateHMSPrimaryEMail"]; }
            set { this["populateHMSPrimaryEMail"] = value; }
        }

        [ConfigurationProperty("enforcesShippingMethodRules", DefaultValue = "false", IsRequired = false, IsKey = false)
        ]
        public bool EnforcesShippingMethodRules
        {
            get { return (bool) this["enforcesShippingMethodRules"]; }
            set { this["enforcesShippingMethodRules"] = value; }
        }

        /// <summary>
        ///     The payments must be pre-authenticated in order to submit order
        /// </summary>
        [ConfigurationProperty("requiresPaymentAuthenticationToSubmit", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool RequiresPaymentAuthenticationToSubmit
        {
            get { return (bool) this["requiresPaymentAuthenticationToSubmit"]; }
            set { this["requiresPaymentAuthenticationToSubmit"] = value; }
        }

        /// <summary>
        ///     The payments must be pre-authenticated in order to submit order
        /// </summary>
        [ConfigurationProperty("requiresAcknowledgementToSubmit", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool RequiresAcknowledgementToSubmit
        {
            get { return (bool) this["requiresAcknowledgementToSubmit"]; }
            set { this["requiresAcknowledgementToSubmit"] = value; }
        }

        /// <summary>
        ///     For wire payment, acknowledge checkbox requires user action
        /// </summary>
        [ConfigurationProperty("requiresAcknowledgementToSubmitWireOnly", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool RequiresAcknowledgementToSubmitWireOnly
        {
            get { return (bool) this["requiresAcknowledgementToSubmitWireOnly"]; }
            set { this["requiresAcknowledgementToSubmitWireOnly"] = value; }
        }

        /// <summary>
        ///     For payment gateway, acknowledge checkbox requires user action
        /// </summary>
        [ConfigurationProperty("requiresAcknowledgementToSubmitGatewayOnly", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool RequiresAcknowledgementToSubmitGatewayOnly
        {
            get { return (bool) this["requiresAcknowledgementToSubmitGatewayOnly"]; }
            set { this["requiresAcknowledgementToSubmitGatewayOnly"] = value; }
        }

        /// <summary>
        ///     The terms and conditions must be accepted in order to submit order.
        /// </summary>
        [ConfigurationProperty("requiresAcknowledgementTermsToSubmit", DefaultValue = "false", IsRequired = false,
            IsKey = false)]
        public bool RequiresAcknowledgementTermsToSubmit
        {
            get { return (bool)this["requiresAcknowledgementTermsToSubmit"]; }
            set { this["requiresAcknowledgementTermsToSubmit"] = value; }
        }

        /// <summary>
        ///     Allow to change the order category, this affects only to RU
        /// </summary>
        [ConfigurationProperty("useExtendedOrderCategory", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool UseExtendedOrderCategory
        {
            get { return (bool) this["useExtendedOrderCategory"]; }
            set { this["useExtendedOrderCategory"] = value; }
        }

        /// <summary>
        ///     special SKUs
        /// </summary>
        [ConfigurationProperty("specialSKUList", DefaultValue = "", IsRequired = false, IsKey = false),
         TypeConverter(typeof (StringListConverter))]
        public List<string> SpecialSKUList
        {
            get { return this["specialSKUList"] as List<string>; }
            set { this["specialSKUList"] = value; }
        }

        /// <summary>
        ///  freight SKUs
        /// </summary>
        [ConfigurationProperty("freeFreightSKUList", DefaultValue = "", IsRequired = false, IsKey = false),
         TypeConverter(typeof(StringListConverter))]
        public List<string> FreeFreightSKUList
        {
            get { return this["freeFreightSKUList"] as List<string>; }
            set { this["freeFreightSKUList"] = value; }
        }

        /// <summary>
        ///     The address need to be confirmed
        /// </summary>
        [ConfigurationProperty("requiresAddressConfirmation", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool RequiresAddressConfirmation
        {
            get { return (bool) this["requiresAddressConfirmation"]; }
            set { this["requiresAddressConfirmation"] = value; }
        }

        /// <summary>
        ///     To show the range for volume points.
        ///     When it is showing, the discount percentage is hidding.
        /// </summary>
        [ConfigurationProperty("showVolumePointRange", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowVolumePointRange
        {
            get { return (bool) this["showVolumePointRange"]; }
            set { this["showVolumePointRange"] = value; }
        }

        /// <summary>
        ///     To show the help icon in delete items CO1 page.
        /// </summary>
        [ConfigurationProperty("showDeleteItemsHelp", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowDeleteItemsHelp
        {
            get { return (bool) this["showDeleteItemsHelp"]; }
            set { this["showDeleteItemsHelp"] = value; }
        }

        /// <summary>
        ///     if country allows zero discount
        /// </summary>
        [ConfigurationProperty("allowZeroDiscount", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool AllowZeroDiscount
        {
            get { return (bool) this["allowZeroDiscount"]; }
            set { this["allowZeroDiscount"] = value; }
        }

        [ConfigurationProperty("displayCourierDetails", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayCourierDetails
        {
            get { return (bool)this["displayCourierDetails"]; }
            set { this["displayCourierDetails"] = value; }
        }

        [ConfigurationProperty("showDistributorSubTotalForETO", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ShowDistributorSubTotalForETO
        {
            get { return (bool)this["showDistributorSubTotalForETO"]; }
            set { this["showDistributorSubTotalForETO"] = value; }
        }

        /// <summary>
        /// When show an error if there is no delivery option to display
        /// </summary>
        [ConfigurationProperty("errorNoDeliveryOption", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool ErrorNoDeliveryOption
        {
            get { return (bool)this["errorNoDeliveryOption"]; }
            set { this["errorNoDeliveryOption"] = value; }
        }

        /// <summary>
        /// Checks any Pending Order in payment gateway
        /// </summary>
        [ConfigurationProperty("checkPaymentPendingOrder", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool CheckPaymentPendingOrder
        {
            get { return (bool)this["checkPaymentPendingOrder"]; }
            set { this["checkPaymentPendingOrder"] = value; }
        }

        /// <summary>
        /// Display Tell Your Story 
        /// </summary>
        [ConfigurationProperty("displayTellYourStory", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayTellYourStory
        {
            get { return (bool)this["displayTellYourStory"]; }
            set { this["displayTellYourStory"] = value; }
        }
        /// <summary>
        /// Display Transaction time on confirmation page
        /// </summary>
        [ConfigurationProperty("displayTransactiontime", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayTransactionTime
        {
            get { return (bool)this["displayTransactiontime"]; }
            set { this["displayTransactiontime"] = value; }
        }

        /// <summary>
        /// Flag to display the message in the checkout order summary control.
        /// </summary>
        [ConfigurationProperty("hasSummaryMessage", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasSummaryMessage
        {
            get { return (bool)this["hasSummaryMessage"]; }
            set { this["hasSummaryMessage"] = value; }
        }

        /// <summary>
        /// Flag to display shipping instructions in ShoppingCart page in CheckOutOptions control.
        /// </summary>
        [ConfigurationProperty("hasShippingInstructionsMessage", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HasShippingInstructionsMessage
        {
            get { return (bool)this["hasShippingInstructionsMessage"]; }
            set { this["hasShippingInstructionsMessage"] = value; }
        }
        /// <summary>
        /// Flag to display Volume point culture specific decimal format
        /// </summary>
        [ConfigurationProperty("displayFormatNeedsDecimal", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayFormatNeedsDecimal
        {
            get { return (bool)this["displayFormatNeedsDecimal"]; }
            set { this["displayFormatNeedsDecimal"] = value; }
        }


        [ConfigurationProperty("includePUNameForPUFromCourier", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool IncludePUNameForPUFromCourier
        {
            get { return (bool)this["includePUNameForPUFromCourier"]; }
            set { this["includePUNameForPUFromCourier"] = value; }
        }

        #region HR
               
        /// <summary>
        /// Flag to add P&H and Shipping charges and show one label on UI
        /// </summary>
        [ConfigurationProperty("mergePHAndShippingCharges", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool MergePHAndShippingCharges
        {
            get { return (bool)this["mergePHAndShippingCharges"]; }
            set { this["mergePHAndShippingCharges"] = value; }
        }

        #endregion

        /// <summary>
        /// Hide Statement on COP1 - ShoppingCart.aspx
        /// </summary>
        [ConfigurationProperty("HideStatementOnCOP1", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HideStatementOnCOP1
        {
            get { return (bool)this["HideStatementOnCOP1"]; }
            set { this["HideStatementOnCOP1"] = value; }
        }


        /// <summary>
        /// Fraud control Enabled
        /// </summary>
        [ConfigurationProperty("fraudControlEnabled", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool FraudControlEnabled
        {
            get { return (bool)this["fraudControlEnabled"]; }
            set { this["fraudControlEnabled"] = value; }
        }

        /// <summary>
        /// Kount Enabled
        /// </summary>
        [ConfigurationProperty("KountEnabled", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool KountEnabled
        {
            get
            {
                var gdoConfig = HlCountryConfigurationProvider.GetCountryConfiguration(Locale);
                if (gdoConfig != null && gdoConfig.FControlSettings!=null)
                {
                    return gdoConfig.FControlSettings.Enabled;
                }

                try
                {
                    var value = this["KountEnabled"];
                    if (null != value)
                    {
                        return (bool)value;
                    }

                    value = HLConfigManager.DefaultConfiguration.CheckoutConfiguration.KountEnabled;
                    return (bool)value;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("Configuration", ex, "KountEnabled");
                }

                return false;
            }
            //get { return (bool)this["KountEnabled"]; }
            set { this["KountEnabled"] = value; }
        }

        /// <summary>
        /// MPC Fraud 
        /// </summary>
        [ConfigurationProperty("holdPickupOrder", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool HoldPickupOrder
        {
            get { return (bool)this["holdPickupOrder"]; }
            set { this["holdPickupOrder"] = value; }
        }

        /// <summary>
        /// First Order Installment Logic enable
        /// </summary>
        [ConfigurationProperty("FirstOrderInstallmentLogic", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool FirstOrderInstallmentLogic
        {
            get { return (bool)this["FirstOrderInstallmentLogic"]; }
            set { this["FirstOrderInstallmentLogic"] = value; }
        }

        /// <summary>
        /// Defines the default invoice option for customer invoices in CO-DO process.
        /// Default value: RecycleInvoice
        /// </summary>
        [ConfigurationProperty("defaultCustomerInvoiceOption", DefaultValue = "RecycleInvoice", IsRequired = false, IsKey = false)]
        public string DefaultCustomerInvoiceOption
        {
            get { return (string)this["defaultCustomerInvoiceOption"]; }
            set { this["defaultCustomerInvoiceOption"] = value; }
        }

        /// <summary>
        /// Control to display on COP1 and COP2 for HAP Orders.
        /// </summary>
        [ConfigurationProperty("checkOutHAPOptionsControl", DefaultValue = "~/Ordering/Controls/HAP/HAPCheckoutOptions.ascx", IsRequired = false, IsKey = false)]
        public string CheckOutHAPOptionsControl
        {
            get { return (string)this["checkOutHAPOptionsControl"]; }
            set { this["checkOutHAPOptionsControl"] = value; }
        }

        /// <summary>
        /// To display the weight in the COP.
        /// </summary>
        [ConfigurationProperty("displayWeight", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisplayWeight
        {
            get { return (bool)this["displayWeight"]; }
            set { this["displayWeight"] = value; }
        }

        [ConfigurationProperty("CheckOutMessageNotify", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool CheckOutMessageNotify
        {
            get { return (bool)this["CheckOutMessageNotify"]; }
            set { this["CheckOutMessageNotify"] = value; }
        }
        [ConfigurationProperty("CheckOutShowNotification", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool CheckOutShowNotification
        {
            get { return (bool)this["CheckOutShowNotification"]; }
            set { this["CheckOutShowNotification"] = value; }
        }

        [ConfigurationProperty("disableDefaultInvoiceOption", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool DisableDefaultInvoiceOption
        {
            get { return (bool)this["disableDefaultInvoiceOption"]; }
            set { this["disableDefaultInvoiceOption"] = value; }
        }

        [ConfigurationProperty("requiresInvoiceOption", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool RequiresInvoiceOption
        {
            get { return (bool)this["requiresInvoiceOption"]; }
            set { this["requiresInvoiceOption"] = value; }
        }
        #endregion Config Properties
    }
}