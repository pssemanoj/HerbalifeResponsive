using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Widgets.Model
{
    public class GridPageModel
    {
        public int take { get; set; }
        public int skip { get; set; }
        public Filter filter { get; set; }
    }

    public class Filter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public string Logic { get; set; }
        public IEnumerable<Filter> Filters { get; set; }
    }
}