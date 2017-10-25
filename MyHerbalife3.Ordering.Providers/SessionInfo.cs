using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using Payment = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Payment;

namespace MyHerbalife3.Ordering.Providers
{
    [Serializable]
    public class SessionInfo
    {
        #region Consts

        public const string SessionInfoPrefix = "SESSIONINFO_";

        #endregion Consts

        #region Fields

        private List<ShippingAddress_V02> _orderShippingAddresses;
        private List<ShippingAddress_V02> _shippingAddresses;
        private MyHLShoppingCart _shoppingCart;
        private Dictionary<string, string> taxAreaIds;
        private SurveyDetails _surveyDetails;
        public string SelectedPaymentChoice { get; set; }

        public bool OrderConverted { get; set; }

        public bool CustomerOrderAddressWasValid { get; set; }

        public string CustomerOrderNumber { get; set; }

        public int CustomerAddressID { get; set; }

        public bool UseHMSCalc { get; set; }

        public bool ShowAllInventory { get; set; }

        public bool IsEventTicketMode { get; set; }

        public bool IsHAPMode { get; set; }

        public bool HasEventTicket { get; set; }

        //public string DSLevel { get; set; }
        public string BRPF { get; set; }

        public string NationaId { get; set; }

        public string ChangedEmail { get; set; }

        public string OrderNumber { get; set; }

        public bool IsTodaysMagazineInCart { get; set; }

        public string ShippingMethodNameMX { get; set; }

        public string ShippingMethodNameUSCA { get; set; }

        public string OrderMonthString { get; set; }

        public string OrderMonthShortString { get; set; }

        public int SelectedPaymentMethod { get; set; }

        public decimal DiscountForPurchasingLimits { get; set; }

        public SubmitOrderStatus OrderStatus { get; set; }

        public bool ProductAvailabilityRetrieved { get; set; }

        public List<Payment> Payments { get; set; }

        public bool TrainingBreached { get; set; }

        public string SelectedPickupLocationPhone { get; set; }

        public bool HmsPricing { get; set; }

        public string PendingOrderId { get; set; }

        public string LocalPaymentId { get; set; }

        public string TrackingUrl { get; set; }

        public bool HasPromoSku { get; set; }

        public DistributorProfileModel ReplacedPcDistributorProfileModel { get; set; }

        public DistributorOrderingProfile ReplacedPcDistributorOrderingProfile { get; set; }

        public bool IsReplacedPcOrder { get; set; }

        public bool IsSplitted { get; set; }

        public bool CreateHapOrder { get; set; }

        public bool IsNotFirstOrder { get; set; }

        public bool IsFreightExempted { get; set; }

        public List<string> FirstOrderPromoSku { get; set; }
        public int ChinaPromoSKUQuantity { get; set; }

        public List<DistributorTraining_V01> DsTrainings { get; set; }

        public bool LimitsHasModified { get; set; }

        public bool IsVenuzulaShipping { get; set; }
        public bool IsVenuzulaShippingNew { get; set; }
        #region UseSessionLessInfo

        public const string Key_UseSessionLessInfo = "UseSessionLessInfo";

        public bool UseSessionLessInfo
        {
            get
            {
                if (!IsChina) return false;

                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                {
                    return false;
                }

                // this property lifespan should only be within a Request cycle, shouldn't be carry/persist over to next request (different page request)
                // Context cache serve this right.

                var httpCntx = HttpContext.Current;
                if (httpCntx == null) return false;

                var cntxItems = httpCntx.Items;
                if ((cntxItems == null) || !cntxItems.Contains(Key_UseSessionLessInfo)) return false;

                var chk = cntxItems[Key_UseSessionLessInfo];
                if (chk == null) return false;

                bool ret = false;
                bool.TryParse(chk.ToString(), out ret);
                return ret;
            }
            set
            {
                HttpContext.Current.Items[Key_UseSessionLessInfo] = value;
            }
        }

        public string StandAloneDonationError { get; set; }

        public decimal CancelSelfDonation { get; set; }
        public decimal CancelBehalfOfDonation { get; set; }

        public decimal StandAloneDonationNotSubmit { get; set; }
        #endregion

        /// <summary>
        /// As many codes been coded rely on SessionInfo, when time you need to load different data into SessionInfo, use this as alternative to avoid impact.
        /// 1st usage (2015 Jan) - ticket 149783 - to load in order acknowledgement data which is used in Confirm.aspx .
        /// </summary>
        public SessionInfo SessionLessInfo { get; set; }

        private bool IsChina
        {
            get { return HLConfigManager.Configurations.DOConfiguration.IsChina; }
        }

        /// <summary>
        /// Read-only
        /// </summary>
        public string SessionLessInfoOrderNumber
        {
            get { return (SessionLessInfo != null) ? SessionLessInfo.OrderNumber : null; }
        }

        #endregion Fields

        #region Constructor

        public SessionInfo()
        {
            ShowAllInventory = null != HLConfigManager.Configurations &&
                               null != HLConfigManager.Configurations.DOConfiguration && HLConfigManager.Configurations.DOConfiguration.InventoryViewDefault == 0;
            HasEventTicket = true;
            ProductAvailabilityRetrieved = false;
        }

        #endregion Constructor

        public MyHLShoppingCart ShoppingCart
        {
            get
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    return _shoppingCart;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            set
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    _shoppingCart = value;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }
       
        public List<ShippingAddress_V02> ShippingAddresses
        {
            get
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    return _shippingAddresses;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            set
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    _shippingAddresses = value;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public List<ShippingAddress_V02> OrderShippingAddresses
        {
            get
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    return _orderShippingAddresses;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            set
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    _orderShippingAddresses = value;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public ShippingInfo DeliveryInfo { get; set; }
        public OLCDataProvider olcDataprovider { get; set; }

        public bool ConfirmedAddress { get; set; }
        public ServiceProvider.DistributorSvc.Scheme? DsType { get; set; }

        public string HAPOrderType { get; set; }

        public string CO2DOSMSNumber { get; set; }

        public ThreeDSecuredCreditCard ThreeDSecuredCardInfo { get; set; }

        public static string GetSessionKey(string distributorID, string locale)
        {
            return string.Format("{0}_{1}_{2}", SessionInfoPrefix, distributorID, locale);
        }

        public static bool SessionInfoExist(string distributorID, string locale)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
                return false;

            string key = GetSessionKey(distributorID, locale);

            return HttpContext.Current.Session[key] as SessionInfo != null;
        }

        public static SessionInfo GetSessionInfo(string distributorID, string locale)
        {
            var key = GetSessionKey(distributorID, locale);

            if (HttpContext.Current == null)
            {
                return null;
            }

            if (HttpContext.Current.Session != null)
            {
                var info = HttpContext.Current.Session[key] as SessionInfo;
                if (null == info)
                {
                    info = new SessionInfo();
                    //info.UseHMSCalc = HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc;
                    HttpContext.Current.Session[key] = info;
                    key = string.Format("{0}_{1}_{2}", "JustEntered", distributorID, locale);
                    HttpContext.Current.Session[key] = true;
                }
                return info;
            }
            else
            {
                var info = new SessionInfo();
                return info;
            }

        }

        public static void SetSessionInfo(string distributorID, string locale, SessionInfo sessionInfo)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
            {
                return;
            }

            string key = GetSessionKey(distributorID, locale);
            HttpContext.Current.Session[key] = sessionInfo;
        }

        public bool SetTaxAreadId(string postalCode, string taxAreaId)
        {
            try
            {
                if (null == taxAreaIds)
                {
                    taxAreaIds = new Dictionary<string, string>();
                }

                if (string.IsNullOrEmpty(postalCode) || string.IsNullOrEmpty(taxAreaId))
                {
                    return false;
                }

                if (taxAreaIds.ContainsKey(postalCode))
                {
                    return false;
                }
                taxAreaIds.Add(postalCode, taxAreaId);
                return true;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "SetTaxAreadId " + postalCode + " " + taxAreaId);
                return false;
            }
        }

        public string GetTaxAreaId(string postalCode)
        {
            try
            {
                if (string.IsNullOrEmpty(postalCode))
                {
                    return string.Empty;
                }

                if (null == taxAreaIds || taxAreaIds.Count == 0)
                {
                    return string.Empty;
                }

                if (taxAreaIds.ContainsKey(postalCode))
                {
                    return taxAreaIds[postalCode];
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "GetTaxAreaId " + postalCode);
                return string.Empty;
            }
        }

        public bool OrderQueryStatus99BillInprocess { set; get; }
        public SurveyDetails surveyDetails
        {
            get
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    return _surveyDetails;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }

            set
            {
                var locker = new ReaderWriterLockSlim();
                locker.EnterWriteLock();
                try
                {
                    _surveyDetails = value;
                }
                finally
                {
                    locker.ExitWriteLock();
                }
            }
        }

        public void ClearStandAloneDonation()
        {
            if (IsChina)
            {
                if (ShoppingCart != null)
                {
                    if ((ShoppingCart.Totals as ServiceProvider.OrderSvc.OrderTotals_V02) != null)
                        (ShoppingCart.Totals as ServiceProvider.OrderSvc.OrderTotals_V02).Donation = 0;
                    ShoppingCart.Calculate();
                    ShoppingCart.BehalfOfAmount = decimal.Zero;
                    ShoppingCart.BehalfOfSelfAmount = decimal.Zero;
                    StandAloneDonationNotSubmit = decimal.Zero;
                    ShoppingCart.BehalfDonationName = string.Empty;
                    ShoppingCart.BehalfOfContactNumber = string.Empty;
                    CancelSelfDonation = decimal.Zero;
                    CancelBehalfOfDonation = decimal.Zero;
                }
            }
        }
        public bool IsAPFOrderFromPopUp { get; set; }

        public bool IsAdvertisementShowed { get; set;  }

        public bool IsFirstTimeSpainPopup { get; set; }
    }
}