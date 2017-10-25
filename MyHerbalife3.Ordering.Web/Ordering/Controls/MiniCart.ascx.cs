using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.ViewModel.Models;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class MiniCart : UserControlBase
    {
        private readonly List<string> _errors = new List<string>();
        private int totalQuantity;
        public bool UpdateCalled { get; set; }
        public OrderTotals_V01 Totals { get; set; }

        [Publishes(MyHLEventTypes.QuoteError)]
        public event EventHandler OnQuoteError;

        [Publishes(MyHLEventTypes.ProceedingToCheckoutFromMiniCart)]
        public event EventHandler ProceedingToCheckoutFromMiniCart;

        [Publishes(MyHLEventTypes.ProductDetailBeingLaunched)]
        public event EventHandler ProductDetailBeingLaunched;

        [Publishes(MyHLEventTypes.ShoppingCartDeleted)]
        public event EventHandler ShoppingCartDeleted;

        [SubscribesTo(MyHLEventTypes.PickUpNicknameNotSelectedInMiniCart)]
        public void OnPickUpNicknameNotSelectedInMiniCart(object sender, EventArgs e)
        {
            int itemId = 0;
            string message = "ShippingOrPickupNeeded";
            var args = e as DeliveryOptionEventArgs;
            if (null != args)
            {
                itemId = args.DeliveryOptionId;
            }
            switch (itemId)
            {
                case 1:
                    {
                        message = "ShippingOptionNotPopulated";
                        break;
                    }
                case 2:
                    {
                        message = "PickUpOptionNotPopulated";
                        break;
                    }
                case 3:
                    {
                        message = "PickUpFromCourierOptionNotPopulated";
                        break;
                    }
            }
            _errors.Add(GetLocalResourceObject(message).ToString());
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnRtHelpClose.Click += btnRtHelpClose_Click;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (trYourLevel != null) trYourLevel.Visible = false; // Hide row by default
            if (ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled && SessionInfo.DsType == ServiceProvider.DistributorSvc.Scheme.Member && SessionInfo.IsHAPMode == true)
            {
                ViewEntireCart.Visible = false;
            }
            else
                ViewEntireCart.Visible = true;
            ProductsBase.OrderMonth.RegisterOrderMonthControls(pnlOrderMonth, null, ProductsBase.IsEventTicketMode,
                                                               pnlOrderMonthLabel, SessionInfo.IsHAPMode);

            var PLControl = loadPurchasingLimitsControl(false);
            if (PLControl != null)
            {
                trPurchaseLimits.Visible = true;
                pnlPurchaseLimits.Controls.Add(PLControl);
            }

            ShowClearCartLink();

            if (ShoppingCart.OrderCategory == OrderCategoryType.HSO && SessionInfo.IsHAPMode
                && HLConfigManager.Configurations.DOConfiguration.AllowHAP)
            {
                var hapControl = loadHAPControl(false);
                trHAPControl.Visible = true;
                pnlHAP.Visible = true;
                pnlHAP.Controls.Add(hapControl);
                //TODO: add HAP control
            }


            if (!IsPostBack)
            {
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                loadCart();
            }
            else
            {
                bool isNotified;
                bool.TryParse(Session["displayNonResidentModal"] != null ? Session["displayNonResidentModal"].ToString() : "false", out isNotified);

                if (isNotified)
                {
                    Session["displayNonResidentModal"] = false;
                    Response.Redirect("~/Ordering/ShoppingCart.aspx");
                }
            }

            // Display the saved cart name and command 
            if (HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts &&
                ShoppingCart.OrderCategory == OrderCategoryType.RSO)
            {
                if (!(APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems) && HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed))
                {
                    var savedCartCommand = LoadControl("~/Ordering/Controls/SaveCartCommand.ascx");
                    (savedCartCommand as SaveCartCommand).IsInMinicart = true;
                    plSavedCartCommand.Controls.Add(savedCartCommand);
                    if (ShoppingCart != null && ShoppingCart.IsSavedCart && !string.IsNullOrEmpty(ShoppingCart.CartName))
                    {
                        lblSavedCartName.Text = ShoppingCart.CartName;
                        SavedCartTitle.Visible = true;
                    }
                    else
                    {
                        SavedCartTitle.Visible = false;
                    }
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                trOrderMonth.Visible = trOrderMonthVP.Visible = false;
                if (ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
                {
                    trDiscountRate.Visible = false;
                    if (trYourLevel != null) trYourLevel.Visible = true;
                    uxYourLevel.Text = GetGlobalResourceString(string.Format("DisplayLevel_{0}", ProductsBase.LevelSubType), defaultValue: string.Empty);
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.DisplayBifurcationKeys &&
                ShoppingCart.DsType != null && ShoppingCart.DsType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                TxtVolumePoint.Text = (string)GetLocalResourceObject("TxtVolumePointResource1MB.Text");
            }
        }

        private void btnRtHelpClose_Click(object sender, EventArgs e)
        {
            ModalpnlHelp.Hide();
            //string script = "return HideRtHelp();";
            //ScriptManager.RegisterStartupScript(this, GetType(), "script_btnRtHelpClose", script, true);
        }

        protected string GetLink(DistributorShoppingCartItem item)
        {
            string link = string.Empty;

            if (!APFDueProvider.IsAPFSku(item.SKU) && null != item.ProdInfo && null != item.ParentCat)
            {
                link = string.Format("~/Ordering/ProductDetail.aspx?ProdInfoID={0}&CategoryID={1}", item.ProdInfo.ID,
                                     item.ParentCat.ID);
            }

            return link;
        }

        protected void ProductDetailClicked(object sender, EventArgs e)
        {
            int CategoryID = 0, ProductID = 0;
            var lb = sender as LinkButton;

            if (lb.CommandArgument.Equals("disable"))
            {
                return;
            }

            if (lb != null)
            {
                string commandArgument = lb.CommandArgument;
                var commandParts = commandArgument.Split(' ');
                ProductID = int.Parse(commandParts[0]);
                CategoryID = int.Parse(commandParts[1]);
                ProductDetailBeingLaunched(this, new ProductDetailEventArgs(CategoryID, ProductID));
            }
        }

        private void loadCart()
        {
            decimal distributorDiscount = ProductsBase.DistributorDiscount;

            Totals = null;

            trSubtotal.Visible = HLConfigManager.Configurations.CheckoutConfiguration.DisplaySubTotalOnMinicart;
            if (ShoppingCart != null && ShoppingCart.CartItems != null)
            {
                totalQuantity = getTotalNumberItems(ShoppingCart.ShoppingCartItems);

                if (HLConfigManager.Configurations.DOConfiguration.IsResponsive && (Master as OrderingMaster).IsMobile())
                {
                    (Master as OrderingMaster).setTotalItemsMobile(totalQuantity);
                }

                if (HLConfigManager.Configurations.DOConfiguration.ShowTotalItems)
                {
                    TotalQty.Visible = true;
                    lbNumItems.Visible = true;
                    TotalQty.Text = totalQuantity.ToString();
                }
                else
                {
                    TotalQty.Visible = false;
                    lbNumItems.Visible = false;
                }
                CartItemListView.DataSource = ShoppingCart.SelectMaxItems();
                CartItemListView.DataBind();
                if (ShoppingCart.Totals != null && ShoppingCart.CartItems.Count > 0)
                {
                    OrderTotals_V01 totals = ShoppingCart.Totals as OrderTotals_V01;
                    if (HLConfigManager.Configurations.CheckoutConfiguration.YourPriceWithAllCharges &&
                        null != totals.ItemTotalsList)
                    {
                        uxSubtotal.Text = getAmountString(OrderProvider.getPriceWithAllCharges(ShoppingCart.Totals as OrderTotals_V01));
                    }
                    else
                    {
                        uxSubtotal.Text = getAmountString(totals.DiscountedItemsTotal);
                    }

                    uxVolume.Text = ProductsBase.GetVolumePointsFormat(totals.VolumePoints);
                    if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                    {
                        TxtVolumePoint.Visible = uxVolume.Visible = false;
                    }
                    if (!HLConfigManager.Configurations.CheckoutConfiguration.HasOrderMonthVolumePoints)
                    {
                        trOrderMonthVP.Visible = false;
                    }
                    if (HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePointRange)
                    {
                        uxVolumePointRange.Text = HLRulesManager.Manager.PerformDiscountRangeRules(ShoppingCart, Locale,
                                                                                                   ProductsBase
                                                                                                       .DistributorDiscount);
                        trVolumePointRange.Visible = !string.IsNullOrEmpty(uxVolumePointRange.Text);
                        trDiscountRate.Visible = false;
                    }
                    else
                    {
                        uxDiscountRate.Text = (totals.DiscountPercentage).ToString() + "%";
                    }
                    Totals = ShoppingCart.Totals as OrderTotals_V01;
                }
                else
                {
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                    {
                        if ((ShoppingCart.Totals == null || (ShoppingCart.Totals as OrderTotals_V01).AmountDue == 0.0M) &&
                            !(ShoppingCart.OrderCategory == OrderCategoryType.ETO && HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket) &&
                            (ShoppingCart.ShoppingCartItems != null && ShoppingCart.ShoppingCartItems.Any()))
                        {
                            OnQuoteError(null, null);
                        }
                    }

                    uxVolume.Text = ProductsBase.GetVolumePointsFormat(0.00M);
                    if (DistributorOrderingProfile != null && DistributorOrderingProfile.IsPC)
                    {
                        TxtVolumePoint.Visible = uxVolume.Visible = false;
                    }
                    if (HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePointRange)
                    {
                        uxVolumePointRange.Text = HLRulesManager.Manager.PerformDiscountRangeRules(ShoppingCart, Locale,
                                                                                                   ProductsBase
                                                                                                       .DistributorDiscount);
                        trVolumePointRange.Visible = !string.IsNullOrEmpty(uxVolumePointRange.Text);
                        trDiscountRate.Visible = false;
                    }
                    else
                    {
                        uxDiscountRate.Text = (distributorDiscount).ToString() + "%";
                    }
                    uxSubtotal.Text = getAmountString((0.00M));
                }

                lblOrderMonthVolume.Text = (Page as ProductsBase).CurrentMonthVolume;
                if (!HLConfigManager.Configurations.CheckoutConfiguration.HasOrderMonthVolumePoints)
                {
                    trOrderMonthVP.Visible = false;
                }

                //New Code to hide/display the ?

                string levelDSType = (Page as ProductsBase).Level;
                if (levelDSType == "DS")
                {
                    imgOrderMonth.Visible = true;
                }
                else
                {
                    imgOrderMonth.Visible = false;
                }

                noItem.Visible = false;
                noItemToPurchase.Visible = false;
                if (ShoppingCart.CartItems.Count == 0)
                {
                    //New use case  GlobalDO_UC_08.0 Can checkout with empty cart
                    //this.noItem.Visible = true;
                    //this.noItemToPurchase.Visible = false;
                    //this.RecentlyAdded.Visible = false;
                }
            }
            else
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.ShowVolumePointRange)
                {
                    uxVolumePointRange.Text = HLRulesManager.Manager.PerformDiscountRangeRules(ShoppingCart, Locale,
                                                                                               ProductsBase
                                                                                                   .DistributorDiscount);
                    trVolumePointRange.Visible = !string.IsNullOrEmpty(uxVolumePointRange.Text);
                    trDiscountRate.Visible = false;
                }
                else
                {
                    uxDiscountRate.Text = (distributorDiscount).ToString() + "%";
                }
                noItem.Visible = true;
                noItemToPurchase.Visible = false;

                if (!HLConfigManager.Configurations.CheckoutConfiguration.HasOrderMonthVolumePoints)
                {
                    trOrderMonthVP.Visible = false;
                }
            }

            // Only call the update if the control has not been rendered yet.
            // This may need to be re-worked to properly update when the quote is recalculated
            // on changes of shipping location, etc by the ShippingInfoControl.
            //if (!this._isRendered)
            //{
            //upMiniCart.Update();
            //}
            ShowClearCartLink();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            if (UpdateCalled)
            {
                upMiniCart.Update();
            }
        }

        [SubscribesTo(MyHLEventTypes.CartItemRemovedDueToSKULimitationRules)]
        public void OnCartItemRemovedDueToSKULimitationRules(object sender, EventArgs e)
        {
            if (null != ShoppingCart && null != ShoppingCart.CartItems && ShoppingCart.CartItems.Count > 0)
            {
                ShoppingCart.Calculate();
            }
            loadCart();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.OrderMonthChanged)]
        public void OnOrderMonthChanged(object sender, EventArgs e)
        {
            if (null != ShoppingCart && null != ShoppingCart.CartItems && ShoppingCart.CartItems.Count > 0)
            {
                ShoppingCart.Calculate();
            }
            loadCart();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.QuoteRetrieved)]
        public void OnQuoteRetrieved(object sender, EventArgs e)
        {
            loadCart();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void OnShoppingCartChanged(object sender, EventArgs e)
        {
            //if (this.ShoppingCart.DeliverInfoRemoved)
            //{
            //    this.ShoppingCart.DeliverInfoRemoved = false;
            //    (this.Page as ProductsBase).NoSavedAddress();
            //    return;
            //}
            loadCart();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.ShowBackorderMessages)]
        public void OnShowBackorderMessages(object sender, EventArgs e)
        {
            var args = e as ShowBackorderMessagesEventArgs;
            if (args != null)
            {
                ProductsBase.ShowBackorderMessage(args.BackorderMessages);
            }
        }

        private int getTotalNumberItems(List<DistributorShoppingCartItem> items)
        {
            int total = 0;
            if (items != null)
            {
                if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
                {
                    total = items.Where(i => i.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                                             i.SKU !=
                                             HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                                 .Sum(i => i.Quantity);
                }
                else
                {
                    total = items.Sum(i => i.Quantity);
                }
            }
            return total;
        }

        protected void checkoutClicked(object sender, EventArgs e)
        {
            if (!CheckDSCantBuyStatus())
            {
                return;
            }

            CheckHAPOptions();

            ProceedingToCheckoutFromMiniCart(this, null);

            if (_errors.Count > 0)
            {
                //this.noItemToPurchase.Visible = true;
                //this.noItemToPurchase.Text = _errors[0];
                (Page.Master as OrderingMaster).Status.AddMessage(
                    StatusMessageType.Error, _errors[0]);
                return;
            }
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.PurchasingLimitsControl))
            {
                //if (PurchasingLimitProvider.RequirePurchasingLimits(this.DistributorID, this.CountryCode))
                //{

                //Check Value Selected in Drop Down
                if (String.IsNullOrEmpty(ShoppingCart.SelectedDSSubType))
                {
                    //TODO- Replace with resource entry
                    noItemToPurchase.Visible = true;
                    noItemToPurchase.Text = GetLocalResourceObject("OrderTypeNotSelected").ToString();
                    return;
                }
                //}
            }

            if (!CheckDRFraud())
            {
                return;
            }

            noItemToPurchase.Visible = false;
            errDRFraud.Visible = false;
            //}
            //else
            //{
            //    this.noItemToPurchase.Visible = true;
            //}

            bool isNotified;
            bool.TryParse(Session["isNonResidentNotified"] != null ? Session["isNonResidentNotified"].ToString() : "false", out isNotified);

            if (HLConfigManager.Configurations.DOConfiguration.DisplayNonResidentsMessage && !isNotified)
            {
                var member = (MembershipUser<DistributorProfileModel>)System.Web.Security.Membership.GetUser();

                if (ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup &&
                    !SessionInfo.IsEventTicketMode && !APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems) &&
                    member != null && member.Value != null && !member.Value.ProcessingCountryCode.Equals(CountryCode))
                {
                    Session["isNonResidentNotified"] = Session["displayNonResidentModal"] = true;
                    (this.Master as OrderingMaster).DisplayHtml("NonResidentsDisclaimer.html");
                    return;
                }
            }

            if (SessionInfo != null && !SessionInfo.IsFirstTimeSpainPopup && ShoppingCart != null && ShoppingCart.Locale == "es-ES" && !APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems) &&
                ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0 && ShoppingCart.OrderCategory == OrderCategoryType.RSO && 
                !HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed && ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
            {
                SpainPlasticBagAlert.Show();                
            }
            else
                Response.Redirect("~/Ordering/ShoppingCart.aspx");
        }
        protected void btnOK_Click(object sender, EventArgs e)
        {
            string countries = HL.Common.Configuration.Settings.GetRequiredAppSetting("CatalogCategorySpainIDs", "5655,1017,1017");
            var commandParts = countries.Split(',');
            if (SessionInfo != null)
                SessionInfo.IsFirstTimeSpainPopup = true;
            Response.Redirect(string.Format("~/Ordering/Catalog.aspx?cid={0}&root={1}&parent={2}", commandParts[0], commandParts[1], commandParts[2]));
        }
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            if (SessionInfo != null)
                SessionInfo.IsFirstTimeSpainPopup = true;
            Response.Redirect("~/Ordering/ShoppingCart.aspx");
        }

        private bool CheckDRFraud()
        {
            //Check for DR fraud..

            ShoppingCartProvider.CheckDSFraud(ShoppingCart);
            //if (HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
            //{
            //    if (null != ShoppingCart && null != ShoppingCart.DeliveryInfo &&
            //        null != ShoppingCart.DeliveryInfo.Address &&
            //        null != ShoppingCart.DeliveryInfo.Address.Address)
            //    {
            //        ShoppingCart.DSFraudValidationError = ShoppingCartProvider.GetDSFraudResxKey(DistributorOrderingProfileProvider.CheckForDRFraud(DistributorID, CountryCode,
            //                                                                                  ShoppingCart.DeliveryInfo
            //                                                                                              .Address
            //                                                                                              .Address
            //                                                                                              .PostalCode));

            if (!string.IsNullOrEmpty(ShoppingCart.DSFraudValidationError))
            {
                errDRFraud.Visible = true;
                errDRFraud.Text =
                    HttpContext.GetGlobalResourceObject(
                        string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                        ShoppingCart.DSFraudValidationError) as string;
                return false;
            }
            //    }
            //}
            return true;
        }

        private bool CheckDSCantBuyStatus()
        {
            if ((Page as ProductsBase).CantBuy || (Page as ProductsBase).Deleted)
            {
                //this.errDRFraud.Visible = true;
                //this.errDRFraud.Text = MyHL_ErrorMessage.CantOrder;
                (Master).CantBuyCantVisitShoppingCart = true;
                return false;
            }
            return true;
        }

        protected void ViewEntireCart_Clicked(object sender, EventArgs e)
        {
            if (!CheckDSCantBuyStatus())
            {
                return;
            }

            //Let the generic event check to launch the address popup.
            //if (null != ShoppingCart && null == ShoppingCart.DeliveryInfo)
            //{
            Response.Redirect("~/Ordering/ShoppingCart.aspx");
            errDRFraud.Visible = false;
            noItemToPurchase.Visible = false;
            //}

            //if (ShoppingCart.CartItems != null && ShoppingCart.CartItems.Count > 0)
            //{
            //    Response.Redirect("~/Ordering/ShoppingCart.aspx");
            //    this.errDRFraud.Visible = false;
            //    this.noItemToPurchase.Visible = false;
            //}
            //else
            //{
            //    this.noItemToPurchase.Visible = true;
            //}
        }

        protected void CartItemListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                var prodDesc = (LinkButton)e.Item.FindControl("LinkProductDetail");
                var hdnSKU = (HiddenField)e.Item.FindControl("hdnSKU");
                var nonLinkedSkus = APFDueProvider.GetAPFSkuList();
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku);
                }
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku))
                {
                    nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku);
                }
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku))
                {
                    nonLinkedSkus.Add(HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku);
                }
                if (prodDesc != null && hdnSKU != null)
                {
                    if (nonLinkedSkus.Contains(hdnSKU.Value))
                    {
                        prodDesc.OnClientClick = "return false;";
                        prodDesc.CommandArgument = "disable";
                        prodDesc.CssClass = "disable-link";
                    }

                    if (IsChina)
                    {
                        //Extra checking to ensure the command argument being updated properly for the item without product info.
                        if (string.IsNullOrEmpty(prodDesc.CommandArgument) || prodDesc.CommandArgument.Trim() == string.Empty)
                        {
                            prodDesc.OnClientClick = "return false;";
                            prodDesc.CommandArgument = "disable";
                            prodDesc.CssClass = "disable-link";
                        }
                    }
                    if (ProductsBase.GlobalContext.CultureConfiguration.IsBifurcationEnabled && SessionInfo.DsType == ServiceProvider.DistributorSvc.Scheme.Member && SessionInfo.IsHAPMode == true)
                    {
                        prodDesc.OnClientClick = "return false;";
                        prodDesc.CommandArgument = "disable";
                        prodDesc.CssClass = "disable-link";
                    }
                }
            }
        }

        protected void btnYesCancel_Click(object sender, EventArgs e)
        {
            doCartEmpty();
            ShoppingCartDeleted(this, null);
        }

        /// <summary>
        ///     The do cart empty.
        /// </summary>
        private void doCartEmpty()
        {
            if (ShoppingCart != null)
            {
                if (!ShoppingCart.CartItems.Count.Equals(0))
                {
                    ProductsBase.ClearCart();
                }
            }
        }

        //sets the visibility of 'Clear Cart' link.
        private void ShowClearCartLink()
        {
            if (ShoppingCart != null)
            {
                if (ShoppingCart.CartItems.Count.Equals(0) || ShoppingCart.CustomerOrderDetail != null)
                {
                    ClearCart.Visible = false;
                }
                else
                {
                    if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                    {
                        ClearCart.Visible = APFDueProvider.CanRemoveAPF(DistributorID, Locale, null);
                        if (DistributorOrderingProfile != null && HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            if (!HLConfigManager.Configurations.APFConfiguration.AllowDSRemoveAPFWhenDue)
                                ClearCart.Visible = DistributorOrderingProfile.CNAPFStatus != 2;

                        }
                    }
                    else
                    {
                        ClearCart.Visible = true;
                    }
                }
            }
        }

        private void CheckHAPOptions()
        {
            if (HLConfigManager.Configurations.DOConfiguration.AllowHAP && SessionInfo.IsHAPMode && ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                if (ShoppingCart.HAPScheduleDay == 0)
                {
                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "MissingHapDate"));
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void OnOrderSubTypeChanged(object sender, EventArgs e)
        {
            loadCart();
            UpdateCalled = true;
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            loadCart();
            UpdateCalled = true;
        }
    }
}
