using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PromotionResponse_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc.PromotionResponse_V01;

namespace MyHerbalife3.Ordering.Providers.Interface
{
   public interface IChinaPromotionProviderLoader
    {
       PromotionResponse_V01 GetEffectivePromotionList(string local, DateTime? dateTime = null);
    }
}
