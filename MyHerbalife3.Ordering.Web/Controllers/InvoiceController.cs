#region

using System;
using System.Globalization;
using System.Web.Mvc;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Invoices;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ViewModel.Request;
using MyHerbalife3.Ordering.Web.Helper;
using MyHerbalife3.Shared.Infrastructure.Mvc;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.LegacyProviders.ValueObjects;
using MyHerbalife3.Shared.Analytics.Mvc;
using System.Collections.Generic;
using MyHerbalife3.Ordering.Invoices.Helper;

#endregion

namespace MyHerbalife3.Ordering.Web
{
    [AmbientNavigation]
    [CultureSwitching]
    [AmbientAnalyticsFact]
    [Authorize]
    public class InvoiceController : AsyncController
    {
        internal IAsyncLoader<InvoiceModel, GetInvoiceById> _invoiceModelLoader;
        internal IInvoiceProvider _invoiceProvider;

        public InvoiceController()
        {
            var invoiceConverter = new InvoiceConverter();
            var invoiceLoader = new InvoiceLoader(invoiceConverter);
            var invoiceProvider = new InvoiceProvider(invoiceLoader, invoiceLoader, invoiceConverter);
            _invoiceProvider = invoiceProvider;
            _invoiceModelLoader = invoiceProvider;
        }

        public ActionResult Index()
        {
            return View("Index", new InvoiceSearchModel {Action = "Invoice_Search"});
        }

        public ActionResult Create()
        {
            return View("Edit",
                new InvoiceEditModel {InvoiceId = 0, OrderId = string.Empty, CopyInvoiceId = 0, Action = "Invoice_Edit", CreatedDate = DateTime.Now});
        }

        public ActionResult Edit(int? id)
        {
            return
                View(new InvoiceEditModel
                {
                    InvoiceId = id != null ? id.Value : 0,
                    OrderId = string.Empty,
                    CopyInvoiceId = 0,
                    Action = "Invoice_Edit"
                });
        }


        public ActionResult EditByOrderId(string id)
        {
            return View("Edit",
                new InvoiceEditModel { InvoiceId = 0, OrderId = id, CopyInvoiceId = 0, Action = "Invoice_Edit", Source = "GDO" });
        }

        public ActionResult CreateByOrderId(string id)
        {
            return View("Edit",
                new InvoiceEditModel { InvoiceId = 0, OrderId = id, CopyInvoiceId = 0, Action = "Invoice_Edit", Source = "GDO"});
        }

        public ActionResult Copy(int id)
        {
            return View("Edit",
                new InvoiceEditModel
                {
                    InvoiceId = 0,
                    OrderId = string.Empty,
                    CopyInvoiceId = id,
                    Action = "Invoice_Edit"
                });
        }

        public ActionResult Display(int? id)
        {
            return View(new InvoiceDisplayModel {InvoiceId = id != null ? id.Value : 0, Action = "Invoice_Display"});
        }

        public ActionResult Delete(int id)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name;
            _invoiceProvider.Delete(id, memberId,locale);
            return View("Index", new InvoiceSearchModel {Action = "Invoice_Search"});
        }

        public ActionResult Print(int id)
        {
            var locale = CultureInfo.CurrentUICulture.Name;
            var memberId = User.Identity.Name;
            var task = _invoiceModelLoader.Load(new GetInvoiceById {Id = id, MemberId = memberId, Locale = locale});
            if (null != task && null != task.Result && task.Result.Id >0)
            {
                if (task.Result.ReceiptChannel == "ClubSaleReceipt" || task.Result.ReceiptChannel == "Club Visit/Sale")
                {
                    var Price = task.Result.ClubInvoice.ClubRecieptDisplayTotalDue.Replace(",",".");
                    var  yourPrice = Convert.ToDecimal(Price, CultureInfo.InvariantCulture);                                       
                    yourPrice.FormatPrice();
                       
                    task.Result.InvoiceLines.Add(new InvoiceLineModel()
                    {
                        ProductName = task.Result.ClubInvoice.ClubRecieptProductName,
                        Quantity = Convert.ToInt32(task.Result.ClubInvoice.ClubRecieptQuantity),
                        DisplayTotalVp = task.Result.ClubInvoice.ClubRecieptTotalVolumePoints,
                        
                    });
                    task.Result.InvoicePrice.DisplayTotalYourPrice = yourPrice.ToString("0.##");
                }

                var htmlContent = this.RenderViewAsString("Print", task.Result);

                var pdfBytes = _invoiceProvider.GeneratePdf(htmlContent);
                var contentDisposition = Request.QueryString["disposition"] == null
                    ? "inline"
                    : Request.QueryString["disposition"];

                if (null != pdfBytes && pdfBytes.Length > 0)
                {
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.Charset = "";
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Pragma", "public");
                    if(task.Result.ReceiptChannel == "ClubSaleReceipt" || task.Result.ReceiptChannel== "ProductSaleReceipt" || task.Result.ReceiptChannel == "Club Visit/Sale")
                    {
                        Response.AddHeader("content-disposition",
                           string.Format("{0}; filename ={1}", contentDisposition, "Receipt"));
                    }
                    else
                    {
                        Response.AddHeader("content-disposition",
                           string.Format("{0}; filename ={1}", contentDisposition, "Invoice"));
                    }
                   
                    Response.BinaryWrite(pdfBytes);
                    Response.Flush();
                    try
                    {
                        Response.End();
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Exception("System.Exception", ex, "Error in Print InvoiceController");
                    }
                }
                return null;
            }
            return View("Index", new InvoiceSearchModel { Action = "Invoice_Search" });
        }

        public ActionResult HtmlFragment(string path)
        {
            var result = new ContentResult {ContentType = "text/html"};

            var reader = new ContentReader {Enabled = true, Visible = true, ContentPath = path, UseLocal = true};
            reader.LoadContent();
            result.Content = reader.HtmlContent;

            return result;
        }

        public void CreateOrderRedirect()
        {
            Session["IsCopingFromInvoice"] = "Y";
        }
    }
}