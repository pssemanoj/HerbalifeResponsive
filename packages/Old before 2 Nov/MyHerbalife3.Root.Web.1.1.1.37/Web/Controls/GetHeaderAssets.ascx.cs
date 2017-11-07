using System;
using System.Web;
using System.Web.UI;
using MyHerbalife3.Shared.Infrastructure.Interfaces;

namespace MyHerbalife3.Web.Controls
{
    public partial class getheader : UserControl
    {
        protected readonly IGlobalContext _GlobalContext = (HttpContext.Current.ApplicationInstance as IGlobalContext);

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string locale
        {
            get {
                return _GlobalContext.CultureConfiguration.Locale;

            }
        }
    }
}