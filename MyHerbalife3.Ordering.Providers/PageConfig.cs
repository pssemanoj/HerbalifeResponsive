using System;
using System.Runtime.Serialization;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class PageConfig
    {
        [DataMember]
        [XmlAttribute("Name")]
        public string PageName { get; set; }

        [DataMember]
        [XmlArray("Params")]
        [XmlArrayItem("Param")]
        public UserControlParam[] Params { get; set; }

        [DataMember]
        [XmlElement("LeftPanel", Form = XmlSchemaForm.None)]
        public PanelConfig LeftPanelConfig { get; set; }

        [DataMember]
        [XmlElement("RightPanel", Form = XmlSchemaForm.None)]
        public PanelConfig RightPanelConfig { get; set; }

        [DataMember]
        [XmlArray("ConfiguredProperties")]
        [XmlArrayItem("ConfiguredProperty")]
        public ConfiguredProperty[] ConfiguredProperties { get; set; }
    }
}