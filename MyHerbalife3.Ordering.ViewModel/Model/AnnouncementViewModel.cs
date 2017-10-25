#region

using System;
using Newtonsoft.Json;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class AnnouncementViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int Id { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? CreatedDate { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? LastUpdatedDate { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? BeginDate { get; set; }

        [JsonConverter(typeof (CustomDateTimeConverter))]
        public DateTime? EndDate { get; set; }

        public bool IsMarkedAsNew { get; set; }
    }
}