using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Lib
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ScriptInjecter runat=server></{0}:ScriptInjecter>")]
    public class ScriptInjecter : WebControl
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                String s = (String)ViewState["Text"];
                return ((s == null) ? String.Empty : s);
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(Text);
        }
    }
}
