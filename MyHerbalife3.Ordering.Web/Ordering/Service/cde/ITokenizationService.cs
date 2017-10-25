using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MyHerbalife3.Ordering.Web.Ordering.Service.cde
{
    /// <summary>Service Contract for HPS Tokenizer</summary>
    [ServiceContract(Namespace = "http://www.herbalife.com/2013/WS/HPS")]
    public interface ITokenizationService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "Tokenize")]
        TokenizedCard Tokenize(TokenizeRequest request);
    }

    [DataContract(Namespace = "http://www.herbalife.com/2013/WS/HPS")]
    public class TokenizeRequest
    {
        [DataMember]
        public Guid AuthToken { get; set; }
        [DataMember]
        public string CardNumber { get; set; }
    }

    [DataContract(Namespace = "http://www.herbalife.com/2013/WS/HPS", Name = "TokenizeResponse")]
    public class TokenizedCard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="tokenized"></param>
        public TokenizedCard(string token)
        {
            Token = token;
        }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public string Token
        {
            get;
            set;
        }

        [DataMember]
        public string FailureReason
        {
            get;
            set;
        }
    }
}