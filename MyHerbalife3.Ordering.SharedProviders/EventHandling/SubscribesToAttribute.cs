using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]

    public class SubscribesToAttribute : PublishSubscribeAttribute
    {
        public SubscribesToAttribute(string eventName)
            : base(eventName)
        {
        }


        public SubscribesToAttribute(object eventType)
            : base(eventType)
        {
        }
    }
}
