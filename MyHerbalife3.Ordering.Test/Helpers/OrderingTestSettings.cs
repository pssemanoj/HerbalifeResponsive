// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderingTestSettings.cs" company="Herbalife">
//   Herbalife 2013
// </copyright>
// <summary>
//  
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using MyHerbalife3.Ordering.Test.Mocks;
using MyHerbalife3.Shared.ViewModel;
using System.Globalization;
using System.Threading;
using System.Web.Security;

namespace MyHerbalife3.Ordering.Test.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderingTestSettings
    {
        /// <summary>
        /// Gets or sets the locale.
        /// </summary>
        /// <value>
        /// The locale.
        /// </value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the distributor.
        /// </summary>
        /// <value>
        /// The distributor.
        /// </value>
        public string Distributor { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderingTestSettings" /> class.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="distributor">The distributor.</param>
        public OrderingTestSettings(string locale, string distributor)
        {
            this.Locale = locale;
            this.Distributor = distributor;

            // Setting the current culture.
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(locale);

            //var membershipUser = (Membership.GetUser(distributor, true)) as MembershipUser<DistributorProfileModel>;
            FormsAuth v = new FormsAuth();
            v.Login(distributor);
        }
    }
}
