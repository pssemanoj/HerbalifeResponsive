using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class ProductDetailControlJapan : ProductDetailControl
    {
        protected override void dataBind(Category_V02 currCategory, ProductInfo_V02 currProduct)
        {
            if (currProduct != null)
            {
                if (currProduct.Links != null && currProduct.Links.Count > 0)
                {
                    ProductLinks.Visible = true;
                    ProductLinks.DataBind(currProduct.Links);
                }
                else
                {
                    ProductLinks.Visible = false;
                }

                // check hot key
                //if (!string.IsNullOrEmpty(currProduct.HotKeys))
                //{
                //    var si = new ScriptInjecter();
                //    si.Text = currProduct.HotKeys;
                //    divAddscript.Controls.Add(si);
                //}

                uxProductName.InnerHtml = currProduct.DisplayName;

                // overview
                if (!string.IsNullOrEmpty(currProduct.Overview))
                {
                    this.uxOverview.InnerHtml = currProduct.Overview;
                    lbOverview.Visible = true;
                }
                else
                {
                    lbOverview.Visible = false;
                }

                // key benefits
                if (!string.IsNullOrEmpty(currProduct.Benefits))
                {
                    pBenefits.InnerHtml = currProduct.Benefits;
                    lbKeyBenefits.Visible = true;
                }
                else
                {
                    lbKeyBenefits.Visible = false;
                }

                // usage
                if (!string.IsNullOrEmpty(currProduct.Usage))
                {
                    this.pUsage.InnerHtml = currProduct.Usage;
                    lbUsage.Visible = true;
                }
                else
                {
                    lbUsage.Visible = false;
                }

                //fast facts
                if (!string.IsNullOrEmpty(currProduct.FastFacts))
                {
                    pQuickFacts.InnerHtml = currProduct.FastFacts;
                    lbFastFacts.Visible = true;
                }
                else
                {
                    lbFastFacts.Visible = false;
                }


                ProdImage.ImageName = getDefaultSKUImagePath(currProduct);
                //ProdImage.Enlarged = false;

                ProductInfoFooter1.ProdDetails = new ProductDetailFooter(currProduct);
            }
        }

    }
}