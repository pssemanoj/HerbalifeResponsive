using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class CyberSourcePaymentGatewayResponse : PaymentGatewayResponse
    {
        private const string CreditCardNumber = "req_card_number";
        private const string CreditCardType = "req_card_type";
        private const string PaymentGateWayName = "CyberSource";
        private const string Agency = "Agency";
        private const string Decision = "decision";
        private const string Order = "req_reference_number";
        private const string billTransRefNum = "bill_trans_ref_no";
        private const string AuthTransRefNum = "auth_trans_ref_no";
        private const string SignatureField = "signature";
        private const string Accept = "ACCEPT";
        private const string HKSecretKey = "HKSecretKey";
        private const string IDSecretKey = "IDSecretKey";
        private const string SGSecretKey = "SGSecretKey";
        private const string signed_field_names = "signed_field_names";

        private string SecretKey = string.Empty;
        private Dictionary<string, string> _configEntries;

        public CyberSourcePaymentGatewayResponse()
        {
            base.GatewayName = PaymentGateWayName;
            GetSecretKeyForLocale();
        }

        public override bool CanProcess
        {
            get
            {
                //This is coded to follow the simple redirect to / response redirect back from agency. 
                bool canProcess = false;
                if (QueryValues[Agency] == PaymentGateWayName)
                {
                    canProcess = true;
                    if (string.IsNullOrEmpty(PostedValues[Order]))
                    {
                        base.AuthResultMissing = true;
                    }
                    else
                    {
                        OrderNumber = PostedValues[Order];
                    }
                    //Log Response as soon as it comes
                    LogResponseFromGateway(GetType().Name.Replace("Response", string.Empty));

                    // do security check before move forward
                    if (sign(ConvertToDictionary(PostedValues)) != PostedValues[SignatureField])
                    {
                        try
                        {
                            if (PostedValues.AllKeys.Contains(Decision) && !string.IsNullOrEmpty(PostedValues[Decision]) &&
                                PostedValues[Decision].ToUpper() == Accept)
                            {
                                var locale = HLConfigManager.Configurations.Locale;
                                var member = ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value;
                                string distributorId = (null != member) ? member.Id : string.Empty;
                                if (!string.IsNullOrEmpty(distributorId))
                                {
                                    var sessionInfo = SessionInfo.GetSessionInfo(distributorId, locale);
                                    if (sessionInfo != null)
                                    {
                                        sessionInfo.OrderStatus = SubmitOrderStatus.OrderSubmitted;
                                        sessionInfo.OrderNumber = OrderNumber;
                                        HttpContext.Current.Response.Redirect("~/Ordering/Confirm.aspx?OrderNumber=" +
                                                                              sessionInfo.OrderNumber);
                                        HttpContext.Current.Response.End();
                                    }
                                    else
                                    {
                                        LoggerHelper.Error(
                                            string.Format(
                                                "CyberSourcePaymentGatewayResponse, Session is null. Order Number  : {0} ",
                                                OrderNumber));
                                    }
                                }
                                else
                                {
                                    LoggerHelper.Error(
                                        string.Format(
                                            "CyberSourcePaymentGatewayResponse, DistributorID is null. Order Number  : {0} ",
                                            OrderNumber));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string error =
                                string.Format(
                                    "CyberSource PaymentGateway Reprocess - CanProcess - Exception after the sign did not match and trying to analyse for failure or success. Exception - {0}",
                                    ex.Message);
                            LoggerHelper.Error(error);
                        }

                        LogSecurityWarning(PaymentGateWayName);
                        return canProcess;
                    }

                    if (string.IsNullOrEmpty(PostedValues[Decision]))
                    {
                        base.AuthResultMissing = true;
                        IsApproved = false;
                    }
                    else
                    {
                        IsApproved = PostedValues[Decision].ToUpper() == Accept;
                    }
                    AuthorizationCode = !string.IsNullOrEmpty(PostedValues[AuthTransRefNum])
                                            ? PostedValues[AuthTransRefNum]
                                            : PostedValues[billTransRefNum];
                    TransactionCode = PostedValues[billTransRefNum];
                    base.CardNumber = PostedValues[CreditCardNumber];
                    base.CardType = MapCardType(PostedValues[CreditCardType]);
                }

                return canProcess;
            }
        }

        private IssuerAssociationType MapCardType(string gatewayCardType)
        {
            IssuerAssociationType cardType;
            switch (gatewayCardType)
            {
                case "001":
                    cardType = IssuerAssociationType.Visa;
                    break;
                case "002":
                    cardType = IssuerAssociationType.MasterCard;
                    break;
                case "003":
                    cardType = IssuerAssociationType.AmericanExpress;
                    break;
                case "004":
                    cardType = IssuerAssociationType.Discover;
                    break;
                case "005":
                    cardType = IssuerAssociationType.Diners;
                    break;
                case "007":
                    cardType = IssuerAssociationType.JCB;
                    break;
                default:
                    cardType = IssuerAssociationType.Visa;
                    break;
            }
            return cardType;
        }

        private string GetConfigEntry(string entryName)
        {
            _configEntries = new Dictionary<string, string>();
            string configEntries = Settings.GetRequiredAppSetting("CyberSourcePaymentGateway");
            if (!string.IsNullOrEmpty(configEntries))
            {
                var allEntries = configEntries.Split(new[] {';'});
                if (null != allEntries && allEntries.Length > 0)
                {
                    foreach (string entry in allEntries)
                    {
                        var item = entry.Split(new[] {'='});
                        if (null != item && item.Length > 1)
                        {
                            _configEntries.Add(item[0], item[1]);
                        }
                    }
                }
            }

            string entryVal = string.Empty;
            if (!string.IsNullOrEmpty(entryName))
            {
                try
                {
                    entryVal = _configEntries[entryName];
                    if (string.IsNullOrEmpty(entryVal))
                    {
                        throw new ApplicationException(
                            string.Format(
                                "The Configuration Parameter {0} was found in external config, but it had no value",
                                entryName));
                    }
                }
                catch (Exception ex)
                {
                    string error =
                        string.Format(
                            "Missing Gateway information in External Config for: {0}, parameter: {1} Error: {2}",
                            Thread.CurrentThread.CurrentCulture.Name, entryName, ex.Message);
                    LoggerHelper.Error(error);
                    throw;
                }
            }

            return entryVal;
        }

        private string sign(IDictionary<string, string> paramsArray)
        {
            string secretKey = GetConfigEntry(SecretKey);
            return sign(buildDataToSign(paramsArray), secretKey);
        }

        private string sign(String data, String secretKey)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }
            var encoding = new UTF8Encoding();
            var keyByte = encoding.GetBytes(secretKey);

            var hmacsha256 = new HMACSHA256(keyByte);
            var messageBytes = encoding.GetBytes(data);
            return Convert.ToBase64String(hmacsha256.ComputeHash(messageBytes));
        }

        private string buildDataToSign(IDictionary<string, string> paramsArray)
        {
            IList<string> dataToSign = new List<string>();
            try
            {
                if (paramsArray != null && paramsArray.ContainsKey(signed_field_names))
                {
                    var signedFieldNames = paramsArray[signed_field_names].Split(',');
                    dataToSign = (from signedFieldName in signedFieldNames
                                  where paramsArray.Any(x => x.Key.Contains(signedFieldName))
                                  select signedFieldName + "=" + paramsArray[signedFieldName]).ToList();
                }
                return commaSeparate(dataToSign);
            }
            catch (Exception ex)
            {
                string error = string.Format("Missing query information from cybersource: {0}, Error: {1}",
                                             ConstructQueryString(PostedValues), ex.Message);
                LoggerHelper.Error(error);
            }
            return string.Empty;
        }

        private string commaSeparate(IList<string> dataToSign)
        {
            return String.Join(",", dataToSign.ToArray());
        }

        private IDictionary<string, string> ConvertToDictionary(NameValueCollection source)
        {
            return
                source.Cast<string>()
                      .Where(s => s != null)
                      .Select(s => new {Key = s, Value = source[s]})
                      .ToDictionary(p => p.Key, p => p.Value);
        }

        private void GetSecretKeyForLocale()
        {
            switch (HLConfigManager.Configurations.Locale.ToUpper())
            {
                case "ID-ID":
                    SecretKey = IDSecretKey;
                    break;
                case "ZH-HK":
                    SecretKey = HKSecretKey;
                    break;
                case "EN-SG":
                    SecretKey = SGSecretKey;
                    break;
            }
        }

        public String ConstructQueryString(NameValueCollection parameters)
        {
            return String.Join(";", (from string name in parameters
                                     let element = name
                                     where element != null
                                     let value = parameters[element]
                                     where value != null
                                     select String.Concat(name, ":=", HttpUtility.UrlDecode(value))).ToArray()
                );
        }
    }
}