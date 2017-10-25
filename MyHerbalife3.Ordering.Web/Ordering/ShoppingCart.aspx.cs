#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using HL.Common.EventHandling;
using MyHerbalife3.Core.DistributorProvider.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.CrossSell;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.Providers.Shipping;


#endregion

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class ShoppingCart : ProductsBase
    {
        private CheckOutOptions _checkoutOptions;
        private CheckoutOrderSummary _checkoutOrderSummary;

        [Publishes(MyHLEventTypes.CrossSellFound)]
        public event EventHandler CrossSellFound;

        [Publishes(MyHLEventTypes.NoCrossSellFound)]
        public event EventHandler NoCrossSellFound;

        //CheckoutTotalsMini _checkoutTotalsMini = null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var currentSession = SessionInfo;
            if (currentSession.DeliveryInfo != null && !string.IsNullOrEmpty(currentSession.CustomerOrderNumber))
            {
                try
                {
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Instruction))
                        ShoppingCart.DeliveryInfo.Instruction = currentSession.DeliveryInfo.Instruction;
                    if (!ShoppingCart.DeliveryInfo.PickupDate.HasValue)
                        ShoppingCart.DeliveryInfo.PickupDate = currentSession.DeliveryInfo.PickupDate;
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))
                        ShoppingCart.DeliveryInfo.Address.Phone = currentSession.DeliveryInfo.Address.Phone;
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                        ShoppingCart.DeliveryInfo.Address.Recipient = currentSession.DeliveryInfo.Address.Recipient;
                }
                catch
                {
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina && !SessionInfo.IsReplacedPcOrder)
            {

                var SurveyEligibility = Providers.China.OrderProvider.GetCustomerSurvey(DistributorID);
                if (SurveyEligibility != null)
                {
                    if (Session["CustomerSurveyCancelled"] == null ||
                        !Convert.ToBoolean(Session["CustomerSurveyCancelled"]))
                    {
                        Response.Redirect("Survey.aspx?@ctrl=CustomerSurvey");
                    }

                }
                else if (SessionInfo.surveyDetails != null &&ShoppingCart.CartItems.Any() &&
                         ShoppingCart.CartItems.Find(
                             x => x.SKU == SessionInfo.surveyDetails.SurveySKU.Trim()) == null &&
                         !SessionInfo.surveyDetails.SurveyCompleted)
                {

                    Providers.China.OrderProvider.AddFreeGift(
                        SessionInfo.surveyDetails.SurveySKU.Trim(),
                        SessionInfo.surveyDetails.SurveySKUQuantity,
                        ProductInfoCatalog.AllSKUs, ShoppingCart.DeliveryInfo.WarehouseCode,ShoppingCart);
                }
            }

            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderProducts").ToString());

            if (ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageHeaderEvents").ToString());
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl))
            {
                var _checkoutOptionsControl =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOptionsControl);

                _checkoutOptions = _checkoutOptionsControl as CheckOutOptions;
                _checkoutOptions.IsStatic = (Page as ProductsBase).CheckoutOptionsAreStatic;
                plCartOptions.Controls.Add(_checkoutOptions);
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsMiniControl))
            {
                var _checkoutTotalsMini =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsMiniControl);
                plCheckOutTotalsMini.Controls.Add(_checkoutTotalsMini);
            }

            if(!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl) && 
                HLConfigManager.Configurations.DOConfiguration.AllowHAP && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                var _checkoutHAPOptions =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckOutHAPOptionsControl);
                plCheckOutHAPOptions.Controls.Add(_checkoutHAPOptions);

                if (ShoppingCart.DsType == null)
                {
                    var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(DistributorID, CountryCode);
                    ShoppingCart.DsType = DistributorType;
                }
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary))
            {
                _checkoutOrderSummary =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutOrderSummary) as
                        CheckoutOrderSummary;
                _checkoutOrderSummary.DisplayReadOnlyGrid = false;
                _checkoutOrderSummary.OmnitureState = "scView";
                plCheckOutOrderDetails.Controls.Add(_checkoutOrderSummary);
            }

            if (!IsPostBack)
            {
                // display cart name
                if (HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts)
                {
                    if (ShoppingCart != null && ShoppingCart.IsSavedCart
                        && !ShoppingCart.IsFromCopy && !string.IsNullOrEmpty(ShoppingCart.CartName))
                    {
                        SavedCartTitle.Visible = true;
                        lblSavedCartName.Text = string.Format(GetLocalResourceObject("lblSavedCartName").ToString(),
                            ShoppingCart.CartName);
                    }
                }
                if (ShoppingCart != null && ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0)
                {
                    if (ShoppingCart.DeliveryInfo != null)
                    {
                        var skuList = new List<SKU_V01>();
                        skuList.AddRange(from s in ShoppingCart.CartItems
                                         from k in AllSKUS
                                         where s.SKU == s.SKU
                                         select k.Value);
                        CatalogProvider.GetProductAvailability(skuList, Locale, DistributorID,
                            ShoppingCart.DeliveryInfo.WarehouseCode);
                    }
                    findCrossSell(ShoppingCart.CartItems);
                    if (ShoppingCart.CustomerOrderDetail != null)
                    {
                        divcustomerOrderStaticMessage.Visible = true;
                        lblCustomerOrderStaticMessage.Text =
                            GetLocalResourceObject("CustomerOrderStaticMessage").ToString();
                    }
                }

                else
                {
                    NoCrossSellFound(this, null);
                }

                // Process copy invoice
                var invoiceId = Request.QueryString["invoiceId"];

                if (!string.IsNullOrEmpty(invoiceId) || IsFromMemberInvoice())
                {
                    var isCopingFromInvoice = Session["IsCopingFromInvoice"] as string;
                    if (!string.IsNullOrEmpty(isCopingFromInvoice) && isCopingFromInvoice.Equals("Y"))
                    {
                        // Save the active cart if needed
                        if (ShoppingCart.CartItems.Count != 0 && !ShoppingCart.IsSavedCart)
                        {
                            txtSaveCartName.Text = SaveCartCommand.SuggestCartName(ShoppingCart.DeliveryInfo,
                                string.Empty, DistributorID, Locale);
                            mdlClearCart.Show();
                        }
                        else
                        {
                            CopyInvoice();
                        }
                    }
                }

                 if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                 {
                     HLRulesManager.Manager.ProcessCart(ShoppingCart, ShoppingCartRuleReason.CartItemsAdded);
                 }
                var allowedCountries = HL.Common.Configuration.Settings.GetRequiredAppSetting("AllowAPFPopupForStandAloneContries", "CH");
                if (Session["showedAPFPopup"] == null) Session["showedAPFPopup"] = false;
                if (!(bool)Session["showedAPFPopup"] && allowedCountries.Contains(Locale.Substring(3)) && (APFDueProvider.IsAPFDueWithinOneYear(DistributorID, Locale.Substring(3)) || APFDueProvider.IsAPFDueAndNotPaid(DistributorID, Locale)))
                {
                    APFDuermndrPopUp.ShowPopUp();
                }
                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                {
                    List<DeliveryOption> shippingAddresses =
                        (Page as ProductsBase).GetShippingProvider()
                                              .GetShippingAddresses((Page as ProductsBase).DistributorID,
                                                                    (Page as ProductsBase).Locale)
                                              .Where(s => s.HasAddressRestriction == true)
                                              .ToList();
                    if (shippingAddresses.Count == 0)
                    {
                        AddressResPopUP1.ShowAddressRestrictionPopUp();
                    }
                }

            }

            if (HLConfigManager.Configurations.DOConfiguration.IsChina && DistributorOrderingProfile.IsTermConditionAlert)
            {
               // TermConditionPopupExtender.Show();
            }

            (Master as OrderingMaster).SetDivSpacerVisibility(false);
            (Master as OrderingMaster).SetHeaderRowVisibility(false);
            //(this.Master as OrderingMaster).SetRightPanelStyle("margin", "13px 8px");
            if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
            {
                (Master as OrderingMaster).divLeftVisibility = true;
            }
            else
            {
                (Master as OrderingMaster).divLeftVisibility = false;
            }
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-xs-12 col-sm-9 gdo-nav-mid-sp");

            var strScript1 = @"$(document).ready(function()
                                { 
                                    scrollTo(0, 0);
                                });";

            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", strScript1, true);
            DisplayELearningMessage();

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && !string.IsNullOrEmpty(ShoppingCart.HAPAction) && ShoppingCart.HAPAction == "UPDATE")
            {
                divHapEditMessage.Visible = true;
            }
            if (HLConfigManager.Configurations.DOConfiguration.CheckSKUExpirationDate)
            {
                ExpireDatePopUp1.ShowPopUp();
            }
        }

        public void findCrossSell(List<ShoppingCartItem_V01> products)
        {
            var crossSellList = new List<CrossSellInfo>();
            foreach (var cartItem in products)
            {
                var item = ShoppingCart.ShoppingCartItems.Find(s => s.SKU == cartItem.SKU);
                if (item != null)
                {
                    if (item.ProdInfo == null || item.ParentCat == null || item.ProdInfo.CrossSellProducts == null)
                        continue;

                    foreach (var cat in item.ProdInfo.CrossSellProducts.Keys)
                    {
                        crossSellList.AddRange(from a in item.ProdInfo.CrossSellProducts[cat]
                                               select new CrossSellInfo(cat, a));
                    }
                }
            }
            if (crossSellList.Count > 0)
            {
                CrossSellInfo candidate = null;
                foreach (var c in crossSellList)
                {
                    if (ProductDetail.ShouldSelectThisCrossSell(c.Product, ShoppingCart,
                        Session[ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY]
                            as CrossSellInfo,
                        Session[
                            ProductDetail.
                                LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] as
                            CrossSellInfo))
                    {
                        if (!ProductDetail.IfOutOfStock(c.Product, AllSKUS))
                        {
                            candidate = c;
                            break;
                        }
                    }
                }

                // if nothing found
                if (candidate == null)
                {
                    candidate = ProductDetail.SelectCrossSellFromPreviousDisplayOrCrssSellList(ShoppingCart,
                        crossSellList,
                        Session[
                            ProductDetail.
                                LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY
                            ] as
                            CrossSellInfo, AllSKUS);

                    if (candidate != null && ProductDetail.IfOutOfStock(candidate.Product, AllSKUS))
                    {
                        candidate = null;
                    }
                }

                if (candidate != null)
                {
                    Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] = candidate;

                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(candidate));
                    return;
                }
            }
            //findCrossSell();
            NoCrossSellFound(this, null);
        }

        private void findCrossSell()
        {
            var cs = Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] as CrossSellInfo;
            if (cs != null)
            {
                var category = CatalogHelper.FindCategory(ProductInfoCatalog, cs.CategoryID);
                if (category != null)
                {
                    // fire event
                    CrossSellFound(this, new CrossSellEventArgs(cs));
                    return;
                }
            }
            NoCrossSellFound(this, null);
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            if (ShoppingCart != null && ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0)
            {
                findCrossSell(ShoppingCart.CartItems);
            }
            else
            {
                NoCrossSellFound(this, null);
            }
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartDeleted)]
        public void OnShoppingCartDeleted(object sender, EventArgs e)
        {
            try
            {
                if (ShoppingCart != null)
                {
                    NoCrossSellFound(null, null);
                }
            }
            catch
            {
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            if (_checkoutOrderSummary != null)
            {
                _checkoutOrderSummary.ShoppingCartItemsDataBind();
            }
            if (ShoppingCart != null && ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0 &&
                ShoppingCart.DeliveryInfo != null)
                findCrossSell(ShoppingCart.CartItems);
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressPopupCancelled)]
        public void OnShippingAddressPopupCancelled(object sender, EventArgs e)
        {
            var isCopingFromInvoice = Session["IsCopingFromInvoice"] as string;
            if (!string.IsNullOrEmpty(isCopingFromInvoice) && isCopingFromInvoice.Equals("Y"))
            {
                OnCancelCopyInvoice(this, null);
            }
        }

        /// <summary>
        ///     Verifies if the cart can be saved
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        protected void OnSaveCart(object sender, EventArgs e)
        {
            var cartName = txtSaveCartName.Text;
            if (!string.IsNullOrEmpty(cartName))
            {
                var shoppingCart = (Page as ProductsBase).ShoppingCart;
                if (!ShoppingCartProvider.CartExists(DistributorID, Locale, cartName))
                {
                    SaveCart(cartName, false);
                }
                else
                {
                    // Validate if the active cart is the same cart to save
                    if (ShoppingCart.IsSavedCart && ShoppingCart.CartName.ToUpper().Equals(cartName.ToUpper()))
                    {
                        SaveCart(cartName, true);
                    }
                    else
                    {
                        ShowInvalidCartName();
                    }
                }
            }
            else
            {
                ShowInvalidCartName();
            }
        }

        protected void OnCopyWOSave(object sender, EventArgs e)
        {
            if (ShoppingCart.CustomerOrderDetail == null)
            {
                ShoppingCart.ClearCart();
            }
            mdlClearCart.Hide();
            CopyInvoice();
        }


        private int GetMemberInvoiceId()
        {
            if (!IsFromMemberInvoice()) return 0;
            int memberInvoiceId;
            return int.TryParse(Request.QueryString["memberInvoiceId"],
                out memberInvoiceId)
                ? memberInvoiceId
                : 0;
        }

        private bool IsFromMemberInvoice()
        {
            return !string.IsNullOrEmpty(Request.QueryString["memberInvoiceId"]);
        }

        private void CopyInvoice()
        {
            if (IsFromMemberInvoice())
            {
                CopyMemberInvoice();
            }
            else
            {
                 long invoiceId = 0;

                 if (long.TryParse(Request.QueryString["invoiceId"], out invoiceId))
                 {
                     var copyOfInvoice = CopyOrderProvider.CopyShoppingCart(DistributorID, Locale, invoiceId);
                     if (copyOfInvoice != null)
                     {
                         SessionInfo.ConfirmedAddress = false;
                         ShoppingCart = copyOfInvoice;
                         if (ShoppingCart.IsFromInvoice
                             && ShoppingCart.CopyInvoiceStatus == CopyInvoiceStatus.AddressValidationFail
                             && ShoppingCart.CopyInvoiceAddress != null)
                         {
                             _checkoutOptions.ValidateInvoiceAddress();
                         }
                         else
                         {
                             Session["IsCopingFromInvoice"] = null;
                             Response.Redirect(GetRequestURLWithOutPort());
                         }
                     }
                     else
                     {
                         OnCancelCopyInvoice(this, null);
                     }
                 }
            }
        }

        private void CopyMemberInvoice()
        {
            var invoiceId = int.Parse(Request.QueryString["memberInvoiceId"]);
            var copyOfInvoice = CopyOrderProvider.CopyShoppingCartFromMemberInvoice(DistributorID, Locale, invoiceId);
            if (copyOfInvoice != null)
            {
                SessionInfo.ConfirmedAddress = false;
                ShoppingCart = copyOfInvoice;
                if (ShoppingCart.IsFromInvoice
                    && ShoppingCart.CopyInvoiceStatus == CopyInvoiceStatus.AddressValidationFail
                    && ShoppingCart.CopyInvoiceAddress != null)
                {
                    _checkoutOptions.ValidateInvoiceAddress();
                }
                else
                {
                    Session["IsCopingFromInvoice"] = null;
                    Response.Redirect(GetRequestURLWithOutPort());
                }
            }
            else
            {
                OnCancelCopyMemberInvoice();
            }
        }

        /// <summary>
        ///     Saves the cart and redirects.
        /// </summary>
        /// <param name="cartName">Cart name.</param>
        /// <param name="update">Flag to update the same saved cart</param>
        private void SaveCart(string cartName, bool update)
        {
            if (!update)
            {
                var optionType = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), OptionType.ToString());
                ShoppingCart.CopyCartWithShippingInfo(true, cartName, ShippingAddresssID, DeliveryOptionID, optionType);
                if (ShoppingCart.CustomerOrderDetail == null)
                {
                    ShoppingCart.ClearCart();
                }
            }
            mdlClearCart.Hide();
            CopyInvoice();
        }

        /// <summary>
        ///     Show invalid cart name div.
        /// </summary>
        private void ShowInvalidCartName()
        {
            // Set the action.
            var script = "if (localAction) { SetConfirmationPopUp(localAction, false); }";
            ScriptManager.RegisterStartupScript(Page, Page.GetType(), Guid.NewGuid().ToString(), script, true);

            // Show the div.
            divExistentCart.Visible = true;
            txtSaveCartName.BorderColor = Color.Red;
            if (string.IsNullOrEmpty(txtSaveCartName.Text.Trim()))
            {
                lblEmptyCartName.CssClass = lblEmptyCartName.CssClass.Replace("hide", "").Trim();
                lblExistentCart.CssClass += " hide";
            }
            else
            {
                lblExistentCart.CssClass = lblExistentCart.CssClass.Replace("hide", "").Trim();
                lblEmptyCartName.CssClass += " hide";
            }
        }

        public void OnCancelCopyInvoice(object sender, EventArgs e)
        {
            if (IsFromMemberInvoice())
            {
                OnCancelCopyMemberInvoice();
            }
            else
            {
                Session["IsCopingFromInvoice"] = null;
                var invoiceNum = Request.QueryString["invoiceNum"];
                if (string.IsNullOrEmpty(invoiceNum))
                {
                    Response.Redirect("~/Bizworks/MyInvoices.aspx");
                }
                else
                {
                    Response.Redirect(string.Format("~/Bizworks/MyInvoiceDetails.aspx?invoiceId={0}",
                        Request.QueryString["invoiceNum"]));
                }
            }
        }

        private void OnCancelCopyMemberInvoice()
        {
            Session["IsCopingFromInvoice"] = null;
            var invoiceNum = Request.QueryString["invoiceNum"];
            if (string.IsNullOrEmpty(invoiceNum))
            {
                Response.Redirect("~/Ordering/Invoice");
            }
            else
            {
                Response.Redirect(string.Format("~/Ordering/Invoice/Display/{0}",
                    GetMemberInvoiceId()));
            }
        }

        protected void OnTermConditionOk(object sender, EventArgs e)
        {
            TermConditionPopupExtender.Hide();
            if (DistributorOrderingProfile.IsTermConditionAlert)
            {
                if (DistributorOrderingProfileProvider.UpdateAlertCustomerExtention(DistributorID))
                {
                    DistributorOrderingProfile.IsTermConditionAlert = false;
                }
            }
        }

        protected void OnTermConditionNo(object sender, EventArgs e)
        {
            TermConditionPopupExtender.Hide();
            Response.Redirect("~/Ordering/Pricelist.aspx?ETO=False");
        }
    }
}