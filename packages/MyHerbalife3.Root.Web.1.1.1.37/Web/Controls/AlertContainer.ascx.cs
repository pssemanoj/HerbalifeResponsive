#region

using System;
using System.Web;
using MyHerbalife3.Shared.UI;

#endregion

namespace HL.MyHerbalife.Web.Controls
{
    public partial class AlertContainer : UserControlBase
    {
        #region Protected Fields

        protected const string AlertContainerCollapsedKey = "IsAlertContainerCollapsed";

        /// <summary>
        ///     Gets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        protected bool IsEnabled
        {
            get { return Page.User.Identity.IsAuthenticated && GlobalContext.CultureConfiguration.IsDSAlertsEnabled; }
        }       

        /// <summary>
        ///     Gets or sets a value indicating whether this control is collapsed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is collapsed; otherwise, <c>false</c>.
        /// </value>
        protected bool IsCollapsed
        {
            get
            {
                if (ViewState[AlertContainerCollapsedKey] != null)
                {
                    return Convert.ToBoolean(ViewState[AlertContainerCollapsedKey]);
                }
                return false;
            }
            set { ViewState[AlertContainerCollapsedKey] = value; }
        }

        /// <summary>
        ///     Gets the current URL.
        /// </summary>
        /// <value>
        ///     The current URL.
        /// </value>
        protected string CurrentUrl
        {
            get
            {
                return HttpContext.Current.Request.Url.AbsolutePath.Remove(0,
                                                                           (VirtualPathUtility.ToAbsolute("~/")).Length -
                                                                           1);
            }
        }

        #endregion
    }
}