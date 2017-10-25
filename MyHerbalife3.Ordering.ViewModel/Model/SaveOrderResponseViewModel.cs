using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class SaveOrderResponseViewModel
    {
        public OrderViewModel Data { get; set; }
        public ErrorViewModel Error { get; set; }
    }
}
