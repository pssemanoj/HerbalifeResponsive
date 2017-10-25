using HL.Common.EventHandling;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class OrderSubTypeEventArgs : HLEventArgs
    {
        public string DSSubType { get; set; }
        public bool IsEarnings { get; set; }
        public bool IsVolume { get; set; }
        public bool RefreshCartDisplay { get; set; }

        public OrderSubTypeEventArgs(string dSSubType, bool isEarnings, bool isVolume, bool refreshCartDisplay)
        {
            DSSubType = dSSubType;
            IsEarnings = isEarnings;
            IsVolume = isVolume;
            RefreshCartDisplay = refreshCartDisplay;
        }
    }
}
