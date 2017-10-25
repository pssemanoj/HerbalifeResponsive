using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class APFConfiguration : HLConfiguration
    {
        /// <summary>
        ///     specify distributors processing fee
        ///     format [distributors processing fee],[supervisor processing fee]
        /// </summary>
        [ConfigurationProperty("supervisorSku", DefaultValue = "9909", IsRequired = true, IsKey = false)]
        public string SupervisorSku
        {
            get { return (string) this["supervisorSku"]; }
            set { this["supervisorSku"] = value; }
        }

        /// <summary>
        ///     specify distributors processing fee
        ///     format [distributors processing fee],[supervisor processing fee]
        /// </summary>
        [ConfigurationProperty("distributorSku", DefaultValue = "0909", IsRequired = true, IsKey = false)]
        public string DistributorSku
        {
            get { return (string) this["distributorSku"]; }
            set { this["distributorSku"] = value; }
        }

        /// <summary>
        /// Specifies an alternative APF sku.
        /// </summary>
        [ConfigurationProperty("alternativeSku", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string AlternativeSku
        {
            get { return (string)this["alternativeSku"]; }
            set { this["alternativeSku"] = value; }
        }

        [ConfigurationProperty("apfwarehouse", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string APFwarehouse
        {
            get { return (string) this["apfwarehouse"]; }
            set { this["apfwarehouse"] = value; }
        }

        [ConfigurationProperty("initalApfwarehouse", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string InitialAPFwarehouse
        {
            get { return (string) this["initalApfwarehouse"]; }
            set { this["initalApfwarehouse"] = value; }
        }

        [ConfigurationProperty("apfFreightCode", DefaultValue = "NOF")]
        public string APFFreightCode
        {
            get { return (string) this["apfFreightCode"]; }
            set { this["apfFreightCode"] = value; }
        }

        [ConfigurationProperty("allowAddItemWhenAPFDue")]
        public bool AllowAddItemWhenAPFDue
        {
            get { return (bool) this["allowAddItemWhenAPFDue"]; }
            set { this["allowAddItemWhenAPFDue"] = value; }
        }

        [ConfigurationProperty("allowAddAPF")]
        public bool AllowAddAPF
        {
            get { return (bool) this["allowAddAPF"]; }
            set { this["allowAddAPF"] = value; }
        }

        [ConfigurationProperty("allowDSRemoveAPFWhenDue")]
        public bool AllowDSRemoveAPFWhenDue
        {
            get { return (bool) this["allowDSRemoveAPFWhenDue"]; }
            set { this["allowDSRemoveAPFWhenDue"] = value; }
        }

        [ConfigurationProperty("allowNonProductItemsWithStandaloneAPF", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool AllowNonProductItemsWithStandaloneAPF
        {
            get { return (bool) this["allowNonProductItemsWithStandaloneAPF"]; }
            set { this["allowNonProductItemsWithStandaloneAPF"] = value; }
        }

        [ConfigurationProperty("standaloneAPFOnlyAllowed", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool StandaloneAPFOnlyAllowed
        {
            get { return (bool) this["standaloneAPFOnlyAllowed"]; }
            set { this["standaloneAPFOnlyAllowed"] = value; }
        }

        [ConfigurationProperty("apfRequired", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool APFRequired
        {
            get { return (bool) this["apfRequired"]; }
            set { this["apfRequired"] = value; }
        }

        [ConfigurationProperty("apfExemptCountriesOfProcessing", DefaultValue = ""),
         TypeConverter(typeof (StringListConverter))]
        public List<string> ApfExemptCountriesOfProcessing
        {
            get
            {
                var items = this["apfExemptCountriesOfProcessing"] as List<string>;
                if (items.Count == 0)
                {
                    items =
                        new StringListConverter().ConvertFromString(
                            ConfigurationManager.AppSettings["ApfExemptCountriesOfProcessing"]) as List<string>;
                }

                return items;
            }
            set { this["apfExemptCountriesOfProcessing"] = value; }
        }

        [ConfigurationProperty("apfRestrictedByPurchaseLocation", DefaultValue = ""),
       TypeConverter(typeof(StringListConverter))]
        public List<string> ApfRestrictedByPurchaseLocation
        {
            get
            {
                var items = this["apfRestrictedByPurchaseLocation"] as List<string>;
                if (items.Count == 0)
                {
                    items =
                        new StringListConverter().ConvertFromString(
                            ConfigurationManager.AppSettings["ApfRestrictedByPurchaseLocation"]) as List<string>;
                }

                return items;
            }
            set { this["apfRestrictedByPurchaseLocation"] = value; }
        }

        [ConfigurationProperty("dueDateDisplayFormat", DefaultValue = "MMM d, yyyy", IsRequired = false, IsKey = false)]
        public string DueDateDisplayFormat
        {
            get { return (string) this["dueDateDisplayFormat"]; }
            set { this["dueDateDisplayFormat"] = value; }
        }

        [ConfigurationProperty("orderType", DefaultValue = "RSO", IsRequired = false, IsKey = false)]
        public string OrderType
        {
            get { return (string) this["orderType"]; }
            set { this["orderType"] = value; }
        }

        [ConfigurationProperty("showOrderQuickViewForStandaloneAPF", DefaultValue = true, IsRequired = false,
            IsKey = false)]
        public bool ShowOrderQuickViewForStandaloneAPF
        {
            get { return (bool) this["showOrderQuickViewForStandaloneAPF"]; }
            set { this["showOrderQuickViewForStandaloneAPF"] = value; }
        }

        /// <summary>
        /// The only allowed delivery option.
        /// If this parameter has a value, it will be the only option displayed in delivery option.
        /// </summary>
        [ConfigurationProperty("deliveryAllowed", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string DeliveryAllowed
        {
            get { return (string)this["deliveryAllowed"]; }
            set { this["deliveryAllowed"] = value; }
        }


        [ConfigurationProperty("cantDSRemoveAPF", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool CantDSRemoveAPF
        {
            get { return (bool)this["cantDSRemoveAPF"]; }
            set { this["cantDSRemoveAPF"] = value; }
        }
        [ConfigurationProperty("apfisApplicable", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool APFisApplicable
        {
            get { return (bool)this["apfisApplicable"]; }
            set { this["apfisApplicable"] = value; }
        }

        public static APFConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "APF") as APFConfiguration;
        }

        [ConfigurationProperty("hasExtendedLevelNotAllowToRemoveAPF", DefaultValue = false, IsRequired = false, IsKey = false)]
        public bool HasExtendedLevelNotAllowToRemoveAPF
        {
            get { return (bool)this["hasExtendedLevelNotAllowToRemoveAPF"]; }
            set { this["hasExtendedLevelNotAllowToRemoveAPF"] = value; }
        }

        [ConfigurationProperty("dsLevelNotAllowToRemoveAPF", DefaultValue = "SP,", IsRequired = false, IsKey = false)]
        public string DsLevelNotAllowToRemoveAPF
        {
            get { return (string)this["dsLevelNotAllowToRemoveAPF"]; }
            set { this["dsLevelNotAllowToRemoveAPF"] = value; }
        }
    }
}