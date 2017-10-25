using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security.AntiXss;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace MyHerbalife3.Ordering.Web.Ordering
{
    public partial class ZoomImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var src = Server.UrlDecode(this.Request.QueryString["Image"]);
            this.Img2.Src = validInput(src);
        }

        protected string validInput(string input)
        {
            var regex = "[\'<>\"]";
            if (null != input && !input.Contains("\"") && input.StartsWith("/"))
            {
                    return !Regex.IsMatch(input, regex) ? AntiXssEncoder.XmlAttributeEncode(input):string.Empty;
            }
            return string.Empty;
        }

    }
}
