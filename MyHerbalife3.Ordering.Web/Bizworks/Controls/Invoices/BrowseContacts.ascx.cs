using System;
using System.Collections.Generic;
using System.Web.Security;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.SharedProviders.Bizworks;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class BrowseContacts : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadContacts();
        }

         protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs command = args as CommandEventArgs;

            if (command != null)
            {
                switch (command.CommandName)
                {
                    case "LoadContacts":

                        ContactFilter_V01 searchCriteria = contactsBasicSearch.GetContactSearchCriteria();
                        if (searchCriteria != null)
                        {
                            OnSearch(searchCriteria);
                        }
                        else
                        {
                            LoadContacts();
                        }

                        ContactsListView1.DataBind();
                        break;
                    case "BasicSearch":
                        OnSearch((ContactFilter_V01)command.CommandArgument);
                        ContactsListView1.DataBind();
                        break;
                    case "ClearSearch":
                        //begin - shan - mar 20, 2012 - clear the session value
                        //else it is pre-select the items whenever clear button is clicked
                        Session["ListDelete"] = null;
                        //end
                        LoadContacts();
                        ContactsListView1.DataBind();
                        break;
                    case "SelectAll":
                        ContactsListView1.SelectAll(bool.Parse(command.CommandArgument.ToString()));
                        break;
                    default:
                        break;
                }
            }

            return base.OnBubbleEvent(source, args);
        }

         public string DistributorID
         {
             get { return OnlineDistributor.Id; }
         }
         public DistributorProfileModel OnlineDistributor
         {
             get { return ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value; }
         }

        protected void OnSearch(ContactFilter_V01 contactsSearchCriteria)
        {
            if (contactsSearchCriteria != null)
            {
                List<Contact_V03> filteredContacts = ContactsDataProvider.FilterContacts(DistributorID, contactsSearchCriteria);
                if (null != filteredContacts)
                {
                    ContactsListView1.OnContactsLoaded(filteredContacts, new List<string>() { "FollowUp", "FullNameNotLinked", "CreatedDate", "Fullname", "Email", "ContactSource" });
                }
            }
            else
            {
                LoadContacts();
            }
        }

        protected virtual void LoadContacts()
        {
            List<Contact_V03> Contacts = ContactsDataProvider.GetContactsByOwner(DistributorID);
            ContactsListView1.OnContactsLoaded(Contacts, new List<string>() { "FollowUp", "FullNameNotLinked", "CreatedDate", "Fullname", "Email", "ContactSource" });
        }

        void DeleteContacts()
        {

            List<int> checkedcontacts = ContactsListView1.GetCheckedContactIDs();

            if (checkedcontacts.Count == 0)
                ShowMsg("PleaseSelectContact", true);

            bool success = DeleteContacts(checkedcontacts);

            if (success)
            {
                LoadContacts();
                DataBind();
            }
            else
            {
                ShowMsg("FailedRemoveContacts", true);
            }
        }

        protected virtual bool DeleteContacts(List<int> checkedcontacts)
        {
            bool success = ContactsDataProvider.DeleteContacts(DistributorID, checkedcontacts);
            return success;
        }

        protected void ShowMsg(string messageKey, bool isError)
        {

        }

        protected void DeleteChecked_click(object sender, EventArgs e)
        {
            DeleteContacts();
        }

        public event EventHandler onAddToContactClick;

        protected void OnInvoiceContactClicked(object sender, EventArgs e)
        {
            var assignedIds = new List<int>();
            var checkedcontacts = ContactsListView1.GetCheckedContactIDs();   
            if(null != checkedcontacts && checkedcontacts.Count >0)
            {
                var contact = ContactsDataProvider.GetContactDetail(checkedcontacts[0], out assignedIds);

                if (null != contact)
                {
                    if (onAddToContactClick != null)
                    {
                        onAddToContactClick(this, new CommandEventArgs("InvoiceContact", contact));
                    }
                }
            }

            Session["Selected"] = null;
            ContactsListView1.ResetSelectItem();
        }

        protected  void CloseContacts_OnClick(object sender, EventArgs e)
        {
            Session["Selected"] = null;
            ContactsListView1.ResetSelectItem();
        }
    }
}
