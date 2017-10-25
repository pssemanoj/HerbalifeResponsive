using System;
using System.Text;
using System.Web.Security;
using System.Web.UI;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class ThreeDAuthenticate : Page
    {
        #region Constants and Fields

        protected SessionInfo CurrentSession = null;

        #endregion Constants and Fields

        #region Methods

        protected void Page_Load(object sender, EventArgs e)
        {
            string locale = HLConfigManager.Configurations.Locale;
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            var user = member.Value;
            string distributorId = (null != user) ? user.Id : string.Empty;
            if (!string.IsNullOrEmpty(distributorId))
            {
                CurrentSession = SessionInfo.GetSessionInfo(distributorId, locale);
                if (CurrentSession == null)
                {
                    LoggerHelper.Error(string.Format("PaymentGatewayInvoker - ThreeDAuthenticate, Session is null. Distributor Id : {0} ",
                            distributorId));
                    return;
                }
            }

            Response.Clear();

            var termUrlPrefix = Settings.GetRequiredAppSetting("RootURLPerfix", "https://");
            var termUrl = string.Format("{0}{1}", termUrlPrefix, Request.Url.DnsSafeHost + "/Ordering/Checkout.aspx");
            var paReq = CurrentSession.ThreeDSecuredCardInfo.PaReq;
            var md = "Herbalife3DSecuredCreditCardAuthentication";

            var sb = new StringBuilder();
            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form3D""].submit()'>");
            sb.AppendFormat("<form id='form3D' name='form3D' action='{0}' method='post'>", CurrentSession.ThreeDSecuredCardInfo.AcsUrl);

            if (CurrentSession.ThreeDSecuredCardInfo.CountryCode == "TR")
            {
                sb.AppendFormat("<input type='hidden' name='mid' value='{0}'>", CurrentSession.ThreeDSecuredCardInfo.MerchantId);
                sb.AppendFormat("<input type='hidden' name='posnetID' value='{0}'>", CurrentSession.ThreeDSecuredCardInfo.AccountId);
                sb.AppendFormat("<input type='hidden' name='posnetData' value='{0}'>", CurrentSession.ThreeDSecuredCardInfo.PaReq);
                sb.AppendFormat("<input type='hidden' name='posnetData2' value='{0}'>", CurrentSession.ThreeDSecuredCardInfo.CommerceIndicator);
                sb.AppendFormat("<input type='hidden' name='digest' value='{0}'>", CurrentSession.ThreeDSecuredCardInfo.RequestToken);
                sb.AppendFormat("<input type='hidden' name='vftCode' value='{0}'>", string.Empty);
                sb.AppendFormat("<input type='hidden' name='merchantReturnURL' value='{0}'>", termUrl);
                sb.AppendFormat("<input type='hidden' name='lang' value='{0}'>", "tr");
                sb.AppendFormat("<input type='hidden' name='url' value='{0}'>", string.Empty);
                sb.AppendFormat("<input type='hidden' name='openANewWindow' value='{0}'>", "0");
            }
            else
            {
                sb.AppendFormat("<input type='hidden' name='PaReq' value='{0}'>", paReq);
                sb.AppendFormat("<input type='hidden' name='TermUrl' value='{0}'>", termUrl);
                sb.AppendFormat("<input type='hidden' name='MD' value='{0}'>", md);
            }

            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            Response.Write(sb.ToString());
            Response.End();
        }
        #endregion Methods
    }
}