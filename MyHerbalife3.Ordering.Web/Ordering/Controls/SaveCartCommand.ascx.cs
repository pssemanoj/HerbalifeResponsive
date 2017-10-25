using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Providers.EventHandling;
using System.Collections.Generic;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.ViewModel.Models;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    /// <summary>
    ///     Class used to save a cart.
    /// </summary>
    public partial class SaveCartCommand : UserControlBase
    {
        [Publishes(MyHLEventTypes.OnSaveCart)]
        public event EventHandler OnSaveCartCheckAddressInfo;

        #region Properties

        /// <summary>
        ///     Gets set a flag to indicate when the control is in Minicart
        /// </summary>
        public bool IsInMinicart { get; set; }

        #endregion Properties

        #region Methods and event handling

        /// <summary>
        ///     Loads the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.ShoppingCart.OrderCategory != OrderCategoryType.APF)
            {
                if (this.IsInMinicart)
                {
                    this.lnkToSavedCarts.Visible = false;
                    this.OvalSaveCartLink.Visible = true;
                    DisplayData();
                }
                else
                {
                    if (this.ShoppingCart.ShoppingCartItems != null & this.ShoppingCart.ShoppingCartItems.Count > 0)
                    {
                        this.OvalSaveCartButton.Visible = true;
                     
                    }
                 
                }

                btnCancel.Visible = this.ShoppingCart.IsSavedCart;
                if (this.ShoppingCart.IsSavedCart)
                {
                    this.mdlSaveCart.TargetControlID = "FakeButtonTarget";
                    this.mdlContinue.TargetControlID = this.IsInMinicart
                                                           ? "OvalSaveCartLink"
                                                           : this.OvalSaveCartButton.Visible
                                                                 ? "OvalSaveCartButton"
                                                                 : "FakeButtonTarget";
                    this.lblSavedCartMessage1.Text =
                        string.Format(GetLocalResourceObject("lblSavedCartMessage1.Text") as string,
                                      this.ShoppingCart.CartName);
                    this.updSavedCart.Update();
                }

                //this.txtSaveCartName.Text = SuggestCartName(this.ShoppingCart.DeliveryInfo, string.Empty, this.DistributorID, this.Locale);
            }
            else
            {
                this.lnkToSavedCarts.Visible =
                    this.imgSavedCartsHelp.Visible =
                    this.OvalSaveCartLink.Visible = false;
            }
        }

        public void ListnerMethod()
        {
            
            if (ShoppingCart.ShoppingCartItems != null & ShoppingCart.ShoppingCartItems.Count > 0)
            {
                this.OvalSaveCartButton.Visible = true;
                this.OvalSaveCartButton.Enabled =
                    !(APFDueProvider.containsOnlyAPFSku(ShoppingCart.ShoppingCartItems) &&
                      HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed);
                this.OvalSaveCartButton.Disabled = !this.OvalSaveCartButton.Enabled;
            }
            else
            {
                this.OvalSaveCartButton.Visible = false;
            }
            btnCancel.Visible = this.ShoppingCart.IsSavedCart;
        }
        /// <summary>
        ///     When renders the control
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.OvalSaveCartLink.Disabled = this.ShoppingCart.CartItems.Count.Equals(0);
            this.OvalSaveCartLink.Enabled = !this.OvalSaveCartLink.Disabled;
            this.OvalSaveCartLink.OnClientClick =
                this.OvalSaveCartLink.OnClientClick.Replace("return false;", " ");
        }

        /// <summary>
        ///     When the Save Cart button is clicked
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">EventArgs.</param>
        protected void OnSaveCartClick(object sender, EventArgs e)
        {
            OnSaveCartCheckAddressInfo(this, null);
            var CantSaveCart = Session["CantSaveCart"] != null ? (bool)Session["CantSaveCart"] : false;
            if(CantSaveCart)
            {
                Session.Remove("CantSaveCart");
                return; // DeliveryInfo, ShippingAddrees or PickupLocation not selected
            }

            this.ShowError(false);
            this.txtSaveCartName.Text = SuggestCartName(this.ShoppingCart.DeliveryInfo, string.Empty, this.DistributorID,
                                                        this.Locale);
            this.mdlSaveCart.Show();
        }

        /// <summary>
        ///     Verifies if the cart can be saved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnSaveCart(object sender, EventArgs e)
        {
            string cartName = txtSaveCartName.Text;
            if (!string.IsNullOrEmpty(cartName))
            {
                if (!ShoppingCartProvider.CartExists(this.DistributorID, this.Locale, cartName))
                {
                    this.SaveCart(cartName, false);
                }
                else
                {
                    // Validate if the active cart is the same cart to save
                    if (this.ShoppingCart.IsSavedCart && this.ShoppingCart.CartName.ToUpper().Equals(cartName.ToUpper()))
                    {
                        this.SaveCart(cartName, true);
                    }
                    else
                    {
                        this.ShowError(true);
                    }
                }
            }
            else
            {
                this.ShowError(true);
            }
        }

        /// <summary>
        ///     Shows a confirmation popup.
        /// </summary>
        protected void ShowContinuePopup()
        {
            this.mdlSaveCart.Hide();
            this.mdlContinue.Show();
        }

        /// <summary>
        ///     Redirects to the on line price list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnContinue(object sender, EventArgs e)
        {
            if (this.ShoppingCart.IsSavedCart)
            {
                (this.Page as ProductsBase).NewCart();
                Response.Redirect(string.Format("~/Ordering/Pricelist.aspx?CartID={0}",
                                                this.ShoppingCart.ShoppingCartID));
            }
            else
            {
                if (this.ShoppingCart.CustomerOrderDetail == null)
                {
                    this.ShoppingCart.ClearCart();
                    SetShoppingCartModuleCache(this.ShoppingCart);
                    Response.Redirect("~/Ordering/PriceList.aspx");
                }
                else
                {
                    (this.Page as ProductsBase).NewCart();
                    Response.Redirect(string.Format("~/Ordering/PriceList.aspx?CartID={0}",
                                                    (this.Page as ProductsBase).ShoppingCart.ShoppingCartID));
                }
            }
        }

        private void SetShoppingCartModuleCache(MyHLShoppingCart _shoppingCart)
        {
            var cartWidgetSource = new CartWidgetSource();
            cartWidgetSource.SetCartWidgetCache(_shoppingCart);
        }

        /// <summary>
        ///     User cancel Price list redirection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void CancelContinue(object sender, EventArgs e)
        {
            // Reloading saved cart properties, getting property IsSavedCart = true and CartName.
            this.ShoppingCart.IsSavedCart = true;
            this.ShoppingCart.CartName = txtSaveCartName.Text;

            // Redirection.
            Response.Redirect(this.IsInMinicart ? "~/Ordering/Pricelist.aspx?ETO=FALSE" : "~/Ordering/ShoppingCart.aspx");
        }

        /// <summary>
        ///     Saves the cart.
        /// </summary>
        /// <param name="cartName">Cart name.</param>
        /// <param name="update">Flag to update the same saved cart</param>
        private void SaveCart(string cartName, bool update)
        {
            if (!update)
            {
                if(HLConfigManager.Configurations.DOConfiguration.IsChina)
                    ShoppingCart.removeItem(APFDueProvider.GetAPFSku());
                var optionType = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), ProductsBase.OptionType.ToString());
                this.ShoppingCart.CopyCartWithShippingInfo(true, cartName, ProductsBase.ShippingAddresssID,
                                                           ProductsBase.DeliveryOptionID, optionType);
            }
            this.lblSavedCartMessage1.Text = string.Format(
                GetLocalResourceObject("lblSavedCartMessage1.Text") as string, cartName);
            this.updSavedCart.Update();
            this.ShowContinuePopup();
        }

        /// <summary>
        ///     Shows or hides the line error in the popup.
        /// </summary>
        /// <param name="show">Flag to show the error.</param>
        private void ShowError(bool show)
        {
            this.divExistentCart.Visible = show;
            this.txtSaveCartName.BorderColor = show ? Color.Red : (new TextBox()).BorderColor;
            var emptyName = string.IsNullOrEmpty(txtSaveCartName.Text);
            lblEmptyCartName.Visible = emptyName;
            lblExistentCart.Visible = !emptyName;

            // Ensure that the pop up is visible.
            if (show)
            {
                this.mdlSaveCart.Show();
            }
        }

        /// <summary>
        ///     Suggests a name for the cart to be saved.
        /// </summary>
        /// <param name="cartName">The initial cart name</param>
        /// <returns></returns>
        public static string SuggestCartName(ShippingInfo deliveryInfo,
                                             string cartName,
                                             string distributorID,
                                             string locale)
        {
            string suggestedName = cartName;
            if (string.IsNullOrEmpty(suggestedName))
            {
                if (deliveryInfo != null)
                {
                    if (deliveryInfo.Option == DeliveryOptionType.Shipping)
                    {
                        suggestedName = deliveryInfo.Address.Recipient ?? string.Empty;
                    }
                    else
                    {
                        suggestedName =
                            DistributorProfileModelHelper.DistributorName(
                                ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value);
                    }
                }

                if (string.IsNullOrEmpty(suggestedName))
                {
                    return suggestedName;
                }
            }
            else
            {
                Match match = Regex.Match(suggestedName, @"([A-Za-z0-9\-\s_]+)_\d+");
                if (match.Success)
                {
                    suggestedName = match.Groups[1].Value;
                }
            }

            var savedCarts = ShoppingCartProvider.GetCarts(distributorID, locale);
            int index = 0;
            if (savedCarts != null && savedCarts.Count > 0)
            {
                index =
                    savedCarts.Count(
                        c =>
                        !string.IsNullOrEmpty(c.CartName) && c.CartName.ToUpper().StartsWith(suggestedName.ToUpper()));
            }
            return (index == 0) ? suggestedName : string.Format("{0}_{1}", suggestedName, index + 1);
        }

        #endregion Methods and event handling

        private void DisplayData()
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {

                List<Tuple<string, bool>> cartItems = new List<Tuple<string, bool>>();
                foreach (var i in ShoppingCart.ShoppingCartItems) cartItems.Add(new Tuple<string, bool>(i.SKU, i.IsPromo));

                if (ShoppingCart.ItemsBeingAdded != null)
                { 
                    foreach (var i2 in ShoppingCart.ItemsBeingAdded)
                    {
                        if (!cartItems.Contains(new Tuple<string, bool>(i2.SKU, i2.IsPromo)))
                            cartItems.Add(new Tuple<string, bool>(i2.SKU, i2.IsPromo));
                    }
                }

                //var apfsku  = this.ShoppingCart.ShoppingCartItems.Find(s => s.SKU ==APFDueProvider.GetAPFSku());
                if (cartItems != null)
                {
                    if (cartItems.Count == 1)
                    {
                        if (cartItems[0].Item1 == APFDueProvider.GetAPFSku())
                        {
                            OvalSaveCartLink.Visible = false;
                            imgSavedCartsHelp.Visible = false;
                        }
                        else if (cartItems[0].Item2 == true)
                        {
                            OvalSaveCartLink.Visible = false;
                            imgSavedCartsHelp.Visible = false;
                        }
                        else
                        {
                            OvalSaveCartLink.Visible = true;
                            imgSavedCartsHelp.Visible = true;
                        }

                    }
                    else if (cartItems.Count < 1)
                    {
                        OvalSaveCartLink.Visible = false;
                        imgSavedCartsHelp.Visible = false;
                    }
                    else
                    {
                        OvalSaveCartLink.Visible = true;
                        imgSavedCartsHelp.Visible = true;
                    }
                }
            }
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void UpdateFromCart(object sender, EventArgs e)
        {
            DisplayData();
        }
    }
}