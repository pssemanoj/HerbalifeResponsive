using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers
{
    public class Helper
    {
        #region singleton
        private Helper() { }
        public static Helper Instance = new Helper();
        #endregion

        /// <summary>
        /// Standard Response validation.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool ValidateResponse(MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseValue response)
        {
            return (response != null) && (response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderSvc.ServiceResponseStatusType.Success);
        }

        public bool ValidateResponse(MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseValue response)
        {
            return (response != null) && (response.Status == MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.ServiceResponseStatusType.Success);
        }

        /// <summary>
        /// Return true if the List is not null and contains at least 1 data.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool HasData(IList list)
        {
            return (list != null) && (list.Count > 0);
        }

        public void LogError(string errorMessage, string messageHeader = null)
        {
            string msg = (messageHeader == null) ? errorMessage : string.Format("{0}, error: {1}", messageHeader, errorMessage);
            HL.Common.Logging.LoggerHelper.Error(msg);
        }

        /// <summary>
        /// Standard exception handling.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="messageHeader">Can pass in something like "YourClassName.MethodName()," where the exception being thrown out.</param>
        public void HandleException(Exception ex, string messageHeader = null)
        {
            LogError(ex.Message, messageHeader);
        }
    }
}
