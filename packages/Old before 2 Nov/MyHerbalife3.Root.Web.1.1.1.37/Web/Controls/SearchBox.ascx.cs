using System;
using System.Web;
using System.Web.Security;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.Extensions;
using MyHerbalife3.Shared.ViewModel;

namespace HL.MyHerbalife.Web.Controls
{
    public partial class SearchBox : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Visible = !Roles.IsUserInRole(RoleDefinitions.ROLE_DELETED_ID);

            _GoButton.Click += _GoButton_Click;
        }

        private void _GoButton_Click(object sender, EventArgs e)
        {
            if (!_search_txt.Text.Equals(GetLocalResourceObject("SearchTerms.Text")) && !_search_txt.Text.Equals(""))
            {
                string targetUrl =
                    ResolveClientUrl("~/Home/SiteSearch.aspx?" + RequestExtension.SEARCH_TERMS_KEY + '=' +
                                     HttpUtility.HtmlEncode(_search_txt.Text));
                Response.Redirect(targetUrl);
            }
        }
    }
}