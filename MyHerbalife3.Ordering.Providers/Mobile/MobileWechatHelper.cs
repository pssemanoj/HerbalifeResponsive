using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileWechatHelper
    {
        public bool QueryWechatOrder(string orderNumber)
        {
            GetWechatPaymentQueryOrderResponse response = null;
            using (var proxy = ServiceClientProvider.GetChinaOrderServiceProxy())
            {
                response = proxy.GetWechatPaymentQueryOrder(new GetWechatPaymentQueryOrderRequest1(new GetWechatPaymentQueryOrderRequest_V01
                {
                    OrderNumber = orderNumber,
                })).GetWechatPaymentQueryOrderResult;
            }
            if (null != response)
            {
                var responseV01 = response as GetWechatPaymentQueryOrderResponse_V01;
                if (null != responseV01 && responseV01.Status == ServiceResponseStatusType.Success &&
                    responseV01.ReturnCode == "SUCCESS")
                {
                    return "SUCCESS" == responseV01.TradeState;
                }
            }
            return false;
        }
    }
}