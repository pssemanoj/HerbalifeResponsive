using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using HL.PGH.Api;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class PGHPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private PGHPaymentGatewayInvoker(string paymentMethod, decimal amount) : base("PGHPaymentGateway", paymentMethod, amount)
        {

        }

        public override void Submit()
        {
            try
            {
                PaymentRequest request = HttpContext.Current.Session["PGHInterface.aspx"] as PaymentRequest;
                if (null != request)
                {
                    if (HLConfigManager.Configurations.PaymentsConfiguration.ConvertAmountDue)
                    {
                        request.Amount = OrderProvider.GetConvertedAmount(request.Amount, this._country);                        
                    }
                    request.Submit();
                }
            }
            catch (Exception ex)
            {
                //Logging
                string s = ex.Message;
            }
            finally
            {
                HttpContext.Current.Session.Remove("PGHInterface.aspx");
            }        
        }

        protected override void GetOrderNumber()
        {
        }
    }
}
