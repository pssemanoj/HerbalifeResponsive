using System.Runtime.Serialization;
using System.ServiceModel;

namespace Herbalife.HPS.Tokenization
{
    [ServiceContract(Namespace = "http://www.herbalife.com/2012/WS/HPS", ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign, ConfigurationName = "Herbalife.HPS.Tokenization.ITokenizationService")]
    public interface ITokenizationService
    {
        [OperationContract]
        Herbalife.HPS.Tokenization.TokenizedCard Tokenize(string creditCardNumber);
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