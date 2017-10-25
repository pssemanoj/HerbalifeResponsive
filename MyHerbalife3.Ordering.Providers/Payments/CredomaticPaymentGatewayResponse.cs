using System.Security.Cryptography;
using System.Text;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CredomaticPaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string GateWay = "Agency";
        private const string Order = "orderid";
        private const string Amount = "amount";
        private const string Response = "response";
        private const string Result = "response_code";
        private const string AuthResultCode = "authcode";
        private const string TransactionId = "transactionid";
        private const string AVSResponse = "avsresponse";
        private const string CVVResponse = "cvvresponse";
        private const string Time = "time";
        private const string HashValue = "hash";
        private const string Visa = "VI";
        private const string MasterCard = "MC";
        private const string AmericanExpress = "AX";
        private const string ResultCardType = "tc";

        private const string PaymentGatewayName = "Credomatic";

        protected PaymentsConfiguration _config = HLConfigManager.Configurations.PaymentsConfiguration;

        public CredomaticPaymentGatewayResponse()
        {
            base.GatewayName = this.GatewayName;
        }

        public override bool CanProcess
        {
            get
            {
                bool canProcess = false;
                if (QueryValues[GateWay] == PaymentGatewayName)
                {
                    canProcess = true;
                    OrderNumber = QueryValues[Order];
                    string result = QueryValues[Result];
                    string credoHash = QueryValues[HashValue];
                    string cardType = QueryValues[ResultCardType];
                    string encryptionKey = _config.PaymentGatewayEncryptionKey;
                    string calculatedHash = GenerateResponseHash(OrderNumber, QueryValues[Amount], QueryValues[Response],
                                                                 QueryValues[TransactionId],
                                                                 QueryValues[AVSResponse], QueryValues[CVVResponse],
                                                                 QueryValues[Time], encryptionKey);

                    //Validate this is not a spoof
                    if (credoHash == calculatedHash)
                    {
                        IsApproved = result == "100";
                        AuthorizationCode = QueryValues[AuthResultCode];
                        TransactionCode = QueryValues[TransactionId];
                        GatewayName = PaymentGatewayName;
                        switch (cardType)
                        {
                            case Visa:
                                {
                                    CardType = IssuerAssociationType.Visa;
                                    break;
                                }
                            case MasterCard:
                                {
                                    CardType = IssuerAssociationType.MasterCard;
                                    break;
                                }
                            case AmericanExpress:
                                {
                                    CardType = IssuerAssociationType.AmericanExpress;
                                    break;
                                }
                            default:
                                {
                                    CardType = IssuerAssociationType.Visa;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        LogSecurityWarning(this.GatewayName);
                    }
                }
                else
                {
                    base.AuthResultMissing = true;
                }
                return canProcess;
            }
        }

        private string GenerateResponseHash(string orderId,
                                            string amount,
                                            string response,
                                            string transactionId,
                                            string avsResposne,
                                            string cvvResponse,
                                            string timeStamp,
                                            string encryptionKey)
        {
            string preHash = string.Concat(orderId, "|", amount, "|", response, "|", transactionId, "|", avsResposne,
                                           "|", cvvResponse, "|", timeStamp, "|", encryptionKey);
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.Default.GetBytes(preHash);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2").ToLower());
            }
            return sb.ToString();
        }
    }
}