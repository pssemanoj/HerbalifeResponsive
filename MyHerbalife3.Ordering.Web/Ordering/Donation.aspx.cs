using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Ordering.Providers.Shipping;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout;
using HL.Common.Utilities;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.Providers.RulesManagement;
using HL.PGH.Api;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using DeliveryOptionType = MyHerbalife3.Ordering.ServiceProvider.ShippingSvc.DeliveryOptionType;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class Donation : ProductsBase
    {
        [Publishes(MyHLEventTypes.OnStandAloneDonation)]
        public event EventHandler OnStandAloneDonation;
        decimal _selfAmount = decimal.Zero;
        decimal _behalfAmount = decimal.Zero;
        decimal _otherAmount = decimal.Zero;
        decimal _totalDonation = decimal.Zero;
        int _skuQuantity = 0;
        List<string> errors = null;
        bool isAmountValid = false;
        private PaymentInfoBase _paymentOptions;
        private InvoiceOptions _invoicOptions;
        private List<Payment> _payments;
        private List<FailedCardInfo> _failedCards;
        private CheckoutTotalsDetailed _checkoutTotalsDetailed;

        private readonly List<string> _errors = new List<string>();
        private Controls.Payments.OneTimePin _oneTimePinControl;

        [Publishes(MyHLEventTypes.QuoteRetrieved)]
        public event EventHandler OnQuoteRetrieved;

        private bool _use3DSecuredCreditCard;
        private bool _isPaymentGatewayResponse;

        [Publishes(MyHLEventTypes.CreditCardAuthorizationCompleted)]
        public event EventHandler onCreditCardAuthorizationComplete;

        [SubscribesTo(MyHLEventTypes.ShoppingCartRecalculated)]
        public void OnCartRecalculated(object sender, EventArgs e)
        {
            //HideMessage();
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsChina)
            {
                _oneTimePinControl = LoadControl("~/Ordering/Controls/Payments/OneTimePin.ascx") as OneTimePin;
                _oneTimePinControl.ValidatePinEvent += _oneTimePinControl_ValidatePinEvent;
                OTPPanel.Controls.Add(_oneTimePinControl);
            }
            else
            {
                OTPPanel.Visible = false;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (SessionInfo.IsReplacedPcOrder && SessionInfo.ReplacedPcDistributorProfileModel != null)
            {
                mdlPCConfirm.Show();
                return;
            }
            Page.Title = GetLocalResourceObject("_PageHeader.Title") as string;
            (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("_PageHeader.Title") as string);


            var orders = OrdersProvider.GetOrdersInProcessing(DistributorID, Locale);

            if (orders != null && orders.Any())
            {
                var orderNumber = orders.FirstOrDefault().OrderId;
                ViewState["pendingOrderNumber"] = orderNumber;
                var isOrderSubmitted = CheckPendingOrderStatus("CN_99BillPaymentGateway", orderNumber, true);
                ViewState["isOrderSubmitted"] = isOrderSubmitted;
                if (isOrderSubmitted)
                {
                    lblDupeOrderMessage.Text =
                        string.Format(GetLocalResourceObject("PendingOrderSubmittedResource").ToString(),
                                      orderNumber);
                    dupeOrderPopupExtender.Show();
                }
            }
            

            if (!IsPostBack)
            {
                

                PaymentFailed(false);
                var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                if ((HLConfigManager.Configurations.DOConfiguration.IsChina && OrderTotals != null && (OrderTotals.AmountDue > 0)) && SessionInfo.StandAloneDonationError != "ERROR")
                {
                    mdlConfirm.Show();
                }
                else if (SessionInfo.StandAloneDonationError == "ERROR")
                {
                    PaymentFailed(true);
                }
                ClearDonationFields();

                //SessionInfo.ClearStandAloneDonation();
            }
            if (btn5Rmb.Checked == true || btn10Rmb.Checked == true)
            {
                txtOtherAmount.Text = String.Empty;
                txtOtherAmount.Enabled = false;
            }
            else
                txtOtherAmount.Enabled = true;

            if (btnBehalf5Rmb.Checked == true || btnBehalf10Rmb.Checked == true)
            {
                txtOtherAmount2.Text = String.Empty;
                txtOtherAmount2.Enabled = false;
            }
            else
                txtOtherAmount2.Enabled = true;

            LoadControls();

            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-md-12 gdo-nav-mid-plg");
        }

        protected void btnDonationNow_Click(object sender, EventArgs e)
        {

            try
            {
                if (blErrors != null) blErrors.Items.Clear();
                ShoppingCartID.Value = ShoppingCart.ShoppingCartID.ToString();
                divSubmitDonation.Visible = divCancelDonation.Visible = false;

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
                        if (Decimal.TryParse(txtOtherAmount.Text.ToString(), out _otherAmount))
                        {
                            _selfAmount = _otherAmount;

                        }
                    }

                }

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
                else if (!String.IsNullOrEmpty(txtOtherAmount2.Text.ToString()) && Decimal.TryParse(txtOtherAmount2.Text.ToString(), out _otherAmount))
                {
                    _behalfAmount = _otherAmount;

                }
                bool isValidDonation = true;
                if (_behalfAmount < 1 && _selfAmount < 1)
                {
                    isValidDonation = false;
                    ShowError("InvalidDonationAmount");
                }
                //On Behalf of downline Donation properties
                var cart = ShoppingCart as MyHLShoppingCart;
                cart.BehalfOfSelfAmount = cart.BehalfOfAmount = decimal.Zero;
                if (_behalfAmount >= 1)
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
                            return;
                        }
                    }
                    cart.BehalfDonationName = txtDonatorName.Text;
                    cart.BehalfOfContactNumber = phone;
                    cart.BehalfOfMemberId = DistributorID;
                    cart.BehalfOfAmount = _behalfAmount;
                    cart.BehalfOfSelfAmount = _selfAmount;
                }
                else if (_selfAmount >= 1)
                {
                    if (!isValidDonation)
                    {
                        if (errors != null && errors.Count > 0)
                        {
                            blErrors.DataSource = errors;
                            blErrors.DataBind();
                            return;
                        }
                    }
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
                    cart.BehalfDonationName = DistributorName;
                    cart.BehalfOfMemberId = DistributorID;
                    cart.BehalfOfAmount = _behalfAmount;
                    cart.BehalfOfSelfAmount = _selfAmount;
                }


                if ((_selfAmount >= 1 || _behalfAmount >= 1) && HLConfigManager.Configurations.DOConfiguration.DonationWithoutSKU)
                {

                    var myShoppingCart = (Page as ProductsBase).ShoppingCart;
                    ShippingProvider_CN objCNShipping = new ShippingProvider_CN();
                    myShoppingCart = objCNShipping.StandAloneAddressForDonation(myShoppingCart);

                    if (myShoppingCart.Totals == null)
                    {
                        myShoppingCart.Totals = new OrderTotals_V02();
                    }

                    OrderTotals_V02 totals = myShoppingCart.Totals as OrderTotals_V02;
                    if (totals == null)
                    {
                        totals = new OrderTotals_V02();
                    }
                    if (_selfAmount > decimal.Zero && _behalfAmount > decimal.Zero)
                        _totalDonation = _selfAmount + _behalfAmount;
                    else if (_selfAmount > decimal.Zero)
                        _totalDonation = _selfAmount;
                    else if (_behalfAmount > decimal.Zero)
                        _totalDonation = _behalfAmount;

                    totals.Donation = _totalDonation;
                    SessionInfo.StandAloneDonationNotSubmit = _totalDonation;
                    myShoppingCart.Calculate();
                    OnQuoteRetrieved(null, null);



                }
                if (errors != null && errors.Count > 0)
                {
                    blErrors.DataSource = errors;
                    blErrors.DataBind();
                    return;
                }
                else
                {
                    PaymentFailed(true);
                    OnStandAloneDonation(this, e);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("AddHFFFailed!\n" + ex);
            }
        }

        protected void btnDonationLater_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/PriceList.aspx");
        }

        private static bool AddressNotExists(ShippingInfo deliveryInfo)
        {
            if (deliveryInfo == null || deliveryInfo.Address == null ||
                deliveryInfo.Address.Address == null ||
                (string.IsNullOrEmpty(deliveryInfo.Address.Address.PostalCode) &&
                 string.IsNullOrEmpty(deliveryInfo.Address.Address.Line1) &&
                 string.IsNullOrEmpty(deliveryInfo.Address.Address.City))
                )
            {
                return true;
            }

            return false;
        }

        [SubscribesTo(MyHLEventTypes.WarehouseChanged)]
        public void OnWarehouseChanged(object sender, EventArgs e)
        {

        }

        protected void rdolistDonation_SelectedIndexChanged(object sender, EventArgs e)
        {
            //SelfDonation.Visible = rdolistDonation.SelectedValue == "1" ? true : false;
            //DonateOnBehalf.Visible = rdolistDonation.SelectedValue == "2" ? true : false;
            //btnOtherAmount.Visible = !DonateOnBehalf.Visible;
        }
        protected void btnYes_OnClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Ordering/PriceList.aspx", false);
        }
        protected void OnNoClick(object sender, EventArgs e)
        {
            ShoppingCart.CloseCart();
            SessionInfo.ClearStandAloneDonation();
            mdlConfirm.Hide();

        }

        protected void btnDonateYes_Click(object sender, EventArgs e)
        {
            ShoppingCart.ClearCart();
            Response.Redirect("~/Ordering/PriceList.aspx", false);
            SessionInfo.ClearStandAloneDonation();
        }
        protected void OnCancelDonation(object sender, EventArgs e)
        {
            ShoppingCart.ClearCart();
            Response.Redirect("~/Ordering/PriceList.aspx", false);
            SessionInfo.ClearStandAloneDonation();
        }
        //Donation Order process Click
        protected void OnSubmit(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.DOConfiguration.IsChina)
            {
                // LoadControls();
                var distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(ShoppingCart.DistributorID, "CN");
                var custid = distributorOrderingProfile != null ? distributorOrderingProfile.CNCustomorProfileID : 0;
                IDictionaryEnumerator en = Cache.GetEnumerator();
                while (en.MoveNext())
                {
                    if (
                        en.Key.ToString()
                          .StartsWith(string.Format(OrderListView.OrderListCaheKey + ShoppingCart.DistributorID)))
                    {
                        HttpRuntime.Cache.Remove(en.Key.ToString());
                    }
                }

                CacheFactory.Create().Expire(typeof(PromotionInformation), string.Format("CN_{0}_{1}", "PCPromotionInfo", custid.ToString().Trim()));

                string PCPromoSessionKey = ChinaPromotionProvider.PCPromoSessionKey(distributorOrderingProfile.CNCustomorProfileID.ToString());
                if (HttpContext.Current.Session[PCPromoSessionKey] != null)
                    Session.Remove(PCPromoSessionKey);
                //HttpContext.Current.Session[PCPromoSessionKey] = null;
            }
            //Check For Errors
            bool PaymentEntered = checkPayment();
            bool ShipmentEntered = checkShipment();
            //bool PickupInstructionsEntered = checkPickupInstructions();
            //bool checkPricing = CheckPricing();
            //bool EmailAddress = checkEmailAddress();

            if (!PaymentEntered || !ShipmentEntered)
            {
                //blErrors.DataSource = _errors;
                //blErrors.DataBind();
                return;
            }

            ShoppingCart.InvoiceOption = _invoicOptions != null ? _invoicOptions.SelectedInvoiceOption : null;
            SubmitOrder(String.Empty);
        }

        private bool checkPayment()
        {
            if (!HasZeroPriceEventTickets(ShoppingCart))
            {
                if (null != _paymentOptions)
                {
                    if (
                        !_paymentOptions.ValidateAndGetPayments(ObjectMappingHelper.Instance.GetToOrder(ShoppingCart.DeliveryInfo.Address.Address), out _payments))
                    {
                        //_errors.AddRange(_paymentOptions.Errors);
                        return false;
                    }
                }
                if (ShoppingCart.Totals != null && (ShoppingCart.Totals as OrderTotals_V01).AmountDue == 0.0M)
                {
                    // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TotalNotMatch"));
                    ShowError("TotalNotMatch");
                    return false;
                }

            }

            return true;
        }

        private bool checkShipment()
        {
            bool noError = true;
            if (ShoppingCart == null || ShoppingCart.DeliveryInfo == null)
                return false;

            if (ShoppingCart.DeliveryInfo == null || ShoppingCart.DeliveryInfo.Address == null || ShoppingCart.DeliveryInfo.Address.Address == null)
            {
                noError = false;
            }
            else if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Address.Line1))
            {
                if (!HLConfigManager.Configurations.PickupOrDeliveryConfiguration.DoNotCheckAddressLine1)
                {
                    noError = false;
                }
            }

            if (!noError)
            {
                if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Pickup)
                {
                    var config = HLConfigManager.Configurations.CheckoutConfiguration;
                    if (config.ShippingAddressRequiredForPickupOrder)
                    {
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoDefaultShippingAddress"));
                        ShowError("NoDefaultShippingAddress");
                    }
                    else
                    {
                        noError = true;
                    }
                }
                else
                {
                    // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "ShippingInfoIncomplete"));
                    ShowError("ShippingInfoIncomplete");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Recipient))
                {
                    var distributor = GlobalContext.CurrentDistributor;
                    if (distributor != null)
                        ShoppingCart.DeliveryInfo.Address.Recipient = string.Format("{0} {1}",
                                                                                    distributor.FirstName,
                                                                                    distributor.LastName);
                }

                //if (HLConfigManager.Configurations.DOConfiguration.IsChina && string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.Address.Phone))
                //{
                //    var OrderTotals = ShoppingCart != null ? ShoppingCart.Totals != null ? ShoppingCart.Totals as OrderTotals_V02 : null : null;
                //    if (OrderTotals != null && ShoppingCart.CartItems.Count == 0 && OrderTotals.Donation > Decimal.Zero)
                //    {
                //        ShoppingCart.DeliveryInfo.Address.Phone = "11111111111";
                //    }
                //    else
                //    {
                //        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPhone"));
                //        ShowError("NoPhone");
                //        noError = false;
                //    }

                //}
            }
            return noError;
        }
        private void SubmitOrder(string orderID)
        {

            if (string.IsNullOrEmpty(orderID))
            // If the order number is present, then don't show the popup. It is redirected from the Payment site.
            {
                var orderMonth = new OrderMonth(CountryCode);
                if (orderMonth.IsDualOrderMonth && !IsEventTicketMode &&
                    HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled)
                {
                }
                else
                {
                    DoSubmitOrder(orderID);
                }
            }
            else
            {
                DoSubmitOrder(orderID);
            }

        }

        private bool CheckPricing()
        {
            //if (!HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc)
            //{
            //    return true;
            //}

            //var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, CultureInfo.CurrentCulture.Name);
            //var pricingType = sessionInfo.ShoppingCart != null && sessionInfo.ShoppingCart.Totals != null
            //                      ? (sessionInfo.ShoppingCart.Totals as OrderTotals_V01).PricingType
            //                      : string.Empty;
            //if (HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc && sessionInfo.HmsPricing &&
            //    !string.IsNullOrEmpty(pricingType) && pricingType == "HMSCalc")
            //{
            //    return true;
            //}
            //else
            //{
            return false;
            //}
        }
        private static bool HasZeroPriceEventTickets(MyHLShoppingCart shoppingCart)
        {
            bool result = shoppingCart != null && shoppingCart.OrderCategory == OrderCategoryType.ETO &&
                          HLConfigManager.Configurations.DOConfiguration.AllowZeroPricingEventTicket &&
                          shoppingCart.ShoppingCartItems != null && shoppingCart.ShoppingCartItems.Any() && shoppingCart.ShoppingCartItems.Sum(i => i.CatalogItem.ListPrice) == 0;

            return result;
        }

        private void DoSubmitOrder(string orderID)
        {
            // Get the current logged in DS details.
            //var order = new Order_V01();

            if (HasZeroPriceEventTickets(ShoppingCart))
            {
                //Generate Order Number
                var request = new GenerateOrderNumberRequest_V01
                {
                    Amount = (ShoppingCart.Totals as OrderTotals_V01).AmountDue,
                    Country = CountryCode,
                    DistributorID = DistributorID
                };
                var response = OrderProvider.GenerateOrderNumber(request);
                if (null != response)
                {
                    orderID = response.OrderID;
                    //SessionInfo.OrderNumber = orderID;
                }
            }

            var order = OrderCreationHelper.CreateOrderObject(ShoppingCart) as Order_V01;
            // Populate generic/distributor details
            order.DistributorID = DistributorID;
            order.CountryOfProcessing = CountryCode;
            string countryCode = CountryCode;
            order.OrderID = orderID;
            var recvdDate = DateUtils.GetCurrentLocalTime(countryCode);
            order.ReceivedDate = recvdDate;
            //order.OrderMonth = getYYMMOrderMonth();
            //order.OrderMonth = (this.Page as ProductsBase).OrderMonthShortString;
            order.OrderMonth = GetOrderMonthShortString();
            order.OrderCategory =
                (ServiceProvider.OrderSvc.OrderCategoryType)Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), ShoppingCart.OrderCategory.ToString());
            //OrderCategoryType.RSO;
            if (order.OrderCategory == ServiceProvider.OrderSvc.OrderCategoryType.ETO)
            {
                if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType))
                {
                    order.OrderCategory =
                        (ServiceProvider.OrderSvc.OrderCategoryType)
                        Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType),
                                   HLConfigManager.Configurations.CheckoutConfiguration.EventTicketOrderType);
                }
            }

            if (APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, string.Empty))
            {
                OrderProvider.SetAPFDeliveryOption(ShoppingCart);
                order.OrderCategory =
                    (ServiceProvider.OrderSvc.OrderCategoryType)
                    Enum.Parse(typeof(ServiceProvider.OrderSvc.OrderCategoryType), HLConfigManager.Configurations.APFConfiguration.OrderType);
            }
            order.Shipment = OrderProvider.CreateShippingInfo(countryCode, ShoppingCart);

            // invoice option
            //string invOpt = _invoicOptions.SelectedInvoiceOption == null ? string.Empty : _invoicOptions.SelectedInvoiceOption;
            string invOpt = _invoicOptions == null ? string.Empty : _invoicOptions.SelectedInvoiceOption;
            order.Handling = OrderProvider.CreateHandlingInfo(CountryCode, invOpt, ShoppingCart,
                                                              order.Shipment as ShippingInfo_V01);

            // Populate payment information.
            if (_payments != null && _payments.Count > 0)
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in _payments select p).ToArray());
            }
            else if (SessionInfo.Payments != null && SessionInfo.Payments.Count > 0)
            // this is for 3D Secured orders; we put Payments in session before popup 3D bank page
            {
                order.Payments = new PaymentCollection();
                order.Payments.AddRange((from p in SessionInfo.Payments select p).ToArray());
            }
            else
            {
                if (!HasZeroPriceEventTickets(ShoppingCart))
                {
                    //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPaymentInfo"));
                    //ShowError("NoPaymentInfo");
                    //blErrors.DataSource = _errors;
                    //blErrors.DataBind();
                    //return;
                }
            }

            if (IsChina && !APFDueProvider.hasOnlyAPFSku(ShoppingCart.CartItems, Locale))
            {
                //Defensive handling to prevent wrong express company being assigned unintentionally.
                if (ShoppingCart.DeliveryInfo != null && (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping || ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier))
                {
                    if (string.IsNullOrEmpty(ShoppingCart.DeliveryInfo.FreightCode) || ShoppingCart.DeliveryInfo.FreightCode == "0")
                    {
                        HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                            "\nDS: " + ShoppingCart.DistributorID +
                                                            "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                            "\nAddress ID: " + ShoppingCart.DeliveryInfo.Address == null ? "" : ShoppingCart.DeliveryInfo.Address.ID +
                                                            "\nUrlReferrer: " + Request.UrlReferrer ?? "");

                        //Set this to bring the user attention.
                        if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier)
                        {
                            ShoppingCart.DeliveryInfo.FreightCode = "";
                            ShoppingCart.DeliveryInfo.Id = -1;
                        }

                        Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    else
                    {
                        if (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.Shipping)
                        {
                            if (ShoppingCart.CartItems.Any())
                            {
                                var deliveryOptions = base.GetShippingProvider().GetDeliveryOptionsListForShipping(CountryCode, Locale, ShoppingCart.DeliveryInfo.Address);

                                if (!deliveryOptions.Any(oi => oi.FreightCode.Equals(ShoppingCart.DeliveryInfo.FreightCode)))
                                {
                                    var availableFreightCode = String.Join(",", deliveryOptions.Select(a => a.FreightCode).ToList());
                                    HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                                           "\nDS: " + ShoppingCart.DistributorID +
                                                                            "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                                            "\nAddress ID: " + ShoppingCart.DeliveryInfo.Address == null ? "" : ShoppingCart.DeliveryInfo.Address.ID +
                                                                           "\nAvailable freight code: " + availableFreightCode +
                                                                           "\nUrlReferrer: " + Request.UrlReferrer ?? "");
                                    Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                                    return;
                                }
                            }

                        }
                        else
                        {
                            var deliveryOption = base.GetShippingProvider().GetShippingInfoFromID(ShoppingCart.DistributorID, Locale, ShoppingCart.DeliveryInfo.Option, ShoppingCart.DeliveryInfo.Id, 0);

                            if (deliveryOption == null || deliveryOption.FreightCode == null || !deliveryOption.FreightCode.Equals(ShoppingCart.DeliveryInfo.FreightCode))
                            {
                                HL.Common.Logging.LoggerHelper.Error("CnCheckOutOptions: Invalid Express Company error : FreightCode = " + ShoppingCart.DeliveryInfo.FreightCode +
                                                                       "\nDS: " + ShoppingCart.DistributorID +
                                                                        "\nDelivery Option: " + (ShoppingCart.DeliveryInfo.Option == DeliveryOptionType.PickupFromCourier ? "PickupFromCourier" : "Shipping") +
                                                                        "\nDelivery ID: " + ShoppingCart.DeliveryInfo.Id.ToString() +
                                                                       "\nAvailable freight code: " + ((deliveryOption == null || deliveryOption.FreightCode == null) ? "" : deliveryOption.FreightCode) +
                                                                       "\nUrlReferrer: " + Request.UrlReferrer ?? "");

                                //Set this to bring the user attention.
                                ShoppingCart.DeliveryInfo.FreightCode = "";
                                ShoppingCart.DeliveryInfo.Id = -1;

                                Response.Redirect("~/Ordering/ShoppingCart.aspx", false);
                                HttpContext.Current.ApplicationInstance.CompleteRequest();
                                return;
                            }
                        }
                    }
                }
            }
            SessionInfo.StandAloneDonationError = "STANDALONEDONATION";
            prepareOrderAndSubmit(order, countryCode);
        }
        private void prepareOrderAndSubmit(Order_V01 order, string countryCode)
        {
            HLRulesManager.Manager.PerformTaxationRules(order, (Page as ProductsBase).Locale);

            #region 3D Secured Credit Card: Determind if it is 3D Secured
            if (null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
            {
                _use3DSecuredCreditCard = true;
            }
            else
            {
                _use3DSecuredCreditCard = HLConfigManager.Configurations.PaymentsConfiguration.Use3DSecuredCreditCard
                                          && order.Payments.OfType<CreditPayment_V01>().Any()
                                          && (null == _paymentOptions || !_paymentOptions.IsUsingPaymentGateway);
            }
            #endregion 3D Secured Credit Card: Determind if it is 3D Secured

            if (PurchasingLimitProvider.RequirePurchasingLimits(DistributorID, countryCode))
            {
                if (order.PurchasingLimits == null)
                {
                    var limits = PurchasingLimitProvider.GetCurrentPurchasingLimits(DistributorID);
                    if (null != limits)
                    {
                        limits.PurchaseSubType = ShoppingCart.SelectedDSSubType;
                        order.PurchasingLimits = limits;
                        order.PurchaseCategory = limits.PurchaseType;
                    }
                }
            }

            if (HLConfigManager.Configurations.DOConfiguration.usesOrderManagementRules)
            {
                HLRulesManager.Manager.PerformOrderManagementRules(ShoppingCart, order, ShoppingCart.Locale, OrderManagementRuleReason.OrderBeingSubmitted);
            }

            //errors.Clear();
            order = OrderProvider.PopulateLineItems(CountryCode, order, ShoppingCart) as Order_V01;
            if (!_isPaymentGatewayResponse)
            {
                if (!_use3DSecuredCreditCard || (null != SessionInfo && null == SessionInfo.ThreeDSecuredCardInfo))
                {
                    ////Validate for Dupe
                    ////if (!ValidateForDupe(order, ShoppingCart.ShoppingCartID)) return;
                    //ValidateForDupe(order, ShoppingCart.ShoppingCartID);
                    //if (DupeOkClicked.Value != Boolean.TrueString)
                    //    return;
                }
            }

            string error = string.Empty;
            var currentSession = SessionInfo;

            if (ShoppingCart != null && ShoppingCart.Totals != null)
            {
                order.DiscountPercentage = (ShoppingCart.Totals as OrderTotals_V01).DiscountPercentage;

                if (IsChina)
                {
                    var payment = order.Payments.FirstOrDefault();

                    //Quick Pay payment handling.
                    if (payment != null && payment.TransactionType == "QP")
                    {
                        if (_oneTimePinControl != null)
                        {
                            if (!_oneTimePinControl.IsMobilePinProvided())
                            {
                                OrderNumber = ThreeDPaymentProvider.GenerateOrderNumberFor3DPayment((ShoppingCart.Totals as OrderTotals_V01).AmountDue, CountryCode, DistributorID);
                                SessionInfo.OrderNumber = OrderNumber;
                                if (string.IsNullOrEmpty(OrderNumber))
                                {
                                    SessionInfo.ThreeDSecuredCardInfo = null;
                                    SessionInfo.Payments = null;
                                    SessionInfo.OrderNumber = null;
                                    _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                                    blErrors.DataSource = _errors;
                                    blErrors.DataBind();
                                    return;
                                }

                                _oneTimePinControl.MobilePin = "";
                                _oneTimePinControl.RequestPin(OrderNumber, order, ShoppingCart);

                                if (!string.IsNullOrEmpty(_oneTimePinControl.LastErrorMessage))
                                {
                                    _errors.Add(_oneTimePinControl.LastErrorMessage);
                                    blErrors.DataSource = _errors;
                                    blErrors.DataBind();
                                }
                                return;
                            }
                            else
                            {
                                if (order.Payments != null && order.Payments.Count > 0)
                                {
                                    if (string.IsNullOrEmpty(order.OrderID))
                                    {
                                        order.OrderID = SessionInfo.OrderNumber;
                                    }

                                    ((order.Payments[0] as CreditPayment_V01).Card as QuickPayPayment).MobilePin = _oneTimePinControl.MobilePin;
                                    ((order.Payments[0] as CreditPayment_V01).Card as QuickPayPayment).Token = _oneTimePinControl.Token;
                                }
                            }
                        }
                    }
                }

                #region 3D Secured Credit Card: Check Enrollment and Redirect to Bank Page

                if (_use3DSecuredCreditCard && null != SessionInfo && null == SessionInfo.ThreeDSecuredCardInfo)
                {
                    string Agency = "MD";
                    var md = Request.Form[Agency] ?? Request.QueryString[Agency] ?? string.Empty;
                    if (string.IsNullOrEmpty(md))
                    {
                        // step 1: get order number before calling 3D Enrollment service
                        if (string.IsNullOrEmpty(order.OrderID))
                        {
                            OrderNumber = ThreeDPaymentProvider.GenerateOrderNumberFor3DPayment((ShoppingCart.Totals as OrderTotals_V01).AmountDue, CountryCode, DistributorID);
                            SessionInfo.OrderNumber = OrderNumber;
                            if (string.IsNullOrEmpty(OrderNumber))
                            {
                                SessionInfo.ThreeDSecuredCardInfo = null;
                                SessionInfo.Payments = null;
                                SessionInfo.OrderNumber = null;
                                //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                                ShowError("Resubmit");
                                //blErrors.DataSource = _errors;
                                //blErrors.DataBind();
                                return;
                            }
                        }
                        else
                        {
                            OrderNumber = order.OrderID;
                            SessionInfo.OrderNumber = OrderNumber;
                        }

                        // Step 2: Check 3D Secured credit card enrollment
                        var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                        var rootUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost);
                        var threeDSecuredCreditCard = ThreeDPaymentProvider.Check3DSecuredEnrollment(order.Payments, order.CountryOfProcessing, OrderNumber, DistributorID, Locale, rootUrl);

                        // Exception when calling Enrollment Check
                        if (null == threeDSecuredCreditCard || threeDSecuredCreditCard.IsErrored)
                        {
                            SessionInfo.ThreeDSecuredCardInfo = null;
                            SessionInfo.Payments = null;
                            SessionInfo.OrderNumber = null;
                            // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                            ShowError("Resubmit");
                            //blErrors.DataSource = _errors;
                            //blErrors.DataBind();
                            return;
                        }

                        // Step 3: if enrolled, generate popup to go to issuer bank page, and handle the responese; 
                        // if not enrolled, move to create order and submit; 
                        SessionInfo.Payments = _payments;
                        SessionInfo.ThreeDSecuredCardInfo = threeDSecuredCreditCard;

                        // CC is Enrolled but not been to bank page, popup to go to bank page
                        if (threeDSecuredCreditCard.IsEnrolled && !threeDSecuredCreditCard.IsAuthenticated)
                        {
                            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/Controls/Payments/3DSecured/ThreeDAuthenticate.aspx");

                            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", "LoadUrl('" + termUrl + "');", true);
                            btnModalPopup_ModalPopupExtender.Show();
                            return;
                        }
                    }
                }
                #endregion 3D Secured Credit Card: Check Enrollment and Redirect to Bank Page

                // Create theOrder.
                #region 3D Secured Credit Card: Create Order
                object theOrder = null;
                if (_use3DSecuredCreditCard && null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
                {
                    // CC is enrolled but not authenticated, will not submit. 
                    if (SessionInfo.ThreeDSecuredCardInfo.IsEnrolled && (!SessionInfo.ThreeDSecuredCardInfo.IsAuthenticated || SessionInfo.ThreeDSecuredCardInfo.IsErrored))
                    {
                        RemoveDupCheckCookie();
                        LoggerHelper.Error(string.Format("Order Payment error when submitting order:{0} Error : {1}", OrderNumber,
                                          "Credit Card is 3D Secured Enrolled but not Authenticated or is Errored"));
                        SessionInfo.ThreeDSecuredCardInfo = null;
                        SessionInfo.Payments = null;
                        SessionInfo.OrderNumber = null;
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                        ShowError("PaymenyFail");
                        //blErrors.DataSource = _errors;
                        //blErrors.DataBind();
                        return;
                    }

                    // Generate 3D CC theOrder when: (1) Enrolled and Authenticated without errored, or (2) Not Enrolled. Need to pass Enrollment and Authentication status data in either case.
                    if (string.IsNullOrEmpty(order.OrderID))
                    {
                        order.OrderID = SessionInfo.OrderNumber;
                    }
                    theOrder = OrderProvider.CreateOrder(order, ShoppingCart, countryCode, SessionInfo.ThreeDSecuredCardInfo);
                    ThreeDPaymentProvider.Update3DPaymentRecord(SessionInfo.OrderNumber, SessionInfo.ThreeDSecuredCardInfo, ThreeDPaymentStatus.OrderCreated);
                }
                else
                {
                    theOrder = OrderProvider.CreateOrder(order, ShoppingCart, countryCode);
                }
                #endregion 3D Secured Credit Card: Create Order

                if (theOrder != null)
                {
                    (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).AgreedDuplicate = "true";//DupeOkClicked.Value;
                    if (Settings.GetRequiredAppSetting("DupCheckWithReferenceId", false))
                    {
                        //if (_use3DSecuredCreditCard && null != SessionInfo && null != SessionInfo.ThreeDSecuredCardInfo)
                        //{
                        //    if (string.IsNullOrEmpty(OrderRefId.Value))
                        //    {
                        //        string refId = GetDupCheckCookie();
                        //        if (!string.IsNullOrEmpty(refId))
                        //            (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = refId;
                        //    }
                        //    else
                        //        (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = OrderRefId.Value;
                        //}
                        if (!_use3DSecuredCreditCard && (_paymentOptions != null && !_paymentOptions.IsUsingPaymentGateway))
                        {
                            (theOrder as ServiceProvider.SubmitOrderBTSvc.Order).ReferenceID = OrderRefId.Value;
                        }
                    }
                }
                //Redirect if using payment gateway (make sure we're not returning from one currently)
                if (!_isPaymentGatewayResponse && null != _paymentOptions && _paymentOptions.IsUsingPaymentGateway)
                {
                    if (!HLConfigManager.Configurations.PaymentsConfiguration.IsUsingHub)
                    {
                        if (!string.IsNullOrEmpty(SessionInfo.PendingOrderId) &&
                            HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            var orders = OrderProvider.GetPaymentGatewayOrder(SessionInfo.PendingOrderId);
                            if (orders != null)
                            {
                                bool valid = OrderProvider.ValidateOrders(orders.BTOrder, theOrder as ServiceProvider.SubmitOrderBTSvc.Order);
                                if (!valid)
                                {
                                    TermConditionPopupExtender.Show();
                                    return;

                                }
                            }
                        }

                        //OrderAmount = double.Parse((ShoppingCart.Totals as OrderTotals_V01).AmountDue.ToString());
                        OrderProvider.ValidateInstructions(theOrder, order, ShoppingCart);

                        string creditCardNumber = "";
                        string cvv = "";

                        if (IsChina)
                        {
                            if (((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments != null && ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments.Count() > 0)
                            {
                                creditCardNumber = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber;
                                cvv = ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV;

                                if (!string.IsNullOrEmpty(creditCardNumber))
                                {
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = "123";

                                    if (((order.Payments[0]) as CreditPayment_V01) != null && ((order.Payments[0]) as CreditPayment_V01).Card != null)
                                    {
                                        ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = PaymentInfoProvider.GetDummyCreditCardNumber(IssuerAssociationType.Visa);
                                        ((order.Payments[0]) as CreditPayment_V01).Card.CVV = "123";
                                    }
                                }
                            }
                        }

                        // TODO: how to get auth token
                        string serializedOrder = OrderProvider.SerializeOrder(theOrder, order, ShoppingCart, new Guid());
                        Session[PaymentGatewayInvoker.PaymentGateWayOrder] = serializedOrder;

                        if (IsChina)
                        {
                            if (((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments != null && ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments.Count() > 0)
                            {
                                if (!string.IsNullOrEmpty(creditCardNumber))
                                {
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].AccountNumber = creditCardNumber;
                                    ((ServiceProvider.SubmitOrderBTSvc.Order)theOrder).Payments[0].CVV = cvv;

                                    if (((order.Payments[0]) as CreditPayment_V01) != null && ((order.Payments[0]) as CreditPayment_V01).Card != null)
                                    {
                                        ((order.Payments[0]) as CreditPayment_V01).Card.AccountNumber = creditCardNumber;
                                        ((order.Payments[0]) as CreditPayment_V01).Card.CVV = cvv;
                                    }
                                }
                            }
                        }

                        Response.Redirect("PaymentGatewayManager.aspx");
                    }
                    else
                    {
                        PaymentRequest pghRequest = _paymentOptions.PaymentGatewayInterface.PaymentRequest;
                        pghRequest.OrderHolder = OrderProvider.GetSerializedOrderHolder(theOrder, order, ShoppingCart, new Guid()).ToSafeString();
                        Session.Add(PGHInterface.PageName, pghRequest);
                        if (_paymentOptions.PaymentGatewayInterface.CanUsePGHWindow(pghRequest.PaymentMethod))
                        {
                            var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
                            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/" + PGHInterface.PageName);
                            ScriptManager.RegisterStartupScript(this, GetType(), "ScriptPopup", "LoadUrl('" + termUrl + "');", true);
                            btnModalPopup_ModalPopupExtender.Show();
                            return;
                        }
                        else
                        {
                            Response.Redirect("PaymentGatewayManager.aspx");
                        }
                    }
                }


                if (!string.IsNullOrEmpty(currentSession.PendingOrderId))
                {
                    OrderProvider.UpdatePaymentGatewayRecord(currentSession.PendingOrderId, "Pending Order submitting thru Pay be Phone", PaymentGatewayLogEntryType.Unknown, PaymentGatewayRecordStatusType.Declined);
                    currentSession.PendingOrderId = string.Empty;
                    SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
                }

                //var dupReferenceIdCheck = Settings.GetRequiredAppSetting("DupCheckWithReferenceId", false);
                //if (dupReferenceIdCheck)
                //{
                //    //Check For Dup Order thru Cookie
                //    var btOrder = theOrder as Providers.OrderImportBtWS.Order;

                //    if (btOrder != null)
                //    {
                //        SetDupCheckCookie(btOrder.ReferenceID);
                //    }
                //}
                string orderID = OrderProvider.ImportOrder(theOrder, out error, out _failedCards,
                                                           ShoppingCart.ShoppingCartID);
                //DupeCheckDone.Value = string.Empty;

                #region 3D Secured Credit Card: Clean up after Order is Submitted

                SessionInfo.ThreeDSecuredCardInfo = null;

                #endregion 3D Secured Credit Card: Clean up after Order is Submitted

                if (string.IsNullOrEmpty(error) || error.Trim().ToUpper().Contains("PROCESSING") || error.Trim().ToUpper().Contains("KOUNTREVIEW"))
                {
                    try
                    {
                        if (error.Trim().ToUpper().Contains("PROCESSING"))
                        {
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedProcessing;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }
                        if (error.Trim().ToUpper().Contains("KOUNTREVIEW"))
                        {
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedInReview;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }

                        // Verify if it is a BR order in FControl status pending
                        if (HLConfigManager.Configurations.CheckoutConfiguration.FraudControlEnabled && orderID.Contains("PENDING-FCONTROL"))
                        {
                            orderID = orderID.Replace("PENDING-FCONTROL-", string.Empty);
                            SessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmittedPending;
                            SessionInfo.SetSessionInfo(DistributorID, Locale, SessionInfo);
                        }
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

                        // if is a customer order, check email address.
                        if (ShoppingCart.CustomerOrderDetail != null)
                        {
                            if (HLConfigManager.Configurations.CheckoutConfiguration.RequireEmail &&
                                string.IsNullOrEmpty(ShoppingCart.EmailAddress))
                            {
                                var sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
                                if (!String.IsNullOrEmpty(sessionInfo.ChangedEmail))
                                {
                                    ShoppingCart.EmailAddress = sessionInfo.ChangedEmail;
                                }
                            }
                        }

                        if (!String.IsNullOrEmpty(ShoppingCart.EmailAddress))
                        {
                            EmailHelper.SendEmail(ShoppingCart, order);
                        }

                        onCreditCardAuthorizationComplete(this, new EventArgs());
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(string.Format("Order submitted successfully but error occurs: OrderNumber:{0} Error : {1}", orderID, ex.Message));
                    }
                    OrderSubmitted(order, orderID);
                }
                else
                {
                    if (error.Contains("DUPLICATE"))
                    {
                        ////Your previous order was already submitted prior to the changes that were made to your cart. Your order number is <Order Number>. You can verify the status of this order by calling Order Support at 1-866-866-4744, 9 a.m. to 6 p.m. PST, Monday - Friday. Saturday 6 a.m. to 2 p.m. PST for assistance or by clicking here. (Please note that it may take up to 1 hour before the order is displayed.)
                        ////_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateOrder"));
                        //if (orderID != string.Empty)
                        //{
                        //    DisplayPreviousOrderSubmitted(orderID);
                        //}
                        //else
                        //{
                        //    //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail"));
                        //    ShowError("TransactionFail");
                        //}
                    }
                    else if (error.Contains("AUTHORIZE PAYMENT"))
                    {
                        RemoveDupCheckCookie();
                        if (error.Contains("KOUNTDECLINED"))
                            //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "KountDeclinedOrder"));
                            ShowError("KountDeclinedOrder");
                        else
                            // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                            ShowError("PaymenyFail");
                        if (_failedCards.Count > 0)
                        {
                            onCreditCardAuthorizationComplete(this,
                                                              new CreditCardAuthorizationFailedEventArgs(_failedCards, order.Payments.Count));
                        }
                    }
                    else if (error.Contains("TIMEOUT"))
                    {
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "Resubmit"));
                        ShowError("Resubmit");
                    }
                    else if (error.Contains("ORDER CANNOT BE FULFILLED FOR THE DISTRIBUTOR"))
                    {
                        // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "CantOrder"));
                        ShowError("CantOrder");
                    }
                    else if (error.Contains("DECLINED-FCONTROL"))
                    {
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "FControlDeclined"));
                        ShowError("FControlDeclined");
                        if (_failedCards.Count > 0)
                        {
                            onCreditCardAuthorizationComplete(this,
                                                              new CreditCardAuthorizationFailedEventArgs(_failedCards));
                        }
                    }
                    else
                    {
                        //_errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "TransactionFail"));
                        ShowError("TransactionFail");
                    }
                    //blErrors.DataSource = _errors;
                    //blErrors.DataBind();
                }
            }
            if (errors != null && errors.Count > 0)
            {
                blErrors.DataSource = errors;
                blErrors.DataBind();
                return;
            }
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
            try
            {
                //this.OrderNumber = OrderNumber;
                var currentSession = SessionInfo;
                currentSession.OrderNumber = orderNumber;
                if (ShoppingCart.CustomerOrderDetail != null)
                {
                    SettleCustomerOrderPayment(orderNumber);
                }
                currentSession.ThreeDSecuredCardInfo = null;
                currentSession.Payments = null;
                SessionInfo.SetSessionInfo(DistributorID, Locale, currentSession);
                ResetInventoryView();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Order submitted successfully but error occurs in OrderSubmitted method: OrderNumber:{0} Error : {1}",
                        orderNumber, ex.Message));
            }
            Response.Redirect("Confirm.aspx?OrderNumber=" + orderNumber);
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
        protected virtual void LoadControls()
        {
            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl))
            {
                if (!HasZeroPriceEventTickets(ShoppingCart))
                {
                    var paymentsControl = LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl);
                    plPaymentOptions.Controls.Add(paymentsControl);
                    _paymentOptions = paymentsControl as PaymentInfoBase;
                }
            }

            if (!String.IsNullOrEmpty(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsMiniControl))
            {
                var _checkoutTotalsMini =
                    LoadControl(HLConfigManager.Configurations.CheckoutConfiguration.CheckoutTotalsMiniControl);
                plCheckOutTotalsMini.Controls.Add(_checkoutTotalsMini);
            }

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
        private void PaymentFailed(Boolean bln)
        {
            var cart = ShoppingCart as MyHLShoppingCart;

            divPaymentGrid.Visible = divdonationSummary.Visible = bln;
            divSubmitDonation.Visible = divCancelDonation.Visible = bln;

            if (bln)
            {
                if (SessionInfo.StandAloneDonationError == "ERROR")
                {
                    ShowError("PaymenyFail");
                    if (errors != null && errors.Count > 0)
                    {
                        blErrors.DataSource = errors;
                        blErrors.DataBind();
                        //return;
                    }

                }
                SessionInfo.StandAloneDonationError = "NOERROR";
                // _errors.Add(PlatformResources.GetGlobalResourceString("ErrorMessage", "PaymenyFail"));
                //blErrors.DataSource = _errors;
                //blErrors.DataBind();
                //if (behalfOfOther.Value == "3" && cart.BehalfOfAmount != null && cart.BehalfOfAmount > 0)
                //{
                //    btnBehalfOther.Checked = true;
                //    txtDonatorName.Text = cart.BehalfDonationName;
                //    txtContactNumber.Text = cart.BehalfOfContactNumber;
                //    txtOtherAmount2.Text = Convert.ToString(cart.BehalfOfAmount);
                //}
                //else
                //{  txtDonatorName.Text = String.Empty;
                //    txtContactNumber.Text = String.Empty;
                //    txtOtherAmount2.Text = String.Empty;
                //}
                //if (SelfOther.Value == "3" && cart.BehalfOfSelfAmount != null && cart.BehalfOfSelfAmount > 0)
                //{
                //    btnOtherAmount.Checked = true;
                //    txtOtherAmount2.Text = Convert.ToString(cart.BehalfOfSelfAmount);
                //}
                //else
                //    txtOtherAmount2.Text = String.Empty;

            }

        }
        private void ClearDonationFields()
        {
            btn5Rmb.Checked = true;
            txtOtherAmount.Enabled = false;
            txtOtherAmount2.Enabled = true;
            btnBehalfOther.Checked = true;
            txtOtherAmount.Text = txtOtherAmount2.Text = String.Empty;
            txtDonatorName.Text = txtContactNumber.Text = String.Empty;

        }

        private void _oneTimePinControl_ValidatePinEvent(object sender, ValidatePinEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.MobilePin))
            {
                OnSubmit(null, null);
            }
        }
        protected void OnDupeOrderOK(object sender, EventArgs e)
        {
            dupeOrderPopupExtender.Hide();
            var pendingOrderNumber = ViewState["pendingOrderNumber"];
            var checkOrderSubmitted = ViewState["isOrderSubmitted"];
            if (checkOrderSubmitted != null && (bool)checkOrderSubmitted)
            {
                RedirectPendingOrder(pendingOrderNumber.ToString(), true);
            }
        }
    }

}