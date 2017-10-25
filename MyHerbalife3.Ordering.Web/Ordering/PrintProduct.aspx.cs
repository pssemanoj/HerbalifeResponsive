using System;
using System.Globalization;
using System.IO;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class PrintProduct : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            PrintWebControl();
        }

        public void PrintWebControl()
        {
            var ispc = Request.QueryString["IsPC"];
            var stringWrite = new StringWriter();
            var htmlWrite = new HtmlTextWriter(stringWrite);

            var pg = new Page();
            pg.EnableEventValidation = false;
            var frm = new HtmlForm();
            pg.Controls.Add(frm);
            frm.Attributes.Add("runat", "server");

            var currentProduct = Session["CurrentProduct"] as ProductInfo_V02;
            if (currentProduct != null)
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var user = member.Value;
                string locale = CultureInfo.CurrentCulture.Name;
                SessionInfo sessionInfo = SessionInfo.GetSessionInfo(user.Id, locale);


                MyHLShoppingCart shoppingCart = ShoppingCartProvider.GetShoppingCart(user.Id, locale);
                ProductInfoCatalog_V01 ProductInfoCatalog = CatalogProvider.GetProductInfoCatalog(locale, 
                                                                                                  shoppingCart
                                                                                                      .DeliveryInfo !=
                                                                                                  null
                                                                                                      ? shoppingCart
                                                                                                            .DeliveryInfo
                                                                                                            .WarehouseCode
                                                                                                      : HLConfigManager
                                                                                                            .Configurations
                                                                                                            .ShoppingCartConfiguration
                                                                                                            .DefaultWarehouse);

                var ucPrintThisPage =
                    LoadControl("~/Ordering/Controls/PrintThisPageContent.ascx") as PrintThisPageContent;
                ucPrintThisPage.BindProduct(currentProduct, sessionInfo.ShowAllInventory, ProductInfoCatalog.AllSKUs,ispc);
                frm.Controls.Add(ucPrintThisPage);
            }

            pg.DesignerInitialize();
            pg.RenderControl(htmlWrite);

            ClientScript.RegisterStartupScript(GetType(), "PrintThisProduct", stringWrite.ToString());
            ClientScript.RegisterStartupScript(GetType(), "PrintPage", "<script>window.print();</script>");
            ClientScript.RegisterStartupScript(GetType(), "CloseWindow",
                                               "<script>setTimeout('window.close()', 2000);</script>");
        }
    }
}