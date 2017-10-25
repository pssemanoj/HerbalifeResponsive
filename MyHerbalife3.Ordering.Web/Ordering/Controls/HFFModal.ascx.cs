using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class HFFModal : UserControlBase
    {
        #region Fields and properties

        public bool CanSubmitNow = false;
        private PaymentInfoBase PaymentOptions { get; set; }

        /// <summary>
        ///     Gets or sets the shopping cart ID for the previous order.
        /// </summary>
        public int ActiveShoppingCartId
        {
            get
            {
                if (string.IsNullOrEmpty(btnDone.CommandArgument))
                {
                    return 0;
                }
                else
                {
                    int shopCart = 0;
                    if (int.TryParse(btnDone.CommandArgument, out shopCart))
                    {
                        return shopCart;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            set { btnDone.CommandArgument = value.ToString(); }
        }

        /// <summary>
        ///     Gets or sets the HFF shopping cart ID.
        /// </summary>
        public int HFFShoppingCartId
        {
            get
            {
                if (string.IsNullOrEmpty(btnMake.CommandArgument))
                {
                    return 0;
                }
                else
                {
                    int shopCart = 0;
                    if (int.TryParse(btnMake.CommandArgument, out shopCart))
                    {
                        return shopCart;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            set { btnMake.CommandArgument = value.ToString(); }
        }

        #endregion

        #region EventBus events

        [Publishes("HFFOrderPlaced")]
        public event EventHandler onHFFOrderPlaced;

        [Publishes("HFFOrderCreated")]
        public event EventHandler onHFFOrderCreated;

        [Publishes("HFFOrderCancel")]
        public event EventHandler onHFFOrderCancel;

        #endregion EventBus events

        protected void Page_Load(object sender, EventArgs e)
        {
            var paymentsControl =
                LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl);
            plPaymentOptions.Controls.Add(paymentsControl);
            PaymentOptions = paymentsControl as PaymentInfoBase;

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl))
            {
                var _paymentsSummary =
                    LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentsSummaryControl) as
                    PaymentsSummary;
                plPaymentSummary.Controls.Add(_paymentsSummary);
            }

            if (!IsPostBack)
            {
                ActiveShoppingCartId = ShoppingCart.ShoppingCartID;
                pEmailComment.InnerText = GetLocalResourceObject("EmailComment") as string;
                lbEmailValue.Text = Email;
                tbQuantity.Text = HLConfigManager.Configurations.DOConfiguration.HFFHerbalifeDefaultValue.ToString();
            }
            else
            {
                if (HFFShoppingCartId != 0 && ShoppingCart.ShoppingCartID != HFFShoppingCartId)
                {
                    ShoppingCart.ShoppingCartID = HFFShoppingCartId;
                    (Page as ProductsBase).RefreshFromService();
                }
            }
        }

        #region client events

        protected void btnMake_Click(object sender, EventArgs e)
        {
            int qty = 0;
            if (int.TryParse(tbQuantity.Text, out qty))
            {
                if (AddToCart(qty))
                {
                    onHFFOrderCreated(this, new EventArgs());

                    (PaymentOptions as PaymentInfoGrid).RefreshCardPayment();
                    PaymentOptions.Refresh();
                    lbTaxValue.Text = (ShoppingCart.Totals as OrderTotals_V01).TaxAmount.ToString("N2");
                    lbTotalValue.Text = (ShoppingCart.Totals as OrderTotals_V01).AmountDue.ToString("N2");
                    divHFFDonation_Step1.Attributes.Add("class",
                                                        string.Format("{0} hide",
                                                                      divHFFDonation_Step1.Attributes["class"]));
                    divHFFDonation_Step2.Attributes["class"] = divHFFDonation_Step2.Attributes["class"].Replace("hide",
                                                                                                                string
                                                                                                                    .Empty);
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            var _errors = new List<string>();
            if (ShoppingCart != null)
            {
                if (ShoppingCart.Totals == null)
                {
                    ShoppingCart.Calculate();
                }
                // Set the order
                var order = OrderCreationHelper.CreateOrderObject(ShoppingCart) as Order_V01;
                order.DistributorID = DistributorID;
                order.CountryOfProcessing = CountryCode;
                order.ReceivedDate = DateUtils.GetCurrentLocalTime(CountryCode);
                order.OrderMonth = GetOrderMonthShortString();
                order.OrderCategory =
                    (OrderCategoryType)
                    Enum.Parse(typeof (OrderCategoryType),
                               HLConfigManager.Configurations.DOConfiguration.HFFModalOrderType);
                order.Shipment = OrderProvider.CreateShippingInfo(CountryCode, ShoppingCart);
                var provider = (Page as ProductsBase).GetShippingProvider();
                (order.Shipment as ShippingInfo_V01).Address = ObjectMappingHelper.Instance.GetToOrder(provider.GetHFFDefaultAddress(ShoppingCart.DeliveryInfo.Address));
                order.Handling = OrderProvider.CreateHandlingInfo(CountryCode, string.Empty, ShoppingCart,
                                                                  order.Shipment as ShippingInfo_V01);
                List<Payment> payments = null;
                if (PaymentOptions != null)
                {
                    if (PaymentOptions.ValidateAndGetPayments(ObjectMappingHelper.Instance.GetToOrder(ShoppingCart.DeliveryInfo.Address.Address), out payments))
                    {
                        if (payments != null && payments.Count > 0)
                        {
                            order.Payments = new PaymentCollection();
                            order.Payments.AddRange((from p in payments select p).ToArray());
                        }

                        HLRulesManager.Manager.PerformTaxationRules(order, (Page as ProductsBase).Locale);

                        if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID,CountryCode))
                        {
                            if (order.PurchasingLimits == null)
                            {
                                var limits =
                                    PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
                                if (null != limits)
                                {
                                    limits.PurchaseSubType = ShoppingCart.SelectedDSSubType;
                                    order.PurchasingLimits = limits;
                                    order.PurchaseCategory = limits.PurchaseType;
                                }
                            }
                        }

                        order = OrderProvider.PopulateLineItems(CountryCode, order, ShoppingCart) as Order_V01;
                        order.DiscountPercentage = (ShoppingCart.Totals as OrderTotals_V01).DiscountPercentage;
                        var theOrder = OrderProvider.CreateOrder(order, ShoppingCart, CountryCode);
                        List<FailedCardInfo> failedCards = null;
                        string error = string.Empty;
                        string orderID = OrderProvider.ImportOrder(theOrder, out error, out failedCards,
                                                                   ShoppingCart.ShoppingCartID);

                        if (string.IsNullOrEmpty(error))
                        {
                            // TODO: how to get auth token
                            ShoppingCartProvider.UpdateShoppingCart(ShoppingCart,
                                                                    OrderProvider.SerializeOrder(theOrder, order,
                                                                                                 ShoppingCart,
                                                                                                 new Guid()),
                                                                    orderID, order.ReceivedDate);
                            if (string.IsNullOrEmpty(order.OrderID))
                            {
                                order.OrderID = orderID;
                                order.Pricing = ShoppingCart.Totals;
                            }
                            if (!String.IsNullOrEmpty(ShoppingCart.EmailAddress))
                            {
                                EmailHelper.SendEmail(ShoppingCart, order);
                            }

                            OrderSubmitted(order, orderID);

                            // Handling UI
                            lbOrderNumValue.Text = orderID;
                            divPayment.Attributes.Add("class",
                                                      string.Format("{0} hide", divSubmitCommand.Attributes["class"]));
                            divPaymentSummary.Attributes["class"] =
                                divEmailNotification.Attributes["class"].Replace("hide", string.Empty);

                            //hide the PaymentSummary if the Payment choice for HFF Order is Wire
                            if (SessionInfo.SelectedPaymentChoice == "WireTransfer")
                            {
                                divPaymentSummary.Attributes["class"] = "hide";
                            }
                            else
                            {
                                divPaymentSummary.Attributes["class"] =
                                    divEmailNotification.Attributes["class"].Replace("hide", string.Empty);
                            }

                            divOrderComplete.Attributes["class"] = divOrderComplete.Attributes["class"].Replace("hide",
                                                                                                                string
                                                                                                                    .Empty);
                            divEmailNotification.Attributes["class"] =
                                divEmailNotification.Attributes["class"].Replace("hide", string.Empty);
                            divSubmitCommand.Attributes.Add("class",
                                                            string.Format("{0} hide",
                                                                          divSubmitCommand.Attributes["class"]));
                            divEndCommand.Attributes["class"] = divEndCommand.Attributes["class"].Replace("hide",
                                                                                                          string.Empty);

                            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, CountryCode))
                            {
                                PurchasingLimitProvider.ReconcileAfterPurchase(ShoppingCart, DistributorID, CountryCode);
                            }

                            var currentSession = SessionInfo.GetSessionInfo(DistributorID, Locale);
                            if (currentSession != null)
                            {
                                if (!String.IsNullOrEmpty(currentSession.OrderNumber))
                                {
                                    currentSession.OrderNumber = String.Empty;
                                    currentSession.OrderMonthShortString = string.Empty;
                                    currentSession.OrderMonthString = string.Empty;
                                    currentSession.ShippingMethodNameMX = String.Empty;
                                    currentSession.ShippingMethodNameUSCA = String.Empty;
                                    currentSession.ShoppingCart.CustomerOrderDetail = null;
                                    // currentSession.CustomerPaymentSettlementApproved = false; Commented out for Merge. Need to investigate
                                    currentSession.CustomerOrderNumber = String.Empty;
                                    currentSession.CustomerAddressID = 0;
                                    if (null != currentSession.ShippingAddresses)
                                    {
                                        var customerAddress =
                                            currentSession.ShippingAddresses.Find(
                                                p => p.ID == currentSession.CustomerAddressID);
                                        if (customerAddress != null)
                                        {
                                            currentSession.ShippingAddresses.Remove(customerAddress);
                                        }
                                    }
                                }
                            }

                            //Clear the order month session...
                            Session["OrderMonthDataSessionKey"] = null;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);

                            if (ShoppingCart != null)
                            {
                                // take out quantities from inventory
                                ShoppingCartProvider.UpdateInventory(ShoppingCart, CountryCode, Locale, true);
                            }

                            ShoppingCart.CloseCart();
                            RecoverActiveCart();
                            onHFFOrderPlaced(this, new EventArgs());
                        }
                        else
                        {
                            LoggerHelper.Error(error);
                            if (error.Contains("AUTHORIZE PAYMENT"))
                            {
                                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                            }
                            else if (error.Contains("TIMEOUT"))
                            {
                                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                            }
                            else if (error.Contains("ORDER CANNOT BE FULFILLED FOR THE DISTRIBUTOR"))
                            {
                                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
                            }
                            else
                            {
                                _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail"));
                            }
                        }
                    }
                }
            }
        }

        protected void OnCancel(object sender, EventArgs e)
        {
            CancelOrder();
            onHFFOrderCancel(this, new EventArgs());
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            divHFFDonation_Step1.Attributes["class"] = divHFFDonation_Step1.Attributes["class"].Replace("hide",
                                                                                                        string.Empty);
            divHFFDonation_Step2.Attributes.Add("class",
                                                string.Format("{0} hide", divHFFDonation_Step2.Attributes["class"]));
        }

        #endregion client events

        #region private methods

        private bool AddToCart(int quantity)
        {
            bool added = false;
            try
            {
                if (ShoppingCart != null)
                {
                    // Verify the active shopping cart
                    if (ShoppingCart.ShoppingCartID == ActiveShoppingCartId)
                    {
                        ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                        // Create a new cart.
                        (Page as ProductsBase).ShoppingCart = ShoppingCartProvider.createShoppingCart(DistributorID,
                                                                                                      Locale);
                    }
                    else if (HFFShoppingCartId == 0)
                    {
                        HFFShoppingCartId = ShoppingCart.ShoppingCartID;
                    }
                    (Page as ProductsBase).ShoppingCart.OrderCategory = ServiceProvider.CatalogSvc.OrderCategoryType.RSO;
                    ShoppingCart.CheckAPFShipping();
                    ShoppingCart.AddHFFSKU(quantity);
                    ShoppingCartProvider.UpdateShoppingCart(ShoppingCart);
                    added = true;
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format("Error in HFFModal AddToCart. error message:  {0}; \r\n Stack Info: {1}",
                                  ex.GetBaseException().Message, ex.GetBaseException().StackTrace));
            }

            return added;
        }

        private string GetOrderMonthShortString()
        {
            var currentSession = SessionInfo;
            var orderMonth = new OrderMonth(CountryCode);
            if (null != currentSession)
            {
                if (!string.IsNullOrEmpty(currentSession.OrderMonthShortString))
                {
                    return currentSession.OrderMonthShortString;
                }
                else
                {
                    return orderMonth.OrderMonthShortString;
                }
            }
            else
            {
                return orderMonth.OrderMonthShortString;
            }
        }

        private void OrderSubmitted(Order_V01 order, string orderNumber)
        {
            var currentSession = SessionInfo;
            currentSession.OrderNumber = orderNumber;
            if (ShoppingCart.CustomerOrderDetail != null)
            {
                SettleCustomerOrderPayment(orderNumber);
            }
            SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
            ProductsBase.ResetInventoryView();
        }

        private bool SettleCustomerOrderPayment(string orderNumber)
        {
            var customerOrderV01 =
                CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
            bool orderIDUpdate = CustomerOrderingProvider.UpdateCustomerOrderDistributorOrderID(customerOrderV01.OrderID,
                                                                                             orderNumber);
            bool orderStatusUpdate = CustomerOrderingProvider.UpdateCustomerOrderStatus(customerOrderV01.OrderID,
                                                                                     customerOrderV01.OrderStatus,
                                                                                     ServiceProvider.CustomerOrderSvc.CustomerOrderStatusType.ShippedAsDo);
            return orderIDUpdate && orderStatusUpdate;
        }

        private void CancelOrder()
        {
            // If there is an internal active cart, just clear the cart and recover the previous one
            if (ActiveShoppingCartId != 0 & ShoppingCart.ShoppingCartID != ActiveShoppingCartId)
            {
                //when cancel HFF, a new cart is aleady created, Clear on new cart, clear APF too
                //ShoppingCart.ClearCart();
                RecoverActiveCart();
            }
        }

        private void RecoverActiveCart()
        {
            if (ActiveShoppingCartId != 0)
            {
                (Page as ProductsBase).ShoppingCart = ShoppingCartProvider.GetShoppingCart(DistributorID, Locale,
                                                                                           ActiveShoppingCartId);
                ActiveShoppingCartId = 0;
            }
        }

        #endregion private methods
    }
}