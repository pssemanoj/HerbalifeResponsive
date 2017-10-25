using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Interfaces
{
    public interface IMobileAnnouncementProvider
    {
        AnnouncementResponseViewModel GetAnnouncement(AnnouncementRequestViewModel request);
    }
}
