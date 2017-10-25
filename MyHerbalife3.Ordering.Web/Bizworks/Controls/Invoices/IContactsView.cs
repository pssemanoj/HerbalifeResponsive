using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;
using System.Collections.Generic;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public interface IContactsView
    {
        List<int> GetCheckedContactIDs();

        List<string> GetCheckedContactEmails();
      
        void OnListsLoaded(List<ContactListInfo_V01> lists);

        void OnContactsLoaded(List<Contact_V03> contacts,List<string> fieldsToHide);

        void OnContactDetailsLoaded(Contact_V01 contactWithDetails, List<int> assignedLists);

        void OnAddingNewContact();

        bool HasEditor();

        void SelectAll(bool selected);

        void DoExport(string exportType);
            
    }
}
