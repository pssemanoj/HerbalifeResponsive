using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class ProductLink : UserControlBase
    {
        public string GetIconSrc(string key)
        {
            return key == null
                       ? "icon_pdf"
                       : (key.Contains("icon_")
                              ? GetLocalResourceObject(key) as string
                              : GetLocalResourceObject("icon_" + key) as string);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ProductInfo_V02 currentProduct = Session["CurrentProduct"] as ProductInfo_V02;
            if (currentProduct != null && currentProduct.DisplaySizeChart == true)
            {
                lbFactSheets.Visible = false;
            }
        }

        public void DataBind(List<Link_V01> productLink)
        {
            uxProductLinks.DataSource = productLink;
            uxProductLinks.DataBind();
        }
    }
}