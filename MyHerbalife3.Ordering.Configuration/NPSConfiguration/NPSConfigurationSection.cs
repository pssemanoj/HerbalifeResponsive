using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Configuration.NPSConfiguration
{
    public class NPSConfigurationSection : ConfigurationSection
    {
        NPSMapElement url;

        public NPSConfigurationSection()
        {
            url = new NPSMapElement();
        }


        [ConfigurationProperty("mappings", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(NPSCollection),
            AddItemName = "addMapping",
            ClearItemsName = "clearMappings",
            RemoveItemName = "removeMappings")]
        public NPSCollection Mappings
        {
            get
            {
                NPSCollection mappingCollection = (NPSCollection)base["mappings"];
                return mappingCollection;
            }
        }
    }

    public class NPSCollection : ConfigurationElementCollection, IEnumerable<NPSMapElement>
    {
        public NPSCollection()
        {
            NPSMapElement url = (NPSMapElement)CreateNewElement();
            Add(url);
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NPSMapElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((NPSMapElement)element).Country;
        }

        public NPSMapElement this[int index]
        {
            get
            {
                return (NPSMapElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public int IndexOf(NPSMapElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(NPSMapElement url)
        {
            BaseAdd(url);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(NPSMapElement url)
        {
            if (BaseIndexOf(url) >= 0)
                BaseRemove(url.Country);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            throw new InvalidOperationException();
        }

        public void Clear()
        {
            BaseClear();
        }

        #region IEnumerable<NPSMapElement> Members

        public new IEnumerator<NPSMapElement> GetEnumerator()
        {
            for (int i = 0; i < this.Count; i++)
                yield return this[i];
        }

        #endregion
    }

    public class NPSMapElement : ConfigurationElement
    {
        /// <summary>
        ///  Locale 
        /// </summary>
        [ConfigurationProperty("country", DefaultValue = "US", IsRequired = true)]
        public string Country
        {
            get
            {
                return (string)this["country"];
            }
            set
            {
                this["country"] = value;
            }
        }

        /// <summary>
        /// has packing and handling or not
        /// </summary>
        [ConfigurationProperty("hasPHCharge", DefaultValue = "true", IsRequired = false, IsKey = false)]
        public bool HasPHCharge
        {
            get
            {
                return (bool)this["hasPHCharge"];
            }
            set
            {
                this["hasPHCharge"] = value;
            }
        }

        /// <summary>
        /// this is the starting date when PH charge should be removed from GDO, yyyy-mm-dd
        /// </summary>
        [ConfigurationProperty("removePHChargeStartDate", DefaultValue = "2063-02-19", IsRequired = false, IsKey = false)]
        public string RemovePHChargeStartDate
        {
            get
            {
                return (string)this["removePHChargeStartDate"];
            }
            set
            {
                this["removePHChargeStartDate"] = value;
            }
        }

        /// <summary>
        /// to get the resx key for package and handling text
        /// </summary>
        [ConfigurationProperty("phChargeResxKey", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PHChargeResxKey
        {
            get
            {
                return (string)this["phChargeResxKey"];
            }
            set
            {
                this["phChargeResxKey"] = value;
            }
        }

        /// <summary>
        /// to get the resx key for pickup and handling text
        /// </summary>
        [ConfigurationProperty("pickupChargeResxKey", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string PickupChargeResxKey
        {
            get
            {
                return (string)this["pickupChargeResxKey"];
            }
            set
            {
                this["pickupChargeResxKey"] = value;
            }
        }

        /// <summary>
        /// to get the resx key for shipping and handling text
        /// </summary>
        [ConfigurationProperty("shippingChargeResxKey", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShippingChargeResxKey
        {
            get
            {
                return (string)this["shippingChargeResxKey"];
            }
            set
            {
                this["shippingChargeResxKey"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [total earn base PRDCT type only].
        /// </summary>
        /// <value>
        /// <c>true</c> if [total earn base PRDCT type only]; otherwise, <c>false</c>.
        /// </value>
        [ConfigurationProperty("totalEarnBasePrdctTypeOnly", DefaultValue = "false", IsRequired = false, IsKey = false)]
        public bool TotalEarnBasePrdctTypeOnly
        {
            get
            {
                return (bool)this["totalEarnBasePrdctTypeOnly"];
            }
            set
            {
                this["totalEarnBasePrdctTypeOnly"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the total earn base PRDCT type only date.
        /// </summary>
        /// <value>
        /// The total earn base PRDCT type only date.
        /// </value>
        [ConfigurationProperty("totalEarnBasePrdctTypeOnlyDate", DefaultValue = "2063-02-19", IsRequired = false, IsKey = false)]
        public string TotalEarnBasePrdctTypeOnlyDate
        {
            get
            {
                return (string)this["totalEarnBasePrdctTypeOnlyDate"];
            }
            set
            {
                this["totalEarnBasePrdctTypeOnlyDate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the combined value to which the ISO code will be mapped
        /// </summary>
        /// <value>
        /// The value to map the ISO code
        /// </value>
        [ConfigurationProperty("mapFrom", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string MapFrom
        {
            get
            {
                return (string)this["mapFrom"];
            }
            set
            {
                this["mapFrom"] = value;
            }
        }

        [ConfigurationProperty("showEarnbaseHelpDate", DefaultValue = "", IsRequired = false, IsKey = false)]
        public string ShowEarnbaseHelpDate
        {
            get { return (string)this["showEarnbaseHelpDate"]; }
            set { this["showEarnbaseHelpDate"] = value; }
        }
    }
}
