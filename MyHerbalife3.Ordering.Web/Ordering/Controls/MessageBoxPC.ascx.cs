using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.RulesManagement;
namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class MessageBoxPC : UserControlBase
    {
        string sessionKey = MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules.sessionKey; // "PromotionInfo";
        public bool DisplaySubmitButton { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            var currentSession = Providers.SessionInfo.GetSessionInfo(DistributorID, Locale);
            var URL = HttpContext.Current.Request.Url.AbsolutePath.ToLower() == "/ordering/checkout.aspx" || HttpContext.Current.Request.Url.AbsolutePath.ToLower() == "/ordering/confirm.aspx";

            if (URL)
            {
            if (currentSession.ReplacedPcDistributorProfileModel != null)
            {
                var nm = string.Format("{0} {1}", currentSession.ReplacedPcDistributorProfileModel.Id, currentSession.ReplacedPcDistributorProfileModel.FirstNameLocal);
                lbMessage.Text = string.Format((string)GetLocalResourceObject("MessageResource"), nm);
                }
                btnSubmit.Visible = false;
            }
            else
            {
                if (currentSession.ReplacedPcDistributorProfileModel != null)
                {
                    var nm = string.Format("{0} {1}", currentSession.ReplacedPcDistributorProfileModel.Id, currentSession.ReplacedPcDistributorProfileModel.FirstNameLocal);
                    lbMessage.Text = string.Format((string)GetLocalResourceObject("MessageResource"), nm);
                btnSubmit.Visible = true;
            }
            else
            {
                btnSubmit.Visible = false;
                
            }
        }
        }

        protected void btnSubmit_OnClick(object sender, EventArgs e)
        {
            //var currentSession = Providers.SessionInfo.GetSessionInfo(DistributorID, Locale);
            //currentSession.ReplacedPcDistributorProfileModel = null;
            //currentSession.IsReplacedPcOrder = false;
            //Providers.SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
            //Response.Redirect("~/" + HttpContext.Current.Request.Url.PathAndQuery);

        } 

        protected void btnYes_OnClick(object sender, EventArgs e)
        {
            var currentSession = Providers.SessionInfo.GetSessionInfo(DistributorID, Locale);
            currentSession.ReplacedPcDistributorProfileModel = null;
            ShoppingCart.SrPlacingForPcOriginalMemberId = null;
            currentSession.IsReplacedPcOrder = false;
            ShoppingCart.HasYearlyPromoTaken = false;
            Providers.SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
            //var info = HttpContext.Current.Session[sessionKey] as PromotionCollection;
            //if (info == null)
            //    loadPromotion();
            //var removelist = new List<string>();
            //if (info != null)
            //{
            //    removelist.AddRange(
            //        info.Select(i => i.FreeSKUList)
            //            .Select(freeList => (from c in ShoppingCart.CartItems
            //                                 from s in freeList
            //                                 where c.SKU == s.SKU
            //                                 select c.SKU).FirstOrDefault())
            //            .Where(s => !String.IsNullOrEmpty(s)));
            //}

            //if(removelist.Any())
            //    ShoppingCart.DeleteItemsFromCart(removelist, true);
           // HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartItemsAdded);
            Response.Redirect("~/" + HttpContext.Current.Request.Url.PathAndQuery);
        }
        private void loadPromotion()
        {
            
            var info = HttpContext.Current.Session[sessionKey] as PromotionCollection;
            if (null == info)
            {
                const string format = "MM-dd-yyyy";
                info = PromotionConfigurationProvider.GetPromotionCollection(HLConfigManager.Platform, "zh-CN");
                if (info != null)
                {
                    DateTime currentDateTime = DateUtils.GetCurrentLocalTime("CN");
                    PromotionCollection col = new PromotionCollection();
                    foreach (var promoItem in info)
                    {
                        DateTime startDateTime = convertDateTime(promoItem.StartDate);
                        DateTime endDateTime = convertDateTime(promoItem.EndDate);
                        if (startDateTime != DateTime.MaxValue && endDateTime != DateTime.MaxValue)
                        {
                            if (startDateTime <= currentDateTime && endDateTime > currentDateTime)
                            {
                                col.Add(promoItem);
                            }
                        }

                    }
                    HttpContext.Current.Session[sessionKey] = col;
                }
            }
        }
        private DateTime convertDateTime(string datetime)
        {
            const string format = "MM-dd-yyyy";
            if (!string.IsNullOrEmpty(datetime))
            {
                DateTime dt;
                if (DateTime.TryParseExact(datetime, format, CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out dt))
                {
                    return dt;
                }
            }
            return DateTime.MaxValue;
        }
    }
}