using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Configuration;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.SharedProviders.Bizworks
{
    public static class ContactsDataProvider
    {
        private static readonly TimeSpan DefaultSimpleCacheDuration
            = new TimeSpan(0, Settings.GetRequiredAppSetting("DistributorCRMContactCacheMinutes", 6), 0);

        #region Contacts

        public static List<Contact_V02> GetContactsByName(string distributorID, string partialName)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new ListContactsByNameRequest_V01();
                request.Name = partialName;
                request.DistributorID = distributorID;
                try
                {
                    var response = proxy.ListContactsByName(new ListContactsByNameRequest(request)).ListContactsByNameResult as ListContactsByNameResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Contacts;
                    }
                    else
                    {
                        
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        public static List<Contact_V03> GetContactsByOwner(string distributorID)
        {
            try
            {
                return SimpleCache.Retrieve(
                    delegate
                    {
                        using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
                        {
                            var request = new ListContactsByOwnerRequest_V01();

                            request.DistributorID = distributorID;

                            var response = proxy.ListContactsByOwner(new ListContactsByOwnerRequest(request)).ListContactsByOwnerResult as ListContactsByOwnerResponse_V01;
                            if (response != null && response.Status == ServiceResponseStatusType.Success)
                            {
                                return response.Contacts;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    },
                    distributorID, DefaultSimpleCacheDuration);
            }
            catch (Exception ex)
            {
                Log(
                    new ApplicationException(
                        "Error retrieving contacts in GetContactsByOwner. DistributorID: " + distributorID, ex));

                ExpireAllContacts(distributorID);
                return null;
            }
        }

        public static List<Contact_V02> GetContactsByContactListIDs(string distributorID, List<int> contactListIDs)
        {
            using (var bwService = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new ListContactsByListIDsRequest_V01();

                request.DistributorID = distributorID;
                request.ContactListIDs = contactListIDs;

                try
                {
                    var response = bwService.ListContactsByContactListIDs(new ListContactsByContactListIDsRequest(request)).ListContactsByContactListIDsResult as ListContactsByListIDsResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Contacts;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, bwService);
                    return null;
                }
            }
        }

        public static int? GetContactIDByContactDistributorID(string distributorID, string contactDistributorID)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new GetContactByContactDistributorIDRequest_V01();

                request.ContactDistributorID = contactDistributorID;
                request.DistributorID = distributorID;
                try
                {
                    var response =
                        proxy.GetContactByContactDistributorID(new GetContactByContactDistributorIDRequest(request)).GetContactByContactDistributorIDResult as GetContactByContactDistributorIDResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.ContactID;
                    }
                    else
                    {
                        throw new Exception("GetContactByContactDistributorIDResponse status is failure.");
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        public static List<Contact_V03> FilterContacts(string DistributorID, ContactFilter_V01 contactFilter)
        {
            var contactList = GetContactsByOwner(DistributorID);

            return FilterContacts(DistributorID, contactFilter, contactList);
        }

        private static List<Contact_V03> FilterContacts(string DistributorID,
                                                        ContactFilter_V01 contactFilter,
                                                        List<Contact_V03> allContacts)
        {
            if (contactFilter == null)
                return new List<Contact_V03>();

            List<int> matchedContactIDs = null;

            if (ValidateContactFilter(contactFilter))
            {
                return allContacts;
            }

            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new ListContactsByFilterRequest_V01();

                request.DistributorID = DistributorID;
                request.ContactFilter = contactFilter;

                var response = proxy.ListContactsByFilter(new ListContactsByFilterRequest(request)).ListContactsByFilterResult as ListContactsByFilterResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    matchedContactIDs = response.ContactIDs;
                }
                else
                {
                    return null;
                }
            }

            if (allContacts == null)
                return null;

            if (matchedContactIDs.Count == 0)
                return new List<Contact_V03>();

            var result = new List<Contact_V03>();

            var filteredContacts = allContacts.Where(contact => matchedContactIDs.Contains(contact.ContactID));

            if (null != filteredContacts && filteredContacts.Count() > 0)
                result = filteredContacts.ToList();

            return result;
        }

        private static bool ValidateContactFilter(ContactFilter_V01 contactFilter)
        {
            if (string.IsNullOrEmpty(contactFilter.BirthDay) && string.IsNullOrEmpty(contactFilter.City) &&
                string.IsNullOrEmpty(contactFilter.ContactSource) && string.IsNullOrEmpty(contactFilter.ContactType) &&
                string.IsNullOrEmpty(contactFilter.Country) && string.IsNullOrEmpty(contactFilter.Email)
                && string.IsNullOrEmpty(contactFilter.FirstName) && string.IsNullOrEmpty(contactFilter.LastName) &&
                string.IsNullOrEmpty(contactFilter.Name) &&
                string.IsNullOrEmpty(contactFilter.State) && string.IsNullOrEmpty(contactFilter.Interest) &&
                string.IsNullOrEmpty(contactFilter.ZipCode) && string.IsNullOrEmpty(contactFilter.PhoneNumber) &&
                contactFilter.SearchByEmail == false)
            {
                return true;
            }
            return false;
        }

        public static Contact_V01 GetContactDetail(int contactID, out List<int> assignedLists)
        {
            var request = new GetContactRequest_V01 { ContactID = contactID };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.GetContact(new GetContactRequest1(request)).GetContactResult as GetContactResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        assignedLists = response.Lists;
                        return response.Contact as Contact_V01;
                    }
                    else
                    {
                        assignedLists = null;
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    assignedLists = null;
                    return null;
                }
            }
        }

        public static Contact_V01 SaveContact(Contact_V01 contact, string distributorID)
        {
            ExpireAllContacts(distributorID);

            var request = new SaveContactRequest_V01
            {
                Contact = contact,
                DistributorID = distributorID,
                IsImport = false
            };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.SaveContact(new SaveContactRequest1(request)).SaveContactResult as SaveContactResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Contact as Contact_V01;
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        /// <summary>
        ///     Save contact confirmation email
        /// </summary>
        /// <param name="contact">Contact saved</param>
        /// <param name="distributorId">Name of the owner of the site</param>
        /// ///
        /// <param name="extension">Website extension</param>
        /// ///
        /// <param name="locale">Customer locale</param>
        /// ///
        /// <param name="domain">Domain for this locale</param>
        /// ///
        /// <param name="primaryLocale"></param>
        /// <param name="registrationSource">Where did the customer got the invitation</param>
        /// <param name="distributorName"></param>
        /// <param name="distributorEmail"></param>
        /// <param name="isDistributorCopied"></param>
        /// <returns>True if success</returns>
        public static bool SendContactConfirmationEmail(
            Contact_V01 contact,
            string distributorId,
            string extension,
            string locale,
            string domain,
            String distributorName,
            String distributorEmail,
            Boolean isDistributorCopied,
            String primaryLocale,
            RegistrationSource registrationSource = RegistrationSource.Other
            )
        {
            if (contact.EmailAddresses.Count < 1)
            {
                throw new ArgumentException("Argument can not be null", "Contact Email");
            }

            if (contact.LocalName == null)
            {
                throw new ArgumentException("Argument can not be null", "Contact Name");
            }

            if (string.IsNullOrEmpty(contact.DistributorID))
            {
                throw new ArgumentException("Argument can not be null", "Distributor ID");
            }

            var request = new TriggeredSendRequestRequest_V01();
            request.Data = new TriggeredSaveContactConformationSendData_V01
            {
                CustomerEmail = contact.EmailAddresses[0].Address,
                CustomerName = contact.LocalName,
                CustomerPhoneNumber =
                    (contact.Phones.Count > 0) ? contact.Phones.FirstOrDefault().Number : String.Empty,
                DistributorEmail = distributorEmail,
                DistributorId = distributorId,
                DistributorName = distributorName,
                DataKey = Guid.NewGuid().ToString(),
                Locale = primaryLocale,
                Extension = extension,
                IsDistributorCopied = isDistributorCopied,
                Domain = domain,
                RegistrationSource = registrationSource,
                EmailLanguageLocale = locale
            };

            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.SendTriggeredMessage(new SendTriggeredMessageRequest(request));
                    return response.SendTriggeredMessageResult.Status == ServiceResponseStatusType.Success;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return false;
                }
            }
        }

        public static bool DeleteContact(int contactID, string distributorID)
        {
            ExpireAllContacts(distributorID);

            //TODO: verify distributor ID is owner?
            var request = new DeleteContactRequest_V01 { ContactID = contactID };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.DeleteContact(new DeleteContactRequest1(request));
                    if (response.DeleteContactResult != null && response.DeleteContactResult.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return false;
                }
            }
        }

        public static bool DeleteContacts(string distributorID, List<int> contactIDs)
        {
            ExpireAllContacts(distributorID);

            var request = new DeleteContactsByIDRequest_V01 { ContactIDs = contactIDs, DistributorID = distributorID };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.DeleteContactsByID(new DeleteContactsByIDRequest(request));
                    if (response.DeleteContactsByIDResult != null && response.DeleteContactsByIDResult.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return false;
                }
            }

            return false;
        }

        public static List<Contact_V03> GetDistributorByFilter(string distributorID, ContactFilter_V01 contactFilterV01)
        {
            var request = new ListContactsByFilterRequest_V01
            {
                ContactFilter = contactFilterV01,
                DistributorID = distributorID
            };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.ListDistributorByFilter(new ListDistributorByFilterRequest(request)).ListDistributorByFilterResult as ListContactsByOwnerResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.Contacts;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return new List<Contact_V03>();
                }
            }

            return new List<Contact_V03>();
        }

        public static List<string> AddDistributorsToContacts(string distributorID,
                                                             List<string> distributorIDs,
                                                             int importMode)
        {
            ExpireAllContacts(distributorID);

            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new AddDistributorsToContactsRequest_V01();

                request.DistributorID = distributorID;
                request.DistributorIDs = distributorIDs;
                request.ImportMode = importMode;

                try
                {
                    var response = proxy.AddDistributorsToContacts(new AddDistributorsToContactsRequest1(request)).AddDistributorsToContactsResult as AddDistributorsToContactsResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.MissingDistributors;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        #endregion Contacts

        #region ContactLists

        public static List<ContactListInfo_V01> GetContactListsByOwner(string distributorID, bool RetrieveCount)
        {
            try
            {
                return SimpleCache.Retrieve(
                    delegate
                    {
                        using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
                        {
                            var request = new ListContactListsByOwnerRequest_V01();

                            request.DistributorID = distributorID;
                            request.RetrieveCount = RetrieveCount;

                            var response =
                                proxy.ListContactListsByOwner(new ListContactListsByOwnerRequest(request)).ListContactListsByOwnerResult as ListContactListsByOwnerResponse_V01;
                            if (response != null && response.Status == ServiceResponseStatusType.Success)
                            {
                                return response.Lists;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    },
                    GetContactListCacheKey(distributorID, RetrieveCount), DefaultSimpleCacheDuration);
            }
            catch (Exception ex)
            {
                Log(
                    new ApplicationException(
                        "Error retrieving contact list in GetContactListssByOwner. DistributorID: " + distributorID, ex));

                ExpireAllContacts(distributorID);
                return null;
            }
        }

        public static ContactListInfo_V01 GetContactListByID(string distributorID, int contactListID, bool retrieveCount)
        {
            var lists = GetContactListsByOwner(distributorID, retrieveCount);

            return lists.SingleOrDefault(list => list.ID == contactListID);
        }

        public static int? AddContactsToList(string distributorID, ContactListInfo_V01 list, List<int> contacts)
        {
            ExpireAllContacts(distributorID);

            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new AddContactsToListRequest_V01();

                request.DistributorID = distributorID;
                request.ContactIDs = contacts;
                request.List = list;

                try
                {
                    var response = proxy.AddContactsToList(new AddContactsToListRequest1(request)).AddContactsToListResult as AddContactsToListResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.ListID;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        public static int? AddContactToList(string distributorID, int listID, int contactID)
        {
            ExpireAllContacts(distributorID);

            return AddContactsToList(distributorID,
                                     new ContactListInfo_V01 { ID = listID }
                                     , new List<int> { contactID });
        }

        public static int? AddContactsToNewList(string distributorID,
                                                List<int> ContactIDs,
                                                string newListName,
                                                string newListDescription)
        {
            if (String.IsNullOrEmpty(newListName))
                return null;

            return AddContactsToList(distributorID,
                                     new ContactListInfo_V01
                                     {
                                         ID = 0,
                                         Name = newListName,
                                         Description = newListDescription
                                     }
                                     , ContactIDs);
        }

        public static int? AddContactToNewList(string distributorID,
                                               int contactID,
                                               string newListName,
                                               string newListDescription)
        {
            return AddContactsToNewList(distributorID, new List<int> { contactID }, newListName, newListDescription);
        }

        public static int? CopyContactList(string distributorID, int sourceContactListID, string newListName)
        {
            if (sourceContactListID == 0)
                return null;

            ExpireAllContacts(distributorID);

            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new CopyContactListRequest_V01();

                request.DistributorID = distributorID;
                request.Name = newListName;
                request.SourceListID = sourceContactListID;

                try
                {
                    var response = proxy.CopyContactList(new CopyContactListRequest1(request)).CopyContactListResult as CopyContactListResponse_V01;
                    if (response != null && response.Status == ServiceResponseStatusType.Success)
                    {
                        return response.ListID;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return null;
                }
            }
        }

        public static void UpdateContactList(int contactListID, string distributorID, string name, List<int> contactIDs)
        {
            ExpireAllContacts(distributorID);

            using (var bwService = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                var request = new SaveContactListRequest_V01();

                request.DistributorID = distributorID;
                request.Name = name;
                request.ContactIDs = contactIDs;
                request.ContactListID = contactListID;

                try
                {
                    var response = bwService.SaveContactList(new SaveContactListRequest1(request)).SaveContactListResult as SaveContactListResponse_V01;
                }
                catch (Exception ex)
                {
                    Log(ex, bwService);
                }
            }
        }

        public static bool DeleteContactList(string distributorID, int contactListID)
        {
            ExpireAllContacts(distributorID);

            // TODO: verify distributor ID is owner?
            var request = new DeleteContactListRequest_V01 { ContactListID = contactListID };
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var response = proxy.DeleteContactList(new DeleteContactListRequest1(request));
                    if (response.DeleteContactListResult != null && response.DeleteContactListResult.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    return false;
                }
            }
        }

        #endregion ContactLists

        #region Other

        private static void Log(Exception ex)
        {
            WebUtilities.LogExceptionWithContext(ex);
        }

        private static void Log(Exception ex, DistributorCRMServiceClient dCrmService)
        {
            WebUtilities.LogServiceExceptionWithContext(ex, dCrmService);
        }

        /// <summary>
        ///     Forces a flush of all Contact related data from the cache.
        /// </summary>
        public static void ExpireAllContacts(string distributorID)
        {
            SimpleCache.Expire(typeof(List<Contact_V01>), distributorID);
            SimpleCache.Expire(typeof(List<Contact_V02>), distributorID);
            SimpleCache.Expire(typeof(List<Contact_V03>), distributorID);
            SimpleCache.Expire(typeof(List<ContactListInfo_V01>), GetContactListCacheKey(distributorID, true));
            SimpleCache.Expire(typeof(List<ContactListInfo_V01>), GetContactListCacheKey(distributorID, false));
        }

        public static string GetContactListCacheKey(string distributorID, bool includesCounts)
        {
            return distributorID + includesCounts.ToString();
        }

        #endregion Other
    }
}