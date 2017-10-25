using System;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Checkout
{
    public partial class ConfirmAddress : UserControlBase
    {
        public bool IsConfirmed
        {
            get
            {
                return cbConfirmAddress.Checked;
            }
            set
            {
                var oldValue = cbConfirmAddress.Checked;
                cbConfirmAddress.Checked = value;
                if (oldValue != cbConfirmAddress.Checked)
                {
                    this.ConfirmAddChanged(this, null);
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            this.cbConfirmAddress.Checked = this.SessionInfo.ConfirmedAddress;
        }

        protected void ConfirmAddChanged(object sender, EventArgs e)
        {
            this.SetConfirmAddressContentStyle();
            this.SessionInfo.ConfirmedAddress = this.cbConfirmAddress.Checked;
        }

        public void SetConfirmAddressContentStyle()
        {
            panelConfirmAddress.CssClass = cbConfirmAddress.Checked ? "mandatoryConfirm" : "errorConfirm";
        }

        public void SetVisibility(bool visible)
        {
            if (!visible)
            {
                this.cbConfirmAddress.Checked = true;
            }
            this.Visible = visible;
        }
    }
}