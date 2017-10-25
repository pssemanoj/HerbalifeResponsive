using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Rules.APF.Global;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Web.Test.Rules
{
    [TestClass]
    public class APFRulesTest
    {
        DistributorOrderingProfile distributor;

        [TestInitialize]
        public void Init()
        {
            distributor = new DistributorOrderingProfile();

            List<DistributorVolume_V01> vol = new List<DistributorVolume_V01> { 
                
                new DistributorVolume_V01()
                {
                    VolumeDate = DateTime.Now,
                    Volume = 200,
                    VolumeMonth = "201509"
                }
            };

            distributor.DistributorVolumes = vol;

        }

        [TestMethod]
        public void APFExempt_distributor_null()
        {
            
            distributor = null;
            decimal volume = 0;
            bool result;

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, 0);
            Assert.IsFalse(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            object[] args = new object[2] { distributor, volume };
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));
            Assert.IsFalse(result);
            
        }

        [TestMethod]
        public void APFExempt_distributor_Qualified()
        {
            bool result;
            decimal volume = 0;
            result = APFDueProvider.IsAPFExemptOn200VP(distributor, 0);
            Assert.IsTrue(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void APFExempt_distributor_Qualified_Wf_Additional_Volume()
        {
            bool result;
            decimal volume = 50;
            distributor.DistributorVolumes[0].Volume = 150;

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, volume);
            Assert.IsTrue(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));

            Assert.IsTrue(result);

        }

        [TestMethod]
        public void APFExempt_distributor_Qualified_wfDecimal()
        {
            distributor.DistributorVolumes[0].Volume = Decimal.Parse("200.1");
            bool result;
            decimal volume = 0;

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, volume);
            Assert.IsTrue(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void APFExempt_distributor_notQualified()
        {
            distributor.DistributorVolumes[0].Volume = 199;
            bool result;
            decimal volume = 0;

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, volume);
            Assert.IsTrue(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));

            Assert.IsFalse(result);

        }

        [TestMethod]
        public void APFExempt_distributor_notQualified_Wf_Additional_Volume()
        {
            distributor.DistributorVolumes[0].Volume = 150;
            bool result;
            decimal volume = decimal.Parse("49.999");

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, volume);
            Assert.IsFalse(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));

            Assert.IsFalse(result);

        }

        [TestMethod]
        public void APFExempt_distributor_notQualified_wfDecimal()
        {
            distributor.DistributorVolumes[0].Volume = Decimal.Parse("199.99");
            bool result;
            decimal volume = 0;

            result = APFDueProvider.IsAPFExemptOn200VP(distributor, volume);
            Assert.IsFalse(result);

            PrivateObject apf = new PrivateObject(typeof(APFRules));
            result = Convert.ToBoolean(apf.Invoke("IsAPFExempt", distributor, volume));

            Assert.IsFalse(result);

        }
    }
}