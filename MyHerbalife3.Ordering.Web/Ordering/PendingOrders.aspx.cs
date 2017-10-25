using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.Web.MasterPages;
using Telerik.Web.UI;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class PendingOrders : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceString("PageResource1.Title", "Pending Orders"));
            
        }

        protected void OrdersGrid_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            bindData();
        }

        private void bindData()
        {
            string locale = CultureInfo.CurrentCulture.Name;
            var MyOrders = OrdersProvider.GetOrdersInProcessing(DistributorID, locale);
            OrdersGrid.DataSource = MyOrders;
            
        }

        protected void OrdersGrid_ItemDataBound(object sender, GridItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case GridItemType.Item:

                case GridItemType.AlternatingItem:

                    var pendingorder = e.Item.DataItem as PendingOrder;

                    var lblOrderId = e.Item.FindControl("lblOrderId") as Label;
                    lblOrderId.Text = pendingorder.OrderId;

                    //Payment Status

                    var lblPaymentStatus = e.Item.FindControl("lblPaymentStatus") as Label;
                    HLConfigManager.Configurations.DOConfiguration.SendEmailUsingSubmitOrder.ToString().ToLower();
                    if (!HLConfigManager.Configurations.DOConfiguration.PendingOrdersUsesPaymentGateway)
                    {
                        if (pendingorder.PaymentStatus == PendingPaymentStatusType.Processing.ToString())
                        {
                            lblPaymentStatus.Text = GetLocalResourceString(PendingPaymentStatusType.Processing.ToString(),
                                                                           "In Processing");
                        }
                        else if (pendingorder.PaymentStatus == PendingPaymentStatusType.PaymentDeclined.ToString()
                                 ||
                                 pendingorder.PaymentStatus == PendingPaymentStatusType.PaymentDeclinedOldOrder.ToString()
                                 ||
                                 pendingorder.PaymentStatus == PendingPaymentStatusType.PaymentDeclinedMaxTries.ToString())
                        {
                            lblPaymentStatus.Text =
                                GetLocalResourceString(PendingPaymentStatusType.PaymentDeclined.ToString(),
                                                       "Payment Declined");
                        }
                        else if (pendingorder.PaymentStatus == PendingPaymentStatusType.PaymentApproved.ToString())
                        {
                            lblPaymentStatus.Text =
                                GetLocalResourceString(PendingPaymentStatusType.PaymentApproved.ToString(),
                                                       "Payment Approved");
                        }
                        else if (pendingorder.PaymentStatus == PendingPaymentStatusType.Transitioning.ToString())
                        {
                            lblPaymentStatus.Text = GetLocalResourceString(
                                PendingPaymentStatusType.Transitioning.ToString(), "Transitioning");
                        }
                    }
                    else
                    {
                        lblPaymentStatus.Text = GetOrderStatus(pendingorder.PaymentStatus);
                    }
                    
                    //SubmittedDate

                    var lblSubmittedDate = e.Item.FindControl("lblSubmittedDate") as Label;
                    lblSubmittedDate.Text = pendingorder.SubmittedDate.ToString();

                    //Volume Points
                    var lblVolumePoints = e.Item.FindControl("lblVolumePoints") as Label;
                    lblVolumePoints.Text = pendingorder.VolumePoints.ToString();


                    //Amount Due
                    var lblAmountDue = e.Item.FindControl("lblAmountDue") as Label;
                    lblAmountDue.Text = pendingorder.AmountDue.ToString();

                    break;
            }
        }

        private string GetOrderStatus(string paymentstatus)
        {
            switch (paymentstatus)
            {
                case "Abandoned":
                case "Declined":
                case "PaymentDeclined":
                case "PaymentDeclinedOldOrder":
                case "PaymentDeclinedMaxTries":
                    return GetLocalResourceString("Declined");
                case "OrderSubmitted":
                    return GetLocalResourceString("OrderSubmitted");                                   
                default:
                    return GetLocalResourceString("Processing");     
            }
        }

        private enum PendingPaymentStatusType
        {
            [EnumMember] Processing = 0,

            [EnumMember] PaymentDeclined = 1,

            [EnumMember] PaymentApproved = 2,

            [EnumMember] Transitioning = 3,

            [EnumMember] PaymentDeclinedOldOrder = 4,

            [EnumMember] PaymentDeclinedMaxTries = 5,
        }

        private enum PendingPaymentOrderStatusType
        {
            [EnumMember] NotSubmitted = 0,

            [EnumMember] ReadyToSubmit = 1,

            [EnumMember] PendingValidation = 2,

            [EnumMember] Declined = 3,
        }
    }
}