using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Shared.UI.Helpers;

namespace MyHerbalife3.Web.Controls.Logon
{
    public partial class LocaleSelector : UserControl
    {
        protected ListItemCollection AllCountries = new ListItemCollection();

        protected void Page_Load(object sender, EventArgs e)
        {
            var loader = new LocaleCountryLoader();
            var result = new ListItemCollection();

            var allCountries = loader.Load();

            var listItems =
                allCountries.OrderBy(kvp => kvp.Value).Select(entry => new ListItem(entry.Value, entry.Key)).ToArray();

            result.AddRange(listItems);

            AllCountries = result;
        }
    }
}