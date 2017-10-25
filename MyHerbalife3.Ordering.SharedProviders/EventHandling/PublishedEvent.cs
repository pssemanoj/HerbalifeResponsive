using System;
using System.Collections.Generic;
using System.Reflection;

namespace MyHerbalife3.Ordering.SharedProviders.EventHandling
{
    internal class PublishedEvent
    {

        #region Fields
        private List<object> _publishers;
        private List<EventHandler> _subscribers;
        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedEvent"/> class.
        /// </summary>
        public PublishedEvent()
        {
            _publishers = new List<object>();
            _subscribers = new List<EventHandler>();
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Gets the publishers.
        /// </summary>
        /// <value>The publishers.</value>
        public IEnumerable<object> Publishers
        {
            get
            {
                foreach (object publisher in _publishers)
                {
                    yield return publisher;
                }
            }
        }


        /// <summary>
        /// Gets the subscribers.
        /// </summary>
        /// <value>The subscribers.</value>
        public IEnumerable<EventHandler> Subscribers
        {
            get
            {
                foreach (EventHandler subscriber in _subscribers)
                {
                    yield return subscriber;
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has publishers.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has publishers; otherwise, <c>false</c>.
        /// </value>
        public bool HasPublishers
        {
            get { return _publishers.Count > 0; }
        }


        /// <summary>
        /// Gets a value indicating whether this instance has subscribers.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has subscribers; otherwise, <c>false</c>.
        /// </value>
        public bool HasSubscribers
        {
            get { return _subscribers.Count > 0; }
        }


        /// <summary>
        /// Adds the publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="eventName">Name of the event.</param>
        public void AddPublisher(object publisher, string eventName)
        {
            _publishers.Add(publisher);
            EventInfo targetEvent = publisher.GetType().GetEvent(eventName);
            CheckEventExists(eventName, publisher, targetEvent);

            MethodInfo addEventMethod = targetEvent.GetAddMethod();
            CheckAddMethodExists(targetEvent);

            EventHandler newSubscriber = OnPublisherFiring;
            addEventMethod.Invoke(publisher, new object[] { newSubscriber });
        }


        /// <summary>
        /// Removes the publisher.
        /// </summary>
        /// <param name="publisher">The publisher.</param>
        /// <param name="eventName">Name of the event.</param>
        public void RemovePublisher(object publisher, string eventName)
        {
            _publishers.Remove(publisher);
            EventInfo targetEvent = publisher.GetType().GetEvent(eventName);
            CheckEventExists(eventName, publisher, targetEvent);

            MethodInfo removeEventMethod = targetEvent.GetRemoveMethod();
            CheckRemoveMethodExists(targetEvent);

            EventHandler subscriber = OnPublisherFiring;
            removeEventMethod.Invoke(publisher, new object[] { subscriber });
        }


        /// <summary>
        /// Adds the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public void AddSubscriber(EventHandler subscriber)
        {
            _subscribers.Add(subscriber);
        }


        /// <summary>
        /// Removes the subscriber.
        /// </summary>
        /// <param name="subscriber">The subscriber.</param>
        public void RemoveSubscriber(EventHandler subscriber)
        {
            _subscribers.Remove(subscriber);
        }
        #endregion

        #region Private methods
        private void OnPublisherFiring(object sender, System.EventArgs e)
        {
            foreach (EventHandler subscriber in _subscribers)
            {
                subscriber(sender, e);
            }
        }

        private static void CheckEventExists(string eventName, object publisher, EventInfo targetEvent)
        {
            if (targetEvent == null)
            {
                throw new ArgumentException(string.Format("The event '{0}' is not implemented on type '{1}'", eventName, publisher.GetType().Name));
            }
        }

        private static void CheckAddMethodExists(EventInfo targetEvent)
        {
            if (targetEvent.GetAddMethod() == null)
            {
                throw new ArgumentException(string.Format("The event '{0}' does not have a public Add method", targetEvent.Name));
            }
        }

        private static void CheckRemoveMethodExists(EventInfo targetEvent)
        {
            if (targetEvent.GetRemoveMethod() == null)
            {
                throw new ArgumentException(string.Format("The event '{0}' does not have a public Remove method", targetEvent.Name));
            }
        }
        #endregion

    }
}
