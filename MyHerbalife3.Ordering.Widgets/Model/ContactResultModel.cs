using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class ContactResultModel
    {
        public List<ContactViewModel> Items { get; set; }
        public int TotalCount { get; set; }
    }
}