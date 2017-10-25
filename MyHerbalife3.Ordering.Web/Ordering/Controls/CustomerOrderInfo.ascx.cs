using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.SharedProviders;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Ordering.ServiceProvider.CustomerOrderSvc;
using Resources;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class CustomerOrderInfo : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SessionInfo sessionInfo = SessionInfo.GetSessionInfo(DistributorID, Locale);
            CustomerOrder_V01 customerOrderV01 =
                CustomerOrderingProvider.GetCustomerOrderByOrderID(ShoppingCart.CustomerOrderDetail.CustomerOrderID);
            if (customerOrderV01 != null)
            {
                lblCustomerOrderDate.Text = customerOrderV01.Submitted.HasValue
                                                ? customerOrderV01.Submitted.Value.ToString()
                                                : String.Empty;
                lblCustomerOrderNumber.Text = customerOrderV01.FriendlyId;
                lblCustomerName.Text = String.Format("{0} {1} {2}",
                                                     ((CustomerShippingInfo_V01) customerOrderV01.Shipping).Recipient
                                                                                                           .First,
                                                     ((CustomerShippingInfo_V01) customerOrderV01.Shipping).Recipient
                                                                                                           .Middle,
                                                     ((CustomerShippingInfo_V01) customerOrderV01.Shipping).Recipient
                                                                                                           .Last);

                lblOrderStatus.Text = GetCustomerOrderStatus(customerOrderV01.OrderStatus);

                lblCustomerComments.Text = customerOrderV01.CustomerNote;
                lblCustomerPaymentPreference.Text = GetPaymentMethodChoice(customerOrderV01);
                lblCustomerPrefferedShippingMethod.Text =
                    GetCustomerShippingSpeed(((ServiceProvider.CustomerOrderSvc.CustomerShippingInfo_V01)customerOrderV01.Shipping).ShippingSpeed);
                    //((CustomerShippingInfo_V01) customerOrderV01.Shipping).ShippingSpeed.ToString();
            }

            if (!sessionInfo.CustomerOrderAddressWasValid)
            {
                lblErrors.Visible = true;
                lblErrors.Text = GetLocalResourceObject("CustomerAddressNotValid").ToString();
            }

            var dictionary = new Dictionary<string, string>();
            var orderTagValues = Enum.GetNames(typeof (CustomerOrderStatusTag)).ToList();

            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("DistributorSelfNoteTags");

            dictionary.Add("0", GetLocalResourceObject("Select").ToString());

            foreach (var entry in entries.Where(entry => orderTagValues.Contains(entry.Key)
                                                         &&
                                                         (CustomerOrderStatusTag)
                                                         Enum.Parse(typeof (CustomerOrderStatusTag), entry.Key) !=
                                                         CustomerOrderStatusTag.None))
            {
                dictionary.Add(entry.Key, entry.Value);
            }

            ddlNotesToSelf.DataSource = dictionary;
            ddlNotesToSelf.DataTextField = "Value";
            ddlNotesToSelf.DataValueField = "Key";
            ddlNotesToSelf.DataBind();

            if (ShoppingCart.CustomerOrderDetail != null)
            {
                if (customerOrderV01 != null)
                {
                    ListItem liSelectedTag = ddlNotesToSelf.Items.FindByValue(customerOrderV01.StatusTag.ToString());
                    if (liSelectedTag != null)
                    {
                        liSelectedTag.Selected = true;
                    }
                }
            }

            if (!String.IsNullOrEmpty(sessionInfo.OrderNumber))
            {
                ddlNotesToSelf.Enabled = false;

                if (customerOrderV01.PaymentMethodChoice == ServiceProvider.CustomerOrderSvc.CustomerPaymentMethodChoice.CreditCardOnline)
                {
                    dvPaymentStatus.Visible = true;
                    AuthorizationClientResponseType paymentStatus = GetPaymentStatus(customerOrderV01.CardAuthorizations);
                    if (paymentStatus == AuthorizationClientResponseType.Approved)
                    {
                        lblCustomerPaymentStatus.Text = GetLocalResourceObject("PaymentStatusApproved").ToString();
                    }
                    else
                    {
                        lblCustomerPaymentStatus.Text = GetLocalResourceObject("PaymentStatusDeclined").ToString();
                    }
                }
            }
        }

        private AuthorizationClientResponseType GetPaymentStatus(IEnumerable<CardAuthorization> cardAuthorizations)
        {
            if (cardAuthorizations == null)
                return AuthorizationClientResponseType.None;

            if (cardAuthorizations.ToList().Count == 0)
                return AuthorizationClientResponseType.None;

            CardAuthorization_V01 settleAuth =
                cardAuthorizations.OfType<CardAuthorization_V01>()
                                  .SingleOrDefault(
                                      a => a.AuthorizationIntention == AuthorizationIntentionType.Capture);

            if (settleAuth == null)
                return AuthorizationClientResponseType.None;

            return
                settleAuth.ClientResponse;
        }

        protected void ddlNotesToSelf_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlNotesToSelf.SelectedItem.Value != "0")
            {
                if (ShoppingCart != null && ShoppingCart.CustomerOrderDetail != null)
                {
                    CustomerOrderingProvider.UpdateCustomerOrderTags(ShoppingCart.CustomerOrderDetail.CustomerOrderID,
                                                                  (CustomerOrderStatusTag)
                                                                  Enum.Parse(typeof (CustomerOrderStatusTag),
                                                                             ddlNotesToSelf.SelectedItem.Value));
                    upCustOrderInfo.Update();
                }
            }
        }

        private string GetCustomerOrderStatus(CustomerOrderStatusType customerOrderStatusType)
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("CustomerOrderStatusTypeEnum");
            foreach (var entry in entries.Where(entry => customerOrderStatusType.ToString() == entry.Key.ToString()))
            {
                return entry.Value;
            }

            return customerOrderStatusType.ToString();
        }

        private string GetCustomerShippingSpeed(ServiceProvider.CustomerOrderSvc.CustomerShippingMethod customerShippingSpeed)
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("CustomerShippingSpeed");
            foreach (var entry in entries.Where(entry => customerShippingSpeed.ToString() == entry.Key.ToString()))
            {
                return entry.Value;
            }
            return customerShippingSpeed.ToString();
        }

        private string GetCustomerPaymentMethodChoice(ServiceProvider.CustomerOrderSvc.CustomerPaymentMethodChoice customerPaymentMethodChoice)
        {
            var entries = GlobalResourceHelper.GetGlobalEnumeratorElements("CustomerPaymentMethodChoice");
            foreach (var entry in entries.Where(entry => customerPaymentMethodChoice.ToString() == entry.Key.ToString()))
            {
                return entry.Value;
            }
            return customerPaymentMethodChoice.ToString();
        }

        private string GetPaymentMethodChoice(ServiceProvider.CustomerOrderSvc.CustomerOrder_V01 customerOrder)
        {
            var paymentMethodChoice = customerOrder.PaymentMethodChoice.ToString();
            var paypalPayment = customerOrder.Payments.OfType<PayPalPayment_V01>().FirstOrDefault();

            if (customerOrder.PaymentMethodChoice == ServiceProvider.CustomerOrderSvc.CustomerPaymentMethodChoice.CreditCardRedirect
                && paypalPayment != null)
            {
                if (paypalPayment.SolutionType == "SOLE")
                {
                    paymentMethodChoice = @"Credit\Debit card";
                }
                else
                {
                    paymentMethodChoice = "PayPal Account";
                }
            }
            var value = GetCustomerPaymentMethodChoice(customerOrder.PaymentMethodChoice);
            return string.IsNullOrEmpty(value) ? paymentMethodChoice : value;
        }
    }
}