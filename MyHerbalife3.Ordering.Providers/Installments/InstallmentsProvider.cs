using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Xml.Serialization;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Installments
{

    #region Installment

    //// TODO move to ValueObjects
    //public class InstallmentConfiguration
    //{
    //    public byte Month { get; set; }
    //    public int Year { get; set; }

    //    public DateTime LastDateTimeToPlaceOrders { get; set; }
    //    public DateTime LastPaymentDateTime { get; set; }
    //    public DateTime DraftExclusionDateTime { get; set; }
    //    public DateTime TicketDueDate { get; set; }

    //    public Cards Cards { get; set; }

    //}

    //public enum InstallmentStrategy
    //{
    //    Price,
    //    Volume
    //}

    //public class Cards
    //{
    //    [XmlElement]
    //    public InstallmentStrategy Strategy { get; set; }
    //    [XmlElement]
    //    public string FeeSKU { get; set; }
    //    [XmlElement]
    //    public int MaxEventTicketInstallments { get; set; }
    //    [XmlElement]
    //    public List<CardInfo> Card = new List<CardInfo>();

    //}
    //public class CardInfo
    //{
    //    [XmlElement]
    //    public string CardId { get; set; }
    //    [XmlElement]
    //    public string Name { get; set; }
    //    [XmlElement]
    //    public int Id { get; set; }
    //    [XmlElement]
    //    public List<InstallmentFee> InstallmentFees { get; set; }
    //    [XmlElement]
    //    public List<VolumeStrategy> VolumeStrategy { get; set; }
    //    [XmlElement]
    //    public List<PriceStrategy> PriceStrategy { get; set; }
    //}

    //public class InstallmentFee
    //{
    //    [XmlElement]
    //    public int InstallmentNumber { get; set; }
    //    [XmlElement]
    //    public decimal FeeRate { get; set; }
    //}

    //public class PriceStrategy
    //{
    //    [XmlElement]
    //    public decimal PriceThreshold { get; set; }
    //    [XmlElement]
    //    public int MaxInstallments { get; set; }
    //    [XmlElement]
    //    public bool ChargeFee { get; set; }
    //}
    //public class VolumeStrategy
    //{
    //    [XmlElement]
    //    public decimal Volume { get; set; }
    //    [XmlElement]
    //    public int MaxInstallments { get; set; }
    //    [XmlElement]
    //    public bool ChargeFee { get; set; }
    //    [XmlElement]
    //    public EffectiveDates EffectiveDates { get; set; }
    //}

    //public class EffectiveDates
    //{
    //    [XmlElement]
    //    public DateTime StartDateTime { get; set; }
    //    [XmlElement]
    //    public DateTime EndDateTime { get; set; }
    //}

    #endregion

    #region Installments Service based methods

    public class InstallmentsProvider
    {
        #region constants

        public const string INSTALLMENTSINFO_CACHE_PREFIX = "InstallmentsInfo_";

        #endregion

        #region Methods (Cache Related)

        // **********************************************************************************************
        public static string GetCacheKey(string CountryCode, DateTime applyingDate, string orderType)
        {
            return string.Concat(INSTALLMENTSINFO_CACHE_PREFIX, CountryCode, applyingDate.ToString("yyyymm"), orderType);
        }

        // **********************************************************************************************
        private static InstallmentConfiguration GetInstallmentsConfigurationInfoFromCache(string cacheKey)
        {
            // Retrieve object from Cache
            try
            {
                var session = HttpContext.Current.Session;
                return session[cacheKey] as InstallmentConfiguration;
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
            }

            return null;
        }

        // **********************************************************************************************
        private static void SaveInstallmentConfigurationInfoToCache(string cacheKey,
                                                                    InstallmentConfiguration configuration)
        {
            SaveInstallmentConfigurationInfoToCache(cacheKey, configuration, null);
        }

        // **********************************************************************************************
        private static void SaveInstallmentConfigurationInfoToCache(string cacheKey,
                                                                    InstallmentConfiguration configuration,
                                                                    HttpSessionState session)
        {
            if (configuration != null)
            {
                if (null != HttpContext.Current)
                {
                    var thisSession = session ?? HttpContext.Current.Session;
                    thisSession[cacheKey] = configuration;
                }
            }
        }

        #endregion

        // **********************************************************************************************

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static InstallmentConfiguration GetInstallmentsConfiguration(string CountryCode,
                                                                            DateTime applicationDate,
                                                                            string orderType)
        {
            if (string.IsNullOrEmpty(CountryCode))
                return null;

            string cacheKey = GetCacheKey(CountryCode, applicationDate, orderType);

            var installmentConfiguration = GetInstallmentsConfigurationInfoFromCache(cacheKey);

            if (installmentConfiguration == null || installmentConfiguration.Month != applicationDate.Month)
            {
                installmentConfiguration = new InstallmentConfiguration();

                var results = LoadInstallmentsConfigurationInfoFromService(CountryCode, applicationDate);

                if (results != null)
                {
                    var result = results.Find(r => r.OrderType == orderType);
                    var serializer = new XmlSerializer(typeof (Cards));

                    if (result != null && !string.IsNullOrEmpty(result.ConfigurationData))
                    {
                        var creditCardsInstallmentsConfiguration =
                            (Cards) serializer.Deserialize(new StringReader(result.ConfigurationData));

                        installmentConfiguration.Cards = creditCardsInstallmentsConfiguration;
                    }

                    if (result != null)
                    {
                        if (result.DraftExclusionDateTime != null)
                            installmentConfiguration.DraftExclusionDateTime = (DateTime) result.DraftExclusionDateTime;
                        if (result.LastDateTimeToPlaceOrders != null)
                            installmentConfiguration.LastDateTimeToPlaceOrders =
                                (DateTime) result.LastDateTimeToPlaceOrders;
                        if (result.LastPaymentDateTime != null)
                            installmentConfiguration.LastPaymentDateTime = (DateTime) result.LastPaymentDateTime;
                        if (result.TicketDueDate != null)
                            installmentConfiguration.TicketDueDate = (DateTime) result.TicketDueDate;

                        installmentConfiguration.Month = (byte) result.ApplyDate.Month;
                        installmentConfiguration.Year = result.ApplyDate.Year;
                    }
                }
                else
                {
                    installmentConfiguration.Month = (byte) applicationDate.Month;
                    installmentConfiguration.Year = applicationDate.Year;
                }

                SaveInstallmentConfigurationInfoToCache(cacheKey, installmentConfiguration);
            }

            return installmentConfiguration;
        }

        // **********************************************************************************************

        [DataObjectMethod(DataObjectMethodType.Select)]
        public static InstallmentConfiguration GetInstallmentsConfiguration(string CountryCode, DateTime applicationDate)
        {
            if (string.IsNullOrEmpty(CountryCode))
                return null;

            return GetInstallmentsConfiguration(CountryCode, applicationDate, "RSO");
        }

        // **********************************************************************************************
        private static List<CardInstallment> LoadInstallmentsConfigurationInfoFromService(string CountryCode,
                                                                                          DateTime applicationDate)
        {
            if (string.IsNullOrEmpty(CountryCode))
            {
                return null;
            }

            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            var response =
                (RetrieveCardInstallmentsConfigurationResponse_V01)
                proxy.RetrieveCardInstallmentsConfiguration(new RetrieveCardInstallmentsConfigurationRequest1(new RetrieveCardInstallmentsConfigurationRequest_V01
                    {
                        CountryCode = CountryCode,
                        ApplicationDate = applicationDate
                    })).RetrieveCardInstallmentsConfigurationResult;

            if (response != null && response.Status == ServiceResponseStatusType.Success &&
                response.Installments != null)
            {
                return response.Installments;
            }

            return null;
        }

        // **********************************************************************************************
    }

    #endregion
}