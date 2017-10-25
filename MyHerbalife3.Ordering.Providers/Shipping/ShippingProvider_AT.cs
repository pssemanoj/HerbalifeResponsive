using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Common.ValueObjects;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_AT : ShippingProviderBase
    {
        private const decimal MaxTotalAmount = 2000.00M;
        private const decimal MinTotalAmount = 1.00M;
        public override bool ValidateTotalAmountForPaymentOption(string paymentOption, decimal totalAmount)
        {
            var isValid = true;

            //Restriccion for Sofort min and max amount 
            if(paymentOption == "PaymentGateway"){

                if (totalAmount > MaxTotalAmount || (totalAmount < MinTotalAmount && totalAmount > 0M ) )
                {
                    isValid = false;
                }

            }

            return isValid;
        }
    }
}
