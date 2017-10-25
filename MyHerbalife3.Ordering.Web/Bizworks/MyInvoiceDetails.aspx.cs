using System;
using System.Globalization;
using System.Web.Security;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.Invoices;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class MyInvoiceDetails : Invoicebase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/Invoice");
            Response.End();

            if (!HLConfigManager.Configurations.DOConfiguration.AllowToCreateInvoice)
            {
                Response.Redirect("~/Ordering/Catalog.aspx");
                Response.End();
            }
            int invoiceId = 0;
            int.TryParse(Request.QueryString["invoiceId"], out invoiceId);
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            string DistributorID = member.Value.Id;

            var invoice = InvoiceProvider.GetDetailedInvoice(DistributorID, invoiceId);
            if (invoice != null)
            {
                lblError.Visible = false;
                displayInvoice.Visible = true;
                //begin - shan - mar 09, 2012 - set invoice date insterad of created date
                //displayInvoice.InvoiceDate = (invoice.CreatedDate == null) ? "" : invoice.CreatedDate.ToString("MM/dd/yyyy");
                //set standard date format 
                //displayInvoice.InvoiceDate = (DateTime.MinValue == invoice.InvoiceDate) ? string.Empty : invoice.InvoiceDate.ToString("MM/dd/yyyy");
                displayInvoice.InvoiceDate = (DateTime.MinValue == invoice.InvoiceDate) ? string.Empty : invoice.InvoiceDate.ToString("d");
                //set invoice number formatted to be of six digits
                //displayInvoice.InvoiceNumber = invoice.DistributorInvoiceNumber.ToString();
                displayInvoice.InvoiceNumber = invoice.DistributorInvoiceNumber.ToString("000000");
                //end
                displayInvoice.FullName = invoice.FirstName + " " + invoice.LastName;
                displayInvoice.Shipto = invoice.Address1 + 
                    ((String.IsNullOrEmpty(invoice.Address2.Trim())) ? "" :
                    ("<br />" + invoice.Address2)) + "<br />" + invoice.City + ", " + invoice.State + " " + invoice.PostalCode;
                displayInvoice.Phone = invoice.PhoneNumber;
                displayInvoice.Email = invoice.Email;
                displayInvoice.InvoiceType = Resources.InvoiceTypes.ResourceManager.GetString(invoice.Type.ToString());
                displayInvoice.InvoiceStatus = Resources.InvoiceStatusTypes.ResourceManager.GetString(invoice.Status.ToString());
                displayInvoice.Notes = invoice.Notes;
                displayInvoice.SendPaymentTo = invoice.PaymentAddress;
                //shan - mar 09, 2012 - need to display total price and subtotal
                decimal totalPrice = 0;
                invoice.InvoiceSkus.ForEach(sku =>
                    {
                        totalPrice += sku.TotalPrice;
                    });
                displayInvoice.Price = totalPrice.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                displayInvoice.LoyaltyProgram = invoice.CustomerDiscount.ToString("N2",
                                                                                  CultureInfo.GetCultureInfo("en-US"));
                displayInvoice.Subtotal = (totalPrice - invoice.CustomerDiscount).ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                //begin - shan - mar 14, 2012 - set tax and shipping based on amount or percentage entered
                //current property will display value only if tax amount exists..it will display 0 if tax percentage has been entered
                //displayInvoice.Tax = invoice.TaxAmount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                //displayInvoice.Shipping = invoice.ShippingAmount.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                displayInvoice.Tax =
                    ((invoice.TaxAmount == 0) ? Math.Round((invoice.TaxPercentage / 100 * (totalPrice - invoice.CustomerDiscount)), 2) : invoice.TaxAmount).
                    ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                displayInvoice.Shipping =
                    ((invoice.ShippingAmount == 0) ? Math.Round((invoice.ShippingPercentage / 100 * (totalPrice - invoice.CustomerDiscount)), 2) : invoice.ShippingAmount).
                    ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                //end
                displayInvoice.TotalDue = invoice.TotalDue.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
                displayInvoice.BindInvoiceSkuData(invoice);
            }
            else
            {
                lblError.Visible = true;
                displayInvoice.Visible = false;
            }
            if (null != Page.Master)
            {
                (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-7 gdo-nav-mid-invoices");
            }
        }
    }
}