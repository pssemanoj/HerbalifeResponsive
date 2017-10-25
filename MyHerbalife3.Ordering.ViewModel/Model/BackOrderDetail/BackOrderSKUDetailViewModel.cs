using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model.BackOrderDetail
{
    public class BackOrderSKUDetailViewModel
    {
        public string SKU { get; set; }

        public string SKUDescription { get; set; }

        public List<BackOrderLocationViewModel> InventoryDetails { get; set; }
        
    }
}
