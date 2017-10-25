using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HL.Common.ValueObjects;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class DiscontinuedSkuItemViewModel
    {
        public string DistributorId { get; set; }
        public string Locale { get; set; }
        public string ShoppingCartId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public OrderCategoryType CategoryType { get; set; }
    }
}
