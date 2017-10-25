using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class NostalgicDropDownList : DropDownList, INamingContainer
    {
        protected override object SaveViewState()
        {
            base.ViewState["previousValue"] = SelectedValue;
            return base.SaveViewState();
        }


        public string PreviousSelectedValue
        {
            get { return ViewState["previousValue"] as string; }
        }
    }
}