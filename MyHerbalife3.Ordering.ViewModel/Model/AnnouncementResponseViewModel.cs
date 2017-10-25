#region

using System.Collections.Generic;

#endregion

namespace MyHerbalife3.Ordering.ViewModel.Model
{
    public class AnnouncementResponseViewModel : MobileResponseViewModel
    {
        public List<AnnouncementViewModel> Announcements { get; set; }
    }
}