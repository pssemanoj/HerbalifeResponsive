using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Shipping.ValueObjects;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.Interfaces;
using MyHerbalife3.Ordering.Providers.Shipping;

namespace MyHerbalife3.Ordering.Test.Providers
{
    [TestClass]
    public class ShippingProviderTests
    {
        /// <summary>
        /// Test the method in a normal circunstance
        /// </summary>
        [TestMethod]
        public void GetCountiesForCity_PE_HappyPath()
        {
            //arrange
            IShippingProvider provider = new ShippingProviderBase();
            var country = "PE";
            var state = "ancash";
            var city = "bolognesi";
            var county = "CAJACAY";

            //act
            var response = provider.GetCountiesForCity(country, state, city);

            //assert
            Assert.IsTrue(response.Contains("CAJACAY", new StringComparerInList()), 
                string.Format("County '{0}' was not found in country '{1}', state '{2}', city '{3}'", county, country, state, city));
        }

        /// <summary>
        /// Test case when the method does not find the city
        /// </summary>
        [TestMethod]
        public void GetCountiesForCity_PE_NoCountiesFound()
        {
            //arrange
            IShippingProvider provider = new ShippingProviderBase();
            var country = "WrongCountry";
            var state = "WrongState";
            var city = "WrongCity";

            //act
            var response = provider.GetCountiesForCity(country, state, city);

            //assert
            Assert.IsNotNull(response, "The returned list must be initialized and it returned null.");
            Assert.IsTrue(response.Count == 0, "The returned list returned values when it should have zero values.");
        }

        [TestMethod]
        public void GetZipsForCounty_PE_HappyPath()
        {
            //arrange
            IShippingProvider provider = new ShippingProviderBase();
            var country = "PE";
            var state = "ancash";
            var city = "bolognesi";
            var county = "CAJACAY";

            //act
            var response = provider.GetZipsForCounty(country, state, city, county);

            //assert
            Assert.IsTrue(response.Count > 0,
                string.Format("County '{0}' was not found in country '{1}', state '{2}', city '{3}', county '{4}'", county, country, state, city, county));
        }

        [TestMethod]
        public void GetFreightCodeAndWarehouse_MY_HappyPath()
        {
            var ci = new CultureInfo("en-MY");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            IShippingProvider provider = new ShippingProvider_MY();            
            string[] response;
            var states = new[]
            {
                "Sarawak", 
                "Sabah", 
                "Labuan", 
                "Labuan Wilayah Persekutuan"
            };
            var statesDefult = new[]
            {
                "Johor", 
                "Kedah", 
                "Kelantan", 
                 "Kuala Lumpur", 
                "Kuala Lumpur Wilayah Persekutuan",
                "Langkawi", 
                "Melaka", 
                "Negeri Sembilan", 
                "Pahang", 
                "Perak", 
                "Perlis", 
                "Pulau Pinang", 
                "Putrajaya", 
                "Putrajaya Wilayah Persekutuan"
            };
            var addressMY = new ShippingAddress_V01
            {
                Address = new Address_V01 { StateProvinceTerritory = ""}                
            };

            for (var i = 0; i < 4; i++)
            {
                addressMY.Address.StateProvinceTerritory = states[i];
                response = provider.GetFreightCodeAndWarehouse(addressMY);
                var errorMsg = string.Format("The state '{0}' must have assigned the freight code '{1}' and warehouse code '{2}'", states[i], "MYE", "K2");
                Assert.AreEqual(response[0], "MYE", errorMsg);
                Assert.AreEqual(response[1], "K2", errorMsg);
            }

            for (var i = 0; i < 14; i++)
            {
                addressMY.Address.StateProvinceTerritory = statesDefult[i];
                response = provider.GetFreightCodeAndWarehouse(addressMY);
                var errorMsg = string.Format("The state '{0}' must have assigned the freight code '{1}' and warehouse code '{2}'", statesDefult[i], "MYW", "K9");
                Assert.AreEqual(response[0], "MYW", errorMsg);
                Assert.AreEqual(response[1], "K9", errorMsg);
            }

            ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }

    public class StringComparerInList : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Compare(x, y, true) == 0;
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
