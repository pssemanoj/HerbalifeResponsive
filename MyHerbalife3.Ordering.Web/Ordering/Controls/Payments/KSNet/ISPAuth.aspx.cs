using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.KSNet
{
    public partial class ISPAuth : System.Web.UI.Page
    {

#region Fields
        protected string ordername = string.Empty;
        protected string ordernumber = string.Empty;
        protected string idnum = string.Empty;
        protected string email = string.Empty;
        protected string phoneno = string.Empty;
        protected string goodname = string.Empty;
        protected int currencytype = 0;
        protected int amount = 0;
        protected int cardcode = 0;
        protected int installments = 0;
        protected int BCPoints = 0;
#endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            KSAuthInfo info = Session[KSAuthInfo.Key] as KSAuthInfo;
            Session[KSAuthInfo.Key] = null;

            if (null != info)
            {
                goodname = info.Name;
                currencytype = info.CurrencyType;
                amount = info.Amount;
                cardcode = info.CardCode;
                installments = info.Installments;
            }
        }
    }
}