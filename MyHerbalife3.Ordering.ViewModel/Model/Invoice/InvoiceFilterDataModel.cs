using System;
using System.Collections.Generic;
using System.Linq;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class InvoiceFilterDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
    }

    public enum InvoiceFilterCriteria
    {
        FirstName = 1,
        LastName = 2,
        InvoiceNumber = 3,
        VolumePoints = 4,
        Total = 5
    }

  
}