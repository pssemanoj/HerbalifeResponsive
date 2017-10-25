using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Web.MasterPages;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using Order = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.OnlineOrder;
using System.Web.Script.Serialization;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OnlineInvoiceDetail : ProductsBase
    {
        private string _orderNumber;
        private string _type;
        private string _startDate;
        private string _endDate;
        private List<Order> _orders;
        private Order _currentOrder;
        private Order _currentOrderAddress;


        protected void Page_Load(object sender, EventArgs e)
        {
            _orderNumber = Request.QueryString["orderid"];
            _type = Request.QueryString["type"];
            _startDate = Request.QueryString["startDate"];
            _endDate = Request.QueryString["endDate"];

            decimal apfAmount = 0;

            if (_orderNumber == null)
            {
                divDetailSingle.Visible = false;
                divDetailSplit.Visible = false;
                return;
            }

            //IEnumerable<Order> testGetLatestDateOrders;
            IEnumerable<Order> orders = GetOrders();

            Order currentOrder = new Order(); //= Orders.Where(x => x.OrderID == _orderNumber);
            if (orders.Any(x => x.OrderID == _orderNumber))
            {
                currentOrder = orders.FirstOrDefault(x => x.OrderID == _orderNumber);
                _currentOrder = currentOrder;
                apfAmount = GetApfAmount(currentOrder);
            }

            if (orders.Any(x => x.InvoiceOrder.InvoiceId > 0))
            {
                var latestInvoice = orders.OrderByDescending(t => t.InvoiceOrder.UpdatedOn).First(); //= Orders.Where(x => x.OrderID == _orderNumber);
                _currentOrderAddress = latestInvoice;
            }
            else
            {
                _currentOrderAddress = currentOrder;
            }

            OrderTotals_V01 priceInfo;
            OrderTotals_V02 priceInfoV02;
            if (currentOrder != null)
            {
                priceInfo = currentOrder.Pricing as OrderTotals_V01;
                priceInfoV02 = currentOrder.Pricing as OrderTotals_V02;
            }
            else
            {
                priceInfo = new OrderTotals_V01();
                priceInfoV02 = new OrderTotals_V02();
            }
            decimal freightCharges = priceInfoV02 != null && null != priceInfoV02.ChargeList && priceInfoV02.ChargeList.Any() ? GetFreightCharges(priceInfoV02.ChargeList) : decimal.Zero;


            if (!IsPostBack)
            {
                int currentRdc = currentOrder != null ? currentOrder.RDC : 0;
                string header;
                switch (_type)
                {
                    case "single":
                        header = GetLocalResourceString("InvoiceDetailSingleInvoiceHeader.Text");
                        break;
                    case "split":
                        header = GetLocalResourceString("InvoiceDetailSplitInvoiceHeader.Text");
                        break;
                    default:
                        header = GetLocalResourceString("InvoiceDetailDefaultHeader.Text");
                        break;
                }
                var rdcConfig = OrderProvider.GetRdcConfig(currentRdc);
                if (rdcConfig == null || rdcConfig.Count == 0)
                    ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "$(document).ready(function(){ShowMessageError();});", true);

                var orderingMaster = Master as OrderingMaster;
                if (orderingMaster != null) orderingMaster.SetPageHeader(header);
                LookupStates();
                LookupStatesSplit();
                EnablingSelectiveControl(currentRdc);
            }

            BindUi(currentOrder, priceInfo, apfAmount, freightCharges);

        }

        private void BindUi(Order currentOrder, OrderTotals_V01 priceInfo, decimal apfAmount, decimal freight)
        {
            switch (_type)
            {
                case "single":
                    divDetailSingle.Visible = true;
                    divDetailSplit.Visible = false;
                    btnConfirm.Enabled = true;

                    if (priceInfo != null) lblInvoiceAmount.InnerText = (priceInfo.AmountDue + freight - apfAmount).ToString("0.##");
                    break;
                case "split":
                    divDetailSingle.Visible = false;
                    divDetailSplit.Visible = true;
                    txtSplitInvoice.Visible = false;
                    ddlSplitInvoice.Visible = true;
                    btnSplitConfirm.Enabled = true;
                    if (priceInfo != null) lblSplitAmount.InnerText = (priceInfo.AmountDue + freight - apfAmount).ToString("0.##");
                    break;
                case "view":

                    panelSingle.Enabled = false;
                    panelSplit.Enabled = false;

                    if (!IsPostBack)
                    {
                        if (currentOrder.InvoiceOrder.Category == "SI")
                            DisplayViewSingle(currentOrder);
                        else
                            DisplayViewSplit(currentOrder);
                    }
                    break;
                default:
                    divDetailSingle.Visible = false;
                    divDetailSplit.Visible = false;
                    break;
            }
        }

        private decimal GetApfAmount(Order order)
        {
            if (order != null && order.OrderItems != null)
            {
                var apfSku = (from c in order.OrderItems where APFDueProvider.IsAPFSku((c.SKU != null ? c.SKU.Trim() : "")) select c).FirstOrDefault();
                if (apfSku != null)
                {
                    var price = apfSku as OnlineOrderItem;
                    return price != null ? price.RetailPrice : 0;
                }
                return 0;
            }
            return 0;
        }


        private void DisplayViewSingle(Order order)
        {
            panelSingle.Enabled = false;
            panelSplit.Enabled = false;
            btnConfirm.Enabled = false;
            btnConfirm.Visible = false;
            var type = order.InvoiceOrder.Category;
            var priceInfo = order.Pricing as OrderTotals_V01;
            var priceInfoV02 = order.Pricing as OrderTotals_V02;
            decimal freightCharges = priceInfoV02 != null && null != priceInfoV02.ChargeList && priceInfoV02.ChargeList.Any() ? GetFreightCharges(priceInfoV02.ChargeList) : decimal.Zero;
            decimal apfAmount = GetApfAmount(order);

            if (type == "SI")
            {
                divDetailSingle.Visible = true;
                divDetailSplit.Visible = false;
                divEdit.Visible = false;
                divView.Visible = true;
                bttnDummy.Visible = false;
                chkDefaultAddr.Visible = false;


                lblInvoiceAmount.InnerText = priceInfo != null ? (priceInfo.AmountDue + freightCharges - apfAmount).ToString("0.##") : "0";

                rbInvoiceTitle.SelectedIndex = order.InvoiceOrder.InvoiceType == "PE" ? 0 : 1;

                switch (order.InvoiceOrder.InvoiceTitle)
                {
                    case "PE":
                        rbInvoiceTitle.SelectedIndex = 0;
                        break;
                    case "CU":
                        rbInvoiceTitle.SelectedIndex = 1;
                        break;
                    default:
                        rbInvoiceTitle.SelectedIndex = 2;
                        break;
                }

                switch (order.InvoiceOrder.InvoiceContent)
                {
                    case "PF":
                        ddlInvoiceContent.SelectedIndex = 0;
                        break;
                    case "PD":
                        ddlInvoiceContent.SelectedIndex = 1;
                        break;
                    default:
                        ddlInvoiceContent.SelectedIndex = 2;
                        break;
                }


                lblStreet.Text = order.InvoiceOrder.Street;
                lblPostal.Text = order.InvoiceOrder.PostalCode;
                txtReceiver.Text = order.InvoiceOrder.Receiver;
                txtEmail.Text = order.InvoiceOrder.email;
                txtPhone.Text = order.InvoiceOrder.Phone;
                lblProvice.Text = order.InvoiceOrder.ProvinceNameCn;
                lblCity.Text = order.InvoiceOrder.City;
                lblDistrict.Text = order.InvoiceOrder.District;

            }
            else if (type == "SP")
            {
                divDetailSingle.Visible = false;
                divDetailSplit.Visible = true;
            }

        }
        private void DisplayViewSplit(Order order)
        {
            var priceInfo = order.Pricing as OrderTotals_V01;
            decimal apfAmount = GetApfAmount(order);

            divDetailSingle.Visible = false;
            divDetailSplit.Visible = true;
            divSplitEdit.Visible = false;
            divSplitView.Visible = true;
            chkSplitAddressConfirm.Visible = false;
            txtSplitInvoice.Visible = true;
            ddlSplitInvoice.Visible = false;
            btnSplitConfirm.Enabled = false;
            btnSplitConfirm.Visible = false;
            btnsplitDummy.Visible = false;

            var priceInfoV02 = order.Pricing as OrderTotals_V02;
            decimal freightCharges = priceInfoV02 != null && null != priceInfoV02.ChargeList && priceInfoV02.ChargeList.Any() ? GetFreightCharges(priceInfoV02.ChargeList) : decimal.Zero;


            lblSplitAmount.InnerText = priceInfo != null ? (priceInfo.AmountDue + freightCharges - apfAmount).ToString("0.##") : "0";
            txtSplitInvoice.Text = order.InvoiceOrder.SplitCount.ToString().Trim();

            txtInvoiceAmount1.Text = order.InvoiceOrder.Amount1.ToString("0.##");
            txtInvoiceAmount2.Text = order.InvoiceOrder.Amount2.ToString("0.##");
            txtInvoiceAmount3.Text = order.InvoiceOrder.Amount3.ToString("0.##");

            rbSplitInvoiceType.SelectedIndex = order.InvoiceOrder.InvoiceType == "PE" ? 0 : 1;

            switch (order.InvoiceOrder.InvoiceTitle)
            {
                case "PE":
                    rbSplitInvoiceTitle.SelectedIndex = 0;
                    break;
                case "CU":
                    rbSplitInvoiceTitle.SelectedIndex = 1;
                    break;
            }

            switch (order.InvoiceOrder.InvoiceContent)
            {
                case "PF":
                    ddlSplitInvoiceContext.SelectedIndex = 0;
                    break;
                case "PD":
                    ddlSplitInvoiceContext.SelectedIndex = 1;
                    break;
                default:
                    ddlSplitInvoiceContext.SelectedIndex = 2;
                    break;
            }

            txtSplitReceiver.Text = order.InvoiceOrder.Receiver;
            txtSplitEmail.Text = order.InvoiceOrder.email;
            txtSplitPhone.Text = order.InvoiceOrder.Phone;
            lblSplitProvince.Text = order.InvoiceOrder.ProvinceNameCn;
            lblSplitCity.Text = order.InvoiceOrder.City;
            lblSplitDistrict.Text = order.InvoiceOrder.District;
            lblSplitStreet.Text = order.InvoiceOrder.Street;
            lblSplitPostal.Text = order.InvoiceOrder.PostalCode;
        }
        private List<Order> GetOrders()

        {
            DateTime startdate = DateTime.ParseExact(_startDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            DateTime enddate = DateTime.ParseExact(_endDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).AddDays(1).AddMilliseconds(-1);

            var orders = OrderProvider.GetOrdersWithDetail(DistributorID,
                                              DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode).CNCustomorProfileID,
                                              CountryCode, startdate, enddate, Providers.China.OrderStatusFilterType.Complete, "", "", true);

            if (orders == null)
                return new List<Order>();
            return orders;

        }

        protected void btnConfirm_Click(object sender, EventArgs e)
        {
            bttnDummy.Enabled = false;
            var success = UpdateOrderInvoice();

            if (success)
            {
                ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "$(document).ready(function(){ShowMessage();});", true);
                blErrors.Visible = true;
            }
            else
            {
                blErrors.Visible = true;
                ScriptManager.RegisterStartupScript(this, typeof(Page), "EnableSubmit", "$(document).ready(function(){ EnableDummy();});", true);
            }
        }

        protected bool VerifySplitOrder()
        {
            bool valid = true;
            blErrors.Items.Clear();
            decimal amt1, amt2, amt3 = 0;
            decimal.TryParse(txtInvoiceAmount1.Text, out amt1);
            decimal.TryParse(txtInvoiceAmount2.Text, out amt2);
            decimal.TryParse(txtInvoiceAmount3.Text, out amt3);
            //int amt1 = string.IsNullOrEmpty(txtInvoiceAmount1.Text) ? 0 : int.Parse(txtInvoiceAmount1.Text);
            //int amt2 = string.IsNullOrEmpty(txtInvoiceAmount2.Text) ? 0 : int.Parse(txtInvoiceAmount2.Text);
            //int amt3 = string.IsNullOrEmpty(txtInvoiceAmount3.Text) ? 0 : int.Parse(txtInvoiceAmount3.Text);

            decimal amountWithOutApf = lblSplitAmount.InnerText != "" ? decimal.Parse(lblSplitAmount.InnerText) : 0;

            var errors = blErrors.DataSource as List<string> ?? new List<string>();

            if (ddlSplitInvoice.SelectedValue.Trim() == "2")
            {
                if (amt1 < 0) errors.Add(GetLocalResourceString("ErrorSplitAmount1.Text"));
                if (amt2 < 0) errors.Add(GetLocalResourceString("ErrorSplitAmount2.Text"));
                if (amt1 + amt2 > amountWithOutApf || (amt1 + amt2) < amountWithOutApf) errors.Add(string.Format(GetLocalResourceString("ErrorSplitAmountNotMatch.Text"), amt1 + amt2, amountWithOutApf - (amt1 + amt2)));
            }
            else
            {
                if (amt1 < 0) errors.Add(GetLocalResourceString("ErrorSplitAmount1.Text"));
                if (amt2 < 0) errors.Add(GetLocalResourceString("ErrorSplitAmount2.Text"));
                if (amt3 < 0) errors.Add(GetLocalResourceString("ErrorSplitAmount3.Text"));
                if (amt1 + amt2 + amt3 > amountWithOutApf || (amt1 + amt2 + amt3) < amountWithOutApf)
                    errors.Add(string.Format(GetLocalResourceString("ErrorSplitAmountNotMatch.Text"), amt1 + amt2 + amt3, amountWithOutApf - (amt1 + amt2 + amt3)));
            }

            if (rbSplitInvoiceTitle.SelectedValue == "CU")
                if (string.IsNullOrEmpty(txtSplitInvoiceCustom.Text)) errors.Add(GetLocalResourceString("ErrorEmptyCustomTitle.Text"));

            if (!chkSplitAddressConfirm.Checked)
            {
                if (ddlSplitProvince.SelectedItem == null || ddlSplitProvince.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyProvince.Text"));
                if (ddlSplitCity.SelectedItem == null || ddlSplitCity.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyCity.Text"));
                if (ddlSplitDistrict.SelectedItem == null || ddlSplitDistrict.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyDistrict.Text"));
                if (string.IsNullOrEmpty(txtSplitStreet.Text.Trim())) errors.Add(GetLocalResourceString("EmptyStreetAddress.Text"));
                if (string.IsNullOrEmpty(txtSplitPostal.Text.Trim())) errors.Add(GetLocalResourceString("EmptyPostCode.Text"));

                if (!string.IsNullOrEmpty(txtSplitPostal.Text) && txtSplitPostal.Text.Length != 6)
                {
                    errors.Add(GetLocalResourceString("InvoiceDetailPostCodeInvalidLength.Text"));
                }
            }
            else
            {
                if (lblSplitPostal.Text.Length != 6)
                {
                    errors.Add(GetLocalResourceString("InvoiceDetailPostCodeInvalidLength.Text"));
                }
            }


            if (!string.IsNullOrEmpty(txtSplitEmail.Text))
            {
                if (!Regex.IsMatch(txtSplitEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                {
                    errors.Add(GetLocalResourceString("ErrorInvalidEmailFormat.Text"));
                }
            }

            if ( !string.IsNullOrEmpty(txtSplitPhone.Text) && txtSplitPhone.Text.Length != 11)
            {
                errors.Add(GetLocalResourceString("InvoiceDetailContactNoInvalidLength.Text"));
            }

            if (string.IsNullOrEmpty(txtSplitPhone.Text))
            {
                errors.Add(GetLocalResourceString("EmptyContactNo.Text"));
            }

            if (string.IsNullOrEmpty(txtSplitReceiver.Text.Trim())) errors.Add(GetLocalResourceString("EmptyRecipient.Text"));

            blErrors2.DataSource = errors;
            blErrors2.DataBind();

            if (errors.Count > 0) valid = false;

            return valid;
        }

        protected bool VerifySingleOrder()
        {
            bool valid = true;
            blErrors.Items.Clear();

            var errors = blErrors.DataSource as List<string> ?? new List<string>();

            if (rbSplitInvoiceTitle.SelectedValue == "CO")
                if (string.IsNullOrEmpty(txtInvoiceTitleCorporate.Text)) errors.Add(GetLocalResourceString("ErrorEmptyInvoiceTitle.Text"));

            if (string.IsNullOrEmpty(txtReceiver.Text)) errors.Add(GetLocalResourceString("ErrorEmptyRecipient.Text"));

            if (!chkDefaultAddr.Checked)
            {
                if (ddlProvince.SelectedItem == null || ddlProvince.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyProvince.Text"));
                if (ddlCity.SelectedItem == null || ddlCity.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyCity.Text"));
                if (ddlDistrict.SelectedItem == null || ddlDistrict.SelectedItem.Value == "") errors.Add(GetLocalResourceString("ErrorEmptyDistrict.Text"));
                if (string.IsNullOrEmpty(txtStreet.Text.Trim())) errors.Add(GetLocalResourceString("EmptyStreetAddress.Text"));
                if (string.IsNullOrEmpty(txtPostal.Text.Trim())) errors.Add(GetLocalResourceString("EmptyPostCode.Text"));
                if (!string.IsNullOrEmpty(txtPostal.Text) && txtPostal.Text.Length != 6)
                {
                    errors.Add(GetLocalResourceString("InvoiceDetailPostCodeInvalidLength.Text"));
                }
            }
            else
            {
                if (lblPostal.Text.Length != 6)
                {
                    errors.Add(GetLocalResourceString("InvoiceDetailPostCodeInvalidLength.Text"));
                }
            }
            if (!string.IsNullOrEmpty(txtEmail.Text))
                if (!Regex.IsMatch(txtEmail.Text, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                {
                    errors.Add(GetLocalResourceString("ErrorInvalidEmailFormat.Text"));
                }

            if ( !string.IsNullOrEmpty(txtPhone.Text) && txtPhone.Text.Length != 11)
            {
                errors.Add(GetLocalResourceString("InvoiceDetailContactNoInvalidLength.Text"));
            }
            if (string.IsNullOrEmpty(txtPhone.Text))
            {
                errors.Add(GetLocalResourceString("EmptyContactNo.Text"));
            }
           

            blErrors.DataSource = errors;
            blErrors.DataBind();

            if (errors.Count > 0) valid = false;

            return valid;
        }

        protected bool UpdateOrderInvoice()
        {

            try
            {
                ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                if (!VerifySingleOrder()) return false;
              var isdone=  CreateCrmInvoice(false);
                
                if (isdone)
                {
                    UpdateOrderInvoiceRequest_V01 req = new UpdateOrderInvoiceRequest_V01
                    {
                        OrderHeaderID = _currentOrder.OrderHeaderID,
                        Invoice = new InvoiceOrder_V01
                        {
                            InvoiceType = rbInvoiceType.SelectedValue,
                            InvoiceTitle = rbInvoiceTitle.SelectedValue,


                            ProvinceNameCn = chkDefaultAddr.Checked ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn)
                                ? shippingInfo.Address.StateProvinceTerritory
                                : _currentOrderAddress.InvoiceOrder.ProvinceNameCn
                            : ddlProvince.SelectedItem.ToString(),

                            City = chkDefaultAddr.Checked ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City)
                                ? shippingInfo.Address.City
                                : _currentOrderAddress.InvoiceOrder.City
                            : ddlCity.SelectedItem.ToString(),

                            District = chkDefaultAddr.Checked ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District)
                                ? shippingInfo.Address.CountyDistrict
                                : _currentOrderAddress.InvoiceOrder.District
                            : ddlDistrict.SelectedItem.ToString(),

                            PostalCode = chkDefaultAddr.Checked ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode)
                                ? shippingInfo.Address.PostalCode
                                : _currentOrderAddress.InvoiceOrder.PostalCode
                            : txtPostal.Text,
                            email = txtEmail.Text,

                            Phone = txtPhone.Text,
                            Receiver = txtReceiver.Text,
                            Street = chkDefaultAddr.Checked ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.Street) ? shippingInfo.Address.Line1 + "" + shippingInfo.Address.Line2 : _currentOrderAddress.InvoiceOrder.Street : txtStreet.Text,
                            Category = "SI",
                            Amount1 = Decimal.Parse(lblInvoiceAmount.InnerText),
                            InvoiceTitleCustom = GetCustomInvoiceTitle(rbInvoiceType.SelectedValue, rbInvoiceTitle.SelectedValue),
                            InvoiceContent = ddlInvoiceContent.SelectedValue
                        }
                    };
                    req.Invoice.ProvinceID = (chkDefaultAddr.Checked
                        ? int.Parse(ddlProvince.Items.FindByText(req.Invoice.ProvinceNameCn).Value)
                        : int.Parse(ddlProvince.SelectedValue));

                    OrderProvider.UpdateOrderInvoice(DistributorID, req);
                    OrderProvider.RemoveOrdersCache(DistributorID);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("OnlineInvoice.UpdateSplitOrderInvoice() Error \n" + ex.StackTrace);
                return false;
            }
        }

        private string GetCustomInvoiceTitle(string invoiceType, string invoiceTitle)
        {
            if (invoiceType == "CO")
                return txtInvoiceTitleCorporate.Text;
            if (invoiceType == "PE" && invoiceTitle == "CU")
                return txtInvoiceTitleCustom.Text;
            return string.Empty;
        }

        private bool CreateCrmInvoice(bool isSplit)
        {

            if (!isSplit)
            {
                try
                {
                    ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                    ExternalOnlineInvoiceRequest_V01 invoiceObject = new ExternalOnlineInvoiceRequest_V01();

                    invoiceObject.InvoiceType = 0;
                    invoiceObject.TypeOfInvoice = GetTypeOfInvoice(rbInvoiceType.SelectedValue);
                    invoiceObject.InvoiceTitle = GetInvoiceTitle(rbInvoiceTitle.SelectedValue);
                    invoiceObject.City = (chkDefaultAddr.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City)
                            ? shippingInfo.Address.City
                            : _currentOrderAddress.InvoiceOrder.City
                        : ddlCity.SelectedItem.ToString());
                    invoiceObject.Province =
                        chkDefaultAddr.Checked
                            ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn)
                                ? shippingInfo.Address.StateProvinceTerritory
                                : _currentOrderAddress.InvoiceOrder.ProvinceNameCn
                            : ddlProvince.SelectedItem.ToString();
                    invoiceObject.Area =
                        chkDefaultAddr.Checked
                            ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District)
                                ? shippingInfo.Address.CountyDistrict
                                : _currentOrderAddress.InvoiceOrder.District
                            : ddlDistrict.SelectedItem.ToString();
                    invoiceObject.ContactNo = txtPhone.Text;
                    invoiceObject.NoOfSplitOrder = 0;
                    invoiceObject.InvoiceDetails = GetInvoiceDetail(ddlInvoiceContent.SelectedValue);
                    invoiceObject.PostCode = (chkDefaultAddr.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode)
                            ? shippingInfo.Address.PostalCode
                            : _currentOrderAddress.InvoiceOrder.PostalCode
                        : txtPostal.Text);
                    invoiceObject.Recipient = txtReceiver.Text;
                    invoiceObject.EmailAddress = txtEmail.Text;
                    invoiceObject.InvoiceTotal = decimal.Parse(lblInvoiceAmount.InnerText);
                    invoiceObject.Amount = lblInvoiceAmount.InnerText;
                    invoiceObject.CustomerId = DistributorID;
                    invoiceObject.OrderNumbers = "888-" + _orderNumber;

                    invoiceObject.Address = chkDefaultAddr.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn)
                            ? shippingInfo.Address.Line1 + "" + shippingInfo.Address.Line2
                            : _currentOrderAddress.InvoiceOrder.Street
                        : txtStreet.Text;
                    invoiceObject.CustomText = GetCustomInvoiceTitleExternalInvoice(invoiceObject.TypeOfInvoice, invoiceObject.InvoiceTitle);
                     
                    if (ValidateExternalInvoice(invoiceObject,isSplit))
                    {
                        var externalInvoice = OrderProvider.CreateExternalInvoice(invoiceObject);

                        if (externalInvoice != null)
                        {
                            return externalInvoice.ApplyForInvoiceResult;
                        }
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    LoggerHelper.Write(
                        "Error while creating online invoice CRM Side, please check the CRM Service apply for invoice",
                        ex.StackTrace);
                    return false;
                }
            }
            else
            {
                try
                {
                    ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                    ExternalOnlineInvoiceRequest_V01 invoiceObject = new ExternalOnlineInvoiceRequest_V01();
                    invoiceObject.InvoiceType = 1;
                    invoiceObject.TypeOfInvoice = GetTypeOfInvoice(this.rbSplitInvoiceType.SelectedValue);
                    invoiceObject.InvoiceTitle = GetInvoiceTitle(this.rbSplitInvoiceTitle.SelectedValue);
                    invoiceObject.City = chkSplitAddressConfirm.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City) ? shippingInfo.Address.City : _currentOrderAddress.InvoiceOrder.City
                        : ddlSplitCity.SelectedItem.ToString();
                    invoiceObject.Province =
                        chkSplitAddressConfirm.Checked
                            ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn) ? shippingInfo.Address.StateProvinceTerritory : _currentOrderAddress.InvoiceOrder.ProvinceNameCn
                            : ddlSplitProvince.SelectedItem.ToString();
                    invoiceObject.Area =
                        chkSplitAddressConfirm.Checked
                            ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District) ? shippingInfo.Address.CountyDistrict : _currentOrderAddress.InvoiceOrder.District
                            : ddlSplitDistrict.SelectedItem.ToString();
                    invoiceObject.ContactNo = txtSplitPhone.Text;
                    invoiceObject.NoOfSplitOrder = int.Parse(ddlSplitInvoice.SelectedValue);
                    invoiceObject.InvoiceDetails = GetInvoiceDetail(ddlInvoiceContent.SelectedValue);
                    invoiceObject.PostCode = (chkSplitAddressConfirm.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode) ? shippingInfo.Address.PostalCode : _currentOrderAddress.InvoiceOrder.PostalCode
                        : txtSplitPostal.Text);
                    invoiceObject.Recipient = txtSplitReceiver.Text;
                    invoiceObject.EmailAddress = txtSplitEmail.Text;
                    invoiceObject.InvoiceTotal = decimal.Parse(lblSplitAmount.InnerText);
                    invoiceObject.Amount = string.Join(",", txtInvoiceAmount1.Text, txtInvoiceAmount2.Text,
                        txtInvoiceAmount3.Text);
                    invoiceObject.CustomerId = DistributorID;
                    invoiceObject.OrderNumbers = "888-" + _orderNumber;

                    invoiceObject.Address = chkDefaultAddr.Checked
                        ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn)
                            ? shippingInfo.Address.Line1 + "" + shippingInfo.Address.Line2
                            : _currentOrderAddress.InvoiceOrder.Street
                        : txtSplitStreet.Text;

                    invoiceObject.CustomText = txtSplitInvoiceCustom.Text;
                   
                    if (ValidateExternalInvoice(invoiceObject,isSplit))
                    {
                        var externalInvoice = OrderProvider.CreateExternalInvoice(invoiceObject);

                        if (externalInvoice != null)
                        {
                            return externalInvoice.ApplyForInvoiceResult;
                        }
                    }
                    return false;
                }

                catch (Exception ex)
                {
                    LoggerHelper.Write(
                      "Error while creating online invoice CRM Side, please check the CRM Service apply for invoice",
                      ex.StackTrace);
                    return false;
                }
            }
        }
 
        private string GetCustomInvoiceTitleExternalInvoice(int typeOfInvoice, int invoiceTitle)
        {
            if (typeOfInvoice == 1&& invoiceTitle==3)
                return txtInvoiceTitleCorporate.Text;
            if (typeOfInvoice == 0 && invoiceTitle == 3)
                return txtInvoiceTitleCustom.Text;
            return string.Empty;
        }

        private int GetInvoiceDetail(string selectedValue)
        {
            switch (selectedValue)
            {
                case "PD":
                    return 1;
                case "P":
                    return 0;
                case "HF":
                    return 2;
                default:
                    return -1;
            }
        }

        private bool ValidateExternalInvoice(ExternalOnlineInvoiceRequest_V01 invoiceObject,bool IsSplit)
        {
            List<string> errorList = new List<string>();

            if (string.IsNullOrEmpty(invoiceObject.City))
            {
                errorList.Add(GetLocalResourceString("ErrorEmptyCity.Text"));
            }
            if (string.IsNullOrEmpty(invoiceObject.Area))
            {
                errorList.Add(GetLocalResourceString("ErrorEmptyDistrict.Text"));
            }
            if (string.IsNullOrEmpty(invoiceObject.Province))
            {
                errorList.Add(GetLocalResourceString("ErrorEmptyProvince.Text"));
            }
            if (string.IsNullOrEmpty(invoiceObject.PostCode))
            {
                errorList.Add(GetLocalResourceString("EmptyPostCode.Text"));
            }
            if (string.IsNullOrEmpty(invoiceObject.Recipient))
            {
                errorList.Add(GetLocalResourceString("EmptyRecipient.Text"));
            }
            if (string.IsNullOrEmpty(invoiceObject.ContactNo))
            {
                errorList.Add(GetLocalResourceString("EmptyContactNo.Text"));
            }
            if (!string.IsNullOrEmpty(invoiceObject.EmailAddress))
            {
                if (!Regex.IsMatch(invoiceObject.EmailAddress, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase))
                {
                    errorList.Add(GetLocalResourceString("ErrorInvalidEmailFormat.Text"));
                }
            }

          
                if (IsSplit)
            {
                blErrors2.DataSource = errorList;
                blErrors2.DataBind();
            }
            else
            {
                blErrors.DataSource = errorList;
                blErrors.DataBind();
            }
           
            return errorList.Count <= 0;

        }
        private int GetInvoiceTitle(string selectedValue)
        {
            switch (selectedValue)
            {
                case "PE":
                    return 0;
                case "CS":
                    return 1;
                case "OB":
                    return 2;
                case "CU":
                    return 3;
                default:
                    return -1;
            }
        }
        private int GetTypeOfInvoice(string selectedValue)
        {
            switch (selectedValue)
            {
                case "PE":
                    return 0;
                case "CO":
                    return 1;
                default:
                    return -1;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("OnlineInvoice.aspx");
        }

        private void EnablingSelectiveControl(int rdcId)
        {
            var rdcConfig = OrderProvider.GetRdcConfig(rdcId).FirstOrDefault();
            try
            {
                rbInvoiceType.Items.Clear();
                rbInvoiceTitle.Items.Clear();
                ddlInvoiceContent.Items.Clear();
                rbSplitInvoiceType.Items.Clear();
                rbSplitInvoiceTitle.Items.Clear();
                ddlSplitInvoice.Items.Clear();

                if (rdcConfig != null)
                {
                    if (rdcConfig.PersonalInvoice)
                    {
                        rbInvoiceType.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailRadioButtonPersonal.Text"), "PE"));
                        rbSplitInvoiceType.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailRadioButtonPersonal.Text"), "PE"));
                    }

                    if (rdcConfig.CorporateInvoice)
                    {
                        rbInvoiceType.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailRadioButtonCorporate.Text"), "CO"));
                        rbSplitInvoiceType.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailRadioButtonCorporate.Text"), "CO"));
                    }

                    rbInvoiceType.SelectedIndex = 0;
                    rbSplitInvoiceType.SelectedIndex = 0;

                    // Populating Invoice Title
                    if (rdcConfig.Personal)
                    {
                        rbInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailTitleRadioButtonPersonal.Text"), "PE"));
                        rbSplitInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailTitleRadioButtonPersonal.Text"), "PE"));
                    }

                    if (rdcConfig.Customer)
                    {
                        rbInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitleCustomer.Text"), "CS"));
                        rbSplitInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitleCustomer.Text"), "CS"));
                    }

                    if (rdcConfig.Purchaser)
                    {
                        rbInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitlePurchaser.Text"), "OB"));
                        rbSplitInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitlePurchaser.Text"), "OB"));
                    }

                    if (rdcConfig.OrderedBy)
                    {
                        rbInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitleCustomize.Text"), "CU"));
                        rbSplitInvoiceTitle.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailInvoiceTitleCustomize.Text"), "CU"));
                    }

                    rbInvoiceTitle.SelectedIndex = 0;
                    rbSplitInvoiceTitle.SelectedIndex = 0;
                    this.rbInvoiceTitle_SelectedIndexChanged(null, null);
                    this.rbSplitInvoiceTitle_SelectedIndexChanged(null, null);
                    // Populating InvooiceDetail
                    //if (rdcConfig.Healthyfood)
                    //{
                    //    ddlInvoiceContent.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHealthyFood.Text"), "HF"));
                    //    ddlSplitInvoiceContext.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHealthyFood.Text"), "HF"));
                    //}

                    if (rdcConfig.DeliveryFreight)
                    {
                        ddlInvoiceContent.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHerbalifeShipment.Text"), "PD"));
                        ddlSplitInvoiceContext.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHerbalifeShipment.Text"), "PD"));
                    }

                    if (rdcConfig.ProductPrice)
                    {
                        ddlInvoiceContent.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHerbalifeProduct.Text"), "P"));
                        ddlSplitInvoiceContext.Items.Add(new ListItem(GetLocalResourceString("InvoiceDetailProductDetailHerbalifeProduct.Text"), "P"));
                    }
                    ddlSplitInvoice.Items.Add(new ListItem { Text = @"2", Value = @"2" });
                    ddlSplitInvoice.Items.Add(new ListItem { Text = @"3", Value = @"3" });
                    ddlSplitInvoice.SelectedValue = "3";
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Write(
                    string.Format(
                        "There is an error while setting the display of the online invoice :Distributor:{0}, Locale{1}, rdcId{2}, strack trace: {3}",
                        DistributorID, Locale, rdcId, ex.StackTrace), "error");

            }



        }

        private void LookupStates()
        {
            ddlProvince.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry("CN");
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        string[] item = province.Split('-');
                        ddlProvince.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlProvince.SelectedIndex = 0;
                }
            }
        }

        private void LookupStatesSplit()
        {
            ddlSplitProvince.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetStatesForCountry("CN");
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var province in lookupResults)
                    {
                        string[] item = province.Split('-');
                        ddlSplitProvince.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlSplitProvince.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlSplitProvince.SelectedIndex = 0;
                }
            }
        }


        private void LookupCities(string state)
        {

            ddlCity.Items.Clear();
            ddlDistrict.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState("CN", state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        string[] item = city.Split('-');
                        ddlCity.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlCity.SelectedIndex = 0;
                }
                else
                {
                    ddlProvince.Focus();
                }
            }
        }

        private void LookupCitiesSplit(string state)
        {
            ddlSplitCity.Items.Clear();
            ddlSplitDistrict.Items.Clear();
            IShippingProvider provider = ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetCitiesForState("CN", state);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var city in lookupResults)
                    {
                        string[] item = city.Split('-');
                        ddlSplitCity.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlSplitCity.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlSplitCity.SelectedIndex = 0;
                }
                else
                {
                    ddlSplitProvince.Focus();
                }
            }
        }

        private void LookupCounties(string state, string city)
        {
            ddlDistrict.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetStreetsForCity("CN", state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var district in lookupResults)
                    {
                        string[] item = district.Split('-');
                        ddlDistrict.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlDistrict.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlDistrict.SelectedIndex = 0;
                }
                else
                {
                    ddlCity.Focus();
                }
            }
        }

        private void LookupCountiesSplit(string state, string city)
        {
            ddlSplitDistrict.Items.Clear();
            IShippingProvider provider =
                ShippingProvider.GetShippingProvider("CN");
            if (provider != null)
            {
                var lookupResults = provider.GetStreetsForCity("CN", state, city);
                if (lookupResults != null && lookupResults.Count > 0)
                {
                    foreach (var district in lookupResults)
                    {
                        string[] item = district.Split('-');
                        ddlSplitDistrict.Items.Add(new ListItem(item[1], item[0]));
                    }
                    ddlSplitDistrict.Items.Insert(0, new ListItem(GetLocalResourceObject("Select") as string, string.Empty));
                    ddlSplitDistrict.SelectedIndex = 0;
                }
                else
                {
                    ddlSplitCity.Focus();
                }
            }
        }

        protected void ddlProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlProvince.SelectedItem.Value))
            {
                LookupCities(ddlProvince.SelectedItem.Value);
            }
            else
            {
                ddlCity.Items.Clear();
                ddlDistrict.Items.Clear();
            }
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlProvince.SelectedItem.Value) && !string.IsNullOrEmpty(ddlCity.SelectedItem.Value))
            {
                LookupCounties(ddlProvince.SelectedItem.Value, ddlCity.SelectedItem.Value);
            }
            else
            {
                ddlDistrict.Items.Clear();
            }
        }

        protected void ddlDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlProvince.SelectedItem.Value) &&
                    !string.IsNullOrEmpty(ddlCity.SelectedItem.Value) && !string.IsNullOrEmpty(ddlDistrict.SelectedItem.Value))
            {
                ShippingProvider_CN shippingProvider = ShippingProvider.GetShippingProvider("CN") as ShippingProvider_CN;
                if (shippingProvider != null)
                {
                    shippingProvider.GetUnsupportedAddress(ddlProvince.SelectedItem.Text.Trim(), ddlCity.SelectedItem.Text.Trim(),
                                                           ddlDistrict.SelectedItem.Text.Trim());

                }
            }
        }

        protected void ddlSplitInvoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = sender as DropDownList;

            if (ddl != null)
                switch (ddl.SelectedValue)
                {
                    case "1":
                        txtInvoiceAmount1.Visible = true;
                        txtInvoiceAmount2.Visible = false;
                        txtInvoiceAmount3.Visible = false;
                        break;
                    case "2":
                        txtInvoiceAmount1.Visible = true;
                        txtInvoiceAmount2.Visible = true;
                        txtInvoiceAmount3.Visible = false;
                        lblamount3.Visible = false;
                        break;
                    case "3":
                        txtInvoiceAmount1.Visible = true;
                        txtInvoiceAmount2.Visible = true;
                        txtInvoiceAmount3.Visible = true;
                        lblamount3.Visible = true;
                        break;
                }
        }

        protected void chkDefaultAddr_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null && cb.Checked)
            {
                divEdit.Visible = false;
                divView.Visible = true;
                ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                lblProvice.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn) ? shippingInfo.Address.StateProvinceTerritory : _currentOrderAddress.InvoiceOrder.ProvinceNameCn;
                lblCity.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City) ? shippingInfo.Address.City : _currentOrderAddress.InvoiceOrder.City;
                lblDistrict.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District) ? shippingInfo.Address.CountyDistrict : _currentOrderAddress.InvoiceOrder.District;
                lblStreet.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.Street) ? shippingInfo.Address.Line1 : _currentOrderAddress.InvoiceOrder.Street;
                lblPostal.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode) ? shippingInfo.Address.PostalCode : _currentOrderAddress.InvoiceOrder.PostalCode;
            }
            else
            {
                divEdit.Visible = true;
                divView.Visible = false;
            }
        }

        protected void chkSplitAddressConfirm_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb != null && cb.Checked)
            {
                divSplitEdit.Visible = false;
                divSplitView.Visible = true;
                ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                lblSplitProvince.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn) ? shippingInfo.Address.StateProvinceTerritory : _currentOrderAddress.InvoiceOrder.ProvinceNameCn;
                lblSplitCity.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City) ? shippingInfo.Address.City : _currentOrderAddress.InvoiceOrder.City;
                lblSplitDistrict.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District) ? shippingInfo.Address.CountyDistrict : _currentOrderAddress.InvoiceOrder.District;
                lblSplitStreet.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.Street) ? shippingInfo.Address.Line1 : _currentOrderAddress.InvoiceOrder.Street;
                lblSplitPostal.Text = string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode) ? shippingInfo.Address.PostalCode : _currentOrderAddress.InvoiceOrder.PostalCode;
            }
            else
            {
                divSplitEdit.Visible = true;
                divSplitView.Visible = false;
            }
        }

        protected void btnSplitConfirm_Click(object sender, EventArgs e)
        {
             
            var success = UpdateSplitOrderInvoice();

            if (success)
            {
                ScriptManager.RegisterStartupScript(this, typeof(Page), "UpdateMsg", "$(document).ready(function(){ShowMessage();});", true);
                blErrors2.Visible = false;
            }
            else
            {
                blErrors2.Visible = true;
                ScriptManager.RegisterStartupScript(this, typeof(Page), "EnableSplitSubmit", "$(document).ready(function(){EnableSpitDummy();});", true);
            }
        }

        protected bool UpdateSplitOrderInvoice()
        {
            try
            {
                ShippingInfo_V01 shippingInfo = (ShippingInfo_V01)_currentOrderAddress.Shipment;
                if (!VerifySplitOrder()) return false;
                var isdone = CreateCrmInvoice(true);
                 if (!isdone) return false;
                UpdateOrderInvoiceRequest_V01 req = new UpdateOrderInvoiceRequest_V01
                {
                    OrderHeaderID = _currentOrder.OrderHeaderID
                };
                req.Invoice = new InvoiceOrder_V01();
                req.Invoice.InvoiceType = rbSplitInvoiceType.SelectedValue;
                req.Invoice.InvoiceTitle = rbSplitInvoiceTitle.SelectedValue;

                req.Invoice.ProvinceNameCn = chkSplitAddressConfirm.Checked
                    ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.ProvinceNameCn)
                        ? shippingInfo.Address.StateProvinceTerritory
                        : _currentOrderAddress.InvoiceOrder.ProvinceNameCn
                    : ddlSplitProvince.SelectedItem.Text;

                req.Invoice.ProvinceID = chkSplitAddressConfirm.Checked
                    ? int.Parse(ddlProvince.Items.FindByText(req.Invoice.ProvinceNameCn).Value)
                    : int.Parse(ddlSplitProvince.SelectedValue);

                req.Invoice.City = chkSplitAddressConfirm.Checked
                    ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.City)
                        ? shippingInfo.Address.City
                        : _currentOrderAddress.InvoiceOrder.City
                    : ddlSplitCity.SelectedItem.Text;

                req.Invoice.District = chkSplitAddressConfirm.Checked
                    ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.District)
                        ? shippingInfo.Address.CountyDistrict
                        : _currentOrderAddress.InvoiceOrder.District
                    : ddlSplitDistrict.SelectedItem.Text;

                req.Invoice.PostalCode = chkSplitAddressConfirm.Checked
                    ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.PostalCode)
                        ? shippingInfo.Address.PostalCode
                        : _currentOrderAddress.InvoiceOrder.PostalCode
                    : txtSplitPostal.Text;

                req.Invoice.Street = chkSplitAddressConfirm.Checked
                    ? string.IsNullOrEmpty(_currentOrderAddress.InvoiceOrder.Street)
                        ? req.Invoice.PostalCode + "" + req.Invoice.ProvinceNameCn + "" + req.Invoice.City +
                          "" + req.Invoice.District
                        : _currentOrderAddress.InvoiceOrder.Street
                    : txtSplitStreet.Text;

                req.Invoice.email = txtSplitEmail.Text;
                req.Invoice.Phone = txtSplitPhone.Text;
                req.Invoice.Receiver = txtSplitReceiver.Text;

                req.Invoice.Category = "SP";
                req.Invoice.InvoiceTitleCustom = txtSplitInvoiceCustom.Text;
                req.Invoice.InvoiceContent = ddlInvoiceContent.SelectedValue;
                req.Invoice.SplitCount = int.Parse(ddlSplitInvoice.SelectedValue);

                switch (ddlSplitInvoice.SelectedValue)
                {
                    case "1":
                        req.Invoice.Amount1 = decimal.Parse(txtInvoiceAmount1.Text);
                        break;
                    case "2":
                        req.Invoice.Amount1 = decimal.Parse(txtInvoiceAmount1.Text);
                        req.Invoice.Amount2 = decimal.Parse(txtInvoiceAmount2.Text);
                        break;
                    case "3":
                        req.Invoice.Amount1 = decimal.Parse(txtInvoiceAmount1.Text);
                        req.Invoice.Amount2 = decimal.Parse(txtInvoiceAmount2.Text);
                        req.Invoice.Amount3 = decimal.Parse(txtInvoiceAmount3.Text);
                        break;

                }

                OrderProvider.UpdateOrderInvoice(DistributorID, req);
                OrderProvider.RemoveOrdersCache(DistributorID);
                return true;

            }
            catch (Exception ex)
            {
                LoggerHelper.Error("OnlineInvoice.UpdateSplitOrderInvoice() Error \n" + ex.StackTrace);
                return false;
            }
        }

        protected void ddlSplitProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSplitProvince.SelectedItem.Value))
            {
                LookupCitiesSplit(ddlSplitProvince.SelectedItem.Value);
            }
            else
            {
                ddlSplitCity.Items.Clear();
                ddlSplitDistrict.Items.Clear();
            }
        }

        protected void ddlSplitCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSplitProvince.SelectedItem.Value) && !string.IsNullOrEmpty(ddlSplitCity.SelectedItem.Value))
            {
                LookupCountiesSplit(ddlSplitProvince.SelectedItem.Value, ddlSplitCity.SelectedItem.Value);
            }
            else
            {
                ddlSplitDistrict.Items.Clear();
            }
        }

        protected void ddlSplitDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlSplitProvince.SelectedItem.Value) &&
                    !string.IsNullOrEmpty(ddlSplitCity.SelectedItem.Value) && !string.IsNullOrEmpty(ddlSplitDistrict.SelectedItem.Value))
            {
                ShippingProvider_CN shippingProvider = ShippingProvider.GetShippingProvider("CN") as ShippingProvider_CN;
                if (shippingProvider != null)
                {
                    shippingProvider.GetUnsupportedAddress(ddlSplitProvince.SelectedItem.Text.Trim(), ddlSplitCity.SelectedItem.Text.Trim(),
                                                           ddlSplitDistrict.SelectedItem.Text.Trim());
                }
            }
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
        protected void rbInvoiceType_SelectedIndexChanged(object sender, EventArgs e)
        {

            this.rbInvoiceTitle_SelectedIndexChanged(null, null);

        }

        protected void rbInvoiceTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbInvoiceTitle.SelectedValue == "CU"
                &&rbInvoiceType.SelectedValue=="CO")
            {
             txtInvoiceTitleCustom.Visible = false;
             txtInvoiceTitleCorporate.Visible = true;
            }
            else if(rbInvoiceTitle.SelectedValue == "CU"
                && rbInvoiceType.SelectedValue == "PE")
            {
                txtInvoiceTitleCustom.Visible = true;
                txtInvoiceTitleCorporate.Visible = false;
            }
            txtInvoiceTitleCustom.Text = string.Empty;
            txtInvoiceTitleCorporate.Text = string.Empty;
            //if (rbInvoiceTitle.SelectedValue == "CU" && rbInvoiceType.SelectedValue.Trim() == "CO")
            //{
            //    txtInvoiceTitleCustom.Visible = false;
            //}
        }

        protected void rbSplitInvoiceType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.rbSplitInvoiceTitle_SelectedIndexChanged(null, null);
        }

        protected void rbSplitInvoiceTitle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rbSplitInvoiceTitle.SelectedValue == "CU" )
            {
                txtSplitInvoiceCustom.Visible = true;
               
            }
            else
            {
                txtSplitInvoiceCustom.Visible = false;
               
            }
            txtSplitInvoiceCustom.Text = string.Empty;
       
            //if (rbInvoiceTitle.SelectedValue == "CU" && rbInvoiceType.SelectedValue.Trim() == "CO")
            //{

            //    txtSplitInvoiceCustom.Visible = true;
            //}
        }
    }
}
