using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using System.Collections.Specialized;
using MyHerbalife3.Ordering.Providers.Payments;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using HL.Common.Utilities;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class PaymentDeclinedInfo : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
          pnlDelinedPaymentsDetails.Visible = false;
          ResponseContext _responseContext;          

          string paymentGatewayLogs = string.Empty;
          if (CountryCode.Equals("RS"))
          {
              string _orderNumber = string.Empty;             
              if (HttpContext.Current.Session["declinedOrderNumber"] != null)
              {
                  _orderNumber = HttpContext.Current.Session["declinedOrderNumber"].ToString();
                  HttpContext.Current.Session["declinedOrderNumber"] = null;
              }  
              if ((!string.IsNullOrEmpty(_orderNumber)) && (string.IsNullOrEmpty(lblPaymentCode.Text)))
              {
                  PaymentGatewayRecordStatusType orderStatus = OrderProvider.GetPaymentGatewayRecordStatus(_orderNumber.ToString());

                  if (orderStatus == PaymentGatewayRecordStatusType.Declined)
                  {
                      paymentGatewayLogs = OrderProvider.GetPaymentGatewayLog(_orderNumber, PaymentGatewayLogEntryType.Response).Where(l => l.Contains("Error:=") || l.Contains("result:=") || l.Contains("Error=") || l.Contains("result=")).FirstOrDefault();
                      if (!string.IsNullOrEmpty(paymentGatewayLogs))
                      {
                          _responseContext = new ResponseContext(paymentGatewayLogs);
                          if (_responseContext.PostedValues.AllKeys.Contains("Error"))
                          {
                              if (!string.IsNullOrEmpty(_responseContext.PostedValues["paymentid"]))
                              {
                                  lblPaymentCode.Text = _responseContext.PostedValues["paymentid"];
                              }
                              lblTransactionCode.Text = "0000000000000000";
                              lblTransactionCodeText.Visible = true;
                              pnlDelinedPaymentsDetails.Visible = true;                              
                          }
                          else
                              if (_responseContext.PostedValues.AllKeys.Contains("result"))
                              {
                                  if (!string.IsNullOrEmpty(_responseContext.PostedValues["paymentid"]))
                                  {
                                      lblPaymentCode.Text = _responseContext.PostedValues["paymentid"];
                                  }
                                  if (!string.IsNullOrEmpty(_responseContext.PostedValues["tranid"]))
                                  {
                                      lblTransactionCode.Text = _responseContext.PostedValues["tranid"];
                                  }
                                  else
                                  {
                                      lblTransactionCode.Text = "0000000000000000";
                                  }
                                  pnlDelinedPaymentsDetails.Visible = true;                                  
                              }
                          lblTransactionDatetime.Text = DateUtils.GetCurrentLocalTime(CountryCode).ToString();
                          lblAutorizationCode.Text = "000000";
                      }
                  }
                  else
                  {
                      pnlDelinedPaymentsDetails.Visible = false;
                  }                  
              }
              else
              {
                  pnlDelinedPaymentsDetails.Visible = false;
              }
          }
          else
          {
              pnlDelinedPaymentsDetails.Visible = false;
          }
        }
    }
}