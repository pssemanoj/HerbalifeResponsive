using System;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.Web.MasterPages;
using MyHerbalife3.Ordering.Web.Ordering.Controls;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class HFFPayment : ProductsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var _hFFModal = LoadControl("~/Ordering/Controls/GHFFModal.ascx") as GHFFModal;
            plHFFModal.Controls.Add(_hFFModal);
            ((OrderingMaster) Page.Master).EventBus.RegisterObject(_hFFModal);
        }

        #region Bussed Events

        [SubscribesTo("HFFOrderPlaced")]
        public void HFFOrderTypePlaced(object sender, EventArgs e)
        {
        }

        [SubscribesTo("HFFOrderCreated")]
        public void HFFOrderCreated(object sender, EventArgs e)
        {
            mdlDonate.Show();
        }

        #endregion
    }
}