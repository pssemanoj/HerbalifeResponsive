using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    public class EventBroker
    {

        #region Constants
        private const string SystemEventMarker = "__SystemEvent.{0}";
        #endregion

        #region Fields
        private Dictionary<string, PublishedEvent> _eventPublishers = new Dictionary<string, PublishedEvent>();
        #endregion

        #region Public methods
        public IEnumerable<string> RegisteredEvents
        {
            get
            {
                foreach (string eventName in _eventPublishers.Keys)
                {
                    yield return eventName;
                }
            }
        }

        public void RegisterPublisher(string publishedEventName, object publisher, string eventName)
        {
            PublishedEvent @event = GetEvent(publishedEventName);
            @event.AddPublisher(publisher, eventName);
        }

        public void UnregisterPublisher(string publishedEventName, object publisher, string eventName)
        {
            PublishedEvent @event = GetEvent(publishedEventName);
            @event.RemovePublisher(publisher, eventName);
            RemoveDeadEvents();
        }

        public void RegisterSubscriber(string publishedEventName, EventHandler subscriber)
        {
            PublishedEvent publishedEvent = GetEvent(publishedEventName);
            publishedEvent.AddSubscriber(subscriber);
        }

        public void UnregisterSubscriber(string publishedEventName, EventHandler subscriber)
        {
            PublishedEvent @event = GetEvent(publishedEventName);
            @event.RemoveSubscriber(subscriber);
            RemoveDeadEvents();
        }

        public IEnumerable<object> GetPublishersFor(string publishedEvent)
        {
            foreach (object publisher in GetEvent(publishedEvent).Publishers)
            {
                yield return publisher;
            }
        }

        public IEnumerable<EventHandler> GetSubscribersFor(string publishedEvent)
        {
            foreach (EventHandler subscriber in GetEvent(publishedEvent).Subscribers)
            {
                yield return subscriber;
            }
        }
        #endregion

        #region Private methods
        private PublishedEvent GetEvent(string eventName)
        {
            if (!_eventPublishers.ContainsKey(eventName))
            {
                _eventPublishers[eventName] = new PublishedEvent();
            }
            return _eventPublishers[eventName];
        }

        private void RemoveDeadEvents()
        {
            List<string> deadEvents = new List<string>();
            foreach (KeyValuePair<string, PublishedEvent> publishedEvent in _eventPublishers)
            {
                if (!publishedEvent.Value.HasPublishers && !publishedEvent.Value.HasSubscribers)
                {
                    deadEvents.Add(publishedEvent.Key);
                }
            }

            deadEvents.ForEach(delegate(string eventName) { _eventPublishers.Remove(eventName); });
        }
        #endregion

        #region Public static methods

        public static string MakeSystemEventName(object eventType)
        {
            return string.Format(SystemEventMarker, eventType.ToString());
        }
        #endregion

    }
}
