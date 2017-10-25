using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class ApfViewModel
    {
        public string Sku { get; set; }  //sku name
        public string Name { get; set; }  // Rule Name eg. China - APF rule
        public string Type { get; set; }  // ApfSkuCannotBeRemoved, ApfSkuCanBeRemoved, 
        public string Action { get; set; } // Display message
        public string Message { get; set; } //Apf Due
        public bool ApfOptional { get; set; }
    }
}
