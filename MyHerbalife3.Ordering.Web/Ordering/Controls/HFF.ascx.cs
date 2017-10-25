using System;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using RulesManager = MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.Providers;
using System.Linq;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using HL.Common.Logging;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class HFF : UserControlBase
    {
        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnQuoteRetrieved;

        [SubscribesTo(MyHLEventTypes.ShoppingCartRecalculated)]
        public void OnCartRecalculated(object sender, EventArgs e)
        {
            HideMessage();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if ((null != ProductsBase.SessionInfo && ProductsBase.SessionInfo.IsEventTicketMode) || this.ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.HSO)
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
            if (HLConfigManager.Configurations.DOConfiguration.HaveNewHFF)
            {
                OldHFF.Visible = false;
            }
            else
            {
                NewHFF.Visible = false;
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
                tbQuantity.Attributes["onkeypress"] = "Numeric(event, this)";

                tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
                tbQuantity.MaxLength = HLConfigManager.Configurations.DOConfiguration.HFFSkuMaxQuantity.ToString().Length;
                
                lblHFFMsg.Text = string.Empty;
                divMsg.Visible = false;
            }

            lblHFFCurrencyType.Text = HLConfigManager.Configurations.DOConfiguration.HasHFFUnitDescription
                                          ? HLConfigManager.Configurations.DOConfiguration.HFFUnitDescription
                                          : HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            ClearDonation.Visible = ClearDonation2.Visible = HLConfigManager.Configurations.DOConfiguration.CanCancelDonation;

            if (ClearDonation.Visible || ClearDonation2.Visible)
            {
                if (this.ShoppingCart.Totals != null)
                {
                    OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
                    if (totals != null)
                    {
                        ClearDonation.Visible = ClearDonation2.Visible = totals.Donation > decimal.Zero;
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
            tbQuantity.Attributes["onkeypress"] = "Numeric(event, this)";
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.HaveNewHFF)
            {
                int Amount = GetDonationAmount();
                DonationClicked(Amount);
            }
            else
            {
                DonationClicked();
            }
        }
        private int GetDonationAmount()
        {
            int Donation = 0;

            try
            {
                if (RadioButton1.Checked)
                {
                    Donation = HLConfigManager.Configurations.DOConfiguration.HFFValueOne;
                }
                else if (RadioButton2.Checked)
                {
                    Donation = HLConfigManager.Configurations.DOConfiguration.HFFValueTwo;
                }
                else
                {
                    if (!string.IsNullOrEmpty(txtAmount.Text))
                    {
                        Donation = Convert.ToInt32(txtAmount.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("AddHFFFailed!\n" + ex);
            }
            return Donation;
        }
        private void DonationClicked(int Amount)
        {
            int quantity = Amount;
            if (quantity > 0)
            {
                var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                //Add the donation amount to cart
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku) ||
                    (HLConfigManager.Configurations.DOConfiguration.HFFSkuList != null && HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0))
                {
                    try
                    {
                        if (quantity > 0)
                        {
                            if (!ProductsBase.AddHFFSKU(quantity))
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
                        else
                        {
                            lblHFFMsg.Text =
                                string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero"));
                            divMsg.Visible = true;
                        }
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
                            totals.Donation = decimal.Parse(quantity.ToString());
                            myShoppingCart.Calculate();
                            OnQuoteRetrieved(null, null);

                            lblHFFMsg.Text =
                                    string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                            "AddHFFSucceeded"));
                            divMsg.Visible = true;
                            ClearDonation.Visible = ClearDonation2.Visible = true;
                        }
                    }

                    tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
                }
                else
                {
                    LoggerHelper.Error(string.Format("HFF SKU missing from Config for {0}.", Locale));
                }
            }
            else
            {
                tbQuantity.Text = string.Empty;
                lblHFFMsg.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero");
                divMsg.Visible = true;
            }
            RadioButton3.Checked = true;
        }

        protected void btnClearDonation_Click(object sender, EventArgs e)
        {
            OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
            if (totals != null)
            {
                if (ShoppingCart.CartItems == null || ShoppingCart.CartItems.Count == 0)
                    ShoppingCart.Totals = new OrderTotals_V02();
                else
                {
                    totals.Donation = decimal.Zero;
                    ShoppingCart.Calculate();
                }
                OnQuoteRetrieved(null, null);

                lblHFFMsg.Text = "";
                divMsg.Visible = true;
                ClearDonation.Visible = ClearDonation2.Visible = false;
            }
        }

        private void DonationClicked()
        {
            int quantity = 0;
            if (int.TryParse(tbQuantity.Text.Trim(), out quantity))
            {
                var myShoppingCart = (Page as ProductsBase).ShoppingCart;

                //Add the donation amount to cart
                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku) ||
                    (HLConfigManager.Configurations.DOConfiguration.HFFSkuList != null && HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0))
                {
                    try
                    {
                        if (quantity > 0)
                        {
                            if (!ProductsBase.AddHFFSKU(quantity))
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
                        else
                        {
                            lblHFFMsg.Text =
                                string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero"));
                            divMsg.Visible = true;
                        }
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
                            totals.Donation = decimal.Parse(quantity.ToString());
                            myShoppingCart.Calculate();
                            OnQuoteRetrieved(null, null);

                            lblHFFMsg.Text =
                                    string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                            "AddHFFSucceeded"));
                            divMsg.Visible = true;
                            ClearDonation.Visible = ClearDonation2.Visible = true;
                        }
                    }

                    tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
                }
                else
                {
                    LoggerHelper.Error(string.Format("HFF SKU missing from Config for {0}.", Locale));
                }
            }
            else
            {
                tbQuantity.Text = string.Empty;
                lblHFFMsg.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "AddHFFZero");
                divMsg.Visible = true;
            }
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {
            ShowModule();
        }

        private void HideMessage()
        {
            MyHLShoppingCart cart = (ProductsBase).ShoppingCart;
            //string sku = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku;
            string sku = "";
            if (HLConfigManager.Configurations.DOConfiguration.HFFSkuList != null && HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0)
            {
                sku = HLConfigManager.Configurations.DOConfiguration.HFFSkuList.FirstOrDefault();
            }
            else
            {
                sku = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku;
            }
            if(!cart.CartItems.Any(c=> c.SKU == sku))
            {
                lblHFFMsg.Text = "";
            }
        }
    }
}