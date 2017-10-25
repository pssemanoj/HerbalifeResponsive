using System;
using System.Globalization;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.Invoices;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class DisplayInvoice : MyHerbalife3.Shared.UI.UserControlBase
    {
        #region Properties
        public string InvoiceDate
        {
            get
            {
                return ltInvoiceDateVal.Text.Trim();
            }
            set
            {
                ltInvoiceDateVal.Text = value;
            }
        }

        public string InvoiceNumber
        {
            get
            {
                return ltInvoiceNumberVal.Text.Trim();
            }
            set
            {
                ltInvoiceNumberVal.Text = value;
            }
        }

        public string FullName
        {
            get
            {
                return ltNameVal.Text.Trim();
            }
            set
            {
                ltNameVal.Text = value;
            }
        }

        public string Shipto
        {
            get
            {
                return ltShiptoVal.Text.Trim();
            }
            set
            {
                ltShiptoVal.Text = value;
            }
        }

        public string Phone
        {
            get
            {
                return ltPhoneVal.Text.Trim();
            }
            set
            {
                ltPhoneVal.Text = value;
            }
        }

        public string Email
        {
            get
            {
                return ltEmailVal.Text.Trim();
            }
            set
            {
                ltEmailVal.Text = value;
            }
        }

        public string InvoiceType
        {
            get
            {
                return ltInvoiceTypeVal.Text.Trim();
            }
            set
            {
                ltInvoiceTypeVal.Text = value;
            }

        }

        public string InvoiceStatus
        {
            get
            {
                return ltInvoiceStatusVal.Text.Trim();
            }
            set
            {
                ltInvoiceStatusVal.Text = value;
            }
        }

        public string ItemsTotalVolumes
        {
            get
            {
                return lblItemTotalVolume.Text.Trim();
            }
            set
            {
                lblItemTotalVolume.Text = value;
            }
        }

        public string ItemsSubTotal
        {
            get
            {
                return lblItemSubTotal.Text.Trim();
            }
            set
            {
                lblItemSubTotal.Text = value;
            }
        }

        public string Notes
        {
            get
            {
                return ltNotesVal.Text.Trim();
            }
            set
            {
                ltNotesVal.Text = value;
            }
        }

        public string SendPaymentTo
        {
            get
            {
                return ltSendPaymentToVal.Text.Trim();
            }
            set
            {
                ltSendPaymentToVal.Text = value;
            }
        }

        public string Price
        {
            get
            {
                return lblPriceVal.Text.Trim();
            }
            set
            {
                lblPriceVal.Text = value;
            }
        }

        public string LoyaltyProgram
        {
            get
            {
                return lblLoyaltyProgramVal.Text.Trim();
            }
            set
            {
                lblLoyaltyProgramVal.Text = value;
            }
        }

        public string Subtotal
        {
            get
            {
                return lblSubtotalVal.Text.Trim();
            }
            set
            {
                lblSubtotalVal.Text = value;
            }
        }

        public string Tax
        {
            get
            {
                return lblTaxVal.Text.Trim();
            }
            set
            {
                lblTaxVal.Text = value;
            }
        }

        public string Shipping
        {
            get
            {
                return lblShippingVal.Text.Trim();
            }
            set
            {
                lblShippingVal.Text = value;
            }
        }

        public string TotalDue
        {
            get
            {
                return ltlTotalDueVal.Text.Trim();

            }
            set
            {
                ltlTotalDueVal.Text = value;
            }
        }

        public string PrintUrl { get; set; }

        #endregion

        public void BindInvoiceSkuData(Invoice invoice)
        {
            try
            {
                if (invoice.CustomerDiscount >0)
                {
                    trCustomerLoyalty.Visible = true;
                }
                skuItems.DataSource = invoice.InvoiceSkus;
                skuItems.DataBind();
                decimal totalItemsVolume = 0;
                decimal totalItemsPrice = 0;
                foreach (var item in invoice.InvoiceSkus)
                {
                    totalItemsVolume = totalItemsVolume + item.TotalVolumePoints;
                    totalItemsPrice = totalItemsPrice + item.TotalPrice;
                }

                lblItemTotalVolume.Text = totalItemsVolume.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                lblItemSubTotal.Text = totalItemsPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                Session["Invoice"] = invoice;

                //Prefix customer loyalty program with the percentage
                lblLoyaltyProgram.Text = (invoice != null && 0 < invoice.DiscountPercentage) ?
                   invoice.DiscountPercentage.ToString("N2", CultureInfo.GetCultureInfo("en-US")) + "%" +
                   " " + GetLocalResourceString("lblLoyaltyProgram.Text", "Customer Loyalty Program:") :
                   GetLocalResourceString("lblLoyaltyProgram.Text", "Customer Loyalty Program:");
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            lnkBackToInvoiceList.Click += new EventHandler(lnkBackToInvoiceList_Click);
        }

        void lnkBackToInvoiceList_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Bizworks/MyInvoices.aspx");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           
            PrintUrl = string.Format("~/Bizworks/MyInvoiceDetailsPrint.aspx?invoiceId={0}", InvoiceNumber);
            if(lnkPrintInvoice != null)
            {
                lnkPrintInvoice.PostBackUrl = PrintUrl;
            }

            if (lnkSavePDF != null)
            {
                lnkSavePDF.PostBackUrl = PrintUrl;
            }

            if (lnkEmailInvoice != null)
            {
                lnkEmailInvoice.PostBackUrl = PrintUrl;
            }

            if (lnkCreateOrder != null)
            {
                // Link to create order.
                lnkCreateOrder.Visible = HLConfigManager.Configurations.DOConfiguration.AllowCreateOrderFromInvoice;
            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        public void OnSavePdfClick(object sender, EventArgs e)
        {
            try
            {
                var invoicePdfName = "CustomerInvoice.pdf";

                if (Session["Invoice"] != null)
                {
                    var invoice = Session["Invoice"] as Invoice;

                    if(invoice != null)
                    {
                        invoicePdfName = String.Format("Invoice{0:000000}{1}.pdf", invoice.DistributorInvoiceNumber, invoice.LastName);
                    }
                }

                var htmlContent = HhtmlContent.Value;
                var pdfBytes = InvoiceProvider.GenerateInvoicePdf(htmlContent, "tJ+GlIaGlIWFlIKahJSHhZqFhpqNjY2N");
                var contentDisposition = Request.QueryString["disposition"] == null ? "inline" : Request.QueryString["disposition"].ToString();

                if (null != pdfBytes && pdfBytes.Length > 0)
                {
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.Charset = "";
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Pragma", "public");
                    Response.AddHeader("content-disposition", string.Format("{0}; filename ={1}", contentDisposition, invoicePdfName));
                    Response.BinaryWrite(pdfBytes);
                    Response.Flush();
                    try { Response.End(); }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
        }

        protected void OnEditInvoice(object sender, EventArgs e)
        {
            Session["ActionMode"] = "Edit";
            Response.Redirect("~/Bizworks/CreateInvoice.aspx");
        }

        protected void OnCopyInvoice(object sender, EventArgs e)
        {
            Session["ActionMode"] = "Copy";
            Response.Redirect("~/Bizworks/CreateInvoice.aspx");
        }

        protected void OnCreateOrder(object sender, EventArgs e)
        {
            try
            {
                if (Session["Invoice"] != null)
                {
                    var invoice = Session["Invoice"] as Invoice;
                    if (invoice != null)
                    {
                        Session["IsCopingFromInvoice"] = "Y";
                        Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?invoiceId={0}&invoiceNum={1}",
                            invoice.ID, invoice.DistributorInvoiceNumber));
                    }
                }
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
        }        

        protected void OnDeleteInvoiceClick(object sender, EventArgs e)
        {
            try
            {
                if (Session["Invoice"] != null)
                {
                    var invoice = Session["Invoice"] as Invoice;
                    var checkDelete = InvoiceProvider.DeleteInvoice(invoice.DistributorID, invoice.ID, invoice.ContactInfoID);
                    if (checkDelete)
                    {
                        Response.Redirect("~/Bizworks/MyInvoices.aspx");
                    }
                }
            }
            catch (Exception ex)
            {
                MyHLWebUtil.LogExceptionWithContext(ex);
            }
        }
    }
}