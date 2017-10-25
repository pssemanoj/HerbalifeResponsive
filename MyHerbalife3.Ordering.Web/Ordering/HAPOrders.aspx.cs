using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    struct HAPOrder_Item
    {
        public string OrderID { get; set; }
        public string ProgramType { get; set; }
        public string CountryCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; }
        public decimal Volume { get; set; }
        public decimal AmontDue { get; set; }
    }

    public partial class HAPOrders : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!HLConfigManager.Configurations.DOConfiguration.AllowHAP)
            {
                divHAPDisabled.Visible = true;
                divMainContent.Visible = false;
            }
            else if (!IsPostBack)
            {
                SessionInfo.IsHAPMode = false;
                ShoppingCart.HAPType = string.Empty;
                ShoppingCart.HAPScheduleDay = 0;
                ShoppingCart.HAPAction = "";
                if (ShoppingCart.DsType == null)
                {
                    var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(DistributorID, CountryCode);
                    ShoppingCart.DsType = DistributorType;
                }
                setNotificationMessage();
                setHAPOrdersListView();
            }
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-cho");
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
        }

        #region Private methods
        private void setHAPOrdersListView()
        {
            var oHAPOrders = ActiveHAPOrders;
            if (oHAPOrders != null && oHAPOrders.Count > 0)
            {
                if (!(ShoppingCart.DsType == Scheme.Member))
                {
                    if (oHAPOrders[0].HapOrderProgramType == "01")
                    {
                        SessionInfo.HAPOrderType = "Personal";
                    }
                    else
                    {
                        SessionInfo.HAPOrderType = "RetailOrder";
                    }
                }
            }
            var dsHAPOrders = new List<HAPOrder_Item>();
            var expDate = getHapExpirationDate();
            if (oHAPOrders != null && oHAPOrders.Count > 0 && expDate != null)
            {
                foreach (Order_V02 hapOrder in oHAPOrders)
                {

                    dsHAPOrders.Add(new HAPOrder_Item
                    {
                        OrderID = hapOrder.OrderID,
                        ProgramType = hapOrder.HapOrderProgramType == "01" ? GetLocalResourceObject("PersonalResource1.Text").ToString() : GetLocalResourceObject("ResaleResource1.Text").ToString(),
                        CountryCode = CountryCode,
                        StartDate = hapOrder.ReceivedDate,
                        ExpirationDate = (DateTime)expDate,
                        Status = hapOrder.HapOrderStatus,
                        Volume = (hapOrder.Pricing as OrderTotals_V01).VolumePoints,
                        AmontDue = (hapOrder.Pricing as OrderTotals_V01).AmountDue
                    });
                }
            }
            lstHAPOrders.DataSource = dsHAPOrders;
            lstHAPOrders.DataBind();

            if (GlobalContext.CultureConfiguration.IsBifurcationEnabled)
            {
                if (ShoppingCart.DsType == Scheme.Member)
                {
                    CreateHapOrder.Visible = !(dsHAPOrders.Count > 0);
                }
                else
                {
                    CreateHapOrder.Visible = false;
                }
            }
            else
            {
                divHAPbuttons.Visible = !(dsHAPOrders.Count > 1);
            }
            litHapEditBullet.Visible = (dsHAPOrders.Count > 0);
            var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser());
            if (member !=null && !string.IsNullOrEmpty(member.Value.ProcessingCountryCode) && member.Value.ProcessingCountryCode != "CA")
            {

                CreateHapOrder.Visible = false;
                lblNotification.Visible = true;
            }
        }

        private void setNotificationMessage()
        {
            if (GlobalContext.CultureConfiguration.IsBifurcationEnabled)
            {
                if (ShoppingCart.DsType == Scheme.Member)
                {
                    CreateHapOrder.Visible = true;
                    NoteForHapOrder.Visible = false;
                }
                else
                {
                    CreateHapOrder.Visible = false;
                    if (HLConfigManager.Configurations.DOConfiguration.DisplayBifurcationKeys)
                    {
                        lblHapDescription.Text = (string)GetLocalResourceString("hapDescriptionForDS.Text");
                        hapInfo.ContentPath = "HapLandingPageInfoForDS.html";
                    }
                }
            }

            divNotificationRenew.Visible = false;
            var expDate = getHapExpirationDate();
            var oHAPOrders = ActiveHAPOrders;
            if (expDate != null && DateUtils.GetCurrentLocalTime(CountryCode) >= ((DateTime)expDate).AddDays(-30) && oHAPOrders != null && oHAPOrders.Count > 0)
            {
                litNotificationRenew.Text = string.Format(GetLocalResourceString("NotificationRenewHAPOrder"), ((DateTime)expDate).ToString("MM/dd/yyyy"));
                divNotificationRenew.Visible = true;
            }
        }

        private bool hasAPSPayment(string orderId)
        {
            if (!string.IsNullOrEmpty(orderId))
            {
                var oHAPOrders = ActiveHAPOrders;
                var hapOrder = oHAPOrders != null ? oHAPOrders.Find(oi => oi.OrderID == orderId) : null;
                if (hapOrder != null && hapOrder.Payments != null && hapOrder.Payments.Count() > 0)
                {
                    var payment = hapOrder.Payments.OrderByDescending(p => p.PaymentDate).First();
                    if (payment is LegacyPayment_V01 && (payment as LegacyPayment_V01).PaymentCode == "APS")
                        return true;
                }
            }

            return false;
        }

        private string getMyOrderDetailsLink(string orderId)
        {
            if (GlobalContext.CultureConfiguration.IsBifurcationEnabled)
            {
                if (ShoppingCart.DsType == Scheme.Member)
                {
                    return string.Format("https:" + "//www.myherbalife.com/{0}/Account/OrderHistory/GetOrderDetails/{1}/Mb", Locale, orderId);
                }
                else
                {
                    return string.Format("https:" + "//www.myherbalife.com/account/MyOrderDetails.aspx?oid={0}", orderId);
                }
            }
            return string.Format("~/account/MyOrderDetails.aspx?oid={0}", orderId);
        }
        #endregion

        #region Events
        protected void CreateHapOrder_Click(object sender, EventArgs e)
        {
            if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
            {
                ShoppingCart.CloseCart(true);
            }
            var sessionInfo = SessionInfo.GetSessionInfo(this.DistributorID, this.Locale);
            sessionInfo.CreateHapOrder = true;
            // Systems launches HAP order items price list page
            Response.Redirect("~/Ordering/PriceList.aspx?HAP=True");
        }

        protected void LinkRenewHAPOrder_Click(object sender, EventArgs e)
        {
            HAPModal.Visible = true;
            HAPModal.ModalOption = Ordering.Controls.HAP.ModalType.Renew;
            HAPModal.SelectModal();
        }

        protected void lstHAPOrders_ItemCommand(object sender, ListViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ViewOrder":
                    //Redirect to MyOrders (MyAccount)
                    Response.Redirect(getMyOrderDetailsLink(e.CommandArgument.ToString()));
                    break;
                case "EditOrder":
                    if (hasAPSPayment(e.CommandArgument.ToString()))
                    {
                        HAPModal.Visible = true;
                        HAPModal.ModalOption = Ordering.Controls.HAP.ModalType.EditHapAPS;
                        HAPModal.HAPOrderID = e.CommandArgument.ToString();
                        HAPModal.SelectModal();
                    }
                    else
                    {
                        //redirect to COP1 passing via querystring the #hap order
                        Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?HAP=True&hapId={0}", e.CommandArgument));
                    }
                    break;
                case "CancelOrder":
                    //Display Modal pupup for cancel a Hap Order and send the HAP Order ID to cancel
                    HAPModal.Visible = true;
                    HAPModal.ModalOption = Ordering.Controls.HAP.ModalType.Cancel;
                    HAPModal.HAPOrderID = e.CommandArgument.ToString();
                    HAPModal.SelectModal();
                    break;
            }
        }
        #endregion

        protected void lstHAPOrders_DataBound(object sender, EventArgs e)
        {
            ListView listviewHAP = (ListView)sender;
            if (HLConfigManager.Configurations.DOConfiguration.DisplayBifurcationKeys && ShoppingCart.DsType != null && ShoppingCart.DsType == Scheme.Member)
            {
                if (listviewHAP.FindControl("lbTblHead_Volume") != null )
                {
                    (listviewHAP.FindControl("lbTblHead_Volume") as Literal).Text = GetLocalResourceObject("lbVolumeResource1MB.Text").ToString();
                }
            }
        }
    }
}