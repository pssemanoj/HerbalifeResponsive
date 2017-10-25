using System;
using System.ServiceModel;
using Herbalife.HPS.Tokenization;

namespace MyHerbalife3.Ordering.Web.Ordering.Service.cde
{
    /// <summary>Constructable Singleton wrapper</summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false)]
    public class Tokenizer : ITokenizationService
    {
        /// <summary></summary>
        public ITokenizationService Service
        {
            get { return TokenizerSingleton.Service; }
        }

        #region ITokenizationService wrapper Implementation

        public TokenizedCard Tokenize(TokenizeRequest request)
        {
            TokenizedCard tokenized = null;
            string result = string.Empty;
            if (null != request)
            {
                //var provider = Membership.Provider as MyHlMembershipProvider;
                //if (provider != null &&
                //    !string.IsNullOrEmpty(provider.ValidateToken(request.AuthToken.ToString(), true)))
                {
                    if (string.IsNullOrEmpty(request.CardNumber))
                    {
                        tokenized = new TokenizedCard(string.Empty) { FailureReason = "Null CardNumber Passed" };
                    }
                    else
                    {
                        if (request.CardNumber[1] > 57)
                        {
                            tokenized = new TokenizedCard(request.CardNumber)
                            {
                                FailureReason = "Not a valid Card Number"
                            };
                        }
                        else
                        {
                            tokenized = Service.Tokenize(request);
                        }
                    }
                }
            }
            else
            {
                tokenized = new TokenizedCard(string.Empty) { FailureReason = "Null Request" };
            }
            return tokenized;
        }

        #endregion
    }

    #region TokenizerSingleton Implementation

    internal class TokenizerSingleton : ITokenizationService
    {
        #region Constructor

        private readonly ITokenizationService _underlyingService;

        private TokenizerSingleton()
        {
            _underlyingService = new TokenizationService();
        }

        #endregion

        #region Singleton Implementation

        private static readonly TokenizerSingleton _Tokenizer = new TokenizerSingleton();

        static TokenizerSingleton()
        {
        }

        public static TokenizerSingleton Service
        {
            get { return _Tokenizer; }
        }

        #endregion

        #region ITokenizationService Implementation

        public TokenizedCard Tokenize(TokenizeRequest request)
        {
            try
            {
                return _underlyingService.Tokenize(request);
            }
            catch (Exception ex)
            {
                var result = new TokenizedCard(string.Empty);
                result.FailureReason = ex.Message;
                return result;
            }
        }

        #endregion
    }

    #endregion

    #region Tokenization Service

    public class TokenizationService : ITokenizationService
    {
        public TokenizedCard Tokenize(TokenizeRequest request)
        {
            var tokenizedCard = new TokenizedCard(string.Empty);
            string cardNumber = request.CardNumber.Trim();
            string token = string.Empty;
            TokenizeData(cardNumber, out tokenizedCard);

            return tokenizedCard;
        }

        private bool TokenizeData(string cardNumber, out TokenizedCard token)
        {
            bool tokenized = false;
            token = new TokenizedCard(string.Empty);

            var proxy = new TokenizationServiceProxy();
            try
            {
                proxy.Open();
                Herbalife.HPS.Tokenization.TokenizedCard card = proxy.Tokenize(cardNumber);
                token.Token = card.Token;
                token.FailureReason = card.FailureReason;
                tokenized = true;
            }
            catch (Exception ex)
            {
                //Whatever
                string s = ex.Message;
            }
            finally
            {
                proxy.Close();
            }

            return tokenized;
        }
    }

    #endregion
}