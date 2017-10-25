using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
   public class InvoiceCRMConatactModel: ICloneable
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public InvoiceAddressModel Address { get; set;}
        public InvoiceEmailAddressDetail EmailDetail { get; set; }
        public InvoicePhoneDetail PhoneDetail { get; set; }

        public bool  IsDuplicateCheckRequire { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    public class InvoiceEmailAddressDetail:ICloneable
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
    public class InvoicePhoneDetail:ICloneable
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class SaveUpdateResponseModel : ICloneable
    {
        public List<InvoiceCRMConatactModel> DuplicatedContacts { get; set; }

        public InvoiceCRMConatactModel Data { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
