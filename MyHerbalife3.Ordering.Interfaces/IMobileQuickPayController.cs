using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileQuickPayProvider
    {
        Order_V01 Getorder(OrderViewModel orderViewModel,
                           ref List<ValidationErrorViewModel> errors,
                           OrderTotals_V02 orderTotalsV02, OrderTotals_V01 orderTotalsV01, out decimal amount);

        string GetOrderNumber(decimal amount, string country, string memberId);
    }
}
