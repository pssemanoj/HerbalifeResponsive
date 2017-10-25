using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileQuickPayProvider : IMobileQuickPayProvider
    {
        public Order_V01 Getorder(OrderViewModel orderViewModel,
                                  ref List<ValidationErrorViewModel> errors,
                                  OrderTotals_V02 orderTotalsV02, OrderTotals_V01 orderTotalsV01, out decimal amount)
        {
            return MobileOrderProvider.ModelConverter.ConvertOrderViewModelToOrderV01(orderViewModel,
                                                                                      ref errors, orderTotalsV02,
                                                                                      orderTotalsV01,
                                                                                      out amount);
        }

        public string GetOrderNumber(decimal amount, string country, string memberId)
        {
            var request = new GenerateOrderNumberRequest_V01
            {
                Amount = amount,
                Country = country,
                DistributorID = memberId
            };
            var response = OrderProvider.GenerateOrderNumber(request);
            if (null != response)
            {
                return response.OrderID;
            }
            return string.Empty;
        }
    }
}
