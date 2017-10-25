using System;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class PrintPageModalPopupExtender : ModalPopupExtenderBase
    {
    }

    public partial class ProductImage : UserControlBase
    {
        public bool FromPopup { get; set; }

        public string ImageName
        {
            set
            {
                this.ProdImage.Src = value;
                this.ProdImageLarge.Src = value;
                this.ProdImgPopup.Src = value;
                LinkEnlargeImage.NavigateUrl = "javascript:OpenWindow('"+
                Server.UrlEncode("ZoomImage.aspx?Image=" + value + "',420,420,'')");
            }
        }

        const string PrintThisPageControlName = "~/Ordering/Controls/PrintThisPage.ascx";
        const string PrintPageShown = "PrintPageShown";
        protected void Page_Load(object sender, EventArgs e)
        {
            ProductInfo_V02 currentProduct = Session["CurrentProduct"] as ProductInfo_V02;

            if (!IsPostBack)
            {
                if (currentProduct != null)
                {
                    if (currentProduct.DisplaySizeChart == true)
                    {
                        ProdImage.Attributes.Add("class", string.Format("{0} apparel-image", ProdImage.Attributes["class"]));
                        printThisPageContainer.Visible = false;
                    }

                    divSizeChartLnk.Visible = currentProduct.DisplaySizeChart;
                }
            }

            if (currentProduct != null)
            {
                PrintThisPageContent ucPrintThisPage = LoadControl("~/Ordering/Controls/PrintThisPageContent.ascx") as PrintThisPageContent;
                ucPrintThisPage.BindProduct(currentProduct, SessionInfo.ShowAllInventory, this.AllSKUS);
                printThisPageContainer.Controls.Add(ucPrintThisPage);
            }

        }
        PrintThisPage ucPrintThisPage;
        protected void OnClickPrintThisPage(object sender, EventArgs e)
        {
            createControl();
            popup_PrintThisPage.Show();
            ViewState[PrintPageShown] = true;

        }

        private void createControl()
        {
            ucPrintThisPage = LoadControl(PrintThisPageControlName) as PrintThisPage;
            ucPrintThisPage.PopupExtender = popup_PrintThisPage;
            divPrintThisPage.Controls.Add(ucPrintThisPage);
           // ucPrintThisPage.Show();
        }
        public override void HidePopup()
        {
            popup_PrintThisPage.Hide();
            ViewState[PrintPageShown] = null;
            // workaround for problem after close print popup, image button doesn't work.
            Response.Redirect(Request.RawUrl);
        }

        public void EnlargeImage(bool fromPopup)
        {
            imageSection.Visible = !fromPopup;
            imageSectionFromPopup.Visible = fromPopup;
            HdFromPopup.Value = fromPopup.ToString();
        }

    }
}
