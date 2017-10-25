using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Shared.ViewModel.Requests;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using ProductType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.ProductType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;
using ShippingAddress_V02 = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.ShippingAddress_V02;
using DistributorShoppingCartItem = MyHerbalife3.Ordering.ViewModel.Model.DistributorShoppingCartItem;
using DistributorProfileModel = MyHerbalife3.Shared.ViewModel.Models.DistributorProfileModel;

namespace MyHerbalife3.Ordering.Providers
{
    public class MyHLShoppingCart : ShoppingCart_V02
    {
        public ICatalogProviderLoader CatalogProviderLoader { get; set; }
        public ICatalogInterface CatalogProxy { get; set; }

        #region Constants

        #endregion Constants

        #region Fields

        private ShippingInfo _DeliveryInfo;

        private string error;

        #endregion Fields

        #region Events

        [Publishes(MyHLEventTypes.ShoppingCartDeleted)]
        public event EventHandler ShoppingCartDeleted;

        #endregion Events

        #region Construction/Finalization

        public MyHLShoppingCart()
        {
            PassDSFraudValidation = true;
            IsFromCopy = false;
            InitializaRefObjects();
        }

        public MyHLShoppingCart(ShoppingCart_V02 cart)
        {
            RuleResults = new List<ShoppingCartRuleResult>();
            PassDSFraudValidation = true;
            IsFromCopy = false;
            CopyCart(cart);
            InitializaRefObjects();
        }

        public MyHLShoppingCart(ShoppingCart_V03 cart)
        {
            RuleResults = new List<ShoppingCartRuleResult>();
            PassDSFraudValidation = true;
            IsFromCopy = false;
            CopyCart(cart);
            InitializaRefObjects();
        }

        public MyHLShoppingCart(ShoppingCart_V04 cart)
        {
            RuleResults = new List<ShoppingCartRuleResult>();
            PassDSFraudValidation = true;
            IsFromCopy = false;
            CopyCart(cart);
            InitializaRefObjects();
        }

        public MyHLShoppingCart(ShoppingCart_V05 cart)
        {
            RuleResults = new List<ShoppingCartRuleResult>();
            PassDSFraudValidation = true;
            IsFromCopy = false;
            CopyCart(cart);
            InitializaRefObjects();
        }

        #endregion Construction/Finalization

        #region Properties

        public string ErrorCode { get; set; }

        public bool ShoppingCartIsFreightExempted { get; set; }

        public bool IgnorePromoSKUAddition { get; set; }

        public string SrPlacingForPcOriginalMemberId  { get; set; }

        /// <summary>
        ///     Copy invoice address.
        /// </summary>
        public MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.Address_V01 CopyInvoiceAddress { get; set; }

        /// <summary>
        ///     Copy invoice name.
        /// </summary>
        public ServiceProvider.CatalogSvc.Name_V01 CopyInvoiceName { get; set; }

        /// <summary>
        ///     Copy invoice phone.
        /// </summary>
        public string CopyInvoicePhone { get; set; }

        /// <summary>
        ///     Copy invoice status.
        /// </summary>
        public CopyInvoiceStatus CopyInvoiceStatus { get; set; }

        public CopyMemberInvoiceStatus CopyMemberInvoiceStatus { get; set; }

        /// <summary>
        ///     SKUs removed
        /// </summary>
        public List<string> SKUsRemoved { get; set; }

        /// <summary>
        ///     To specify that is from an invoice.
        /// </summary>
        public bool IsFromInvoice { get; set; }

        public bool IsFromCopy { get; set; }

        //public bool DeliverInfoRemoved { get; set; }

        public string CountryCode { get; set; }

        public bool HasYearlyPromoTaken { get; set; }
        public ServiceProvider.DistributorSvc.Scheme? DsType { get; set; }

        public class QRCode
        {
            public string memberId { get; set; }
            public string orderNumber { get; set; }
            public string orderDate { get; set; }
            public string eventId { get; set; }
            public List<Ticket> tickets { get; set; }

        }
        public class Ticket
        {
            public string ticketSKU { get; set; }
            public int quantity { get; set; }
           

        }

        public ShippingInfo DeliveryInfo
        {
            get { return _DeliveryInfo; }
            set
            {
                try
                {
                    _DeliveryInfo = value;
                    if (null != value)
                    {
                        value.FreightCodeChanged += value_FreightCodeChanged;
                    }
                    //Restored this due to issues like Pickup with FED for US
                    //Need to understand why it was removed and if restoring it causes repercussions
                    ShoppingCartProvider.UpdateShoppingCart(this);
                    // this update is necessary? [This line commented out by Sophia on 7/8]
                    // Since dev principal team wants this line here, I will add a initialized parameter to the method to ignore it in some cases.
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Exception occurred updating ShoppingCart.DeliveryInfo in database. Details:\r\n{0}",
                            ex.Message));
                }
            }
        }

        public List<DistributorShoppingCartItem> ShoppingCartItems { get; set; }

        public OrderTotals Totals { get; set; }

        public bool ShippingInfoSelected { get; set; }

        public string GreetingMsg { get; set; }

        public bool PassDSFraudValidation { get; set; }

        public string DSFraudValidationError { get; set; }

        public string EmailAddress { get; set; }

        public string DeliveryDate { get; set; }

        public string SMSNotification { get; set; }

        public string InvoiceOption { get; set; }

        public string SelectedDSSubType
        {
            get { return OrderSubType; }
            set
            {
                OrderSubType = value;
                if (HLConfigManager.Configurations.DOConfiguration.SaveDSSubType)
                    ShoppingCartProvider.UpdateShoppingCart(this);
            }
        }

        public decimal EarningsInCart
        {
            get
            {
                decimal CartEarnings = 0.0M;
                foreach (ShoppingCartItem_V01 CartItem in CartItems)
                {
                    var item = CatalogProvider.GetCatalogItem(CartItem.SKU, CountryCode);
                    decimal Discount = 0.0M;
                    if (SessionInfo.GetSessionInfo(DistributorID, Locale) != null)
                    {
                        Discount = SessionInfo.GetSessionInfo(DistributorID, Locale).DiscountForPurchasingLimits;
                        if (Discount == 0.0M)
                        {
                            if (Totals != null)
                            {
                                Discount = (Totals as OrderTotals_V01).DiscountPercentage;
                            }
                        }
                    }
                    if (item != null && Totals != null)
                    {
                        CartEarnings += (item.EarnBase) * CartItem.Quantity * Discount / 100;
                    }
                }

                return CartEarnings;
            }
        }

        public decimal ProductEarningsInCart
        {
            get
            {
                decimal CartEarnings = 0.0M;
                foreach (ShoppingCartItem_V01 CartItem in CartItems)
                {
                    var item = CatalogProvider.GetCatalogItem(CartItem.SKU, CountryCode);
                    if (item.ProductType == ProductType.Product)
                    {
                        decimal Discount = 0.0M;
                        if (SessionInfo.GetSessionInfo(DistributorID, Locale) != null)
                        {
                            Discount = SessionInfo.GetSessionInfo(DistributorID, Locale).DiscountForPurchasingLimits;
                            if (Discount == 0.0M)
                            {
                                if (Totals != null)
                                {
                                    Discount = (Totals as OrderTotals_V01).DiscountPercentage;
                                }
                            }
                        }
                        if (item != null && Totals != null)
                        {
                            CartEarnings += (item.EarnBase) * CartItem.Quantity * Discount / 100;
                        }
                    }
                }

                return CartEarnings;
            }
        }

        public decimal ProductDiscountedRetailInCart
        {
            get
            {
                decimal cartDiscountedRetail = 0.0M;
                try
                {
                    //One calc from OrderTotals

                    var skus = (from s in CartItems where s.SKU != null select s.SKU).ToList();
                    var allItems = CatalogProvider.GetCatalogItems(skus, CountryCode);
                    if (null != allItems && allItems.Count > 0)
                    {
                        cartDiscountedRetail = (from t in (Totals as OrderTotals_V01).ItemTotalsList
                                                from c in allItems.Values
                                                where
                                                    (c as CatalogItem_V01).ProductType == ProductType.Product &&
                                                    c.SKU == (t as ItemTotal_V01).SKU
                                                select (t as ItemTotal_V01).DiscountedPrice).Sum();
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        "Exception occurres while getting ProductDiscountedRetailInCart\r\n" + ex.Message);
                }

                return cartDiscountedRetail;
            }
        }

        public decimal VolumeInCart
        {
            get
            {
                decimal CartVolume = 0.0M;
                OrderTotals_V01 totlas = Totals as OrderTotals_V01;
                if (null != totlas && totlas.ItemTotalsList != null && totlas.AmountDue != 0.0M)
                {
                    CartVolume = totlas.VolumePoints;
                }
                else
                {
                    foreach (ShoppingCartItem_V01 CartItem in CartItems)
                    {
                        var item = CatalogProvider.GetCatalogItem(CartItem.SKU, CountryCode);
                        if (item != null)
                        {
                            CartVolume += (item.VolumePoints) * CartItem.Quantity;
                        }
                    }
                }

                return CartVolume;
            }
        }

        public decimal ProductVolumeInCart
        {
            get
            {
                decimal CartVolume = 0.0M;
                foreach (ShoppingCartItem_V01 CartItem in CartItems)
                {
                    var item = CatalogProvider.GetCatalogItem(CartItem.SKU, CountryCode);
                    if (item.ProductType == ProductType.Product)
                    {
                        if (item != null)
                        {
                            CartVolume += (item.VolumePoints) * CartItem.Quantity;
                        }
                    }
                }

                return CartVolume;
            }
        }

        public decimal ProductPromoVolumeInCart
        {
            get
            {
                decimal CartVolume = 0.0M;
                foreach (ShoppingCartItem_V01 CartItem in CartItems)
                {
                    var item = CatalogProvider.GetCatalogItem(CartItem.SKU, CountryCode);
                    if (item.ProductType != ProductType.Literature)
                    {
                        if (item != null)
                        {
                            CartVolume += (item.VolumePoints) * CartItem.Quantity;
                        }
                    }
                }

                return CartVolume;
            }
        }

        public bool HastakenSrPromotion
        {
            get
            {
                bool val = false;
                var SRPromoSku = Settings.GetRequiredAppSetting("ChinaSRPromo", string.Empty).Split('|');
                var itemsInBoth =
                                    this.CartItems.Select(c => c.SKU)
                                        .Intersect(SRPromoSku, StringComparer.OrdinalIgnoreCase);

                if (itemsInBoth.Any())
                {
                    val = true;
                }
                return val;
            }

        }
        public bool HastakenBadgePromotion
        {
            get
            {
                bool val = false;
                var SRPromoSku = Settings.GetRequiredAppSetting("ChinaBadgePromo", string.Empty).Split(',');
                var itemsInBoth =
                                    this.CartItems.Select(c => c.SKU)
                                        .Intersect(SRPromoSku, StringComparer.OrdinalIgnoreCase);

                if (itemsInBoth.Any())
                {
                    val = true;
                }
                return val;
            }

        }

        public bool HastakenNewSrpromotion
        {
            get
            {
                return CartItems.Any(s => NewSrPromotionSkus.Contains(s.SKU));
            }
        }

        private IEnumerable<string> s_newSrPromotionSkus;
        public IEnumerable<string> NewSrPromotionSkus
        {
            get
            {
                if(s_newSrPromotionSkus == null)
                {
                    s_newSrPromotionSkus = Settings.GetRequiredAppSetting("NewSRPromotionSkuList", string.Empty).Split(',');
                }

                return s_newSrPromotionSkus;
            }
        }

        public bool HastakenSrPromotionGrowing
        {
            get
            {
                
               
                var SRPromoSku = Settings.GetRequiredAppSetting("ChinaSRPromoPhase2", string.Empty).Split('|');
                var itemsInBoth =
                                    this.CartItems.Select(c => c.SKU)
                                        .Intersect(SRPromoSku, StringComparer.OrdinalIgnoreCase);

                if (itemsInBoth.Any() && ChinaPromotionProvider.IsEligibleForSRQGrowingPromotion(this, HLConfigManager.Platform))
                {
                    return true;
                }
                return false;
            }

        }
        public bool HastakenSrPromotionExcelnt
        {
            get
            {
               
                var SRPromoSku = Settings.GetRequiredAppSetting("ChinaSRPromoPhase2", string.Empty).Split('|');
                var itemsInBoth =
                                    this.CartItems.Select(c => c.SKU)
                                        .Intersect(SRPromoSku, StringComparer.OrdinalIgnoreCase);

                if (itemsInBoth.Any() && ChinaPromotionProvider.IsEligibleForSRQExcellentPromotion(this, HLConfigManager.Platform))
                {
                   return true;
                }
                return false;
            }

        }
        public decimal pcLearningPointOffSet { get; set; }

        private Nullable<int> _changeRate;
        public int ChangeRate
        {
            get
            {
                if (!_changeRate.HasValue)
                {
                    _changeRate = OrderProvider.GetAccumulatedPCLearningPoint(DistributorID, "changeRate");
                }

                return _changeRate.Value;
            }
        }
        private Nullable<int> _changeETORate;
        public int ETOChangeRate
        {
            get
            {
                if (!_changeETORate.HasValue)
                {
                    _changeETORate = OrderProvider.GetAccumulatedETOLearningPoint(EtoSku, DistributorID, "ETOchangeRate");
                }

                return _changeETORate.Value;
            }
        }

        public string EtoSku
        {
            get
            {

                if (OrderCategory == OrderCategoryType.ETO &&
                    CartItems != null &&
                    CartItems.Any())
                {
                    try
                    {
                        return CartItems.First().SKU;
                    }
                    catch
                    {
                        return String.Empty;
                    }
                }

                return String.Empty;
            }
        }

        private Nullable<int> _pcLearningPoint;
        public decimal PCLearningPoint
        {
            get
            {
                if (!_pcLearningPoint.HasValue)
                {
                    _pcLearningPoint = OrderProvider.GetAccumulatedPCLearningPoint(DistributorID, "pcpoint");
                }

                return _pcLearningPoint.Value;
            }
        }
        



        public decimal AmountduepriorpcLearningoffset { get; set; } 
        //public string CurrentMonthVolume { get; set; }

        //public string RemainingVolume { get; set; }
        public HLShoppingCartEmailValues EmailValues { get; set; }
        
        public bool IsSavedCart { get; set; }

        public string CartName { get; set; }

        public string OrderNumber { get; set; }

        public DateTime OrderDate { get; set; }

        public string CourierName { get; set; }

        public int TodaysMagaZineQuantity { get; set; }

        /// <summary>
        ///     FOr the case of StandAloneAPF, keep the origal one, so we get correct inventory
        /// </summary>
        public string ActualWarehouseCode { get; set; }

        public bool IsPromoDiscarted { get; set; }
        public string PromoSkuDiscarted { get; set; }

        public bool IsPromoNotified { get; set; }

        public bool DisplayPromo { get; set; }

        public int PromoQtyToAdd { get; set; }

        // added for china do
        public int OrderHeaderID { get; set; }
        public bool IsFirstOrderChecked { get; set; }
        public List<MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionType> FirstOrderPromotionTypes { get; set; }

        // FOP
        public int OrderMonth { get; set; }
        public ShoppingCartItemList ItemsBeingAdded { get; set; }
        public bool OnCheckout { get; set; }
        // END FOP

        // Split Order
        public bool IsSplit { get; set; }

        // Kount - SessionId
        public string FraudControlSessonId { get; set; }

        /// <summary>
        ///  MPC Fraud
        /// </summary>
        public bool HoldCheckoutOrder { get; set; }

        private void value_FreightCodeChanged(object sender, EventArgs e)
        {
            ShoppingCartProvider.UpdateShoppingCart(this);
        }

        public bool IsResponsive { get; set; }

        public int HAPScheduleDay { get; set; }

        public string HAPType { get; set; }

        /// <summary>
        /// HAP Action
        /// </summary>
        public string HAPAction { get; set; }
        public DateTime HAPExpiryDate { get; set; }

        public List<ShoppingCartRuleResult> SaveCopyResults { get; set; }
        //On Behalf of downline Donation properties
        public string BehalfDonationName { get; set; }
        public string BehalfOfContactNumber { get; set; }
        public string BehalfOfMemberId { get; set; }
        public decimal BehalfOfSelfAmount { get; set; }
        public decimal BehalfOfAmount { get; set; }
        public DeliveryOptionType PcLearningDeliveryoption { get; set; }
        //End of On Behalf of downline Donation properties

        public string AltPhoneNumber { get; set; }
        public string ReplaceAltPhoneNumber { get; set; }

        public string TaxPersonalId { get; set; }
        public bool HasBrochurePromotion { get; set; }
        public bool HerbalifeFastPickup { get; set; }
        #endregion Properties

        #region Public methods

        public void SetDiscountForLimits(decimal Discount)
        {
            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
            currentSession.DiscountForPurchasingLimits = Discount;
            SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
        }

        public void AddItemsToCart(List<ShoppingCartItem_V01> items)
        {
            AddItemsToCart(items, false);
            
        }

        private bool updateQuantity(ShoppingCartItem_V01 newItem)
        {
            var cartItem = ShoppingCartItems.Find(s => s.SKU == newItem.SKU);
            var oldItem = CartItems.Find(s => s.SKU == newItem.SKU);

            if (oldItem != null)
            {
                oldItem.Updated = DateTime.Now;
                oldItem.Quantity += newItem.Quantity;
                oldItem.PartialBackordered = newItem.PartialBackordered;
            }
            else
            {
                CartItems.Add(newItem);
            }
            if (cartItem != null)
            {
                cartItem.Quantity += newItem.Quantity;
                cartItem.Updated = DateTime.Now;
            }
            else
            {
                var newCartItem = CreateShoppingCartItem(newItem);
                if (newCartItem != null)
                {
                    ShoppingCartItems.Add(newCartItem);
            }
            }
            return cartItem == null && oldItem == null;
        }

        public void AddItemsToCart(List<ShoppingCartItem_V01> items, bool suppressRules)
        {
            bool bFOP = Settings.GetRequiredAppSetting<bool>("FOPEnabled", false);
            if (bFOP)
            {
                AddItemsToCart2(items, suppressRules);
                return;
            }
            if (items.Count == 0)
                return;
            var shoppingCartRuleResults = new List<ShoppingCartRuleResult>();
            foreach (ShoppingCartItem_V01 item in items)
            {
                if (item.Quantity == 0)
                {
                    continue;
                }
                //bool isNew = true;
                if (!suppressRules)
                {
                    var cartRuleResults = ShoppingCartProvider.processCart(this, item,
                                                                           ShoppingCartRuleReason
                                                                               .CartItemsBeingAdded);
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && cartRuleResults.Any(c => c.Result == RulesResult.Failure && c.RuleName == "ETO Rules"))
                    {
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Failure));
                        var currentitem = this.CurrentItems.FirstOrDefault(x => x.SKU == item.SKU);
                        if (currentitem !=null)
                        {
                            item.Quantity = currentitem.Quantity;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else if (cartRuleResults.Any(c => c.Result == RulesResult.Failure))
                    {
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina && cartRuleResults.Any(c => c.Result == RulesResult.Failure && c.RuleName == "PurchasingLimits Rules"))
                        {
                            item.Quantity = 0;
                        }
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Failure));
                        //if (HLConfigManager.Configurations.DOConfiguration.IsChina && cartRuleResults.Any(r => r.Result == RulesResult.Failure && r.RuleName == "PurchasingLimits Rules"))
                        //    updateQuantity(item);
                        continue;
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && cartRuleResults.Any(c => c.Result == RulesResult.Feedback && c.RuleName == "ETO Rules"))
                    {
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Feedback));
                        continue;
                    }
                    else if (cartRuleResults.Any(c => c.Result == RulesResult.Feedback))
                    {
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Feedback));
                    }
                    if (HLConfigManager.Configurations.DOConfiguration.IsChina && cartRuleResults.Any(c => c.Result == RulesResult.Unknown && c.RuleName == "ETO Rules"))
                    {
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Unknown));
                        var currentitem = this.CurrentItems.FirstOrDefault(x => x.SKU == item.SKU);
                        if (currentitem != null)
                        {
                            item.Quantity = currentitem.Quantity;
                }
                    }
                }
                if (ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item, ShoppingCartID))
                {
                    updateQuantity(item);
                    //if ((isNew = updateQuantity(item)) == true)
                    //{
                    //    CartItems.Add(item);
                    //    ShoppingCartItems.Add(CreateShoppingCartItem(item));
                    //}

                    // This validate the limits when more than 1 sku is added at the same time
                    Totals = Calculate();
                }
            }

            Totals = Calculate();
            if (!suppressRules)
            {
                CurrentItems.AddRange(items);
                var ruleResults = HLRulesManager.Manager.ProcessCart(this,
                                                                     ShoppingCartRuleReason
                                                                         .CartItemsAdded);
                if (ruleResults.Any(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback))
                {
                    shoppingCartRuleResults.AddRange(
                        ruleResults.Where(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback));
                }
                RuleResults = shoppingCartRuleResults;
            }

            ClearMyHL3ShoppingCartCache();
        }

        public void AddItemsToCart2(List<ShoppingCartItem_V01> items, bool suppressRules)
        {
            if (items.Count == 0)
                return;

            var shoppingCartRuleResults = new List<ShoppingCartRuleResult>();
            List<ShoppingCartItem_V01> itemsToAdd = new List<ShoppingCartItem_V01>();
            this.ItemsBeingAdded = new ShoppingCartItemList();

            foreach (ShoppingCartItem_V01 item in items)
            {
                if (item.Quantity == 0)
                {
                    continue;
                }
                                
                this.ItemsBeingAdded.Add(item);
                List<ShoppingCartRuleResult> cartRuleResults;
                if (!suppressRules)
                {
                    cartRuleResults = ShoppingCartProvider.processCart(this, item,
                                                                           ShoppingCartRuleReason
                                                                               .CartItemsBeingAdded);
                    if (cartRuleResults.Any(c => c.Result == RulesResult.Failure))
                    {
                        var messagesA = shoppingCartRuleResults.Select(x => x.Messages[0]);
                        //var messagesB = cartRuleResults.Select(x => x.Messages[0]);
                        var messagesB= (from itemsB in cartRuleResults where itemsB.Messages != null && itemsB.Messages.Count > 0 select itemsB.Messages[0]).ToList();
                        if (!messagesA.Intersect(messagesB.Select(b => b)).Any())
                        {
                            shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Failure));
                        }
                        continue;
                    }
                    if (cartRuleResults.Any(c => c.Result == RulesResult.Feedback))
                    {
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Feedback));
                    }
                    itemsToAdd.Add(item);
                }
                else
                {
                    itemsToAdd.Add(item);
                }
            }
            if (itemsToAdd.Count > 0)
            {
                List<ShoppingCartRuleResult> cartRuleResults;
                if (!suppressRules)
                {
                    cartRuleResults = ShoppingCartProvider.processCart(this, itemsToAdd, ShoppingCartRuleReason.CartItemsBeingAdded);
                    if (this.OrderCategory == OrderCategoryType.HSO)
                    {
                        ServerRulesManager.Instance.ValidateSKUForHAP(this, this.Locale, itemsToAdd);
                        
                    }
                    if (cartRuleResults.Any(c => c.Result == RulesResult.Failure))
                    {
                        itemsToAdd.Clear();
                        shoppingCartRuleResults.AddRange(cartRuleResults.Where(c => c.Result == RulesResult.Failure));
                        itemsToAdd.AddRange(this.ItemsBeingAdded);
                    }
                    ItemsBeingAdded.Clear();
                }
                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    if (ShoppingCartProvider.InsertShoppingCartItemList(DistributorID, itemsToAdd, ShoppingCartID))
                    {
                        foreach (var i in itemsToAdd)
                        {
                            updateQuantity(i);
                        }
                    }
                }
                else
                {
                    foreach (var i in itemsToAdd)
                    {
                        if (ShoppingCartProvider.InsertShoppingCartItem(DistributorID, i, ShoppingCartID))
                        {
                            updateQuantity(i);
                        }
                    }
                }
                
            }
            if (itemsToAdd.Count > 0)
            {
                Totals = Calculate();
                if (!suppressRules)
                {
                    CurrentItems.AddRange(itemsToAdd);
                    var ruleResults = HLRulesManager.Manager.ProcessCart(this,
                                                                         ShoppingCartRuleReason
                                                                             .CartItemsAdded);
                    if (ruleResults.Any(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback))
                    {
                        shoppingCartRuleResults.AddRange(
                            ruleResults.Where(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback));
                    }
                    RuleResults = shoppingCartRuleResults;
                }
            }
            else
                RuleResults = shoppingCartRuleResults;

            ClearMyHL3ShoppingCartCache();
            SetShoppingCartModuleCache(this);
        }

        private void ClearMyHL3ShoppingCartCache()
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

        private void SetShoppingCartModuleCache(MyHLShoppingCart _shoppingCart)
        {
            var cartWidgetSource = new CartWidgetSource();
            cartWidgetSource.SetCartWidgetCache(_shoppingCart);
        }

        public List<string> DeleteItemsFromCart(List<string> itemsToRemove)
        {
            return DeleteItemsFromCart(itemsToRemove, false);
        }

        public void removeItem(string sku)
        {
            CartItems.RemoveAll(
                delegate(ShoppingCartItem_V01 x) { return x.SKU == sku; });
                ShoppingCartItems.RemoveAll(
                    delegate(DistributorShoppingCartItem x) { return x.SKU == sku; });
        }

        public List<string> DeleteItemsFromCart(List<string> itemsToRemove, bool suppressRules)
        {
            var skus = itemsToRemove;
            if (itemsToRemove == null) // skus may be null
            {
                skus = (from s in CartItems select s.SKU).ToList<string>();
            }

            if (!suppressRules)
            {
                CurrentItems = new ShoppingCartItemList();
                CurrentItems.AddRange(from k in skus select new ShoppingCartItem_V01() { ID = 0, SKU = k, Quantity = 1, Updated = DateTime.Now });
                RuleResults = HLRulesManager.Manager.ProcessCart(this, ShoppingCartRuleReason.CartItemsBeingRemoved);

                var ruleresults =
                    (from results in RuleResults
                     where results.RuleName == "APF Rules" && (results.Result == RulesResult.Recalc || results.Result == RulesResult.Failure)
                     select results);

                    if (null != ruleresults && ruleresults.Count() > 0)
                    {
                        var apfSKUs = from sku in CurrentItems where APFDueProvider.IsAPFSku(sku.SKU) select sku;
                        if (apfSKUs != null && apfSKUs.Count() > 0)
                        {
                            CurrentItems.Remove(apfSKUs.First());
                        }
                    }
                
                skus = (from s in CurrentItems select s.SKU).ToList();
                CurrentItems.Clear();
                if (skus.Count() == 0)
                    return skus;
            }
            //skus is ths list actually should be deleted
            if (ShoppingCartProvider.DeleteShoppingCart(this, skus))
            {
                if (skus == null)
                {
                    CartItems.Clear();
                    ShoppingCartItems.Clear();
                }
                else
                {
                     Array.ForEach(skus.ToArray(), a => removeItem(a));
                     this.HasYearlyPromoTaken = false;
                    
                }
                Totals = Calculate();
            }
            if (!suppressRules)
            {
                RuleResults = HLRulesManager.Manager.ProcessCart(this, ShoppingCartRuleReason.CartItemsRemoved);
            }

            return skus;
        }

        //public MyHLShoppingCart CopyCart(bool isDraft, string cartName)
        //{
        //    MyHLShoppingCart newCart = null;
        //    try
        //    {
        //        newCart = ShoppingCartProvider.InsertShoppingCart(this.DistributorID, this.Locale, this.OrderCategory, this.DeliveryInfo, isDraft, cartName);
        //        if (newCart != null)
        //        {
        //            if (CartItems != null)
        //            {
        //                foreach (ShoppingCartItem_V01 item in this.CartItems)
        //                {
        //                    ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item, newCart.ShoppingCartID);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerHelper.Error(string.Format("Error occurred copy a Cart. Cart Name is {0}.\r\n{1}", cartName, ex.Message), "ShoppingCart");
        //    }
        //    return newCart;
        //}
        /// <summary>
        ///     CopyCart
        /// </summary>
        /// <param name="isDraft"></param>
        /// <param name="cartName"></param>
        /// <returns></returns>
        /// <summary>
        ///     CopyCart
        /// </summary>
        /// <param name="isDraft"></param>
        /// <param name="cartName"></param>
        /// <param name="shippingAddressID"></param>
        /// <param name="deliveryOptionID"></param>
        /// <param name="deliveryOption"></param>
        /// <returns></returns>
        public MyHLShoppingCart CopyCartWithShippingInfo(bool isDraft,
                                                         string cartName,
                                                         int shippingAddressID,
                                                         int deliveryOptionID,
                                                         DeliveryOptionType deliveryOption)
        {
            MyHLShoppingCart newCart = null;
            try
            {
                newCart = ShoppingCartProvider.InsertShoppingCart(DistributorID, Locale, OrderCategory,
                                                                  DeliveryInfo, isDraft, cartName);
                if (newCart != null)
                {
                    if (CartItems != null)
                    {
                        newCart.CartItems = new ShoppingCartItemList();
                        newCart.ShoppingCartItems = new List<DistributorShoppingCartItem>();
                        foreach (ShoppingCartItem_V01 item in CartItems)
                        {
                            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                            {
                                ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item, newCart.ShoppingCartID);
                            }
                            else
                            {
                            if (!item.IsPromo)
                                    ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item,
                                                                                newCart.ShoppingCartID);
                            }
                    }
                }
            }

                var dOption = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), deliveryOption.ToString());
                newCart.UpdateShippingInfo(shippingAddressID, deliveryOptionID, dOption);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Error occurred copy a Cart. Cart Name is {0}.\r\n{1}", cartName, ex.Message));
            }
            return newCart;
        }

        public bool CloseCart()
        {
            return CloseCart(false);
        }

        public bool CloseCart(bool suppressRules)
        {
            bool closeCartStatus = true;
            //var ruleResults = new List<ShoppingCartRuleResult>();
            if (suppressRules)
            {
                closeCartStatus = ShoppingCartProvider.DeleteShoppingCart(this, null);
            }
            else
            {
                //Allow clients to react to this - ie non-web clients can clean up.
                HLRulesManager.Manager.ProcessCart(this, ShoppingCartRuleReason.CartBeingClosed);
                //We're not reacting to ruleresults for this operation as there is no precedent to stop cart closure
                try
                {
                    closeCartStatus = ShoppingCartProvider.DeleteShoppingCart(this, null);
                    HLRulesManager.Manager.ProcessCart(this, ShoppingCartRuleReason.CartClosed);
                    ShoppingCartProvider.processCart(this, new List<ShoppingCartItem_V01>(), ShoppingCartRuleReason.CartClosed);
                    //Same here
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("Error occurred Closing a Cart. Id is {0}.\r\n{1}", ShoppingCartID,
                                      ex.Message));
                }
            }
            return closeCartStatus;
        }

        public void ClearCart()
        {
            if (CartItems != null)
            {
                DeleteItemsFromCart((from i in CartItems select i.SKU).ToList<string>());
                if (null != ShoppingCartDeleted)
                {
                    ShoppingCartDeleted(this, new ShoppingCartEventArgs(this));
                }
            }
            ClearMyHL3ShoppingCartCache();
        }

        /// <summary>
        ///     call by UI to update shipping infomation
        /// </summary>
        /// <param name="shippingAddressID"></param>
        /// <param name="deliveryOptionID"></param>
        /// <param name="optionType"></param>
        public void UpdateShippingInfo(int shippingAddressID, int deliveryOptionID, ServiceProvider.ShippingSvc.DeliveryOptionType optionType)
        {
            string previousFreightCode = DeliveryInfo != null ? DeliveryInfo.FreightCode : string.Empty;
            int oldShippingAddressID = DeliveryInfo != null ? DeliveryInfo.Address.ID : -1;
            var oldAddress = DeliveryInfo != null ? DeliveryInfo.Address.Address : null;
            var oldDeliveryInfo = DeliveryInfo;
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
            var oType = (ServiceProvider.CatalogSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.CatalogSvc.DeliveryOptionType), optionType.ToString());
            if (shippingProvider.UpdateShippingInfo(ShoppingCartID, OrderCategory, oType,
                                                    deliveryOptionID, shippingAddressID))
            {
                LoadShippingInfo(deliveryOptionID, shippingAddressID, oType);
                var newDeliveryInfo = DeliveryInfo;
                var ListDeliveryInfo = new List<ShippingInfo>();
                ListDeliveryInfo.Add(oldDeliveryInfo);
                ListDeliveryInfo.Add(newDeliveryInfo);
                if (DeliveryInfo != null)
                {

                    DeliveryInfo.Option = optionType;
                    //if (DeliveryInfo.WarehouseCode != previousWarehouse)
                    //{
                    //CatalogProvider.GetProductAvailability(CatalogProvider.GetProductInfoCatalog(Locale, this.DistributorID, DeliveryInfo.WarehouseCode), Locale, this.DistributorID, DeliveryInfo.WarehouseCode);
                    //OnWarehouseChanged(this, null);
                    //}
                    if (shouldRecalculate(previousFreightCode, oldAddress))
                    {
                        Calculate();
                        //QuoteRetrieved(this, null);
                    }
                    if (optionType == ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping)
                    {
                        /// existing address was updated, check if user has picked freight code
                        /// if so, set it back 'cause it may be overriden
                        if (oldShippingAddressID == DeliveryInfo.Address.ID && !string.IsNullOrEmpty(FreightCode))
                        {
                            if (FreightCode != DeliveryInfo.FreightCode)
                            {
                                var lstDeliveryOption =
                                    shippingProvider.GetDeliveryOptionsListForShipping(CountryCode, Locale,
                                                                                       DeliveryInfo.Address);
                                if (lstDeliveryOption != null && lstDeliveryOption.Count != 0)
                                {
                                    // if this is a valid freight
                                    if (lstDeliveryOption.Exists(l => l.FreightCode == FreightCode))
                                    {
                                        DeliveryInfo.FreightCode = FreightCode;
                                        ShoppingCartProvider.UpdateShoppingCart(this);
                                    }
                                }
                            }
                        }
                        #region Log PreviousAddress
                        if (HLConfigManager.Configurations.DOConfiguration.IsChina && Settings.GetRequiredAppSetting("LogShipping", "false").ToLower() == "true")
                        {
                            Logger.SetLogWriter(new LogWriterFactory().Create(), false);
                            LogEntry entry = new LogEntry();
                            entry.Message = OldNewAddressSerialization(ListDeliveryInfo);
                            Logger.Write(entry, "SelectedShippingAddress");
                        }
                       
                        #endregion
                    }
                }
            }
        }



        private static string OldNewAddressSerialization(List<ShippingInfo> ListDeliveryInfo)
        {
            var sb = new StringBuilder();
            foreach (var DeliveryInfo in ListDeliveryInfo)
            {
                if (DeliveryInfo != null)
                {
                    var address = DeliveryInfo.Address.Address as ServiceProvider.ShippingSvc.Address_V01;
                    if (address != null)
                    {

                        sb.Append(LogObjectTo(address, "Address Details************"));
                        sb.Append(LogObjectTo(DeliveryInfo, "DeliveryInfo"));
                    
                    }
                }
            }
            

            return sb.ToString();
        }
        private static string LogObjectTo(object obj, string nameObj)
        {
            var log = new StringBuilder();
            if (obj != null)
            {
                log.AppendLine(nameObj);
                foreach (PropertyInfo propiedad in obj.GetType().GetProperties())
                {
                    if (propiedad.PropertyType.Namespace == "System")
                    {
                        var valorObject = propiedad.GetValue(obj, null);
                        string valor = valorObject == null ? "null" : valorObject.ToString();
                        log.AppendLine(propiedad.Name + " = " + valor);
                        
                    }
                }
            }
            return log.ToString();
        }

        public List<DistributorShoppingCartItem> SelectMaxItems()
        {
            var cartItems = new List<DistributorShoppingCartItem>();
            if (ShoppingCartItems != null && ShoppingCartItems.Count > 0)
            {
                if (HLConfigManager.Configurations.DOConfiguration.NotToshowTodayMagazineInCart)
                {
                    cartItems.AddRange(
                        ShoppingCartItems.Where(
                            c =>
                            c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku &&
                            c.SKU != HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku)
                                         .OrderByDescending(c => c.Updated)
                                         .Take(HLConfigManager.Configurations.ShoppingCartConfiguration.MaxMiniCartItem)
                                         .ToList());
                    return cartItems;
                }
                else
                {
                    cartItems.AddRange(
                        ShoppingCartItems.OrderByDescending(c => c.Updated)
                                         .Take(HLConfigManager.Configurations.ShoppingCartConfiguration.MaxMiniCartItem)
                                         .ToList());
                }
            }
            return cartItems;
        }

        public List<DistributorShoppingCartItem> GetShoppingCartUnfiltered(bool calculate,
                                                                   bool useCurrentLocale = false,
                                                                   bool validateSKU = false)
        {
            try
            {
                var itemList = new List<DistributorShoppingCartItem>();
                var cartItems = CartItems;
                if (cartItems != null && CartItems.Count > 0)
                {
                    DistributorShoppingCartItem item = null;
                    itemList.AddRange(from c in cartItems
                                      orderby c.Updated ascending
                                      where (item = CreateShoppingCartItem(c, useCurrentLocale, validateSKU)) != null
                                      select item);
                    ShoppingCartItems = itemList;


                }
                else
                {
                    CartItems = new ShoppingCartItemList();
                    ShoppingCartItems = new List<DistributorShoppingCartItem>();
                }
            }

            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                LoggerTempWireup.WriteInfo(
                    string.Format(
                        "getShoppingCartForDisplayFromService ! DS:{0} COUNTRY:{1} LOCALE:{2} WH:{3} SHIPPINGMETHOD:{4}, ERR{5}",
                        DistributorID, CountryCode, Locale, DeliveryInfo.WarehouseCode,
                        DeliveryInfo.FreightCode, ex), "Shoppingcart");
            }

            return ShoppingCartItems;
        }


        public List<DistributorShoppingCartItem> GetShoppingCartForDisplay(bool calculate,
                                                                           bool useCurrentLocale = false,
                                                                           bool validateSKU = false)
        {
            try
            {
                var itemList = new List<DistributorShoppingCartItem>();
                var cartItems = CartItems;
                if (cartItems != null && CartItems.Count > 0)
                {
                    DistributorShoppingCartItem item = null;
                    itemList.AddRange(from c in cartItems
                                      orderby c.Updated ascending
                                      where (item = CreateShoppingCartItem(c, useCurrentLocale, validateSKU)) != null
                                      select item);
                    ShoppingCartItems = itemList;
                    // if any ShoppingCartItem_V01 does not have matching DistributorShoppingCartItem, remove it
                    var skuToRemove = (from c in cartItems
                                       where ShoppingCartItems.Find(s => s.SKU == c.SKU) == null
                                       select c).ToList<ShoppingCartItem_V01>();
                    if (skuToRemove.Count() > 0)
                    {
                        ShoppingCartProvider.DeleteShoppingCart(this, skuToRemove.Select(s => s.SKU).ToList());
                        CartItems.RemoveAll(
                            delegate(ShoppingCartItem_V01 x) { return (skuToRemove.Find(s => s.SKU == x.SKU) != null); });
                    }
                    // if qty = 0, remove
                    CartItems.RemoveAll(
                        delegate(ShoppingCartItem_V01 x) { return x.Quantity == 0; });
                }
                else
                {
                    CartItems = new ShoppingCartItemList();
                    ShoppingCartItems = new List<DistributorShoppingCartItem>();
                }
            }

            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                LoggerTempWireup.WriteInfo(
                    string.Format(
                        "getShoppingCartForDisplayFromService ! DS:{0} COUNTRY:{1} LOCALE:{2} WH:{3} SHIPPINGMETHOD:{4}, ERR{5}",
                        DistributorID, CountryCode, Locale, DeliveryInfo.WarehouseCode,
                        DeliveryInfo.FreightCode, ex), "Shoppingcart");
            }

            return ShoppingCartItems;
        }

        public void AddHFFSKU(int quantity)
        {
            // if HFF SKU exists
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

            if (!string.IsNullOrEmpty(sku))
            {
                if (CartItems.Any(c => c.SKU == sku))
                {
                    ShoppingCartProvider.DeleteShoppingCart(this, new List<string> { sku });
                    CartItems.Remove(CartItems.SingleOrDefault(s => s.SKU == sku));
                    ShoppingCartItems.Remove(ShoppingCartItems.SingleOrDefault(s => s.SKU == sku));
                }
                var item = new ShoppingCartItem_V01() { ID = 0, SKU = sku, Quantity = quantity, Updated = DateTime.Now };
                var shoppingCartRuleResults = new List<ShoppingCartRuleResult>();
                shoppingCartRuleResults = ShoppingCartProvider.processCart(this, item,
                                                                           ShoppingCartRuleReason.CartItemsBeingAdded);

                if (shoppingCartRuleResults.Any(c => c.Result == RulesResult.Failure || c.Result == RulesResult.Feedback))
                {
                    return;
                }
                if (ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item, ShoppingCartID))
                {
                    CartItems.Add(item);
                    ShoppingCartItems.Add(CreateShoppingCartItem(item));
                }
                Totals = Calculate();

                var Results = HLRulesManager.Manager.ProcessCart(this,
                                                                 ShoppingCartRuleReason
                                                                     .CartItemsAdded);
                RuleResults = Results;

                //FireShoppingCartChangedEvent();
            }
        }

        public DistributorShoppingCartItem CreateShoppingCartItem(ShoppingCartItem_V01 cartItem,
                                                                  bool useCurrentLocale = false,
                                                                  bool validateCartItem = false)
        {
            try
            {
                var specialSKUs = new List<string>()
                    {
                        HLConfigManager.Configurations.APFConfiguration.SupervisorSku,
                        HLConfigManager.Configurations.APFConfiguration.DistributorSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku,
                        HLConfigManager.Configurations.DOConfiguration.TodayMagazineSecondarySku,
                        HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeSku
                    };
                specialSKUs.AddRange(HLConfigManager.Configurations.DOConfiguration.HFFSkuList);

                if (!string.IsNullOrEmpty(HLConfigManager.Configurations.APFConfiguration.AlternativeSku))
                {
                    specialSKUs.Add(HLConfigManager.Configurations.APFConfiguration.AlternativeSku);
                }

                //Bug 232448 *:SPLUNK LOGS: CNDO: object ref not set -MyHLShoppingCart.CreateShoppingCartItem >> ShoppingCartItem_V01 before you use the cart item please check whether its a null object
                if (cartItem == null) return null;

                string sku = cartItem.SKU;
                Dictionary<string, SKU_V01> allSKU;
                var ProductInfoCatalog = CatalogProviderLoader == null ? new CatalogProviderLoader().GetProductInfoCatalog(Locale) : CatalogProviderLoader.GetProductInfoCatalog(Locale);
                allSKU = ProductInfoCatalog.AllSKUs;
                
                if (allSKU == null)
                {
                    return null;
                }
                SKU_V01 sku_v01 = null;
                allSKU.TryGetValue(sku, out sku_v01);
                DistributorShoppingCartItem distributorShoppingCartItem = null;
                if (sku_v01 != null)
                {
                    //if (SKUsRemoved == null) SKUsRemoved = new List<string>();
                    int newQty = cartItem.Quantity;
                    string error = string.Empty;
                    //string error = validateCartItem
                    //                   ? ShoppingCartProvider.ValidateCartItem(sku, cartItem.Quantity, out newQty,
                    //                                                           Locale,
                    //                                                           DeliveryInfo != null
                    //                                                               ? DeliveryInfo.WarehouseCode
                    //                                                               : string.Empty)
                    //                   : string.Empty
                    //    ;
                    //if (newQty != 0)
                        distributorShoppingCartItem =
                            new DistributorShoppingCartItem
                                {
                                    ID = cartItem.ID,
                                    SKU = cartItem.SKU,
                                    MinQuantity = cartItem.MinQuantity,
                                    Updated = cartItem.Updated,
                                    Description = allSKU[sku].Description,
                                    Flavor = allSKU[sku].Description,
                                    RetailPrice = allSKU[sku].CatalogItem.ListPrice,
                                    EarnBase = allSKU[sku].CatalogItem.EarnBase,
                                    CatalogItem = allSKU[sku].CatalogItem,
                                    Quantity = newQty,
                                    PartialBackordered = cartItem.PartialBackordered,
                                    ErrorMessage = error,
                                    IsPromo = cartItem.IsPromo,
                                };
                    //if (newQty == 0)
                    //{
                    //    SKUsRemoved.Add(
                    //        string.Format(
                    //            HttpContext.GetGlobalResourceObject(
                    //                string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "InvalidSKU")
                    //                       .ToString(), cartItem.SKU));
                    //    cartItem.Quantity = newQty;
                    //    ShoppingCartProvider.DeleteShoppingCart(this, new List<string> {cartItem.SKU});
                    //}
                    //else if (newQty < cartItem.Quantity)
                    //{
                    //    cartItem.Quantity = newQty;
                    //    ShoppingCartProvider.DeleteShoppingCart(this, new List<string> {cartItem.SKU});
                    //    ShoppingCartProvider.InsertShoppingCartItem(DistributorID, cartItem, cartItem.ID);
                    //}
                        getAdditionalInfo(useCurrentLocale ? CultureInfo.CurrentCulture.Name : Locale,
                            distributorShoppingCartItem);
                }
                else
                {
                    var catalogItem = CatalogProvider.GetCatalogItem(cartItem.SKU, CountryCode);
                    if (catalogItem != null && specialSKUs.Any(x => x == cartItem.SKU))
                    {
                        distributorShoppingCartItem =
                            new DistributorShoppingCartItem
                                {
                                    ID = cartItem.ID,
                                    SKU = cartItem.SKU,
                                    Updated = cartItem.Updated,
                                    Flavor = catalogItem.Description,
                                    Description = catalogItem.Description,
                                    RetailPrice = catalogItem.ListPrice,
                                    EarnBase = catalogItem.EarnBase,
                                    CatalogItem = catalogItem,
                                    // InvList = catalogItem.InventoryList,
                                    Quantity = cartItem.Quantity,
                                    PartialBackordered = false,
                                    IsPromo = cartItem.IsPromo,
                                };
                    }
                }
                return distributorShoppingCartItem;
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                LoggerTempWireup.WriteInfo(
                    string.Format(
                        "getShoppingCartForDisplayFromService ! DS:{0} COUNTRY:{1} LOCALE:{2} WH:{3} SHIPPINGMETHOD:{4}, ERR:{5}",
                        DistributorID, CountryCode, Locale, DeliveryInfo.WarehouseCode,
                        DeliveryInfo.FreightCode, ex), "Shoppingcart");
            }

            return null;
        }

        // called when creating shopping cart
        public void LoadShippingInfo(int deliveryOptionID,
                                     int shippingAddressID,
                                     DeliveryOptionType optionType,
                                     OrderCategoryType categoryType,
                                     string freightCode,
                                     bool ignoreDeliveryInfoUpdate = false)
        {
            ShippingInfo deliveryInfo = null;
            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
            if (OrderCategory == OrderCategoryType.ETO &&
                !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
            {
                var option = shippingProvider.GetEventTicketShippingInfo();
                if (option != null)
                {
                    option.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                    var etoShippingInfo = new ShippingInfo(option);
                    deliveryInfo = etoShippingInfo;
                    DeliveryInfo = deliveryInfo;
                    return;
                }
            }
            var oType = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), optionType.ToString());
            if (sessionInfo == null || String.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
            {
                deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale, oType,
                                                                      deliveryOptionID, shippingAddressID);
            }
            else
            {
                if (sessionInfo.CustomerAddressID < 0 && sessionInfo.OrderConverted)
                {
                    deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale, oType,
                                                                          deliveryOptionID,
                                                                          sessionInfo.CustomerAddressID);
                }
                else
                {
                    deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale, oType,
                                                                          deliveryOptionID, shippingAddressID);
                }
            }
            if (null != deliveryInfo)
            {
                if (optionType == DeliveryOptionType.Unknown ||
                    (optionType == DeliveryOptionType.Shipping && deliveryInfo.Address != null &&
                     deliveryInfo.Address.ID != shippingAddressID))
                {
                    //shippingProvider.UpdateShippingInfo(this.ShoppingCartID, this.OrderCategory, DeliveryOptionType.Shipping, deliveryInfo.Id, deliveryInfo.Address.ID);
                    deliveryInfo.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                }
                // not to copy freight code from db blindly
                if (!string.IsNullOrEmpty(freightCode) && !freightCode.Equals("NOF") && OrderCategory == OrderCategoryType.RSO)
                {
                    deliveryInfo.FreightCode = freightCode;
                }
            }

            // Since dev principal team wants this line here, I will add a initialized parameter to the method to ignore it in some cases.
            if (!ignoreDeliveryInfoUpdate)
            {
                DeliveryInfo = deliveryInfo; // invoke a save
            }
            else
            {
                _DeliveryInfo = deliveryInfo;
                if (null != deliveryInfo)
                {
                    deliveryInfo.FreightCodeChanged += value_FreightCodeChanged;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="deliveryOptionID"></param>
        /// <param name="deliveryOption"></param>
        /// <param name="shippingAddressID"></param>
        /// <param name="shoppingCartId"></param>
        /// <param name="newDeliveryOption"></param>
        /// <param name="newShippingAddressID"></param>
        /// <param name="newDeliveryOptionID"></param>
        /// <param name="bCreateAddress"></param>
        /// <summary>
        ///     Matching the address
        /// </summary>
        public void MatchShippingInfo(int shoppingCartId,
                                      DeliveryOptionType deliveryOption,
                                      int shippingAddressID,
                                      int deliveryOptionID,
                                      out DeliveryOptionType newDeliveryOption,
                                      out int newShippingAddressID,
                                      out int newDeliveryOptionID,
                                      bool bCreateAddress = true)
        {
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);

            newShippingAddressID = shippingAddressID;
            newDeliveryOptionID = deliveryOptionID;
            newDeliveryOption = deliveryOption == DeliveryOptionType.Unknown
                                    ? DeliveryOptionType.Shipping
                                    : deliveryOption;

            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);

            if (newDeliveryOption == DeliveryOptionType.Shipping)
            {
                var shippingAddresses = shippingProvider.GetShippingAddresses(DistributorID,
                                                                              Locale);
                ShippingAddress_V02 theOrderShippingAddress;
                ShippingAddress_V02 addressToSave = null;

                if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                {
                    //For GDO China, never rely on the Order Shipping address.
                    theOrderShippingAddress = null;
                }
                else
                {
                    theOrderShippingAddress = (sessionInfo.OrderShippingAddresses != null)
                                           ? sessionInfo.OrderShippingAddresses.Find(
                                               o =>
                                               o.AltPhone != null && o.AltPhone.Equals(shoppingCartId.ToString()))
                                           : null;
                }

                var theShippingAddress = shippingAddresses != null
                                             ? shippingAddresses.Find(s => s.ID == shippingAddressID)
                                             : null;
                if (HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                {
                    if (theShippingAddress != null && theShippingAddress.HasAddressRestriction == null)
                    {
                        theShippingAddress = shippingAddresses != null
                                                 ? shippingAddresses.FirstOrDefault(s => s.HasAddressRestriction == true)
                                                 : null;
                        newShippingAddressID = theShippingAddress.ID;
                    }
                }
                if (theOrderShippingAddress != null && theShippingAddress != null)
                {
                    var addr1 = new ShippingAddress_V02
                    {
                        Alias = theOrderShippingAddress.Alias,
                        Phone = theOrderShippingAddress.Phone,
                        Address = new ServiceProvider.ShippingSvc.Address_V01
                        {
                            StateProvinceTerritory =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.StateProvinceTerritory)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.StateProvinceTerritory.ToUpper(),
                            City =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.City)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.City.ToUpper(),
                            CountyDistrict =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.CountyDistrict)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.CountyDistrict.ToUpper(),
                            Line1 =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.Line1)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.Line1.ToUpper(),
                            Line2 =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.Line2)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.Line2.ToUpper(),
                            Line3 =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.Line3)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.Line3.ToUpper(),
                            Line4 =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.Line4)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.Line4.ToUpper(),
                            PostalCode =
                                        string.IsNullOrEmpty(theOrderShippingAddress.Address.PostalCode)
                                            ? string.Empty
                                            : theOrderShippingAddress.Address.PostalCode.ToUpper(),
                        }
                    };
                    var addr2 = new ShippingAddress_V02
                    {
                        Alias = theShippingAddress.Alias,
                        Phone = theShippingAddress.Phone,
                        Address = new ServiceProvider.ShippingSvc.Address_V01
                        {
                            StateProvinceTerritory =
                                        string.IsNullOrEmpty(theShippingAddress.Address.StateProvinceTerritory)
                                            ? string.Empty
                                            : theShippingAddress.Address.StateProvinceTerritory.ToUpper(),
                            City =
                                        string.IsNullOrEmpty(theShippingAddress.Address.City)
                                            ? string.Empty
                                            : theShippingAddress.Address.City.ToUpper(),
                            CountyDistrict =
                                        string.IsNullOrEmpty(theShippingAddress.Address.CountyDistrict)
                                            ? string.Empty
                                            : theShippingAddress.Address.CountyDistrict.ToUpper(),
                            Line1 =
                                        string.IsNullOrEmpty(theShippingAddress.Address.Line1)
                                            ? string.Empty
                                            : theShippingAddress.Address.Line1.ToUpper(),
                            Line2 =
                                        string.IsNullOrEmpty(theShippingAddress.Address.Line2)
                                            ? string.Empty
                                            : theShippingAddress.Address.Line2.ToUpper(),
                            Line3 =
                                        string.IsNullOrEmpty(theShippingAddress.Address.Line3)
                                            ? string.Empty
                                            : theShippingAddress.Address.Line3.ToUpper(),
                            Line4 =
                                        string.IsNullOrEmpty(theShippingAddress.Address.Line4)
                                            ? string.Empty
                                            : theShippingAddress.Address.Line4.ToUpper(),
                            PostalCode =
                                        string.IsNullOrEmpty(theShippingAddress.Address.PostalCode)
                                            ? string.Empty
                                            : theShippingAddress.Address.PostalCode.ToUpper(),
                        }
                    };
                    if (addr1.Address.StateProvinceTerritory != addr2.Address.StateProvinceTerritory ||
                        addr1.Address.City != addr2.Address.City ||
                        addr1.Address.CountyDistrict != addr2.Address.CountyDistrict ||
                        addr1.Address.Line1 != addr2.Address.Line1 ||
                        addr1.Address.Line2 != addr2.Address.Line2 ||
                        addr1.Address.Line3 != addr2.Address.Line3 ||
                        addr1.Address.Line4 != addr2.Address.Line4 ||
                        addr1.Address.PostalCode != addr2.Address.PostalCode ||
                        addr1.Alias != addr2.Alias ||
                        addr1.Phone != addr2.Phone)
                    {
                        addressToSave = theOrderShippingAddress;
                    }
                }
                else if (theShippingAddress == null && theOrderShippingAddress == null)
                {
                    if (shippingAddresses.Count > 0)
                    {
                        if (shippingAddresses.Find(s => s.IsPrimary) != null)
                        {
                            newShippingAddressID = shippingAddresses.Find(s => s.IsPrimary).ID;
                        }
                        else
                        {
                            newShippingAddressID = shippingAddresses.Select(a => a.ID).FirstOrDefault();
                            //take the first address as no other better choice.
                        }
                    }
                }
                else
                {
                    if (theShippingAddress == null && theOrderShippingAddress != null)//shipping address is deleted, with order shipping address presented.
                    {
                        addressToSave = theOrderShippingAddress;
                    }
                    else if (theOrderShippingAddress == null && theShippingAddress != null) //probably order declined with the shipping address presented.
                    {
                        newShippingAddressID = theShippingAddress.Id;
                    }
                }
                if (!HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction)
                {
                    if (addressToSave != null && bCreateAddress)
                    {
                        // If address does not match with any one in the options, add it.
                        addressToSave.ID = shippingAddresses != null && shippingAddresses.Where(s => s.ID < 0).Count() > 0
                                               ? shippingAddresses.Where(s => s.ID < 0).Select(s => s.ID).Min() - 1
                                               : -1;
                        addressToSave.Alias = string.Empty;
                        ShippingAddressID = new ShippingProviderBase().SaveShippingAddress(
                            DistributorID,
                            Locale,
                            addressToSave,
                            true,
                            false,
                            false);
                        newShippingAddressID = ShippingAddressID;
                    }
                }
            }
            else if (newDeliveryOption == DeliveryOptionType.Pickup)
            {
                var deliveryOptions = shippingProvider.GetDeliveryOptions(Locale);
                if (deliveryOptions != null && deliveryOptions.Count() > 0)
                {
                    if (deliveryOptions.Find(d => d.Id == deliveryOptionID) == null)
                    // can't find in shipping table? match address
                    {
                        // get address from OrderShippingTable
                        var theOrderShippingAddress = (sessionInfo.OrderShippingAddresses != null)
                                                          ? sessionInfo.OrderShippingAddresses.Find(
                                                              o =>
                                                              o.AltPhone != null &&
                                                              o.AltPhone.Equals(shoppingCartId.ToString()))
                                                          : null;
                        if (theOrderShippingAddress != null)
                        {
                            // Addresses comparisson.
                            var matchedAddress =
                                from address in deliveryOptions
                                where address.Address != null
                                      && address.Address.City == theOrderShippingAddress.Address.City
                                      && address.Address.Country == theOrderShippingAddress.Address.Country
                                      &&
                                      address.Address.CountyDistrict == theOrderShippingAddress.Address.CountyDistrict
                                      && address.Address.Line1 == theOrderShippingAddress.Address.Line1
                                      && address.Address.Line2 == theOrderShippingAddress.Address.Line2
                                      && address.Address.Line3 == theOrderShippingAddress.Address.Line3
                                      && address.Address.Line4 == theOrderShippingAddress.Address.Line4
                                      &&
                                      address.Address.StateProvinceTerritory ==
                                      theOrderShippingAddress.Address.StateProvinceTerritory
                                select address;

                            // If any matchs.
                            if (matchedAddress != null && matchedAddress.Count() > 0)
                            {
                                newDeliveryOptionID = matchedAddress.First().Id;
                            }
                            else
                            {
                                // Selecting default address.
                                newDeliveryOptionID =
                                    deliveryOptions.Where(address => address.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                                                   .OrderBy(address => address.displayIndex).FirstOrDefault().Id;
                            }
                            //this.DeliveryOptionID = matchedAddressID;
                        }
                        else
                        {
                            // Selecting default address.
                            if (deliveryOptions.Any(address => address.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup))
                            {
                                newDeliveryOptionID =
                                    deliveryOptions.Where(address => address.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                                                   .OrderBy(address => address.displayIndex)
                                                   .FirstOrDefault()
                                                   .Id;
                            }
                        }
                    }
                }
            }
            else if (newDeliveryOption == DeliveryOptionType.PickupFromCourier)
            {
                var newDOption = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), newDeliveryOption.ToString());
                var theOrderShippingAddress = shippingProvider.GetShippingInfoFromID(DistributorID, Locale,
                                                                                     newDOption, deliveryOptionID,
                                                                                     shippingAddressID);
                if (theOrderShippingAddress != null)
                {
                    newDeliveryOptionID = theOrderShippingAddress.Id;
                    newShippingAddressID = newDeliveryOptionID;
                }
            }
            // this.DeliveryOption = deliveryOption;
        }

        // called by UpdateShippingInfo
        public void LoadShippingInfo(int deliveryOptionID,
                                     int shippingAddressID,
                                     DeliveryOptionType optionType,
                                     bool ignoreDeliveryInfoUpdate = false)
        {
            ShippingInfo deliveryInfo = null;
            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
            var oType = (ServiceProvider.ShippingSvc.DeliveryOptionType)Enum.Parse(typeof(ServiceProvider.ShippingSvc.DeliveryOptionType), optionType.ToString());
            if (null != sessionInfo && sessionInfo.IsEventTicketMode &&
                !HLConfigManager.Configurations.DOConfiguration.ShowOrderQuickViewForEventTicket)
            {
                var option = shippingProvider.GetEventTicketShippingInfo();
                if (option != null)
                {
                    option.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                    var etoShippingInfo = new ShippingInfo(option);
                    deliveryInfo = etoShippingInfo;
                }
                else
                {
                    deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale, oType,
                                                                          deliveryOptionID, shippingAddressID);
                }
            }
            else
            {
                if (null == sessionInfo || String.IsNullOrEmpty(sessionInfo.CustomerOrderNumber))
                {
                    deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale, oType,
                                                                          deliveryOptionID, shippingAddressID);
                }
                else
                {
                    if (null != sessionInfo && sessionInfo.CustomerAddressID < 0 && sessionInfo.OrderConverted)
                    {
                        deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale,
                                                                              oType,
                                                                              deliveryOptionID,
                                                                              sessionInfo.CustomerAddressID);
                    }
                    else
                    {
                        deliveryInfo = shippingProvider.GetShippingInfoFromID(DistributorID, Locale,
                                                                              oType,
                                                                              deliveryOptionID,
                                                                              shippingAddressID);
                    }
                }
                if (null != deliveryInfo)
                {
                    if (optionType == DeliveryOptionType.Unknown ||
                        (optionType == DeliveryOptionType.Shipping && deliveryInfo.Address != null &&
                         deliveryInfo.Address.ID != shippingAddressID))
                    {
                        //shippingProvider.UpdateShippingInfo(this.ShoppingCartID, this.OrderCategory, DeliveryOptionType.Shipping, deliveryInfo.Id, deliveryInfo.Address.ID);
                        deliveryInfo.Option = ServiceProvider.ShippingSvc.DeliveryOptionType.Shipping;
                    }
                }
            }

            // Since dev principal team wants this line here, I will add a initialized parameter to the method to ignore it in some cases.
            if (!ignoreDeliveryInfoUpdate)
            {
                DeliveryInfo = deliveryInfo; // invoke a save
            }
            else
            {
                _DeliveryInfo = deliveryInfo;
                if (null != deliveryInfo)
                {
                    deliveryInfo.FreightCodeChanged += value_FreightCodeChanged;
                }
            }
        }

        public void GetShippingInfoForCopyOrder(string distributorID,
                                                string locale,
                                                int shoppingCartID,
                                                DeliveryOptionType option,
                                                bool ignoreDeliveryInfoUpdate = false)
        {
            var deliveryInfo = ShoppingCartProvider.GetShippingInfoForCopyOrder(distributorID, locale, shoppingCartID,
                                                                                option);

            // Since dev principal team wants this line here, I will add a initialized parameter to the method to ignore it in some cases.
            if (!ignoreDeliveryInfoUpdate)
            {
                DeliveryInfo = deliveryInfo; // invoke a save
            }
            else
            {
                _DeliveryInfo = deliveryInfo;
                if (null != deliveryInfo)
                {
                    deliveryInfo.FreightCodeChanged += value_FreightCodeChanged;
                }
            }
        }

        public void CheckAPFShipping()
        {
            string initialWarehouseCode = HLConfigManager.Configurations.APFConfiguration.InitialAPFwarehouse;
            string warehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
            string freightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
            string actualWarehouseCode = string.IsNullOrEmpty(initialWarehouseCode)
                                             ? warehouseCode
                                             : initialWarehouseCode;
            var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
            if (null == DeliveryInfo)
            {
                ActualWarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                var apfOption = shippingProvider.GetAPFShippingInfo();
                if (null != apfOption)
                {
                    // apfOption.FreightCode = freightCode;
                    var shippingInfo = new ShippingInfo(apfOption);
                    shippingInfo.FreightCode = freightCode;
                    DeliveryInfo = shippingInfo; // will invoke a save
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format("Could not resolve the APF warehouse code {0} for {1}", warehouseCode, CountryCode));
                }
            }
            else
            {
                if (HLConfigManager.Configurations.APFConfiguration.StandaloneAPFOnlyAllowed &&
                    !HLConfigManager.Configurations.APFConfiguration.ShowOrderQuickViewForStandaloneAPF)
                {
                    ActualWarehouseCode = HLConfigManager.Configurations.ShoppingCartConfiguration.DefaultWarehouse;
                    var apfOption = shippingProvider.GetAPFShippingInfo();
                    if (null != apfOption)
                    {
                        // apfOption.FreightCode = freightCode;
                        var shippingInfo = new ShippingInfo(apfOption);
                        shippingInfo.FreightCode = freightCode;
                        DeliveryInfo = shippingInfo; // will invoke a save
                    }
                    else
                    {
                        LoggerHelper.Error(
                            string.Format("Could not resolve the APF warehouse code {0} for {1}", warehouseCode,
                                          CountryCode));
                    }
                }
                else
                {
                    ActualWarehouseCode = DeliveryInfo.WarehouseCode;
                    DeliveryInfo.FreightCode = freightCode;
                    DeliveryInfo.WarehouseCode = string.IsNullOrEmpty(actualWarehouseCode)
                                                     ? ActualWarehouseCode
                                                     : actualWarehouseCode;
                }
            }

            if (null != DeliveryInfo)
            {
                if (DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup)
                {
                    var distributorName = string.Empty;
                    var membershipUser = (MyHerbalife3.Shared.ViewModel.Models.MembershipUser<DistributorProfileModel>)Membership.GetUser();
                    if (membershipUser == null)
                    {
                        var profile = DistributorProfile(this.DistributorID);
                        if (null != profile)
                        {
                            distributorName = DistributorProfileModelHelper.DistributorName(profile);
                        }
                    }
                    else
                    {
                        distributorName = DistributorProfileModelHelper.DistributorName(
                        ((MyHerbalife3.Shared.ViewModel.Models.MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value);    
                    }
                    
                    DeliveryInfo.Address.Recipient = distributorName;
                }
            }
            else
            {
                LoggerHelper.Error(
                    string.Format("Could not resolve the APF warehouse code {0} for {1}", warehouseCode, CountryCode));
            }
        }

        private static DistributorProfileModel DistributorProfile(string distributorId)
        {
            var loader = new DistributorProfileLoader();
            var profile = loader.Load(new GetDistributorProfileById() { Id = distributorId });
            if (profile != null)
                return profile;
            return null;

        }

        public void CheckShippingForNonStandAloneAPF()
        {
            string initialWarehouseCode = HLConfigManager.Configurations.APFConfiguration.InitialAPFwarehouse;
            string warehouseCode = HLConfigManager.Configurations.APFConfiguration.APFwarehouse;
            string apfFreightCode = HLConfigManager.Configurations.APFConfiguration.APFFreightCode;
            string actualWarehouseCode = string.IsNullOrEmpty(initialWarehouseCode)
                                             ? warehouseCode
                                             : initialWarehouseCode;

            if (DeliveryInfo != null)
            {
                if (DeliveryInfo.FreightCode == apfFreightCode)
                {
                    var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
                    int shippingAddressID = 0;

                    if (null != DeliveryInfo.Address)
                        shippingAddressID = DeliveryInfo.Address.ID;

                    var shippingInfo =
                        shippingProvider.GetShippingInfoFromID(DistributorID, Locale, DeliveryInfo.Option,
                                                               DeliveryInfo.Id, shippingAddressID);

                    if (null != shippingInfo && !string.IsNullOrEmpty(shippingInfo.FreightCode) &&
                        !string.IsNullOrEmpty(shippingInfo.WarehouseCode))
                    {
                        // If the user keyed some info for pickup, it will remain in the new object
                        if (DeliveryInfo.Option == ServiceProvider.ShippingSvc.DeliveryOptionType.Pickup && shippingInfo.Address != null)
                        {
                            shippingInfo.Address.Recipient = DeliveryInfo.Address.Recipient;
                            shippingInfo.Address.Phone = DeliveryInfo.Address.Phone;
                        }
                        DeliveryInfo = shippingInfo;
                    }
                    else
                    {
                        if (DeliveryInfo.FreightCode != apfFreightCode)
                        {
                            ShoppingCartProvider.UpdateShoppingCart(this);
                        }
                        DeliveryInfo.FreightCode = apfFreightCode;
                        DeliveryInfo.WarehouseCode = warehouseCode;
                    }
                }
            }
        }

        private void calculateTaxedNet(OrderTotals_V01 totals, string countryCode)
        {
            switch (countryCode)
            {
                case "UY":
                case "SV":
                case "CR":
                case "HN":
                case "PY":
                    {
                        var pHCharge =
                            totals.ChargeList.Find(
                                delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.PH; }) as
                            Charge_V01 ?? new Charge_V01(ChargeTypes.PH, (decimal)0.0);
                        var freightCharge =
                            totals.ChargeList.Find(
                                delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                            Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                        totals.TaxableAmountTotal = totals.DiscountedItemsTotal + pHCharge.Amount + freightCharge.Amount;
                    }
                    break;
                case "EC":
                    {
                        if (totals.TaxableAmountTotal == 0)
                        {
                            var pHCharge =
                                totals.ChargeList.Find(
                                    delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.PH; }) as
                                Charge_V01 ?? new Charge_V01(ChargeTypes.PH, (decimal)0.0);
                            var freightCharge =
                                totals.ChargeList.Find(
                                    delegate(Charge p) { return ((Charge_V01)p).ChargeType == ChargeTypes.FREIGHT; }) as
                                Charge_V01 ?? new Charge_V01(ChargeTypes.FREIGHT, (decimal)0.0);
                            decimal taxAmountTotal = 0;
                            foreach (var item in totals.ItemTotalsList as List<ItemTotal>)
                            {
                                taxAmountTotal += (item as ItemTotal_V01).DiscountedPrice;
                            }
                            totals.TaxableAmountTotal = taxAmountTotal + pHCharge.Amount + freightCharge.Amount;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public OrderTotals Calculate()
        {
            if (CartItems == null || CartItems.Count == 0)
            {
                if (!HLConfigManager.Configurations.DOConfiguration.CalculateWithoutItems)
                    return new OrderTotals_V01();
            }
            if ((DeliveryInfo == null || string.IsNullOrEmpty(DeliveryInfo.WarehouseCode) || string.IsNullOrEmpty(DeliveryInfo.FreightCode)) && !HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                return new OrderTotals_V01();
            }
            var total = new OrderTotals_V01();
            OrderTotals total2 = total;

            var order = OrderCreationHelper.CreateOrderObject(this) as Order_V01;
            // lookup ECM Linkable table for linked SKU
            order = ShoppingCartProvider.AddLinkedSKU(order, Locale, CountryCode, DeliveryInfo!=null? DeliveryInfo.WarehouseCode: string.Empty) as Order_V01;
            order = OrderCreationHelper.FillOrderInfo(order, this) as Order_V01;

            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode) &&
                HLConfigManager.Configurations.DOConfiguration.PurchasingLimitRestrictionType ==
                PurchasingLimitRestrictionType.Annually)
            {
                if (order.PurchasingLimits == null || (order.PurchasingLimits) == null ||
                    String.IsNullOrEmpty(((PurchasingLimits_V01)order.PurchasingLimits).PurchaseSubType))
                {
                    if (!String.IsNullOrEmpty(SelectedDSSubType))
                    {
                        order.PurchasingLimits = PurchasingLimitProvider.GetPurchasingLimits(DistributorID,
                                                                                             SelectedDSSubType);
                    }
                }
            }
            //SessionInfo sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            //if (sessionInfo.IsEventTicketMode)
            //{
            //    (order.Shipment as ShippingInfo_V01).ShippingMethodID = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
            //    (order.Shipment as ShippingInfo_V01).WarehouseCode = HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
            //}

            // Verify if it is standalone
            if (APFDueProvider.containsOnlyAPFSku(ShoppingCartItems))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }

            // Verify if it is a standalone HFF
            if (ShoppingCartProvider.IsStandaloneHFF(ShoppingCartItems))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                               HLConfigManager.Configurations.DOConfiguration.HFFModalOrderType);
            }

            // if totals is null, check carts' integrity
            if ((Totals = CalcTotal(ShoppingCartItems, order)) == null)
            {
                if (CartItems.Count > 0)
                {
                    var results = HLRulesManager.Manager.ProcessCart(this,
                                                                     ShoppingCartRuleReason
                                                                         .CartCalculated);
                    if (results.Count > 0 &&
                        results.Any(r => r.Result == RulesResult.Failure && r.RuleName == "CartIntegrity Rules"))
                    {
                        Totals = CalcTotal(ShoppingCartItems, order); //if we had a rule failure, attempt a recalc
                    }
                }
            }

            if (Totals != null)
            {
                if (HLConfigManager.Configurations.CheckoutConfiguration.CalculateSubtotal)
                {
                    calculateTaxedNet(Totals as OrderTotals_V01, order.CountryOfProcessing);
                }
            }
            ShippingProvider.GetShippingProvider(null).SetShippingInfo(this);
            if (!string.IsNullOrEmpty(error))
                  ErrorCode = error; 
                           
            return Totals;
        }

        public OrderTotals Calculate(List<ShoppingCartItem_V01> itemsToCalc)
        {
            //errorCode = null;
            if (itemsToCalc == null || itemsToCalc.Count == 0)
            {
                return new OrderTotals_V01();
            }
            var total = new OrderTotals_V01();
            OrderTotals total2 = total;

            List<DistributorShoppingCartItem> listItems = (from item in itemsToCalc
                                                    select new DistributorShoppingCartItem { SKU = item.SKU, Quantity = item.Quantity }).ToList();

            var order = HLConfigManager.Configurations.DOConfiguration.IsChina ? China.OrderProvider.CreateOrderObject(listItems) : OrderCreationHelper.CreateOrderObject(itemsToCalc) as Order_V01;

            order = ShoppingCartProvider.AddLinkedSKU(order, Locale, CountryCode,
                                                      DeliveryInfo == null
                                                          ? HLConfigManager.Configurations.ShoppingCartConfiguration
                                                                           .DefaultWarehouse
                                                          : DeliveryInfo.WarehouseCode) as Order_V01;
            order = OrderCreationHelper.FillOrderInfo(order, this) as Order_V01;

            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
            {
                order.PurchasingLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
            }
            if (HLConfigManager.Configurations.DOConfiguration.UsesTaxRules)
            {
                if (null != order)
                {
                    HLRulesManager.Manager.PerformTaxationRules(order, Locale);
                }
            }
            // Verify if it is standalone
            if (APFDueProvider.containsOnlyAPFSku(itemsToCalc))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }

            // Verify if it is a standalone HFF
            if (ShoppingCartProvider.IsStandaloneHFF(itemsToCalc))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                               HLConfigManager.Configurations.DOConfiguration.HFFModalOrderType);
            }

            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            bool HMSCalc = false;
            if (sessionInfo.IsEventTicketMode)
            {
                (order.Shipment as ShippingInfo_V01).ShippingMethodID =
                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                (order.Shipment as ShippingInfo_V01).WarehouseCode =
                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
            }

            return
                HLConfigManager.Configurations.DOConfiguration.IsChina ? (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, Totals, true) : (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, QuotePartType.Complete, HMSCalc,out error);
                
        }

        public OrderTotals Calculate(List<ShoppingCartItem_V01> itemsToCalc, bool useHmsPricing)
        {
            if (itemsToCalc == null || itemsToCalc.Count == 0)
            {
                return new OrderTotals_V01();
            }
            

            List<DistributorShoppingCartItem> listItems = (from item in itemsToCalc
                                                           select new DistributorShoppingCartItem { SKU = item.SKU, Quantity = item.Quantity }).ToList();

            var order = HLConfigManager.Configurations.DOConfiguration.IsChina ? China.OrderProvider.CreateOrderObject(listItems) : OrderCreationHelper.CreateOrderObject(itemsToCalc) as Order_V01;

            order = ShoppingCartProvider.AddLinkedSKU(order, Locale, CountryCode,
                                                      DeliveryInfo == null
                                                          ? HLConfigManager.Configurations.ShoppingCartConfiguration
                                                                           .DefaultWarehouse
                                                          : DeliveryInfo.WarehouseCode) as Order_V01;
            order = OrderCreationHelper.FillOrderInfo(order, this) as Order_V01;

            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
            {
                order.PurchasingLimits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
            }
            if (HLConfigManager.Configurations.DOConfiguration.UsesTaxRules)
            {
                if (null != order)
                {
                    HLRulesManager.Manager.PerformTaxationRules(order, Locale);
                }
            }
            // Verify if it is standalone
            if (APFDueProvider.containsOnlyAPFSku(itemsToCalc))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }

            // Verify if it is a standalone HFF
            if (ShoppingCartProvider.IsStandaloneHFF(itemsToCalc))
            {
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                               HLConfigManager.Configurations.DOConfiguration.HFFModalOrderType);
            }

            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            if (sessionInfo.IsEventTicketMode)
            {
                (order.Shipment as ShippingInfo_V01).ShippingMethodID =
                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketFreightCode;
                (order.Shipment as ShippingInfo_V01).WarehouseCode =
                    HLConfigManager.Configurations.CheckoutConfiguration.EventTicketWarehouseCode;
            }

            return
                HLConfigManager.Configurations.DOConfiguration.IsChina ? (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, Totals, false) : (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, QuotePartType.Complete, useHmsPricing,out error);

        }

        public void AddTodayMagazine(int quantity, string todayMagazineSKU)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            bool bEventTicket = sessionInfo == null ? false : !sessionInfo.IsEventTicketMode ? sessionInfo.IsHAPMode : true;
            if (bEventTicket)
            {
                return;
            }

            string skuToAdd = string.IsNullOrEmpty(todayMagazineSKU)
                                  ? HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku
                                  : todayMagazineSKU;
            if (HLConfigManager.Configurations.DOConfiguration.AllowTodaysMagazine && !string.IsNullOrEmpty(skuToAdd))
            {
                var skuCatItem = CatalogProvider.GetCatalogItem(skuToAdd, CountryCode);
                if (skuCatItem != null)
                {
                    var item = new ShoppingCartItem_V01(0, skuToAdd, quantity, DateTime.Now);
                    var shoppingCartRuleResults = new List<ShoppingCartRuleResult>();
                    shoppingCartRuleResults = ShoppingCartProvider.processCart(this, item,
                                                                               ShoppingCartRuleReason
                                                                                   .CartItemsBeingAdded);
                    if (shoppingCartRuleResults.Any(c => c.Result == RulesResult.Failure))
                    {
                        var ruleresults = (from r in RuleResults where r.RuleName == "APF Rules" select r);
                        if (null != ruleresults && ruleresults.Count() > 0)
                        {
                            return;
                        }
                    }
                    ShoppingCartProvider.InsertShoppingCartItem(DistributorID, item, ShoppingCartID);
                    CartItems.Add(item);
                    ShoppingCartItems.Add(CreateShoppingCartItem(item));
                    sessionInfo.IsTodaysMagazineInCart = true;
                }

                //FireShoppingCartChangedEvent();
            }
        }

        public void DeleteTodayMagazine(string skuToRemove)
        {
            if (CartItems == null || CartItems.Count == 0)
                return;
            skuToRemove = string.IsNullOrEmpty(skuToRemove)
                              ? HLConfigManager.Configurations.DOConfiguration.TodayMagazineSku
                              : skuToRemove;
            if (!string.IsNullOrEmpty(skuToRemove))
            {
                var skuCatItem = CatalogProvider.GetCatalogItem(skuToRemove, CountryCode);
                if (skuCatItem != null)
                {
                    // var inventoryItems = ShoppingCartItems.Where(s => s.CatalogItem.IsInventory == true && s.SKU != skuToRemove);
                    if (ShoppingCartItems.Any(t => t.SKU.Trim() == skuToRemove))
                    {
                        ShoppingCartProvider.DeleteShoppingCart(this, new List<string> { skuToRemove });
                        CartItems.Remove(CartItems.SingleOrDefault(s => s.SKU == skuToRemove));
                        ShoppingCartItems.Remove(ShoppingCartItems.SingleOrDefault(s => s.SKU == skuToRemove));
                    }
                }
            }
        }

        public void Copy(MyHLShoppingCart cart)
        {
            ActualWarehouseCode = cart.ActualWarehouseCode;
            CountryCode = cart.CountryCode;
            DeliveryInfo = cart.DeliveryInfo;
            DeliveryOption = cart.DeliveryOption;
            DeliveryOptionID = cart.DeliveryOptionID;
            DistributorID = cart.DistributorID;
            EmailAddress = cart.EmailAddress;
            FreightCode = cart.FreightCode;
            InvoiceOption = cart.InvoiceOption;
            Locale = cart.Locale;
            OrderCategory = cart.OrderCategory;
            OrderSubType = cart.OrderSubType;
            ShippingAddressID = cart.ShippingAddressID;
        }

        /// <summary>
        /// Helper method that populates additional information on the distributor shopping cart item.
        /// A basic wrapper for the private implementation of the same functionality.
        /// </summary>
        /// <param name="locale">ISO local</param>
        /// <param name="item">DistributorShoppingCartItem that needs the extra information populated</param>
        public void AdditionalInfoHelper(string locale, DistributorShoppingCartItem item)
        {
            this.getAdditionalInfo(locale, item);
        }

        public void ResetPCLearningPoint()
        {
            _pcLearningPoint = null;
            _changeRate = null;
        }

        #endregion Public methods

        #region Private methods

        public bool shouldRecalculate(string previousFreightCode, ServiceProvider.ShippingSvc.Address_V01 oldAddress)
        {
            if (DeliveryInfo != null)
            {
                var shippingProvider = ShippingProvider.GetShippingProvider(CountryCode);
                if (shippingProvider != null)
                {
                    return shippingProvider.ShouldRecalculate(previousFreightCode, DeliveryInfo.FreightCode, oldAddress,
                                                              DeliveryInfo.Address.Address);
                }
            }
            return true;
        }

        private bool containSKU(string sku, List<ProductInfo_V02> listProds)
        {
            return listProds.Where(p => p.SKUs != null && p.SKUs.Any(s => s.SKU == sku)).Count() > 0;
        }

        private void getAdditionalInfo(string Locale, DistributorShoppingCartItem item)
        {
            if (item != null)
            {
                ProductInfoCatalog_V01 productInfo=new ProductInfoCatalog_V01();
                SKU_V01 sku;
                Dictionary<string, SKU_V01> AllSKUS;


                productInfo = CatalogProviderLoader == null ? new CatalogProviderLoader().GetProductInfoCatalog(Locale) : CatalogProviderLoader.GetProductInfoCatalog(Locale);
                AllSKUS = productInfo.AllSKUs;
                
                
                //if (CatalogProviderLoader != null)
                //{
                //    productInfo = CatalogProviderLoader.GetProductInfoCatalog(Locale);
                //    AllSKUS = productInfo.AllSKUs;
                //}
                //else
                //{
                //    productInfo = CatalogProvider.GetProductInfoCatalog(Locale);
                //    AllSKUS= productInfo.AllSKUs;
                //}



                if (AllSKUS.TryGetValue(item.SKU, out sku))
                {
                    var varCategory = from c in productInfo.AllCategories
                                      where c.Value.Products != null &&
                                            c.Value.Products.Where(p => p.SKUs != null &&
                                                                        p.SKUs.Any(s => s.SKU == sku.SKU)).Count() > 0
                                      select c.Value;
                    if (varCategory.Count() > 0)
                    {
                        var category = varCategory.First();
                        item.ParentCat = category;
                        var varProd = category.Products.Where(p => p.SKUs != null &&
                                                                   p.SKUs.Any(s => s.SKU == sku.SKU));
                        if (varProd.Count() > 0)
                        {
                            item.ProdInfo = varProd.First();
                            item.Description = string.Format("{0}{1}", item.ProdInfo.DisplayName,
                                                             string.IsNullOrEmpty(item.Flavor)
                                                                 ? string.Empty
                                    : " - " + item.Flavor);
                        }
                    }
                }
            }
        }

        private DistributorShoppingCartItem getDistributorShoppingCartItem(ShoppingCartItem_V01 item)
        {
            return ShoppingCartItems.Single(s => s.SKU == item.SKU);
        }

        private OrderTotals CalcTotal(List<DistributorShoppingCartItem> items, Order_V01 order)
        {
            var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            bool HMSCalc = sessionInfo == null
                               ? HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc
                               : sessionInfo.UseHMSCalc;

            if (HLConfigManager.Platform == "MyHL")
            {
                if (HttpContext.Current != null)
                {
                    string rawURL = HttpContext.Current.Request.RawUrl.ToLower();
                    if (rawURL.Contains("checkout.aspx") || rawURL.Contains("confirm.aspx") || rawURL.Contains("service.svc") ||
                        rawURL.Contains("paymentgatewaymanager.aspx"))
                    {
                        HMSCalc = HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc;
                        if (sessionInfo != null && (sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.OrderCategory == OrderCategoryType.ETO &&
                                                    HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket &&
                                                    items != null && items.Any() && items.Sum(i => i.CatalogItem.ListPrice) == 0))
                        {
                            HMSCalc = false;
                        }
                        if (!HMSCalc)
                        {
                            var countriesNonAuth = new string[] { "MX", "AR", "CL", "CO", "PE", "CR", "VE", "KR", "CA", "CN", "HR" };
                            if (!countriesNonAuth.Contains(CountryCode))
                            {
                                var mssge = string.Format(
                                    "{0} -Calling non Auth Pricing for Country: {1} in Locale: {2} for DSID:{3}.",
                                    DateTime.Now, CountryCode, Locale, DistributorID);
                                LoggerHelper.Info(mssge);
                            }
                        }
                    }
                    else
                    {
                        HMSCalc = false;
                    }
                }
                else
                {
                    LoggerHelper.Info(
                        string.Format(
                            "Pricing Request Received with a null context. Internet Pricing would be used. DSID:{0}",
                            DistributorID));
                }
            }

            if (sessionInfo != null && (HLConfigManager.Configurations.DOConfiguration.IsChina && sessionInfo.IsEventTicketMode))
            {
                var orderV01 = order as Order_V01;
                if (orderV01 != null)
                {
                    var shipmentV01 = orderV01.Shipment as ShippingInfo_V01;
                    if (shipmentV01 != null)
                    {
                        if (shipmentV01.ShippingMethodID == "NOF")
                        {
                            shipmentV01.ShippingMethodID = "3";
                        }
                    }
                }
            }

            if(HLConfigManager.Configurations.DOConfiguration.AllowHAP && sessionInfo != null && sessionInfo.IsHAPMode && OrderCategory == OrderCategoryType.HSO)
            {
                order.OrderSubType = HAPType;
            }

            OrderTotals_V01 orderTotal = HLConfigManager.Configurations.DOConfiguration.IsChina ? (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, Totals, true) : (OrderTotals_V01)ShoppingCartProvider.GetQuote(order, QuotePartType.Complete, HMSCalc, out error);
            //if (HLConfigManager.Configurations.DOConfiguration.IsChina && sessionInfo !=null && !string.IsNullOrEmpty(sessionInfo.FirstOrderPromoSku))
            //{
            //    OrderTotals_V02 ordertotal = orderTotal as OrderTotals_V02;
            //    if (ordertotal !=null && ordertotal.OrderFreight !=null)
            //    {
                   
            //        OLCDataProvider olcDataProvider = new OLCDataProvider();
            //        olcDataProvider.CaseRate = ordertotal.OrderFreight.CaseRate;
            //        olcDataProvider.Weight = ordertotal.OrderFreight.Weight;
            //        olcDataProvider.ActualFreight = ordertotal.OrderFreight.ActualFreight;
            //        olcDataProvider.BeforeWeight = ordertotal.OrderFreight.BeforeWeight;
            //        olcDataProvider.VolumeWeight = ordertotal.OrderFreight.VolumeWeight;
            //        olcDataProvider.PhysicalWeight = ordertotal.OrderFreight.PhysicalWeight;
            //        olcDataProvider.Insurance = ordertotal.OrderFreight.Insurance;
            //        olcDataProvider.InsuranceRate = ordertotal.OrderFreight.InsuranceRate;
            //        sessionInfo.olcDataprovider = olcDataProvider;
                    
            //    }
              
            //}
            if (HLConfigManager.Platform == "MyHL")
            {
                if (orderTotal != null && orderTotal.DiscountPercentage == decimal.Zero &&
                    !HLConfigManager.Configurations.CheckoutConfiguration.AllowZeroDiscount)
                {
                    return null;
                }
            }

            // calculate discount amount
            if (HLConfigManager.Configurations.DOConfiguration.UsesDiscountRules)
            {
                HLRulesManager.Manager.PerformDiscountRules(this, order, Locale, ShoppingCartRuleReason.CartCalculated);
            }
            ItemTotalsList itemTotalList;

            var AllSKUs = CatalogProvider.GetAllSKU(Locale);
            if (orderTotal != null && (itemTotalList = orderTotal.ItemTotalsList) != null)
            {
                Func<ItemTotalsList, DistributorShoppingCartItem, DistributorShoppingCartItem> setTotal =
                    delegate(ItemTotalsList itemTotalsList, DistributorShoppingCartItem item)
                        {
                            var itemTotal =
                            (ItemTotal_V01)itemTotalsList.Find(p => ((ItemTotal_V01)p).SKU == item.SKU.Trim());
                            if (itemTotal != null)
                            {
                                item.RetailPrice = itemTotal.RetailPrice;
                                item.DiscountPrice = itemTotal.DiscountedPrice;
                                item.VolumePoints = itemTotal.VolumePoints;
                                item.EarnBase = itemTotal.EarnBase;

                                SKU_V01 sku01;
                                if (AllSKUs.TryGetValue(item.SKU.Trim(), out sku01))
                                {
                                    Func<DistributorShoppingCartItem, ItemTotal_V01, bool> addLineItem =
                                        delegate(DistributorShoppingCartItem to, ItemTotal_V01 from)
                                            {
                                                to.RetailPrice += from.RetailPrice;
                                                to.DiscountPrice += from.DiscountedPrice;
                                                to.VolumePoints += from.VolumePoints;
                                                to.EarnBase += from.EarnBase;
                                                return true;
                                            };
                                    if (sku01.SubSKUs != null)
                                    {
                                        ItemTotal_V01 linkedSKUItemTotal = null;
                                        var t = from linkedsku in sku01.SubSKUs
                                                where
                                                    ((linkedSKUItemTotal =
                                                      (ItemTotal_V01)
                                                  itemTotalsList.Find(x => ((ItemTotal_V01)x).SKU == linkedsku.SKU)) !=
                                                     null)
                                                select addLineItem(item, linkedSKUItemTotal);
                                    }
                                }
                            }
                            return item;
                        };

                Array.ForEach(items.ToArray(), a => setTotal(orderTotal.ItemTotalsList, a));
            }
            return orderTotal;
        }

        private void CopyCart(ShoppingCart_V02 cart)
        {
            CartItems = cart.CartItems;
            CurrentItems = cart.CurrentItems;
            DistributorID = cart.DistributorID.Trim();
            LastUpdated = cart.LastUpdated;
            Locale = cart.Locale;
            CountryCode = cart.Locale.Substring(3);
            ShoppingCartID = cart.ShoppingCartID;
            Version = cart.Version;

            DeliveryOption = cart.DeliveryOption;
            DeliveryOptionID = cart.DeliveryOptionID;
            ShippingAddressID = cart.ShippingAddressID;
            OrderCategory = cart.OrderCategory;
            FreightCode = cart.FreightCode;
            OrderSubType = cart.OrderSubType;

            CustomerOrderDetail = cart.CustomerOrderDetail;
            LastUpdatedUtc = cart.LastUpdatedUtc;
        }

        private void CopyCart(ShoppingCart_V03 cart)
        {
            PassDSFraudValidation = true;
            CartItems = cart.CartItems;
            CurrentItems = cart.CurrentItems;
            DistributorID = cart.DistributorID.Trim();
            LastUpdated = cart.LastUpdated;
            Locale = cart.Locale;
            CountryCode = cart.Locale.Substring(3);
            ShoppingCartID = cart.ShoppingCartID;
            Version = cart.Version;

            DeliveryOption = cart.DeliveryOption;
            DeliveryOptionID = cart.DeliveryOptionID;
            ShippingAddressID = cart.ShippingAddressID;
            OrderCategory = cart.OrderCategory;
            FreightCode = cart.FreightCode;
            OrderSubType = cart.OrderSubType;

            CustomerOrderDetail = cart.CustomerOrderDetail;
            IsSavedCart = cart.IsDraft;
            CartName = cart.DraftName;
            LastUpdatedUtc = cart.LastUpdatedUtc;
        }

        /// <summary>
        /// This method will initialize all the referenced objects.
        /// Should be called in the constructors.
        /// </summary>
        private void InitializaRefObjects()
        {
            this.EmailValues = new HLShoppingCartEmailValues();
        }

        #endregion Private methods
    }
}