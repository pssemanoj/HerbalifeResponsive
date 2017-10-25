using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class QuickPayPaymentViewModel : CardPaymentViewModel
    {
     
        public bool BindCard { get; set; }
      
        public string CardHolderId { get; set; }
       
        public string CardHolderType { get; set; }
       
        public bool IsDebitCard { get; set; }
      
        public string MobilePhoneNumber { get; set; }
       
        public string MobilePin { get; set; }
     
        public string StorablePAN { get; set; }
     
        public string Token { get; set; }
    }
}
