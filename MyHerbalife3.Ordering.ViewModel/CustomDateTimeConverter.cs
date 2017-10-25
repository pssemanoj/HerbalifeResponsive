#region

using Newtonsoft.Json.Converters;

#endregion

namespace MyHerbalife3.Ordering.ViewModel
{
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            base.DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        }
    }
}