using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using AjaxControlToolkit;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class OrderPreferences : ProductsBase
    {
        #region fields

        private ModalPopupExtender mpPaymentInformation;

        //PaymentInfoBase PaymentInformationBase;
        private List<PaymentInformation> paymentInformations;
        private List<PickupLocationPreference_V01> pickupLocationPreferences;
        private PaymentInformation primaryPaymentInformation;
        private PickupLocationPreference_V01 primaryPickupLocationPreference;
        private DeliveryOption primaryShippingAddress;
        private List<DeliveryOption> shippingAddresses;

        private UserControlBase ucPaymentInfoControl;

        #endregion fields

        #region event

        [Publishes(MyHLEventTypes.CreditCardProcessing)]
        public event EventHandler onCreditCardProcessing;

        #endregion event

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                (Master as OrderingMaster).SetPageHeader(GetLocalResourceObject("PageResource1.Title") as string);
                if (ShoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                {
                    lbOrderPref.Text = GetLocalResourceObject("ETOOrderPreferencesHeader").ToString();
                }
                else
                {
                    lbOrderPref.Text = GetLocalResourceObject("RSOOrderPreferencesHeader").ToString();
                }
            }

            var paymentsControl =
                LoadControl(HLConfigManager.Configurations.PaymentsConfiguration.PaymentInfoControl);
            ucPaymentInfoControl = paymentsControl as UserControlBase;
            phPaymentInfoControl.Controls.Add(paymentsControl);

            if (HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry)
            {
                lbtAddPaymentInformation.Visible = false;
            }

            dvSavedPaymentInformation.Visible = HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards;
            dvSavedShippingAddress.Visible = HLConfigManager.Configurations.DOConfiguration.AllowShipping;
            processShippingAddress();
            processPaymentInformation();
            processPickupLocation();
            processPUFromCourierLocation();
            (Master as OrderingMaster).gdoNavMidCSS("gdo-nav-mid col-sm-10 gdo-nav-mid-op");
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressCreated)]
        public void OnShippingAddressCreated(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
        }

        [SubscribesTo(MyHLEventTypes.CreditCardProcessed)]
        public void onCreditCardProcessed(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceCreated)]
        public void OnPickupPreferenceCreated(object sender, EventArgs e)
        {
            Response.Redirect(GetRequestURLWithOutPort());
        }

        #region Shippping Address process

        /// <summary>
        ///     process shipping Address
        /// </summary>
        private void processShippingAddress()
        {
            if (getShippingAddressList() == null || shippingAddresses.Count() == 0)
            {
                dvViewAllShippingAddress.Visible = false;
                dvNonPrimaryShippingAddress.Visible = true;
            }
            else
            {
                dvViewAllShippingAddress.Visible = true;
                lbtShowAllShippingAddress.PostBackUrl = "/Ordering/SavedShippingAddress.aspx";

                if ((primaryShippingAddress = shippingAddresses.Where(s => s.IsPrimary).FirstOrDefault()) == null)
                {
                    dvPrimaryShippingAddress.Visible = false;
                    dvNonPrimaryShippingAddress.Visible = true;
                    lblPickupDisplay.Visible = false;
                }
                else
                {
                    string formattedAddress = (Page as ProductsBase).GetShippingProvider().
                                                                     FormatOrderPreferencesAddress(
                                                                         primaryShippingAddress);
                    var pAddress = new HtmlGenericControl();
                    pAddress.InnerHtml = formattedAddress;
                    pnlPrimaryShippngAddress.Controls.Add(pAddress);

                    dvPrimaryShippingAddress.Visible = true;
                    dvNonPrimaryShippingAddress.Visible = false;
                }
            }
        }

        private void RemoveDashForPhone(DeliveryOption primaryShippingAddress)
        {
            string phone = primaryShippingAddress.Phone;

            if (!string.IsNullOrEmpty(phone))
            {
                if (phone.StartsWith("-"))
                    phone = phone.Remove(0, 1);
                if (phone.EndsWith("-"))
                    phone = phone.Remove(phone.Length - 1, 1);
                primaryShippingAddress.Phone = phone;
            }
        }

        /// <summary>
        ///     get primary shipping address
        /// </summary>
        /// <returns></returns>
        private List<DeliveryOption> getShippingAddressList()
        {
            try
            {
                shippingAddresses =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetShippingAddresses((Page as ProductsBase).DistributorID,
                                                                (Page as ProductsBase).Locale);

                if (shippingAddresses == null)
                {
                    return null;
                }

                return shippingAddresses;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     create shipping address
        /// </summary>
        /// <param name="controlPath"></param>
        /// <param name="isXML"></param>
        /// <returns></returns>
        protected AddressBase createShippingAddress(string controlPath, bool isXML)
        {
            AddressBase shippingAddressBase = null;
            try
            {
                if (!isXML)
                {
                    shippingAddressBase = LoadControl(controlPath) as AddressBase;
                }
                else
                {
                    shippingAddressBase = new AddressControl();
                    shippingAddressBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return shippingAddressBase;
        }

        /// <summary>
        ///     button clicked for add shipping address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtAddShippingAddress_Click(object sender, EventArgs e)
        {
            ucShippingInfoControl.ShowPopupForShipping(CommandType.Add,
                                                       new ShippingAddressEventArgs(DistributorID, null, false, true));
        }

        #endregion Shippping Address process

        #region Pickup Location process

        /// <summary>
        ///     process pickup location
        /// </summary>
        private void processPickupLocation()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupPreferences == false)
            {
                dvSavedPickupLocation.Visible = false;
                return;
            }

            if (getPickupLocationList() == null || pickupLocationPreferences.Count() == 0)
            {
                dvViewAllPickupLocation.Visible = false;
                dvNonPrimaryShippingAddress.Visible = true;
            }
            else
            {
                dvViewAllPickupLocation.Visible = true;
                lbtViewAllPickupLocation.PostBackUrl = "/Ordering/SavedPickupLocation.aspx";

                if (getPrimaryPickupLocation() == null)
                {
                    dvPrimaryPickupLocation.Visible = false;
                    dvNonPrimaryPickupLocation.Visible = true;
                }
                else
                {
                    bool isXML = true;
                    string controlPath = getAddressControlPath(ref isXML);
                    var pickupLocationBase = createPickupLocation(controlPath, isXML);
                    pickupLocationBase.DataContext = getPrimaryPickupLocation();
                    pnlPrimaryPickupLocation.Controls.Add((Control) pickupLocationBase);

                    dvPrimaryPickupLocation.Visible = true;
                    dvNonPrimaryPickupLocation.Visible = false;
                }
            }
        }

        /// <summary>
        ///     get pickup Location List
        /// </summary>
        /// <returns></returns>
        private List<PickupLocationPreference_V01> getPickupLocationList()
        {
            try
            {
                pickupLocationPreferences =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences(DistributorID, CountryCode);
            }
            catch (Exception)
            {
                return null;
            }
            return pickupLocationPreferences;
        }

        /// <summary>
        ///     get primary pickup location
        /// </summary>
        /// <returns></returns>
        private DeliveryOption getPrimaryPickupLocation()
        {
            try
            {
                pickupLocationPreferences =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode);
                if (pickupLocationPreferences == null)
                {
                    return null;
                }

                primaryPickupLocationPreference = pickupLocationPreferences.Where(s => s.IsPrimary).First();
                if (null != primaryPickupLocationPreference)
                {
                    var shippingInfo =
                        ShippingProvider.GetShippingProvider(CountryCode)
                                        .GetShippingInfoFromID(DistributorID, Locale, DeliveryOptionType.Pickup,
                                                               primaryPickupLocationPreference.PickupLocationID, 0);
                    if (null != shippingInfo)
                    {
                        var deliveryOption = new DeliveryOption(shippingInfo.WarehouseCode, shippingInfo.FreightCode,
                                                                DeliveryOptionType.Pickup);
                        if (shippingInfo.Address != null)
                            deliveryOption.Address = shippingInfo.Address.Address;
                        deliveryOption.Alias = shippingInfo.Name;
                        deliveryOption.Id = primaryPickupLocationPreference.PickupLocationID;
                        deliveryOption.Description = shippingInfo.Description;
                        return deliveryOption;
                        //pickupLocations.Add(deliveryOption);
                    }
                    //primaryPickupLocation = pickupLocations.Where(s => s.Id == primaryPickupLocationPreference.PickupLocationID).First();
                    //if (null != primaryPickupLocation)
                    //{
                    //    primaryPickupLocation.Alias = primaryPickupLocationPreference.PickupLocationNickname;
                    //}
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        ///     create address for pickup location
        /// </summary>
        /// <returns></returns>
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
        ///     create pickup location
        /// </summary>
        /// <param name="controlPath"></param>
        /// <param name="isXML"></param>
        /// <returns></returns>
        protected AddressBase createPickupLocation(string controlPath, bool isXML)
        {
            AddressBase PickupLocationBase = null;
            try
            {
                if (!isXML)
                {
                    PickupLocationBase = LoadControl(controlPath) as AddressBase;
                }
                else
                {
                    PickupLocationBase = new AddressControl();
                    PickupLocationBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return PickupLocationBase;
        }

        /// <summary>
        ///     get path for address control
        /// </summary>
        /// <param name="isXML"></param>
        /// <returns></returns>
        private string getAddressControlPath(ref bool isXML)
        {
            isXML = true;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress;
        }

        /// <summary>
        ///     button clicked for add pickup location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtAddPickupLocation_Click(object sender, EventArgs e)
        {
            Session["AddClickedFromPickupPref"] = true;
            ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(true));
        }

        #endregion Pickup Location process

        #region Pickup From Courier Location process

        private void processPUFromCourierLocation()
        {
            if (HLConfigManager.Configurations.CheckoutConfiguration.SavePickupFromCourierPreferences == false)
            {
                dvSavedPickpFromCourier.Visible = false;
                return;
            }

            if (getPickupLocationList() == null || pickupLocationPreferences.Count() == 0)
            {
                dvViewAllPUFromCourier.Visible = false;
                dvNonPrimaryPUFromCourierLocation.Visible = true;
            }
            else
            {
                dvViewAllPUFromCourier.Visible = true;
                lbtViewAllPUFromCourier.PostBackUrl = "/Ordering/SavedPickupCourierLocation.aspx";

                var primaryPUFromCourierLoc = getPrimaryPUFromCourierLocation();
                if (primaryPUFromCourierLoc == null)
                {
                    dvPrimaryPUFromCourierLocation.Visible = false;
                    dvNonPrimaryPUFromCourierLocation.Visible = true;
                }
                else
                {
                    bool isXML = true;
                    string controlPath = getAddressControlPath(ref isXML);
                    var pickupLocationBase = createPickupLocation(controlPath, isXML);
                    pickupLocationBase.DataContext = primaryPUFromCourierLoc;
                    pnlPrimaryPUFromcourierLocation.Controls.Add((Control) pickupLocationBase);

                    dvPrimaryPUFromCourierLocation.Visible = true;
                    dvNonPrimaryPUFromCourierLocation.Visible = false;
                }
            }
        }

        private DeliveryOption getPrimaryPUFromCourierLocation()
        {
            try
            {
                pickupLocationPreferences =
                    (Page as ProductsBase).GetShippingProvider()
                                          .GetPickupLocationsPreferences((Page as ProductsBase).DistributorID,
                                                                         (Page as ProductsBase).CountryCode);
                if (pickupLocationPreferences == null)
                {
                    return null;
                }

                primaryPickupLocationPreference = pickupLocationPreferences.Where(s => s.IsPrimary).First();
                if (null != primaryPickupLocationPreference)
                {
                    var shippingInfo =
                        ShippingProvider.GetShippingProvider(CountryCode)
                                        .GetShippingInfoFromID(DistributorID, Locale,
                                                               DeliveryOptionType.PickupFromCourier,
                                                               primaryPickupLocationPreference.ID, 0);
                    if (null != shippingInfo)
                    {
                        var deliveryOption = new DeliveryOption(shippingInfo.WarehouseCode, shippingInfo.FreightCode,
                                                                DeliveryOptionType.Pickup);
                        if (shippingInfo.Address != null)
                            deliveryOption.Address = shippingInfo.Address.Address;

                        if (HLConfigManager.Configurations.DOConfiguration.IsChina)
                        {
                            deliveryOption.Alias = primaryPickupLocationPreference.PickupLocationNickname; 
                            deliveryOption.Id = primaryPickupLocationPreference.PickupLocationID;
                            deliveryOption.Description = shippingInfo.Name + " " + shippingInfo.Description;
                        }
                        else
                        {
                            deliveryOption.Alias = shippingInfo.Name;
                            deliveryOption.Id = primaryPickupLocationPreference.PickupLocationID;
                            deliveryOption.Description = shippingInfo.Description;
                        }
                        return deliveryOption;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        protected void lbtAddPUFromCourier_Click(object sender, EventArgs e)
        {
            Session["AddClickedFromPUFromCourierPref"] = true;
            ucShippingInfoControl.ShowPopupForPickup(CommandType.Add, new DeliveryOptionEventArgs(true));
        }

        #endregion Pickup From Courier Location process

        #region Payment Information process

        /// <summary>
        ///     process payment information
        /// </summary>
        private void processPaymentInformation()
        {
            if (HLConfigManager.Configurations.PaymentsConfiguration.AllowSavedCards)
            {
                mpPaymentInformation = (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");

                if (getPrimaryPaymentInformationList() == null || paymentInformations.Count() == 0)
                {
                    if (HLConfigManager.Configurations.PaymentsConfiguration.UseCardRegistry)
                    {
                        // pre defect 23720 don't show it and an error is thrown when click on visible link
                        dvViewAllPaymentInformation.Visible = false;
                        lblPrimaryPaymentInformation.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                                      "NoCard");
                        dvNonPrimaryPaymentInformation.Visible = false;
                    }
                    else
                    {
                        dvViewAllPaymentInformation.Visible = false;
                        dvNonPrimaryPaymentInformation.Visible = true;
                    }
                }
                else
                {
                    dvViewAllPaymentInformation.Visible = true;
                    lbtViewAllPaymentInformation.PostBackUrl = "/Ordering/SavedPaymentInformation.aspx";

                    if (
                        (primaryPaymentInformation =
                         paymentInformations.Where(s => s.IsPrimary && !s.IsTemporary).FirstOrDefault()) == null)
                    {
                        dvPrimaryPaymentInformation.Visible = false;
                        dvNonPrimaryPaymentInformation.Visible = true;
                    }
                    else
                    {
                        int cardNumberLength = primaryPaymentInformation.CardNumber.Length;
                        lblPrimaryPaymentInformation.Text =
                            primaryPaymentInformation.CardHolder.First + " " +
                            primaryPaymentInformation.CardHolder.Middle + " " +
                            primaryPaymentInformation.CardHolder.Last + "<br /> " +
                            getCardName(primaryPaymentInformation.CardType) + "<br /> " +
                            primaryPaymentInformation.CardNumber.Substring(
                                primaryPaymentInformation.CardNumber.Length - 4, 4) + "<br /> " +
                            primaryPaymentInformation.Expiration.ToString("MM-yyyy");

                        dvPrimaryPaymentInformation.Visible = true;
                        dvNonPrimaryPaymentInformation.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        ///     get primary payment information
        /// </summary>
        /// <returns></returns>
        private List<PaymentInformation> getPrimaryPaymentInformationList()
        {
            try
            {
                paymentInformations =
                    PaymentInfoProvider.GetPaymentInfo(
                        (Page as ProductsBase).DistributorID, (Page as ProductsBase).Locale);
                if (paymentInformations != null)
                {
                    return paymentInformations;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        ///     button clicked for add payment information
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lbtAddPaymentInformation_Click(object sender, EventArgs e)
        {
            onCreditCardProcessing(this,
                                   new PaymentInfoEventArgs(PaymentInfoCommandType.Add, new PaymentInformation(), false));
            mpPaymentInformation = (ModalPopupExtender) ucPaymentInfoControl.FindControl("ppPaymentInfoControl");
            mpPaymentInformation.Show();
        }

        /// <summary>Mask the card number for display</summary>
        /// <param name="cardNum">The card number</param>
        /// <returns>The masked value</returns>
        protected string getCardName(string cardType)
        {
            string cardName = cardType;
            if (!string.IsNullOrEmpty(cardType))
            {
                cardName =
                    GetGlobalResourceObject(string.Format("{0}_GlobalResources", HLConfigManager.Platform),
                                            string.Format("CardType_{0}_Description", cardType)) as string;
            }

            return cardName;
        }

        #endregion Payment Information process
    }
}