#region

using System;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileWechatProvider
    {
        WechatPrepayIdResponseViewModel GetPrepayId(WechatPrepayIdViewModel request, string orderNumber, decimal amount);
        int InsertToPaymentGatewayRecord(OrderViewModel order, Guid authToken, ref decimal amount);
    }
}