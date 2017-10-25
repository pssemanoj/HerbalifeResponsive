using System;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    /// <summary>
    /// Base class for the two publish / subscribe attributes. Stores
    /// the event name to be published or subscribed to.
    /// </summary>
    public abstract class PublishSubscribeAttribute : Attribute
    {

        private string _eventName;

        protected PublishSubscribeAttribute(string eventName)
        {
            this._eventName = eventName;
        }

        protected PublishSubscribeAttribute(object eventType)
        {
            this._eventName = EventBroker.MakeSystemEventName(eventType);
        }

        public string EventName
        {
            get { return _eventName; }
            set { _eventName = value; }
        }
    }
}
