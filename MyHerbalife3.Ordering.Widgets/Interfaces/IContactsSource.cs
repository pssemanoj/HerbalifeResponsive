using System.Collections.Generic;
using MyHerbalife3.Ordering.Widgets.Model;

namespace MyHerbalife3.Ordering.Widgets.Interfaces
{
    public interface IContactsSource
    {
        List<ContactViewModel> GetContacts(string id);
        IEnumerable<ContactViewModel> SearchContacts(string id, string filter);
    }
}