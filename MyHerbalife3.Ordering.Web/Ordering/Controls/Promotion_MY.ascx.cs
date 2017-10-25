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
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class Promotion_MY : UserControlBase
    {
        private SessionInfo currentSession = null;
        private const string RequiredSKU = "K301";
        private const string PromoSKU = "K461";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (currentSession == null)
                currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
        }

        protected void HidePromoMsg(object sender, EventArgs e)
        {
            currentSession.ShoppingCart.PromoQtyToAdd = 0;
            addPromoSkuPopupExtender.Hide();
        }

        protected void ApplyPromo(object sender, EventArgs e)
        {
            var cart = currentSession.ShoppingCart as ShoppingCart_V02;

            var result = (new PromotionalRules()).ProcessPromoInCart(cart, null, ShoppingCartRuleReason.CartItemsBeingAdded);

            if (result != null && result.Count > 0 && result[0].Result == RulesResult.Success)
            {
                // Reload PopUp ProductDetail
                if (NamingContainer is ProductDetailControl && (NamingContainer as ProductDetailControl).FromPopup)
                    (NamingContainer as ProductDetailControl).ReloadProduct();
                else
                    Response.Redirect(Request.RawUrl);
            }

            HidePromoMsg(sender, e);
        }

        public void ShowPromo()
        {
            addPromoSkuPopupExtender.Show();
        }
    }
}