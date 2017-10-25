using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class AddressRestrictionPopUp : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            AddressRestrictionPopupExtender.Hide();
            Session["showedAddressRestrictionPopup"] = true;
            Response.Redirect("~/Ordering/SavedShippingAddress.aspx");
        }

        public void ShowAddressRestrictionPopUp()
        {
            AddressRestrictionPopupExtender.Show();
        }
    }
}