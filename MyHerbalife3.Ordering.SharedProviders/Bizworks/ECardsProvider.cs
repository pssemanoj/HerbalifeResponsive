using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.SharedProviders.Bizworks
{
    [DataObject]
    public class ECardsProvider
    {
        public const string TEMPLATE_IMG_SMALL = "/Bizworks/Images/myecards/Ecard_{0}_sm.jpg";
        public const string TEMPLATE_IMG_MEDUIM = "/Bizworks/Images/myecards/Ecard_{0}_md.jpg";

        #region Email Template Management

        /// <summary>
        /// Gets the email templates for a given locale.
        /// </summary>
        /// <param name="locale">The locale to filter by.</param>
        /// <returns>A list of templates for the given locale</returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IEnumerable<EmailTemplate_V01> GetEmailTemplates(string locale)
        {
            return SimpleCache.Retrieve(
                key =>
                {
                    using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                    {
                        var response =
                            proxy.GetEmailTemplates(new GetEmailTemplatesRequest1(new GetEmailTemplatesByLocaleRequest { Locale = locale })).GetEmailTemplatesResult as
                            GetEmailTemplatesResponse_V01;

                        if (response != null && response.Status == ServiceResponseStatusType.Success)
                        {
                            return response.EmailTemplates.Where(t => t.Locale == locale);
                        }
                        return new List<EmailTemplate_V01>();
                    }
                }
                , locale
                , TimeSpan.FromMinutes(60));
        }

        /// <summary>
        /// Gets the email templates for a given locale and category.
        /// </summary>
        /// <param name="locale">The locale to filter by.</param>
        /// <param name="category">The category to fliter by.</param>
        /// <returns>A list of templates for a given locale and category</returns>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IEnumerable<EmailTemplate_V01> GetECardTemplates(string locale, string category)
        {
            return
                GetECardTemplates(locale)
                .Where(t => (String.IsNullOrEmpty(category) || t.Category == category));
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static IEnumerable<EmailTemplate_V01> GetECardTemplates(string locale)
        {
            return
                GetEmailTemplates(locale)
                .Where(t => t.Usage == HL.DistributorCRM.ValueObjects.Email.EmailTemplateUsageType.ECARD.Key);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static EmailTemplate_V01 GetEmailTemplate(string locale, HL.DistributorCRM.ValueObjects.Email.EmailTemplateUsageType usage)
        {
            return GetEmailTemplates(locale).FirstOrDefault(t => t.Locale == locale && t.Usage == usage.Key);
        }

        public static bool SaveDefaultEcardSettings(string distributorID, Dictionary<int, string> defaultTokens)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response = proxy.SaveDefaultMailingTokens(new SaveDefaultMailingTokensRequest1(
                                        new SaveDefaultMailingTokensRequest_V01
                                        {
                                            DistributorID = distributorID,
                                            MailingTokens = defaultTokens
                                        }));

                    return (response.SaveDefaultMailingTokensResult.Status == ServiceResponseStatusType.Success);
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("SaveDefaultEcardSettings DistributorID:[{0}] failed", distributorID), ex));
                return false;
            }
        }

        public static void GetDefaultEcardSettings(string distributorID, out Dictionary<int, string> defaultValues, out Dictionary<int, string> tokenReference)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response = proxy.GetDefaultMailingTokens(new GetDefaultMailingTokensRequest1(new GetDefaultMailingTokensRequest() { DistributorID = distributorID })).GetDefaultMailingTokensResult as GetDefaultMailingTokensResponse_V01;
                    defaultValues = response.DefaultTokenValues;
                    tokenReference = response.TokenReference;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("SaveDefaultEcardSettings DistributorID:[{0}] failed", distributorID), ex));
                defaultValues = new Dictionary<int, string>();
                tokenReference = new Dictionary<int, string>();
            }
        }

        public static Dictionary<int, string> GetApplicableTokenValues(string distributorID, EmailTemplate_V01 template)
        {
            var result = new Dictionary<int, string>();
            Dictionary<int, string> defaultValues, tokenReference;
            GetDefaultEcardSettings(distributorID, out defaultValues, out tokenReference);
            if (defaultValues == null) { return result; }

            foreach (var item in defaultValues)
            {
                if (template.Tokens.Any(kvp => ((MailingToken_V01)kvp.Value).ID == item.Key))
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
        }

        #endregion

        #region Compliance Settings

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public static bool SaveComplianceSettings(EmailComplianceSetting emailComplianceSetting)
        {
            using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var response =
                    proxy.SaveEmailComplianceSettings(new SaveEmailComplianceSettingsRequest1(new SaveEmailComplianceSettingsRequest_V01 { Settings = emailComplianceSetting }));
                return (response.SaveEmailComplianceSettingsResult.Status == ServiceResponseStatusType.Success);
            }
        }

        public static bool? HasComplianceSettings(string distributorID)
        {
            using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var response =
                    proxy.GetEmailComplianceSettings(new GetEmailComplianceSettingsRequest1(new GetEmailComplianceSettingsByDistributorID() { DistributorID = distributorID })).GetEmailComplianceSettingsResult as
                    GetEmailComplianceSettingsResponse_V01;

                if (response.Status == ServiceResponseStatusType.Success)
                {
                    return response.EmailComplianceSettings != null;
                }
                else
                {
                    return null;
                }
            }
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static EmailComplianceSetting GetComplianceSettings(DistributorBizworksProfile distributor)
        {
            EmailComplianceSetting result = null;
            using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var response =
                    proxy.GetEmailComplianceSettings(new GetEmailComplianceSettingsRequest1(new GetEmailComplianceSettingsByDistributorID { DistributorID = distributor.Id })).GetEmailComplianceSettingsResult as
                    GetEmailComplianceSettingsResponse_V01;

                if (response.Status == ServiceResponseStatusType.Success && response.EmailComplianceSettings != null)
                {
                    result = response.EmailComplianceSettings;
                }
                else
                {
                    MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.Address address = (distributor.MailingAddress == null) ? new MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.Address() : distributor.MailingAddress;
                    MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.EmailAddress email = distributor.EmailAddresses.FirstOrDefault(e => e.IsPrimary) ??
                                         new MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.EmailAddress() { Address = "", IsPrimary = false };

                    result = new EmailComplianceSetting
                    {
                        City = address.City,
                        Country = address.Country,
                        DistributorID = distributor.Id,
                        FromEmail = email.Address,
                        FromName =
                            String.Format("{0} {1}", distributor.EnglishName.First,
                                          distributor.EnglishName.Last),
                        State = address.StateProvinceTerritory,
                        StreetAddress1 = address.Line1,
                        StreetAddress2 = address.Line2,
                        Zip = address.PostalCode
                    };
                }
            }
            return result;
        }

        #endregion

        #region optout

        public static GetOptOutResponse_V01 GetOptOut(int mailingID, int uniqueEmailID)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.GetOptOut(new GetOptOutRequest1(new GetOptOutRequest_V01 { MailingID = mailingID, UniqueEmailID = uniqueEmailID })).GetOptOutResult as
                        GetOptOutResponse_V01;
                    return response;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(
                                                               String.Format("GetOptOut mailingid:{0} email master ID:{1}", mailingID, uniqueEmailID), ex));
                return null;
            }
        }

        public static bool SaveOptOut(int mailingID, int uniqueEmailID, bool optOutFromSender, bool optOutFromAll, string reactivationUrl, string locale)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response = proxy.SaveOptOut(new SaveOptOutRequest1(new SaveOptOutRequest_V01
                    {
                        MailingID = mailingID,
                        OptOutFromAll = optOutFromAll,
                        OptOutFromSender = optOutFromSender,
                        UniqueEmailID = uniqueEmailID,
                        OptInUrlTemplate = reactivationUrl,
                        Locale = locale
                    }));
                    return (response != null && response.SaveOptOutResult.Status == ServiceResponseStatusType.Success);
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(
                                                               String.Format("SaveOptOut mailingid:{0} email master ID:{1} sender:{2} all:{3}", mailingID,
                                                                             uniqueEmailID, optOutFromSender, optOutFromAll), ex));
                return false;
            }
        }

        public static bool SendCancelOptOutMessage(string locale, string distributorID, string emailAddress, string resubscribeUrlTemplate)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response = proxy.SendCancelOptOutMessage(new SendCancelOptOutMessageRequest1(
                                        new SendCancelOptOutMessageRequest_V01
                                        {
                                            DistributorID = distributorID,
                                            EmailAddress = emailAddress,
                                            Locale = locale,
                                            ResubscribeUrlTemplate = resubscribeUrlTemplate
                                        }));

                    return (response.SendCancelOptOutMessageResult.Status == ServiceResponseStatusType.Success);
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("SendCancelOptOutMessage DistributorID:[{0}] failed", distributorID), ex));
                return false;
            }
        }

        #endregion

        #region Mailing

        public Mailing_V01 GetMailing(string distributorID, int mailingID)
        {
            using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                //TODO: get mailing
                throw new NotImplementedException("TODO");
            }
        }

        public static SaveMailingResponse_V01 SaveMailing(string distributorID, Mailing_V01 mailing)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.SaveMailing(new SaveMailingRequest1(new SaveMailingRequest_V01 { DistributorID = distributorID, Mailing = mailing })).SaveMailingResult
                        as SaveMailingResponse_V01;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        SimpleCache.Expire(typeof(DataTable), mailingHistoryCacheKey(distributorID));
                    }
                    return response;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("SaveMailing distributorID:{0} failed", distributorID), ex));
                return null;
            }
        }

        /// <summary>
        /// Gets the mailing history.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <returns>A data table containing summary delivery report for all mailings of a distributor</returns>
        public static DataTable GetMailingHistory(string distributorID)
        {
            try
            {
                return SimpleCache.Retrieve(f =>
                {
                    using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                    {
                        var response =
                            proxy.GetMailingHistory(new GetMailingHistoryRequest1(new GetMailingHistoryRequest { DistributorID = distributorID })).GetMailingHistoryResult as GetMailingHistoryResponse;
                        //return response.Data;
                        return new DataTable();
                    }
                }
                        , mailingHistoryCacheKey(distributorID)
                        , TimeSpan.FromMinutes(1));
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("GetMailingHistory distributorID:{0} failed", distributorID), ex));
                return null;
            }
        }

        public static DataTable GetMailingHistoryDetail(string distributorID, int mailingID)
        {
            try
            {
                return SimpleCache.Retrieve(
                    h =>
                    {
                        using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                        {
                            var response =
                                proxy.GetMailingHistoryDetail(new GetMailingHistoryDetailRequest1(new GetMailingHistoryDetailRequest_V01 { DistributorID = distributorID, MailingID = mailingID })).GetMailingHistoryDetailResult as GetMailingHistoryDetailResponse;
                            //return response.Data;
                            return new DataTable();
                        }
                    },
                   mailingHistoryDetailCacheKey(distributorID, mailingID),
                    TimeSpan.FromMinutes(1)
                    );
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(
                                                               String.Format("GetMailingHistoryDetail distributorID:{0} mailingid:{1} failed", distributorID,
                                                                             mailingID), ex));
                return null;
            }
        }

        public static string GetEmailBounceStatus(string emailId)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response = (GetExactTargetSubscriberResponse_V01)
                                   proxy.GetExactTargetSubscriber(new GetExactTargetSubscriberRequest1(new GetExactTargetSubscriberRequest_V01
                                   {
                                       SubscriberKey = emailId
                                   })).GetExactTargetSubscriberResult;
                    return response.ExactTargetSubscriber == null ? null : response.ExactTargetSubscriber.Status.ToString();
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("Error getting ExactTargetSubscriberStatus for:{0}", emailId), ex));
                return null;
            }
        }

        public static bool DeleteMailing(string distributorID, params int[] mailingIDs)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.SaveMailing(new SaveMailingRequest1(new SaveMailingStatusRequest
                        {
                            DistributorID = distributorID,
                            MailingIDList = mailingIDs.ToList(),
                            MailingStatus = HL.DistributorCRM.ValueObjects.Email.MailingStatusType.HIDDEN.Key
                        })).SaveMailingResult;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        // expire cache
                        SimpleCache.Expire(typeof(DataTable), mailingHistoryCacheKey(distributorID));
                        foreach (int mailingID in mailingIDs)
                        {
                            SimpleCache.Expire(typeof(DataTable), mailingHistoryDetailCacheKey(distributorID, mailingID));
                        }
                        return true;
                    }
                    return false;

                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("DeleteMailing distributorID:{0} mailingid count:{1} failed", distributorID, mailingIDs == null ? 0 : mailingIDs.Length), ex));
                return false;
            }
        }

        public static bool RescheduleMailing(string distributorID, int mailingID, DateTime? newSendDateUTC)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.SaveMailing(new SaveMailingRequest1(new SaveMailingScheduleRequest
                        {
                            DistributorID = distributorID,
                            MailingID = mailingID,
                            NewSendDate = newSendDateUTC
                        })).SaveMailingResult;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        // expire cache
                        SimpleCache.Expire(typeof(DataTable), mailingHistoryCacheKey(distributorID));
                        SimpleCache.Expire(typeof(DataTable), mailingHistoryDetailCacheKey(distributorID, mailingID));
                        return true;
                    }
                    return false;

                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("RescheduleMailing distributorID:{0} mailingid:{1} newDate:{2} failed",
                                                                                       distributorID,
                                                                                       mailingID,
                                                                                       newSendDateUTC.HasValue ? newSendDateUTC.Value : DateTime.MaxValue), ex));
                return false;
            }
        }

        public static InvokeMailingWorkflowResponse InvokeMailingWorkflow(string distributorID, int mailingID)
        {
            using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var response =
                    proxy.InvokeMailingWorkflow(new InvokeMailingWorkflowRequest1(
                    new InvokeMailingWorkflowRequest_V01 { DistributorID = distributorID, MailingID = mailingID }))
                    as InvokeMailingWorkflowResponse;

                SimpleCache.Expire(typeof(DataTable), mailingHistoryCacheKey(distributorID));
                return response;
            }
        }

        public static int? GetMailingQuotaInPastHours(string distributorID, int numberOfHours)
        {
            try
            {
                DateTime utcYesterday = DateTime.UtcNow.AddHours(-numberOfHours);
                TimeSpan durationTime = new TimeSpan(numberOfHours, 0, 0);

                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.CheckMailingQuota(new CheckMailingQuotaRequest1(new CheckMailingQuotaRequest_V01
                        {
                            DistributorID = distributorID,
                            StartDate = utcYesterday,
                            Duration = durationTime
                        })).CheckMailingQuotaResult 
                        as CheckMailingQuotaResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Count;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("GetMailingQuotaInLast24Hours distributorID:{0} failed", distributorID), ex));
                return null;
            }
        }

        #endregion

        #region helper methods

        private static string mailingHistoryDetailCacheKey(string distributorID, int mailingID)
        {
            return distributorID + "_MailingHistoryDetail_" + mailingID;
        }

        private static string mailingHistoryCacheKey(string distributorID)
        {
            return distributorID + "_MailingHistory";
        }

        public static void ExpireMailingHistory(string distributorID)
        {
            SimpleCache.Expire<string>(typeof(DataTable), mailingHistoryCacheKey(distributorID));
        }

        private static void ExpireMailingHistoryDetail(string distributorID, int mailingID)
        {
            SimpleCache.Expire<string>(typeof(DataTable), mailingHistoryDetailCacheKey(distributorID, mailingID));
        }

        #endregion

        public static bool RemoveOptOut(string reactivationCode)
        {
            try
            {
                using (var proxy = ServiceProvider.ServiceClientProvider.GetDistributorCRMServiceProxy())
                {
                    var response =
                        proxy.SaveOptOut(new SaveOptOutRequest1(new SaveOptOutCancelRequest_V01 { ReactivationCode = reactivationCode }));
                    return response.SaveOptOutResult.Status == ServiceResponseStatusType.Success;
                }
            }
            catch (Exception ex)
            {

                LoggerHelper.Exception("System.Exception", new Exception(String.Format("RemoveOptOut reactivationCode:[{0}] failed", reactivationCode), ex));
                return false;
            }
        }

        public static readonly Regex EmailAddressRegex = new Regex(@"^([a-zA-Z0-9_\!\#\$\%\&\'\*\-\/\=\?\^\`\{\|\}\~\.\+]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9_\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})$", RegexOptions.Compiled);
    }
}