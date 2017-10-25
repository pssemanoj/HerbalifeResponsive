using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class PaymentsSummary : UserControlBase
    {
        private List<Payment> _paymentInfo;
        public List<Payment> CurrentPaymentInfo
        {
            get
            {
                return _paymentInfo;
            }
            set
            {
                _paymentInfo = value;                
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string key = PaymentsConfiguration.GetCurrentPaymentSessionKey( this.Locale, this.DistributorID);
            List<Payment> payments = SessionInfo.GetSessionInfo(this.DistributorID, this.Locale).Payments;                 
            if (null == payments)
            {
                payments = Session[key] as List<Payment>;
            }
            if (null != payments && payments.Count > 0)
            {
                dlPaymentInfo.DataSource = payments;
                dlPaymentInfo.DataBind();
                CurrentPaymentInfo = payments;                
            }
            SessionInfo.GetSessionInfo(this.DistributorID, this.Locale).Payments = null;
        }
        protected void dlPaymentInfo_ItemDataBound(object sender, System.Web.UI.WebControls.DataListItemEventArgs e)
        {
             StaticPaymentInfo ctl = e.Item.FindControl("paymentInfo") as StaticPaymentInfo;
             if (null != ctl)
             {
                 ctl.CurrentPaymentInfo = e.Item.DataItem as Payment;
             }
        }
    }
}