using System;
using System.Web.Security;
using System.Web.UI;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel;
using HL.PGH.Api;
using HL.PGH.Contracts.ValueObjects;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments
{
    public partial class PGHInterface : Page
    {

#region Constants and Fields
        public const string PageName = "PGHInterface.aspx";
#endregion

#region Methods
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                PaymentRequest request = Session[PageName] as PaymentRequest;
                if (null != request)
                {
                    request.Submit();
                }
            }
            catch (Exception ex)
            {
                //Logging
            }
            finally
            {
                Session.Remove(PageName);
            }
        }
#endregion

    }
}