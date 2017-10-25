using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class CnChkout24h : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsChinaApp)
            {
                string msgChk24h = GetLocalResourceObject("ProductsChkOutWithin24Hours") as string;
                lblChkout24hMessage.Text = msgChk24h.Replace("\r\n", @"<br/>").Replace(" ", @"&nbsp;");

                if (IsPostBack)
                {
                    if ((AddToCartControlList != null) && (AddToCartControlList.Count > 0))
                    {
                        var postbackCause = Request.Params["__EVENTTARGET"];
                        if (!string.IsNullOrWhiteSpace(postbackCause))
                        {
                            var postbackCtrl = this.Page.FindControl(postbackCause);
                            if (postbackCtrl != null)
                            {
                                if (AddToCartControlList.Any(x => x == postbackCtrl))
                                {
                                    var deliveryTypeUiId = Page.Request.Form["uiId_BrGrOrderQuickView_DeliveryType"]; // Ordering\Controls\BrGrOrderQuickView.ascx
                                    if (!string.IsNullOrWhiteSpace(deliveryTypeUiId))
                                    {
                                        var deliveryType = Page.Request.Form[deliveryTypeUiId];
                                        bool noDeliveryInfo = ProductsBase.NoDeliveryOptionInfo();
                                        if ((deliveryType != null))
                                        {
                                            if ((deliveryType == "Shipping") ||
                                                ((deliveryType.Length >= 6) && (deliveryType.Substring(0, 6) == "Pickup") && !noDeliveryInfo)
                                                )
                                            {
                                                // Shipping - ticket 140747
                                                Chkout24hPopupExtender.Show();
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

        }

        protected bool IsChinaApp
        {
            get
            {
                return HLConfigManager.Configurations.DOConfiguration.IsChina;
            }
        }

        public List<Control> AddToCartControlList
        {
            get;
            set;
        }
    }
}