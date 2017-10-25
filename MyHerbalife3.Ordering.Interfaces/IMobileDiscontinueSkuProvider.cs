
using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileDiscontinueSkuProvider
    {
        List<DiscontinuedSkuItemResponseViewModel> GetDiscontinuedSkuRequest(GetDiscontinuedSkuParam param);
    }
}
