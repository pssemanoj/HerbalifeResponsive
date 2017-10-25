using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model.BackOrderDetail
{
    public class BackOrderDetailsViewModel
    {
        public List<BackOrderSKUDetailViewModel> FullCatalog { get; set; }

        public List<WarehouseDetails> WhDetails { get; set; }

        public bool HasShipping { get; set; }
    }


    public class WarehouseDetails
    {
        public string Name { get; set; }
        public string Option { get; set; }
        public string OptionText { get; set; }
        public string WHCode { get; set; }
    }

}
