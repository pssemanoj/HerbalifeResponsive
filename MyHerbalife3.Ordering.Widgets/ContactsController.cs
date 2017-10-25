using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Mvc;

namespace MyHerbalife3.Ordering.Widgets
{
    public class ContactsController : ApiController
    {
        private static IContactsSource _contactsSource;

        public static void Inject(IContactsSource contactsSource)
        {
            _contactsSource = contactsSource;
        }


        [WebApiCultureSwitching]
        [Authorize]
        public List<ContactViewModel> Get()
        {
            return Get(User.Identity.Name);
        }

        internal List<ContactViewModel> Get(string id)
        {
            try
            {
                var model =
                    _contactsSource.GetContacts(id);
                return model;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading contactsViewModel list for id" + id);
            }

            return null;
        }

        [WebApiCultureSwitching]
        [Authorize]
        public async Task<ContactResultModel> Post(GridPageModel data)
        {
            var memberId = User.Identity.Name;
            return await LoadContacts(memberId, data);
        }

        private Task<ContactResultModel> LoadContacts(string memberId, GridPageModel data)
        {
            return Task<ContactResultModel>.Factory.StartNew(() => DoLoadContacts(memberId, data));
        }

        private static ContactResultModel DoLoadContacts(string memberId, GridPageModel data)
        {
            try
            {
                if (null != data && null != data.filter && null != data.filter.Filters && data.filter.Filters.Any())
                {
                    var filter = data.filter.Filters.FirstOrDefault();

                    if (null != filter && null != filter.Filters && filter.Filters.Any())
                    {
                        var anyFilter = filter.Filters.FirstOrDefault();
                        if (null != anyFilter && null != anyFilter.Value)
                        {
                            var filteredContacts = _contactsSource.SearchContacts(memberId, anyFilter.Value.ToString());
                            if (null != filteredContacts && filteredContacts.Any())
                            {
                                return new ContactResultModel
                                {
                                    Items = filteredContacts.Skip(data.skip).Take(data.take).ToList(),
                                    TotalCount = filteredContacts.Count()
                                };
                            }
                            return new ContactResultModel { Items = new List<ContactViewModel>(), TotalCount = 0 };
                        }
                    }
                }
                var contacts = _contactsSource.GetContacts(memberId);

                if (null == contacts)
                {
                    LoggerHelper.Error("_contactsSource is returning null for " + memberId);
                    return new ContactResultModel
                    {
                        Items = new List<ContactViewModel>(),
                        TotalCount = 0
                    };
                }
                if (null == data)
                {
                    return new ContactResultModel { Items = contacts, TotalCount = contacts.Count };
                }
                return new ContactResultModel
                {
                    Items = contacts.Skip(data.skip).Take(data.take).ToList(),
                    TotalCount = contacts.Count
                };
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "DoLoadContacts ContactsController is throwing error for " + memberId);
                return new ContactResultModel
                {
                    Items = new List<ContactViewModel>(),
                    TotalCount = 0
                };
            }
        }
    }
}