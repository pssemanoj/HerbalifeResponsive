using System;
using System.Reflection;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    public class MyHLEventBus<T>
    {

        #region Fields
        private EventBroker _eventBroker = null;
        #endregion

        #region Construction
        public MyHLEventBus()
        {
            _eventBroker = new EventBroker();
        }
        #endregion

        #region Event Registration
        /// <summary>Publish an event on the event bus</summary>
        /// <param name="PublishedEventName">The Published name of the event</param>
        /// <param name="EventPublisher">The object containing the event</param>
        /// <param name="EventName">The name of the event on the object</param>
        public void PublishEvent(string publishedEventName, object eventPublisher, string eventName)
        {
            _eventBroker.RegisterPublisher(publishedEventName, eventPublisher, eventName);
        }

        /// <summary>Publish a System event on the event bus</summary>
        /// <param name="EventType">The System Event Type to publish</param>
        /// <param name="EventPublisher">The object containing the event</param>
        /// <param name="EventName">The name of the event on the object</param>
        public void PublishEvent(T eventType, object eventPublisher, string eventName)
        {
            _eventBroker.RegisterPublisher(EventBroker.MakeSystemEventName(eventType), eventPublisher, eventName);
        }

        /// <summary>Subscribe to an even published on the event bus</summary>
        /// <param name="PublishedEventName">The Published name of the event</param>
        /// <param name="EventHandler">The eventhandler to hook up</param>
        public void SubscribeEvent(string publishedEventName, EventHandler eventHandler)
        {
            _eventBroker.RegisterSubscriber(publishedEventName, eventHandler);
        }

        /// <summary>Subscribe to a System event published on the event bus</summary>
        /// <param name="PublishedEventName">The Published name of the event</param>
        /// <param name="EventHandler">The eventhandler to hook up</param>
        public void SubscribeEvent(T eventType, EventHandler eventHandler)
        {
            _eventBroker.RegisterSubscriber(EventBroker.MakeSystemEventName(eventType), eventHandler);
        }

        /// <summary>Register an object and wire up any event subscriptions or publications</summary>
        /// <param name="Target">The object</param>
        /// <remarks>The event bus examines the object for PublishSubscribeAttributes and appropriately registers any decorated events or delegates</remarks>
        public void RegisterObject(object target)
        {
            PublishSubscribeEvents(target);
        }

        /// <summary>Deregister an object and remove any event subscriptions or publications</summary>
        /// <param name="Target">The object</param>
        /// <remarks>The event bus examines the object for PublishSubscribeAttributes and appropriately deregisters any decorated events or delegates</remarks>

        public void DeregisterObject(object target)
        {
            UnpublishUnsubscribeEvents(target);
        }
        #endregion

        #region Private methods
        /// <summary>Examine an object and wire up any event subscriptions or publications</summary>
        /// <param name="Target">The object to examine</param>

        private void PublishSubscribeEvents(object target)
        {
            Type t = target.GetType();

            foreach (EventInfo eventInfo in t.GetEvents())
            {
                PublishesAttribute[] attrs = (PublishesAttribute[])eventInfo.GetCustomAttributes(typeof(PublishesAttribute), true);
                if (attrs.Length > 0)
                {
                    _eventBroker.RegisterPublisher(attrs[0].EventName, target, eventInfo.Name);
                }
            }

            foreach (MethodInfo method in t.GetMethods())
            {
                SubscribesToAttribute[] attrs = (SubscribesToAttribute[])method.GetCustomAttributes(typeof(SubscribesToAttribute), true);
                if (attrs.Length > 0)
                {
                    _eventBroker.RegisterSubscriber(attrs[0].EventName, (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), target, method));
                }
            }
        }

        /// <summary>Examine an object and undo any event subscriptions or publications</summary>
        /// <param name="Target">The object to examine</param>
        private void UnpublishUnsubscribeEvents(object target)
        {
            Type t = target.GetType();

            foreach (MethodInfo method in t.GetMethods())
            {
                SubscribesToAttribute[] attrs = (SubscribesToAttribute[])method.GetCustomAttributes(typeof(SubscribesToAttribute), true);
                if (attrs.Length > 0)
                {
                    _eventBroker.UnregisterSubscriber(attrs[0].EventName, (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), target, method));
                }
            }

            foreach (EventInfo eventInfo in t.GetEvents())
            {
                PublishesAttribute[] attrs = (PublishesAttribute[])eventInfo.GetCustomAttributes(typeof(PublishesAttribute), true);
                if (attrs.Length > 0)
                {
                    _eventBroker.UnregisterPublisher(attrs[0].EventName, target, eventInfo.Name);
                }
            }

        }

        #endregion

    }
}
