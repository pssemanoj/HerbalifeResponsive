using System;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Shared.Interfaces;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using OrderProvider = MyHerbalife3.Ordering.Providers.China.OrderProvider;
using System.Web;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using System.Collections.Generic;
using HL.Common.Utilities;
using System.Globalization;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class CreateOrderForPC : ProductsBase
    {
        private static string sessionKey = MyHerbalife3.Ordering.Rules.Promotional.zh_CN.PromotionalRules.sessionKey;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                var data = OrderProvider.GetPCCustomerIdByReferralId(DistributorID);
                if (data != null && data.Any())
                {
                    foreach (var info in data)
                    {
                        ddlMemberID.Items.Add(new ListItem(info.CustomerId + " " + info.NameCn, info.CustomerId));
                    }
                    var strTextSelect = (string) GetLocalResourceObject("TextSelect");
                    ddlMemberID.Items.Insert(0, new ListItem(strTextSelect, ""));
                }
            }

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid gdo-nav-mid-copc");

        }

        protected void btnSubmit_OnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlMemberID.SelectedValue))
            {
                //Need to display user friendly mesage, will take up in the next iteration
                return;
            }
            ILoader<DistributorProfileModel, GetDistributorProfileById> distributorProfileLoader = new DistributorProfileLoader();
            var distributorProfileModel = distributorProfileLoader.Load(new GetDistributorProfileById { Id = ddlMemberID.SelectedValue });
            var pcDistInfo = DistributorOrderingProfileProvider.GetProfile(ddlMemberID.SelectedValue, CountryCode);
            var currentSession = Providers.SessionInfo.GetSessionInfo(DistributorID, Locale);
            currentSession.ReplacedPcDistributorProfileModel = distributorProfileModel;
            currentSession.ReplacedPcDistributorOrderingProfile = pcDistInfo;
            currentSession.IsReplacedPcOrder = true;
            base.ShoppingCart.SrPlacingForPcOriginalMemberId = currentSession.ReplacedPcDistributorOrderingProfile.Id;
            Providers.SessionInfo.SetSessionInfo(DistributorID,Locale, currentSession);
            var allSKU = CatalogProvider.GetAllSKU(Locale);
            var SKUsToRemove = new List<string>();
            foreach (var cartitem in ShoppingCart.CartItems)
            {
                SKU_V01 PrmoSku;
                
                allSKU.TryGetValue(cartitem.SKU, out PrmoSku);
                if (PrmoSku != null)
                {
                    if (!PrmoSku.IsPurchasable)
                    {
                        SKUsToRemove.Add(PrmoSku.SKU.Trim());
                    }
                }
               
            }
            Array.ForEach(SKUsToRemove.ToArray(),
                        a => ShoppingCart.CartItems.Remove(ShoppingCart.CartItems.Find(x => x.SKU == a)));
            Array.ForEach(SKUsToRemove.ToArray(),
                       a => ShoppingCart.ShoppingCartItems.Remove(ShoppingCart.ShoppingCartItems.Find(x => x.SKU == a)));

            Response.Redirect("PriceList.aspx?ETO=False");
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