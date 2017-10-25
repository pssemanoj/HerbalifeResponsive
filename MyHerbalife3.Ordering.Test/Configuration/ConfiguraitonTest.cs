using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;

namespace MyHerbalife3.Ordering.Test.Configuration
{
    [TestClass]
    public class ConfiguraitonTest
    {
        [TestMethod]
        public void ReadPaymentProperties()
        {
            var paymentsConfiguration = new PaymentsConfiguration();
            var propertyInfos = paymentsConfiguration.GetType().GetProperties(BindingFlags.Public| BindingFlags.Instance);

            foreach (var propertyInfo in propertyInfos)
            {
                var customAttribute =
                    propertyInfo.GetCustomAttributes(typeof (ConfigurationPropertyAttribute), false)
                                .Cast<ConfigurationPropertyAttribute>().ToList();

                if (customAttribute.Any())
                {
                    var configurationProperty = customAttribute.Single();
                    var f = configurationProperty.Name;
                }
            }
        }
    }
}
