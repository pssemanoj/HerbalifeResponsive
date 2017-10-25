using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using NSubstitute;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.Web.Test
{
     [TestClass]
    public class APFExemptOnGracePeriodTest
    {
         [TestInitialize]
         public void ChinaCulture()
         {
             System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-CN");
         }

        [TestMethod]
        public void IsAPFExemptOnGracePeriod_Test()
        {
            var distributorProfiles = new DistributorOrderingProfile();
            distributorProfiles = GetDistributorProfile();

            GetDistributorVolumeResponse_V01 Volume;
            var apfExemptOnGracePeriod = Substitute.For<IChinaDistributorProviderLoader>();
            apfExemptOnGracePeriod.getVolumePoints().Returns(getVolumePoints(out Volume));

            decimal accumulatedVP = Volume.VolumePoints[0].Volume;
            if (distributorProfiles.ApfDueDate <= DateTime.UtcNow.Date)
                Assert.AreEqual(accumulatedVP, 200);

        }

        public GetDistributorVolumeResponse_V01 getVolumePoints(out GetDistributorVolumeResponse_V01 results)
        {
            var result = new GetDistributorVolumeResponse_V01();
            var volum = new List<DistributorVolume_V01>();
            volum.Add(new DistributorVolume_V01() { Volume = 200 });
            result.VolumePoints = volum;
            return results = result;

        }

        public DistributorOrderingProfile GetDistributorProfile()
        {
            DistributorOrderingProfile result = new DistributorOrderingProfile();
            result.Id = "CN175803";
            result.ApfDueDate = DateTime.Now.AddYears(-1);

            return result;

        }

    }
}
