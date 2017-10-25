using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Pickup
{
    public class PickupCommand
    {
        //ADD/DELETE

        public PickupCommand(PickupCommandType mode)
        {
            this.Mode = mode;
        }

        public PickupCommandType Mode { get; set; }

        public static PickupCommand Parse(int mode)
        {
            PickupCommandType cmdType = (PickupCommandType)
                                        Enum.Parse(typeof (PickupCommandType), mode.ToString());
            return new PickupCommand(cmdType);
        }
    }

    public enum PickupCommandType : int
    {
        ADD = 1,
        DELETE = 2
    }

    public partial class AddDeletePickupControl : UserControlBase
    {
        public static string SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND = "SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND";
        public static string SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID = "SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID";

        public int WorkedUponDeliveryOptionId
        {
            get { return (int) this.Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID]; }
            set { this.Session[SESSION_KEY_WORKED_UPON_DELIVERY_OPTION_ID] = value; }
        }

        public PickupCommand SourceCommand
        {
            get { return (PickupCommand) this.Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND]; }
            set { this.Session[SESSION_KEY_PICKUP_POPUP_SOURCE_COMMAND] = value; }
        }

        public List<DeliveryOption> PickupLocations { get; set; }

        [Publishes(MyHLEventTypes.PickupPreferenceCreated)]
        public event EventHandler OnPickupPreferenceCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceDeleted)]
        public event EventHandler OnPickupPreferenceDeleted;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.lblErrors.Text = string.Empty;
            FetchPickupLocations();
            if (!this.Page.IsPostBack)
            {
                PopulateDropDown();
                RenderCommandCentricView(new PickupCommand(PickupCommandType.ADD));
            }
        }

        public void RenderCommandCentricView(PickupCommand command)
        {
            this.SourceCommand = command;
            switch (this.SourceCommand.Mode)
            {
                case PickupCommandType.ADD:
                    RenderAddPickupView();
                    return;

                case PickupCommandType.DELETE:
                    RenderDeletePickupView();
                    return;
            }
        }

        private void FetchPickupLocations()
        {
            ShippingAddress_V02 address = new ShippingAddress_V02();
            Address_V01 addressV01 = new Address_V01();
            addressV01.Country = (this.Page as ProductsBase).CountryCode;
            address.Address = addressV01;

            PickupLocations = (this.Page as ProductsBase).
                GetShippingProvider().GetDeliveryOptions(DeliveryOptionType.Pickup, address);
        }

        private void RenderAddPickupView()
        {
            this.divAddPickUp.Visible = true;
            this.divDeletePickUp.Visible = false;
            this.btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            PopulateDropDown();

            var pickupPreferences = (this.Page as ProductsBase).GetShippingProvider()
                                                               .GetPickupLocationsPreferences
                ((this.Page as ProductsBase).DistributorID,
                 (this.Page as ProductsBase).CountryCode);
        }

        private void PopulateDropDown()
        {
            this.ddlPickupLocations.DataSource = PickupLocations;
            this.ddlPickupLocations.DataTextField = "Description";
            this.ddlPickupLocations.DataValueField = "Id";
            this.ddlPickupLocations.DataBind();
            this.ddlPickupLocations.Items.Insert(0, new ListItem(GetLocalResourceObject("Select.Text") as string, "0"));
        }

        protected void ddlPickupLocations_IndexChanged(object sender, EventArgs e)
        {
            string text = this.ddlPickupLocations.SelectedItem.Text;
            this.ViewState["SelectedPickupLocation"] = this.ddlPickupLocations.SelectedItem.Value;
            int selectedValue = int.Parse(this.ddlPickupLocations.SelectedItem.Value);

            if (selectedValue != 0)
            {
                DeliveryOption deliveryOption = this.PickupLocations.Find(p => p.Id == selectedValue);
                this.pAddress.InnerHtml = (this.Page as ProductsBase).GetShippingProvider().
                                                                      FormatPickupLocationAddress(deliveryOption.Address);
            }
            else
                this.pAddress.InnerHtml = string.Empty;
        }

        protected void ContinueChanges_Clicked(object sender, EventArgs e)
        {
            if ((this.ViewState["SelectedPickupLocation"] == null) ||
                this.ViewState["SelectedPickupLocation"].ToString().Equals("0"))
            {
                this.lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage", "NoPickUpLocation");
                return;
            }
            UpdateViewChanges();
        }

        private void UpdateViewChanges()
        {
            if (this.SourceCommand.Mode == PickupCommandType.ADD)
            {
                int selectedPickupLocationId = int.Parse(this.ViewState["SelectedPickupLocation"].ToString());
                OnPickupPreferenceCreated(this, new DeliveryOptionEventArgs(selectedPickupLocationId, string.Empty));
            }

            if (this.SourceCommand.Mode == PickupCommandType.DELETE)
            {
                int returnId = (this.Page as ProductsBase).GetShippingProvider().DeletePickupLocationsPreferences
                    ((this.Page as ProductsBase).DistributorID,
                     this.WorkedUponDeliveryOptionId,
                     (this.Page as ProductsBase).CountryCode
                    );

                OnPickupPreferenceDeleted(this,
                                          new DeliveryOptionEventArgs(this.WorkedUponDeliveryOptionId, string.Empty));
            }
        }

        protected void CancelChanges_Clicked(object sender, EventArgs e)
        {
            if (this.PopupExtender != null)
                this.PopupExtender.ClosePopup();
        }

        private void RenderDeletePickupView()
        {
            bool isXML = true;
            this.divAddPickUp.Visible = false;
            this.divDeletePickUp.Visible = true;
            this.btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;

            ShippingAddress_V01 shipAddr = new ShippingAddress_V01();
            shipAddr.Address = new Address_V01();
            shipAddr.Address.Country = CountryCode;

            var deliveryOptionList = (this.Page as ProductsBase).GetShippingProvider().
                                                                 GetDeliveryOptions(DeliveryOptionType.Pickup, shipAddr);

            DeliveryOption pickupDeliveryOption = deliveryOptionList.Find(p => p.Id == this.WorkedUponDeliveryOptionId);
            this.lblName.Text = pickupDeliveryOption.Description;

            string controlPath = getDeleteAddressControlPath(ref isXML);
            AddressBase addressBase = new AddressControl();
            addressBase.XMLFile = controlPath;
            this.colDeletePickUp.Controls.Add((Control) addressBase);

            addressBase.DataContext = pickupDeliveryOption;

            var pickUpLocationPreferences = (this.Page as ProductsBase).GetShippingProvider().
                                                                        GetPickupLocationsPreferences(
                                                                            (this.Page as ProductsBase).DistributorID,
                                                                            CountryCode);
            PickupLocationPreference_V01 selectedPickupPreference = pickUpLocationPreferences.Find
                (p => p.PickupLocationID == this.WorkedUponDeliveryOptionId);

            if (selectedPickupPreference != null)
            {
                this.lblDeleteIsPrimaryText.Text = selectedPickupPreference.IsPrimary
                                                       ? GetLocalResourceObject("PrimaryYes.Text") as string
                                                       : GetLocalResourceObject("PrimaryNo.Text") as string;
                this.lblDeleteNicknameText.Text = selectedPickupPreference.PickupLocationNickname;

                if (selectedPickupPreference.IsPrimary) //Eval UC:3.5.3.7 (deleting primary)
                {
                    this.lblErrors.Text = PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                    "PrimaryPickupPreferenceDeleteNotAllowed");
                    this.btnContinue.Enabled = false;
                    return;
                }
                else //Eval UC:3.5.3.6 (deleting non-primary)
                {
                    this.btnContinue.Enabled = true;
                }
            }
        }

        private string getDeleteAddressControlPath(ref bool isXML)
        {
            isXML = true;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress;
        }

        protected AddressBase createAddress(string controlPath, bool isXML)
        {
            AddressBase addressBase = null;
            try
            {
                if (!isXML)
                {
                    addressBase = LoadControl(controlPath) as AddressBase;
                }
                else
                {
                    addressBase = new AddressControl();
                    addressBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
            return addressBase;
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public void OnPickupPreferenceBeingCreated(object sender, EventArgs e)
        {
            if ((this.Page as ProductsBase).Locale == "es-MX")
                return;

            this.lblErrors.Text = "";
            //Set the MODE & Load respective controlset
            this.SourceCommand = new PickupCommand(PickupCommandType.ADD);
            RenderCommandCentricView(SourceCommand);
        }

        [SubscribesTo(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public void OnPickupPreferenceBeingDeleted(object sender, EventArgs e)
        {
            if ((this.Page as ProductsBase).Locale == "es-MX")
                return;

            this.lblErrors.Text = "";
            this.SourceCommand = new PickupCommand(PickupCommandType.DELETE);
            DeliveryOptionEventArgs args = e as DeliveryOptionEventArgs;
            this.WorkedUponDeliveryOptionId = args.DeliveryOptionId;

            RenderCommandCentricView(SourceCommand);
        }
    }
}