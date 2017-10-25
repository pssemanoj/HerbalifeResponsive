using System;
using System.Web.Management;

namespace MyHerbalife3.Ordering.SharedProviders
{
    /// <summary>
    /// Implements a web event for error bubling to WMI aware monitoring
    /// </summary>
    public class HLErrorAuditEvent : WebErrorEvent
    {
        public const int WEB_ERROR_EVENT_CODE_OFFSET = 2000;

        /// <summary>
        /// Initializes a new instance of the <see cref="HLErrorAuditEvent"/> class.
        /// </summary>
        /// <param name="msg">The message text for this error.</param>
        /// <param name="eventSource">The event source object.</param>
        /// <param name="e">The exception associated with this error.</param>
        public HLErrorAuditEvent(string msg, object eventSource, Exception e)
            : base(msg, eventSource, WebEventCodes.WebExtendedBase + WEB_ERROR_EVENT_CODE_OFFSET, e)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HLErrorAuditEvent"/> class.
        /// </summary>
        /// <param name="msg">The message text for this error.</param>
        /// <param name="eventSource">The event source object.</param>
        /// <param name="detailedCode">The detail code - an extra app specific ID to associate as a 
        /// detail to enable distringuishing between events coming from different parts of the code.</param>
        /// <param name="e">The exception associated with this error.</param>
        public HLErrorAuditEvent(string msg, object eventSource, int detailedCode, Exception e)
            : base(msg, eventSource, WebEventCodes.WebExtendedBase + WEB_ERROR_EVENT_CODE_OFFSET, detailedCode, e)
        {
        }
    }
}
