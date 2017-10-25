using System;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class ContactViewModel
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string PhoneNumber { get; set; }
        public string MobilePhoneNumber { get; set; }
        public string PrimaryEmailAddress { get; set; }
        public string SecondaryEmailAddress { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Notes { get; set; }
        public ContactOrigin ContactSource { get; set; }
        public ContactType ContactType { get; set; }
    }
}