using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using MyHerbalife3.Ordering.Providers.China;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using System.Web.Security;
using MyHerbalife3.Shared.ViewModel.Models;
using HL.Common.Configuration;
using System.Web;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    [ScriptService]
    public partial class ExpressTrack : ProductsBase
    {
        public const string YearMonthDayFormat = "yyyy-MM-dd";
        Providers.MyHLShoppingCartView view = null;
        List<MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket> TicketDetails = new List<MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket>();

        protected void Page_Load(object sender, EventArgs e)
        {
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
            lbExpressHeading.Text = GetLocalResourceString("lbExpressHeading.Text");
            if (!IsPostBack)
            {
                if ((Request["OrderId"] != null) && (Request["OrderDate"] != null))
                {
                    DateTime orderDate;

                    if (DateTime.TryParseExact(Request["OrderDate"].ToString(), YearMonthDayFormat, null, System.Globalization.DateTimeStyles.AssumeLocal, out orderDate))
                    {
                        OrderDetail(Request["OrderId"], orderDate);
                        BindData(Request["OrderId"]);
                        if (TicketDetails.Count == 0)
                        {
                            btnQRcodeDownload.Visible = false;
                        }
                        else
                        {
                            string QRCodeUrl = MyHerbalife3.Ordering.Providers.China.OrderProvider.DownloadQRCode(view.DistributorID, Request["OrderId"], Request["OrderDate"].ToString(), Locale, TicketDetails);

                            if (string.IsNullOrEmpty(QRCodeUrl))
                            {
                                btnQRcodeDownload.Visible = false;
                            }
                            else
                            {
                                ViewState["QRCodeUrl"] = QRCodeUrl;

                            }

                        }

                    }
                }
            }
            
             
            
        }
        private void OrderDetail(string orderId, DateTime orderDate)
        {
            DistributorOrderingProfile distributorOrderingProfile = null;
            var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (membershipUser != null && membershipUser.Value != null)
            {
                distributorOrderingProfile = MyHerbalife3.Ordering.Providers.DistributorOrderingProfileProvider.GetProfile(DistributorID, "CN");
            }

            var obj = new MyHerbalife3.Ordering.Providers.MyHLShoppingCartView();
            var orderDetails = obj.GetOrdersWithDetail(DistributorID, distributorOrderingProfile.CNCustomorProfileID, "zh-CN", orderDate.AddDays(-1), orderDate.AddDays(1), MyHerbalife3.Ordering.Providers.China.OrderStatusFilterType.All, "", "", false, false, orderId);
            if (orderDetails != null && orderDetails.Any() && orderDetails.FirstOrDefault().DonationAmount > 0)
            {
                Donation.Visible = true;
                DonationGrd.DataSource = orderDetails;
                DonationGrd.DataBind();
            }
            if (orderDetails != null)
                view = orderDetails.FirstOrDefault();
           if (view != null && membershipUser != null && distributorOrderingProfile != null)
            {
                lblOrdernumber.Text = view.OrderNumber ?? string.Empty;
                lblOrderMonth.Text = view.OrderMonth ?? string.Empty;
                lblOrderStatus.Text = view.OrderStatus ?? string.Empty;
                lblShipStore.Text = view.StoreInfo ?? string.Empty;
                lblCustomerName.Text = membershipUser.Value.FirstNameLocal ?? string.Empty;
                lblCustomerNumber.Text = membershipUser.UserName ?? string.Empty;
                lblSalesChannels.Text = view.ChannelInfo ?? string.Empty;
                lblShippingAddress.Text = view.Address ?? String.Empty;
                DateTime Recivedate = view.DateTimeForOrder;
                lbldeliverydate.Text = Recivedate.ToString("yyyy-MM-dd");
           
              if (membershipUser.IsOnline == true)
                {
                    lblProcessingStore.Text = "在线订购";
                    lblReceivingMode.Text = "送货上门";
                }
             

                    var modifiedcartItems = new List<MyHerbalife3.Ordering.Providers.MyHLProductItemView>();
                    foreach (var cartitem in view.CartItems)
                    {
                    var sku = Providers.CatalogProvider.GetCatalogItem(cartitem.SKU, "CN");
                    if (sku.ProductCategory =="ETO" && view.OrderHeaderId != 0)
                    {
                        var Tickets = new MyHerbalife3.Ordering.Providers.MyHLShoppingCart.Ticket
                        {

                            quantity = cartitem.Quantity,
                            ticketSKU = cartitem.SKU
                        };
                        TicketDetails.Add(Tickets);
                    }
                    if (sku != null)
                        {
                          cartitem.RetailPrice = sku.ListPrice * cartitem.Quantity;
                            modifiedcartItems.Add(cartitem);
                        }
                    }

                    view.CartItems = modifiedcartItems;
                    ProductList.DataSource = view.CartItems;
                    ProductList.DataBind();
                if (view.OrderStatus == "取消订单")
                {
                    btnQRcodeDownload.Visible = false;
                }
                if (view.OrderHeaderId != 0)
                {
                var orderHeaderList = new List<int>();
                orderHeaderList.Add(view.OrderHeaderId);

                  var paymentDetails = MyHerbalife3.Ordering.Providers.China.OrderProvider.GetPaymentDetails(orderHeaderList);

                    if (paymentDetails != null)
                    {
                        paymentDetails.PaymentDetails.ForEach(c =>
                        {
                            c.PaymentDate = c.PaymentDate.AddHours(8);
                            c.Amount = c.Amount + view.DonationAmount;
                        });
                        
                       
                        DateTime ChinaTime = Convert.ToDateTime(paymentDetails.PaymentDetails[0].PaymentDate);
                        lblNTSdate.Text = ChinaTime.ToString("yyyy-MM-dd");
                        lblPaymentTime.Text = ChinaTime.ToString("yyyy-MM-dd HH:mm:ss");
                        PaymentDetail.DataSource = paymentDetails.PaymentDetails;
                        PaymentDetail.DataBind();
                    }
                    else
                    {
                        btnQRcodeDownload.Visible = false;   
                    }
                }
                else
                {
                    btnQRcodeDownload.Visible = false;
                }
            }
        }

        private void BindData(string orderId)
        {
            var expressInfo = OrderProvider.GetExpressTrackInfo(orderId);
            if (expressInfo != null)
            {
                lblReceiver.Text = expressInfo.ReceivingName ?? string.Empty;
                lblReceiverPhone.Text = expressInfo.ReceivingPhone ?? string.Empty;
                lblCarriers.Text = expressInfo.ExpressCompanyName ?? string.Empty;
                lblWaybillNumber.Text = expressInfo.ExpressNum ?? string.Empty;
                lblOrderStatus.CssClass = expressInfo.Status.ToString().ToLower();

                if (expressInfo.TotalPackageUnits != null)
                {
                    string packUnitsTxt = GetLocalResourceString("TotalNPackage");
                    lblPackageUnit.Text = string.Format(packUnitsTxt, expressInfo.TotalPackageUnits.Value);
                }

                if (!string.IsNullOrWhiteSpace(expressInfo.ExpressNum))
                {
                    var delivertType = expressInfo.OrderDeliveryType != null
                                           ? expressInfo.OrderDeliveryType.Trim().ToLower()
                                           : string.Empty;
                    if (delivertType == "exp" || delivertType == "puca")
                    {
                        switch (expressInfo.ExpressCode.ToLower().Trim())
                        {
                            case "yunda":
                            case "sf":
                            case "fedex":
                            case "bestway":
                                ScriptManager.RegisterStartupScript(this, this.GetType(), "GetExternalExpressTrackInfo"
                                    , string.Format("setTimeout( function() {{ GetExpressTrackInfo('{0}','{1}'); }}, 1000);", expressInfo.ExpressCode, expressInfo.ExpressNum)
                                    , true);
                                break;
                        }
                    }
                }
            }
        }

        protected void btnExpressTrack_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/OrderListView.aspx");
        }

        [WebMethod]
        [ScriptMethod]
        public static ExpressTrackInfoResult GetExpressTrackInfo(string expressCode, string expressNo)
        {
            ExpressTrackInfoResult ret = new ExpressTrackInfoResult
            {
                ExpressCode = expressCode,
                ExpressNo = expressNo,
            };

            ret.Result = "false";
            try
            {
                ret.Result = OrderProvider.GetExternalExpressTrackInfo(expressCode, expressNo);
            }
            catch (Exception ex)
            {
                ret.ErrorMessage = ex.Message;
            }

            return ret;
        }

        public class ExpressTrackInfoResult
        {
            public string ExpressCode { get; set; }
            public string ExpressNo { get; set; }
            public string Result { get; set; }
            public string ErrorMessage { get; set; }
        }

        protected void btnQRcodeDownload_Click(object sender, EventArgs e)
        {
            var QRCodeUrl = ViewState["QRCodeUrl"] as string;
            if (!string.IsNullOrEmpty(QRCodeUrl))
            {
                HttpContext.Current.Response.Redirect(QRCodeUrl);
            }
            else
            {
                string DownLoadURL = Settings.GetRequiredAppSetting("TicketDownLoadAPI").ToString().Trim();
                string path = string.Format(DownLoadURL, "", Locale, "-10000" + "&");
                LoggerHelper.Error(string.Format("China QR  code Exception  URL :{0}, Message :{1}", Settings.GetRequiredAppSetting("MyHerbalife.InternalUrl").ToString().Trim() + path, "Due to exception GDO has created download link with Errorcode -10000"));
                HttpContext.Current.Response.Redirect(Settings.GetRequiredAppSetting("MyHerbalife.InternalUrl").ToString().Trim() + path);

            }
        }
    }
}