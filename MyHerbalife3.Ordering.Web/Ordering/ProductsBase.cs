using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.Services;
using System.Web.UI.HtmlControls;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;
using Resources;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using System.Text;
using MyHerbalife3.Ordering.Providers.FOP;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using CatalogProvider = MyHerbalife3.Ordering.Providers.CatalogProvider;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using System.Web.Caching;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    /// <summary>
    ///     The products base.
    /// </summary>
    public class ProductsBase : PageBase
    {
        #region Events

        [Publishes(MyHLEventTypes.PageVisitRefused)]
        public event EventHandler PageRedirected;

        [Publishes(MyHLEventTypes.ProceedingToCheckoutFromMiniCart)]
        public event EventHandler ProceedingToCheckoutFromMiniCart;

        [Publishes(MyHLEventTypes.ShoppingCartChanged)]
        public event EventHandler ShoppingCartChanged;

        [Publishes(MyHLEventTypes.APFChanged)]
        public event EventHandler APFCartChanged;

        [Publishes(MyHLEventTypes.ProductAvailabilityTypeChanged)]
        public event EventHandler ProductAvailabilityTypeChanged;

        [SubscribesTo(MyHLEventTypes.PickUpNicknameNotSelectedInMiniCart)]
        public void OnPickUpNicknameNotSelectedInMiniCart(object sender, EventArgs e)
        {
            PickUpLocationNotSelected = true;
        }

        #endregion Events

        #region Constants

        protected const string DupCheckKey = "DUPCHECKWITHREFID_STATUS";
        /// <summary>
        ///     The decline casa session key.
        /// </summary>
        private const string DECLINE_CASA_SESSION_KEY = "Decline_Casa";

        private const string DS = "DS";
        private const string SC = "SC";
        private const string PQP = "PQP";
        private const string QP = "QP";
        private const string FiveQS = "5QS";

        public const string AttemptLoadSerializedData_Key = "AttemptLoadSerializedData";
        public const string AttemptLoadSerializedData_Value_Yes = "1";

        private const int Honors2016EventId = 2462;
        #endregion Constants

        #region Properties

        public const string ViewStateNoDeliveryOptionInfo = "NoDeliveryOptionInfo";
        public static List<string> MLMAllowedSubtypes = new List<string>(Settings.GetRequiredAppSetting("MLMAllowedSubtypes").Split(new char[] { ',' }));

        /// <summary>
        ///     The _all skus.
        /// </summary>
        private Dictionary<string, SKU_V01> _allSKUS;

        /// <summary>
        ///     The _order month.
        /// </summary>
        private OrderMonth _orderMonth;

        /// <summary>
        ///     The _product info catalog.
        /// </summary>
        private ProductInfoCatalog_V01 _productInfoCatalog;

        private SessionInfo _sessionInfo;

        /// <summary>
        ///     The _shopping cart.
        /// </summary>
        private MyHLShoppingCart _shoppingCart;

        public bool PickUpLocationNotSelected { get; set; }

        /// <summary>
        ///     Gets or sets Locale.
        /// </summary>
        public new string Locale { get; set; }

        /// <summary>
        ///     Gets or sets DistributorID.
        /// </summary>
        public new string DistributorID { get; set; }

        /// <summary>
        ///     Gets or sets CountryCode.
        /// </summary>
        public new string CountryCode { get; set; }

        /// <summary>
        ///     Gets or sets Distritutor Name.
        /// </summary>
        public new string DistributorName { get; set; }

        /// <summary>
        ///     Gets or sets Level.
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        ///     Gets or sets LevelSubType.
        /// </summary>
        public string LevelSubType { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether CantBuy.
        /// </summary>
        public bool CantBuy { get; set; }

        /// <summary>
        ///     Is DS blocked by his Sponsor
        /// </summary>
        public virtual bool CantBuyBlockedBySponsor { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether HardCash.
        /// </summary>
        public bool HardCash { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether Deleted.
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        ///     Gets or sets DistributorDiscount.
        /// </summary>
        public decimal DistributorDiscount { get; set; }

        /// <summary>
        ///     Gets or sets CurrencyCode.
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        ///     Gets or sets OrderNumber.
        /// </summary>
        public string OrderNumber { get; set; }

        /// <summary>
        ///     Gets or sets OrderAmount.
        /// </summary>
        //public double OrderAmount { get; set; }

        /// <summary>
        ///     Gets or sets PrimaryEmail.
        /// </summary>
        public string PrimaryEmail { get; set; }

        /// <summary>
        ///     Gets or sets PaymentGatewayRedirectUrl.
        /// </summary>
        public string PaymentGatewayRedirectUrl { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether TodaysMagazine.
        /// </summary>
        public bool TodaysMagazine { get; set; }

        public bool AllowDecimal { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether DisableSaveAddressCheckbox.
        /// </summary>
        public bool DisableSaveAddressCheckbox { get; set; }

        /// <summary>
        ///     Gets or sets OrderPreferences submenus
        /// </summary>
        public bool ShowOrderPreferenceSubmenu { get; set; }

        /// <summary>
        ///     Gets or sets IsEventTicketMode
        /// </summary>
        public bool IsEventTicketMode { get; set; }

        /// <summary>
        ///     Gets or sets IsHAPMode
        /// </summary>
        public bool IsHAPMode { get; set; }

        /// <summary>
        ///     Whether this page requires Cart Management Rules to be applied to the cart before showing
        /// </summary>
        public bool RequiresCartManagementRules { get; set; }

        /// <summary> indicates when the static distributor discount is not been retrived by service</summary>
        public bool DistributorError { get; set; }
        public bool NotAllowed { get; set; }
        public bool NotAllowedForPM { get; set; }

        /// <summary>
        /// Distributor Ordering Profile
        /// </summary>
        private DistributorOrderingProfile _distributorOrderingProfile;
        public DistributorOrderingProfile DistributorOrderingProfile
        {
            get
            {
                return _distributorOrderingProfile ??
                       (_distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(DistributorID, CountryCode));
            }

            set
            {
                _distributorOrderingProfile = value;
            }
        }


        /// <summary>
        ///     Gets or sets ShoppingCart.
        /// </summary>
        public MyHLShoppingCart ShoppingCart
        {
            get
            {
                if (IsChina && SessionInfo.UseSessionLessInfo) return SessionInfo.SessionLessInfo.ShoppingCart;

                if (_shoppingCart == null)
                {
                    int  cartID = 0;
                    if (!string.IsNullOrEmpty(Request.QueryString["CartID"]) && Request.QueryString["CartID"].ToLower() != "null")
                    {
                        if(!Int32.TryParse(Request.QueryString["CartID"], out cartID))
                        {
                            cartID = 0;
                        }
                       
                    }

                    _shoppingCart = cartID == 0
                                        ? ShoppingCartProvider.GetShoppingCart(DistributorID, Locale)
                                        : ShoppingCartProvider.GetShoppingCart(DistributorID, Locale, cartID);

                    //Validation for CartIDs > 0 that return a null cart
                    if (_shoppingCart == null && cartID > 0)
                    {
                        _shoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale);
                    }
                    if (cartID > 0 && !string.IsNullOrEmpty(Request.QueryString["CartID"]) && Request.QueryString["CartID"].ToLower() != "null")
                    {
                        ClearShoppingCartModuleCache();
                        SetShoppingCartModuleCache(_shoppingCart);
                    }

                    if(IsChina && _shoppingCart != null)
                    {
                        _shoppingCart.ResetPCLearningPoint(); 
                    }
                }
                return _shoppingCart;
            }

            set { _shoppingCart = value; }
        }

        private void SetShoppingCartModuleCache(MyHLShoppingCart _shoppingCart)
        {
            var cartWidgetSource = new CartWidgetSource();
            cartWidgetSource.SetCartWidgetCache(_shoppingCart);
        }

        private void ClearShoppingCartModuleCache()
        {
            try
            {
                var cartWidgetSource = new CartWidgetSource();
                cartWidgetSource.ExpireShoppingCartCache(DistributorID, Locale);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                       string.Format("Error occurred ClearMyHL3ShoppingCartCache. Id is {0}-{1}.\r\n{2}", DistributorID, Locale,
                                     ex.Message));
            }
        }

        /// <summary>
        ///     Gets or sets ProductInfoCatalog.
        /// </summary>
        /// <exception cref="ApplicationException">
        /// </exception>
        public ProductInfoCatalog_V01 ProductInfoCatalog
        {
            get
            {
                if (_productInfoCatalog == null)
                {
                    _productInfoCatalog = CatalogProvider.GetProductInfoCatalog(Locale, CurrentWarehouse);
                }

                if (_productInfoCatalog == null)
                {
                    Response.Redirect("~/Home/Default");
                    throw new ApplicationException(
                        string.Format("Unable to retrieve Product Info Catalog, redirecting to Home. Locale: {0}", Locale));

                }

                return _productInfoCatalog;
            }

            set { _productInfoCatalog = value; }
        }

        /// <summary>
        ///     Gets or sets AllSKUS.
        /// </summary>
        public Dictionary<string, SKU_V01> AllSKUS
        {
            get
            {
                if (_allSKUS == null)
                {
                    _allSKUS = CatalogProvider.GetAllSKU(Locale, CurrentWarehouse);
                    }

                return _allSKUS;
            }

            set { _allSKUS = value; }
        }

        /// <summary>
        ///     Gets or sets OrderMonth.
        /// </summary>
        public OrderMonth OrderMonth
        {
            get
            {
                if (_orderMonth == null)
                {
                    _orderMonth = new OrderMonth(CountryCode);
                }

                return _orderMonth;
            }

            set { _orderMonth = value; }
        }

        /// <summary>
        ///     Gets CurrentWarehouse.
        /// </summary>
        public string CurrentWarehouse
        {
            get
            {
                if (_shoppingCart == null || _shoppingCart.DeliveryInfo == null ||
                    string.IsNullOrEmpty(_shoppingCart.DeliveryInfo.WarehouseCode))
                {
                    return HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                }
                if (
                    _shoppingCart.DeliveryInfo.WarehouseCode.Equals(
                        HLConfigManager.Configurations.APFConfiguration.APFwarehouse))
                {
                    if (_shoppingCart != null && _shoppingCart.ShoppingCartItems != null &&
                        APFDueProvider.containsOnlyAPFSku(_shoppingCart.ShoppingCartItems))
                    {
                        return string.IsNullOrEmpty(_shoppingCart.ActualWarehouseCode)
                                   ? HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse
                                   : _shoppingCart.ActualWarehouseCode;
                    }
                }
                return _shoppingCart.DeliveryInfo.WarehouseCode;
            }
        }

        /// <summary>
        ///     Gets DeliveryOptionID.
        /// </summary>
        public int DeliveryOptionID
        {
            get
            {
                if (_shoppingCart == null || _shoppingCart.DeliveryInfo == null)
                {
                    return 0;
                }

                return _shoppingCart.DeliveryInfo.Id;
            }
        }

        /// <summary>
        ///     Gets DeliveryOptionID.
        /// </summary>
        public ServiceProvider.ShippingSvc.DeliveryOptionType OptionType
        {
            get
            {
                if (_shoppingCart == null || _shoppingCart.DeliveryInfo == null)
                {
                    return ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                }

                return _shoppingCart.DeliveryInfo.Option;
            }
        }

        /// <summary>
        ///     Gets ShippingAddresssID.
        /// </summary>
        public int ShippingAddresssID
        {
            get
            {
                if (_shoppingCart == null || _shoppingCart.DeliveryInfo == null)
                {
                    return 0;
                }

                return _shoppingCart.DeliveryInfo.Address != null ? _shoppingCart.DeliveryInfo.Address.ID : 0;
            }
        }

        /// <summary>
        ///     Gets CurrentFreight.
        /// </summary>
        public string CurrentFreight
        {
            get
            {
                if (_shoppingCart == null || _shoppingCart.DeliveryInfo == null ||
                    string.IsNullOrEmpty(_shoppingCart.DeliveryInfo.FreightCode))
                {
                    return HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultFreightCode;
                }

                return _shoppingCart.DeliveryInfo.FreightCode;
            }
        }

        /// <summary>
        ///     Gets CurrentFreight.
        /// </summary>
        public string CurrentMonthVolume
        {
            get
            {

                string volumePoints = "0.00";

                if (null != _distributorOrderingProfile)
                {
                    //Get the current Order Month volume
                    DistributorVolume_V01 volume = null;
                    if (null != _distributorOrderingProfile.DistributorVolumes)
                    {
                        volume =
                            _distributorOrderingProfile.DistributorVolumes.Find(
                                v => v.VolumeDate.Month == OrderMonth.CurrentOrderMonth.Month);
                    }

                    //If we can't get that volume, get the most recent one available.
                    if (null == volume)
                    {
                        if (null != _distributorOrderingProfile.DistributorVolumes && _distributorOrderingProfile.DistributorVolumes.Count > 0)
                        {
                            volume = _distributorOrderingProfile.DistributorVolumes.OrderByDescending(v => v.VolumeDate).First();
                        }
                    }

                    if (null != volume)
                    {
                        if (Level != "SP")
                        {
                            volumePoints = GetVolumePointsFormat(volume.PersonallyPurchasedVolume + volume.DownlineVolume);
                        }
                        else
                        {
                            volumePoints = GetVolumePointsFormat(volume.Volume + volume.GroupVolume);
                        }
                    }
                }

                _shoppingCart.EmailValues.CurrentMonthVolume = volumePoints;
                return volumePoints;
            }
        }

        public SessionInfo SessionInfo
        {
            get
            {
                if (_sessionInfo == null)
                    return _sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
                return _sessionInfo;
            }
            set
            {
                _sessionInfo = value;
                SessionInfo.SetSessionInfo(DistributorID, Locale, _sessionInfo);
            }
        }

        public bool ShowAllInventory { get; set; }

        public bool HasNotYetTakenTraining { get; set; }

        public bool HasNotYetTakenTrainingAfterElapsedDays { get; set; }

        public bool DisplayCCMessage { get; set; }

        #region Page Visit Denial properties

        /// <summary>Whether page is not accessible due to CantBuy DS</summary>
        public bool CantBuyCantVisit { get; set; }

        /// <summary>Whether page is not accessible due to HardCash DS</summary>
        public bool HardCashCantVisit { get; set; }

        /// <summary>Whether page is not accessible due to Incomplete Cart DeliveryInfo</summary>
        public bool CantVisitWithoutDeliveryInfo { get; set; }

        /// <summary>Whether page is not accessible due to Saved Credit Card restrictions</summary>
        public bool CantVisitSavedCardsNotAllowed { get; set; }

        /// <summary>Whether page is not accessible due to Empty</summary>
        public bool CantVisitWithEmptyCart { get; set; }

        /// <summary>Whether page is not accessible due to Exceeded Purchasing Limits</summary>
        public bool CantVisitWithExceededPurchasingLimits { get; set; }

        /// <summary>whether page is not accessible due to DS fraud./// </summary>
        public bool CantVisitWithDSFraud { get; set; }

        /// <summary>whether page is not accessible if unable to price./// </summary>
        public bool CantVisitWithNoTotals { get; set; }

        /// <summary>whether page is not accessible if unable to price./// </summary>
        public bool CantVisitWithoutOrderNumber { get; set; }

        /// <summary>whether page is not accessible if unable to price./// </summary>
        public bool CantDeleteFinalAddress { get; set; }

        /// <summary>whether checkout options are editable or static./// </summary>
        public bool CheckoutOptionsAreStatic { get; set; }

        /// <summary>whether page is not accesible to manage saved carts</summary>
        public bool CantVisitSavedCartsNotAllowed { get; set; }

        #endregion Page Visit Denial properties

        /// <summary>
        ///     The shipping info not filled.
        /// </summary>
        [Publishes(MyHLEventTypes.ShippingInfoNotFilled)]
        public event EventHandler ShippingInfoNotFilled;

        private List<Order_V02> _activeHapOrders;

        public List<Order_V02> ActiveHAPOrders
        {
            get
            {
                return OrderProvider.GetActiveHAPOrders(this.DistributorID,this.Locale, this.CountryCode);
            }
            set { _activeHapOrders = value; }
        }

        #endregion Properties

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductsBase" /> class.
        /// </summary>
        public ProductsBase()
        {
            ShowOrderPreferenceSubmenu = false;
            Locale = CultureInfo.CurrentCulture.Name;
            CountryCode = Locale.Substring(3, 2);

            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var user = member.Value;

            DistributorID = user.Id;
            DistributorName = user.FullEnglishName();

            DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfile;
            DistributorDiscount = distributorOrderingProfile.StaticDiscount;

            try
            {
                DistributorDiscount = DistributorOrderingProfile.StaticDiscount;
                DistributorError = false;
            }
            catch (Exception ex)
            {
                DistributorError = true;
                DistributorDiscount = 0m;
                LoggerHelper.Error(string.Format("Static Discount error {0}", ex));
            }

            LevelSubType = (user.SubTypeCode ?? "").ToUpper();
            Level = HLConfigManager.Configurations.DOConfiguration.IsChina ? "SP" : user.TypeCode;

            PrimaryEmail = user.PrimaryEmail;
            ShowAllInventory = DistributorOrderingProfile.ShowAllInventory;

            CantBuy = distributorOrderingProfile.CantBuy;
            if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false) && !CantBuy && HLConfigManager.Configurations.DOConfiguration.EnforcesPurchasingPermissions)
            {
                CantBuy = !HLRulesManager.Manager.CanPurchase(DistributorID, CountryCode);
            }


            if (HLConfigManager.Configurations.DOConfiguration.HasMLMCheck && Level == "SP" && !distributorOrderingProfile.TrainingFlag && !CantBuy)
            {
                TimeSpan dateDifference = new TimeSpan();
                var regionInfo = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
                DateTime currentLoggedInDate = DateUtils.GetCurrentLocalTime(regionInfo);
                if (distributorOrderingProfile.SPQualificationDate != null &&
                    distributorOrderingProfile.SPQualificationDate > new DateTime(2014, 02, 01))
                {
                    DateTime spQualificationDate = (DateTime)distributorOrderingProfile.SPQualificationDate;
                    dateDifference = currentLoggedInDate.Subtract(spQualificationDate);


                    int daysDifference = (int)dateDifference.TotalDays;
                    if (daysDifference > 60)
                    {
                        //Scenario-2
                        HasNotYetTakenTrainingAfterElapsedDays = true;
                    }
                    else
                    {
                        //Scenario-1
                        HasNotYetTakenTraining = true;
                    }

                }
            }

            HardCash = distributorOrderingProfile.HardCashOnly;

            Deleted = user.DistributorStatus == "Deleted";

            if (user.IsIncomplete)
            {
                HttpContext.Current.Response.Redirect("~/Home/Default");
                HttpContext.Current.Response.End();
            }
            TodaysMagazine = distributorOrderingProfile.TodaysMagazine;

            //  CantVisitWithoutDeliveryInfo = (ShoppingCart != null && ShoppingCart.DeliveryInfo != null);

            DisableSaveAddressCheckbox = false;

            AllowDecimal = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal;


        }
        /// <summary>Determines if a card has expired</summary>
        /// <param name="exp">The Expiration date of the card</param>
        /// <returns>true if expired</returns>
        protected bool isExpires(DateTime exp, string CountryCode)
        {
            var now = DateUtils.GetCurrentLocalTime(CountryCode);
            var lastDate = DateTimeUtils.GetFirstDayOfMonth(now);
            if (exp == DateTime.MinValue)
            {
                return false;
            }
            return exp <= lastDate;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether UseHmsCalc.
        /// </summary>
        public bool UseHmsCalc { get; set; }

        /// <summary>
        ///     The on init.
        /// </summary>
        /// <param name="e">
        ///     The e.
        /// </param>
        protected override void OnInit(EventArgs e)
        {
            var RegisteredCC = PaymentInfoProvider.GetPaymentInfo(DistributorID, Locale);
            DisplayCCMessage = true;
            if (null != RegisteredCC && RegisteredCC.Count > 0)
            {

                if (RegisteredCC.Any(pi => !isExpires(pi.Expiration, CountryCode)))
                {
                    DisplayCCMessage = false;
                }

            }
            if (Session.IsNewSession && IsPostBack)
            {
                Response.Redirect(GetRequestURLWithOutPort());
            }

            if (HLConfigManager.Configurations.DOConfiguration.RedirectToShop)
            {
                var DistributorType = DistributorOrderingProfileProvider.CheckDsLevelType(DistributorID, CountryCode);
                if (DistributorType == Scheme.Member)
                {
                    string shopURL_MB = string.Empty;
                    if (HttpContext.Current.Request.RawUrl.ToLower().Contains("invoice"))
                    {
                        shopURL_MB = HLConfigManager.Configurations.DOConfiguration.ShopUrlForMBInvoice;
                    }
                    else
                    {
                        shopURL_MB = HLConfigManager.Configurations.DOConfiguration.ShopUrlForMB;
                    }
                    
                    if (!string.IsNullOrEmpty(shopURL_MB))
                    {
                        Response.Redirect(shopURL_MB);
                    }
                }
                else
                {
                    string shopURL_DS = string.Empty;
                    if (HttpContext.Current.Request.RawUrl.ToLower().Contains("invoice"))
                    {
                        shopURL_DS = HLConfigManager.Configurations.DOConfiguration.ShopUrlForDSInvoice;
                    }
                    else
                    {
                        shopURL_DS = HLConfigManager.Configurations.DOConfiguration.ShopUrlForDS;
                    }
                    
                    if (!string.IsNullOrEmpty(shopURL_DS))
                    {
                        Response.Redirect(shopURL_DS);
                    }
                }
            }

            // Can they get in?
            var global = (Context.ApplicationInstance as Global);

            var allowDo = Settings.GetRequiredAppSetting("allowDO", true);
            //Putting All DO in maintenance
            if (!allowDo)
            {
                Response.Redirect("~/Ordering/Error.aspx");
            }
            else
            {
                if (!HLConfigManager.Configurations.DOConfiguration.AllowDO ||
                    HLConfigManager.Configurations.DOConfiguration.InMaintenance ||
                    HLConfigManager.DefaultConfiguration.DOConfiguration.InMaintenance)
                {
                    Response.Redirect("~/Ordering/Error.aspx");
                }
            }

            // Check the static discount to let know the user is an error occurs
            if (DistributorError)
            {
                (this.Master as OrderingMaster).Status.AddMessage(StatusMessageType.Error,
                    PlatformResources.GetGlobalResourceString("ErrorMessage", "StaticDiscountError"));
            }

            if (!HLConfigManager.Configurations.DOConfiguration.AllowAllDistributor)
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                var user = member.Value;

                //bool isTabOrAbove = false;

                //TODO: tabTeam group
                //if (user != null)
                //{
                //    isTabOrAbove = (user.IsChairmanClubMember ||
                //                    (user.TeamLevelName != null &&
                //                     user.TabTeam.TabTeamGroup > TabTeamGroupType.World));
                //}
                //if (isTabOrAbove == false)
                //{
                //    Response.Redirect("~/Ordering/Error.aspx?AllowAll=N");
                //}
            }

            //if (Session["CustomerOrderNumber"] != null || !string.IsNullOrEmpty(Request.QueryString["cid"]))
            //{
            //    ShippingProvider.GetShippingProvider(this.CountryCode).GetShippingAddresses(
            //        this.DistributorID, this.Locale);
            //}

            // get/save session info
            var sessionInfo = SessionInfo;

            if (!string.IsNullOrEmpty(Request.QueryString["oid"]))
            {
                var customerOrderV01 = CustomerOrderingProvider.GetCustomerOrderByOrderID(Request.QueryString["oid"]);

                if (customerOrderV01 == null)
                {
                    // Redirect Back with error message
                    Response.Redirect("~/dswebadmin/customerorders.aspx?error=1", false);
                }
                else
                {
                    // Check if DS is valid
                    if (customerOrderV01.DistributorID.ToLower() != DistributorID.ToLower())
                    {
                        // Redirect to customer orders page
                        Response.Redirect("~/dswebadmin/customerorders.aspx", false);
                    }

                    // Check if the order status is valid
                    if (customerOrderV01.OrderStatus == ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.InProgress)
                    {
                        bool addressValid = false;
                        int addressId = 0;
                        sessionInfo.CustomerOrderNumber = Request.QueryString["oid"];
                        CustomerOrderProvider.SetupCustomerOrderData(Request.QueryString["oid"], DistributorID, CountryCode,
                                                                     Locale, out addressValid, out addressId);
                        sessionInfo.OrderConverted = true;
                        sessionInfo.CustomerOrderAddressWasValid = addressValid;
                        sessionInfo.CustomerAddressID = addressId;
                        ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale);

                        var isreadonly = typeof(NameValueCollection).GetProperty("IsReadOnly",
                                                                                  BindingFlags.Instance |
                                                                                  BindingFlags.NonPublic);
                        // make collection editable
                        isreadonly.SetValue(Request.QueryString, false, null);
                        // remove
                        Request.QueryString.Remove("oid");
                    }
                    else
                    {
                        // Redirect To Customer Orders Page
                        Response.Redirect("~/dswebadmin/customerorders.aspx?error=1", false);
                    }
                }
            }

            bool isGoingToETO = (Request.QueryString["ETO"] ?? string.Empty).ToUpper() == "TRUE";
            bool isLeavingETO = (Request.QueryString["ETO"] ?? string.Empty).ToUpper() == "FALSE";

            bool isGoingToHAP = (Request.QueryString["HAP"] ?? string.Empty).ToUpper() == "TRUE";
            bool isLeavingHAP = (Request.QueryString["HAP"] ?? string.Empty).ToUpper() == "FALSE";


            if (isGoingToETO)
            {
                sessionInfo.IsEventTicketMode = true;
                clearCrossSell();
            }
            if (isLeavingETO)
            {
                sessionInfo.IsEventTicketMode = false;
                clearCrossSell();
            }

            if (isGoingToHAP)
            {
                sessionInfo.IsEventTicketMode = false;
                sessionInfo.IsHAPMode = true;
                DisplayCCMessage = false;

                if (!IsPostBack)
                {
                    string hapOrderId = (Request.QueryString["hapId"] ?? string.Empty).ToUpper();
                    if (!string.IsNullOrEmpty(hapOrderId))
                    {
                        LoadHapOrder(hapOrderId);
                        sessionInfo.ShoppingCart = _shoppingCart;
                        if (sessionInfo.ShoppingCart != null)
                            sessionInfo.ShoppingCart.HAPAction = "UPDATE";
                    }
                }
            }
            if(isLeavingHAP || isLeavingETO)
            {
                sessionInfo.IsHAPMode = false;
            }
            IsEventTicketMode = sessionInfo.IsEventTicketMode;
            IsHAPMode = sessionInfo.IsHAPMode;
            sessionInfo.ShowAllInventory = ShowAllInventory || IsEventTicketMode;
            SessionInfo = sessionInfo;

            (Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            (Master as OrderingMaster).LoadControls(
                PanelControlsConfiguration.FromXML(HLConfigManager.Configurations.DOConfiguration.PanelConfiguration));

            if (ShoppingCart != null)
            {
                (Page.Master as OrderingMaster).EventBus.RegisterObject(ShoppingCart);
            }

            if (OrderMonth != null)
            {
                (Page.Master as OrderingMaster).EventBus.RegisterObject(OrderMonth);
                // FOP
                if (ShoppingCart != null)
                {
                    ShoppingCart.OrderMonth = PurchasingLimitProvider.GetOrderMonth(OrderMonth, DistributorID, CountryCode);
                }
            }

            if (RequiresCartManagementRules)
            {
                HLRulesManager.Manager.ProcessCartManagementRules(ShoppingCart);
            }
            CheckCurrentPageVisitation();
            base.OnInit(e);
            includeScript();

            var args = Session[OrderingMaster.SessionRedirectKey] as PageVisitRefusedEventArgs;
            if (null != args)
            {
                if (args.Reason == PageVisitRefusedReason.InvalidDeliveryInfo)
                {
                    if (NeedEnterAddress())
                    {
                        args.Message = null; // clear message
                        ShippingInfoNotFilled(this, null);
                    }
                }
            }

            if (!IsPostBack)
            {
                if (!(Page is ShoppingCart) && !(Page is Checkout))
                {
                    sessionInfo.ConfirmedAddress = false;
                }

                //Check Dup Check Cookie
                string rawUrl = HttpContext.Current.Request.RawUrl;
                var dupCheckWithReferenceId = Settings.GetRequiredAppSetting("DupCheckWithReferenceId", false);
                if (dupCheckWithReferenceId && (!rawUrl.Contains("Confirm.aspx")))
                {
                    var refId = GetDupCheckCookie();
                    if (refId.Length > 0)
                    {
                        var tempOrderId = string.Empty;
                        var isDuplicate = false;
                        OrderProvider.GetOrderByReferenceId(refId, ref tempOrderId, ref isDuplicate);
                        if (isDuplicate)
                        {
                            RemoveDupCheckCookie();
                            SessionInfo.OrderNumber = tempOrderId;
                            Response.Redirect("Confirm.aspx?OrderNumber=" + tempOrderId);
                        }
                    }
                }
            }

            DisplayTrainingMessage();
            if (!IsPostBack)
            {
                DisplayAnnoucement();

                if (ShoppingCart != null)
                {
                    ShoppingCart.IsResponsive = ((Master as OrderingMaster).IsMobile() &&
                                                 HLConfigManager.Configurations.DOConfiguration.IsResponsive);
                }
                DisplayNonResidentMessage();
                DisplayMissingTINMessage();
            }
        }

        #region DupCheckCookie
        protected string GetDupCheckCookie()
        {
            var cookie = HttpContext.Current.Request.Cookies[DupCheckKey];
            if (cookie != null)
            {
                if (cookie.Value != null)
                {
                    return cookie.Value;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// remove Dup check cookie.
        /// </summary>

        public static void RemoveDupCheckCookie()
        {
            if (HttpContext.Current.Request.Cookies[DupCheckKey] != null)
            {
                HttpCookie myCookie = new HttpCookie(DupCheckKey);
                myCookie.Domain = FormsAuthentication.CookieDomain;
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                myCookie.Path = "/Ordering";
                HttpContext.Current.Response.Cookies.Add(myCookie);
            }
        }

        /// <summary>
        /// Sets the Dup check cookie.
        /// </summary>
        protected void SetDupCheckCookie(string refId)
        {
            //string strScript = string.Format("document.cookie = '{0}={1}';", DupCheckKey, refId);
            //System.Web.UI.ScriptManager.RegisterClientScriptBlock(Page, typeof(System.Web.UI.WebControls.PagedDataSource), "SetCookie", strScript, true);

        }
        #endregion

        //private void SetValuesFromQueryString(Providers.Ordering.SessionInfo sessionInfo)
        //{
        //    if (!string.IsNullOrEmpty(Request.QueryString["cav"]) && !string.IsNullOrEmpty(Request.QueryString["coc"]))
        //    {
        //        sessionInfo.CustomerOrderAddressWasValid =
        //            bool.Parse(Request.QueryString["cav"].ToString());
        //        sessionInfo.OrderConverted = bool.Parse(Request.QueryString["coc"].ToString());
        //    }

        //    sessionInfo.CustomerAddressID =
        //        int.Parse(Request.QueryString["cad"].ToString());
        //    sessionInfo.CustomerOrderNumber = Request.QueryString["cid"].ToString();

        //    if (sessionInfo.ShippingAddresses == null)
        //    {
        //        sessionInfo.ShippingAddresses = new List<ShippingAddress_V02>();
        //    }

        //    if (sessionInfo.ShippingAddresses.Find(a => a.ID == sessionInfo.CustomerAddressID) == null && sessionInfo.CustomerAddressID != 0)
        //    {
        //        var address = new Address_V01
        //        {
        //            Line1 = Request.QueryString["l1"],
        //            Line2 = Request.QueryString["l2"],
        //            City = Request.QueryString["cty"],
        //            StateProvinceTerritory = Request.QueryString["st"],
        //            Country = Request.QueryString["cntry"],
        //            CountyDistrict = Request.QueryString["cnty"],
        //            PostalCode = Request.QueryString["zip"]
        //        };

        //        var shippingAddress = new ShippingAddress_V02
        //            {
        //                Address = address,
        //                FirstName = Request.QueryString["fname"],
        //                LastName = Request.QueryString["lname"],
        //                Phone = Request.QueryString["ph"],
        //                ID = sessionInfo.CustomerAddressID,
        //                Recipient = Request.QueryString["fname"] + " " + Request.QueryString["lname"]
        //        };

        //        shippingAddress.DisplayName =
        //            ShippingProvider.GetShippingProvider(this.CountryCode).GetAddressDisplayName(shippingAddress);

        //        sessionInfo.ShippingAddresses.Add(shippingAddress);
        //    }
        //}

        //private void SetValuesFromSession(SessionInfo sessionInfo)
        //{
        //    if (this.Session["CustomerOrderAddressWasValid"] != null && this.Session["OrderConverted"] != null)
        //    {
        //        sessionInfo.CustomerOrderAddressWasValid =
        //            bool.Parse(HttpContext.Current.Session["CustomerOrderAddressWasValid"].ToString());
        //        sessionInfo.OrderConverted = bool.Parse(HttpContext.Current.Session["OrderConverted"].ToString());
        //    }

        //    sessionInfo.CustomerAddressID =
        //        int.Parse(HttpContext.Current.Session["CustomerAddressID"].ToString());
        //    sessionInfo.CustomerOrderNumber = HttpContext.Current.Session["CustomerOrderNumber"].ToString();
        //    this.Session["CustomerOrderAddressWasValid"] = null;
        //    this.Session["CustomerAddressID"] = null;
        //    this.Session["CustomerOrderNumber"] = null;
        //    this.Session["OrderConverted"] = null;
        //}

        private void includeScript()
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.AddressingConfiguration.ScriptPath))
            {
                string sInclude = HLConfigManager.Configurations.AddressingConfiguration.ScriptPath;
                if (!string.IsNullOrEmpty(sInclude))
                {
                    sInclude = ResolveUrl(sInclude);
                    var Include = new HtmlGenericControl("script");
                    Include.Attributes.Add("type", "text/javascript");
                    Include.Attributes.Add("src", sInclude);
                    Page.Header.Controls.Add(Include);
                }
            }
        }

        private void clearCrossSell()
        {
            Session[ProductDetail.LAST_SEEN_PRODUCT_SESSION_EKY] = null;
            Session[ProductDetail.LAST_SEEN_CROSS_SELL_PRODUCT_SESSION_EKY] = null;
        }

        //[SubscribesTo(MyHLEventTypes.PageVisitRefused)]
        //public void PreviousPageRedirected(object sender, EventArgs e)
        //{
        //    PageVisitRefusedEventArgs args = e as PageVisitRefusedEventArgs;
        //    if (null != args)
        //    {
        //        if (args.Reason == PageVisitRefusedReason.CantBuy || args.Reason == PageVisitRefusedReason.BlockedBySponsor || args.Reason == PageVisitRefusedReason.HardCashNoSuitablePaymentMethods)
        //        {
        //            (this.Master as OrderingMaster).ShowMessage("From Base Page Itself:", args.Message);
        //        }
        //    }
        //}

        /// <summary>
        ///     The get shipping provider.
        /// </summary>
        /// <returns>
        /// </returns>
        public IShippingProvider GetShippingProvider()
        {
            return ShippingProvider.GetShippingProvider(CountryCode);
        }

        public IShippingProvider GetShippingProvider(ServiceProvider.CatalogSvc.OrderCategoryType OrderType)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
            shippingProvider.OrderType = (ServiceProvider.ShippingSvc.OrderCategoryType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.OrderCategoryType), OrderType.ToString());
            return shippingProvider;
        }
        /// <summary>
        ///     The get distributor info.
        /// </summary>
        /// <returns>
        ///     The get distributor info.
        /// </returns>
        public string GetDistributorInfo()
        {
            return string.Format("Distributor: {0}, COP: {1}", DistributorID, CountryCode) + "\n";
        }

        /// <summary>
        ///     The no preferences.
        /// </summary>
        /// <returns>
        ///     The no preferences.
        /// </returns>
        public bool NeedEnterAddress()
        {
            if (IsChina && SessionInfo.IsEventTicketMode)
                return false;
            return GetShippingProvider().NeedEnterAddress(DistributorID, Locale);
        }

        /// <summary>
        ///     The no delivery option info.
        /// </summary>
        /// <returns>
        ///     The no delivery option info.
        /// </returns>
        public bool NoDeliveryOptionInfo()
        {
            return _shoppingCart.DeliveryInfo == null ||
                   string.IsNullOrEmpty(_shoppingCart.DeliveryInfo.WarehouseCode) ||
                   string.IsNullOrEmpty(_shoppingCart.DeliveryInfo.FreightCode) ||
                   _shoppingCart.DeliveryInfo.Address == null ||
                   _shoppingCart.DeliveryInfo.Address.Address == null;
        }

        public void NoSavedAddress()
        {
            if (NoDeliveryOptionInfo())
            {
                ShippingInfoNotFilled(this, null);
            }
        }

        public void FireAPFChangedEvent()
        {
            if (null != APFCartChanged)
            {
                var args = new ShoppingCartEventArgs(ShoppingCart);
                APFCartChanged(this, args);
            }
        }

        public void AddItemsToCart(List<ShoppingCartItem_V01> items, AddingItemOption option = AddingItemOption.AddItem)
        {
            if (ShoppingCart != null)
            {
                try
                {
                    if (option == AddingItemOption.DeleteBeforeAdd)
                    {
                        var skus = items.Select(x => x.SKU).ToList();
                        ShoppingCart.DeleteItemsFromCart(skus, true);
                    }
                    else if (option == AddingItemOption.ModifyQuantity)
                    {
                        // Change item quantity adding the excess to an existent sku
                        var skusToChange = from c in ShoppingCart.CartItems
                                           from i in items
                                           where c.SKU == i.SKU 
                                           select i;

                        foreach (var item in skusToChange)
                        {
                            // Add the excess to the existent sku in cart
                            var itemIncart = ShoppingCart.CartItems.Find(i => i.SKU == item.SKU);
                            if (itemIncart != null)
                                item.Quantity -= itemIncart.Quantity;
                        }
                        ShoppingCart.OnCheckout = true;
                    }
                    else if (option == AddingItemOption.ChangeQuantity)
                    {
                        // Remove those skus which quantity is minor that the existent in cart 
                        var skusToRemove = (from c in ShoppingCart.CartItems
                                            from i in items
                                            where c.SKU == i.SKU && c.Quantity > i.Quantity
                                            select i.SKU).ToList<string>();
                        ShoppingCart.DeleteItemsFromCart(skusToRemove, true);

                        // Change item quantity adding the excess to an existent sku
                        var skusToChange = from c in ShoppingCart.CartItems
                                           from i in items
                                           where c.SKU == i.SKU && c.Quantity < i.Quantity
                                           select i;

                        foreach (var item in skusToChange)
                        {
                            // Add the excess to the existent sku in cart
                            var itemIncart = ShoppingCart.CartItems.Find(i => i.SKU == item.SKU);
                            item.Quantity = item.Quantity - itemIncart.Quantity;
                        }
                        ShoppingCart.OnCheckout = true;
                    }

                    ShoppingCart.AddItemsToCart(items);
                    ShoppingCart.OnCheckout = false;
                    bool bProductAvailabilityTypeChanged = false;

                    foreach (ShoppingCartItem_V01 item in items)
                    {
                        SKU_V01 sku01;
                        if (AllSKUS.TryGetValue(item.SKU, out sku01))
                        {
                            var avail = sku01.ProductAvailability;
                            sku01.ProductAvailability = CatalogProvider.GetProductAvailability(sku01, CurrentWarehouse);
                            bProductAvailabilityTypeChanged = bProductAvailabilityTypeChanged
                                                                  ? true
                                                                  : avail != sku01.ProductAvailability;
                        }
                    }
                    if (bProductAvailabilityTypeChanged)
                        ProductAvailabilityTypeChanged(this, null);

                    //if (!ShoppingCart.DeliverInfoRemoved)
                    //{
                    //    var apfSKUs = from sku in items where APFDueProvider.IsAPFSku(sku.SKU) select sku;
                    //    if (null != apfSKUs && apfSKUs.Count() > 0)
                    //    {
                    //        FireAPFChangedEvent();
                    //    }
                    //}
                    ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("AddItemsToCart error {0}", ex));
                }
            }
        }

        public void DeleteItemsFromCart(List<string> itemsToRemove)
        {
            if (ShoppingCart != null)
            {
                bool bProductAvailabilityTypeChanged = false;
                var itemsRemoved = ShoppingCart.DeleteItemsFromCart(itemsToRemove);
                foreach (string sku in itemsRemoved)
                {
                    SKU_V01 sku01;
                    if (AllSKUS.TryGetValue(sku, out sku01))
                    {
                        var avail = sku01.ProductAvailability;
                        sku01.ProductAvailability = CatalogProvider.GetProductAvailability(sku01, CurrentWarehouse);
                        bProductAvailabilityTypeChanged = bProductAvailabilityTypeChanged
                                                              ? true
                                                              : avail != sku01.ProductAvailability;
                    }
                }
                if (bProductAvailabilityTypeChanged)
                    ProductAvailabilityTypeChanged(this, null);
                if (null != itemsToRemove)
                {
                    var apf = from sku in itemsToRemove where APFDueProvider.IsAPFSku(sku) select sku;
                    if (null != apf && apf.Count() > 0)
                    {
                        FireAPFChangedEvent();
                    }
                }
                ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
            }
        }

        public bool AddHFFSKU(int quantity)
        {
            bool added = false;
            if (ShoppingCart != null)
            {
                string hffSku = "";
                if (HLConfigManager.Configurations.DOConfiguration.HFFSkuList != null && HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Count > 0)
                {
                    hffSku = HLConfigManager.Configurations.DOConfiguration.HFFSkuList.FirstOrDefault();
                }
                else
                {
                    hffSku = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku;
                }
                
                if (!string.IsNullOrEmpty(hffSku))
                {
                    if (AllSKUS.ContainsKey(hffSku))
                    {
                        ShoppingCart.AddHFFSKU(quantity);
                        added = true;
                        ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
                    }
                }
            }

            return added;
        }

        public void AddTodayMagazine(int quantity, string todayMagazineSKU)
        {
            if (ShoppingCart != null)
            {
                ShoppingCart.AddTodayMagazine(quantity, todayMagazineSKU);
                ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
            }
        }

        public void ResetInventoryView()
        {
            if (HLConfigManager.Configurations.DOConfiguration.ResetInventoryViewDefaultAfterSumbitOrder)
            {
                SessionInfo.ShowAllInventory = HLConfigManager.Configurations.DOConfiguration.InventoryViewDefault == 0;
                //DistributorOrderingProfile.ShowAllInventory = this.SessionInfo.ShowAllInventory;
            }
        }

        protected IPurchaseRestrictionManager PurchaseRestrictionManager(string distributorId)
        {
            IPurchaseRestrictionManagerFactory purchaseRestrictionManagerFactory = new PurchaseRestrictionManagerFactory();
            return purchaseRestrictionManagerFactory.GetPurchaseRestrictionManager(distributorId);
        }

        public void ClearCart()
        {
            if (ShoppingCart != null)
            {
                ShoppingCart.ClearCart();

                if (HLConfigManager.Configurations.DOConfiguration.IsChina && ShoppingCart.APFEdited)
                    ShoppingCart.APFEdited = false;
            }
            if (null != ShoppingCartChanged)
            {
                ShoppingCartChanged(this, new ShoppingCartEventArgs(ShoppingCart));
            }
            SetShoppingCartModuleCache(ShoppingCart);
        }

        private bool checkHardCashCanBuy(bool hardCash, bool checkCashOnly = true)
        {
            var paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
            if (!paymentsConfig.AllowPurchaseForHardCash &&
               HardCashCantVisit &
               (hardCash &&
                !((paymentsConfig.AllowWireForHardCash && paymentsConfig.AllowWirePayment) || paymentsConfig.AllowCreditCardForHardCash ||
                  (paymentsConfig.AllowWireForHardCash && paymentsConfig.AllowDirectDepositPayment))))
                return false;
            if (HardCashCantVisit)
            {
                if (CashOnly() && HLConfigManager.Configurations.PaymentsConfiguration.HasOnlyCreditCardOption)
                    return false;
            }
            if (checkCashOnly)
            {
                if (CashOnly() && HLConfigManager.Configurations.PaymentsConfiguration.HasOnlyCreditCardOption)
                    return false;
            }
            return true;
        }
        public void CheckCurrentPageVisitation()
        {
            bool canVisit = true;
            NotAllowedForPM = false;
            string message = string.Empty;
            var reason = PageVisitRefusedReason.Unknown;
            PageVisitRefusedEventArgs eventArgs = null;
            var previous = Request.UrlReferrer;
            NotAllowed = false;
            var DSType = DistributorOrderingProfileProvider.CheckDsLevelType(DistributorID, Locale.Substring(3, 2));
            if (DSType == ServiceProvider.DistributorSvc.Scheme.Member)
            {
                message =
                    HttpContext.GetGlobalResourceObject("MyHL_ErrorMessage", "PMTypeRestrictOrdering").ToString();
                NotAllowed = true;
                NotAllowedForPM = true;
                reason = PageVisitRefusedReason.CantBuy;
                eventArgs = new PageVisitRefusedEventArgs(Request.Url.PathAndQuery, message, reason);
                Session[OrderingMaster.SessionRedirectKey] = eventArgs;
            }
            DistributorOrderingProfile distributorProfile = new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(DistributorID, CountryCode);
            if(distributorProfile !=null && distributorProfile.OrderRestrictions !=null) 
            {
                var result = distributorProfile.OrderRestrictions.FirstOrDefault(x => x.RestrictionType.ToUpper() == "ALLOW_ONLY_US_PR_ORDERS");
                if (result != null)
                {
                     NotAllowed = true;
                    NotAllowedForPM = false;
                    message = PlatformResources.GetGlobalResourceString("ErrorMessage", "NonUSPR");
                    reason = PageVisitRefusedReason.CantBuy;
                    eventArgs = new PageVisitRefusedEventArgs(Request.Url.PathAndQuery, message, reason);
                    Session[OrderingMaster.SessionRedirectKey] = eventArgs;
                }
            }
            //Reasons to get bounced out of the page
            if (!checkHardCashCanBuy(HardCash, false))
            {
                canVisit = false;
                message = PlatformResources.GetGlobalResourceString("ErrorMessage", "CantBuy");
                reason = PageVisitRefusedReason.HardCashNoSuitablePaymentMethods;
            }

            if (canVisit)
            {
                if (CantBuy & CantBuyCantVisit)
                {
                    canVisit = false;
                    message = PlatformResources.GetGlobalResourceString("ErrorMessage", "CantBuy");
                    reason = PageVisitRefusedReason.CantBuy;
                }
            }

            if (canVisit)
            {
                if (CantBuyBlockedBySponsor & CantBuyCantVisit)
                {
                    canVisit = false;
                    message = PlatformResources.GetGlobalResourceString("ErrorMessage", "BlockedDS");
                    reason = PageVisitRefusedReason.BlockedBySponsor;
                }
                if (NotAllowed & CantBuyCantVisit)
                {
                    canVisit = false;
                }
            }
            // called from My Invoice
            if (!string.IsNullOrEmpty(Request.QueryString["CartID"]) &&
                ShoppingCart.CopyInvoiceStatus == CopyInvoiceStatus.AddressValidationFail
                && ShoppingCart.CopyInvoiceAddress != null)
            {
                goto CantVisit;
            }
            if (canVisit)
            {
                if (CantVisitWithoutDeliveryInfo)
                {
                    if ((ShoppingCart.IsSavedCart || ShoppingCart.IsFromCopy) && (Page is ShoppingCart))
                    {
                        var savedCopyError = GetSaveCopyError();
                        if (string.IsNullOrEmpty(savedCopyError) &&
                            ((null != ShoppingCart && null == ShoppingCart.DeliveryInfo) ||
                             (ShippingAddressIsInvalid(ShoppingCart.DeliveryInfo))))
                        {
                            canVisit = false;
                            message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                "CantVisitWithNoDeliveryInfo");
                            reason = PageVisitRefusedReason.InvalidDeliveryInfo;
                        }
                    }
                    else if (!ShoppingCart.IsFromInvoice &&
                        ((null != ShoppingCart && null == ShoppingCart.DeliveryInfo) ||
                         (ShippingAddressIsInvalid(ShoppingCart.DeliveryInfo))))
                    {
                        canVisit = false;
                        message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                            "CantVisitWithNoDeliveryInfo");
                        reason = PageVisitRefusedReason.InvalidDeliveryInfo;
                    }
                    else
                    {
                        var provider = ShippingProvider.GetShippingProvider(null);
                        if (provider != null && !provider.ValidateAddress(ShoppingCart))
                        {
                            canVisit = false;
                            message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                "CantVisitWithNoDeliveryInfo");
                            reason = PageVisitRefusedReason.InvalidDeliveryInfo;
                        }
                    }
                }
            }

            if (canVisit)
            {
                if (CantVisitWithDSFraud & (null != ShoppingCart && null != ShoppingCart.DeliveryInfo &&
                                            null != ShoppingCart.DeliveryInfo.Address &&
                                            null != ShoppingCart.DeliveryInfo.Address.Address)
                    & HLConfigManager.Configurations.AddressingConfiguration.ValidateDSFraud)
                {
                    //string errorResxKey = DistributorProvider.CheckForDRFraud(DistributorID,CountryCode,ShoppingCart.DeliveryInfo.Address.Address.PostalCode);
                    if (!string.IsNullOrEmpty(ShoppingCart.DSFraudValidationError))
                    {
                        canVisit = false;
                        message =
                            HttpContext.GetGlobalResourceObject(
                                string.Format("{0}_ErrorMessage", HLConfigManager.Platform),
                                ShoppingCart.DSFraudValidationError) as string;
                        reason = PageVisitRefusedReason.BlockedBySponsor;
                    }
                }
            }

            if (canVisit)
            {
                OrderTotals_V02 totals = ShoppingCart.Totals as OrderTotals_V02;
                if (CantVisitWithEmptyCart &
                    (null != ShoppingCart && (null == ShoppingCart.CartItems || ShoppingCart.CartItems.Count == 0))
                    && (HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU && null != totals && totals.Donation == 0.00m))
                {
                    canVisit = false;
                    message =
                        string.Format(
                            PlatformResources.GetGlobalResourceString("ErrorMessage", "CantVisitWithEmptyCart"),
                            Request.Url.Segments[2].Replace(".aspx", string.Empty));
                    reason = PageVisitRefusedReason.CartIsEmpty;
                }
            }

            if (canVisit)
            {
                if (CantVisitSavedCardsNotAllowed &
                    !HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards)
                {
                    canVisit = false;
                    reason = PageVisitRefusedReason.SavedPaymentInfoNotAllowed;
                }
            }

            if (canVisit)
            {
                var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                string countryofprocessing = member.Value.ProcessingCountryCode;

                bool bLimitExceeded = false;
                if (!Settings.GetRequiredAppSetting<bool>("FOPEnabled", false))
                {
                    if (CantVisitWithExceededPurchasingLimits && PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, countryofprocessing))
                    {
                        if (HLRulesManager.Manager.PurchasingLimitsAreExceeded(DistributorID))
                        {
                            bLimitExceeded = true;
                        }
                    }
                }
                else
                {
                    if (CantVisitWithExceededPurchasingLimits)
                    {
                        if (PurchaseRestrictionManager(DistributorID).PurchasingLimitsAreExceeded(DistributorID, this.ShoppingCart))
                        {
                            bLimitExceeded = true;
                        }
                    }
                }
                if (bLimitExceeded)
                {
                    canVisit = false;
                    message = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                        "NoCheckoutPurchaseLimitsExceeded");
                    reason = PageVisitRefusedReason.PurchasingLimitsExceeded;
                }
            }

            //if (canVisit)
            //{
            //    if (CantVisitWithNoTotals)
            //    {
            //        if (ShoppingCart != null && ShoppingCart.ShoppingCartItems.Count > 0)
            //        {
            //            if (ShoppingCart.Totals == null || ShoppingCart.Totals.AmountDue == 0.0M)
            //            {
            //                canVisit = false;
            //                message = PlatformResources.GetGlobalResourceString("ErrorMessage", "UnableToPrice");
            //                reason = PageVisitRefusedReason.UnableToPrice;
            //            }
            //        }
            //    }
            //}

            if (canVisit)
            {
                if (CantVisitWithoutOrderNumber)
                {
                    var currentSession = SessionInfo;
                    if (currentSession != null)
                    {
                        if (String.IsNullOrEmpty(currentSession.OrderNumber))
                        {
                            if (IsChina && Request.Url.PathAndQuery.ToString().Contains("/Ordering/Confirm.aspx"))
                            {
                                LogInvalidInput();
                            }
                            else
                            {
                                canVisit = false;
                                message = PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidInput");
                                reason = PageVisitRefusedReason.UnableToPrice;

                            }
                        }
                    }
                }
            }

            if (canVisit)
            {
                if (CantVisitSavedCartsNotAllowed & !HLConfigManager.Configurations.DOConfiguration.AllowSavedCarts)
                {
                    canVisit = false;
                    reason = PageVisitRefusedReason.BlockedBySponsor;
                }
            }
            CantVisit:
            if (!canVisit)
            {
                string newUrl = string.Empty;
                string requestUrl = Request.Url.PathAndQuery;
                eventArgs = new PageVisitRefusedEventArgs(Request.Url.PathAndQuery, message, reason);
                if (null != previous)
                {
                    if (DSType == ServiceProvider.DistributorSvc.Scheme.Member || previous.Host != Request.Url.Host || previous.PathAndQuery.ToUpper(CultureInfo.InvariantCulture).IndexOf("/ORDERING") < 0)
                    {
                        newUrl = HLConfigManager.Configurations.DOConfiguration.LandingPage;
                    }
                    else
                    {
                        newUrl = previous.PathAndQuery;
                    }
                    if (string.Compare(newUrl, requestUrl, true) == 0)
                    {
                        //newUrl = "catalog.aspx";
                        //message = string.Empty; //circumvent recursion and keep quiet about it.
                        return;
                    }
                }
                else
                {
                    newUrl = "catalog.aspx";
                }
                //one of the next two will do.
                //Session[OrderingMaster.SessionMessageKey] = message;
                Session[OrderingMaster.SessionRedirectKey] = eventArgs;
                PageRedirected(this, eventArgs);

                Response.Redirect(newUrl, true);
            }
        }

        public void LogInvalidInput()
        {
            OrderTotals_V02 totals_V02 = ShoppingCart.Totals as OrderTotals_V02;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ID " + DistributorID);
            sb.AppendLine("Name " + DistributorName);
            sb.AppendLine("TRXDate " + DateTime.Today);
            sb.AppendLine("DonationAmt " + (totals_V02 != null ? totals_V02.Donation : -1));
            sb.AppendLine("BalanceAmount " + (totals_V02 != null ? totals_V02.AmountDue : -1));
            sb.AppendLine("current visited page: " + Request.Url.PathAndQuery);
            sb.AppendLine("Cart Totals: " + (totals_V02 != null ? totals_V02.AmountDue : -1));

            Exception traceEx = new Exception("Invalid Input currentSession.OrderNumber null", new Exception(sb.ToString()));
            LoggerHelper.Info(string.Format("Confirmation Page : \n {0} ", traceEx));

        }

        /// <summary>
        ///     The can add product.
        /// </summary>
        /// <param name="distributorID">
        ///     The distributor id.
        /// </param>
        /// <param name="errors">
        ///     The errors.
        /// </param>
        /// <returns>
        ///     The can add product.
        /// </returns>
        public bool CanAddProduct(string distributorID, ref List<string> errors)
        {
            if (HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
            {
                if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                {
                    return true; //Let the APFRules handle this so we get the appropriate error message.
                }
            }
            if (errors == null)
            {
                errors = new List<string>();
            }

            bool retCode = true;

            if (CantBuy || Deleted)
            {
                retCode = false;
                (Master as OrderingMaster).CantBuyCantVisitShoppingCart = true;
                //errors.Add(MyHL_ErrorMessage.CantOrder);
            }
            else if(NotAllowed)
            {
                retCode = false;
            }
            else
            {
                if (NoDeliveryOptionInfo() || SessionInfo.IsVenuzulaShipping)
                {
                    Session[ViewStateNoDeliveryOptionInfo] = true;
                    ShippingInfoNotFilled(this, null);
                    return false;
                }
                else
                {
                    if (ShoppingCart.DeliveryOption == DeliveryOptionType.Pickup)
                    {
                        ProceedingToCheckoutFromMiniCart(this, null);

                        if (PickUpLocationNotSelected)
                        {
                            //Need to refactor the below conidtion.
                            if (IsEventTicketMode &&
                                !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
                            {
                            }
                            else
                            {
                                errors.Add(MyHL_ErrorMessage.PickUpLocationNotSelected);
                                return false;
                            }
                        }
                    }
                }

                //if (ShoppingCart.OrderCategory != OrderCategoryType.ETO)
                //{
                //    if(ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Address != null)
                //        this.ShoppingCart.UpdateShippingInfo(ShoppingCart.DeliveryInfo.Address.ID, ShoppingCart.DeliveryInfo.Id, ShoppingCart.DeliveryInfo.Option);
                //}

                if (!checkHardCashCanBuy(HardCash))
                {
                    retCode = false;
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
                }
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.PurchasingLimitsControl))
                {
                    if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
                    {

                        // Check Value Selected in Drop Down
                        if (String.IsNullOrEmpty(_shoppingCart.SelectedDSSubType))
                        {
                            retCode = false;
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NeedToSelectOrderType"));
                        }
                    }
                }

                if (!HLConfigManager.Configurations.DOConfiguration.PreselectedDualOrderMonth && OrderMonth.IsDualOrderMonth &&
                    OrderMonth.CurrentChosenOrderMonth == OrderMonth.DualOrderMonthSelection.NotSelected)
                {
                    retCode = false;
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoOrderMonth"));
                }
            }

            return retCode;
        }

        // 26790, add item from popup
        public bool CanAddProductFromPopup(string distributorID, ref List<string> errors)
        {
            if (HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed)
            {
                if (APFDueProvider.containsOnlyAPFSku(ShoppingCart.CartItems))
                {
                    return true; //Let the APFRules handle this so we get the appropriate error message.
                }
            }
            if (errors == null)
            {
                errors = new List<string>();
            }

            bool retCode = true;

            if (CantBuy || Deleted)
            {
                retCode = false;
                errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
            }
            else
            {
                if (NoDeliveryOptionInfo() || SessionInfo.IsVenuzulaShipping)
                {
                    Session[ViewStateNoDeliveryOptionInfo] = true;
                    ShippingInfoNotFilled(this, null);
                    return false;
                }
                else if (NotAllowed )
                {
                    retCode = false;
                    if(!NotAllowedForPM)
                        errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NonUSPR"));
                }
                else
                {
                    if (ShoppingCart.DeliveryOption == DeliveryOptionType.Pickup)
                    {
                        ProceedingToCheckoutFromMiniCart(this, null);

                        if (PickUpLocationNotSelected)
                        {
                            //Need to refactor the below conidtion.
                            if (IsEventTicketMode &&
                                !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
                            {
                            }
                            else
                            {
                                errors.Add(MyHL_ErrorMessage.PickUpLocationNotSelected);
                                return false;
                            }
                        }
                    }
                }

                if (!checkHardCashCanBuy(HardCash))
                {
                    retCode = false;
                    errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
                }

                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.DOConfiguration.PurchasingLimitsControl))
                {
                    if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
                    {
                        // Check Value Selected in Drop Down
                        if (String.IsNullOrEmpty(_shoppingCart.SelectedDSSubType))
                        {
                            retCode = false;
                            errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NeedToSelectOrderType"));
                        }
                    }
                }
            }

            return retCode;
        }

        /// <summary>
        ///     check qty against inventory
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="sku"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public int CheckInventory(int quantity, SKU_V01 sku, List<string> errors)
        {
            int newQuantity = quantity;
            int availQuantity = 0;
            HLRulesManager.Manager.CheckInventory(ShoppingCart, newQuantity, sku, CurrentWarehouse, ref availQuantity);
            var ruleResultMessages =
                from r in ShoppingCart.RuleResults
                where r.Result == RulesResult.Failure && (r.RuleName == "Back Order" || r.RuleName == "Inventory Rules")
                select r.Messages[0];
            if (ruleResultMessages.Any())
            {
                Array.ForEach(ruleResultMessages.ToArray(), a => errors.Add(ruleResultMessages.First()));
            }
            ShoppingCart.RuleResults.Clear();
            return availQuantity;
        }

        public int QuantityInCart(string sku)
        {
            if (ShoppingCart == null || ShoppingCart.CartItems == null || ShoppingCart.CartItems.Count == 0)
                return 0;
            var item = ShoppingCart.CartItems.Find(i => i.SKU == sku);
            if (item != null)
            {
                return item.Quantity;
            }
            return 0;
        }

        public static string getAmountString(decimal amount)
        {
            var symbol = GetSymbol();
            return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbolPosition ==
                   CheckoutConfiguration.CurrencySymbolLayout.Leading
                       ? (HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                              ? symbol + amount.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                              : symbol + string.Format(GetPriceFormat(amount), amount))
                       : string.Format(GetPriceFormat(amount), amount) + symbol;
        }

        public static string GetPriceFormat(decimal number)
        {
            string PriceFormatString = string.Empty;
            if (HLConfigManager.Configurations.CheckoutConfiguration.UseCommaWithoutDecimalFormat)
            {
                if (HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal)
                {
                    PriceFormatString = number.ToString("N2", CultureInfo.GetCultureInfo("en-US"));

                }
                else
                {
                    if (number == 0)
                    {
                        return "0,000";
                    }

                    PriceFormatString = number.ToString("#,###", CultureInfo.GetCultureInfo("en-US"));
                }
            }
            else
            {
                PriceFormatString = HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal)0.0 ? "{0:0}" : "{0:#,###}");
            }

            return PriceFormatString;
        }

        public static string GetVolumePointsFormat(decimal volumePoints)
        {
            string VolumeFormatString = string.Empty;
            if (HLConfigManager.Configurations.CheckoutConfiguration.UseCommaWithoutDecimalFormat)
            {
                VolumeFormatString = volumePoints.ToString("N", CultureInfo.GetCultureInfo("en-US"));

            }
            else
            {
                VolumeFormatString = HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                                                 ? volumePoints.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                                                : volumePoints.ToString("N2");

            }

            return VolumeFormatString;
        }



        /// <summary>
        ///     Check Backorder Coverage
        /// </summary>
        /// <param name="quantity"></param>
        /// <param name="sku"></param>
        /// <param name="msgs"></param>
        /// <returns></returns>
        public int CheckBackorderCoverage(int quantity, SKU_V01 sku, List<string> msgs)
        {
            int backorderCoverage = 0;
            var cacheKey = string.Format("MakeBackorderFalse_{0}", CountryCode);
            var results = HttpRuntime.Cache[cacheKey] as List<SKUQuantityRestrictInfo>;
            if (results != null && results.Any(k => k.SKU.Contains(sku.SKU)))
            {
                return backorderCoverage;
            }
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.AllowBackorder)
            {
                HLRulesManager.Manager.PerformBackorderRules(ShoppingCart, sku.CatalogItem);
                var ruleResultMessages =
                    from r in ShoppingCart.RuleResults
                    where r.Result == RulesResult.Failure && r.RuleName == "Back Order"
                    select r.Messages[0];
                if (null != ruleResultMessages && ruleResultMessages.Count() > 0)
                {
                    ShoppingCart.RuleResults.Clear();
                    return backorderCoverage;
                }

                if (!HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayMessageForBackorder)
                {
                    return quantity;
                }
                else
                {
                    int quantityInCart = QuantityInCart(sku.SKU);
                    var optionType = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), OptionType.ToString());
                    backorderCoverage = ShoppingCartProvider.CheckForBackOrderableOverage(sku, quantity + quantityInCart,
                                                                                          CurrentWarehouse, optionType);
                    if (backorderCoverage > 0)
                    {
                        string productName = string.Format("{0} {1}", sku.SKU, sku.Description);
                        msgs.Add(string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "BackOrderPartialQuantity"), productName,
                                               quantity - backorderCoverage, backorderCoverage));
                    }
                }
            }
            return backorderCoverage;
        }

        /// <summary>
        ///     get all quantities
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="quantity"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        public int GetAllQuantities(ShoppingCartItemList itemList, int quantity, string sku)
        {
            if (itemList != null && itemList.Exists(i => i.SKU == sku))
            {
                return itemList.Where(l => l.SKU == sku).Sum(m => m.Quantity) + quantity;
            }
            return quantity;
        }

        /// <summary>
        ///     check if quanity excess max
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="quantity"></param>
        /// <param name="sku"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool CheckMaxQuantity(ShoppingCartItemList itemList, int quantity, SKU_V01 sku, List<string> errors)
        {
            int maxQuantity = GetMaxQuantity(sku.SKU);
            int newQuantity = quantity;
            int allQuantity = GetAllQuantities(itemList, quantity, sku.SKU);

            string productName = string.Format("{0} {1}", sku.SKU, sku.Description);

            if (allQuantity > maxQuantity)
            {
                string error = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantity"), productName, maxQuantity);
                if (!errors.Contains(error))
                {
                    errors.Add(error);
                }

                if (HLConfigManager.Configurations.ShoppingCartConfiguration.DisplayETOMaxQuantityMessage && sku.CatalogItem.IsEventTicket)
                {
                    error = string.Format(PlatformResources.GetGlobalResourceString("ErrorMessage", "MaxQuantityETO"), maxQuantity);
                    if (!errors.Contains(error))
                    {
                        errors.Add(error);
                    }
                }
                return false;
            }
            return true;
        }

        public int GetMaxQuantity(string sku)
        {
            if (sku == HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku || HLConfigManager.Configurations.DOConfiguration.HFFSkuList.Exists(s => s.Equals(sku)))
            {
                return HLConfigManager.Configurations.DOConfiguration.HFFSkuMaxQuantity;
            }

            //Check for Today Magazine
            if (sku == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku ||
                sku == HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
            {
                if (HLConfigManager.Configurations.DOConfiguration.ModifyTodaysMagazineQuantity)
                {
                    return ShoppingCart.TodaysMagaZineQuantity;
                }

                return HLConfigManager.Configurations.DOConfiguration.TodayMagazineMax;
            }

            int maxQuantity = HLConfigManager.Configurations.ShoppingCartConfiguration.MaxQuantity;

            var allSKUs = CatalogProvider.GetAllSKU(Locale, CurrentWarehouse);
            if (allSKUs != null)
            {
                SKU_V01 mySKU;
                if (allSKUs.TryGetValue(sku, out mySKU))
                {
                    return (mySKU.MaxOrderQuantity > 0 && mySKU.MaxOrderQuantity < maxQuantity)
                               ? mySKU.MaxOrderQuantity
                               : maxQuantity;
                }
            }
            return maxQuantity;
        }

        public void DisplayAPFMessage()
        {
            //Any APF Due messages?
            if (null != ShoppingCart && null != ShoppingCart.RuleResults && ShoppingCart.RuleResults.Count > 0)
            {
                var ruleresults =
                    (from r in ShoppingCart.RuleResults
                     where r.RuleName == "APF Rules" && r.Result == RulesResult.Success
                     select r);
                if (null != ruleresults && ruleresults.Count() > 0)
                {
                    var result = ruleresults.ToList().Find(p => p.Messages !=null && p.Messages.Count > 0);
                    //string apfError = (from msg in ruleresults where msg.Messages.Count > 0 select msg.Messages[0]).First();
                    string apfError = String.Empty;
                    if (result != null)
                    {
                        if (result.Messages.Count > 0)
                        {
                            apfError = result.Messages[0];
                        }
                    }
                    if (!string.IsNullOrEmpty(apfError))
                    {
                        (Master as OrderingMaster).Status.AddMessage(StatusMessageType.Error,
                                                                     apfError);
                    }
                }
            }
        }

        public void DisplayELearningMessage()
        {
            if (HLConfigManager.Configurations.ShoppingCartConfiguration.CheckELearning)
            {
                if (SessionInfo.DsTrainings == null)
                        SessionInfo.DsTrainings = DistributorOrderingProfileProvider.GetTrainingList(DistributorID, CountryCode);

                if (SessionInfo.DsTrainings != null && SessionInfo.DsTrainings.Count > 0 && SessionInfo.DsTrainings.Exists(t => t.TrainingCode == HLConfigManager.Configurations.ShoppingCartConfiguration.TrainingCode && !t.TrainingFlag))
                {
                    var message = PlatformResources.GetGlobalResourceString("ErrorMessage", "RequiredELearning");
                    var listMessages = (Master as OrderingMaster).Status.GetMessages();

                    if (!listMessages.Any(x=>x.MessageText==message.ToString()))
                    {
                        (Master as OrderingMaster).Status.AddMessage(StatusMessageType.Error, message);
                    }
                    
                }
            }
        }

        public void SetNqsMessage()
        {
            if(Locale.Equals("en-MX") && !DistributorOrderingProfileProvider.IsEventQualified(Honors2016EventId, Locale) && !IsEventTicketMode)
            {
                string nqsMessage = PlatformResources.GetGlobalResourceString("ErrorMessage", "NQSMessage");
                (Master as OrderingMaster).Status.AddMessage(StatusMessageType.Error, nqsMessage);
            }
        }

        public void ShowBackorderMessage(List<string> msgs)
        {
            string displaymsg = string.Join("<br>", msgs.ToArray());
            (Master as OrderingMaster).ShowMessage(
                HttpContext.GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                                    "AvailableForBackorder") as string, displaymsg);
        }

        public static string GetSymbol()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration != null)
            {
                return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
            }
            return Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencySymbol;
        }

        private bool ShippingAddressIsInvalid(ShippingInfo shippingInfo)
        {
            if (null == shippingInfo)
            {
                return true;
            }
            else if (null == shippingInfo.Address)
            {
                return true;
            }
            else if (null == shippingInfo.Address.Address)
            {
                return true;
            }
            else if (null == shippingInfo.Address.Address.City && null == shippingInfo.Address.Address.Line1 &&
                     null == shippingInfo.Address.Address.PostalCode)
            {
                return true;
            }

            return false;
        }

        public void NewCart()
        {
            _shoppingCart = ShoppingCartProvider.GetNewUnsavedShoppingCart(DistributorID, Locale);
        }

        /// <summary>
        /// </summary>
        public void RefreshFromService()
        {
            _shoppingCart = ShoppingCartProvider.GetShoppingCartFromService(ShoppingCart.ShoppingCartID, DistributorID,
                                                                            Locale, true);
        }

        public bool CheckPendingOrderStatus(string paymentMethod, string orderNum, bool isCheckoutPage = false)
        {
            var isOrderSubmitted = false;
            try
            {
                var response = Providers.Payments.PaymentGatewayInvoker.CheckOrderStatus(paymentMethod, orderNum);
                if (response != null && response.IsApproved)
                {
                    (new AsyncSubmitOrderProvider()).AsyncSubmitOrder(response, CountryCode, SessionInfo);
                    if (!isCheckoutPage)
                    {
                        CleanupOrder(ShoppingCart, DistributorID);
                        ShoppingCart = null;
                    }
                    SessionInfo.OrderNumber = orderNum;
                    isOrderSubmitted = true;
                }
                else
                {
                    if (!isCheckoutPage)
                    {
                        ShoppingCart.CloseCart();
                        ShoppingCart = null;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CheckPendingOrderStatus error {0}", ex));
            }
            return isOrderSubmitted;
        }

        public void RedirectPendingOrder(string orderNumber, bool isOrderSubmitted)
        {
            var redirect = string.Empty;
            try
            {
                if (isOrderSubmitted)
                {
                    redirect = "~/Ordering/Confirm.aspx?OrderNumber=" + orderNumber + string.Format("&{0}={1}", AttemptLoadSerializedData_Key, AttemptLoadSerializedData_Value_Yes);
                }
                else
                {
                    redirect = "~/Ordering/OrderListView.aspx";
                }
                if (string.IsNullOrEmpty(redirect)) return;
                Response.Redirect(redirect);
                Response.End();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            finally
            {
                if (!string.IsNullOrEmpty(redirect))
                {
                    Response.Redirect(redirect);
                    Response.End();
                }
            }
        }


        private void CleanupOrder(MyHLShoppingCart cart, string distributorId)
        {
            if (!IsPostBack)
            {
                if (PurchasingLimitProvider.RequirePurchasingLimits(distributorId, CountryCode))
                {
                    PurchasingLimitProvider.ReconcileAfterPurchase(cart, distributorId, CountryCode);
                }

                if (cart != null)
                {
                    // take out quantities from inventory
                    ShoppingCartProvider.UpdateInventory(cart, cart.CountryCode, cart.Locale, true);
                }

                cart.CloseCart();
            }
        }

        public void DisplayTrainingMessage()
        {
            string message = string.Empty;
            var orderingMaster = Master as OrderingMaster;
            if (HasNotYetTakenTraining)
            {
                message = string.Format("{0}<br>",
                                                   PlatformResources.GetGlobalResourceString("ErrorMessage", "TrainingMsgWithinElapsedTime"));

                if (orderingMaster != null) orderingMaster.ShowTrainingMessage("", message);
            }
            else if (HasNotYetTakenTrainingAfterElapsedDays)
            {
                message = string.Format("{0}<br>",
                                                   PlatformResources.GetGlobalResourceString("ErrorMessage", "TrainingMsgAfterElapsedTime"));

                if (orderingMaster != null) orderingMaster.ShowTrainingMessage("", message);
            }

        }

        public void DisplayAnnoucement()
        {
            var orderingMaster = Master as OrderingMaster;
            if (orderingMaster != null)
                orderingMaster.ShowAnnouncementMessage();
        }

        public void DisplayNonResidentMessage()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            bool navigatesDirectlyToCOP1 = (this is ShoppingCart && Request.UrlReferrer == null);

            bool isNotified;
            bool.TryParse(HttpContext.Current.Session["isNonResidentNotified"] != null ? HttpContext.Current.Session["isNonResidentNotified"].ToString() : "false", out isNotified);

            if (HLConfigManager.Configurations.DOConfiguration.DisplayNonResidentsMessage && this is ShoppingCart && !isNotified &&
                ShoppingCart.DeliveryInfo != null && ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup && !IsEventTicketMode && !APFDueProvider.IsAPFSkuPresent(ShoppingCart.CartItems) &&
                member != null && member.Value != null && !member.Value.ProcessingCountryCode.Equals(CountryCode))
            {
                if (navigatesDirectlyToCOP1)
                {
                    ShoppingCart.ClearCart();
                }
                HttpContext.Current.Session["isNonResidentNotified"] = true;
                (this.Master as OrderingMaster).DisplayHtml("NonResidentsDisclaimer.html");
            }
        }

        public void DisplayMissingTINMessage()
        {
            if (HLConfigManager.Configurations.DOConfiguration.DisplayMissingTinMessage)
            {
                bool isNotified;
                bool.TryParse(HttpContext.Current.Session["isMissingTINNotified"] != null ? HttpContext.Current.Session["isMissingTINNotified"].ToString() : "false", out isNotified);
                bool hasToDisplay = false;

                if (!isNotified)
                {
                    if (CountryCode == "IN")
                    {
                        var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                        if (member != null)
                        {
                            var tins = DistributorOrderingProfileProvider.GetTinList(member.Value.Id, true, true);
                            hasToDisplay = new[] { "FB01", "FB02", "FB03", "FB04", "FB05", "FBL1", "FBL2", "FBL3", "FBL4", "FBL5", "FBOR" }.Intersect(from t in tins select t.IDType.Key).ToList().Count == 0;
                        }
                    }

                    if (hasToDisplay)
                    {
                        HttpContext.Current.Session["isMissingTINNotified"] = true;
                        (this.Master as OrderingMaster).DisplayHtml("IndiaBifurcationMsg.html");
                    }
                }
            }
        }

        protected bool IsChina
        {
            get { return HLConfigManager.Configurations.DOConfiguration.IsChina; }
        }

        /// <summary>
        /// Sets Bulletin Board Session
        /// </summary>
        [WebMethod]
        public static void SetBulletinBoardSession()
        {
            var o = HttpContext.Current.Session["showBulletinBoard"];
            if (o != null)
            {
                var showBulletinBoard = false;
                bool.TryParse(o.ToString(), out showBulletinBoard);
                HttpContext.Current.Session["showBulletinBoard"] = !showBulletinBoard;
            }
        }

        /// <summary>
        /// Getting localized resource string from different page.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="otherResource">pass-in format ~/Ordering/abc.aspx</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetOtherResourceString(string key, string otherResource, string defaultValue = null)
        {
            var value = HttpContext.GetLocalResourceObject(otherResource, key);
            if (value == null || !(value is string))
            {
                LoggerHelper.Warn(string.Format("Missing local resource object. Key: {0}", key));
                return defaultValue;
            }

            return value as string;
        }

        /// <summary>
        /// If <paramref name="classKey"/> is null, will default load from string.Format("{0}_GlobalResources", HLConfigManager.Platform) .
        /// </summary>
        /// <param name="key"></param>
        /// <param name="classKey">default: string.Format("{0}_GlobalResources", HLConfigManager.Platform)</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetGlobalResourceString(string key, string classKey = null, string defaultValue = null)
        {
            var clsKey = classKey ?? string.Format("{0}_GlobalResources", HLConfigManager.Platform);
            var value = HttpContext.GetGlobalResourceObject(clsKey, key);

            if (value == null || !(value is string))
            {
                LoggerHelper.Warn(string.Format("Missing {0} resource object. Key: {1}", clsKey, key));
                return defaultValue;
            }

            return value as string;
        }

        public string GetRequestURLWithOutPort()
        {
            if (HttpContext.Current.IsDebuggingEnabled)
                return Request.Url.AbsoluteUri;

            return
                HttpContext.Current.Request.Url.AbsoluteUri.Replace(
                    string.Format(":{0}", HttpContext.Current.Request.Url.Port), "");
        }

        public string GetSaveCopyError()
        {
            if ((ShoppingCart.IsSavedCart || ShoppingCart.IsFromCopy) && ShoppingCart.SaveCopyResults != null && ShoppingCart.SaveCopyResults.Any(x => x.RuleName == "PickUpLocationRemoved"))
            {
                return PlatformResources.GetGlobalResourceString("ErrorMessage", "PickUpLocationRemoved");
            }
            return string.Empty;
        }

        public bool HasToDisplaySaveCopyError()
        {
            if (ShoppingCart.DeliveryInfo != null &&
                ShoppingCart.DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.PickupFromCourier)
            {
                var provider = MyHerbalife3.Ordering.Providers.Shipping.ShippingProvider.GetShippingProvider(null);
                if (provider != null)
                {
                    var shippingInfo = provider.GetShippingInfoFromID(ShoppingCart.DistributorID, Locale,
                                                                      ShoppingCart.DeliveryInfo.Option,
                                                                      ShoppingCart.DeliveryInfo.Id, 0);
                    if (shippingInfo == null)
                    {
                        return true;
                    }
                    else
                    {
                        ShoppingCart.SaveCopyResults = new List<ShoppingCartRuleResult>();
                    }
                }
            }
            else
            {
                ShoppingCart.SaveCopyResults = new List<ShoppingCartRuleResult>();
            }
            return false;
        }

        public bool CashOnly()
        {
            //var distributorLoader = new DistributorProfileLoader();
            //var distributorProfile = distributorLoader.Load(new GetDistributorProfileById { Id = DistributorID });
            DistributorOrderingProfile distributorProfile = new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(DistributorID, CountryCode);

            if (distributorProfile != null)
            {
                return distributorProfile.HardCashOnly;
            }
            return false;
        }
        public DateTime? getHapExpirationDate()
        {
            DateTime? expDate = null;
            try
            {
                DistributorOrderingProfile distributorOrderingProfile = new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(DistributorID, CountryCode);
                if (distributorOrderingProfile != null)
                {
                    DistributorOrderingProfileProvider.GetHAPSettings(distributorOrderingProfile);
                    expDate = (distributorOrderingProfile.HAPExpiryDateSpecified && distributorOrderingProfile.HAPExpiryDate != null) ? distributorOrderingProfile.HAPExpiryDate : null;
                    if (expDate != null && expDate <= DateUtils.GetCurrentLocalTime(CountryCode))
                    {
                        expDate = null;
                    }
                }
            }
            catch (Exception ex) { }

            return expDate;
        }

        private void LoadHapOrder(string HapOrderId)
        {
            _shoppingCart = ShoppingCartProvider.GetShoppingCartFromHAPOrder(DistributorID, Locale, HapOrderId);
            SessionInfo.HAPOrderType = _shoppingCart.HAPType == "01" ? "Personal" : "RetailOrder";
        }

        #region Adobe Target Implementation
        public string AdobeTarget_ConvertCartItemsToString(List<ViewModel.Model.DistributorShoppingCartItem> cartItems)
        {
            string skuList = string.Empty;

            if (cartItems != null && cartItems.Count > 0)
            {
                string itemFormat = "{0}_{1}"; // Locale_SKU
                foreach (var item in cartItems)
                {
                    if (!string.IsNullOrEmpty(skuList))
                        skuList += ",";

                    skuList += string.Format(itemFormat, Locale, item.SKU);
                }
            }

            return skuList;
        }
        #endregion

        #region Salesforce PI Implementation
        public string Salesforce_ConvertCartItemsToString(List<ViewModel.Model.DistributorShoppingCartItem> cartItems)
        {
            string skuList = string.Empty;
            if (cartItems != null && cartItems.Count > 0)
            {
                string itemFormat = HLConfigManager.Configurations.DOConfiguration.SalesforceCollectOrderItemFormat;
                foreach (var item in cartItems)
                {
                    if (!string.IsNullOrEmpty(skuList))
                        skuList += ", ";

                    skuList += string.Format(itemFormat, Locale, item.SKU, item.Quantity, item.RetailPrice);
                }
            }
            return skuList;
        }
        #endregion
        public void GetTWSKUQuantitytorestrict()
        {
            if (Locale != null && Locale == "zh-TW")
            {
                SKUQuantityRestrictRequest_V01 req = new SKUQuantityRestrictRequest_V01();

                req.CountryCode = Locale.Substring(3, 2);
                req.Flag = "R";
                var list = OrderProvider.GetOrUpdateSKUsMaxQuantity(req);
                if (list != null && list.Count > 0)
                {
                    foreach (SKUQuantityRestrictInfo i in list)
                    {                        
                        SKU_V01 s;
                        AllSKUS.TryGetValue(i.SKU, out s);
                        if (s != null && s.SKU != null && i.MaxQuantity <= 0 )
                        {
                            s.CatalogItem.InventoryList.ToList().ForEach(
                                x =>
                                {
                                    var val = x.Value as WarehouseInventory_V01;
                                    if (val != null && val.WarehouseCode == i.WarehouseCode)
                                    {
                                        val.QuantityOnHand = 0;
                                        val.QuantityAvailable = 0;
                                        val.IsBlocked = true;
                                    }
                                });
                        }
                        else if (s != null && s.SKU != null && i.MaxQuantity > 0)
                        {
                            s.CatalogItem.InventoryList.ToList().ForEach(
                                x =>
                                {
                                    var val = x.Value as WarehouseInventory_V01;
                                    if (val != null && val.WarehouseCode == i.WarehouseCode)
                                    {
                                        val.QuantityOnHand = i.MaxQuantity;
                                        val.QuantityAvailable = i.MaxQuantity;
                                        val.IsBlocked = false;
                                    }
                                });
                        }
                    }
                    var cacheKey = string.Format("MakeBackorderFalse_{0}", req.CountryCode);
                    var results = HttpRuntime.Cache[cacheKey] as List<SKUQuantityRestrictInfo>;
                    if (results == null)
                    {
                       HttpRuntime.Cache.Insert(cacheKey, list, null, DateTime.Now.AddMinutes(20), Cache.NoSlidingExpiration, 
                       CacheItemPriority.Low,
                       null);
                    }
                }
            }
        }
    }

    [Flags]
    public enum DSVolumeLimit
    {
        DSLimit = 500,
        SCLimit = 2000,
        PQPLimit = 1000,
        QPLimit = 1000,
        FiveQSLimit = 1000
    }

    /// <summary>
    ///     Specify what to do when adding product to the cart
    /// </summary>
    public enum AddingItemOption
    {
        AddItem,
        DeleteBeforeAdd,
        ChangeQuantity,
        ModifyQuantity
    }
}
