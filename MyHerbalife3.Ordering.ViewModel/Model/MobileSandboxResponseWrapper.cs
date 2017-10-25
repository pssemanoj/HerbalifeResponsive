
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class MobileSandboxResponseWrapper : BaseResponseViewModel
    {
        public List<MobileSandboxResponseViewModel> SandboxEvents { get; set; }
        public MobileSandboxRequestViewModel Parameters { get; set; }
    }
}
