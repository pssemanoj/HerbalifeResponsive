using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class ProductDetailFooter
    {
        public ProductInfo_V02 ProdInfo { get; set; }

        public ProductDetailFooter(ProductInfo_V02 prodInfo)
        {
            ProdInfo = prodInfo;
        }
    }

    public partial class ProductInfoFooter : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void addIcons(List<Icon_V01> iconList)
        {
            if (iconList != null)
            {
                foreach (Icon_V01 icon in iconList)
                {
                    var anker = new HtmlAnchor();
                    anker.HRef = "javascript:OpenWindow(" + Server.UrlEncode("'ZoomImage.aspx?Image=" + icon.ImagePath + "',400,400,'')");
                    var img = new HtmlImage();
                    img.Src = icon.ImagePath;
                    anker.Controls.Add(img);
                    img.Attributes.Add("class", "blurb_icon");
                    divIcons.Controls.Add(anker);
                }
            }
        }

        protected void DisclaimerDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var item = e.Item.DataItem as string;
                if (item != null)
                {
                    var p = e.Item.FindControl("pDisclaimer") as HtmlGenericControl;
                    if (p != null)
                    {
                        p.InnerHtml = item;
                    }
                }
            }
        }

        public ProductDetailFooter ProdDetails
        {
            set
            {
                if (value.ProdInfo.Disclaimers != null)
                {
                    Disclaimer.DataSource = (from p in value.ProdInfo.Disclaimers
                                             select p.Description);
                    Disclaimer.DataBind();
                    divDisclaimer.Visible = value.ProdInfo.Disclaimers.Count > 0;
                }
                else
                {
                    divDisclaimer.Visible = false;
                }

                if (value.ProdInfo.Icons != null)
                {
                    addIcons(value.ProdInfo.Icons);
                    tabIcons.Visible = value.ProdInfo.Icons != null && value.ProdInfo.Icons.Count > 0;
                }
                else
                {
                    tabIcons.Visible = false;
                }
            }
        }
    }
}