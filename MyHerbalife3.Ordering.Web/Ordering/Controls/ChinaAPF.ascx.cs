using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class ChinaAPF : UserControlBase
    {
        private string _distributorId = string.Empty;
        private string _level = "SP";
        private string _apfSku;
        private bool _apfIsDue;
        private int _apfsInCart;
        private int _apfsDue;
        private DateTime _apfDueDate;
        private MyHLShoppingCart _cart;
        private int _quantityToAdd;
        private int _APFStatus = 4;
        private bool _APFExempted = false;

        public void Page_Load(object sender, EventArgs e)
        {
            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            _cart = (Page as ProductsBase).ShoppingCart;
            _APFExempted = APFDueProvider.IsAPFExemptOn200VP(DistributorOrderingProfile, _cart.VolumeInCart);
            Visible = APFDueProvider.ShouldShowAPFModule(DistributorID);
            
            if (DistributorOrderingProfile.IsPC || SessionInfo.IsReplacedPcOrder || ShoppingCart.OrderCategory == OrderCategoryType.ETO)
            {
                Visible = false;
                return;
            }

            _APFStatus = DistributorOrderingProfile.CNAPFStatus;

            if (_APFStatus == 2 && Visible)
            {
                    pnlAPFIsDueWithinThreeMonth.Visible = false;
                    pnlAPFPaid.Visible = false;
                    ReadFromPage();
                    GracePeriodAPFDisplay();
            }
            else //Legacy handling
            {
            Visible = APFDueProvider.ShouldShowAPFModule(DistributorID);

            if (Visible)
            {
                pnlAPFIsDueWithinThreeMonth.Visible = false;
                pnlAPFPaid.Visible = false;
                ReadFromPage();
                DisplayData();
            }
            }

        }

        public void GracePeriodAPFDisplay()
        {
            _apfSku = APFDueProvider.GetAPFSku();
            List<ShoppingCartItem_V01> item = (from c in _cart.CartItems where c.SKU == _apfSku select c).ToList();
            _APFExempted = APFDueProvider.IsAPFExemptOn200VP(DistributorOrderingProfile, _cart.VolumeInCart);

            if (_APFStatus != 2) return;

            if (_APFExempted) pnlAPFPaid.Visible = false;

            if(item.Count >= 1)
            {
                ShowApfAddedMessage();
            }
            else
            {
                var apfSku = new List<ShoppingCartItem_V01>();
                

                if (!_APFExempted)
                {
                    if (_quantityToAdd < 1) _quantityToAdd = 1;
                    apfSku.Add(new ShoppingCartItem_V01(0, APFDueProvider.GetAPFSku(), _quantityToAdd, DateTime.Now));
                    _cart.AddItemsToCart(apfSku);
        }


                item = (from c in _cart.CartItems where c.SKU == _apfSku select c).ToList();
                if (item.Count >= 1) ShowApfAddedMessage();

            }
        }

        private void ReadFromPage()
        {
            int.TryParse(txtQuantity.Text, out _quantityToAdd);
        }

        private void WriteToPage()
        {
            if (_APFStatus == 1)
            {
                if (_apfsInCart >= 1)
                    ShowApfAddedMessage();
                else
                {
                    ShowAddApf();
                }
            }
            else if (_APFStatus == 0 && DistributorOrderingProfile.ApfDueDate.AddYears(-1) <= DateTime.Today && DateTime.Today <= DistributorOrderingProfile.ApfDueDate)
            {
                if (_apfsInCart >= 1)
                    ShowApfAddedMessage();
                else
                {
                    ShowAddApf();
                }
            }
        }


        private void DisplayData()
        {
            ReadFromData();
            WriteToPage();
            string dueDateFormat = HLConfigManager.Configurations.APFConfiguration.DueDateDisplayFormat;
        }

        private void ShowApfAddedMessage()
        {
            pnlAPFPaid.Visible = true;
            pnlAPFIsDueWithinThreeMonth.Visible = false;
            lblAPFPaidMessage.Text = GetLocalResourceObject("ApfFeesHaveBeenAdded") as string;
            btnAddToCart.Visible = false;
        }

        private void ShowAddApf()
        {
            pnlAPFIsDueWithinThreeMonth.Visible = true;
            pnlAPFPaid.Visible = false;
            string dueDateFormat = HLConfigManager.Configurations.APFConfiguration.DueDateDisplayFormat;
            tblEditable.Style.Remove(HtmlTextWriterStyle.Display);
            lblAPFType.Text = GetLocalResourceObject("APFSKU_" + _apfSku) as string;
            CatalogItem item = CatalogProvider.GetCatalogItem(_apfSku, CountryCode);
            if (null != item)
            {
                lblAPFAmount.Text = string.Concat(HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                                                  " ", Math.Round(item.ListPrice,2));
            }
            int numApfs = (_apfIsDue) ? (_apfsDue - _apfsInCart) : 1;
            txtQuantity.Text = numApfs.ToString();

            //txtQuantity.ReadOnly = numApfs == 1 || !APFDueProvider.CanRemoveAPF(_distributorId, Locale, _level);

            txtQuantity.ReadOnly = true;
            btnAddToCart.Visible = true;
            if (CatalogProvider.IsPreordering(ShoppingCart.CartItems, (ShoppingCart.DeliveryInfo != null ? ShoppingCart.DeliveryInfo.WarehouseCode : string.Empty)))
            {
                lblAPFMessage.Text = String.Format(
                               PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                         "PreOrderingSku"));
                lblAPFMessage.ForeColor = Color.FromArgb(240, 56, 56);
                btnAddToCart.Visible = false;
            }
            else
            {
                
                    lblAPFMessage.Text =
                        string.Format(
                            PlatformResources.GetGlobalResourceString("ErrorMessage", "APFDueDateNotification"),
                            _apfDueDate.ToLongDateString()); // this will produce correct format because by defaut
                    lblAPFMessage.ForeColor = Color.FromArgb(240, 56, 56);
                
            }
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (_APFStatus == 1) // due in 3 months
            {
                if (_apfsInCart >= 1)
                {
                    DisplayData();
                    return;
                }
            }


            MyHLShoppingCart cart = (Page as ProductsBase).ShoppingCart;

            //Add the APF amount to cart
            if (HLConfigManager.Configurations.APFConfiguration.DistributorSku != null)
            {
                try
                {
                    if (_quantityToAdd > 0)
                    {
                        var apfSku = new List<ShoppingCartItem_V01>();
                        apfSku.Add(new ShoppingCartItem_V01(0, APFDueProvider.GetAPFSku(), _quantityToAdd,
                                                            DateTime.Now));

                        if(ShoppingCart.DeliveryInfo == null)
                        {
                            lblAPFMessage.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                            "CantVisitWithNoDeliveryInfo");
                            return;
                        }

                        if (CatalogProvider.IsPreordering(ShoppingCart.CartItems,
                                                          ShoppingCart.DeliveryInfo.WarehouseCode))
                        {
                            lblAPFMessage.Text = String.Format(
                                PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                          "PreOrderingSku"));
                        }
                        else
                        {
                            ProductsBase.AddItemsToCart(apfSku);
                        }

                        // Verify if any validation rule fails to show the error in the APF module
                        IEnumerable<string> ruleResultMessages =
                            from r in ShoppingCart.RuleResults
                            where r.Result == RulesResult.Failure
                            select r.Messages[0];
                        if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                        {
                            lblError.Text = ruleResultMessages.FirstOrDefault();
                            lblError.Visible = true;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error("APF Could not be added to cart!\n" + ex);
                }

                DisplayData();
            }
        }


        private void ReadFromData()
        {
            DistributorOrderingProfile distributorOrderingProfile =
                DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode);
            _distributorId = DistributorID;
            _apfDueDate = distributorOrderingProfile.ApfDueDate;
            _apfSku = APFDueProvider.GetAPFSku();
            _cart = (Page as ProductsBase).ShoppingCart;
            _apfIsDue = APFDueProvider.IsAPFDueAndNotPaid(_distributorId);
            if (_apfIsDue)
            {
                _apfsDue = APFDueProvider.APFQuantityDue(_distributorId, HLConfigManager.Configurations.Locale);
            }
            List<ShoppingCartItem_V01> item = (from c in _cart.CartItems where c.SKU == _apfSku select c).ToList();
            _apfsInCart = 0;
            if (item.Count > 0)
            {
                _apfsInCart = item[0].Quantity;
            }
        }

        protected string GetHeading()
        {
            return GetLocalResourceObject("APF") as string;
        }

        [SubscribesTo(MyHLEventTypes.ShoppingCartChanged)]
        public void UpdateFromCart(object sender, EventArgs e)
        {
            DisplayData();
            if (_APFStatus == 2) GracePeriodAPFDisplay();
        }

    }
}