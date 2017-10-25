using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.UI;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class APF : UserControlBase
    {
        private string _distributorId = string.Empty;
        private string _level = "SP";
        private string _apfSku;
        private bool _apfIsDue;
        private bool _apfDueWithinOneYear;
        private bool _apfDueGreaterThanOneYear;
        private int _apfsInCart;
        private int _apfsDue;
        private DateTime _apfDueDate;
        private MyHLShoppingCart _cart;
        private int _quantityToAdd;
        private bool _testing;

        protected void Page_Load(object sender, EventArgs e)
        {
            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            _testing = !string.IsNullOrEmpty(Request["testAPF"]);
            _cart = (Page as ProductsBase).ShoppingCart;
            //TaskID: 9016 fix
            Visible = _testing |
                      (APFDueProvider.ShouldShowAPFModule(DistributorID,Thread.CurrentThread.CurrentCulture.Name.Substring(3)) &&
                       (null != _cart && _cart.OrderCategory == OrderCategoryType.RSO));
            if (DistributorOrderingProfile.IsPC)
            {
                Visible = false;
            }
            if (_cart.OrderCategory == OrderCategoryType.HSO)
            {
                Visible = false;
            }
            SetPanelsVisibility();
            if (Visible)
            {
                if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID,this.CountryCode))
                {
                    ReadFromData();
                    RefreshVisibility(null, null);
                }

                pnlAPFIsDueWithinYear.Visible = false;
                pnlAPFPaid.Visible = false;
                pnlTesting.Visible = _testing;

                //if (IsPostBack)
                //{
                ReadFromPage();
                //}
                //else
                //{
                DisplayData();
                //}
            }
        }

        private void ReadFromPage()
        {
            int.TryParse(txtQuantity.Text, out _quantityToAdd);
        }

        private void WriteToPage()
        {
            if (!HLConfigManager.Configurations.APFConfiguration.ApfExemptCountriesOfProcessing.Contains(APFDueProvider.GetProcessCountry()) 
                && APFDueProvider.CanPurchaseApf(APFDueProvider.GetProcessCountry(), CountryCode, HLConfigManager.Configurations.APFConfiguration.ApfRestrictedByPurchaseLocation))
            {
                if (_apfDueWithinOneYear || _apfIsDue || _apfDueGreaterThanOneYear) // && _level == "DS"))
                {
                    if ((_apfsDue - _apfsInCart) > 0 || (_apfsDue == 0 && _apfsInCart == 0))
                    {
                        ShowAddApf();
                    }
                    else
                    {
                        ShowApfAddedMessage();
                    }
                }
                else
                {
                    if (_apfIsDue && (_apfsDue == _apfsInCart))
                    {
                        ShowApfAddedMessage();
                    }
                }
            }
            
        }

        private void DisplayData()
        {
            ReadFromData();
            WriteToPage();
            string dueDateFormat = HLConfigManager.Configurations.APFConfiguration.DueDateDisplayFormat;
            txtApfDate.Text = APFDueProvider.GetAPFDueDate(DistributorID, CountryCode).ToString(dueDateFormat,
                                                                                                  Thread.CurrentThread
                                                                                                        .CurrentCulture);
        }

        private void ShowApfAddedMessage()
        {
            pnlAPFPaid.Visible = true;
            pnlAPFIsDueWithinYear.Visible = false;
            lblAPFPaidMessage.Text = GetLocalResourceObject("ApfFeesHaveBeenAdded") as string;
            btnAddToCart.Visible = false;
        }

        private void ShowAddApf()
        {
            pnlAPFIsDueWithinYear.Visible = true;
            SetPanelsVisibility();
            pnlAPFPaid.Visible = false;
            // Removing the condition, in order to get the lblAPFMessage text even if the _apfIsDue field is true.
            // Defect 42710.
            //if (!_apfIsDue)
            //{
            string dueDateFormat = HLConfigManager.Configurations.APFConfiguration.DueDateDisplayFormat;
            lblAPFMessage.Text =
                string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "APFDueDateNotification"),
                              _apfDueDate.ToString(dueDateFormat, Thread.CurrentThread.CurrentCulture));
            //}
            if (_apfDueGreaterThanOneYear)
            {
                tblEditable.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            else
            {
                tblEditable.Style.Remove(HtmlTextWriterStyle.Display);
            }
            lblAPFType.Text = GetLocalResourceObject("APFSKU_" + _apfSku) as string;
            CatalogItem item = CatalogProvider.GetCatalogItem(_apfSku, CountryCode);
            if (null != item)
            {
                lblAPFAmount.Text = string.Concat(HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol,
                                                  " ", item.ListPrice);
            }
            int numApfs = (_apfIsDue) ? (_apfsDue - _apfsInCart) : 1;
            txtQuantity.Text = numApfs.ToString();

            txtQuantity.ReadOnly = numApfs == 1 || !APFDueProvider.CanRemoveAPF(_distributorId, Locale, _level);

            if (_apfIsDue && _level == "SP")
            {
                txtQuantity.ReadOnly = true;
            }

            btnAddToCart.Visible = true;
        }

        protected void btnAddToCart_Click(object sender, EventArgs e)
        {
            if (_apfDueWithinOneYear)
            {
                if (_quantityToAdd != 1)
                {
                    DisplayData();
                    return;
                }
            }
            else
            {
                if (_quantityToAdd == 0 || _quantityToAdd > (_apfsDue - _apfsInCart))
                {
                    if (_apfsDue - _apfsInCart == 1)
                    {
                        lblError.Text = GetLocalResourceObject("QuantityMustBeOne") as string;
                    }
                    else
                    {
                        lblError.Text = string.Format((GetLocalResourceObject("InvalidQuantity") as string), 1,
                                                      (_apfsDue - _apfsInCart));
                    }
                    lblError.Visible = true;
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
                        if (Locale == "en-IN" && cart.CartItems.Count > 0)
                        {
                            lblError.Text = GetLocalResourceObject("AAFWithOtherProducts") as string;
                            lblError.Visible = true;
                            return;
                        }
                        else
                            ProductsBase.AddItemsToCart(apfSku);

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

        protected void SetAPFDueDate(object sender, EventArgs e)
        {
            DateTime dueDate = DateTime.MinValue;
            try
            {
                if (DateTime.TryParse(txtDueDate.Text, out dueDate))
                {
                    DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode);
                    if (null != distributorOrderingProfile)
                    {
                        distributorOrderingProfile.ApfDueDate = dueDate;
                        // TODO : this is not necessary?
                        //DistributorProvider.UpdateDistributor(distributor);
                        lblStatus.Text = "Set to: " + dueDate.ToString("M/d/yyyy", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        lblStatus.Text = "Distributor error";
                    }
                }
                else
                {
                    lblStatus.Text = "Date error";
                }
            }
            catch
            {
                lblStatus.Text = "Server Error";
            }

            DisplayData();
        }

        private void ReadFromData()
        {
            DistributorOrderingProfile distributorOrderingProfile =
                DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode);
            _distributorId = DistributorID;
            _apfDueDate = distributorOrderingProfile.ApfDueDate;
            if (_testing)
            {
                APFDueProvider.UpdateAPFDuePaid(_distributorId, _apfDueDate);
            }
            _apfSku = APFDueProvider.GetAPFSku();
            _cart = (Page as ProductsBase).ShoppingCart;
            _apfIsDue = APFDueProvider.IsAPFDueAndNotPaid(_distributorId, HLConfigManager.Configurations.Locale);
            _apfDueWithinOneYear = APFDueProvider.IsAPFDueWithinOneYear(_distributorId, CountryCode);
            _apfDueGreaterThanOneYear = APFDueProvider.IsAPFDueGreaterThanOneYear(_distributorId, CountryCode);
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
        }

        [SubscribesTo(MyHLEventTypes.OrderSubTypeChanged)]
        public void RefreshVisibility(object sender, EventArgs e)
        {
            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
            {
                if (null != _cart)
                {
                    PurchasingLimits_V01 limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
                    if (null != limits)
                    {
                        if (limits.PurchaseType == OrderPurchaseType.Consignment)
                        {
                            pnlAPFIsDueWithinYear.Visible = false;
                            pnlAPFPaid.Visible = false;
                        }
                        SetPanelsVisibility();
                    }
                }
            }
        }

        private void SetPanelsVisibility()
        {
            if (Visible)
            {
                //Begin HD Ticket 406707
                if (_cart.OrderSubType == "A1" || _cart.OrderSubType == "B1")
                {
                    pnlAPFIsDueWithinYear.Visible = false;
                }
                else if (_cart.OrderSubType == "A2" || _cart.OrderSubType == "B2")
                {
                    if (!HLConfigManager.Configurations.APFConfiguration.ApfExemptCountriesOfProcessing.Contains(APFDueProvider.GetProcessCountry()) 
                        && APFDueProvider.CanPurchaseApf(APFDueProvider.GetProcessCountry(), CountryCode, HLConfigManager.Configurations.APFConfiguration.ApfRestrictedByPurchaseLocation))
                    {
                        pnlAPFIsDueWithinYear.Visible = true;
                    }
                    
                }
                //End HD Ticket 406707
            }
        }
    }
}