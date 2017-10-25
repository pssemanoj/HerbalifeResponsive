using System;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PrintThisPage : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                // when click on Print button
                Show();
                getURL();
            }
        }
        protected void OnCancel(object sender, EventArgs e)
        {
            pnlPrintThisPage.Style["display"] = "none";
            if (this.PopupExtender != null)
                PopupExtender.ClosePopup();
        }
        public void Show()
        {
            ProductInfo_V02 currentProduct = Session["CurrentProduct"] as ProductInfo_V02;
            if (currentProduct != null)
            {
                PrintThisPageContent ucPrintThisPage = LoadControl("~/Ordering/Controls/PrintThisPageContent.ascx") as PrintThisPageContent;
                ucPrintThisPage.BindProduct(currentProduct, SessionInfo.ShowAllInventory, this.AllSKUS);
                if (Panel1.Controls.Count > 1)
                {
                    (this.Page.Master as OrderingMaster).EventBus.DeregisterObject(Panel1.Controls[1]);
                    Panel1.Controls.RemoveAt(1);
                }
                Panel1.Controls.Add(ucPrintThisPage);
            }
            pnlPrintThisPage.Style["display"] = "inline";
            upPrintThisPage.Update();
        }
        protected void OnPrintPage(object sender, EventArgs e)
        {
            upPrintThisPage.Update();
            pnlPrintThisPage.Style["display"] = "none";
            if (this.PopupExtender != null)
                PopupExtender.ClosePopup();
        }

        //get the on client click url to be added to the print this page button
        //with the querystring to pass the distributorOrderingProfile.isPC, missing property
        //in the opened window to print
        public void getURL()
        {
            var text = "javascript:window.open('/Ordering/PrintProduct.aspx?IsPC=" + DistributorOrderingProfile.IsPC+
            "','PrintMe','height=200px,width=200px,scrollbars=1')";
            btnPrintThisPage.OnClientClick = text;
        }
    }
}