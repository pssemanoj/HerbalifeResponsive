using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets
{
    public class ContactsSourceStandIn : IContactsSource
    {
        private readonly List<ContactViewModel> _contactViewModels;

        public ContactsSourceStandIn()
        {
            _contactViewModels = new List<ContactViewModel>
            {
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "KV",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "MJ@gmail.com",
                    FirstName = "MR",
                    LastName = "AJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                },
                new ContactViewModel
                {
                    City = "Torrance",
                    Country = "US",
                    PrimaryEmailAddress = "sk@gmail.com",
                    FirstName = "SK",
                    LastName = "MJ",
                    PhoneNumber = "3100000000",
                    PostalCode = "90502",
                    State = "CA",
                    StreetAddress1 = "950 W 190th st"
                }

            };
        }

        public List<ContactViewModel> GetContacts(string id)
        {
            return _contactViewModels;
        }

        public IEnumerable<ContactViewModel> SearchContacts(string id, string filter)
        {
            return string.IsNullOrEmpty(filter)
                ? _contactViewModels
                : _contactViewModels.Where(c => (c.FirstName + c.LastName).Contains(filter));
        }
    }
}