using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class BankListResponseVewModel : MobileResponseViewModel
    {
        public List<ServiceProvider.OrderChinaSvc.BankInformation> BankList { get; set; }
    }
    public class BindedCardswithBanklist : MobileResponseViewModel
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string CardType { get; set; }
        public BindedCardViewModel BindedCard { get; set; }
    }

    public class BindedCardswithBanklistResponseVewModel : MobileResponseViewModel
    {
        public List<BindedCardswithBanklist> BankList { get; set; }
        public string PhoneNumber { get; set; }
        public string CNID { get; set; }
    }
}
