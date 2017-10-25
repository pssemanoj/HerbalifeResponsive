#region

using System;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class AnnouncementRequestViewModel
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Locale { get; set; }
    }
}