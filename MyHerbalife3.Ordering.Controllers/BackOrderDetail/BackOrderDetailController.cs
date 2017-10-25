using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ViewModel.Model.BackOrderDetail;
using MyHerbalife3.Shared.Infrastructure.Interfaces;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;

namespace MyHerbalife3.Ordering.Controllers.BackOrderDetail
{
    public class BackOrderDetailController : ApiController
    {
        internal ILocalizationManager _localization;

        public BackOrderDetailController(ILocalizationManager localizationManager)
        {
            _localization = localizationManager;
        }

        public BackOrderDetailController()
            : this(new LocalizationManager())
        {
            
        }

        [HttpGet]
        public async Task<BackOrderDetailsViewModel> GetInventoryDetails()
        {
            var locale = CultureInfo.CurrentUICulture.Name;

            var details = await CatalogProvider.GetBackOrderDetailsFullWh(locale);

            return details;
        }
    }
}
