using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class FormattingConfiguration : HLConfiguration
    {
        public static FormattingConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "FormattingConfiguration") as FormattingConfiguration;
            //return (FormattingConfiguration)GetConfiguration(config, "FormattingConfiguration");
        }

        #region Props

        //[ConfigurationProperty("formatShortDate", DefaultValue = true, IsRequired = false, IsKey = false)]
        //public string formatShortDate
        //{
        //    get
        //    {
        //        return (string)this["formatShortDate"];
        //    }
        //    set
        //    {
        //        this["formatShortDate"] = value;
        //    }
        //}

        [ConfigurationProperty("decimalSeparatorCharacter")]
        public string DecimalSeparatorCharacter
        {
            get
            {
                return this["decimalSeparatorCharacter"] == null ? "." : this["decimalSeparatorCharacter"].ToString();
                    // Default to . since most countries use it
            }
            set { this["decimalSeparatorCharacter"] = value; }
        }

        [ConfigurationProperty("allowMultipleCardsInTransaction")]
        public bool AllowMultipleCardsInTransaction
        {
            get { return (bool) this["allowMultipleCardsInTransaction"]; }
            set { this["allowMultipleCardsInTransaction"] = value; }
        }

        #endregion
    }
}