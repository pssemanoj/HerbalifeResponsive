using MyHerbalife3.Ordering.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.HAP
{
    /// <summary>
    /// ModalType - Enumerator to select the PopUp to display
    /// </summary>
    public enum ModalType
    {
        Unknown,
        Renew,
        Cancel,
        EditHapAPS
    }

    public partial class HAPModal : UserControlBase
    {
        /// <summary>
        /// ModalOption - Option to display correct popup Renew/Cancel
        /// </summary>
        public ModalType ModalOption { get; set; }

        /// <summary>
        /// HAPOrderID - HAP OrderID to cancel
        /// </summary>
        public string HAPOrderID
        {
            get
            {
                if (ViewState["SelectedHAPOrderId"] == null)
                    return string.Empty;

                return ViewState["SelectedHAPOrderId"].ToString();
            }
            set
            {
                ViewState["SelectedHAPOrderId"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Initialize
                modalRenew.Visible = modalCancel.Visible = modalEdit.Visible = false;
            }
        }

        /// <summary>
        /// SelectModal - Method to display correct popup according the property ModalOption
        /// </summary>
        public void SelectModal()
        {
            switch (ModalOption)
            {
                case ModalType.Renew: modalRenew.Visible = true; break;
                case ModalType.Cancel: modalCancel.Visible = true; break;
                case ModalType.EditHapAPS: modalEdit.Visible = true; break;
            }
        }

        protected void RenewHapOrder_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if(!string.IsNullOrEmpty(DistributorID))
            {
                if(OrderProvider.RenewHap(DistributorID))
                    Response.Redirect("~/Ordering/HAPOrders.aspx");
            }
        }

        /// <summary>
        /// Event to Hide the Modal PopUp
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Cancel_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        protected void CancelHapOrder_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            if (!string.IsNullOrEmpty(HAPOrderID))
            {
                var oHAPOrders = (this.Page as ProductsBase).ActiveHAPOrders;
                var orderToCancel = oHAPOrders != null ? oHAPOrders.Find(oi => oi.OrderID == HAPOrderID) : null;
                if (orderToCancel != null)
                {
                    var error = OrderProvider.SubmitHAPOrder(orderToCancel, (this.Page as ProductsBase).Locale, (this.Page as ProductsBase).DistributorID);
                    if (string.IsNullOrEmpty(error))
                    {
                        var orders = OrderProvider.GetHapOrders((this.Page as ProductsBase).DistributorID, (this.Page as ProductsBase).CountryCode);
                        orders.RemoveAll(r => r.OrderID == orderToCancel.OrderID);
                    }
                    Response.Redirect("~/Ordering/HAPOrders.aspx");
                }
            }

        }

        protected void EditHapOrder_Click(object sender, EventArgs e)
        {
            //redirect to COP1 passing via querystring the #hap order
            Response.Redirect(string.Format("~/Ordering/ShoppingCart.aspx?HAP=True&hapId={0}", HAPOrderID));
        }
    }
}