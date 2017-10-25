using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class OCPaymentsConfiguration : HLConfiguration
    {
        [ConfigurationProperty("Validations", IsDefaultCollection = true),
         ConfigurationCollection(typeof (OCPaymentsValidationCollection),
             AddItemName = "Field")]
        public OCPaymentsValidationCollection Validations
        {
            get { return this["Validations"] as OCPaymentsValidationCollection; }
        }

        public static OCPaymentsConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "OCPayments") as OCPaymentsConfiguration;
        }
    }

    public class CountryValidation : ConfigurationElement
    {
        public CountryValidation()
        {
        }

        public CountryValidation(string name, bool isVisible, bool isRequired, string length)
        {
            Name = name;
            IsVisible = isVisible;
            IsRequired = isRequired;
            Length = length;
        }

        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get { return this["Name"].ToString(); }
            set { this["Name"] = value; }
        }

        [ConfigurationProperty("IsRequired", IsRequired = true)]
        public bool IsRequired
        {
            get { return (bool) this["IsRequired"]; }
            set { this["IsRequired"] = value; }
        }

        [ConfigurationProperty("IsVisible", IsRequired = true)]
        public bool IsVisible
        {
            get { return (bool) this["IsVisible"]; }
            set { this["IsVisible"] = value; }
        }

        [ConfigurationProperty("Length", IsRequired = true)]
        public string Length
        {
            get { return this["Length"].ToString(); }
            set { this["Length"] = value; }
        }

        [ConfigurationProperty("IsNumeric", IsRequired = false)]
        public bool IsNumeric
        {
            get { return (bool) this["IsNumeric"]; }
            set { this["IsNumeric"] = value; }
        }

        [ConfigurationProperty("ExactLength", IsRequired = false)]
        public string ExactLength
        {
            get { return this["ExactLength"].ToString(); }
            set { this["ExactLength"] = value; }
        }

        [ConfigurationProperty("NumZero", IsRequired = false)]
        public string NumZero
        {
            get { return this["NumZero"].ToString(); }
            set { this["NumZero"] = value; }
        }

        [ConfigurationProperty("RegularExpression", IsRequired = false)]
        public string RegularExpression
        {
            get { return this["RegularExpression"].ToString(); }
            set { this["RegularExpression"] = value; }
        }

        [ConfigurationProperty("ErrReq", IsRequired = false)]
        public string ErrReq
        {
            get { return this["ErrReq"].ToString(); }
            set { this["ErrReq"] = value; }
        }

        [ConfigurationProperty("ErrNum", IsRequired = false)]
        public string ErrNum
        {
            get { return this["ErrNum"].ToString(); }
            set { this["ErrNum"] = value; }
        }

        [ConfigurationProperty("ErrZero", IsRequired = false)]
        public string ErrZero
        {
            get { return this["ErrZero"].ToString(); }
            set { this["ErrZero"] = value; }
        }


        [ConfigurationProperty("CustomErrRegex", IsRequired = false)]
        public string CustomErrRegex
        {
            get { return this["CustomErrRegex"].ToString(); }
            set { this["CustomErrRegex"] = value; }
        }

        [ConfigurationProperty("ErrRegex", IsRequired = false)]
        public string ErrRegex
        {
            get { return this["ErrRegex"].ToString(); }
            set { this["ErrRegex"] = value; }
        }

        [ConfigurationProperty("AutoFill", IsRequired = false)]
        public bool AutoFill
        {
            get { return (bool) this["AutoFill"]; }
            set { this["AutoFill"] = value; }
        }

        [ConfigurationProperty("AutoFillValue", IsRequired = false)]
        public string AutoFillValue
        {
            get { return this["AutoFillValue"].ToString(); }
            set { this["AutoFillValue"] = value; }
        }

        //CMR 15167 : added valication for Minimum and Maximum lengths
        [ConfigurationProperty("MinLength", IsRequired = false)]
        public string MinLength
        {
            get { return this["MinLength"].ToString(); }
            set { this["MinLength"] = value; }
        }

        [ConfigurationProperty("MaxLength", IsRequired = false)]
        public string MaxLength
        {
            get { return this["MaxLength"].ToString(); }
            set { this["MaxLength"] = value; }
        }
    }

    public class OCPaymentsValidationCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public CountryValidation this[int index]
        {
            get { return (CountryValidation) BaseGet(index); }
            set
            {
                if (null != BaseGet(index))
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(CountryValidation element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CountryValidation();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CountryValidation) element).Name;
        }

        public void Remove(CountryValidation element)
        {
            BaseRemove((element).Name);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}