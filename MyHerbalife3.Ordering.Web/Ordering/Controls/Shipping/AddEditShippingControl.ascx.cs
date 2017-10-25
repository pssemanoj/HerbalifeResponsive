using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using ordering = MyHerbalife3.Ordering.Web.Ordering.Controls.Address;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Shipping
{

    #region supporting classes (TO BE SHIFTED)

    //this command-patterned command has to be shifted to right place
    public class AddressBaseProvider
    {
        public static ordering.AddressBase Get(string locale, TemplateControl control, string controlPath, bool isXml)
        {
            ordering.AddressBase addressBase = null;

            try
            {
                if (!isXml)
                {
                    addressBase = control.LoadControl(controlPath) as ordering.AddressBase;
                    addressBase.LoadPage();
                }
                else
                {
                    addressBase = new ordering.AddressControl();
                    addressBase.XMLFile = controlPath;
                }
            }
            catch (HttpException ex)
            {
                LoggerHelper.Error(ex.ToString());
            }

            return addressBase;
        }
    }

    [Serializable]
    public class ShippingAddressCommand
    {
        //ADD/EDIT/DELETE

        public ShippingAddressCommand(ShippingAddressCommandType mode)
        {
            Mode = mode;
        }

        public ShippingAddressCommandType Mode { get; set; }

        public static ShippingAddressCommand Parse(int mode)
        {
            var cmdType = (ShippingAddressCommandType)
                          Enum.Parse(typeof(ShippingAddressCommandType), mode.ToString());
            return new ShippingAddressCommand(cmdType);
        }
    }

    public enum ShippingAddressCommandType
    {
        ADD = 1,
        EDIT = 2,
        DELETE = 3
    }

    public class ShippingAddressCommandProcessor
    {
        public static void Process(ShippingAddressCommand command, Page parent)
        {
            switch (command.Mode)
            {
                case ShippingAddressCommandType.ADD:
                    var cntrl = new AddEditShippingControl();
                    //pass (int)command as querystring to control or set property directly
                    return;
            }
        }
    }

    #endregion supporting classes (TO BE SHIFTED)

    public partial class AddEditShippingControl : UserControlBase
    {
        //[Publishes(MyHLEventTypes.CommandCancelled)]
        //public event EventHandler OnCommandCancelled;

        private const string POPUPSHOWN = "addresspopupshown";
        public static string SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND = "SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND";

        public static string SESSION_KEY_SHIPPING_POPUP_WORKED_UPON_SHIPPING_ADDRESS =
            "SESSION_KEY_SHIPPING_POPUP_WORKED_UPON_SHIPPING_ADDRESS";

        public static string SESSION_KEY_SHIPPING_POPUP_OLD_SHIPPING_ADDRESS =
            "SESSION_KEY_SHIPPING_POPUP_OLD_SHIPPING_ADDRESS";

        public static string SESSION_KEY_DISABLE_SAVED_ADDRESS_CHECKBOX = "SESSION_KEY_DISABLE_SAVED_ADDRESS_CHECKBOX";

        public ShippingAddressCommand SourceCommand
        {
            get
            {
                // sessionstate is nulling out.

                //if (this.Session[SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND] == null)
                //    return (ShippingAddressCommand) this.ViewState[SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND];
                return (ShippingAddressCommand)Session[SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND];
            }
            set
            {
                Session[SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND] = value;
                // this.ViewState[SESSION_KEY_SHIPPING_POPUP_SOURCE_COMMAND] = value;
            }
        }

        public ShippingAddress_V02 OldAddress
        {
            get { return (ShippingAddress_V02)Session[SESSION_KEY_SHIPPING_POPUP_OLD_SHIPPING_ADDRESS]; }
            set { Session[SESSION_KEY_SHIPPING_POPUP_OLD_SHIPPING_ADDRESS] = value; }
        }

        public ShippingAddress_V02 WorkedUponAddress
        {
            get { return (ShippingAddress_V02)Session[SESSION_KEY_SHIPPING_POPUP_WORKED_UPON_SHIPPING_ADDRESS]; }
            set { Session[SESSION_KEY_SHIPPING_POPUP_WORKED_UPON_SHIPPING_ADDRESS] = value; }
        }

        public bool DisableSavedAddressCheckBox
        {
            get
            {
                return Session[SESSION_KEY_DISABLE_SAVED_ADDRESS_CHECKBOX] != null
                           ? (bool)Session[SESSION_KEY_DISABLE_SAVED_ADDRESS_CHECKBOX]
                           : false;
            }
            set { Session[SESSION_KEY_DISABLE_SAVED_ADDRESS_CHECKBOX] = value; }
        }

        public List<DeliveryOption> ShippingAddresses { get; set; }

        //public ShippingAddress_V01 WorkedUponAddress { get; set; }
        //public ShippingAddress_V01 OldAddress { get; set; }
        public ordering.AddressBase WorkedUponAddressControl { get; set; }

        [Publishes(MyHLEventTypes.ShippingAddressDeleted)]
        public event EventHandler OnShippingAddressDeleted;

        [Publishes(MyHLEventTypes.ShippingAddressCreated)]
        public event EventHandler OnShippingAddressCreated;

        [Publishes(MyHLEventTypes.ShippingAddressChanged)]
        public event EventHandler OnShippingAddressChanged;

        [Publishes(MyHLEventTypes.ShippingAddressPopupCancelled)]
        public event EventHandler OnShippingAddressPopupCancelled;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HidePrimaryAddressCheckBox)
            {
                cbMakePrimary.Attributes.Add("style", "display:none");
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.PickupInstructionsHaveTinID)
            {
                lblHongkongAddtinalInfo.Visible = true;
            }
            else
            {
                lblHongkongAddtinalInfo.Visible = false;
            }
            if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.ShowShippingAddressMessage)
            {
                var noteMessageText = GetLocalResourceObject("lblNoteResource.Text") as string;
                if (!string.IsNullOrEmpty(noteMessageText) && noteMessageText.Trim().Length > 0)
                {
                    lblNote.Text = noteMessageText.Trim();
                    lblNote.Visible = true;
                }
            }
            lblAddressRestrictionMsg.Visible = HLConfigManager.Configurations.AddressingConfiguration.HasAddressRestriction;
            //Fetch Shipping Address MODEL to evaluate global rules & render accordingly
            //     btnContinue.Attributes.Add("onclick", "HideWhenClicked(this)");
            FetchShippingModel();
            if (ViewState[POPUPSHOWN] != null)
                loadControls();

            RegisterScript();
        }

        public void RenderCommandCentricView(ShippingAddressCommand command)
        {
            SourceCommand = command;
            switch (SourceCommand.Mode)
            {
                case ShippingAddressCommandType.ADD:
                    RenderAddShippingView();
                    return;
                case ShippingAddressCommandType.EDIT:
                    RenderEditShippingView();
                    return;
                case ShippingAddressCommandType.DELETE:
                    RenderDeleteShippingView();
                    return;
            }
        }

        private void RenderDeleteShippingView()
        {
            lblDeleteIsPrimaryText.Text = WorkedUponAddress.IsPrimary
                                              ? GetLocalResourceObject("PrimaryYes.Text") as string
                                              : GetLocalResourceObject("PrimaryNo.Text") as string;

            lblDeleteNicknameText.Text = WorkedUponAddress.Alias;
            bool isXML = true;
            HideCheckboxes();
            string controlPath = getDeleteAddressControlPath(ref isXML);

            //Pull in the right AddressWindow definition (declarative controlset) from XML and load them
            //ordering.AddressBase addressBase = createAddress(controlPath, isXML);
            ordering.AddressBase addressBase = new ordering.AddressControl();
            lblName.Text = WorkedUponAddress.Recipient;
            addressBase.XMLFile = controlPath;
            removeControl(colDeleteShippingAddress);
            if (colDeleteShippingAddress.Controls.Count >= 1)
            {
                colDeleteShippingAddress.Controls.Clear();
            }
            colDeleteShippingAddress.Controls.Add((Control)addressBase);

            //Bug: 27034 -> Remove dashes for the phone number
            RemoveDashForPhone();

            //load actual address user wants to delete
            addressBase.DataContext = WorkedUponAddress;

            if (ShippingAddresses.Count == 1)
            {
                trError.Visible = true;
                blErrors.Items.Add(GetLocalResourceObject("LastShippingAddress.Text") as string);
            }
            else
            {
                btnContinue.Enabled = true;
            }

            //if (this.WorkedUponAddress.IsPrimary) //Eval UC:3.5.3.7 (deleting primary)
            //{
            //    this.trError.Visible = true;
            //    this.blErrors.Items.Add(new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "PrimaryAddressDeleteNotAllowed")));
            //    this.btnContinue.Enabled = false;
            //    return;
            //}
            //else //Eval UC:3.5.3.6 (deleting non-primary)
            //{
            //this.btnContinue.Enabled = true;
            //}
        }

        private void RemoveDashForPhone()
        {
            if (WorkedUponAddress != null)
            {
                string phone = WorkedUponAddress.Phone;

                if (!string.IsNullOrEmpty(phone))
                {
                    if (phone.StartsWith("-"))
                        phone = phone.Remove(0, 1);
                    if (phone.EndsWith("-"))
                        phone = phone.Remove(phone.Length - 1, 1);
                    WorkedUponAddress.Phone = phone;
                }

            }
        }

        private void RenderEditShippingView()
        {
            ShowCheckboxes();
            //load actual address user wants to edit
            ordering.AddressBase shippingAddressCtrl = getShippingControl(colNewShippingAddress);
            if (!HLConfigManager.Configurations.DOConfiguration.PhoneSplit)
            {
                if (WorkedUponAddress != null)
                {
                shippingAddressCtrl.DataContext = WorkedUponAddress;
                txtNickname.Text = WorkedUponAddress.Alias;
            }
            }

            var copyAddress = new ShippingAddress_V02();
            copyAddress.Address = new Address_V01();
            if (HLConfigManager.Configurations.DOConfiguration.PhoneSplit)
            {
                #region phone

                copyAddress.Address.Country = CountryCode;

                if(WorkedUponAddress!=null)
                copyAddress = new ShippingAddress_V02(WorkedUponAddress);
                //set the values for phoneNumber, areacode and extension.
                if (WorkedUponAddress != null && WorkedUponAddress.Phone != null)
                {
                    var phoneValues = WorkedUponAddress.Phone.Split(new[] { '-' });
                    switch (phoneValues.Length)
                    {
                        case 1:
                            copyAddress.Phone = phoneValues[0];
                            break;
                        case 2:
                            copyAddress.AreaCode = phoneValues[0];
                            copyAddress.Phone = phoneValues[1];
                            break;
                        case 3:
                            copyAddress.AreaCode = phoneValues[0];
                            copyAddress.Phone = phoneValues[1];
                            copyAddress.AltAreaCode = phoneValues[2];
                            break;
                        default:
                            break;
                    }
                }

                shippingAddressCtrl.DataContext = copyAddress;
                txtNickname.Text = copyAddress.Alias;
            }
            else
            {
                if (WorkedUponAddress != null && WorkedUponAddress.Phone != null)
                {
                copyAddress.Address.Country = CountryCode;
                    if (WorkedUponAddress != null)
                copyAddress = new ShippingAddress_V02(WorkedUponAddress);
                //set the values for phoneNumber, areacode and extension.
                var phoneValues = WorkedUponAddress.Phone.Split(new[] { '-' });
                switch (phoneValues.Length)
                {
                    case 1:
                        copyAddress.Phone = phoneValues[0];
                        break;
                    case 2:
                        copyAddress.AreaCode = phoneValues[0];
                        copyAddress.Phone = phoneValues[1];
                        break;
                    case 3:
                        copyAddress.AreaCode = phoneValues[0];
                        copyAddress.Phone = phoneValues[1];
                        copyAddress.AltAreaCode = phoneValues[2];
                        break;
                    default:
                        break;
                }
                }
                shippingAddressCtrl.DataContext = copyAddress;
                txtNickname.Text = copyAddress.Alias;
            }

            #endregion phone

            if (WorkedUponAddress != null)
            {
            if (WorkedUponAddress.IsPrimary) //Eval UC:3.5.3.3 (editing primary)
            {
                //•	“Make this my primary shipping address” is checked and not editable
                //•	“Save this shipping address” checked and not editable
                cbSaveThis.Visible = true;
                cbMakePrimary.Checked = true;
                cbMakePrimary.Enabled = false;
                cbSaveThis.Checked = true;
                cbSaveThis.Enabled = false;
                //return;
            }
            else //Eval UC:3.5.3.4 (editing non-primary)
            {
                //•	“Make this my primary shipping address” is unchecked and editable
                //•	“Save this shipping address” checked and editable.
                cbSaveThis.Visible = true;
                cbMakePrimary.Checked = false;
                cbMakePrimary.Enabled = true;
                cbSaveThis.Checked = true;
                cbSaveThis.Enabled = Locale == "de-DE" ? false : true;
            }
            //if the address is saved to session.
            if (WorkedUponAddress.ID < 0)
            {
                cbSaveThis.Visible = true;
                cbSaveThis.Checked = false;
                cbSaveThis.Enabled = Locale == "de-DE" ? false : true;
                cbMakePrimary.Checked = false;
                cbMakePrimary.Enabled = false;
            }

            //For all saved instances the "Save this shipping address" checkbox is invisible.
            if (WorkedUponAddress.ID > 0)
                cbSaveThis.Visible = false;
            }

            //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
            if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
                cbSaveThis.Visible = false;
        }

        private void RenderAddShippingView()
        {
            FetchShippingModel();
            ShowCheckboxes();

            ////Pull in the right AddressWindow definition (declarative controlset) from XML and load them
            ordering.AddressBase shippingAddressCtrl = getShippingControl(colNewShippingAddress);

            IShippingProvider shippingProvider = (Page as ProductsBase).GetShippingProvider();

            //set default data context & put it in session
            //ShippingAddress_V02 shipAddr = new ShippingAddress_V02();
            //shipAddr.Address = new Address_V01();
            //shipAddr.Address.Country = CountryCode;

            var shipAddr = shippingProvider.GetDefaultAddress() as ShippingAddress_V02;

            if (null != shippingAddressCtrl)
            {
                if (ShoppingCart.IsFromInvoice)
                {
                    shippingAddressCtrl.DataContext = WorkedUponAddress;
                }
                else
                {
                    shippingAddressCtrl.DataContext = shipAddr;
                }
            }
            shipAddr.Address.Country = CountryCode;

            if (null == ShippingAddresses || ShippingAddresses.Count == 0) //Eval UC:3.5.3.1 (no existing saved address)
            {
                //Both “Save this shipping address” and “Make this my primary shipping address” are checked.
                //      The actor cannot uncheck either checkboxes
                cbMakePrimary.Checked = true;
                cbMakePrimary.Enabled = false;
                cbSaveThis.Checked = true;
                cbSaveThis.Enabled = false;
            }
            else //Eval UC:3.5.3.2 (atleast 1 address exists)
            {
                cbMakePrimary.Checked = false;
                cbMakePrimary.Enabled = true;
                cbSaveThis.Checked = true;
                cbSaveThis.Enabled = Locale == "de-DE" ? false : true;
            }

            //If the popup is called from “Order Preference” section, then 'Save' button is invisible.
            if (hfDiableSavedCheckbox.Value.ToLower().Equals("true"))
                cbSaveThis.Visible = false;
        }

        private void FetchShippingModel()
        {
            ShippingAddresses =
                ProductsBase.GetShippingProvider().GetShippingAddresses(
                    DistributorID, Locale);
        }

        /// <summary>
        ///     ProcessAddShippingSubmit - click on continue on new shipping address
        /// </summary>
        /// <param name="button"></param>
        protected void ProcessAddShippingSubmit()
        {
            bool isNicknameEntered = false;

            string distributorID = DistributorID;
            string locale = Locale;
            bool toSession = cbSaveThis.Checked ? false : true;

            WorkedUponAddress.ID = -1;
            IShippingProvider shippingProvider = (this.Page as ProductsBase).GetShippingProvider();

            var addressList = GetShippingAddressesFromDeliveryOptions(
                shippingProvider.GetShippingAddresses(distributorID, locale));
            // EMPTY IS ALLOWED OTHERWISE YOU COULD GET BUG 24224 for duplicates
            //  if (!this.txtNickname.Text.Equals(String.Empty))
            {
                WorkedUponAddress.Alias = txtNickname.Text.Trim();
                isNicknameEntered = true;
            }

            try
            {
                //1) Submit validated chnages to Shippping Provider
                WorkedUponAddress.ID = ProductsBase.GetShippingProvider().SaveShippingAddress
                    (distributorID, locale, WorkedUponAddress
                     , toSession
                     , true
                     , isNicknameEntered);
            }
            finally
            {
            }

            if (WorkedUponAddress.ID == -2) //duplicateShippingAddress
            {
                trError.Visible = true;
                blErrors.Items.Add(
                    new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddressExists")));
                return;
            }
            if (WorkedUponAddress.ID == -3) //duplicateNickName
            {
                trError.Visible = true;
                blErrors.Items.Add(
                    new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateAddressNickname")));
                return;
            }
            else
            {
                WorkedUponAddress.DisplayName = WorkedUponAddress.Alias == string.Empty
                                                    ? shippingProvider.GetAddressDisplayName(WorkedUponAddress)
                                                    : WorkedUponAddress.Alias;
                ShoppingCart.CopyInvoiceStatus = ServiceProvider.CatalogSvc.CopyInvoiceStatus.success;
                Session["IsCopingFromInvoice"] = null;
                OnShippingAddressCreated(this,
                                         new ShippingAddressEventArgs(distributorID, WorkedUponAddress, false, false));
                //popup_AddEditShippingControl.Hide();
            }
        }

        /// <summary>
        ///     ProcessEditShippingSubmit - click on continue on edit shipping address
        /// </summary>
        /// <param name="button"></param>
        protected void ProcessEditShippingSubmit()
        {
            string distributorID = DistributorID;
            string countryCode = CountryCode;
            string locale = Locale;
            bool toSession = cbSaveThis.Checked ? false : true;
            int existingId;

            IShippingProvider shippingProvider = (Page as ProductsBase).GetShippingProvider();

            bool isNicknameEntered = false;
            var addressList = GetShippingAddressesFromDeliveryOptions(
                shippingProvider.GetShippingAddresses(distributorID, locale));

            //check for empty nickname in the addressList.
            //if (this.txtNickname.Text.Equals(String.Empty))
            //{
            //   bool isExists = addressList.Exists(l => l.Alias.Equals(this.txtNickname.Text.Trim()));

            //   if (isExists)
            //   {
            //       existingId = addressList.Find(l => l.Alias.Equals(this.txtNickname.Text.Trim())) == null ? 0 :
            //           addressList.Find(l => l.Alias.Equals(this.txtNickname.Text.Trim())).ID;
            //       if (!this.WorkedUponAddress.ID.Equals(existingId))
            //       {
            //           //check for duplicate address.
            //           if (ProductsBase.GetShippingProvider().duplicateShippingAddress(addressList, this.WorkedUponAddress))
            //           {
            //               this.trError.Visible = true;
            //               this.blErrors.Items.Add(new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddressExists")));
            //               return;
            //           }
            //       }
            //   }
            //}

            int saveWorkingID = WorkedUponAddress.ID;

            var existentAdd =
                addressList.Where(l => l.Alias != null && l.Alias.Equals(txtNickname.Text.Trim())).FirstOrDefault();
            existingId = (existentAdd != null) ? existentAdd.ID : 0;

            if (!WorkedUponAddress.ID.Equals(existingId))
            {
                isNicknameEntered = true;
            }

            WorkedUponAddress.Alias = txtNickname.Text.Trim();

            //1) Submit validated chnages to Shippping Provider
            WorkedUponAddress.ID = ProductsBase.GetShippingProvider().SaveShippingAddress
                (distributorID, locale, WorkedUponAddress
                 , toSession
                 , true
                 , isNicknameEntered);

            if (WorkedUponAddress.ID == -2) //duplicateShippingAddress
            {
                trError.Visible = true;
                blErrors.Items.Add(
                    new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "AddressExists")));
                return;
            }
            if (WorkedUponAddress.ID == -3) //duplicateNickName
            {
                trError.Visible = true;
                blErrors.Items.Add(
                    new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "DuplicateAddressNickname")));

                // don't loose the ID , otherwise it will become an insert
                if (isNicknameEntered)
                {
                    WorkedUponAddress.ID = saveWorkingID;
                    WorkedUponAddress.Alias = txtNickname.Text.Trim();
                }

                return;
            }
            else
            {
                WorkedUponAddress.DisplayName = WorkedUponAddress.Alias == string.Empty
                                                    ? shippingProvider.GetAddressDisplayName(WorkedUponAddress)
                                                    : WorkedUponAddress.Alias;

                ShoppingCart.CopyInvoiceStatus = ServiceProvider.CatalogSvc.CopyInvoiceStatus.success;
                Session["IsCopingFromInvoice"] = null;
                OnShippingAddressChanged(this,
                                         new ShippingAddressEventArgs(distributorID, WorkedUponAddress, false, false));
            }
        }

        private static List<ShippingAddress_V02> GetShippingAddressesFromDeliveryOptions(
            List<DeliveryOption> deliveryOptions)
        {
            var results = new List<ShippingAddress_V02>();
            foreach (DeliveryOption option in deliveryOptions)
            {
                results.Add(option);
            }

            return results;
        }

        private bool IsAddressNotChanged(ShippingAddress_V02 oldAddr, ShippingAddress_V02 newAddr)
        {
            if (oldAddr.Recipient == newAddr.Recipient &&
                oldAddr.FirstName == newAddr.FirstName &&
                oldAddr.MiddleName == newAddr.MiddleName &&
                oldAddr.LastName == newAddr.LastName &&
                oldAddr.Phone == newAddr.Phone &&
                oldAddr.Address.Line1 == newAddr.Address.Line1 &&
                oldAddr.Address.Line2 == newAddr.Address.Line2 &&
                oldAddr.Address.Line3 == newAddr.Address.Line3 &&
                oldAddr.Address.City == newAddr.Address.City &&
                oldAddr.Address.PostalCode == newAddr.Address.PostalCode &&
                oldAddr.Address.CountyDistrict == newAddr.Address.CountyDistrict &&
                oldAddr.Address.StateProvinceTerritory == newAddr.Address.StateProvinceTerritory)
            {
                return true;
            }
            return false;
        }

        public ShippingAddress_V01 getCurrentShippingInfo()
        {
            string key = AddressingConfiguration.GetCurrentShippingSessionKey((Page as ProductsBase).Locale,
                                                                              (Page as ProductsBase).DistributorID);

            ShippingAddress_V01 shipping = Session[key] as ShippingAddress_V01 ?? null;
            return shipping;
        }

        protected ordering.AddressBase getShippingControl(Control parent)
        {
            return parent.Controls.Count > 1 ? parent.Controls[1] as ordering.AddressBase : null;
        }

        protected void ContinueChanges_Clicked(object sender, EventArgs e)
        {
            UpdateViewChanges();
            // close popup

            if (WorkedUponAddressControl == null
                || WorkedUponAddressControl.ErrorList == null
                || WorkedUponAddressControl.ErrorList.Count.Equals(0)
                && (!HLConfigManager.Configurations.AddressingConfiguration.HasCustomErrorExpression ||
                (HLConfigManager.Configurations.AddressingConfiguration.HasCustomErrorExpression && Page.IsValid)))
            {
                if (blErrors.Items.Count.Equals(0))
                {
                    if (PopupExtender != null)
                    {
                        PopupExtender.ClosePopup();
                        ViewState[POPUPSHOWN] = null;
                        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "body_overflow_remove", "$('body').css('overflow', 'visible');", true);
                    }
                }
            }
            if (SessionInfo != null && btnContinue != null && btnContinue.Text == "Continuar")
                SessionInfo.IsVenuzulaShipping = false;
        }

        protected void CancelChanges_Clicked(object sender, EventArgs e)
        {
            //popup_AddEditShippingControl.Hide();
            //OnCommandCancelled(this, null);
            if (PopupExtender != null)
            {
                PopupExtender.ClosePopup();
                OnShippingAddressPopupCancelled(sender, e);
                ViewState[POPUPSHOWN] = null;
                ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "body_overflow_remove", "$('body').css('overflow', 'visible');", true);
            }
        }

        protected void cbSaveThis_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbSaveThis.Checked)
            {
                cbMakePrimary.Enabled = false;
                cbMakePrimary.Checked = false;
                //this.btnCancel.Focus();
            }
            else
            {
                cbMakePrimary.Enabled = true;
                //this.cbMakePrimary.Focus();
            }
            cbSaveThis.Focus();
        }

        private void UpdateViewChanges()
        {
            trError.Visible = false;
            blErrors.Items.Clear();
            //Branch out for DELETE
            // need to get to root cause of SourceCommand sometimes being null here. In the meantime check for null
            if (SourceCommand != null && SourceCommand.Mode == ShippingAddressCommandType.DELETE)
            {
                ProcessDeleteShippingSubmit();
                return;
            }

            string countryCode = CountryCode;
            //this.btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            //1) Collect the final changes made by user from VIEW
            ordering.AddressBase shippingAddressCtrl = getShippingControl(colNewShippingAddress);

            // crashing - no point otherwise
            if (shippingAddressCtrl == null)
                return;

            ShippingAddress_V02 address = null;
            address = createAddressFromControl(shippingAddressCtrl, countryCode);
            if (address == null)
                return;
            WorkedUponAddress = address;
            WorkedUponAddressControl = shippingAddressCtrl;

            //Process ADD/EDIT Common Logic
            address.IsPrimary = cbMakePrimary.Checked;

            //2) Validate user Changes on VIEW
            if (!WorkedUponAddressControl.Validate()
                || (HLConfigManager.Configurations.AddressingConfiguration.HasCustomErrorExpression
                    && !Page.IsValid))
            {
                // IsOkToSave moved to NorthAmericaAddressControl, since only US is calling it for AVS
                setUIErrorGrid();
                return;
            }

            // ****updateViewWithAVSRecommendation don't override phone any more, so don't need to call createAddressFromControl again for phone merge
            //US: after the AVS call to update the fields,'createAddressFromControl' is called to set the phone number field.
            //if (Locale.Substring(3, 2).Equals("US"))
            //    createAddressFromControl(this.WorkedUponAddressControl, countryCode);

            //for countries that do not have the phone split(areacode, phone and/or extension,
            //append '-' at the beginning and end of phone number.
            if (!HLConfigManager.Configurations.DOConfiguration.PhoneSplit)
            {
                if (WorkedUponAddress.Phone != null && !WorkedUponAddress.Equals(string.Empty))
                    WorkedUponAddress.Phone = "-" + WorkedUponAddress.Phone + "-";
            }

            //if ((this.txtNickname.Text.Trim() != null) && (!this.txtNickname.Text.Trim().Equals(string.Empty)))
            //{
            //    if (!System.Text.RegularExpressions.Regex.IsMatch(this.txtNickname.Text.Trim(),
            //          @"(^[A-Za-z0-9 ]+$)"))
            //    {
            //        this.trError.Visible = true;
            //        this.blErrors.Items.Add(new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidNickname")));
            //        return;
            //    }
            //}

            IShippingProvider shippingProvider = (Page as ProductsBase).GetShippingProvider();
            if (HLConfigManager.Configurations.AddressingConfiguration.ValidatePostalCode)
            {
                if (shippingProvider != null)
                {
                    if (!shippingProvider.ValidatePostalCode(WorkedUponAddress.Address.Country,
                                                             WorkedUponAddress.Address.Country,
                                                             WorkedUponAddress.Address.Country,
                                                             WorkedUponAddress.Address.PostalCode))
                    {
                        trError.Visible = true;
                        blErrors.Items.Add(
                            new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "InvalidZipCode")));
                        return;
                    }
                }
            }
            if (HLConfigManager.Configurations.AddressingConfiguration.ValidateStreetAddress)
            {
                if (shippingProvider != null)
                {
                    if (!shippingProvider.ValidateAddress(WorkedUponAddress))
                    {
                        trError.Visible = true;
                        blErrors.Items.Add(
                            new ListItem(PlatformResources.GetGlobalResourceString("ErrorMessage", "GeneralInputError")));
                        return;
                    }
                }
            }

            //3) Branch out to perform ADD/EDIT specific Logic
            if (SourceCommand != null)
            {
                switch (SourceCommand.Mode)
                {
                    case ShippingAddressCommandType.ADD:
                        ProcessAddShippingSubmit();
                        return;
                    case ShippingAddressCommandType.EDIT:
                        ProcessEditShippingSubmit();
                        return;
                }
            }
        }

        private void setUIErrorGrid()
        {
            trError.Visible = true;
            blErrors.DataSource = WorkedUponAddressControl.ErrorList;
            blErrors.DataBind();
        }

        private void ProcessDeleteShippingSubmit()
        {
            string distributorID = (Page as ProductsBase).DistributorID;
            string locale = (Page as ProductsBase).Locale;

            IShippingProvider shippingProvider = (Page as ProductsBase).GetShippingProvider();
            shippingProvider.DeleteShippingAddress(distributorID, locale, WorkedUponAddress);

            OnShippingAddressDeleted(this, new ShippingAddressEventArgs(distributorID, WorkedUponAddress, false, false));
            //popup_AddEditShippingControl.Hide();
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressBeingCreated)]
        public void OnShippingAddressBeingCreated(object sender, EventArgs e)
        {
            lblHeader.Text = GetLocalResourceObject("lblHeaderResource1.Text") as string;
            //this.btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            btnContinue.Enabled = true;
            blErrors.Items.Clear();
            txtNickname.Text = string.Empty;

            var arg = e as ShippingAddressEventArgs;
            if (arg != null)
            {
                WorkedUponAddress = arg.ShippingAddress;
                hfDiableSavedCheckbox.Value = arg.DisableSaveAddressCheckbox.ToString();
            }

            //Set the MODE & Load respective controlset
            SourceCommand = new ShippingAddressCommand(ShippingAddressCommandType.ADD);
            loadControls();

            RenderAddShippingView();
            ViewState[POPUPSHOWN] = true;
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressBeingDeleted)]
        public void OnShippingAddressBeingDeleted(object sender, EventArgs e)
        {
            lblDeleteHeader.Text = GetLocalResourceObject("lblDeleteHeaderResource1.Text") as string;
            //this.btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;
            //btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;
            btnContinue.Enabled = true;
            blErrors.Items.Clear();

            SourceCommand = new ShippingAddressCommand(ShippingAddressCommandType.DELETE);
            loadControls();
            var arg = e as ShippingAddressEventArgs;

            if (arg != null)
            {
                WorkedUponAddress = arg.ShippingAddress;
                RenderDeleteShippingView();
                //ProcessDeleteShippingSubmit();
            }
            ViewState[POPUPSHOWN] = true;
        }

        [SubscribesTo(MyHLEventTypes.ShippingAddressBeingChanged)]
        public void OnShippingAddressBeingChanged(object sender, EventArgs e)
        {
            lblHeader.Text = GetLocalResourceObject("lblEditHeaderResource1.Text") as string;
            //this.btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;
            btnContinue.Enabled = true;
            blErrors.Items.Clear();

            //Set the MODE & Load respective controlset
            SourceCommand = new ShippingAddressCommand(ShippingAddressCommandType.EDIT);
            loadControls();

            var arg = e as ShippingAddressEventArgs;
            if (arg != null)
            {
                OldAddress = arg.ShippingAddress;
                WorkedUponAddress = arg.ShippingAddress;
                hfDiableSavedCheckbox.Value = arg.DisableSaveAddressCheckbox.ToString();
                RenderEditShippingView();
            }
            ViewState[POPUPSHOWN] = true;
        }

        #region ControlPaths

        //string getAddressControlPath(ref bool isXML)
        //{
        //    if (!string.IsNullOrEmpty(HLConfigManager.Configurations.AddressingConfiguration.Address))
        //    {
        //        isXML = false;
        //        return HLConfigManager.Configurations.AddressingConfiguration.Address;
        //    }
        //    isXML = true;
        //    return HLConfigManager.Configurations.AddressingConfiguration.AddressXML;
        //}

        private string getEditAddressControlPath(ref bool isXML)
        {
            if (!string.IsNullOrEmpty(HLConfigManager.Configurations.AddressingConfiguration.GDOEditAddressxml))
            {
                isXML = true;
                return HLConfigManager.Configurations.AddressingConfiguration.GDOEditAddressxml;
            }
            isXML = false;
            //return HLConfigManager.Configurations.AddressingConfiguration.EditAddressXML;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOEditAddress;
        }

        private string getDeleteAddressControlPath(ref bool isXML)
        {
            isXML = true;
            return HLConfigManager.Configurations.AddressingConfiguration.GDOStaticAddress;
        }

        #endregion ControlPaths

        #region AddressFromControl

        protected ordering.AddressBase createAddress(string controlPath, bool isXML)
        {
            return AddressBaseProvider.Get(Locale
                                           , this, controlPath, isXML);
        }

        private ShippingAddress_V02 createAddressFromControl(ordering.AddressBase addressCntrl, string country)
        {
            var shippingAddress = addressCntrl.CreateAddressFromControl() as ShippingAddress_V02;
            if (HLConfigManager.Configurations.DOConfiguration.PhoneSplit)
            {
                string areaCode = shippingAddress.AreaCode ?? string.Empty;
                string extension = shippingAddress.AltAreaCode ?? string.Empty;
                string phone = shippingAddress.Phone ?? string.Empty;

                if (!string.IsNullOrEmpty(extension))
                {
                    if (addressCntrl.ErrorList == null)
                        addressCntrl.ErrorList = new List<string>();

                    if (!Regex.IsMatch(extension, @"^\d+$"))
                        addressCntrl.ErrorList.Add(PlatformResources.GetGlobalResourceString("ErrorMessage",
                                                                                             "InvalidExtension"));
                }

                if (!phone.Equals(string.Empty) && areaCode.Equals(string.Empty) && extension.Equals(string.Empty))
                {
                    phone = "-" + phone + "-";
                }
                if (!phone.Equals(string.Empty) && !areaCode.Equals(string.Empty) && extension.Equals(string.Empty))
                {
                    phone = areaCode + "-" + phone + "-";
                }
                if (!phone.Equals(string.Empty) && !areaCode.Equals(string.Empty) && !extension.Equals(string.Empty))
                {
                    phone = areaCode + "-" + phone + "-" + extension;
                }
                if (phone.Equals(string.Empty) && areaCode.Equals(string.Empty) && !extension.Equals(string.Empty))
                {
                    phone = extension + "--";
                }

                shippingAddress.AreaCode = string.Empty;
                shippingAddress.AltAreaCode = string.Empty;

                shippingAddress.Phone = phone;
            }
            //else
            //    shippingAddress.Phone = "-" + shippingAddress.Phone + "-";

            if (shippingAddress != null)
            {
                shippingAddress.Address.Country = country;
            }
            //else
            //{
            //    shippingAddress = new ShippingAddress_V02();
            //    shippingAddress.Address = OrderCreationHelper.CreateDefaultAddress();
            //    shippingAddress.Address.Country = country;
            //}
            return shippingAddress;
        }

        #endregion AddressFromControl

        #region checkbox manip

        private void HideCheckboxes()
        {
            cbMakePrimary.Visible = false;
            cbSaveThis.Visible = false;
        }

        private void ShowCheckboxes()
        {
            cbMakePrimary.Visible = true;
            cbSaveThis.Visible = true;
        }

        #endregion checkbox manip

        #region Control Loading

        private void removeControl(Control parent)
        {
            if (parent.Controls.Count > 1)
            {
                (Page.Master as OrderingMaster).EventBus.DeregisterObject(parent.Controls[1]);
                parent.Controls.RemoveAt(1);
            }
        }

        private void loadControls()
        {
            bool isXML = true;
            string controlPath = "";
            ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "body_overflow_add", "$('body').css('overflow', 'hidden');", true);
            try
            {
                if (SourceCommand == null) return;

                if ((SourceCommand.Mode == ShippingAddressCommandType.ADD) ||
                    (SourceCommand.Mode == ShippingAddressCommandType.EDIT))
                {
                    if (getShippingControl(colNewShippingAddress) != null) return;

                    btnContinue.Text = GetLocalResourceObject("btnContinueContinue") as string;

                    divDeleteShippingAddress.Visible = false;
                    divAddEditShippingAddress.Visible = true;
                    controlPath = getEditAddressControlPath(ref isXML);
                    //Pull in the right AddressWindow definition (declarative controlset) from XML and load them

                    ordering.AddressBase addressBase = createAddress(controlPath, isXML);
                    removeControl(colNewShippingAddress);
                    colNewShippingAddress.Controls.Add((Control)addressBase);
                }

                if (SourceCommand.Mode == ShippingAddressCommandType.DELETE)
                {
                    if (getShippingControl(colDeleteShippingAddress) != null) return;

                    btnContinue.Text = GetLocalResourceObject("btnContinueDelete") as string;
                    divDeleteShippingAddress.Visible = true;
                    divAddEditShippingAddress.Visible = false;

                    controlPath = getDeleteAddressControlPath(ref isXML);

                    //Pull in the right AddressWindow definition (declarative controlset) from XML and load them
                    //ordering.AddressBase addressBase = createAddress(controlPath, isXML);
                    ordering.AddressBase addressBase = new ordering.AddressControl();
                    if (addressBase == null)
                    {
                    }
                    addressBase.XMLFile = controlPath;
                    removeControl(colDeleteShippingAddress);
                    if (colDeleteShippingAddress.Controls.Count >= 1)
                    {
                        colDeleteShippingAddress.Controls.Clear();
                    }
                    colDeleteShippingAddress.Controls.Add((Control)addressBase);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }

        #endregion Control Loading
        private void RegisterScript()
        {
            string script = "function Submission() {" +
                                "$('#" + divAddEditShippingAddress.ClientID + "  :input').prop('disabled', true); " +
                                "$('#" + divDeleteShippingAddress.ClientID + "  :input').prop('disabled', true); " +
                            "}";

            ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "Submission_Shipping", script, true);
        }
    }
}