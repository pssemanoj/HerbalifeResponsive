using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class CrossSell : UserControlBase
    {
        public bool UpdateCalled { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (UpdateCalled)
            {
                upPnlCrossSell.Update();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnPnlCrossSellClose.Click += btnPnlCrossSellClose_Click;
        }

        public int CategoryID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
            //{
            //    this.Visible = false;

            //}

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled && 
                SessionInfo.DsType == ServiceProvider.DistributorSvc.Scheme.Member && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                this.Visible = false;
            }
        }

        protected void ProductDetailClicked(object sender, EventArgs e)
        {
            int CategoryID = 0, ProductID = 0;
            var lb = sender as LinkButton;
            if (lb != null)
            {
                string commandArgument = lb.CommandArgument;
                string[] commandParts = commandArgument.Split(' ');
                ProductID = int.Parse(commandParts[0]);
                CategoryID = int.Parse(commandParts[1]);

                CntrlProductDetail.LoadProduct(CategoryID, ProductID);
                upPnlCrossSell.Update();
            }
        }

        protected string getDefaultSKUImagePath(ProductInfo_V02 product)
        {
            if (product.DefaultSKU != null)
            {
                return product.DefaultSKU.ImagePath;
            }
            return string.Empty;
        }

        [SubscribesTo(MyHLEventTypes.ProductDetailBeingLaunched)]
        public void OnProductDetailBeingLaunched(object sender, EventArgs e)
        {
            var eventArgs = e as ProductDetailEventArgs;
            if (eventArgs != null)
            {
                int CategoryID = eventArgs.CategoryID, ProductID = eventArgs.ProductID;
                CntrlProductDetail.LoadProduct(CategoryID, ProductID);
            }
        }

        [SubscribesTo(MyHLEventTypes.CrossSellFound)]
        public void OnCrossSellFound(object sender, EventArgs e)
        {
            if (null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode)
            {
                TryAlsoPanel.Visible = false;
                UpdateCalled = true;
            }
            else
            {
                var eventArgs = e as CrossSellEventArgs;
                if (eventArgs != null)
                {
                    CategoryID = eventArgs.CrossSellInfo.CategoryID;
                    CrossSellProduct.DataSource = new List<ProductInfo_V02> {eventArgs.CrossSellInfo.Product};
                    CrossSellProduct.DataBind();
                    TryAlsoPanel.Visible = true;
                    UpdateCalled = true;
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.NoCrossSellFound)]
        public void OnNoCrossSellFound(object sender, EventArgs e)
        {
            TryAlsoPanel.Visible = false;
            UpdateCalled = true;
        }

        public void btnPnlCrossSellClose_Click(object sender, EventArgs e)
        {
            ModalPnlCrossSell.Hide();
            UpdateCalled = true;
        }
    }
}