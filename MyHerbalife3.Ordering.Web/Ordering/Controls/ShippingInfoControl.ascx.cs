using System;
using System.Web.UI;
using AjaxControlToolkit;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class ModalPopupExtenderBase : ModalPopupExtender
    {
        public virtual void ClosePopup()
        {
            var userControlBase = TemplateControl as UserControlBase;
            if (userControlBase != null)
            {
                userControlBase.HidePopup();
            }
        }
    }

    public class ShippingModalPopupExtender : ModalPopupExtenderBase
    {
    }

    public class ShippingInfoControlBase : UserControlBase
    {
        public virtual void ShowPopupForShipping(CommandType command, ShippingAddressEventArgs args)
        {
        }

        public virtual void ShowPopupForPickup(CommandType command, DeliveryOptionEventArgs args)
        {
        }
    }

    public partial class ShippingInfoControl : ShippingInfoControlBase
    {
        [Publishes(MyHLEventTypes.ShippingAddressBeingCreated)]
        public event EventHandler OnShippingAddressBeingCreated;

        [Publishes(MyHLEventTypes.ShippingAddressBeingChanged)]
        public event EventHandler OnShippingAddressBeingChanged;

        [Publishes(MyHLEventTypes.ShippingAddressBeingDeleted)]
        public event EventHandler OnShippingAddressBeingDeleted;

        [Publishes(MyHLEventTypes.PickupPreferenceBeingCreated)]
        public event EventHandler OnPickupPreferenceBeingCreated;

        [Publishes(MyHLEventTypes.PickupPreferenceBeingDeleted)]
        public event EventHandler OnPickupPreferenceBeingDeleted;

        [Publishes(MyHLEventTypes.ShippingAddressPopupClosed)]
        public event EventHandler OnShippingAddressPopupClosed;

        private const string ShippingPopupShown = "ShippingPopupShown";
        private const string PickupPopupShown = "PickupPopupShown";

        private readonly string ShippingControl = HLConfigManager.Configurations.AddressingConfiguration.ShippingControl;
        private readonly string PickupControl = HLConfigManager.Configurations.AddressingConfiguration.PickupControl;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                if (Session[ShippingPopupShown] != null)
                {
                    createAddressControl(DeliveryOptionType.Shipping);
                }
                if (Session[PickupPopupShown] != null)
                {
                    createAddressControl(DeliveryOptionType.Pickup);
                }
            }
        }


        private void removeAddressControl()
        {
            if (PanelShipping.Controls.Count > 1)
            {
                (Page.Master as OrderingMaster).EventBus.DeregisterObject(PanelShipping.Controls[1]);
                PanelShipping.Controls.RemoveAt(1);
            }
        }

        private void createAddressControl(DeliveryOptionType type)
        {
            UserControlBase userControlBase = null;
            if (type == DeliveryOptionType.Shipping)
            {
                userControlBase = LoadControl(ShippingControl) as UserControlBase;
            }
            else
            {
                userControlBase = LoadControl(PickupControl) as UserControlBase;
            }
            if (userControlBase != null)
            {
                removeAddressControl();
                userControlBase.PopupExtender = popup_ShippingInfoControl;
                PanelShipping.Controls.Add(userControlBase);
            }
        }

        public void ShowPopupForType()
        {
            ShowPopupForShipping(CommandType.Add, null);
            //trTypeSelection.Visible = true;
        }

        public override void ShowPopupForShipping(CommandType command, ShippingAddressEventArgs args)
        {
            ScriptManager.RegisterStartupScript(Page, typeof(Page), "visibleAreaHeight", "visibleAreaHeight();", true);
            Session[ShippingPopupShown] = true;
            createAddressControl(DeliveryOptionType.Shipping);

            //trTypeSelection.Visible = false;
            switch (command)
            {
                case CommandType.Add:
                    OnShippingAddressBeingCreated(this, args);
                    break;
                case CommandType.Delete:
                    OnShippingAddressBeingDeleted(this, args);
                    break;
                case CommandType.Edit:
                    OnShippingAddressBeingChanged(this, args);
                    break;
            }
            popup_ShippingInfoControl.Show();
            upShippingInfoControl.Update();
        }

        public override void ShowPopupForPickup(CommandType command, DeliveryOptionEventArgs args)
        {
            Session[PickupPopupShown] = true;
            createAddressControl(DeliveryOptionType.Pickup);
            //trTypeSelection.Visible = false;

            switch (command)
            {
                case CommandType.Add:
                    OnPickupPreferenceBeingCreated(this, args);
                    break;
                case CommandType.Delete:
                    OnPickupPreferenceBeingDeleted(this, args);
                    break;
                case CommandType.Edit:
                    break;
            }
            popup_ShippingInfoControl.Show();
            upShippingInfoControl.Update();
            Session["Pickupinfofilled"] = true;
        }

        //public void ResetSelection()
        //{
        //    DeliveryType.SelectedIndex = 0;
        //}
        //public DeliveryOptionType DeliveryOption()
        //{
        //    if (DeliveryType.SelectedIndex == 0)
        //        return DeliveryOptionType.Shipping;
        //    else if (DeliveryType.SelectedIndex == 1)
        //        return DeliveryOptionType.Pickup;
        //    return DeliveryOptionType.Unknown;
        //}
        protected void OnDeliveryTypeChanged(object sender, EventArgs e)
        {
        }

        public void Hide()
        {
            //ScriptManager.RegisterStartupScript(Page, typeof (Page), "EnableMasterScroll", "EnableMasterScrolling();",true);
            Session[PickupPopupShown] = Session[ShippingPopupShown] = null;
            popup_ShippingInfoControl.Hide();
        }

        public override void HidePopup()
        {
            Hide();
            OnShippingAddressPopupClosed(this, null);
        }
    }
}