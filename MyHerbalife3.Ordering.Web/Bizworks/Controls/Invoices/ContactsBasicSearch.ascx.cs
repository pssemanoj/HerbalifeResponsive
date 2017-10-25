using System;
using System.Web.UI.WebControls;
using System.ComponentModel;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;


namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class ContactsBasicSearch : System.Web.UI.UserControl
    {       
        public bool Enabled
        {
            get { return BasicSearchPanel.Enabled; }
            set
            {
                if (value == false)
                {
                    SearchContactsDropDown.SelectedIndex = -1;
                    SearchField.Text = "";
                }
                BasicSearchPanel.Enabled = value;
            }
        }

        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Hides/Shows the advanced search link")]
        public bool ShowAdvancedSearchLink
        {
            get { return AdvancedSearchLinkButton.Visible; }
            set { AdvancedSearchLinkButton.Visible = value; }
        }

        /// <summary>
        /// on page load.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Event Click handler for the Search button
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        protected void BasicSearch_click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(SearchField.Text.Trim()))
            {
                ContactFilter_V01 searchCriteria = GetContactSearchCriteria();

                //used in MyContactsBasic page
                RaiseBubbleEvent(this, new CommandEventArgs("BasicSearch", searchCriteria));
               
            }
            else
            {
                ClearSearch_click(sender, e);
            }
        }

        /// <summary>
        /// Event Click handler for the Search button
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="e">EventArgs</param>
        protected void ClearSearch_click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            SearchContactsDropDown.SelectedIndex = -1;
            SearchField.Text = "";

            RaiseBubbleEvent(this, new CommandEventArgs("ClearSearch", null));

        }


        /// <summary>
        /// Method to construct the search criteria
        /// </summary>
        /// <returns>ContactsSearchCriteria</returns>
        public ContactFilter_V01 GetContactSearchCriteria()
        {
            if (Enabled == false ||
                SearchContactsDropDown.SelectedIndex < 0 ||
                String.IsNullOrEmpty(SearchField.Text))
                return null;

            ContactFilter_V01 searchCriteria = new ContactFilter_V01();


            if (SearchContactsDropDown.SelectedValue == "NameContains")
            {
                searchCriteria.NameSearchTypeInd = ContactNameSearchType.NameContains;
            }
            else if (SearchContactsDropDown.SelectedValue == "FirstNameStartsWith")
            {
                searchCriteria.NameSearchTypeInd = ContactNameSearchType.FirstNameStartsWith;
            }
            else
            {
                searchCriteria.NameSearchTypeInd = ContactNameSearchType.LastNameStartsWith;
            }
            searchCriteria.Name = SearchField.Text.Trim();
            searchCriteria.IsAdvancedSearch = false;
            return searchCriteria;
        }

        protected override bool OnBubbleEvent(object source, EventArgs args)
        {
            CommandEventArgs command = args as CommandEventArgs;

            if (command != null)
            {
                switch (command.CommandName)
                {
                    
                    case "ShowAdvancedSearch":
                        Clear();
                        break;
                   
                    default:
                        break;

                }
            }

            return base.OnBubbleEvent(source, args);
        }

    }
}
