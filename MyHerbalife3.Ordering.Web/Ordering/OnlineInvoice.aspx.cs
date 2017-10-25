using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.MasterPages;
using System.Globalization;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using HL.Common.Configuration;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Web.Ordering
{


    /// <summary>
    ///     The Online Invoice
    /// </summary>
    public partial class OnlineInvoice : ProductsBase
    {
        private int _currentPageIndex;
        public const string YearMonthDayFormat = "yyyy-MM-dd";
        private string _startDate;
        private string _endDate;
        private DateTime _dtStartDate;
        private DateTime _dtEndDate;

        /// <summary>
        ///     The page_ load.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        /// 
        public static readonly int InvoiceStatusCacheMin = Settings.GetRequiredAppSetting<int>("OnlineInvoiceCacheExpireMinutes");
        public const string InvoiceOrderDetails = "InvoiceDetails_{0}";
        protected void Page_Load(object sender, EventArgs e)
        {
            SetDateRange(true);
            if (!IsPostBack)
            {
                var orderingMaster = Master as OrderingMaster;
                if (orderingMaster != null) orderingMaster.SetPageHeader(GetLocalResourceString("OrderInvoicePageHeader.Text"));

                txtStartDate.Text = _startDate;
                txtEndDate.Text = _endDate;
                CreateInvoiceView(_startDate, _endDate);
            }
            _currentPageIndex = PagingControl1.FirstItemIndex;


        }

        private void SetDateRange(bool isInitial)
        {
            if (isInitial)
            {
                _startDate = DateTime.Now.AddDays(-90).ToString("yyyy-MM-dd");
                _endDate = DateTime.Now.ToString("yyyy-MM-dd");
            }
            else
            {
                _startDate = string.IsNullOrEmpty(txtStartDate.Text) ? null : txtStartDate.Text;
                _endDate = string.IsNullOrEmpty(txtEndDate.Text) ? null : txtEndDate.Text;

            }

            _dtStartDate = DateTime.ParseExact(_startDate, YearMonthDayFormat, CultureInfo.InvariantCulture);
            _dtEndDate = DateTime.ParseExact(_endDate, YearMonthDayFormat, CultureInfo.InvariantCulture);
        }

        static bool IsValidDate(string yearMonthDay)
        {
            if (string.IsNullOrWhiteSpace(yearMonthDay) || (yearMonthDay.Length != 10)) return false;
            DateTime tmpDateTime;
            if (DateTime.TryParseExact(yearMonthDay, YearMonthDayFormat, null, DateTimeStyles.None, out tmpDateTime))
            {
                //ECMAScript min date
                return tmpDateTime.Year >= 1970;
            }
            return false;
        }

        protected void SearchOrderInfo(object source, EventArgs e)
        {
            SetDateRange(false);
            if (!VerifyDateRange())
            {
                lblNoRecords.Visible = true;
                //CreateInvoiceView("1800-01-01", "1800-01-01");
                return;
            }
            lblNoRecords.Visible = false;

            PagingControl1.CurrentPage = 0;
            _currentPageIndex = 0;
            CreateInvoiceView(_startDate, _endDate);
            PagingControl1.BuildPages();
        }

        private bool VerifyDateRange()
        {
            TimeSpan span = _dtEndDate.Subtract(_dtStartDate);

            var currentDate = DateTime.Now;
            var lastNinetyDays = DateTime.Now.AddDays(-90).Date;



            if (_dtStartDate > _dtEndDate)
            {
                lblNoRecords.Text = GetLocalResourceString("StartEndDateValidationFail.Text");
                CreateInvoiceView("1971-01-01", "1971-01-01");
                return false;
            }
            if (InvoiceStatusRadioButton.SelectedValue == "0") // Current
            {
                              
                if (!((_dtStartDate >= lastNinetyDays && _dtStartDate <= currentDate) && (_dtEndDate >= lastNinetyDays && _dtEndDate <= currentDate)))
                {
                    lblNoRecords.Text = GetLocalResourceString("ErrorOnlineInvoiceLast90Data.Text");
                    CreateInvoiceView("1971-01-01", "1971-01-01");
                    return false;
                }
                if (FutureDates(_dtStartDate, _dtEndDate))
                {
                    return true;
                }
            }
            if (span.TotalDays > 90)
            {
                lblNoRecords.Text = GetLocalResourceString("Error3MonthsRangeOnly.Text");
                CreateInvoiceView("1971-01-01", "1971-01-01");
                return false;
            }


            if (InvoiceStatusRadioButton.SelectedValue == "1") // Archived
            {
                if (span.TotalDays > 90)
                {
                    lblNoRecords.Text = GetLocalResourceString("Error3MonthsRangeOnly.Text");
                    CreateInvoiceView("1971-01-01", "1971-01-01");
                    return false;
                }
                else
                {
                    return true;
                }
            }


            return true;
        }

        private bool FutureDates(DateTime dtStartDate, DateTime dtEndDate)
        {
            var lastNinetyDays = DateTime.Now.AddDays(-90).Date;
            if ((dtStartDate >= lastNinetyDays && dtStartDate < dtEndDate) && dtEndDate > dtStartDate)
                return true;
            return false;
        }

        public void CreateInvoiceView(string startYearMonthDay, string endYearMonthDay)
        {
            #region determine startDate and endDate
            DateTime? startDate;
            if (IsValidDate(startYearMonthDay))
            {
                startDate = DateTime.ParseExact(startYearMonthDay, YearMonthDayFormat, CultureInfo.InvariantCulture);
            }
            else
            {
                var proposedStartDate = DateTime.Now.AddDays(-30);
                startDate = new DateTime(proposedStartDate.Year, proposedStartDate.Month, proposedStartDate.Day);
                txtStartDate.Text = startDate.Value.ToString(YearMonthDayFormat);
            }

            DateTime? endDate;
            if (IsValidDate(endYearMonthDay))
            {
                endDate = DateTime.ParseExact(endYearMonthDay, YearMonthDayFormat, CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);
            }
            else
            {
                endDate = DateTime.Now;
                txtEndDate.Text = endDate.Value.ToString(YearMonthDayFormat);
            }
            #endregion


            var htmlTable = new StringBuilder();
            htmlTable.Append("<div>");
            htmlTable.Append("<table id=\"TABLE-ID\" class=\"rgMasterTable order-list-view\" border=\"0\" >");
            htmlTable.Append("<thead>" +
                             "<tr>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("OrderNumberResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("OrderSourceResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("DCResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("OrderMonthResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("DeliveryMethodResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("OrderStatusResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("OrderDateResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("SubTotalResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("FreightResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("TotalAmountResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("VolumeTotalResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("InvoiceStatusResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("ActionResource.Text") + "</th>" +
                             "<th class=\"rgHeader\" scope=\"col\">" + GetLocalResourceObject("RegisteredNoResource.Text") + "</th>" +
                            "</tr>" +
                            "</thead>");
            htmlTable.Append("<tbody>");

            DateTime pStartDate = (DateTime)startDate;
            DateTime pEndDate = (DateTime)endDate;

            var pOrders = new List<OnlineOrder>();
            string cacheKey = string.Format(InvoiceOrderDetails, DistributorID);
            var cacheResult = HttpRuntime.Cache[cacheKey] as List<OnlineOrder>;

            if (cacheResult != null)
            {
                pOrders = cacheResult;
            }
            else
            {

                pOrders = OrderProvider.GetOrdersWithDetail(DistributorID,
                                                 DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode).CNCustomorProfileID,
                                                 CountryCode, pStartDate, pEndDate, Providers.China.OrderStatusFilterType.Complete, "", "", true);
                HttpRuntime.Cache.Insert(cacheKey, pOrders, null, DateTime.Now.AddMinutes(InvoiceStatusCacheMin),
                  Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }

            // ensure that the order only belong to GDO
            var orders = pOrders.OrderByDescending(x => x.ReceivedDate).ToList();
            var etoSkuList = Settings.GetRequiredAppSetting("ETOSkuList", string.Empty).Split('|');
            var pureOrders = (from items in orders from ordersItem in items.OrderItems where ordersItem.SKU == null || (ordersItem.SKU.Trim() == "9909" && items.OrderItems.Count == 1 || etoSkuList.Contains(ordersItem.SKU.Trim())) select items).ToList();
            foreach (var item in pureOrders)
            {
                orders.Remove(item);
            }
            string[] orderNumbers = orders.Select(o => o.OrderID).ToArray();
            var orderStatus = OrderProvider.GetOrderStatus(orderNumbers, DistributorID);

            if (orders != null)
            {

                int maximumRows = 5;
                int startIndex = PagingControl1.FirstItemIndex;
                PagingControl1.TotalRecordsCount = orders.Count;


                //var maxRows = startIndex + maximumRows > orders.Count? orders.Count - startIndex: maximumRows;

                //orders = orders.GetRange(maxRows >= 0 ? startIndex : 0, maxRows >= 0 ? maxRows : 0);
                if (orders.Count > 0)
                {
                    if (orders.Count < 5)
                    {
                        PagingControl1.Visible = false;
                    }
                    if (InvoiceStatusddl.SelectedValue == "0" || InvoiceStatusddl.SelectedValue == "01")
                    {
                        if (orders.Count <= maximumRows)
                        {
                            orders = orders.GetRange(0, orders.Count);
                        }
                        else
                        {
                            if (startIndex > 0)
                            {
                                var maxrow = orders.Count - startIndex <= maximumRows ? orders.Count - startIndex : maximumRows;
                                orders = orders.GetRange(startIndex, maxrow);
                            }
                            else
                            {
                                orders = orders.GetRange(0, maximumRows);
                            }
                        }
                    }
                    //orders = orders.GetRange((startIndex == 0) ? 0 : startIndex + 1, orders.Count - 1 - startIndex > 5 ? 5 : orders.Count - 1 - startIndex);
                    PagingControl1.Visible = true;
                    lblNoRecords.Visible = false;
                }
                else
                {
                    lblNoRecords.Text = GetLocalResourceString("GridNoRecordsMessage.Text");
                    TimeSpan span = _dtEndDate.Subtract(_dtStartDate);
                    if (span.TotalDays > 90)
                    {
                        lblNoRecords.Text = GetLocalResourceString("Error3MonthsRangeOnly.Text");

                    }


                    lblNoRecords.Visible = true;
                    PagingControl1.Visible = false;

                }
                int i = 0;
                int y = 0;

                foreach (var order in orders)
                {
                    i++;

                    var invoiceDetailStatus = orderStatus.FirstOrDefault(z => z.OrderNumber == order.OrderID);
                    if (invoiceDetailStatus == null)
                    {
                        invoiceDetailStatus = new InvoiceInfoObject();
                        invoiceDetailStatus.InvoiceStatus = "01";
                    }
                    if (invoiceDetailStatus != null && (invoiceDetailStatus.InvoiceStatus == InvoiceStatusddl.SelectedValue) || (InvoiceStatusddl.SelectedValue == "0"))
                    {
                        y++;

                        decimal Apfprice = 0;
                        var apfSku = (from c in order.OrderItems where APFDueProvider.IsAPFSku((c.SKU != null ? c.SKU.Trim() : "")) select c).FirstOrDefault();

                        if (order.OrderItems[0].SKU != null && apfSku != null)
                        {
                            var APFSkuDetails = CatalogProvider.GetCatalogItem(apfSku.SKU.Trim(), CountryCode);
                            Apfprice = APFSkuDetails.ListPrice;
                        }

                        var invoiceStatus = invoiceDetailStatus != null ? GetInvoiceStatus(invoiceDetailStatus.InvoiceStatus) : GetLocalResourceString("InvoiceStatusUnbilled.Text");

                        var priceInfo = order.Pricing as ServiceProvider.OrderChinaSvc.OrderTotals_V01 ?? new ServiceProvider.OrderChinaSvc.OrderTotals_V01();
                        var priceInfoV02 = order.Pricing as ServiceProvider.OrderChinaSvc.OrderTotals_V02;
                        var shipInfo = order.Shipment as ServiceProvider.OrderChinaSvc.ShippingInfo_V01;
                        //Mapping object issue
                        decimal freightCharges = priceInfoV02 != null && null != priceInfoV02.ChargeList && priceInfoV02.ChargeList.Any() ? GetFreightCharges(priceInfoV02.ChargeList as ServiceProvider.OrderChinaSvc.ChargeList) : decimal.Zero;
                        // decimal freightCharges = 0;
                        string carrier;
                        if (shipInfo != null && shipInfo.Carrier == "EXP")
                            carrier = GetLocalResourceString("InvoiceDeliveryMethod.Text");
                        else if (shipInfo != null && shipInfo.Carrier == "SD")
                            carrier = GetLocalResourceString("InvoicePickupFromStoreMethod.Text");
                        else
                            carrier = GetLocalResourceString("InvoiceSelftPickupMethod.Text");

                        htmlTable.Append("<tr class='" + ((i % 2 != 0) ? "rgRow" : "rgAltRow") + "'>");
                        //htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\" onclick='ShowOrHideDetail(" + order.OrderID + ",\"" + (order.InvoiceOrder == null?"":order.InvoiceOrder.Category) + "\")' >" + order.OrderID + "</span></td>");
                        if (order.InvoiceOrder != null && order.InvoiceOrder.InvoiceId < 1)
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.OrderID + "</td>");
                        else
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\"><a href=\"onlineinvoicedetail.aspx?type=view&orderid=" + order.OrderID + "&startDate=" + _startDate + "&endDate=" + _endDate + "\" >" + order.OrderID + "</td>");
                        //htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\"><a href=\"onlineinvoicedetail.aspx?type=view&orderid=" + order.OrderID + "\" >" + order.OrderID + "</td>");

                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.ChannelInfo + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.RDCName + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.OrderMonth + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + carrier + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.Status + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + order.ReceivedDate.ToString("yyyy-MM-dd") + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + (priceInfo.ItemsTotal - Apfprice).ToString("0.##") + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + freightCharges.ToString("0.##") + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + (priceInfo.ItemsTotal + freightCharges - Apfprice).ToString("0.##") + "</span></td>");
                        //htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + priceInfoV02.ItemsTotal.ToString("0.##") + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + priceInfo.VolumePoints.ToString("0.##") + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + invoiceStatus + "</span></td>");
                        htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\"><span>");
                        if (order.InvoiceOrder != null && order.InvoiceOrder.InvoiceId < 1)
                        {
                            if (invoiceDetailStatus.PostNumber == null)
                            {
                                htmlTable.Append("<a onclick=\"showModalPopUp('single','" + order.OrderID + "','" + _startDate + "','" + _endDate + "')\" runat=\"server\">" + GetLocalResourceString("OnlineInvoiceSingleBilling.Text") + "</asp:HyperLink>");
                                htmlTable.Append("<br/><br/><a onclick=\"showModalPopUp('split','" + order.OrderID + "','" + _startDate + "','" + _endDate + "')\" runat=\"server\">" + GetLocalResourceString("OnlineInvoiceSplitBilling.Text") + "</asp:HyperLink>");
                            }

                            //htmlTable.Append("<a href=\"onlineinvoicedetail.aspx?type=single&orderid=" + order.OrderID + "&startDate=" + _startDate + "&endDate=" + _endDate + "\">"  + "开票");
                            //htmlTable.Append("<br/><a href=\"onlineinvoicedetail.aspx?type=split&orderid=" + order.OrderID + "&startDate=" + _startDate + "&endDate=" + _endDate + "\">" + "拆票");
                        }
                        htmlTable.Append("</span></td>");

                        if (invoiceDetailStatus != null)
                        {
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + invoiceDetailStatus.PostNumber + "</span></td>");
                        }
                        else
                        {
                            htmlTable.Append("<td class=\"olv-vpoints\" scope=\"col\"><span id=\"lblVolumeCoulmn\">" + "" + "</span></td>");
                        }
                        htmlTable.Append("</tr>");

                    }

                    if (invoiceDetailStatus != null && (invoiceDetailStatus.InvoiceStatus == InvoiceStatusddl.SelectedValue) || (InvoiceStatusddl.SelectedValue == "0"))
                    {
                        PagingControl1.Visible = true;
                        lblNoRecords.Visible = false;
                       
                    }
                    else if (y < 1)
                    {
                        lblNoRecords.Text = GetLocalResourceString("GridNoRecordsMessage.Text");
                        lblNoRecords.Visible = true;
                        PagingControl1.Visible = false;
                    }

                }
                htmlTable.Append("</tbody>");
                htmlTable.Append("</table>");
                htmlTable.Append("</div>");
                htmlTable.Append("<div align='right'>" + GetLocalResourceString("OnlineInvoiceDetailFooter.Text") + "</div>");
                var result = htmlTable.ToString();
                OrderViewPlaceHolder.Controls.Clear();
                OrderViewPlaceHolder.Controls.Add(new Literal { Text = result });
            }
        }

        //private List<Order> GetFilteredOrders(List<Order> orders, string selectedValue)
        //{
        //    switch (selectedValue)
        //    {
        //        case "0": //All
        //            return orders;

        //        default:
        //            return orders.Where(x=>x.InvoiceOrder.InvoiceStatus==selectedValue).ToList();


        //    }
        //}

        private string GetInvoiceStatus(string invoiceStatus)
        {
            switch (invoiceStatus)
            {
                case "01":
                    return GetLocalResourceString("InvoiceStatusUnbilled.Text");
                case "02":
                    return GetLocalResourceString("InvoiceStatusApplication.Text");
                case "03":
                    return GetLocalResourceString("InvoiceStatusInvoiced.Text");
                case "04":
                    return GetLocalResourceString("InvoiceStatusFailed.Text");
                default:
                    return string.Empty;
            }
        }

        [WebMethod(EnableSession = true)]
        public static bool SetNeverShowAgain(bool bolRemoveSession)
        {
            try
            {
                if (bolRemoveSession)
                {
                    HttpContext.Current.Session["OnlineOrderShowOnce"] = false;
                }
                else
                {
                    HttpContext.Current.Session["OnlineOrderShowOnce"] = true;
                }

                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        [WebMethod]
        public static bool GetNeverShowAgainSession()
        {
            try
            {
                return HttpContext.Current.Session["OnlineOrderShowOnce"] != null && ((bool)HttpContext.Current.Session["OnlineOrderShowOnce"]);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        protected string IsDisplay(InvoiceOrder_V01 invoiceOrder)
        {
            return invoiceOrder.InvoiceId < 1 ? string.Empty : "display:none";
        }

        protected decimal GetFreighCharge(OrderTotals pricing)
        {
            var priceInfoV02 = pricing as OrderTotals_V02;
            decimal freightCharges = priceInfoV02 != null && null != priceInfoV02.ChargeList && priceInfoV02.ChargeList.Any() ? GetFreightCharges(priceInfoV02.ChargeList) : decimal.Zero;
            return freightCharges;
        }

        private decimal GetFreightCharges(ChargeList chargeList)
        {
            var chargeV01S =
                (from ite in chargeList
                 where ite is Charge_V01 && ((Charge_V01)ite).ChargeType == ChargeTypes.FREIGHT
                 select ite as Charge_V01).ToList();
            if (chargeV01S.Any())
            {
                var firstOrDefault = chargeV01S.FirstOrDefault();
                if (firstOrDefault != null) return firstOrDefault.Amount;
            }
            return decimal.Zero;

        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs e = (CommandEventArgs)args;

            //if (txtSearch.Text == GetLocalResourceObject("Search.Text") as string)
            //{
            //    txtSearch.Text = "";
            //}

            //Go to Next Page
            if (string.Equals(e.CommandName, "MoveNext", StringComparison.OrdinalIgnoreCase))
            {
                CreateInvoiceView(_startDate, _endDate);
            }
            //Go to Previous Page
            else if (string.Equals(e.CommandName, "MovePrev", StringComparison.OrdinalIgnoreCase))
            {
                CreateInvoiceView(_startDate, _endDate);
            }
            //Go to First Page
            else if (string.Equals(e.CommandName, "MoveFirst", StringComparison.OrdinalIgnoreCase))
            {
                CreateInvoiceView(_startDate, _endDate);
            }
            //Go to Last Page
            else if (string.Equals(e.CommandName, "MoveLast", StringComparison.OrdinalIgnoreCase))
            {
                CreateInvoiceView(_startDate, _endDate);
            }
            //Go to Page Number ...
            else if (string.Equals(e.CommandName, "GoToPage", StringComparison.OrdinalIgnoreCase))
            {
                CreateInvoiceView(_startDate, _endDate);
            }

            return base.OnBubbleEvent(source, e);
        }
    }
}