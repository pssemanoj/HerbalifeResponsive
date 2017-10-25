using HL.Common.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public partial class THThirdPartyPickupControl : UserControlBase
    {
        #region Constants
        const string JS7ELEVEN_KEY = "Script7Eleven";
        const string DEFAULT_COURIERTYPE = "7Eleven";

        public static string SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND = "SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND";
        public static string SESSION_KEY_PICKUPLOC_DESCRIPTION = "SESSION_KEY_PICKUPLOC_DESCRIPTION";
        public static string SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID = "SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID";
        public const string PICKUPLOC_PREFERENCE_CACHE_PREFIX = "PickupLocationPreference_";
        #endregion

        #region Events
        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            var args = e as DeliveryOptionEventArgs;
            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.ADD);
            RenderCommandCentricView(SourceCommand);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            lblErrors.Text = "";
            SourceCommand = new PickupCommand(PickupCommandType.DELETE);
            var args = e as DeliveryOptionEventArgs;
            WorkedUponDeliveryOptionId = args.DeliveryOptionId;
            Session["IDToDelete"] = args.DeliveryOptionId;
            PickupLocationDescription = args.Description;
            RenderCommandCentricView(SourceCommand);
        }
        #endregion

        #region Properties and fields
        public int WorkedUponDeliveryOptionId
        {
            get { return (int)Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID]; }
            set { Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID] = value; }
        }

        public string PickupLocationDescription
        {
            get { return (string)Session[SESSION_KEY_PICKUPLOC_DESCRIPTION]; }
            set { Session[SESSION_KEY_PICKUPLOC_DESCRIPTION] = value; }
        }

        public PickupCommand SourceCommand
        {
            get { return (PickupCommand)Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND]; }
            set { Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND] = value; }
        }
        #endregion

        #region Methods
        private void RenderCommandCentricView(PickupCommand command)
        {
            SourceCommand = command;
            switch (SourceCommand.Mode)
            {
                case PickupCommandType.ADD:
                    RenderAddPickupView();
                    return;

                case PickupCommandType.DELETE:
                    RenderDeletePickupView();
                    return;
            }
        }

        private void RenderAddPickupView()
        {
            lblErrors.Text = string.Empty;
            lblHeader.Visible = true;
            divAddPickupLocation.Visible = true;
            divDeletePickupLocation.Visible = false;
            RegisterJSEvents();

            if (Session["AddClickedFromPickupPref"] != null)
            {
                Session["AddClickedFromPickupPref"] = null;
            }
        }

        private void RenderDeletePickupView()
        {
            lblHeader.Visible = false;
            divAddPickupLocation.Visible = false;
            divDeletePickupLocation.Visible = true;

            List<PickupLocationPreference_V01> pickUpLocationPreferences =
                ProductsBase.GetShippingProvider().GetPickupLocationsPreferences(DistributorID, CountryCode, Locale, DeliveryOptionType.PickupFromCourier);

            PickupLocationPreference_V01 selectedPickupPreference = pickUpLocationPreferences.Find(p => p.PickupLocationID == WorkedUponDeliveryOptionId);
            //lblDeleteNicknameText.Text = selectedPickupPreference.PickupLocationNickname;

            if (pickUpLocationPreferences.Count == 1 && ShoppingCart.CartItems.Count > 0)
            {
                lblErrors.Text = GetLocalResourceObject("LastPickupAddress.Text") as string;
            }
        }

        private void ClosePopup()
        {
            if (PopupExtender != null)
                PopupExtender.ClosePopup();
        }

        private void RegisterJSEvents()
        {
            if (!Page.ClientScript.IsStartupScriptRegistered(JS7ELEVEN_KEY))
            {
                var strScriptMap = new StringBuilder();
                strScriptMap.AppendLine(@"if (window.addEventListener && window.postMessage) { ");
                strScriptMap.AppendLine(@"    window.addEventListener('message', receiveMsgFromMap, false); ");
                strScriptMap.AppendLine(@"} else { ");
                strScriptMap.AppendLine(@"    alert('Your browser not support some important functions. \nPlease upgrade or change your browser.'); ");
                strScriptMap.AppendLine(@"}");

                strScriptMap.AppendLine(@"var mapResponse = null;");
                strScriptMap.AppendLine(@"function receiveMsgFromMap(evt) {");
                strScriptMap.AppendLine(@"    if(evt.data != null) { ");
                strScriptMap.AppendLine(@"        document.getElementById('" + hdnMapResponse.ClientID + "').value = evt.data; ");
                strScriptMap.AppendLine(@"    } else   { ");
                strScriptMap.AppendLine(@"        document.getElementById('" + hdnMapResponse.ClientID + "').value = '{ error: \"Error retrieving map response.\"'; ");
                strScriptMap.AppendLine(@"    }");
                strScriptMap.AppendLine(@"    javascript: __doPostBack('" + btnContinueHidden.UniqueID + "', ''); ");
                strScriptMap.AppendLine(@"}");
                ScriptManager.RegisterStartupScript(Page, typeof(Page), JS7ELEVEN_KEY, strScriptMap.ToString(), true);
            }
        }
        #endregion


        #region EventHandlers
        protected void Page_Load(object sender, EventArgs e)
        {
            lblErrors.Text = string.Empty;

            if (!Page.IsPostBack)
            {
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }

            
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "visibleAreaHeight", "visibleAreaHeight();", true);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ClosePopup();
        }

        protected void ContinueChanges_Click(object sender, EventArgs e)
        {
            lblErrors.Text = "";
            if (SourceCommand.Mode == PickupCommandType.DELETE)
            {
                try
                {
                    if (WorkedUponDeliveryOptionId == 0)
                    {
                        int value = 0;
                        int.TryParse(Session["IDToDelete"].ToString(), out value);
                        WorkedUponDeliveryOptionId = value;
                    }
                }
                catch
                {
                    WorkedUponDeliveryOptionId = int.Parse(Session["IDToDelete"].ToString());
                }

                int returnId = (Page as ProductsBase).GetShippingProvider().DeletePickupLocationsPreferences((Page as ProductsBase).DistributorID, WorkedUponDeliveryOptionId, (Page as ProductsBase).CountryCode);

                OnPickupPreferenceDeleted(this, new DeliveryOptionEventArgs(WorkedUponDeliveryOptionId, string.Empty));
            }
            else
            {
                var jsonResponse = JObject.Parse(hdnMapResponse.Value);

                // Validate if could retrieve the address from the 7Eleven map
                if (jsonResponse == null)
                {
                    lblErrors.Text = "Error retrieving map response.";
                    return;
                }

                if (jsonResponse != null && jsonResponse["error"] != null)
                {
                    lblErrors.Text = jsonResponse["error"].ToString();
                    return;
                }

                // Save Address with jsonResponse
                var pickupLocationId = jsonResponse["storeCode"] != null ? (int)jsonResponse["storeCode"] : -1;
                if (pickupLocationId < 0)
                {
                    lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpLocation");
                    return;
                }

                bool isSession = !HLConfigManager.Configurations.CheckoutConfiguration.SavePickupFromCourierPreferences;
                string branchName = jsonResponse["storeName"] != null ? jsonResponse["storeName"].ToString() : string.Empty;
                string addressDetails = jsonResponse["address"] != null ? jsonResponse["address"].ToString() : string.Empty;
                string[] parts = addressDetails.Split(' ');
                string postalCode = string.Empty;
                string province = string.Empty;
                string subdistrict = string.Empty;
                string district = string.Empty;
                string address = string.Empty;
                if (parts.Length > 4)
                {
                    postalCode = parts[parts.Length - 1].ToString();
                    province = parts[parts.Length - 2].ToString();
                    subdistrict = parts[parts.Length - 3].ToString();
                    district = parts[parts.Length - 4].ToString();
                    StringBuilder builder = new StringBuilder();
                    var addressValue = parts.Take(parts.Length - 4);
                    foreach (string add in addressValue)
                    {
                        builder.Append(add);
                        builder.Append(" ");
                    }
                    address = builder.ToString();
                }
                else if (parts.Length == 4)
                {
                    postalCode = parts[parts.Length - 1].ToString();
                    province = parts[parts.Length - 2].ToString();
                    subdistrict = parts[parts.Length - 3].ToString();
                    district = parts[parts.Length - 4].ToString();
                    address = subdistrict;
                }

                var freightCodeAndWarehouse = ProductsBase.GetShippingProvider().GetFreightCodeAndWarehouse(new ShippingAddress_V01() { Address = new Address_V01() { StateProvinceTerritory = "*" } });

                // Create
                var courierLocation = new InsertCourierLookupRequest_V01()
                {
                    Locale = this.Locale,
                    CountryCode = this.CountryCode,
                    FreightCode = freightCodeAndWarehouse[0],
                    WarehouseCode = freightCodeAndWarehouse[1],
                    CourierStoreNumber = pickupLocationId,
                    State = province,
                    CourierStoreName = branchName,
                    Zip = postalCode,
                    City = district,
                    Street1 = address,
                    Street2 = subdistrict,
                    CourierType = DEFAULT_COURIERTYPE,
                    CourierStoreId = pickupLocationId.ToString(),
                    AdditionalInformation = GetLocalResourceObject("DefaultAdditionalInfo") as string
                };

                int retValue = ProductsBase.GetShippingProvider().SavePickupLocation(courierLocation);

                if (retValue == pickupLocationId)
                {
                    // Save PickupLocationPreference
                    int returnId = ProductsBase.GetShippingProvider().
                                    SavePickupLocationsPreferences(DistributorID, isSession, pickupLocationId, string.Format("{0} #{1}", GetLocalResourceObject("DefaultNickname") as string, pickupLocationId), branchName, CountryCode, false, DEFAULT_COURIERTYPE);

                    if (returnId == -2) //duplicate nickname
                    {
                        lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateAddressNickname");
                        return;
                    }

                    if (returnId == -3) //duplicate pickuplocation
                    {
                        lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicatePickupLocation");
                        return;
                    }

                    // Add PickupLocation to cache and select
                    OnPickupPreferenceCreated(this, new DeliveryOptionEventArgs(returnId, DEFAULT_COURIERTYPE));
                }
                else
                {
                    lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "CannotSavePickupLocation");
                    return;
                }
            }

            ClosePopup();
        }
        #endregion

    }
}