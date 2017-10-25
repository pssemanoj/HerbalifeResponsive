#region

using System.Collections.Generic;
using System.Linq;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileAnnouncementProvider : IMobileAnnouncementProvider
    {
        public AnnouncementResponseViewModel GetAnnouncement(AnnouncementRequestViewModel request)
        {
            var response = new AnnouncementResponseViewModel();

            #region validation

            if (request == null)
            {
                response.Announcements = null;
                return response;
            }

            #endregion

            var anouncements = China.CatalogProvider.GetChinaAnnouncementInfo(request.From, request.To,
                request.Locale);
            if (anouncements == null) return response;

            response.Announcements = new List<AnnouncementViewModel>();

            foreach (var model in from AnnouncementInfo_V01 announcement in anouncements
                where announcement != null
                select new AnnouncementViewModel
                {
                    Id = announcement.Id,
                    Title = announcement.Title,
                    Content = HTMLHelper.ToText(announcement.AnnouncementDesc.Replace("<br />","\n")),
                    CreatedDate = announcement.CreatedDate,
                    LastUpdatedDate = announcement.UpdatedDate,
                    BeginDate = announcement.BeginDate,
                    EndDate = announcement.EndDate,
                    IsMarkedAsNew = (announcement.IsUseNew != null && announcement.IsUseNew.HasValue && announcement.IsUseNew.Value ==1) 
                })
            {
                response.Announcements.Add(model);
            }

            return response;
        }
    }
}