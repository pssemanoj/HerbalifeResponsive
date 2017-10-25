using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Rules.Promotional.en_MY;
using MyHerbalife3.Ordering.Providers.RulesManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class ExpireDatePopUp : UserControlBase
    {
        private List<string> skuList = new List<string>();

        protected void LoadComplete(object sender, EventArgs e)
        {
            listMessage.DataSource = null;
            listMessage.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            listMessage.DataSource = null;
            listMessage.DataBind();
        }

        protected void HidePopUp(object sender, EventArgs e)
        {
            addPromoSkuPopupExtender.Hide();
        }


        protected void DeleteSKUSFromCart(object sender, EventArgs e)
        {

            if (!skuList.Any())
            {
                List<string> messagesx = new List<string>();
                messagesx = (List<string>)Session["messagesToShow"];
                var skus = messagesx.Where(c => c.Contains("-SKU:")).Distinct().ToList();
                foreach (var sku in skus)
                {
                    skuList.Add(sku.Replace("-SKU:", ""));
                }
            }

            ProductsBase.DeleteItemsFromCart(skuList);
            Session["showedMessages"] = null;
            addPromoSkuPopupExtender.Hide();
        }
        
        public void ShowPopUp()
        {

            List<string> messagesx = new List<string>();
            messagesx = (List<string>)Session["messagesToShow"];
            //validate if the skus was showed
            List<string> skusShowed = new List<string>();
            skusShowed = (List<string>)Session["showedMessages"];

            if (messagesx != null)
            {
                var messages = messagesx.Where(c => !c.Contains("-SKU:")).ToList();
                var skuMessages = messages.Where(c => !c.Contains("-SKU:")).Distinct().ToList();
                var skus = messagesx.Where(c => c.Contains("-SKU:")).Distinct().ToList();
                List<string> list = new List<string>();
                foreach (var sku in skus)
                {
                    skuList.Add(sku.Replace("-SKU:", ""));
                }
                if (skusShowed != null)
                {
                    foreach (var item in skusShowed)
                    {
                        skuMessages = skuMessages.Where(c =>!c.Contains(item)).ToList();
                    }
                    
                }
                

                if (skuMessages.Any())
                {
                    listMessage.DataSource = skuMessages;
                    listMessage.DataBind();
                    skusShowed = skuList;
                    Session["showedMessages"]= skusShowed;
                    addPromoSkuPopupExtender.Show();
                }
            }
        }
    }
}