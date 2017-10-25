using System;
using System.Collections.Generic;
using System.Configuration;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects
{
    public class RulesConfiguration : HLConfiguration
    {
        [ConfigurationProperty("RulesModules", IsDefaultCollection = true),
         ConfigurationCollection(typeof (RulesModuleCollection),
             AddItemName = "addRulesModule",
             ClearItemsName = "clearRulesModule",
             RemoveItemName = "removeRulesModule")]
        public RulesModuleCollection RulesModules
        {
            get { return this["RulesModules"] as RulesModuleCollection; }
        }

        public static RulesConfiguration GetConfiguration(System.Configuration.Configuration config)
        {
            return GetConfiguration(config, "Rules") as RulesConfiguration;
        }
    }

    public class RulesModule : ConfigurationElement
    {
        //<RulesModule processOrder="1" assemblyName="APFRules.dll" className="MyHerbalifeRules.APF.Global.APFRules" rulesInterface="ICartRule"/>

        public RulesModule()
        {
        }

        public RulesModule(int ProcessOrder, string ClassName, string RuleInterface)
        {
            this.ProcessOrder = ProcessOrder;
            this.ClassName = ClassName;
            this.RuleInterface = RuleInterface;
        }

        [ConfigurationProperty("discontinueOnError", IsRequired = false, DefaultValue = false)]
        public bool DiscontinueOnError
        {
            get { return Convert.ToBoolean(this["discontinueOnError"]); }
            set { this["discontinueOnError"] = value; }
        }

        [ConfigurationProperty("processOrder", IsRequired = true, DefaultValue = "1")]
        public int ProcessOrder
        {
            get { return Convert.ToInt32(this["processOrder"]); }
            set { this["processOrder"] = value; }
        }

        [ConfigurationProperty("className", IsRequired = true, DefaultValue = "")]
        public string ClassName
        {
            get { return (string) this["className"]; }
            set { this["className"] = value; }
        }

        [ConfigurationProperty("ruleInterface", IsRequired = true, DefaultValue = "")]
        public string RuleInterface
        {
            get { return (string) this["ruleInterface"]; }
            set { this["ruleInterface"] = value; }
        }

        [ConfigurationProperty("checkIntegrityOnError", IsRequired = false, DefaultValue = false)]
        public bool CheckIntegrityOnError
        {
            get { return Convert.ToBoolean(this["checkIntegrityOnError"]); }
            set { this["checkIntegrityOnError"] = value; }
        }
    }

    public class RulesModuleCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public RulesModule this[int index]
        {
            get { return (RulesModule) BaseGet(index); }
            set
            {
                if (null != BaseGet(index))
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(RulesModule element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return string.Concat(((RulesModule) element).ClassName, ((RulesModule) element).RuleInterface);
        }

        public void Remove(RulesModule element)
        {
            BaseRemove(string.Concat(((RulesModule) element).ClassName, ((RulesModule) element).RuleInterface));
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RulesModule();
        }

        public void Remove(string Name)
        {
            BaseRemove(Name);
        }

        public void RemoveAt(int Index)
        {
            BaseRemoveAt(Index);
        }

        #region IEnumerable<RulesModule> Members

        public new IEnumerator<RulesModule> GetEnumerator()
        {
            int count = base.Count;

            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as RulesModule;
            }
        }

        #endregion
    }
}