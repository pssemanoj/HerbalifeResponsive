using System;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    [AttributeUsage(AttributeTargets.Event, Inherited = true)]
    public class PublishesAttribute : PublishSubscribeAttribute
    {
        public PublishesAttribute(string publishedEventName)
            : base(publishedEventName)
        {
        }

        public PublishesAttribute(object EventType)
            : base(EventType)
        {
        }
    }
}
