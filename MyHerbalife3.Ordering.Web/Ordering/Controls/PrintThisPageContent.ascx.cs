using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class PrintThisPageContent : UserControlBase
    {
        protected bool _allowDecimal;
        protected string _isPC;

        public PrintThisPageContent()
        {
            ProductTableColumns = 6;
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase)
            {
                ProductTableColumns--;
            }
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice)
            {
                ProductTableColumns--;
            }
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayVolumePointsForEventTicket)
            {
                ProductTableColumns--;
            }
            if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn)
            {
                ProductTableColumns--;
            }
            _allowDecimal = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal;
        }

        protected int ProductTableColumns { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.DisableProductSkuOnPrintpage)
            {
                ProductSKU.Visible = false;
            }
            else
            {
                ProductSKU.Visible = true;
            }
            if (!IsPostBack)
            {
                // when click on Print button
                getURL();
            }
        }

        protected string GetPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
        }

        protected string getImage(ProductAvailabilityType prodAvail)
        {
            if (prodAvail == ProductAvailabilityType.Unavailable)
                return "/Content/Global/Products/Img/circle_red.gif";
            else if (prodAvail == ProductAvailabilityType.AllowBackOrder)
                return "/Content/Global/Products/Img/circle_orange.gif";
            else
                return "/Content/Global/Products/Img/circle_green.gif";
        }

        public void BindProduct(ProductInfo_V02 product, bool showAllInventory, Dictionary<string, SKU_V01> allSKUS)
        {
            lbProductName.Text = product.DisplayName;
            lbOverview.Text = product.Overview;
            pDetails.InnerHtml = product.Details ?? string.Empty;
            lbDetails.Visible = pDetails.InnerHtml != string.Empty;
            pBenefits.InnerHtml = product.Benefits ?? string.Empty;
            lbKeyBenefits.Visible = pBenefits.InnerHtml != string.Empty;
            pUsage.InnerHtml = product.Usage ?? string.Empty;
            lbUsage.Visible = pUsage.InnerHtml != string.Empty;
            pQuickFacts.InnerHtml = product.FastFacts ?? string.Empty;
            lbFastFacts.Visible = pQuickFacts.InnerHtml != string.Empty;
            imgProduct.Src = product.DefaultSKU.ImagePath ?? string.Empty;

            // icons
            if (product.Icons != null)
            {
                addIcons(product.Icons);
                tabIcons.Visible = product.Icons != null && product.Icons.Count > 0;
            }
            else
            {
                tabIcons.Visible = false;
            }

            // Disclaimers
            if (product.Disclaimers != null)
            {
                Disclaimer.DataSource = (from p in product.Disclaimers
                                         select p.Description);
                Disclaimer.DataBind();
                divDisclaimer.Visible = product.Disclaimers.Count > 0;
            }
            else
            {
                divDisclaimer.Visible = false;
            }
            populateProducts(product, showAllInventory, allSKUS);
        }

        //Overloaded Method to get the missing property via querystring
        public void BindProduct(ProductInfo_V02 product, bool showAllInventory, Dictionary<string, SKU_V01> allSKUS, string isPC)
        {
            _isPC = isPC;
            BindProduct(product,showAllInventory,allSKUS);
        }
        
        private void addIcons(List<Icon_V01> iconList)
        {
            if (iconList != null)
            {
                foreach (Icon_V01 icon in iconList)
                {
                    var anker = new HtmlAnchor();
                    anker.HRef = "javascript:OpenWindow(" +
                                 Server.UrlEncode("'ZoomImage.aspx?Image=" + icon.ImagePath + "',400,400,'')");
                    var img = new HtmlImage();
                    img.Src = icon.ImagePath;
                    anker.Controls.Add(img);
                    img.Attributes.Add("class", "blurb_icon");
                    divIcons.Controls.Add(anker);
                }
            }
        }

        protected void populateProducts(ProductInfo_V02 product,
                                        bool showAllInventory,
                                        Dictionary<string, SKU_V01> AllSKUS)
        {
            if (product != null)
            {
                ProductSKU.DataSource = from s in product.SKUs
                                        from a in AllSKUS.Keys
                                        where
                                            s.SKU == a &&
                                            (showAllInventory ||
                                             (showAllInventory == false &&
                                              s.ProductAvailability != ProductAvailabilityType.Unavailable))
                                        select AllSKUS[a];
                ProductSKU.DataBind();
            }
        }

        protected void DisclaimerDataBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                string item = e.Item.DataItem as string;
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

        protected void Products_OnItemCreated(object Sender, RepeaterItemEventArgs e)
        {
            switch (e.Item.ItemType)
            {
                case ListItemType.Header:
                    {
                        var th = e.Item.FindControl("thEarnBase") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase;
                        }
                        th = e.Item.FindControl("idRetailPrice") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice;
                        }
                        th = e.Item.FindControl("thDoc") as HtmlTableCell;
                        if (th != null)
                        {
                            th.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn;
                        }
                        th = e.Item.FindControl("thVolumePoint") as HtmlTableCell;
                        if (th != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .DisplayVolumePointsForEventTicket &&
                                Page as ProductsBase != null &&
                                (Page as ProductsBase).SessionInfo.IsEventTicketMode)
                            {
                                th.Visible = false;
                            }
                            else
                            {
                                th.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.HasVolumePoints;
                            }
                            //Verify if the querystring value is present to prevent 
                            //the exception in the next if fot the DistributorOrderingProfile
                            if (_isPC != null)
                            {
                                if (_isPC == "true")
                                th.Visible = false;
                                break;
                            }

                            if(DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                                th.Visible = false;
                        }
                    }
                    break;
                case ListItemType.Item:
                case ListItemType.AlternatingItem:
                    {
                        var td = e.Item.FindControl("tdEarnBase") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayEarnBase;
                        }
                        td = e.Item.FindControl("tdRetailPrice") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayRetailPrice;
                        }
                        td = e.Item.FindControl("tdDoc") as HtmlTableCell;
                        if (td != null)
                        {
                            td.Visible =
                                HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayProductDetailsColumn;
                        }
                        td = e.Item.FindControl("tdVolumePoint") as HtmlTableCell;
                        if (td != null)
                        {
                            if (
                                !HLConfigManager.Configurations.ShoppingCartConfiguration
                                                .DisplayVolumePointsForEventTicket && Page as ProductsBase != null &&
                                (Page as ProductsBase).SessionInfo.IsEventTicketMode)
                            {
                                td.Visible = false;
                            }
                            else
                            {
                                td.Visible = HLConfigManager.Configurations.ShoppingCartConfiguration.HasVolumePoints;
                            }
                            //Verify if the querystring value is present to prevent 
                            //the exception in the next if fot the DistributorOrderingProfile
                            if (_isPC != null)
                            {
                                if (_isPC == "true")
                                    td.Visible = false;
                                break;
                            }

                            if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                                td.Visible = false;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        //get the on client click url to be added to the print this page button
        //with the querystring to pass the distributorOrderingProfile.isPC, missing property
        //in the opened window to print
        public void getURL()
        {
            var text = "javascript:window.open('/Ordering/PrintProduct.aspx?IsPC=" + DistributorOrderingProfile.IsPC +
            "','PrintMe','height=200px,width=200px,scrollbars=1')";

            if (IsChina)
            {
                text = "javascript:window.open('/Ordering/PrintProduct.aspx?IsPC=" + DistributorOrderingProfile.IsPC +
            "','PrintMe','height=400px,width=700px,scrollbars=1')";
            }

            btnPrintThisPage.OnClientClick = text;
            btnPrintThisPageBottom.OnClientClick = text;
        }
    }
}