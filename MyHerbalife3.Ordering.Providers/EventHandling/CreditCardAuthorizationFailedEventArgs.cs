using System.Collections.Generic;
using HL.Common.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;

namespace MyHerbalife3.Ordering.Providers.EventHandling
{
    public class CreditCardAuthorizationFailedEventArgs : HLEventArgs
    {
        public CreditCardAuthorizationFailedEventArgs(List<FailedCardInfo> failedCards)
        {
            this.FailedCards = failedCards;
        }
        public CreditCardAuthorizationFailedEventArgs(List<FailedCardInfo> failedCards,int noofCraditcardlines)
        {
            this.FailedCards = failedCards;
            this.NoOfCraditcardLines = noofCraditcardlines;
        }
        public List<FailedCardInfo> FailedCards
        {
            get;
            set;
        }
        public int NoOfCraditcardLines
        {
            get;
            set;
        }
    }
}
