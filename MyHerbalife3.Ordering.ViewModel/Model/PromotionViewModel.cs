#region

using Newtonsoft.Json;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class PromotionViewModel
    {
        public string Sku { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public int Quantity { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }

        [JsonIgnore]
        public string Action { get; set; }
    }

    public class PromotionMessage
    {
        public string Message { get; set; }
        public string Type { get; set; }
    }
}