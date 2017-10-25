using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class PanelUserControl
    {
        
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [DataMember]
        [XmlArray("Params")]
        [XmlArrayItem("Param")]
        public UserControlParam[] Params { get; set; }

        [DataMember]
        [XmlArray("ConfiguredProperties")]
        [XmlArrayItem("ConfiguredProperty")]
        public ConfiguredProperty[] ConfiguredProperties { get; set; }
    }
}