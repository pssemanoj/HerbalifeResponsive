using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class AddressingConfiguration : HLConfiguration
    {
        #region Consts

        private const string CURR_SHIPPINGINFO = "CurrentShippingInfo_";
        private const string CURR_PAYMENTINFO = "CurrentPaymentInfo_";

        #endregion

        #region Construction

        public static AddressingConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "Addressing") as AddressingConfiguration;
        }

        #endregion

        #region Static methods

        public static string GetCurrentShippingSessionKey(string locale, string distributorID)
        {
            return CURR_SHIPPINGINFO + locale + "_" + distributorID;
        }

        public static string GetCurrentPaymentSessionKey(string locale, string distributorID)
        {
            return CURR_PAYMENTINFO + locale + "_" + distributorID;
        }

        #endregion

        #region Config Properties

        [ConfigurationProperty("GDOeditaddress", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string GDOEditAddress
        {
            get { return (string) this["GDOeditaddress"]; }
            set { this["GDOeditaddress"] = value; }
        }

        //[ConfigurationProperty("addressxml", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string AddressXML
        //{
        //    get
        //    {
        //        return (string)this["addressxml"];
        //    }
        //    set
        //    {
        //        this["addressxml"] = value;
        //    }
        //}
        //[ConfigurationProperty("editAddressxml", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string EditAddressXML
        //{
        //    get
        //    {
        //        return (string)this["editAddressxml"];
        //    }
        //    set
        //    {
        //        this["editAddressxml"] = value;
        //    }
        //}

        [ConfigurationProperty("GDOeditAddressxml", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string GDOEditAddressxml
        {
            get { return (string) this["GDOeditAddressxml"]; }
            set { this["GDOeditAddressxml"] = value; }
        }

        //[ConfigurationProperty("shippingBoxAddress", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string ShippingBoxAddress
        //{
        //    get
        //    {
        //        return (string)this["shippingBoxAddress"];
        //    }
        //    set
        //    {
        //        this["shippingBoxAddress"] = value;
        //    }
        //}
        [ConfigurationProperty("validatePostalCode", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ValidatePostalCode
        {
            get { return (bool) this["validatePostalCode"]; }
            set { this["validatePostalCode"] = value; }
        }
        [ConfigurationProperty("hasAddressRestriction", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasAddressRestriction
        {
            get { return (bool)this["hasAddressRestriction"]; }
            set { this["hasAddressRestriction"] = value; }
        }
        [ConfigurationProperty("hasAddressRestrictionLimit", DefaultValue = 0, IsRequired = false, IsKey = false)]
        public int  HasAddressRestrictionLimit
        {
            get { return (int)this["hasAddressRestrictionLimit"]; }
            set { this["hasAddressRestrictionLimit"] = value; }
        }

        [ConfigurationProperty("scriptPath", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ScriptPath
        {
            get { return (string) this["scriptPath"]; }
            set { this["scriptPath"] = value; }
        }

        //[ConfigurationProperty("displayShippingLabelAtCheckout", DefaultValue = true, IsRequired = false, IsKey = false)]
        //public bool DisplayShippingLabelAtCheckout
        //{
        //    get
        //    {
        //        return (bool)this["displayShippingLabelAtCheckout"];
        //    }
        //    set
        //    {
        //        this["displayShippingLabelAtCheckout"] = value;
        //    }
        //}
        //[ConfigurationProperty("gridviewCellAddress", DefaultValue = "", IsRequired = false, IsKey = false)]
        //public string GridviewCellAddress
        //{
        //    get
        //    {
        //        return (string)this["gridviewCellAddress"];
        //    }
        //    set
        //    {
        //        this["gridviewCellAddress"] = value;
        //    }
        //}

        [ConfigurationProperty("shippingControl",
            DefaultValue = "/Ordering/Controls/Shipping/AddEditShippingControl.ascx", IsRequired = false, IsKey = false)
        ]
        public string ShippingControl
        {
            get { return (string) this["shippingControl"]; }
            set { this["shippingControl"] = value; }
        }

        [ConfigurationProperty("pickupControl", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupControl
        {
            get { return (string) this["pickupControl"]; }
            set { this["pickupControl"] = value; }
        }

        [ConfigurationProperty("GDOstaticAddress",
            DefaultValue = "/Ordering/Controls/Address/AddressFormat/StaticAddress.xml", IsRequired = false,
            IsKey = false)]
        public string GDOStaticAddress
        {
            get { return (string) this["GDOstaticAddress"]; }
            set { this["GDOstaticAddress"] = value; }
        }

        [ConfigurationProperty("validateDSFraud", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ValidateDSFraud
        {
            get { return (bool) this["validateDSFraud"]; }
            set { this["validateDSFraud"] = value; }
        }
        [ConfigurationProperty("validateStreetAddress", DefaultValue = false, IsRequired = false, IsKey = false)]
        public  bool ValidateStreetAddress
        {
            get { return (bool)this["validateStreetAddress"]; }
            set { this["validateStreetAddress"] = value; }
        }

        /// <summary>
        /// Validate the shipping address in shopping cart page.
        /// </summary>
        [ConfigurationProperty("validateShippingAddress", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool ValidateShippingAddress
        {
            get { return (bool)this["validateShippingAddress"]; }
            set { this["validateShippingAddress"] = value; }
        }

        /// <summary>
        /// Indicates if the error expression should be displayed under each control or in a bullet point at the end of the page.
        /// </summary>
        [ConfigurationProperty("hasCustomErrorExpression", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasCustomErrorExpression
        {
            get { return (bool)this["hasCustomErrorExpression"]; }
            set { this["hasCustomErrorExpression"] = value; }
        }

        /// <summary>
        /// Enable KR Third party API to get the address data
        /// </summary>
        [ConfigurationProperty("KRAdressAPIEnabled", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool KRAdressAPIEnabled
        {
            get { return (bool)this["KRAdressAPIEnabled"]; }
            set { this["KRAdressAPIEnabled"] = value; }
        }
        [ConfigurationProperty("hasHerbalifePickupFreightChange", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasHerbalifePickupFreightChnage
        {
            get { return (bool)this["hasHerbalifePickupFreightChange"]; }
            set { this["hasHerbalifePickupFreightChange"] = value; }
        }

        #endregion
    }
}