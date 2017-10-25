using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    [ServiceContract(Namespace = "http://www.herbalife.com/2012/WS/HPS", ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign, ConfigurationName = "MyHerbalife3.Ordering.Providers.Payments.ITokenizationService")]
    public interface ITokenizationService
    {
        [OperationContract]
        TokenizedCard Tokenize(string creditCardNumber);
    }

    [DataContract(Name = "TokenizedCard", Namespace = "http://www.herbalife.com/2012/WS/HPS")]
    public class TokenizedCard
    {
        [DataMember]
        public string FailureReason { get; set; }
        [DataMember]
        public string Token { get; set; }
        [DataMember]
        public bool Tokenized { get; set; }
    }

    public class TokenizationServiceProxy : ClientBase<ITokenizationService>, ITokenizationService
    {
        public TokenizationServiceProxy()
        {
        }

        public TokenizedCard Tokenize(string creditCardNumber)
        {
            return Channel.Tokenize(creditCardNumber);
        }
    }
}
