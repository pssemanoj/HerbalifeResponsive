//using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using HL.Blocks.Caching.SimpleCache;
using HL.Blocks.CircuitBreaker;
using HL.Common.Configuration;
using HL.Common.DataContract.Interfaces;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers.Interface;
using MyHerbalife3.Ordering.Providers.OrderingProfile;
using MyHerbalife3.Shared.Infrastructure;
using MyHerbalife3.Shared.Infrastructure.ServiceFactory;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;

namespace MyHerbalife3.Ordering.Providers
{
    using System.Web.Security;
    using System.Web;
    using System.Web.Caching;
    using MyHerbalife3.Ordering.SharedProviders.ViewModel;
    using MyHerbalife3.Ordering.ServiceProvider;
    using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
    using MyHerbalife3.Ordering.ServiceProvider.DistributorSvc;
    using ServiceResponseStatusType = MyHerbalife3.Ordering.ServiceProvider.DistributorSvc.ServiceResponseStatusType;
    using Address_V01 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V01;
    using Address_V02 = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.Address_V02;
    using AddressType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.AddressType;

    public static class DistributorOrderingProfileProvider
    {
        private const int EventCacheMinutes = 60;
        private const int PresidentSummitEventId = 1160;
        private const int Honors2016EventId = 2462;
        private static List<string> PresidentSummitTypes = new List<string> { "MT", "PT", "15K", "20K", "30K", "40K", "50K" };
        public static List<string> MpeThresholdCountries = new List<string>(Settings.GetRequiredAppSetting("MPEThresholdCountries").Split(new char[] { ',' }));
        public const int FavouriteSKU_Cache_Minutes = 15;

        public static bool IsMarketingPlanDistributor
        {
            get
            {
                var membershipUser = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
                return membershipUser != null && (MpeThresholdCountries.Contains(membershipUser.Value.ProcessingCountry) && membershipUser.Value.TypeCode == "DS");
            }
        }

        /// <summary>
        /// Gets the distrubutor notes from service if needed and save them in the object
        /// </summary>
        /// <param name="distributor">The distributor</param>
        /// <param name="noteType">The note type</param>
        /// <param name="noteCode">The note code</param>
        public static void GetDistributorNotes(DistributorOrderingProfile distributor, string noteType, string noteCode)
        {
            if (distributor.DistributorNotes == null)
            {
                var request = new GetDistributorNotesRequest_V01
                {
                    DistributorId = distributor.Id,
                    NoteType = noteType,
                    NoteCode = noteCode
                };

                var response = new GetDistributorNotesResponse_V01();

                var proxy = ServiceClientProvider.GetDistributorServiceProxy();
                response = proxy.GetDistributorNotes(new GetDistributorNotesRequest(request)).GetDistributorNotesResult as GetDistributorNotesResponse_V01;
                if (response.Status == ServiceResponseStatusType.Success)
                {
                    distributor.DistributorNotes = response.DistibutorNotes;
                }
            }
        }
        
        public static DistributorOrderingProfile GetProfile(string distributorID, string countryCode)
        {
            return new DistributorOrderingProfileFactory().GetDistributorOrderingProfile(distributorID, countryCode);
        }

        public static Address_V02 GetAddressV02(AddressType type, string distributorID, string countryCode)
        {
            DistributorOrderingProfile profile = GetProfile(distributorID, countryCode);
            if (profile != null && profile.Addresses != null)
            {
                foreach (HL.Common.ValueObjects.Address_V02 a in profile.Addresses)
                {
                    if (a.TypeOfAddress.ToString() == type.ToString())
                        return new Address_V02()
                        {
                            Building = a.Building,
                            CareOf = a.CareOf,
                            City = a.CareOf,
                            Country = a.Country,
                            CountyDistrict = a.CountyDistrict,
                            IsActive = a.IsActive,
                            IsPrimary = a.IsPrimary,
                            IsValidated = a.IsValidated,
                            Line1 = a.Line1,
                            Line2 = a.Line2,
                            Line3 = a.Line3,
                            Line4 = a.Line4,
                            PostalCode = a.PostalCode,
                            StateProvinceTerritory = a.StateProvinceTerritory,
                            Suburb = a.Suburb,
                            To = a.To,
                            TypeOfAddress = (AddressType)Enum.Parse(typeof(AddressType), a.TypeOfAddress.ToString())
                        };
                }
            }
            return null;
        }

        public static Address_V01 GetAddress(AddressType type, string distributorID, string countryCode)
        {
            DistributorOrderingProfile profile = GetProfile(distributorID, countryCode);
            if (profile != null && profile.Addresses != null)
            {
                foreach (HL.Common.ValueObjects.Address_V02 a in profile.Addresses)
                {
                    if (a.TypeOfAddress.ToString() == type.ToString())
                    {
                        return new Address_V01{ Country =  a.Country, City =  a.City, CountyDistrict = a.CountyDistrict, Line1 = a.Line1, Line2 =  a.Line2,
                                                Line3 = a.Line3,
                                                Line4 = a.Line4, 
                                                StateProvinceTerritory = a.StateProvinceTerritory,
                                                PostalCode = a.PostalCode
                        };
                    }
                }

            }
            return null;
        }

        public static string GetPhoneNumberForCN(string distributorID)
        {
            var phoneNum = string.Empty;
            var isoCountryCode = Thread.CurrentThread.CurrentCulture.Name.Substring(3);
            var distributorOrderingProfile = GetProfile(distributorID, isoCountryCode);
            if (distributorOrderingProfile.PhoneNumbers.Any())
            {
                var phoneNumbers =
                    distributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03 !=
                    null
                        ? distributorOrderingProfile.PhoneNumbers.Where(p => p.IsPrimary) as PhoneNumber_V03
                        : distributorOrderingProfile.PhoneNumbers.FirstOrDefault() as PhoneNumber_V03;
                if (phoneNumbers != null)
                    phoneNum = phoneNumbers.Number;
            }
            return phoneNum;

        }
        public static List<TaxIdentification> GetTinList(string distributorID, bool getCurrentOnly, bool reload = false)
        {
            var tinList = new List<TaxIdentification>();
            string isoCountryCode = Thread.CurrentThread.CurrentCulture.Name.Substring(3);

            DistributorOrderingProfile distributorOrderingProfile = reload
                                                                        ? new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(distributorID, isoCountryCode)
                                                                        : GetProfile(distributorID, isoCountryCode);

            var countryCodes = new List<string>();
            countryCodes.Add(isoCountryCode);
            countryCodes.AddRange(HL.Common.ValueObjects.CountryType.Parse(isoCountryCode).HmsCountryCodes);
            var now = DateUtils.GetCurrentLocalTime(isoCountryCode);

            if (null != distributorOrderingProfile && null != distributorOrderingProfile.TinList)
            {
                foreach (TaxIdentification taxId in distributorOrderingProfile.TinList)
                {
                    if (countryCodes.Contains(taxId.CountryCode))
                    {
                        if (getCurrentOnly)
                        {
                            if (taxId.IDType.ExpirationDate > now)
                            {
                                tinList.Add(taxId);
                            }
                        }
                        else
                        {
                            tinList.Add(taxId);
                        }
                    }
                }
            }

            return tinList;
        }

        public static string GetTaxIdentificationId(string distributorID, string tinId)
        {
            var tinList = new List<TaxIdentification>();
            string isoCountryCode = Thread.CurrentThread.CurrentCulture.Name.Substring(3);
            var distributorOrderingProfile = new DistributorOrderingProfileFactory().ReloadDistributorOrderingProfile(distributorID, isoCountryCode);
            var countryCodes = new List<string>();
            countryCodes.Add(isoCountryCode);
            countryCodes.AddRange(HL.Common.ValueObjects.CountryType.Parse(isoCountryCode).HmsCountryCodes);
            var now = DateUtils.GetCurrentLocalTime(isoCountryCode);
            if (distributorOrderingProfile != null && distributorOrderingProfile.TinList != null)
            {
                var tin = distributorOrderingProfile.TinList.Where(t => t.IDType != null && t.IDType.Key == tinId).FirstOrDefault();
                if (tin != null)
                {
                    return tin.ID;
                }
            }

            return null;
        }

        public static DRFraudStatusType CheckForDRFraud(string distributorID, string countryCode, string zipCode)
        {
            var fraudStatus = DRFraudStatusType.None;

            string CacheKey = string.Format("{0}_{1}", "DSFraud", countryCode);
            Dictionary<string, List<string>> blockedPostalCodes = HttpRuntime.Cache[CacheKey] as Dictionary<string, List<string>>;

            DistributorOrderingProfile distributorOrderingProfile = GetProfile(distributorID, countryCode);
            if (blockedPostalCodes == null)
            {
                using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
                {
                    try
                    {
                        var request = new GetDRFraudStatusRequest_V01();
                        request.DistributorID = distributorID;
                        request.CountryCode = countryCode;
                        request.PostalCode = zipCode;

                        request.DistributorIsBlocked = distributorOrderingProfile.IsDistributorBlocked;
                        request.SponsorIsBlocked = distributorOrderingProfile.IsSponsorBlocked;

                        var response =
                            proxy.GetDRFraudStatus(new GetDRFraudStatusRequest(request)).GetDRFraudStatusResult as GetDRFraudStatusResponse_V01;
                        if (response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                        {
                            fraudStatus = response.FraudStatus;
                            blockedPostalCodes = response.BlockedPostCodes;
                        }
                        HttpRuntime.Cache[CacheKey] = response.BlockedPostCodes != null ? response.BlockedPostCodes : new Dictionary<string, List<string>> { };
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Exception("System.Exception", new Exception(
                                                                       string.Format(
                                                                           "Error retrieving DRFraud Status from Order service for: DS:{0} - Country:{1}, {2}",
                                                                           distributorID, countryCode, ex)));
                        fraudStatus = DRFraudStatusType.None;
                    }
                }
            }
            if (distributorOrderingProfile.IsDistributorBlocked && blockedPostalCodes !=null)
            {
                if (blockedPostalCodes.ContainsKey(countryCode) && blockedPostalCodes[countryCode].Contains(zipCode))
                {
                    return DRFraudStatusType.PostalCodeIsBlocked;
                }
            }

            //if (fraudStatus == DRFraudStatusType.DistributorIsBlocked)
            //{
            //    errorResxKey = "BlockedDS";
            //}
            //else if (fraudStatus == DRFraudStatusType.PostalCodeIsBlocked)
            //{
            //    errorResxKey = "BlockedZip";
            //}

            return fraudStatus;
        }

        
        public static bool SetFavouriteSKU(string distributorID, int productID, string productSKU, string locale, int DEL = 0)
        {
            ServiceProvider.DistributorSvc.SetDistributorFavouriteRequest request = new ServiceProvider.DistributorSvc.SetDistributorFavouriteRequest
            {
                DistributorId = distributorID,
                ProductID = productID,
                ProductSKU = productSKU,
                Locale = locale,
                Delete = (DEL > 0 ? true : false)
            };

            if (string.IsNullOrEmpty(request.DistributorId) || string.IsNullOrEmpty(request.ProductSKU) || string.IsNullOrEmpty(request.Locale))
            {
                return false;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetDistributorServiceProxy();
                    var response = proxy.SetDistributorFavouriteSKU(new ServiceProvider.DistributorSvc.SetDistributorFavouriteSKURequest(request)).SetDistributorFavouriteSKUResult as ServiceProvider.DistributorSvc.SetDistributorFavouriteResponse;
                    if (response != null && response.Status == ServiceProvider.DistributorSvc.ServiceResponseStatusType.Success)
                    {
                        ISimpleCache _cache = CacheFactory.Create();
                        var cacheKey = string.Format("Favour_{0}_{1}", distributorID.ToUpper(), Thread.CurrentThread.CurrentUICulture.Name);
                        _cache.Expire(typeof(List<FavouriteSKU>), cacheKey);
                        //HttpContext.Current.Session[cacheKey] = null;
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    return false;
                }
            }
            return false;
        }


        public static bool ClearDistributorCache(string distributorID, string type)
        {
            bool IsCleared = false;
            try
                {
                var proxy = ServiceClientProvider.GetDistributorServiceProxy();

                IsCleared = proxy.ClearCache(new ServiceProvider.DistributorSvc.ClearCacheRequest(distributorID, type)).ClearCacheResult;
            }
            catch (Exception ex)
                {
                    LoggerHelper.Warn(
                        string.Format("MyHerbalife3.Ordering.Providers.ClearDistributorCache - distributorId: {0} - type: {1} - Exception: {2}",
                        distributorID, type, ex.Message));
                }
            

            return IsCleared;
        }
        
        public static bool SetFavouriteSKUList(string distributorID, string locale, string SKUList )
        {
            SetDistributorFavouriteListRequest request = new SetDistributorFavouriteListRequest()
            {
                DistributorId = distributorID,
                Locale = locale,
                SKUList = SKUList
            };

            if (string.IsNullOrEmpty(request.DistributorId) || string.IsNullOrEmpty(request.SKUList) || string.IsNullOrEmpty(request.Locale))
            {
                return false;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetDistributorServiceProxy();
                    var response = proxy.SetDistributorFavouriteSKUList(new SetDistributorFavouriteSKUListRequest(request)).SetDistributorFavouriteSKUListResult as SetDistributorFavouriteResponse;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        ISimpleCache _cache = CacheFactory.Create();
                        var cacheKey = string.Format("Favour_{0}_{1}", distributorID.ToUpper(), Thread.CurrentThread.CurrentUICulture.Name);
                        _cache.Expire(typeof(List<FavouriteSKU>), cacheKey);
                        //HttpContext.Current.Session[cacheKey] = null;
                        return true;
                    }

                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                    return false;
                }
            }
            return false;
        }

        public static List<FavouriteSKU> GetDistributorFavouriteSKU(string distributorID, string locale)
        {
            ISimpleCache _cache = CacheFactory.Create();
            var cacheKey = string.Format("Favour_{0}_{1}", distributorID.ToUpper(), Thread.CurrentThread.CurrentUICulture.Name);
            var result = _cache.Retrieve(_ => LoadFromService(distributorID, locale), cacheKey, TimeSpan.FromMinutes(FavouriteSKU_Cache_Minutes));

            return result ?? new List<FavouriteSKU>();
        }

        public static List<FavouriteSKU> LoadFromService(string distributorID, string locale)
        {
            List<FavouriteSKU> SKUs = new List<FavouriteSKU>();
            GetDistributorFavouriteSKURequest request = new GetDistributorFavouriteSKURequest()
            {
                DistributorID = distributorID,
                Locale = locale
            };

            if (string.IsNullOrEmpty(request.DistributorID))
            {
                return SKUs;
            }
            else
            {
                try
                {
                    var proxy = ServiceClientProvider.GetDistributorServiceProxy();
                    var response = proxy.GetDistributorFavouriteSKUs(new GetDistributorFavouriteSKUsRequest(request)).GetDistributorFavouriteSKUsResult as GetDistributorFavouriteSKUResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        SKUs = response.FavouriteSKUs;
                    }

                }
                catch (Exception ex)
                {
                    ExceptionPolicy.HandleException(ex, ProviderPolicies.SYSTEM_EXCEPTION);
                }
            }

            return SKUs;
        }

        public static List<DistributorTraining_V01> GetTrainingList(string distributorID, string countryCode)
        {
            List<DistributorTraining_V01> trainings = new List<DistributorTraining_V01>();
            using (var proxy = ServiceClientProvider.GetDistributorServiceProxy())
            {
                try
                {
                    var response = proxy.GetBasicDistributor(new GetBasicDistributorRequest(
                        new GetBasicDistributorRequest_V01() { 
                            DistributorID = distributorID, 
                            CountryCode = countryCode, 
                            RequiresTin = false
                        })).GetBasicDistributorResult as GetBasicDistributorResponse_V01;

                    if (response.Status == ServiceResponseStatusType.Success && response.Distributor != null)
                    {
                        trainings = response.Distributor.DsTrainings;
                    }
                }
                catch(Exception ex)
                {
                    LoggerHelper.Warn(
                        string.Format("MyHerbalife3.Ordering.Providers.DistributorOrderingProfileProvider - distributorId: {0} - country: {1} - Exception: {2}", 
                        distributorID, countryCode, ex.Message));
                }
            }
            
            return trainings;
        }
        public static GetBasicDistributorResponse_V01 GetDistributorProfileforEmail(string distributorID, string countryCode)
        {
            var response = new GetBasicDistributorResponse_V01();
            using (var proxy = ServiceClientProvider.GetDistributorServiceProxy())
            {
                try
                {
                      response = proxy.GetBasicDistributor(new GetBasicDistributorRequest(
                        new GetBasicDistributorRequest_V01() 
                        {
                            DistributorID = distributorID,
                            CountryCode = countryCode,
                            RequiresTin = false
                        })).GetBasicDistributorResult as GetBasicDistributorResponse_V01;

                    if (response.Status == ServiceResponseStatusType.Success && response.Distributor != null)
                    {
                        
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Warn(
                        string.Format("MyHerbalife3.Ordering.Providers.DistributorOrderingProfileProvider - distributorId: {0} - country: {1} - Exception: {2}",
                        distributorID, countryCode, ex.Message));
                }
            }

            return response;
        }

        public static void GetHAPSettings(DistributorOrderingProfile distributor)
        {
            if (!distributor.HAPExpiryDateSpecified || distributor.HAPExpiryDate == null)
            {
                using (var proxy = ServiceClientProvider.GetDistributorServiceProxy())
                {
                    try
                    {
                        var response = proxy.GetBasicDistributor(new GetBasicDistributorRequest(
                            new GetBasicDistributorRequest_V01()
                            {
                                DistributorID = distributor.Id,
                                CountryCode = distributor.CurrentLoggedInCountry,
                                RequiresTin = false
                            })).GetBasicDistributorResult as GetBasicDistributorResponse_V01;

                        if (response.Status == ServiceResponseStatusType.Success && response.Distributor != null)
                        {
                            distributor.HAPExpiryDateSpecified = response.Distributor.HAPExpiryDateFieldSpecified;
                            distributor.HAPExpiryDate = response.Distributor.HAPExpiryDateField;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Warn(
                            string.Format("MyHerbalife3.Ordering.Providers.DistributorOrderingProfileProvider.GetHAPSettings - distributorId: {0} - country: {1} - Exception: {2}",
                            distributor.Id, distributor.CurrentLoggedInCountry, ex.Message));
                    }
                }
            }
        }
        public static Scheme CheckDsLevelType(string distributorID,string countryCode)
        {
            //string DsLevel =new LevelGroupType();
            //List<DistributorTraining_V01> trainings = new List<DistributorTraining_V01>();
            using (var proxy = ServiceClientProvider.GetDistributorServiceProxy())
            {
              
                try
                {
                    var response = proxy.GetBasicDistributor(new GetBasicDistributorRequest(
                        new GetBasicDistributorRequest_V01()
                        {
                            DistributorID = distributorID,
                            CountryCode = countryCode,
                            RequiresTin = false

                        })).GetBasicDistributorResult as GetBasicDistributorResponse_V01;

                    if (response.Status == ServiceResponseStatusType.Success && response.Distributor != null)
                    {
                         return response.Distributor.Scheme  ;
                        
                    }
                    
                }
                catch (Exception ex)
                {
                    LoggerHelper.Warn(
                        string.Format("MyHerbalife3.Ordering.Providers.DistributorOrderingProfileProvider - distributorId: {0} - Exception: {1}",
                        distributorID, ex.Message));
                }
            }

            return Scheme.Distributor; 

        }


        public static string CheckForDRFraud(DistributorOrderingProfile distributor, string zipCode)
        {
            string errorResxKey = string.Empty;
            var fraudStatus = DRFraudStatusType.None;
            bool isBlocked, isSponsorBlocked, isDsFound;
            isBlocked = isSponsorBlocked = isDsFound = false;

            using (var proxy = ServiceClientProvider.GetOrderServiceProxy())
            {
                try
                {
                    if (distributor != null && distributor.FraudStatus == null)
                    {
                        using (var proxyDs = ServiceClientProvider.GetDistributorServiceProxy())
                        {
                            var requestV01 = new GetBasicDistributorRequest_V01
                            {
                                DistributorID = distributor.Id
                            };

                            var circuitBreaker =
                                CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetBasicDistributorResponse_V01>();
                            var responseV01 =
                                circuitBreaker.Execute(() => proxyDs.GetBasicDistributor(new GetBasicDistributorRequest(requestV01))).GetBasicDistributorResult as
                                GetBasicDistributorResponse_V01;

                            if (responseV01 != null && responseV01.Status == ServiceResponseStatusType.Success)
                            {
                                if (responseV01.Distributor != null)
                                {
                                    isBlocked = responseV01.Distributor.DRFraudStatusFlags.IsDistributorblocked;
                                    isSponsorBlocked = responseV01.Distributor.DRFraudStatusFlags.IsSponsorBlocked;
                                    isDsFound = true;

                                }
                            }
                        }
                    }
                    else if (distributor != null && distributor.FraudStatus != null)
                    {
                        isBlocked = distributor.FraudStatus.IsDistributorblocked;
                        isSponsorBlocked = distributor.FraudStatus.IsSponsorBlocked;
                        isDsFound = true;
                    }

                    if (isDsFound)
                    {
                        var request = new GetDRFraudStatusRequest_V01();
                        request.DistributorID = distributor.Id;
                        request.CountryCode = distributor.CurrentLoggedInCountry;
                        request.PostalCode = zipCode;
                        request.DistributorIsBlocked = isBlocked;
                        request.SponsorIsBlocked = isSponsorBlocked;

                        var response =
                            proxy.GetDRFraudStatus(new GetDRFraudStatusRequest(request)).GetDRFraudStatusResult as GetDRFraudStatusResponse_V01;
                        if (response.Status == ServiceProvider.OrderSvc.ServiceResponseStatusType.Success)
                        {
                            fraudStatus = response.FraudStatus;
                        }
                    }
                    else
                    {
                        LoggerHelper.Exception("System.Exception", new Exception(
                                                                       string.Format(
                                                                           "Error retrieving DS DRFraudStatusFlags from Cache and Distributor service for: DS:{0} - Country:{1}",
                                                                           distributor.Id, distributor.CurrentLoggedInCountry)));
                        fraudStatus = DRFraudStatusType.None;
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Exception("System.Exception", new Exception(
                                                                   string.Format(
                                                                       "Error retrieving DRFraud Status from Order service for: DS:{0} - Country:{1}, {2}",
                                                                       distributor.Id, distributor.CurrentLoggedInCountry, ex)));
                    fraudStatus = DRFraudStatusType.None;
                }
            }

            if (fraudStatus == DRFraudStatusType.DistributorIsBlocked)
            {
                errorResxKey = "BlockedDS";
            }
            else if (fraudStatus == DRFraudStatusType.PostalCodeIsBlocked)
            {
                errorResxKey = "BlockedZip";
            }

            return errorResxKey;
        }

        /// <summary>
        /// Updates Alert on CustomerExtention table
        /// </summary>
        /// <remarks>Used only for China DO System</remarks>
        /// <param name="distributorId">The distributor identifier.</param>
        public static bool UpdateAlertCustomerExtention(string distributorId)
        {
            var result = false;
            var proxy = ServiceClientProvider.GetDistributorServiceProxy();

            try
            {
                var request = new UpdateAlertCustomerExtentionRequest_V01
                {
                    DistributorId = distributorId,
                };
                var response = proxy.UpdateAlertCustomerExtention(new UpdateAlertCustomerExtentionRequest1(request)).UpdateAlertCustomerExtentionResult;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(
                  new Exception(string.Format("Error in  DistributorProvider.UpdateAlertCustomerExtention, for DS id: {0}; error msg: {1}", distributorId, ex)),
                  ProviderPolicies.SYSTEM_EXCEPTION);
            }
            finally
            {
                ServiceClientFactory.Dispose(proxy);
            }

            return result;
        }

        public static void CheckForMPCFraud(DistributorOrderingProfile distributor)
        {
            string errorResxKey = string.Empty;

            try
            {
                if (distributor != null && distributor.IsMPCFraud == null)
                {
                    using (var proxyDs = ServiceClientProvider.GetDistributorServiceProxy())
                    {
                        var requestV01 = new GetBasicDistributorRequest_V01
                        {
                            DistributorID = distributor.Id
                        };

                        var circuitBreaker =
                            CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetBasicDistributorResponse_V01>();
                        var responseV01 =
                            circuitBreaker.Execute(() => proxyDs.GetBasicDistributor(new GetBasicDistributorRequest(requestV01))).GetBasicDistributorResult as
                            GetBasicDistributorResponse_V01;

                        if (responseV01 != null && responseV01.Status == ServiceResponseStatusType.Success)
                        {
                            if (responseV01.Distributor != null)
                            {
                                distributor.IsMPCFraud = responseV01.Distributor.IsMPCFraud;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", new Exception(
                                                               string.Format(
                                                                   "Error retrieving MPC Fraud from Distributor service for: DS:{0} - Country:{1}, {2}",
                                                                   distributor.Id, distributor.CurrentLoggedInCountry, ex)));
            }
        }

        #region Event qualifiers

        /// <summary>
        /// Defines if the logged member is qualified for the event provided.
        /// </summary>
        /// <param name="eventId">The event id.</param>
        /// <param name="locale">Locale of the event</param>
        /// <returns>This returns false if event logic qualification is not defined.</returns>
        public static bool IsEventQualified(int eventId, string locale)
        {
            if (eventId <= 0)
            {
                return false;
            }
            
            var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();

            if (eventId == PresidentSummitEventId && null != user)
            {
                if (PresidentSummitTypes.Contains(user.Value.SubTypeCode))
                {
                    return true;
                }
                
                var presidentSummitMembers = GetEventQualifierList(PresidentSummitEventId);
                return
                    presidentSummitMembers.Exists(
                        m => m.DistributorID == user.Value.Id && m.CountryOfProcessing == locale.Substring(3));
            }

            if (eventId == Honors2016EventId && null != user)
            {
                var honors2016Members = GetEventQualifierList(Honors2016EventId);
                return
                    honors2016Members.Exists(m => m.DistributorID == user.Value.Id);
            }
            if (eventId > 0 && null != user && (locale == "es-ES" || locale == "ru-BY"))
            {
                var members = GetDSEventQualifierList(eventId, user.Value.Id);
                return members.Exists(m => m.HasEventTicket);
            }
            if (eventId > 0 && null != user)
            {
                var members = GetEventQualifierList(eventId);
                return
                    members.Exists(m => m.DistributorID == user.Value.Id);
            }

            return false;
        }

        /// <summary>
        /// Gets a flag if the member is qualified for an event and if member purchased the ticket
        /// </summary>
        /// <param name="eventId">The event Id</param>
        /// <param name="locale">The locale</param>
        /// <param name="ticketPurchased">Flag if member has a purchased ticket</param>
        /// <returns></returns>
        public static bool IsEventQualified(int eventId, string locale, out bool ticketPurchased)
        {
            var isQualified = false;
            ticketPurchased = false;
            if (eventId <= 0 || string.IsNullOrEmpty(locale))
            {
                return isQualified;
            }

            var user = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (user != null)
            {
                var eventDetails = GetEventDetails(eventId, user.Value.Id, locale);
                if (eventDetails != null)
                {
                    ticketPurchased = eventDetails.IsTicketsPurchased;
                    isQualified = eventDetails.IsQualified;
                }
            }

            return isQualified;
        }

        /// <summary>
        /// Gets the list of qualifiers for an event.
        /// </summary>
        /// <param name="eventId">The event Id.</param>
        /// <returns></returns>
        public static List<ServiceProvider.EventSvc.EventQualifier_V01> GetEventQualifierList(int eventId)
        {
            if (eventId <= 0)
            {
                return null;
            }

            var memberCacheKey = string.Format("MemberList_Event{0}", eventId);
            var eventQualifierList = HttpRuntime.Cache[memberCacheKey] as List<ServiceProvider.EventSvc.EventQualifier_V01>;
            if (eventQualifierList == null || !eventQualifierList.Any())
            {
                eventQualifierList = new List<ServiceProvider.EventSvc.EventQualifier_V01>();
                using (var proxy = ServiceClientProvider.GetEventServiceProxy())
                {
                    try
                    {
                        var request = new ServiceProvider.EventSvc.GetEventQualifierListRequest();
                        var requestV01 = new ServiceProvider.EventSvc.GetEventQualifierRequest_V01
                            {
                                EventID = eventId,
                                //EventIDSpecified = true
                                
                            };
                        request.request = requestV01;
                        var response = proxy.GetEventQualifierList(request);
                        var response01 = response.GetEventQualifierListResult as ServiceProvider.EventSvc.GetEventQualifierResponse_V01;
                        if (response01 != null && response01.EventQualifiers != null)
                        {
                            eventQualifierList = response01.EventQualifiers.ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        eventQualifierList = new List<ServiceProvider.EventSvc.EventQualifier_V01>();
                        LoggerHelper.Error(
                            string.Format("GetEventQualifierList error calling service EventId:{0} ERR:{1}",
                                          eventId, ex.ToString()));
                    }
                }

                if (eventQualifierList.Any())
                {
                    HttpRuntime.Cache.Insert(memberCacheKey, eventQualifierList, null,
                                             DateTime.Now.AddMinutes(EventCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                }
            }

            return eventQualifierList;
        }

        //s
        /// <summary>
        /// Gets the list of qualifiers for an event.
        /// </summary>
        /// <param name="eventId">The event Id.</param>
        /// <returns></returns>
        public static List<ServiceProvider.EventSvc.EventQualifier_V01> GetDSEventQualifierList(int eventId, string distributorid)
        {
            if (eventId <= 0 && string.IsNullOrEmpty(distributorid))
            {
                return null;
            }
               var eventQualifierList = new List<ServiceProvider.EventSvc.EventQualifier_V01>();
                using (var proxy = ServiceClientProvider.GetEventServiceProxy())
                {
                    try
                    {
                        var request = new ServiceProvider.EventSvc.GetEventQualifierListRequest();
                        var requestV01 = new ServiceProvider.EventSvc.GetEventQualifierRequest_V01
                        {
                            EventID = eventId,
                            DistributorID = distributorid
                        };
                        request.request = requestV01;
                        var response = proxy.GetEventQualifierList(request);
                        var response01 = response.GetEventQualifierListResult as ServiceProvider.EventSvc.GetEventQualifierResponse_V01;
                        if (response01 != null && response01.EventQualifiers != null)
                        {
                            eventQualifierList = response01.EventQualifiers.ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        eventQualifierList = new List<ServiceProvider.EventSvc.EventQualifier_V01>();
                        LoggerHelper.Error(
                            string.Format("GetEventQualifierList error calling service EventId:{0} ERR:{1}",
                                          eventId, ex.ToString()));
                    }
                }
            return eventQualifierList;
        }

        /// <summary>
        /// Gets the details of the event for a member qualified
        /// </summary>
        /// <param name="eventId">The event Id</param>
        /// <param name="distributorId">The member Id</param>
        /// <param name="locale">The locale</param>
        /// <returns></returns>
        public static ServiceProvider.EventRestSvc.Event GetEventDetails(int eventId, string distributorId, string locale, bool isPublished = true)
        {
            if (eventId <= 0 || string.IsNullOrEmpty(distributorId) || string.IsNullOrEmpty(locale))
            {
                return null;
            }

            ServiceProvider.EventRestSvc.Event result = null;
            var memberCacheKey = string.Format("Member_EventDetails{0}", eventId);
            var eventDetailsList = HttpRuntime.Cache[memberCacheKey] as Dictionary<string,ServiceProvider.EventRestSvc.Event>;
            if (eventDetailsList == null || !eventDetailsList.Any() || !eventDetailsList.ContainsKey(distributorId))
            {
                if (eventDetailsList == null)
                    eventDetailsList = new Dictionary<string, ServiceProvider.EventRestSvc.Event>();
                var proxy = ServiceClientProvider.GetEventDetailServiceProxy();
                try
                {
                    var request = new ServiceProvider.EventRestSvc.GetEventDetailsRequest() { EventId = eventId, LocaleCode = locale, MemberId = distributorId, isPublished = isPublished };
                    var response = proxy.GetEventDetails(request);
                    if (response != null && response.GetEventDetailsResult != null && response.GetEventDetailsResult.errorCode == 0 && response.GetEventDetailsResult.Event != null)
                    {
                        result = response.GetEventDetailsResult.Event;
                        eventDetailsList.Add(distributorId, result);
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(
                        string.Format("GetEventDetails error calling service EventId:{0} ERR:{1}",
                                        eventId, ex.ToString()));
                }

                if (eventDetailsList.Any())
                {
                    HttpRuntime.Cache.Insert(memberCacheKey, eventDetailsList, null,
                                             DateTime.Now.AddMinutes(EventCacheMinutes),
                                             Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, null);
                }
            }

            if (eventDetailsList != null && eventDetailsList.ContainsKey(distributorId) && result == null)
            {
                result = eventDetailsList.FirstOrDefault(e => e.Key == distributorId).Value;
            }

            return result;
        }

        #endregion
    }
}
