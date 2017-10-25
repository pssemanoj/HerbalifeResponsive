using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers.OrderImportBtWS;
using MyHerbalife3.Ordering.Providers.Shipping;
using System;
using System.Text;

namespace MyHerbalife3.Ordering.Web.Test
{
    [TestClass]
    public class CityLength
    {
        [TestMethod]
        public void LenghtCity_JP()
        {
            string country = "JP";
            var shipment = new Shipment();


            shipment.Address = new Address();
            shipment.Address.City = "マ建や部屋ション名や部屋番号建や部屋建や建や部屋";


            var provider = ShippingProvider.GetShippingProvider(country);
            if (provider != null)
            {
                provider.FormatAddressForHMS(shipment.Address);
            }
            Encoding enc_utf8 = new UTF8Encoding(false, true);

            Assert.IsTrue(enc_utf8.GetByteCount(shipment.Address.City) <= 60);
        }

        [TestMethod]
        public void LenghtCity_TH()
        {
            string country = "TH";
            var shipment = new Shipment();


            shipment.Address = new Address();
            shipment.Address.City = "ส่งต่อ บ.เศรษฐีอีสานขนส่ง";


            var provider = ShippingProvider.GetShippingProvider(country);
            if (provider != null)
            {
                provider.FormatAddressForHMS(shipment.Address);
            }
            Encoding enc_utf8 = new UTF8Encoding(false, true);

            Assert.IsTrue(enc_utf8.GetByteCount(shipment.Address.City) <= 60);
        }
    }
}
