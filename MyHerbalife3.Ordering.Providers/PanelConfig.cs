using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class PanelConfig
    {
        [DataMember]
        [XmlArray("Controls")]
        [XmlArrayItem("UserControl")]
        public PanelUserControl[] Controls { get; set; }
    }
}