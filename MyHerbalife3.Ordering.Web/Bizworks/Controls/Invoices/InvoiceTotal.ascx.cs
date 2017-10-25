using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class InvoiceTotal : MyHerbalife3.Shared.UI.UserControlBase
    {
        #region Public Properties
        public int InvoiceStatus
        {
            get
            {
                int invoiceStatus = 0;
                int.TryParse(ddlInvoiceStatus.SelectedValue, out invoiceStatus);

                return invoiceStatus;
            }
            set
            {
                ddlInvoiceStatus.SelectedValue = value.ToString();
            }
        }

        public decimal TaxPercentage
        {
            get
            {
                decimal taxPercentage = 0;
                decimal.TryParse(txtTaxPercentage.Text.Trim(), out taxPercentage);

                return taxPercentage;
            }
            set
            {
                //shan - mar 08, 2012 - set formatted value
                //txtTaxPercentage.Text = (value == 0) ? "" : value.ToString();
                txtTaxPercentage.Text = (value > 0) ? value.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
            }
        }

        public decimal TaxAmount
        {
            get
            {
                decimal taxAmount = 0;
                decimal.TryParse(txtTaxAmount.Text.Trim(), out taxAmount);
                return taxAmount;
            }
            set
            {
                //shan - mar 08, 2012 - set formatted value
                //txtTaxAmount.Text = (value == 0) ? "" : value.ToString();
                txtTaxAmount.Text = (value > 0) ? value.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
            }
        }

        public decimal ShipAmount
        {
            get
            {
                decimal shipAmount = 0;
                decimal.TryParse(txtShipAmount.Text.Trim(), out shipAmount);
                return shipAmount;
            }
            set
            {
                //shan - mar 08, 2012 - set formatted value
                //txtShipAmount.Text = (value == 0) ? "" : value.ToString();
                txtShipAmount.Text = (value > 0) ? value.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
            }
        }

        public decimal ShipPercentage
        {
            get
            {
                decimal shipPercentage = 0;
                decimal.TryParse(txtShipPercentage.Text.Trim(), out shipPercentage);
                return shipPercentage;
            }
            set
            {
                //shan - mar 08, 2012 - set formatted value
                //txtShipPercentage.Text = (value == 0) ? "" : value.ToString();
                txtShipPercentage.Text = (value > 0) ? value.ToString("N2", CultureInfo.GetCultureInfo("en-US")) : string.Empty;
                if (!String.IsNullOrEmpty(txtShipAmount.Text) && !String.IsNullOrEmpty(txtShipPercentage.Text))
                    txtShipAmount.Text = "";
            }
        }

        public string Note
        {
            get
            {
                return txtNote.Text.Trim();
            }
            set
            {

                txtNote.Text = value;
            }
        }

        public string PaymentAddress
        {
            get
            {
                return txtPaymentAddress.Text.Trim();
            }
            set
            {
                txtPaymentAddress.Text = value;
            }
        }

        public decimal TotalDue
        {
            set
            {
                //shan - mar 08, 2012 - update the field with currency info
                //ltTotalDueAmount.Text = value.ToString();
                ltTotalDueAmount.Text = value.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                ltTotalDueAmount.Visible = true;
            }
        }

        public decimal TotalTaxAmount
        {
            set
            {
                //shan - mar 08, 2012 - update the field with currency info
                //ltTaxAmount.Text = value.ToString();
                ltTaxAmount.Text = value.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public decimal TotalShippingAmount
        {
            set
            {
                ltShipAmount.Text = value.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public string ErrorMessage
        {
            get { return this.lblErrMsg.Text.Trim(); }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnApplyTax.Click += new EventHandler(btnApplyTax_Click);
            btnApplyShip.Click += new EventHandler(btnApplyShip_Click);
            btnCalculate.Click += new EventHandler(btnCalculate_Click);
            btnClear.Click += new EventHandler(btnClear_Click);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindInvoiceStatusReference();
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            Invoice invoice = Session["Invoice"] as Invoice;
            invoice.TaxAmount = 0;
            invoice.TaxPercentage = 0;
            invoice.ShippingAmount = 0;
            invoice.ShippingPercentage = 0;
            txtTaxAmount.Text = string.Empty;
            txtTaxPercentage.Text = string.Empty;
            txtShipAmount.Text = string.Empty;
            txtShipPercentage.Text = string.Empty;
            //shan - mar 13, 2012 - clear err label and icons
            lblErrMsg.Text = string.Empty;
            imgErrTax.Visible =
                imgErrShip.Visible = false;
            //clear total tax and shipping price
            ltTaxAmount.Text =
                ltShipAmount.Text = "0.00";
            calculateTotalDue();
        }

        protected void btnCalculate_Click(object sender, EventArgs e)
        {
            CalculateTax(false);
        }

        protected void btnApplyTax_Click(object sender, EventArgs e)
        {
            //shan - mar 15, 2012 - call apply tax method after validating
            ////begin - shan - mar 06, 2012 - validate fields on applying tax
            ////var invoice = Session["Invoice"] as Invoice;
            ////ltTaxAmount.Text = ((TaxAmount == 0) ? Math.Round((TaxPercentage / 100 * invoice.TotalDue), 2) : TaxAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            ////invoice.TaxAmount = TaxAmount;
            ////invoice.TaxPercentage = TaxPercentage;
            ////calculateTotalDue();
            ////string errMsg = this.ValidateTaxFields();
            ////lblErrMsg.Text = errMsg;
            ////imgErrTax.Visible = !string.IsNullOrEmpty(errMsg);
            ////if no err msg..continue..
            //if (string.IsNullOrEmpty(errMsg))
            //{
            //    var invoice = Session["Invoice"] as Invoice;
            //    //begin - shan - mar 08, 2012 - calculate tax amount by getting the total price from the list of items
            //    //the invoice.TotalDue either will not have value or will have the updated value by adding tax, ship, etc
            //    //ltTaxAmount.Text = ((TaxAmount == 0) ? Math.Round((TaxPercentage / 100 * invoice.TotalDue), 2) : TaxAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            //    decimal totalPrice = 0;
            //    invoice.InvoiceSkus.ForEach(sku => {
            //        totalPrice += sku.TotalPrice;
            //    });
            //    ltTaxAmount.Text = ((TaxAmount == 0) ? Math.Round((TaxPercentage / 100 * totalPrice), 2) : TaxAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            //    //end
            //    invoice.TaxAmount = TaxAmount;
            //    invoice.TaxPercentage = TaxPercentage;
            //    calculateTotalDue();
            //}
            //end
            string errMsg = this.ValidateTaxFields();
            if (string.IsNullOrEmpty(errMsg))
            {
                this.ApplyTaxCharges();
            }
        }

        protected void btnApplyShip_Click(object sender, EventArgs e)
        {
            //shan - mar 15, 2012 - call apply shipping method after validating
            ////begin - shan - mar 06, 2012 - validate field on applying shipment
            ////var invoice = Session["Invoice"] as Invoice;
            ////ltShipAmount.Text = ((ShipAmount == 0) ? Math.Round((ShipPercentage / 100 * invoice.TotalDue), 2) : ShipAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            ////invoice.ShippingAmount = ShipAmount;
            ////invoice.ShippingPercentage = ShipPercentage;
            ////calculateTotalDue();
            //string errMsg = this.ValidateShippingFields();
            //lblErrMsg.Text = errMsg;
            //imgErrShip.Visible = !string.IsNullOrEmpty(errMsg);
            ////if no err msg..continue..
            //if (string.IsNullOrEmpty(errMsg))
            //{
            //    var invoice = Session["Invoice"] as Invoice;
            //    //begin - shan - mar 08, 2012 - calculate tax amount by getting the total price from the list of items
            //    //the invoice.TotalDue either will not have value or will have the updated value by adding tax, ship, etc
            //    //ltShipAmount.Text = ((ShipAmount == 0) ? Math.Round((ShipPercentage / 100 * invoice.TotalDue), 2) : ShipAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            //    decimal totalPrice = 0;
            //    invoice.InvoiceSkus.ForEach(sku =>
            //    {
            //        totalPrice += sku.TotalPrice;
            //    });
            //    ltShipAmount.Text = ((ShipAmount == 0) ? Math.Round((ShipPercentage / 100 * totalPrice), 2) : ShipAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            //    //end
            //    invoice.ShippingAmount = ShipAmount;
            //    invoice.ShippingPercentage = ShipPercentage;
            //    calculateTotalDue();
            //}
            ////end
            string errMsg = this.ValidateShippingFields();
            if (string.IsNullOrEmpty(errMsg))
            {
                this.ApplyShippingCharges();
            }
        }

        public void CalculateTax(bool isSave)
        {
            //shan - mar 09, 2012 - calculate based on the values enterd in textboxes
            //var invoice = Session["Invoice"] as Invoice;
            //invoice.TaxAmount = TaxAmount;
            //invoice.TaxPercentage = TaxPercentage;
            //invoice.ShippingAmount = ShipAmount;
            //invoice.ShippingPercentage = ShipPercentage;
            //calculateTotalDue();
            //check for invalid entries first, and then apply charges

            string errMsgTax = this.ValidateTaxFields();
            string errMsgShipping = this.ValidateShippingFields();
            //if no errors..continue applying charges
            if (string.IsNullOrEmpty(errMsgTax) &&
                string.IsNullOrEmpty(errMsgShipping))
            {
                this.ApplyTaxCharges();
                this.ApplyShippingCharges();
            }
            if (isSave)
            {
                this.calculateTotalDue();
            }
        }

        public void RenderTaxErrorMessage(bool showHideMsg)
        {
            colnTaxErrMsg.Visible = showHideMsg;
        }

        public void SetInvoiceStatus(InvoiceStatus status)
        {
            this.ddlInvoiceStatus.SelectedIndex =
                this.ddlInvoiceStatus.Items.IndexOf(this.ddlInvoiceStatus.Items.FindByText(status.ToString()));
        }

        private void ApplyTaxCharges()
        {
            //shan - mar 15, 2012 - move validating to other method,
            //to display error icons for both tax and shipping at the same time
            ////validate for invalid entries
            //string errMsg = this.ValidateTaxFields();
            //lblErrMsg.Text = errMsg;
            //imgErrTax.Visible = !string.IsNullOrEmpty(errMsg);
            ////if no err msg..continue..
            //if (string.IsNullOrEmpty(errMsg))

            decimal totalPrice = 0;
            var invoice = Session["Invoice"] as Invoice;
            //begin - shan - mar 08, 2012 - calculate tax amount by getting the total price from the list of items
            //the invoice.TotalDue either will not have value or will have the updated value by adding tax, ship, etc
            //ltTaxAmount.Text = ((TaxAmount == 0) ? Math.Round((TaxPercentage / 100 * invoice.TotalDue), 2) : TaxAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));			

            if (invoice != null)
            {
                invoice.InvoiceSkus.ForEach(sku =>
                {
                    totalPrice += sku.TotalPrice;
                });

                totalPrice = totalPrice - invoice.CustomerDiscount;

                ltTaxAmount.Text = ((TaxAmount == 0) ? Math.Round((TaxPercentage / 100 * totalPrice), 2) : TaxAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                invoice.TaxAmount = TaxAmount;
                invoice.TaxPercentage = TaxPercentage;
                calculateTotalDue();
            }
        }

        private void ApplyShippingCharges()
        {
            //shan - mar 15, 2012 - move validating to other method,
            //to display error icons for both tax and shipping at the same time
            ////validate for invalid entries
            //string errMsg = this.ValidateShippingFields();
            //lblErrMsg.Text = errMsg;
            //imgErrShip.Visible = !string.IsNullOrEmpty(errMsg);
            ////if no err msg..continue..
            //if (string.IsNullOrEmpty(errMsg))
            //{
            decimal totalPrice = 0;
            var invoice = Session["Invoice"] as Invoice;

            if (invoice != null)
            {
                invoice.InvoiceSkus.ForEach(sku =>
                {
                    totalPrice += sku.TotalPrice;
                });

                totalPrice = totalPrice - invoice.CustomerDiscount;

                ltShipAmount.Text = ((ShipAmount == 0) ? Math.Round((ShipPercentage / 100 * totalPrice), 2) : ShipAmount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                invoice.ShippingAmount = ShipAmount;
                invoice.ShippingPercentage = ShipPercentage;
                calculateTotalDue();
            }
        }

        private void calculateTotalDue()
        {
            var invoice = Session["Invoice"] as Invoice;
            decimal totalPrice = 0;

            if (null != invoice.InvoiceSkus)
            {
                foreach (var sku in invoice.InvoiceSkus)
                {
                    totalPrice = totalPrice + sku.TotalPrice;
                }

                totalPrice = totalPrice - invoice.CustomerDiscount;
                var taxPer = Math.Round((totalPrice * invoice.TaxPercentage / 100), 2);
                var shipPer = Math.Round((totalPrice * invoice.ShippingPercentage / 100), 2);

                invoice.TotalDue = totalPrice + invoice.TaxAmount + taxPer + invoice.ShippingAmount + shipPer;
                ltTotalDueAmount.Text = invoice.TotalDue.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
            }
        }

        private void BindInvoiceStatusReference()
        {
            var dictionary = new Dictionary<string, string>();
            var invoiceStatus = Enum.GetNames(typeof(InvoiceStatus)).ToList();
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("InvoiceStatusTypes");

            foreach (var entry in entries.Reverse().Where(e => invoiceStatus.Contains(e.Key)))
            {
                var statusKey = (int)Enum.Parse(typeof(InvoiceStatus), entry.Key);
                dictionary.Add(statusKey.ToString(CultureInfo.InvariantCulture), entry.Value);
            }


            ddlInvoiceStatus.DataSource = dictionary;
            ddlInvoiceStatus.DataTextField = "Value";
            ddlInvoiceStatus.DataValueField = "Key";
            ddlInvoiceStatus.DataBind();

            SetInvoiceStatus(Session["Invoice"] != null ? ((Invoice)Session["Invoice"]).Status : ServiceProvider.OrderSvc.InvoiceStatus.Unpaid);
            BindEditNotes(Session["Invoice"] != null ? ((Invoice)Session["Invoice"]).Notes : "");
            BindEditPaymentAddress(Session["Invoice"] != null ? ((Invoice)Session["Invoice"]).PaymentAddress : "");

            CalculateTax(false);
        }

        private void BindEditPaymentAddress(string paymentAddress)
        {
            PaymentAddress = paymentAddress;
        }

        private void BindEditNotes(string notes)
        {
            Note = notes;
        }

        private string ValidateTaxFields()
        {
            string errMsg = string.Empty;
            //check if both loyalty and percentage are entered
            if (!string.IsNullOrEmpty(txtTaxAmount.Text.Trim()) &&
                !string.IsNullOrEmpty(txtTaxPercentage.Text.Trim()))
            {
                errMsg = this.GetLocalResourceString("errMsg.EnterAmountOrPercentage", "Please enter a valid dollar amount OR percentage only");
            }
            //check if valid amount has been entered
            else if (!string.IsNullOrEmpty(txtTaxAmount.Text.Trim()))
            {
                decimal taxAmount = -999;
                taxAmount = decimal.TryParse(txtTaxAmount.Text.Trim(), out taxAmount) ? taxAmount : -999;
                if (taxAmount < 0)
                {
                    errMsg = this.GetLocalResourceString("errMsg.EnterValidTaxAmount", "Please enter a valid tax amount in the following format: 12.95");
                }
            }
            //check if valid percentage has been entered
            else if (!string.IsNullOrEmpty(txtTaxPercentage.Text.Trim()))
            {
                decimal taxPercentage = -999;
                taxPercentage = decimal.TryParse(txtTaxPercentage.Text.Trim(), out taxPercentage) ? taxPercentage : -999;
                if (taxPercentage < 0 ||
                    taxPercentage > 100)
                {
                    errMsg = this.GetLocalResourceString("errMsg.EnterValidPercentage", "Please enter a valid percentage between 0-100");
                }
            }
            //shan - mar 15, 2012 - based on err msg show icon
            lblErrMsg.Text = errMsg;
            imgErrTax.Visible = !string.IsNullOrEmpty(errMsg);
            //return the msg
            return errMsg;
        }

        private string ValidateShippingFields()
        {
            string errMsg = string.Empty;
            //check if both loyalty and percentage are entered
            if (!string.IsNullOrEmpty(txtShipAmount.Text.Trim()) &&
                !string.IsNullOrEmpty(txtShipPercentage.Text.Trim()))
            {
                errMsg = this.GetLocalResourceString("errMsg.EnterAmountOrPercentage", "Please enter a valid dollar amount OR percentage only");
            }
            //check if valid amount has been entered
            else if (!string.IsNullOrEmpty(txtShipAmount.Text.Trim()))
            {
                decimal shipAmount = -999;
                shipAmount = decimal.TryParse(txtShipAmount.Text.Trim(), out shipAmount) ? shipAmount : -999;
                if (shipAmount < 0)
                {
                    errMsg = this.GetLocalResourceString("errMsg.EnterValidShippingAmount", "Please enter a valid shipping amount in the following format: 12.95");
                }
            }
            //check if valid percentage has been entered
            else if (!string.IsNullOrEmpty(txtShipPercentage.Text.Trim()))
            {
                decimal shipPercentage = -999;
                shipPercentage = decimal.TryParse(txtShipPercentage.Text.Trim(), out shipPercentage) ? shipPercentage : -999;
                if (shipPercentage < 0 ||
                    shipPercentage > 100)
                {
                    errMsg = this.GetLocalResourceString("errMsg.EnterValidPercentage", "Please enter a valid percentage between 0-100");
                }
            }
            //shan - mar 15, 2012 - based on err msg show icon
            lblErrMsg.Text = errMsg;
            imgErrShip.Visible = !string.IsNullOrEmpty(errMsg);
            //return the msg
            return errMsg;
        }
    }
}