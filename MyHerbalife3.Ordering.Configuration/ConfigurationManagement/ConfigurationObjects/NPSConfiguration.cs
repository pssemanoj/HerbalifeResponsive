using System;
using System.Configuration;
using System.Threading;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class NPSConfiguration : HLConfiguration
    {
        #region Construction

        public static DOConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "NPS") as DOConfiguration;
        }

        #endregion Construction

        #region Config Properties
        /// <summary>
        ///     to get the resx key for pickup and handling text
        /// </summary>
        [ConfigurationProperty("pickupChargeResxKey", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupChargeResxKey
        {
            get { return (string)this["pickupChargeResxKey"]; }
            set { this["pickupChargeResxKey"] = value; }
        }

        /// <summary>
        /// if has packing and handling charge
        /// </summary>
        [ConfigurationProperty("hasPHCharge", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasPHCharge
        {
            get { return (bool)this["hasPHCharge"]; }
            set { this["hasPHCharge"] = value; }
        }

        /// <summary>
        ///     this is the starting date when PH charge should be removed from GDO, yyyy-mm-dd
        /// </summary>
        [ConfigurationProperty("removePHChargeStartDate", DefaultValue = "2063-02-19", IsRequired = false, IsKey = false)]
        public string RemovePHChargeStartDate
        {
            get { return (string)this["removePHChargeStartDate"]; }
            set { this["removePHChargeStartDate"] = value; }
        }
        #endregion Config Properties
    }
}
