#region

using System;
using System.Collections.Generic;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileOrderProvider
    {
        List<OrderViewModel> GetOrders(OrderSearchParameter searchParameter);
        OrderViewModel GetOrder(Guid orderId, string memberId, string locale, bool suppressRules = false);
        OrderViewModel Save(OrderViewModel orderViewModel, ref List<ValidationErrorViewModel> errors);
        OrderViewModel Submit(OrderViewModel orderViewModel, ref List<ValidationErrorViewModel> errors, Guid authToken, string loginDistributorId = null);
        bool DeleteOrder(Guid orderId);
        OrderViewModel CancelOrder(string memberId, string orderNumber, string locale, string paymentMethodName,ref bool isApproved, Guid id);
        OrderViewModel GetOrderByOrderNumber(string orderNumber, string memberId, string locale, ref int cartId);
    }
}