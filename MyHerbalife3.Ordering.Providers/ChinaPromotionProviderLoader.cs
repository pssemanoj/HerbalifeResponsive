using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers
{
    public class ChinaPromotionProviderLoader : IChinaPromotionProviderLoader
    {
        public PromotionResponse_V01 GetEffectivePromotionList(string local, DateTime? dateTime = default(DateTime?))
        {
          return ChinaPromotionProvider.GetEffectivePromotionList(local, dateTime);
        }
    }
}
