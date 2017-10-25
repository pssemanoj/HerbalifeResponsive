using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class ConfiguredParam
    {
        [DataMember]
        [XmlAttribute("Name")]
        public string Name { get; set; }

        [DataMember]
        [XmlAttribute("Value")]
        public string Value { get; set; }
    }
}