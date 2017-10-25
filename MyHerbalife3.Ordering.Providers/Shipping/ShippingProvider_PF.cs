using System;
using HL.Common.Configuration;
using System.ServiceModel;
using HL.Common.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.ShippingSvc;

namespace MyHerbalife3.Ordering.Providers.Shipping
{
    public class ShippingProvider_PF : ShippingProviderBase
    {
        private const string _ShippingInstructions = "marchandise à récupérer au bateau";
        private const string _PickupInstructions = "marchandise à récupérer sous 10 jours après paiement";

        public override string GetShippingInstructionsForDS(MyHLShoppingCart shoppingCart, string distributorID, string locale)
        {
            ShippingInfo currentShippingInfo = shoppingCart.DeliveryInfo;
            string instructions = "";

            if (currentShippingInfo != null && currentShippingInfo.Option == DeliveryOptionType.Shipping)
            {
                instructions = _ShippingInstructions;
            }
            else if (currentShippingInfo != null && currentShippingInfo.Option == DeliveryOptionType.Pickup)
            {
                instructions = _PickupInstructions;
            }

            return instructions;
        }

        public override bool ValidatePostalCode(string country, string state, string city, string postalCode)
        {
            bool isZipValid = true;
            int postCode = 0;

            if (Int32.TryParse(postalCode, out postCode))
            {
                if (postCode >= 98700 & postCode <= 98799)
                {
                    var proxy = ServiceClientProvider.GetShippingServiceProxy();

                    var request = new ValidatePostalCodeRequest_V01();
                    request.Country = "PF";
                    request.PostalCode = postalCode;

                    try
                    {
                        var response = proxy.ValidatePostalCode(new ValidatePostalCodeRequest(request)).ValidatePostalCodeResult as ValidatePostalCodeResponse_V01;

                        //In DB only zips NOT allowed
                        if (response != null && response.IsValidPostalCode)
                        {
                            isZipValid = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error(string.Format("ValidatePostalCode for PF Failed:{0}", ex.Message));
                        isZipValid = false;
                    }
                }
                else
                {
                    isZipValid = false;
                }
            }

            return isZipValid;
        }
        
    }
}
