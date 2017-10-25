using System.Collections.Generic;
using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;

namespace MyHerbalife3.Ordering.SharedProviders.ViewModel
{
    public class DistributorBizworksProfile
    {
        public HL.Common.ValueObjects.AddressCollection Addresses { get; set; }
        public string Id { get; set; }
        public Address MailingAddress { get; set; }
        public List<EmailAddress> EmailAddresses { get; set; }
        public Name_V01 EnglishName { get; set; }
        public LevelType Level { get; set; }
        public List<DistributorVolume_V01> DistributorVolumes { get; set; }
    }
}
