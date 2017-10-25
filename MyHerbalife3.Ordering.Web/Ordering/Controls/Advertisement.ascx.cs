using MyHerbalife3.Ordering.Web.MasterPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls
{
    public partial class Advertisement : UserControlBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (IsChina)
                {
                    if (!SessionInfo.IsAdvertisementShowed)
                    {
                        SessionInfo.IsAdvertisementShowed = true;
                    }
                    else
                    {
                        divPanel.Visible = false;
                    }
                }
                else
                {
                    divPanel.Visible = false;
                }
            
            }
        }
 
        }
}