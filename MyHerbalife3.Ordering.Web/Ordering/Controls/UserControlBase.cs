using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Security;
using System.Web.UI;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers.FOP;
using HL.Common.Configuration;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class UserControlBase : UserControl
    {
        private bool _FOPEnabled = (Settings.GetRequiredAppSetting<bool>("FOPEnabled", false));
        public string Locale { get; set; }

        public string Email { get; set; }

        public string DistributorID { get; set; }

        public string CountryCode { get; set; }

        public string Level { get; set; }

        public bool FOPEnabled { get { return _FOPEnabled; } }

        public OrderMonth OrderMonth { get; set; }

        public OrderingMaster Master
        {
            get { return ProductsBase.Master as OrderingMaster; }
        }

        public ProductsBase ProductsBase
        {
            get { return (Page as ProductsBase); }
        }

        public SessionInfo SessionInfo
        {
            get
            {
                return ProductsBase.SessionInfo == null
                           ? SessionInfo.GetSessionInfo(DistributorID, Locale)
                           : (Page as ProductsBase).SessionInfo;
            }
        }

        public MyHLShoppingCart ShoppingCart
        {
            get 
            {
                if (IsChina && SessionInfo.UseSessionLessInfo) return SessionInfo.SessionLessInfo.ShoppingCart;

                return ProductsBase.ShoppingCart; 
            }
        }

        public ProductInfoCatalog_V01 ProductInfoCatalog
        {
            get { return ProductsBase.ProductInfoCatalog; }
        }

        public Dictionary<string, SKU_V01> AllSKUS
        {
            get { return ProductsBase.AllSKUS; }
        }

        public IShippingProvider ShippingProvider
        {
            get { return Providers.Shipping.ShippingProvider.GetShippingProvider(CountryCode); }
        }

        public ModalPopupExtenderBase PopupExtender { get; set; }

        public virtual void HidePopup()
        {
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (Page.Master as OrderingMaster != null)
            {
                (Page.Master as OrderingMaster).EventBus.RegisterObject(this);
            }

            if (Page as ProductsBase != null)
            {
                Locale = (Page as ProductsBase).Locale;
                DistributorID = (Page as ProductsBase).DistributorID;
                CountryCode = (Page as ProductsBase).CountryCode;
                Level = (Page as ProductsBase).Level;
                var email = (Page as ProductsBase).PrimaryEmail;
                Email = string.IsNullOrEmpty(email) ? string.Empty : email.Trim();
                OrderMonth = (Page as ProductsBase).OrderMonth;
            }
        }

        public ShippingAddress_V02 CreateAddress()
        {
            return new ShippingAddress_V02
                {
                    Address = new ServiceProvider.ShippingSvc.Address_V01
                        {
                            Country = CountryCode,
                        }
                };
        }

        /// <summary>
        ///     The get price format.
        /// </summary>
        /// <returns>
        ///     The get price format.
        /// </returns>
        protected string getPriceFormat(decimal number)
        {
            return HLConfigManager.Configurations.PaymentsConfiguration.AllowDecimal
                       ? "{0:N2}"
                       : (number == (decimal) 0.0 ? "{0:0}" : "{0:#,###}");
        }

        protected string getAmountString(decimal amount)
        {
            return getAmountString(amount, false);
        }

        protected string getAmountString(decimal amount, bool useAlternateCurrencySymbol)
        {
            string symbol = GetSymbol(useAlternateCurrencySymbol);
            return HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbolPosition ==
                   CheckoutConfiguration.CurrencySymbolLayout.Leading
                       ? (HLConfigManager.Configurations.CheckoutConfiguration.UseUSPricesFormat
                              ? symbol + amount.ToString("N", CultureInfo.GetCultureInfo("en-US"))
                              : symbol + string.Format(ProductsBase.GetPriceFormat(amount), amount))
                       : string.Format(ProductsBase.GetPriceFormat(amount), amount) + symbol;
        }

        protected string GetVolumeString(decimal volumePoints)
        {
            return ProductsBase.GetVolumePointsFormat(volumePoints);
        }

        protected string GetSymbol()
        {
            return GetSymbol(false);
        }

        protected string GetSymbol(bool getAlternateSymbol)
        {
            string currencySymbol = "$";
            if (HLConfigManager.Configurations.CheckoutConfiguration != null)
            {
                currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.CurrencySymbol;
                if (getAlternateSymbol)
                {
                    if (HLConfigManager.Configurations.CheckoutConfiguration.ConvertAmountDue)
                    {
                        currencySymbol = HLConfigManager.Configurations.CheckoutConfiguration.Currency;
                    }
                }
            }

            return currencySymbol;
        }

        protected Control loadPurchasingLimitsControl(bool displayStatic)
        {
            if (SessionInfo.IsHAPMode)
                return null;
           string controlName = HLConfigManager.Configurations.DOConfiguration.PurchasingLimitsControl;
            if (FOPEnabled && PurchaseRestrictionProvider.HasPurchaseRestriction(this.DistributorID, this.DistributorProfileModel.ProcessingCountry, this.Level)) 
            {
                controlName = String.IsNullOrEmpty(controlName) ? "~/Ordering/Controls/PurchasingLimits.ascx" : controlName;
            }
            if (!String.IsNullOrEmpty(controlName))
            {
                var _purchasingLimits = LoadControl(controlName) as IPurchasingLimits;
                _purchasingLimits.HideEmptyListItem = false;
                _purchasingLimits.DisplayStatic = displayStatic;
                return (Control)_purchasingLimits;
            }
            return null;
        }

        protected Control loadHAPControl(bool displayStatic)
        {
            string controlName = "~/Ordering/Controls/HAP/HAPMiniCartControl.ascx";
            if (!String.IsNullOrEmpty(controlName))
            {
                var _hapControl = LoadControl(controlName);
                return (Control)_hapControl;
            }
            return null;
        }


        protected  IPurchasingLimitManager PurchasingLimitManager(string distributorId)
        {
            IPurchasingLimitManagerFactory purchasingLimitManagerFactory = new PurchasingLimitManagerFactory();
            var purchasingLimitManager = purchasingLimitManagerFactory.GetPurchasingLimitManager(distributorId);
            return purchasingLimitManager;
        }

        protected IPurchaseRestrictionManager PurchaseRestrictionManager(string distributorId)
        {
            IPurchaseRestrictionManagerFactory purchaseRestrictionManagerFactory = new PurchaseRestrictionManagerFactory();
            var purchaseRestrictionManager = purchaseRestrictionManagerFactory.GetPurchaseRestrictionManager(distributorId);
            return purchaseRestrictionManager;
        }

        public DistributorProfileModel DistributorProfileModel
        {
            get { return ((MembershipUser<DistributorProfileModel>) Membership.GetUser()).Value; }
        }

        public DistributorOrderingProfile DistributorOrderingProfile
        {
            get { return ProductsBase.DistributorOrderingProfile;  }
        }

        protected bool IsChina
        {
            get { return HLConfigManager.Configurations.DOConfiguration.IsChina; }
        }

        protected string GetGlobalResourceString(string key, string classKey = null, string defaultValue = null)
        {
            var pgBase = this.Page as ProductsBase;
            if (pgBase == null) return null;

            return pgBase.GetGlobalResourceString(key, classKey, defaultValue);
        }
    }
}
