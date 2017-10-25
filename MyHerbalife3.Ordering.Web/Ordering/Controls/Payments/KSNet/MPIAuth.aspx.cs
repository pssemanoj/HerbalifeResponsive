using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.KSNet
{
    public partial class MPIAuth : System.Web.UI.Page
    {

#region Consts
        protected const string MPI_HOST = "https://kspay.ksnet.to/totmpi/veri_host.jsp";
        private const string ThisPagePath = "/ordering/controls/payments/ksnet/mpiauth.aspx";
        private const string CDEPagePath = "/cde/mpitokenizer.ashx";
        private const string DummyPagePath = "/ordering/controls/payments/ksnet/dummy.aspx";
        private const string InitPageScript = "<script type=\"text/javascript\">_init();</script>";
        private const string SubmitRequestScript = "<script type=\"text/javascript\">document.forms[0].submit();</script>";
        private const string SubmitResponseScript = "<script type=\"text/javascript\">var form = document.parentWindow.parent.document.forms[0];if (null != document.parentWindow.parent.setReturnedValues) {document.parentWindow.parent.setReturnedValues('xid', 'eci', 'cavv', '', '', 'cardnumber');}</script>";
        private const string CDEJavasecriptRedirect = "javascript:{alert(document.forms[0].cardno.value);";
#endregion

#region Fields	
        protected string rootUrl = string.Empty;
        protected string returnUrl = string.Empty;
        protected string dummyUrl = string.Empty;
        protected string ordername = string.Empty;                             
        protected string ordernumber = string.Empty;                   
        protected string idnum = string.Empty;
        protected string email = string.Empty;                           
        protected string phoneno = string.Empty;              
        protected string goodname = string.Empty;
        protected string accept = string.Empty;
        protected string userAgent = string.Empty;
        protected int currencytype = 0; 
        protected int amount = 0;
        protected int cardcode = 0;
        protected int installments = 0;
        protected int installmentType = 0;

        protected bool proceed = false;
        protected string Eci = string.Empty;
        protected string Cavv = string.Empty;
        protected string Xid = string.Empty;
        protected string CardNumber = string.Empty;
#endregion    
        
        protected void Page_Load(object sender, EventArgs e)
        {
            string disableTokens = System.Configuration.ConfigurationManager.AppSettings["TokenizationDisabled"];
            string rootScheme = HL.Common.Configuration.Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
            bool tokenizationDisabled = false;
            bool.TryParse(disableTokens, out tokenizationDisabled);

            rootUrl = string.Concat(rootScheme, Request.Url.DnsSafeHost);
            returnUrl = rootUrl + CDEPagePath;

            dummyUrl = rootUrl + DummyPagePath;
            KSAuthInfo info = Session[KSAuthInfo.Key] as KSAuthInfo;
            Session[KSAuthInfo.Key] = null;

            if (null != info)
            {
                amount = info.Amount;
                currencytype = info.CurrencyType;
                cardcode = info.CardCode;
                installments = info.Installments;
                accept = string.Join(",", Request.AcceptTypes);
                userAgent = Request.UserAgent;
                InitCode.Text = InitPageScript;
                ActionCode.Text = SubmitRequestScript;
            }
            else
            {
                if(bool.TryParse(Request["proceed"], out proceed) && proceed)
                {
                    Eci = Request["eci"];
                    Cavv = Request["cavv"];
                    Xid = Request["xid"];
                    CardNumber = Request["cardno"];
                }
                ActionCode.Text = MakeReturnScript();
            }
        }

        private string MakeReturnScript()
        {
            if (string.IsNullOrEmpty(Xid)) Xid = String.Empty;
            if (string.IsNullOrEmpty(Cavv)) Cavv = String.Empty;
            if (string.IsNullOrEmpty(Eci)) Eci = String.Empty;
            if (string.IsNullOrEmpty(CardNumber)) CardNumber = String.Empty;

            return SubmitResponseScript.Replace("xid", Xid).Replace("eci", Eci).Replace("cavv", Cavv).Replace("cardnumber", CardNumber);
        }      
    }
}