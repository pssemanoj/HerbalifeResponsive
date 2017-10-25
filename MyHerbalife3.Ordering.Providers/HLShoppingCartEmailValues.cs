using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers
{
    public class HLShoppingCartEmailValues
    {
        public string CurrentMonthVolume { get; set; }

        public string RemainingVolume { get; set; }

        public decimal DistributorSubTotal { get; set; }

        public string DistributorSubTotalFormatted { get; set; }
    }
}
