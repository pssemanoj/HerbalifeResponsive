using System;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using RulesManager = MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers;
using System.Linq;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class HFF_CN : UserControlBase
    {
        decimal _selfAmount = decimal.Zero;
        decimal _behalfAmount = decimal.Zero;
        decimal _otherAmount = decimal.Zero;
        int _skuQuantity = 0;
        List<string> errors = null;

        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnQuoteRetrieved;

        [Publishes(MyHLEventTypes.OnStandAloneDonationClear)]
        public event EventHandler OnStandAloneDonationClear;

        [SubscribesTo(MyHLEventTypes.ShoppingCartRecalculated)]
        public void OnCartRecalculated(object sender, EventArgs e)
        {
            HideMessage();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if ((null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode) || this.ShoppingCart.OrderCategory == OrderCategoryType.HSO)
            {
                divHFF.Visible = false;
                hffPanel.Update();
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    hlHFF.Visible = false;
                    hffPanel.Visible = false;
                }
                return;
            }
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                if (CatalogProvider.IsPreordering(ShoppingCart.CartItems, (ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.WarehouseCode : string.Empty)))
                {
                    hlHFF.Visible = false;
                    hffPanel.Visible = false;
                    return;
                }
            }
            // Verify special rules to show the module
            ShowModule();
            if (!divHFF.Visible)
            {
                return;
            }

            hlHFF.NavigateUrl = HLConfigManager.Configurations.DOConfiguration.HFFUrl;
            hlHFF.Visible = !string.IsNullOrEmpty(hlHFF.NavigateUrl) &&
                            HLConfigManager.Configurations.DOConfiguration.ShowHFFBox;

            if (!IsPostBack)
            {
                //tbQuantity.Attributes["onkeypress"] = "Numeric(event, this)";

                // tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
                var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                if (HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && OrderTotals.Donation > 0 && SessionInfo.StandAloneDonationNotSubmit > 0)
                {
                    SessionInfo.ClearStandAloneDonation();
                    OnStandAloneDonationClear(this, e);
                }
                lblHFFMsg.Text = string.Empty;
                divMsg.Visible = false;
                DefaultRightRialDonation();
                btnSelf.CssClass = "activeTab";
            }
            if (!IsChina)
                lblHFFCurrencyType.Text = HLConfigManager.Configurations.DOConfiguration.HasHFFUnitDescription
                                              ? HLConfigManager.Configurations.DOConfiguration.HFFUnitDescription
                                              : HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            ClearDonation.Visible = ClearDonation2.Visible = ClearBehalfDonation2.Visible = HLConfigManager.Configurations.DOConfiguration.CanCancelDonation;

            if (ClearDonation.Visible || ClearDonation2.Visible || ClearBehalfDonation2.Visible)
            {
                if (this.ShoppingCart.Totals != null)
                {
                    OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
                    if (totals != null)
                    {
                        ClearDonation.Visible = ClearDonation2.Visible = ClearBehalfDonation2.Visible = totals.Donation > decimal.Zero;
                    }
                }
            }
        }

        /// <summary>
        ///     Validate if there is a special rule to show the HFF module.
        /// </summary>
        private void ShowModule()
        {
            divHFF.Visible = RulesManager.HLRulesManager.Manager.CanDonate(ShoppingCart);
            hffPanel.Update();
        }

        protected void tbQuantity_TextChanged(object sender, EventArgs e)
        {
            //tbQuantity.Attributes["onkeypress"] = "Numeric(event, this)";
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            decimal _selffAmount = GetDonationAmount();
            DonationClicked(_selffAmount);

            if (btnOtherAmount.Checked)
                txtOtherAmount.Enabled = true;
            if (btnBehalfOther.Checked)
                txtOtherAmount2.Enabled = true;
        }

        protected void btnAddToCart2_Click(object sender, EventArgs e)
        {
            decimal _behalfAmount = GetBehalfDonationAmount();
            DonationClicked(_behalfAmount);

            if (btnOtherAmount.Checked)
                txtOtherAmount.Enabled = true;
            if (btnBehalfOther.Checked)
                txtOtherAmount2.Enabled = true;
        }

        protected void btnClearSelfDonation_Click(object sender, EventArgs e)
        {
            OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
            if (totals != null)
            {
                if (totals.Donation > SessionInfo.CancelSelfDonation)
                    totals.Donation = totals.Donation - SessionInfo.CancelSelfDonation;
                else
                    totals.Donation = 0;
                ShoppingCart.Calculate();
                OnQuoteRetrieved(null, null);
                lblHFFMsg.Text = "";
                divMsg.Visible = true;
                ClearDonation2.Visible = false;
                SessionInfo.CancelSelfDonation = decimal.Zero;
                btn5Rmb.Checked = btn10Rmb.Checked = btnOtherAmount.Checked = false;
                txtOtherAmount.Text = string.Empty;
                txtOtherAmount.Enabled = false;
                var cart = ShoppingCart as MyHLShoppingCart;
                cart.BehalfOfSelfAmount = decimal.Zero;
                //SessionInfo.ClearStandAloneDonation();
            }
        }
        protected void btnClearBehalfDonation_Click(object sender, EventArgs e)
        {

            OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
            if (totals != null)
            {
                if (totals.Donation > SessionInfo.CancelBehalfOfDonation)
                    totals.Donation = totals.Donation - SessionInfo.CancelBehalfOfDonation;
                else
                    totals.Donation = 0;
                ShoppingCart.Calculate();
                OnQuoteRetrieved(null, null);
                lblHFFMsg.Text = "";
                divMsg.Visible = true;
                ClearBehalfDonation2.Visible = false;
            }
            SessionInfo.CancelBehalfOfDonation = decimal.Zero;
            btnBehalf5Rmb.Checked = btnBehalf10Rmb.Checked = btnBehalfOther.Checked = false;
            txtOtherAmount2.Text = txtDonatorName.Text = txtContactNumber.Text = string.Empty;
            txtOtherAmount2.Enabled = false;
            var cart = ShoppingCart as MyHLShoppingCart;
            cart.BehalfOfAmount = decimal.Zero;
          //  SessionInfo.ClearStandAloneDonation();
        }

        private void DonationClicked(decimal donationAmount)
        {
            decimal quantity = donationAmount;
            //if (int.TryParse(tbQuantity.Text.Trim(), out quantity))
            if (quantity > 0)
            {
                var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                //Add the donation amount to cart
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku))
                {
                    try
                    {
                        if (quantity > 0)
                        {
                            int quantiti = Decimal.ToInt32(quantity);
                            if (!ProductsBase.AddHFFSKU(quantiti))
                            {
                                lblHFFMsg.Text =
                                    string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                            "AddHFFFailed"));
                                divMsg.Visible = true;
                            }
                            else
                            {
                                lblHFFMsg.Text =
                                    string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                            "AddHFFSucceeded"));
                                divMsg.Visible = true;
                            }
                        }
                        //else
                        //{
                        //    lblHFFMsg.Text =
                        //        string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero"));
                        //    divMsg.Visible = true;
                        //}
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error("HFF Herbalife Can not be added to cart!\n" + ex);
                    }
                }
                else if (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                {
                    if (myShoppingCart.Totals == null)
                        myShoppingCart.Totals = new OrderTotals_V02();

                    if (myShoppingCart.Totals != null)
                    {
                        OrderTotals_V02 totals = myShoppingCart.Totals as OrderTotals_V02;
                        if (totals != null)
                        {
                            if (totals != null && totals.Donation > 0)
                                totals.Donation = totals.Donation + quantity;
                            else
                                totals.Donation = quantity;
                            myShoppingCart.Calculate();
                            OnQuoteRetrieved(null, null);

                            lblHFFMsg.Text =
                                    string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                            "AddHFFSucceeded"));
                            divMsg.Visible = true;
                            ClearDonation.Visible = true;
                            if (SessionInfo.CancelSelfDonation > 0) ClearDonation2.Visible = true;
                            else ClearDonation2.Visible = false;
                            if (SessionInfo.CancelBehalfOfDonation > 0) ClearBehalfDonation2.Visible = true;
                            else ClearBehalfDonation2.Visible = false;
                        }
                    }

                    //tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
                }
                else
                {
                    LoggerHelper.Error(string.Format("HFF SKU missing from Config for {0}.", Locale));
                }
            }
            //else
            //{
            //    //tbQuantity.Text = string.Empty;
            //    lblHFFMsg.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero");
            //    divMsg.Visible = true;
            //}
        }
        private decimal GetDonationAmount()
        {
            if (blErrors != null) blErrors.Items.Clear();
            decimal _totalDonation = decimal.Zero;
            try
            {
                // ShoppingCartID.Value = ShoppingCart.ShoppingCartID.ToString();
                //divSubmitDonation.Visible = divCancelDonation.Visible = false;

                if (btn5Rmb.Checked || btn10Rmb.Checked || btnOtherAmount.Checked)
                {
                    if (btn5Rmb.Checked)
                    {
                        _selfAmount = 5;
                    }
                    else if (btn10Rmb.Checked)
                    {
                        _selfAmount = 10;
                    }
                    else if (btnOtherAmount.Checked && !String.IsNullOrEmpty(txtOtherAmount.Text.ToString()))
                    {
                        if (decimal.TryParse(txtOtherAmount.Text.ToString(), out _otherAmount))
                        {
                            _selfAmount = _otherAmount;

                        }
                    }
                    else if (btnOtherAmount.Checked)
                    { txtOtherAmount.Enabled = true; }

                    SessionInfo.CancelSelfDonation = SessionInfo.CancelSelfDonation + _selfAmount;
                }

                SessionInfo.CancelBehalfOfDonation = _behalfAmount;
                bool isValidDonation = true;
                if (_selfAmount < 1)
                {
                    isValidDonation = false;
                    ShowError("InvalidDonationAmount");
                }

                var cart = ShoppingCart as MyHLShoppingCart;

                if (_selfAmount > 0)
                {
                    var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(cart.DistributorID, "CN");
                    if (distributorOrderingProfile.PhoneNumbers.Any())
                    {
                        var phoneNumber = distributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03 != null
                                ? distributorOrderingProfile.PhoneNumbers.Where(
                                    p => p.IsPrimary) as PhoneNumber_V03
                                : distributorOrderingProfile.PhoneNumbers.FirstOrDefault()
                                  as PhoneNumber_V03;
                        if (phoneNumber != null)
                            cart.BehalfOfContactNumber = phoneNumber.Number;
                        else
                            cart.BehalfOfContactNumber = "21-61033719";
                    }
                    cart.BehalfDonationName = ProductsBase.DistributorName;
                    cart.BehalfOfMemberId = DistributorID;

                    if (cart.BehalfOfSelfAmount > 0)
                        cart.BehalfOfSelfAmount = cart.BehalfOfSelfAmount + _selfAmount;
                    else
                        cart.BehalfOfSelfAmount = _selfAmount;

                    if (cart.BehalfOfAmount > 0)
                        cart.BehalfOfAmount = cart.BehalfOfAmount;
                }
                if ((_selfAmount > decimal.Zero) && HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                {

                    var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                    if (myShoppingCart.Totals == null)
                    {
                        myShoppingCart.Totals = new OrderTotals_V02();
                    }

                    OrderTotals_V02 totals = myShoppingCart.Totals as OrderTotals_V02;
                    if (totals == null)
                    {
                        totals = new OrderTotals_V02();
                    }
                    if (_selfAmount > decimal.Zero)
                        _totalDonation = _selfAmount;
                }
                if (errors != null && errors.Count > 0)
                {
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                    errors.Clear();
                    return 0;
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error("AddHFFFailed!\n" + ex);
            }
            return _totalDonation;
        }

        private decimal GetBehalfDonationAmount()
        {
            if (blErrors != null) blErrors.Items.Clear();
            decimal _totalDonation = decimal.Zero;

            try
            {
                _otherAmount = 0;
                string phone =
                       txtContactNumber.Text.Replace("_", String.Empty)
                                     .Replace("(", String.Empty)
                                     .Replace(")", String.Empty)
                                     .Replace("-", String.Empty)
                                     .Trim();
                if (btnBehalf5Rmb.Checked)
                {
                    _behalfAmount = 5;
                }
                else if (btnBehalf10Rmb.Checked)
                {
                    _behalfAmount = 10;
                }
                else if (!String.IsNullOrEmpty(txtOtherAmount2.Text.ToString()) && decimal.TryParse(txtOtherAmount2.Text.ToString(), out _otherAmount))
                {
                    _behalfAmount = _otherAmount;

                }
                else if (btnBehalfOther.Checked)
                { txtOtherAmount2.Enabled = true; }

                SessionInfo.CancelBehalfOfDonation = SessionInfo.CancelBehalfOfDonation+_behalfAmount;

                bool isValidDonation = true;
                if (_behalfAmount < 1)
                {
                    isValidDonation = false;
                    ShowError("InvalidDonationAmount");
                }

                var cart = ShoppingCart as MyHLShoppingCart;

                if (_behalfAmount > 0)
                {
                    if (String.IsNullOrEmpty(txtDonatorName.Text.Trim()))
                    {
                        //isValidDonation = false;
                        //ShowError("NoCustomerNameEntered");
                        txtDonatorName.Text = "匿名";
                    }

                    if (String.IsNullOrEmpty(phone) || phone.Length < 11)
                    {
                        isValidDonation = false;
                        ShowError("NoCustomerPhoneNoEntered");
                    }
                    if (!isValidDonation)
                    {
                        if (errors != null && errors.Count > 0)
                        {
                            blErrors.DataSource = errors;
                            blErrors.DataBind();
                            errors.Clear();
                            return 0;
                        }
                    }
                    cart.BehalfDonationName = txtDonatorName.Text;
                    cart.BehalfOfContactNumber = phone;
                    cart.BehalfOfMemberId = DistributorID;

                    if (cart.BehalfOfAmount > 0)
                        cart.BehalfOfAmount = cart.BehalfOfAmount + _behalfAmount;
                    else
                        cart.BehalfOfAmount = _behalfAmount;

                    if (cart.BehalfOfSelfAmount > 0)
                        cart.BehalfOfSelfAmount = cart.BehalfOfSelfAmount;
                }

                if ((_behalfAmount > decimal.Zero) && HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                {

                    var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                    if (myShoppingCart.Totals == null)
                    {
                        myShoppingCart.Totals = new OrderTotals_V02();
                    }

                    OrderTotals_V02 totals = myShoppingCart.Totals as OrderTotals_V02;
                    if (totals == null)
                    {
                        totals = new OrderTotals_V02();
                    }
                    if (_behalfAmount > decimal.Zero)
                        _totalDonation = _behalfAmount;

                }
                if (errors != null && errors.Count > 0)
                {
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                    errors.Clear();
                    return 0;
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error("AddHFFFailed!\n" + ex);
            }
            return _totalDonation;
        }

        private List<string> ShowError(string KeyName)
        {
            if (blErrors.DataSource == null && errors == null)
            {
                errors = new List<string>
                                {
                                    PlatformResources.GetGlobalResourceString("ErrorMessage", KeyName)
                                };
            }
            else
            {
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", KeyName));
            }
            return errors;
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            ShowModule();
        }

        private void HideMessage()
        {
            MyHLShoppingCart cart = (ProductsBase).ShoppingCart;
            string sku = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku;
            if (!cart.CartItems.Any(c => c.SKU == sku))
            {
                lblHFFMsg.Text = "";
            }
        }

        protected void btnSelf_Click(object sender, EventArgs e)
        {
            if (SessionInfo.CancelSelfDonation > 0) ClearDonation2.Visible = true;
            else ClearDonation2.Visible = false;

            btnBehalfOther.Checked = true;
            txtOtherAmount2.Enabled = true;
            txtOtherAmount2.Text = string.Empty;

            if (btnOtherAmount.Checked)
                txtOtherAmount.Enabled = true;

            btnSelf.CssClass = "activeTab";
            btnBehalfof.CssClass = "backward";

            MainView.ActiveViewIndex = 0;

        }

        protected void btnBehalfof_Click(object sender, EventArgs e)
        {
            if (SessionInfo.CancelBehalfOfDonation > 0) ClearBehalfDonation2.Visible = true;
            else ClearBehalfDonation2.Visible = false;

            btnOtherAmount.Checked = true;
            txtOtherAmount.Enabled = true;
            txtOtherAmount.Text = string.Empty;

            if (btnBehalfOther.Checked)
                txtOtherAmount2.Enabled = true;

            btnBehalfof.CssClass = "activeTab";
            btnSelf.CssClass = "backward";

            MainView.ActiveViewIndex = 1;

        }
        private void DefaultRightRialDonation()
        {
            btn5Rmb.Checked = btn10Rmb.Checked = btnOtherAmount.Checked = false;
            txtOtherAmount.Text = string.Empty;
            txtOtherAmount.Enabled = false;

            btnBehalf5Rmb.Checked = btnBehalf10Rmb.Checked = btnBehalfOther.Checked = false;
            txtOtherAmount2.Text = txtDonatorName.Text = txtContactNumber.Text = string.Empty;
            txtOtherAmount2.Enabled = false;
        }
        [SubscribesTo(MyHLEventTypes.OnSCancelDonationsVisible)]
        public void OnSCancelDonationsVisible(object sender, EventArgs e)
        {
            ClearBehalfDonation2.Visible = false;
            ClearDonation2.Visible = false;
            ClearDonation.Visible = false;
        }
        
    }
}