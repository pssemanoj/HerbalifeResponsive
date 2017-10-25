
using HL.Blocks.CircuitBreaker;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CRMContactSVC;
using MyHerbalife3.Ordering.ViewModel.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers
{
    public class DistributorContactProvider
    {
        private static string CustomerDirectContactTypes = Settings.GetRequiredAppSetting("CustomerDirectContactTypes", "MB,OTHER,CUS,DS,CS,UNS,LD");
        public static List<InvoiceCRMConatactModel> GetCustomersFromService(List<string> memberIds,string CountryCode)
        {
            var contacts = new List<InvoiceCRMConatactModel>();
            var proxy = ServiceClientProvider.GetCRMContactServiceProxy();
            
            try
            {
                var getCrmContactForMultipleDistributorsRequest = new GetCrmContactForMultipleDistributorsRequest
                {
                    DetailLevel = (ContactListDetailLevel)Enum.Parse(typeof(ContactListDetailLevel), "2"),
                    SearchDescriptor = new ContactSearchFilterDescriptor
                    {
                        Filters = CreateContactFilters(),
                    },
                    DistributorIds = memberIds,
                    ShowMultipleAddress = true,
                    Take = 1000,
                   
                  
                };

                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<GetCrmContactsForMultipleDistributorsResponse>();
                var response =  circuitBreaker.Execute(() =>proxy.GetContacts(new GetContactsRequest(getCrmContactForMultipleDistributorsRequest)).GetContactsResult as GetCrmContactsForMultipleDistributorsResponse);

                if (response == null)
                {
                    LoggerHelper.Warn($"Get customer for ds {string.Join(",", memberIds)} failed: response is null.");
                    return null;
                }

                if (response.Contacts != null)
                {
                    contacts = PopulateCustomerList(response.Contacts.ToList(),CountryCode);
                }

                return contacts;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("GetContacts  error : {0}", ex.Message));
            }
            finally
            {
                proxy.Close();
            }
            return contacts;
        }
        private static  List<KeyValuePairOfstringstring> CreateContactFilters()
        {
            return new List<KeyValuePairOfstringstring>
                         {
                             new KeyValuePairOfstringstring { key = "ContactTypeCode", value = CustomerDirectContactTypes },
                             
                         };            
        }
        private static List<InvoiceCRMConatactModel> PopulateCustomerList(List<CrmContactBase> contacts,string CountryCode)
        {
            var contactList = (from c in contacts

                               select new InvoiceCRMConatactModel
                               {
                                   ContactId = c.ContactId !=null? int.Parse(c.ContactId):0,
                                   Address = ConvertToAddressViewModel((c as CrmContact).Addresses !=null?(c as CrmContact).Addresses.FirstOrDefault():new Address_V03()),
                                   EmailDetail = new InvoiceEmailAddressDetail
                                   {
                                       Id = ((c as CrmContact).EmailAddresses !=null ?int.Parse(((c as CrmContact).EmailAddresses.FirstOrDefault() as ContactEmailAddress).EmailId):0),
                                       EmailAddress = ((c as CrmContact).EmailAddresses != null?((c as CrmContact).EmailAddresses.FirstOrDefault() as ContactEmailAddress).Address:string.Empty)
                                   },
                                   FirstName = (c as CrmContact).Name !=null?(c as CrmContact).Name.First:"",
                                   MiddleName = (c as CrmContact).Name != null ? (c as CrmContact).Name.Middle:"",
                                   LastName = (c as CrmContact).Name != null ? (c as CrmContact).Name.Last:"",
                                   PhoneDetail = new InvoicePhoneDetail()
                                   {
                                       Id = (c as CrmContact).Phones !=null? int.Parse((c as CrmContact).Phones.FirstOrDefault().Id):0,
                                       PhoneNumber = (c as CrmContact).Phones != null ?(c as CrmContact).Phones.FirstOrDefault().Number.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", ""):"",
                                   },
                               }).ToList();


            try
            {
                if (contactList != null && contactList.Any())
                {
                    var blankContacts = contactList.FindAll(oi => oi.ContactId == 0);
                    if (blankContacts != null && blankContacts.Any())
                        for (int i = 0; i < blankContacts.Count; i++)
                        {
                            var contactHasAddress = contactList.FindAll(oi => oi.ContactId == blankContacts[i].ContactId);
                            if (contactHasAddress != null && contactHasAddress.Count > 1)
                            {
                                contactList.Remove(blankContacts[i]);
                            }
                        }
                }
                if (contactList != null && contactList.Any()  && (CountryCode.ToUpper()=="GB" || CountryCode.ToUpper() == "KR"))
                {
                    contactList.RemoveAll(oi => oi.Address != null && !string.IsNullOrEmpty(oi.Address.Country) && oi.Address.Country.ToUpper() != CountryCode);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Info(ex.Message);
            }
            return contactList;
        }

        private static InvoiceAddressModel ConvertToAddressViewModel(Address_V03 addressList   )
        {
            var Address = addressList as ContactAddress;
            if (Address == null)
                return null;
            var address = new InvoiceAddressModel()
            {
                Id = Address.AddressId!=null?  int.Parse(Address.AddressId):0,
                Address1 = (!string.IsNullOrEmpty(Address.Line2)) ? string.Concat((Address.Line1 ?? string.Empty).Trim(), " ", (Address.Line2 ?? string.Empty).Trim()) : Address.Line1,
                Address2 = Address.Line2,
                City = Address.City,
                State = Address.StateProvinceTerritory,
                Country = Address.Country,
                PostalCode = Address.PostalCode

            };
            return address;
        }

        public static SaveUpdateResponseModel SaveUpdate(InvoiceCRMConatactModel contact, string memberId)
        {
            return contact.ContactId == 0 ? Create(contact,memberId) : Update(contact,memberId);
        }


        private static SaveUpdateResponseModel Create(InvoiceCRMConatactModel contact,string memberId)
        {
            var proxy = ServiceClientProvider.GetCRMContactServiceProxy();
            try
            {
                var createCrmContactRequest = new CreateCrmContactRequest();
                createCrmContactRequest.OwnerId = memberId;
                createCrmContactRequest.IsContactInformationRequired = true;

                CrmContact crmContact = new CrmContact();
                crmContact.ContactId = null;
                crmContact.ContactType = CrmContactType.Customer;
                crmContact.ContactPriority = ContactPriorityType.None;
                crmContact.ContactSource = ContactSourceType.ECommerce;
                crmContact.ContactStatus = ContactStatusType.None;
                crmContact.OwnerMemberId = memberId;

                crmContact.IsDuplicateNameCheckRequired = contact.IsDuplicateCheckRequire;

                crmContact.EmailAddresses = contact.EmailDetail != null && string.IsNullOrEmpty(contact.EmailDetail.EmailAddress) ? null : new List<EmailAddress_V01> { new ContactEmailAddress { EmailId = string.Empty, Address = contact.EmailDetail.EmailAddress, IsPrimary = true } };

                crmContact.FollowupAppointments = new List<FollowupAppointment>()
                        ;
                crmContact.Addresses = contact.Address == null 
                    ? null
                    : new List<Address_V03>() {
                        new ContactAddress
                        {
                            AddressId = null,
                            Line1 = contact.Address.Address1,
                            City = contact.Address.City,
                            Line2 = contact.Address.Address2,
                            StateProvinceTerritory = contact.Address.State,
                            PostalCode = contact.Address.PostalCode,
                            Country=contact.Address.Country,
                            IsPrimary = true,
                            TypeOfAddress = AddressType.ShipTo
                        }
                };

                crmContact.Name = new Name_V01
                {
                    First = contact.FirstName,
                    Last = contact.LastName,
                };

                crmContact.Phones = contact.PhoneDetail == null || string.IsNullOrEmpty(contact.PhoneDetail.PhoneNumber)
                    ? null
                    : new List<CrmContactPhone>()
                    {
                        new CrmContactPhone
                        {
                            Id = string.Empty,
                            IsPrimary = true,
                            Number = contact.PhoneDetail.PhoneNumber,
                            PhoneType =CrmContactPhoneType.Mobile
                        }
                    };
                createCrmContactRequest.Value = crmContact;

                var circuitBreaker =
                    CircuitBreakerFactory.GetFactory().GetCircuitBreaker<CreateCrmContactResponse>();
                var response = circuitBreaker.Execute(() => proxy.CreateContact(new CreateContactRequest(createCrmContactRequest)).CreateContactResult as CreateCrmContactResponse);


                if (response == null)
                {
                    LoggerHelper.Warn($"Create customer for ds {memberId} failed: response is null.");
                    return null;
                }
                return createContactResponseModel(response, contact);
            }
            catch (Exception ex)
            {
                LoggerHelper.Info($"ContactSaver:Create Failed. message: {ex.Message}, exception: {ex}");
                SaveUpdateResponseModel errorModel = new SaveUpdateResponseModel();
                return errorModel;
            }
            finally
            {
                proxy.Close();
            }
        }
        private static SaveUpdateResponseModel Update(InvoiceCRMConatactModel contact, string memberId)
        {

            var proxy = ServiceClientProvider.GetCRMContactServiceProxy();
            try
            {
                var hasAddressChanges = (contact.Address != null && contact.Address.Id > 0);
                var hasPhoneChanges = (contact.PhoneDetail != null && contact.PhoneDetail.Id > 0) || (contact.PhoneDetail != null && !string.IsNullOrEmpty(contact.PhoneDetail.PhoneNumber));
                var hasEmailChanges = (contact.EmailDetail != null && contact.EmailDetail.Id > 0) || (contact.EmailDetail != null && !string.IsNullOrEmpty(contact.EmailDetail.EmailAddress));

                var updatingField = new List<string>();
                updatingField.Add("FirstName");
                updatingField.Add("LastName");

                if (hasAddressChanges)
                {
                    updatingField.Add("Addresses");
                }

                if (hasPhoneChanges)
                {
                    updatingField.Add("Phones");
                }

                if (hasEmailChanges)
                {
                    updatingField.Add("EmailAddress");
                }

                var crmContactUpdateRequest = new CrmContactUpdateRequest
                {
                    Fields = updatingField,
                    Value = new CrmContact
                    {
                        EmailAddresses = hasEmailChanges ? new List<EmailAddress_V01>() { new ContactEmailAddress { IsPrimary = true, EmailId = contact.EmailDetail.Id.ToString(), Address = contact.EmailDetail.EmailAddress} } : null,
                        Addresses = hasAddressChanges ? 
                        new List<Address_V03>()
                        {
                              new ContactAddress
                              {
                                 AddressId = contact.Address.Id <= 0 ? null : contact.Address.Id.ToString(),
                                 City = string.IsNullOrEmpty(contact.Address.City)?string.Empty:contact.Address.City,
                                 Country = string.IsNullOrEmpty(contact.Address.Country)?string.Empty:contact.Address.Country,
                                 Line1 =string.IsNullOrEmpty(contact.Address.Address1)?string.Empty:contact.Address.Address1,
                                 Line2 = string.IsNullOrEmpty(contact.Address.Address2)?string.Empty:contact.Address.Address2,
                                 StateProvinceTerritory = string.IsNullOrEmpty(contact.Address.State)?string.Empty:contact.Address.State,
                                 PostalCode =string.IsNullOrEmpty(contact.Address.PostalCode)?string.Empty:contact.Address.PostalCode,
                                 TypeOfAddress = AddressType.ShipTo,
                              } 
                              
                        } : null,
                        Name = new Name_V01 { First = contact.FirstName, Last = contact.LastName},
                        Phones = hasPhoneChanges ? new List<CrmContactPhone>() { new CrmContactPhone { IsPrimary = true, Number = contact.PhoneDetail.PhoneNumber, PhoneType = CrmContactPhoneType.Mobile, Id = contact.PhoneDetail.Id.ToString()=="0" ? null : contact.PhoneDetail.Id.ToString() } } : null,
                        ContactType = CrmContactType.Customer,
                        OwnerMemberId = memberId,
                        ContactId = contact.ContactId.ToString(),
                        IsDuplicateNameCheckRequired = false,
                        FollowupAppointments = new List<FollowupAppointment>(),
                    }
                };
                var circuitBreaker =
                   CircuitBreakerFactory.GetFactory().GetCircuitBreaker<UpdateContactResponse>();
                var response = circuitBreaker.Execute(() => proxy.UpdateContact(new UpdateContactRequest(crmContactUpdateRequest)).UpdateContactResult);


                if (response == null)
                {
                    LoggerHelper.Warn($"Create customer for ds {memberId} failed: response is null.");
                    return null;
                }
                return new SaveUpdateResponseModel { Data = contact};
            }
            catch (Exception ex)
            {
                LoggerHelper.Warn("ContactSaver Update : " + ex.StackTrace.ToString());
                throw;
            }
            finally
            {
                proxy.Close();
            }
        }


        private static SaveUpdateResponseModel createContactResponseModel(CreateCrmContactResponse contactBase, InvoiceCRMConatactModel contact)
        {
            var response = new SaveUpdateResponseModel();
            response.Data = contact;
            if (contactBase.ContactID != null && contactBase.Contact != null)
            {
                var crmContact = contactBase.Contact as CrmContact;
                if (crmContact != null)
                {
                    response.Data.ContactId = string.IsNullOrEmpty(crmContact.ContactId) ? 0 : int.Parse(crmContact.ContactId);

                    if (crmContact.Addresses != null && crmContact.Addresses.Count > 0)
                    {
                        response.Data.Address.Id = string.IsNullOrEmpty((crmContact.Addresses.First() as ContactAddress).AddressId) ? 0 : int.Parse((crmContact.Addresses.First() as ContactAddress).AddressId);
                    }
                    if (crmContact.EmailAddresses != null && crmContact.EmailAddresses.Count > 0)
                    {
                        response.Data.EmailDetail.Id = string.IsNullOrEmpty((crmContact.EmailAddresses.First() as ContactEmailAddress).EmailId) ? 0 : int.Parse((crmContact.EmailAddresses.First() as ContactEmailAddress).EmailId);
                    }
                    if (crmContact.Phones != null && crmContact.Phones.Count > 0)
                    {
                        response.Data.PhoneDetail.Id = string.IsNullOrEmpty((crmContact.Phones.First() as CrmContactPhone).Id) ? 0 : int.Parse((crmContact.Phones.First() as CrmContactPhone).Id);
                    }
                }
            }
            else
            {
                response.DuplicatedContacts = (from c in contactBase.DuplicateContacts
                                               select new InvoiceCRMConatactModel
                                               {
                                                   ContactId = c.ContactId != null ? int.Parse(c.ContactId) : 0,
                                                   FirstName = (c as CrmContact).Name != null ? (c as CrmContact).Name.First : "",
                                                   LastName = (c as CrmContact).Name != null ? (c as CrmContact).Name.Last : "",
                                                   Address = ConvertToAddressViewModel((c as CrmContact).Addresses != null ? (c as CrmContact).Addresses.FirstOrDefault() : new Address_V03()) == null ? new InvoiceAddressModel() : ConvertToAddressViewModel((c as CrmContact).Addresses != null ? (c as CrmContact).Addresses.FirstOrDefault() : new Address_V03()),
                                                   PhoneDetail = new InvoicePhoneDetail()
                                                   {
                                                       Id = (c as CrmContact).Phones != null ? int.Parse((c as CrmContact).Phones.FirstOrDefault().Id) : 0,
                                                   },
                                               }).ToList();

            }
            return response;
        }

    }
}
