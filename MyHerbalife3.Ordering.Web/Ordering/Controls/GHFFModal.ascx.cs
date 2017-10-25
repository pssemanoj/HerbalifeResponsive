using System;
using System.Collections.Generic;
using System.Linq;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Payments;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using ShippingInfo = MyHerbalife3.Ordering.Providers.Shipping.ShippingInfo;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class GHFFModal : UserControlBase
    {
        public bool _canSubmitNow = false;
        private PaymentInfoBase PaymentOptions { get; set; }
        private ShippingInfo DeliveryInfo { get; set; }
        private int ActiveShoppingCartId { get; set; }

        #region EventBus events

        [Publishes("HFFOrderPlaced")]
        public event EventHandler onHFFOrderPlaced;

        [Publishes("HFFOrderCreated")]
        public event EventHandler onHFFOrderCreated;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            var paymentsControl =
                LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentOptionsControl);
            plPaymentOptions.Controls.Add(paymentsControl);
            PaymentOptions = paymentsControl as PaymentInfoBase;

            if (!IsPostBack)
            {
                var myHlShoppingCart = ShoppingCart;
                if (myHlShoppingCart != null)
                {
                    ActiveShoppingCartId = myHlShoppingCart.ShoppingCartID;
                    DeliveryInfo = myHlShoppingCart.DeliveryInfo;
                }
                pEmailComment.InnerText = GetLocalResourceObject("EmailComisment") as string;
                lbEmailValue.Text = Email;
                //this.btnMake.Click += new EventHandler(OnAddToCart);
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
                }

                PaymentOptions.Refresh();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (ShoppingCart != null)
            {
                ShoppingCart.CheckAPFShipping();

                // Set the order
                var order = OrderCreationHelper.CreateOrderObject(ShoppingCart) as Order_V01;
                order.DistributorID = DistributorID;
                order.CountryOfProcessing = CountryCode;
                order.ReceivedDate = DateUtils.GetCurrentLocalTime(CountryCode);
                order.OrderMonth = GetOrderMonthShortString();
                order.OrderCategory = (CountryCode == "GR") ? OrderCategoryType.APF : OrderCategoryType.RSO;
                //** RSO is just for testing (using other countries) and will always be APF
                order.Shipment = OrderProvider.CreateShippingInfo(CountryCode, ShoppingCart);
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
                            onHFFOrderPlaced(this, new EventArgs());
                        }
                    }
                }
            }
        }

        #endregion

        #region private methods

        private bool AddToCart(int quantity)
        {
            bool added = false;
            if (ShoppingCart != null)
            {
                ShoppingCart.AddHFFSKU(quantity);
                added = true;
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

        #endregion
    }
}