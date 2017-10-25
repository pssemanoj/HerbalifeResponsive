using System;
using System.Web.UI;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class ProductDetailModalPopupExtender : ModalPopupExtenderBase
    {
    }

    public partial class ProductDetailPopup : UserControlBase
    {
        private const string ProductDetailShown = "ProductDetailShown";
        private const string ProductDetailProductID = "ProductDetailProductID";
        private const string ProductDetailCategoryID = "ProductDetailCategoryID";

        /// <summary>
        ///     Product detail closed
        /// </summary>
        [Publishes(MyHLEventTypes.ProductDetailBeingClosed)]
        public event EventHandler ProductDetailBeingClosed;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                if (ViewState[ProductDetailShown] != null)
                {
                    loadControl((int) ViewState[ProductDetailCategoryID], (int) ViewState[ProductDetailProductID]);
                }
            }
            if (!HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                ScriptManager.RegisterStartupScript(upProductDetailControl, upProductDetailControl.GetType(), "repositionProductDetailModal", "setTimeout( function () {repositionProductDetailModal($('#" + pnlProductDetailControl.ClientID + "')); }, 50);", true);
        }
        }

        public void LoadProduct(int categoryID, int productID)
        {
            ViewState[ProductDetailShown] = true;
            ViewState[ProductDetailCategoryID] = categoryID;
            ViewState[ProductDetailProductID] = productID;
            loadControl(categoryID, productID);
        }

        private void loadControl(int categoryID, int productID)
        {
            ProductDetailControlBase productDetail =
                LoadControl(HLConfigManager.Configurations.DOConfiguration.ProductDetailCntrl) as ProductDetailControl;
            productDetail.FromPopup = true;
            productDetail.PopupExtender = popup_ProductDetailControl;
            if (PanelProductDetail.Controls.Count > 1)
            {
                (Page.Master as OrderingMaster).EventBus.DeregisterObject(PanelProductDetail.Controls[1]);
                PanelProductDetail.Controls.RemoveAt(1);
            }
            PanelProductDetail.Controls.Add(productDetail);
            productDetail.LoadProduct(categoryID, productID, false);

            popup_ProductDetailControl.Show();
            upProductDetailControl.Update();
        }

        public override void HidePopup()
        {
            ProductDetailBeingClosed(this, new ProductDetailEventArgs((int)ViewState[ProductDetailCategoryID], (int)ViewState[ProductDetailProductID]));
            ViewState[ProductDetailShown] = null;
            ViewState[ProductDetailCategoryID] = ViewState[ProductDetailProductID] = null;
            popup_ProductDetailControl.Hide();
            pnlProductDetailControl.Style["display"] = "none";
        }
    }
}