using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public class HLLinkButton : LinkButton
    {
        public int CategoryID { get; set; }
        public int ProductID { get; set; }
    }
}