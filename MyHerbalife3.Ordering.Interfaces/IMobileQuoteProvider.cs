#region

using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileQuoteProvider
    {
        QuoteResponseViewModel Quote(OrderViewModel order, ref List<ValidationErrorViewModel> validationErrors );

    }
}