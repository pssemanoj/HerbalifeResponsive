using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;


namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    public class FreeSKU
    {
        public string SKU { get; set; }
        public int Quantity { get; set; }
    }

    public class FreeSKUCollection : List<FreeSKU>
    {
    }

    public class PromotionConfiguration : HLConfiguration
    {
        [ConfigurationProperty("Promotions", IsDefaultCollection = true),
         ConfigurationCollection(typeof(PromotionCollection), AddItemName = "Promo")]
        public PromotionCollection Promotions
        {
            get { return this["Promotions"] as PromotionCollection; }
        }

        public static PromotionConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "PromotionConfig") as PromotionConfiguration;
        }
    }

    public class PromotionTypeConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var ret = PromotionType.None;
            string promoTypes = value as string;
            if (!string.IsNullOrEmpty(promoTypes))
            {
                TypeConverter productConverter = TypeDescriptor.GetConverter(typeof(PromotionType));

                string[] types = promoTypes.Split(',');
                foreach (var type in types)
                {
                    ret |= (PromotionType)productConverter.ConvertFromInvariantString(type);
                }
            }

            return ret;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            return string.Empty;
        }
    }

    public class StringListConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var ret = new List<string>();
            string values = value as string;
            if (!string.IsNullOrEmpty(values))
            {
                ret = new List<string>(values.Split(new char[] { ',' }));
            }

            return ret;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            return string.Join(",", (value as List<string>).ToArray());
        }
    }

    public class FreeSKUConverter : ConfigurationConverterBase
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var ret = new FreeSKUCollection();
            string freeSKUs = value as string;
            if (!string.IsNullOrEmpty(freeSKUs))
            {
                string[] skuQtyArr = freeSKUs.Split(',');
                foreach (var l in skuQtyArr)
                {
                    string[] skuQty = l.Split('|');
                    if (skuQty.Length != 0)
                    {
                        ret.Add(new FreeSKU { Quantity = int.Parse(skuQty[1]), SKU = skuQty[0] });
                    }
                }
            }

            return ret;
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value,
                                         Type destinationType)
        {
            return string.Empty;
        }
    }

    public class PromotionElement : ConfigurationElement, ICloneable
    {
        public PromotionElement()
        {
        }
        public object Clone()
        {
            var promoelement = (PromotionElement)this.MemberwiseClone();
            return promoelement;
        }

        [ConfigurationProperty("code", IsRequired = true)]
        public string Code
        {
            get { return this["code"].ToString(); }
            set { this["code"] = value; }
        }

        [Obsolete("Replacing by CustTypeList")]
        [ConfigurationProperty("custType", IsRequired = false)]
        public string CustType
        {
            get { return this["custType"].ToString(); }
            set { this["custType"] = value; }
        }

        [Obsolete("Replacing by CustCategoryTypeList")]
        [ConfigurationProperty("custCategoryType", IsRequired = false)]
        public string CustCategoryType
        {
            get { return this["custCategoryType"].ToString(); }
            set { this["custCategoryType"] = value; }
        }

        [ConfigurationProperty("startDate", IsRequired = true)]
        public string StartDate
        {
            get { return (string)this["startDate"]; }
            set { this["startDate"] = value; }
        }

        [ConfigurationProperty("endDate", IsRequired = true)]
        public string EndDate
        {
            get { return (string)this["endDate"]; }
            set { this["endDate"] = value; }
        }

        /// <summary>
        /// PromotionType
        /// None = 0x0,
        /// DSProvince = 0x1,
        /// ShiptoProvince = 0x2,
        /// Product = 0x4,
        /// Special = 0x8,
        /// Volume = 0x10,
        /// Other = 0x20,
        /// Frieght = 0x40,
        /// Order = 0x80,
        /// SR_FirstOrder = 0x160,
        /// PC_FirstOrder = 0x320,
        /// PC_TO_SR_FirstOrder = 0x640,
        /// AmountDue = 0x1280,
        /// </summary>


        [ConfigurationProperty("promotionType", DefaultValue = ""),
        TypeConverter(typeof(PromotionTypeConverter))]
        public PromotionType PromotionType
        {
            get
            {
                return (PromotionType)this["promotionType"];

            }
            set { this["promotionType"] = value; }
        }


        [ConfigurationProperty("volumeMinInclude", IsRequired = false, DefaultValue = "-1.00")]
        public decimal VolumeMinInclude
        {
            get { return (decimal)this["volumeMinInclude"]; }
            set { this["volumeMinInclude"] = value; }
        }

        [ConfigurationProperty("volumeMaxInclude", IsRequired = false, DefaultValue = "-1.00")]
        public decimal VolumeMaxInclude
        {
            get { return (decimal)this["volumeMaxInclude"]; }
            set { this["volumeMaxInclude"] = value; }
        }
        [ConfigurationProperty("hasIncrementaldegree", IsRequired = false, DefaultValue = false)]
        public bool HasIncrementaldegree
        {
            get { return (bool)this["hasIncrementaldegree"]; }
            set { this["hasIncrementaldegree"] = value; }
        }
        [ConfigurationProperty("volumeMin", IsRequired = false, DefaultValue = "-1.00")]
        public decimal VolumeMin
        {
            get { return (decimal)this["volumeMin"]; }
            set { this["volumeMin"] = value; }
        }

        [ConfigurationProperty("volumeMax", IsRequired = false, DefaultValue = "-1.00")]
        public decimal VolumeMax
        {
            get { return (decimal)this["volumeMax"]; }
            set { this["volumeMax"] = value; }
        }


        [ConfigurationProperty("amountMinInclude", IsRequired = false, DefaultValue = "-1.00")]
        public decimal AmountMinInclude
        {
            get { return (decimal)this["amountMinInclude"]; }
            set { this["amountMinInclude"] = value; }
        }

        [ConfigurationProperty("amountMaxInclude", IsRequired = false, DefaultValue = "-1.00")]
        public decimal AmountMaxInclude
        {
            get { return (decimal)this["amountMaxInclude"]; }
            set { this["amountMaxInclude"] = value; }
        }

        [ConfigurationProperty("amountMin", IsRequired = false, DefaultValue = "-1.00")]
        public decimal AmountMin
        {
            get { return (decimal)this["amountMin"]; }
            set { this["amountMin"] = value; }
        }

        [ConfigurationProperty("amountMax", IsRequired = false, DefaultValue = "-1.00")]
        public decimal AmountMax
        {
            get { return (decimal)this["amountMax"]; }
            set { this["amountMax"] = value; }
        }

        [ConfigurationProperty("maxFreight", IsRequired = false, DefaultValue = "-1.00")]
        public decimal MaxFreight
        {
            get { return (decimal)this["maxFreight"]; }
            set { this["maxFreight"] = value; }
        }

        [Obsolete("Replacing by CustTypeList and CustCategoryTypeList")]
        [ConfigurationProperty("forPC", IsRequired = false, DefaultValue = false)]
        public bool ForPC
        {
            get { return (bool)this["forPC"]; }
            set { this["forPC"] = value; }
        }

        [ConfigurationProperty("onlineOrderOnly", IsRequired = false, DefaultValue = false)]
        public bool OnlineOrderOnly
        {
            get { return (bool)this["onlineOrderOnly"]; }
            set { this["onlineOrderOnly"] = value; }
        }

        [ConfigurationProperty("numOfMonth", IsRequired = false, DefaultValue = 1)]
        public int NumOfMonth
        {
            get { return (int)this["numOfMonth"]; }
            set { this["numOfMonth"] = value; }
        }

        [ConfigurationProperty("dsStoreProvince", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> DSStoreProvince
        {
            get { return (List<string>)this["dsStoreProvince"]; }
            set { this["dsStoreProvince"] = value; }
        }

        [ConfigurationProperty("shippedToProvince", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> ShippedToProvince
        {
            get { return (List<string>)this["shippedToProvince"]; }
            set { this["shippedToProvince"] = value; }
        }

        [ConfigurationProperty("freeSKUList", DefaultValue = ""), TypeConverter(typeof(FreeSKUConverter))]
        public FreeSKUCollection FreeSKUList
        {
            get
            {
                return (FreeSKUCollection)this["freeSKUList"];

            }
            set { this["freeSKUList"] = value; }
        }
        [ConfigurationProperty("freeSKUListForVolume", DefaultValue = ""), TypeConverter(typeof(FreeSKUConverter))]
        public FreeSKUCollection FreeSKUListForVolume
        {
            get
            {
                return (FreeSKUCollection)this["freeSKUListForVolume"];

            }
            set { this["freeSKUListForVolume"] = value; }
        }
        [ConfigurationProperty("freeSKUListForSelectableSku", DefaultValue = ""), TypeConverter(typeof(FreeSKUConverter))]
        public FreeSKUCollection FreeSKUListForSelectableSku
        {
            get
            {
                return (FreeSKUCollection)this["freeSKUListForSelectableSku"];

            }
            set { this["freeSKUListForSelectableSku"] = value; }
        }
        [ConfigurationProperty("selectableSKUList", DefaultValue = ""), TypeConverter(typeof(FreeSKUConverter))]
        public FreeSKUCollection SelectableSKUList
        {
            get
            {
                return (FreeSKUCollection)this["selectableSKUList"];

            }
            set { this["selectableSKUList"] = value; }
        }

        [ConfigurationProperty("custTypeList", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> CustTypeList
        {
            get { return (List<string>)this["custTypeList"]; }
            set { this["custTypeList"] = value; }
        }

        [ConfigurationProperty("deliveryTypeList", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> DeliveryTypeList
        {
            get { return (List<string>)this["deliveryTypeList"]; }
            set { this["deliveryTypeList"] = value; }
        }

        [ConfigurationProperty("excludedExpID", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> excludedExpID
        {
            get { return (List<string>)this["excludedExpID"]; }
            set { this["excludedExpID"] = value; }
        }

        [ConfigurationProperty("custCategoryTypeList", IsRequired = false, DefaultValue = ""), TypeConverter(typeof(StringListConverter))]
        public List<string> CustCategoryTypeList
        {
            get { return (List<string>)this["custCategoryTypeList"]; }
            set { this["custCategoryTypeList"] = value; }
        }
        [ConfigurationProperty("YearlyPromo", IsRequired = false, DefaultValue = false)]
        public bool YearlyPromo
        {
            get { return (bool)this["YearlyPromo"]; }
            set { this["YearlyPromo"] = value; }
        }
    }

    public class PromotionCollection : ConfigurationElementCollection, IEnumerable<PromotionElement>
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public PromotionElement this[int index]
        {
            get { return (PromotionElement)BaseGet(index); }
            set
            {
                if (null != BaseGet(index))
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PromotionElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PromotionElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PromotionElement)element).Code;
        }

        public void Remove(ConfigurationElement element)
        {
            BaseRemove(((PromotionElement)element).Code);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        #region IEnumerable<LocaleUrlMapElement> Members

        public new IEnumerator<PromotionElement> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this[i];
        }

        #endregion
    }
}