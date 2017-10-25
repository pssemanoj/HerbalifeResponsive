using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using MyHerbalife3.Core.DistributorProvider;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Shared.ViewModel.Requests;

namespace MyHerbalife3.Ordering.Providers.RulesManagement
{
    /// <summary>
    ///     Base class for Rule implementations
    ///     More stuff to be added as we go
    /// </summary>
    public class MyHerbalifeRule
    {
      //  IPurchasingLimitManager purchasingLimitManager 

        protected string Locale
        {
            get { return Thread.CurrentThread.CurrentCulture.ToString(); }
        }

        protected string Country
        {
            get { return Locale.Substring(3); }
        }

        protected DistributorProfileModel DistributorProfileModel
        {
            get
            {
                var member = ((MembershipUser<DistributorProfileModel>) Membership.GetUser());
                return member !=null ? member.Value: null;
            }
        }

        protected static DistributorProfileModel DistributorProfile(string distributorId)
        {
            DistributorProfileLoader loader = new DistributorProfileLoader();
            var profile = loader.Load(new GetDistributorProfileById() { Id = distributorId });
            if (profile != null)
                return profile;
            return null;

        }

        protected HttpSessionState Session
        {
            get { return HttpContext.Current.Session; }
        }

        public bool DiscontinueOnError { get; set; }

        public bool CheckIntegrityOnError { get; set; }
    }
}