using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Configuration;
using MyHerbalife3.Ordering.SharedProviders.Bizworks;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using Telerik.Web.UI;
using MyHerbalife3.Shared.UI.Helper;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices
{
    public partial class ContactsListView : MyHerbalife3.Shared.UI.UserControlBase, IContactsView
    {
        private const string UNICODE_CSS_FONT_FAMILIES = "Arial Unicode MS";
        public bool IsLeadView { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public int maxMailsCount { get { return _maxMailsCount.Value; } }
        private List<Contact_V03> MyContacts = new List<Contact_V03>();
        public bool selectAllBool = false;
        private GridSortExpression sortExpression = null;
        bool SelectAllItems = false;

        private ArrayList selectedItems = new ArrayList();
            
        protected bool IsSelectAllChecked()
        {
            return SelectAllItems;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session["MyContacts"] = null;
                Session["ListDelete"] = null;
                Session["Selected"] = null;
            }
        }
      



        protected void ContactsRadGrid_SortCommand(object source, GridSortCommandEventArgs e)
        {
            RememberSelected();
            sortExpression = new GridSortExpression();
            sortExpression.FieldName = e.SortExpression;
            sortExpression.SortOrder = e.NewSortOrder;

            RaiseBubbleEvent(this, new CommandEventArgs("LoadContacts", null));
        }

        


        protected void ContactsRadGrid_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int contactID = (int)(e.Item as GridDataItem).GetDataKeyValue("ContactID");
                RaiseBubbleEvent(this, new CommandEventArgs("ViewDetails", contactID));
            }
            else if (e.CommandName == "Edit" || e.CommandName == "Cancel")//used to open/close the followup form
            {
                RaiseBubbleEvent(this, new CommandEventArgs("LoadContacts", null));
            }
            else if (e.CommandName == "EmailClicked")
            {
                RaiseBubbleEvent(this, new CommandEventArgs("SendEmail", (e.Item as GridDataItem).GetDataKeyValue("ContactID").ToString()));
            }
            else if (e.CommandName == "ReinviteClicked")
            {
                bool isSuccess = false;
                isSuccess = ECardsProvider.SendCancelOptOutMessage(CultureInfo.CurrentCulture.Name
                                                    , ((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.Id
                                                    , (e.Item as GridDataItem).GetDataKeyValue("PrimaryEmailAddress").ToString()
                                                    , string.Format("https://{0}/OptOut.aspx?{1}={{0}}", Request.Url.Host, "re"));

                //TODO: Should handle if isSuccess == false

            }
        }


        protected void ContactsRadGrid_PreRender(object sender, EventArgs e)
        {
            if (IsCardView())
            {

                int itemCount = (sender as RadGrid).MasterTableView.GetItems(GridItemType.Item).Length + (sender as RadGrid).MasterTableView.GetItems(GridItemType.AlternatingItem).Length;
                foreach (GridItem item in (sender as RadGrid).Items)
                {
                    if (item is GridDataItem && item.ItemIndex < itemCount - 1)
                    {
                        ((item as GridDataItem)["ContactID"] as TableCell).Controls.Add(new LiteralControl("<table style=\"float:left;display:none;\"><tr><td>"));
                    }
                }
                List<Contact_V03> ListDelete = new List<Contact_V03>();
                if (Session["ListDelete"] != null)
                    ListDelete = (List<Contact_V03>)Session["ListDelete"];
                if (selText.Value == "true")
                {
                    foreach (GridDataItem grdItem in ContactsRadGrid.Items)
                    {
                        CheckBox cb = (CheckBox)(grdItem.FindControl("SelectorCheckBox"));
                        if (null != cb)
                        {
                            cb.Checked = true;
                        }
                    }
                }
                else
                {
                    foreach (GridDataItem grdItem in ContactsRadGrid.Items)
                    {
                        CheckBox cb = (CheckBox)(grdItem.FindControl("SelectorCheckBox"));
                        if (null != cb)
                        {
                            cb.Checked = false;
                            foreach (Contact_V03 item in ListDelete)
                            {
                                int id;
                                if (int.TryParse(grdItem.GetDataKeyValue("ContactID").ToString(), out id) && item.ContactID == id)
                                {
                                    cb.Checked = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {

                foreach (GridDataItem grdItem in ContactsRadGrid.Items)
                {
                    List<Contact_V03> ListDel = new List<Contact_V03>();
                    CheckBox selectCheckBx = (CheckBox)(grdItem.FindControl("SelectorCheckBox"));
                    if (null != selectCheckBx)
                    {
                        if (Session["ListDelete"] != null)
                            ListDel = (List<Contact_V03>)Session["ListDelete"];
                        selectCheckBx.Checked = false;
                        if (selText.Value == "true")
                        {
                            selectCheckBx.Checked = true;
                        }
                        else
                        {
                            foreach (Contact_V03 ContactItems in ListDel)
                            {
                                if (ContactItems.ContactID == int.Parse(grdItem.GetDataKeyValue("ContactID").ToString()))
                                {
                                    if (Session["Selected"] != null)
                                        selectedItems = (ArrayList)Session["Selected"];
                                    if (!selectedItems.Contains(ContactItems.ContactID))
                                    if(selectCheckBx.Checked)
                                    {
                                        selectCheckBx.Checked=true;
                                    }
                                }
                            }
                        }
                    }

                }

                if (Session["Selected"] != null)
                {
                    selectedItems = (ArrayList)Session["Selected"];
         
                    foreach (GridDataItem grdItem in ContactsRadGrid.Items)
                    {
                        CheckBox selectCheckBx = (CheckBox)(grdItem.FindControl("SelectorCheckBox"));

                        GridDataItem row = (GridDataItem)grdItem;
                        int index = (int)row.GetDataKeyValue("ContactID");
                        if (selectedItems.Contains(index))
                        {
                            selectCheckBx.Checked = true;
                        }

                    }
                }

                flagHidden.Value = "1";
            }

            SetSelectedMail();

        }
        protected void ContactsRadGrid_PageIndexChanged(object source, GridPageChangedEventArgs e)
        {
            RememberSelected();
            flagHidden.Value = "1";
            SetListDelete();
            RaiseBubbleEvent(source, new CommandEventArgs("LoadContacts", null));

        }

        protected void ContactsRadGrid_PageSizeChanged(object source, GridPageSizeChangedEventArgs e)
        {
            flagHidden.Value = "1";
            RaiseBubbleEvent(source, new CommandEventArgs("LoadContacts", null));
        }


        protected void ContactsRadGrid_ItemDataBound(object source, GridItemEventArgs e)
        {
            

            switch (e.Item.ItemType)
            {
                case GridItemType.Header:
                    CheckBox selectAllCheckBox = (CheckBox)(e.Item.Cells[3].FindControl("SelectAllList")); ;
                    if (null != selectAllCheckBox)
                    {
                        selectAllCheckBox.Checked = IsSelectAllChecked() && selectedSpan.InnerHtml == totalSpan.InnerHtml && selVal.Value == string.Empty ? true : false;
                    }
                    break;

                case GridItemType.AlternatingItem:
                case GridItemType.Item:
                    Contact_V03 contact = e.Item.DataItem as Contact_V03;
                    LinkButton lnkEmailColumn = e.Item.FindControl("lnkEmailColumn") as LinkButton;
                    Label lblEmailLinkColumn = e.Item.FindControl("lblEmailLinkColumn") as Label;
                    LinkButton lnkInvite = e.Item.FindControl("lnkInvite") as LinkButton;
                    if (contact.PrimaryEmailInfo.OptoutDate != null && contact.PrimaryEmailInfo.OptoutDate <= DateTime.Now.ToUniversalTime())
                    {
                        lnkEmailColumn.Visible = false;
                        lblEmailLinkColumn.Visible = true;
                        // if last invite date is greater than 6 months
                        lnkInvite.Visible = contact.PrimaryEmailInfo.LastInviteDate == null || Convert.ToDateTime(contact.PrimaryEmailInfo.LastInviteDate).AddDays(60) <= DateTime.Now.ToUniversalTime() ? true : false;
                    }
                    else
                    {
                        lnkInvite.Visible = false;
                        lnkEmailColumn.Visible = true;
                        lblEmailLinkColumn.Visible = false;
                    }
                    break;
                case GridItemType.Pager:

                    break;
            }


        }

        #region Binding

        protected string GetFullName(object dataItem)
        {
            return String.Format("{0} {1}{2}", GetFirstName(dataItem), GetLastName(dataItem), GetFullLocalName(dataItem));
        }

        protected string GetFullLocalName(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null && contact.LocalName != null)
            {
                string fullLocalName = string.Format("{0} {1}", contact.LocalName.First, contact.LocalName.Last);

                if (fullLocalName.Trim() != string.Empty)
                {
                    return String.Format(@"<br/> <span class='local-name'>({0})</span>", fullLocalName);
                }
            }

            return "";
        }


        protected int GetID(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;
            return contact.ContactID;
        }

        protected string GetFirstName(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {
                return contact.Name.First;
            }

            return "";
        }

        protected string GetLastName(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null && contact.Name != null && !String.IsNullOrEmpty(contact.Name.Last))
            {
                return contact.Name.Last;
            }

            return "";
        }

        public string GetPhone(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {


                if (!String.IsNullOrEmpty(contact.HomePhoneNumber))
                    return contact.HomePhoneNumber;
            }

            return "";
        }

        public string GetSecondaryPhone(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {


                if (!String.IsNullOrEmpty(contact.MobilePhoneNumber))
                    return contact.MobilePhoneNumber;
            }

            return "";
        }

        public string GetEmail(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {
                if (!String.IsNullOrEmpty(contact.PrimaryEmailAddress))
                    return contact.PrimaryEmailAddress;
            }

            return "";
        }

        public string GetAddressLines(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;
            StringBuilder address = new StringBuilder();
            if (contact != null)
            {

                if (!String.IsNullOrEmpty(contact.StreetAddress1))
                    address.AppendFormat("{0}<br/>", contact.StreetAddress1.Trim());
                if (!String.IsNullOrEmpty(contact.StreetAddress2))
                    address.AppendFormat("{0}<br/>", contact.StreetAddress2.Trim());

                string s = String.Format("{0} {1} {2}",
                    contact.City,
                    contact.State,
                    contact.Zip).Trim();

                string countryName = null;
                try
                {
                    countryName = HL.Common.ValueObjects.CountryType.Parse(contact.Country).Name;
                }
                catch { }
                if (!string.IsNullOrEmpty(countryName))
                    s = s + "<br/>" + countryName;

                if (!String.IsNullOrEmpty(s))
                {
                    address.AppendFormat("{0}<br/>", s);
                }

                if (!String.IsNullOrEmpty(address.ToString().Trim()))
                {
                    return String.Format("<div class='address-lines-div'>{0}</div>", address.ToString());
                }
            }

            return "";
        }

        public string GetCity(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {
                return contact.City;
            }

            return "";
        }

        public string GetLabelOnNewLine(string value, string className)
        {
            if (String.IsNullOrEmpty(value))
                return "";
            else
            {
                string output = String.Format("<label class='{0}'>{1}</label><br/>", className, value);

                return output;
            }
        }

        public string GetState(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null && !String.IsNullOrEmpty(contact.State))
            {
                return contact.State;
            }

            return "";
        }

        public string GetContactType(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {
                return CetLocalizedContactType(contact.ContactType);
            }

            return "";
        }



        public string GetContactSource(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null)
            {
                return CetLocalizedContactSource(contact.ContactSource);
            }

            return "";
        }

        public string GetDateCrated(object dataItem)
        {
            Contact_V03 contact = dataItem as Contact_V03;

            if (contact != null && contact.CreatedDate.HasValue)
            {
                return CetLocalizedDateCreated(contact.CreatedDate.Value);
            }

            return "";
        }

        private string CetLocalizedDateCreated(DateTime createdDate)
        {
            return LocalizationHelper.GetClientLocalTimeFromUtc(createdDate).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }



        private string CetLocalizedContactType(ContactType contactType)
        {
            if (contactType != null)
                return LocalizeEnumKey(contactType, "ContactType_{0}");

            return "";
        }


        private string CetLocalizedContactSource(ContactSourceType contactSourceType)
        {
            if (contactSourceType != null)
                return LocalizeEnumKey(contactSourceType, "ContactSource_{0}");

            return "";
        }


        #endregion

        #region IContactsView Members

        public List<int> GetCheckedContactIDs()
        {
            SetListDelete();
            List<Contact_V03> ListDelete = new List<Contact_V03>();
            if (Session["ListDelete"] != null)
                ListDelete = (List<Contact_V03>)Session["ListDelete"];
            List<int> selectedContactIDs = new List<int>();
            foreach (Contact_V03 items in ListDelete)
            {
                selectedContactIDs.Add(items.ContactID);
            }
            return selectedContactIDs;
        }

        public List<string> GetCheckedContactEmails()
        {
            SetListDelete();
            List<string> selectedContactEmails = new List<string>();
            List<Contact_V03> ListDelete = new List<Contact_V03>();
            if (Session["ListDelete"] != null)
                ListDelete = (List<Contact_V03>)Session["ListDelete"];
            foreach (Contact_V03 items in ListDelete)
            {
                selectedContactEmails.Add(items.ContactID.ToString());
            }

            return selectedContactEmails;
        }

        public List<Contact_V02> GetCheckedContactNameEmails()
        {
            SetListDelete();
            List<Contact_V02> selectedContactEmails = new List<Contact_V02>();
            Contact_V02 emailItem;
            List<Contact_V03> ListDelete = new List<Contact_V03>();
            if (Session["ListDelete"] != null)
                ListDelete = (List<Contact_V03>)Session["ListDelete"];
            foreach (Contact_V03 items in ListDelete)
            {
                emailItem = new Contact_V02();
                emailItem.ContactID = items.ContactID;
                emailItem.LocalName = items.LocalName;
                emailItem.Name = items.Name;
                emailItem.PrimaryEmailAddress = items.PrimaryEmailAddress;
                emailItem.Version = items.Version;
                selectedContactEmails.Add(emailItem);
            }

            return selectedContactEmails;
        }


        public void OnListsLoaded(List<ContactListInfo_V01> lists)
        {
        }

        private GridSortOrder GetSortOrderFromString(string sortOrder)
        {
            if (string.IsNullOrEmpty(sortOrder))
                return GridSortOrder.Ascending;

            switch (sortOrder.ToUpper())
            {
                case "ASC": return GridSortOrder.Ascending;
                case "DESC": return GridSortOrder.Descending;
                default: return GridSortOrder.Ascending;

            }
        }

        public void OnContactsLoaded(List<Contact_V03> contacts, List<string> fieldsToHide)
        {
            if (IsListView())
            {
                HideColumns(fieldsToHide);
            }
            if (null == contacts || contacts.Count == 0)
            {
                totalResults.Value = "0";
                Session["MyContacts"] = null;
                Session["ListDelete"] = null;
                selectUpDiv.Style.Add("display", "none");
                selectDownDiv.Style.Add("display", "none");
            }
            else
            {
                Session["MyContacts"] = null;
                Session["MyContacts"] = contacts;
                totalResults.Value = contacts.Count.ToString();


                selectUpDiv.Style.Add("display", "block");
                selectDownDiv.Style.Add("display", "block");

                if (sortExpression == null)
                {
                    if (string.IsNullOrEmpty(SortField))
                    {
                        sortExpression = new GridSortExpression();
                        sortExpression.FieldName = "Fullname";
                        sortExpression.SortOrder = GetSortOrderFromString(SortDirection);// GridSortOrder.Ascending;
                    }
                    else
                    {
                        sortExpression = new GridSortExpression();
                        sortExpression.FieldName = SortField;
                        sortExpression.SortOrder = GetSortOrderFromString(SortDirection);
                    }
                }
                Sort(contacts, sortExpression);
            }
            ContactsRadGrid.DataSource = contacts;
            List<Contact_V03> DeleteList = new List<Contact_V03>();
            if (Session["ListDelete"] != null)
            {
                DeleteList = (List<Contact_V03>)Session["ListDelete"];
            }
            totalSpan.InnerHtml = totalSpanBot.InnerHtml = totalResults.Value.Replace("-", "");
            selectedResult.Value = DeleteList.Count.ToString();
            selectedSpan.InnerHtml = selectedSpanBot.InnerHtml = selectedResult.Value.Replace("-", "");
        }

        public void SetSelectedMail()
        {
            List<Contact_V03> DeleteList = new List<Contact_V03>();
            if (Session["ListDelete"] != null)
                DeleteList = (List<Contact_V03>)Session["ListDelete"];

            maxMails.Value = maxMailsCount.ToString();
            selectedMail.Value = "0";

            foreach (Contact_V03 ContactItems in DeleteList)
            {
                if (!string.IsNullOrEmpty(ContactItems.PrimaryEmailAddress))
                {
                    selectedMail.Value = (int.Parse(selectedMail.Value) + 1).ToString();
                    maxMails.Value = (maxMailsCount - Convert.ToInt32(selectedMail.Value)).ToString();

                }

            }
            selectedSpanLabel.InnerHtml = selectedMail.Value.Replace("-", "");
            maxMailsSpan.InnerHtml = maxMails.Value.Replace("-", "");
        }

        public void OnContactDetailsLoaded(Contact_V01 contactWithDetails, List<int> assignedLists)
        {
        }

        public void OnAddingNewContact()
        {
        }

        public bool HasEditor()
        {
            return false;
        }

        public void SelectAll(bool selected)
        {
            SelectAllItems = selected;
            RaiseBubbleEvent(this, new CommandEventArgs("LoadContacts", null));
        }

        public void DoExport(string exportType)
        {
            switch (exportType)
            {
                case "Export_Excel":
                case "Export_CSV":
                    {
                        ExportToExcel();
                    }
                    break;
                case "Export_PDF":
                    {
                        ExportToPDF();
                    }
                    break;
            }
        }


        #endregion

        #region Sorting
        private void Sort(List<Contact_V03> contacts, GridSortExpression gridSortExpression)
        {
            int sortOrder = 0;

            if (gridSortExpression.SortOrder == GridSortOrder.Ascending)
                sortOrder = 1;
            if (gridSortExpression.SortOrder == GridSortOrder.Descending)
                sortOrder = -1;

            if (contacts != null)
            {
                contacts.Sort((a, b) => sortOrder * sort(a, b, gridSortExpression.FieldName));
            }
        }

        private int sort(Contact_V03 a, Contact_V03 b, string fieldName)
        {
            switch (fieldName)
            {
                case "Fullname":
                case "FullnameNotLinked":
                    return Compare(GetFullName(a), GetFullName(b));

                case "LastName":
                    return Compare(GetLastName(a), GetLastName(b));

                case "LastName FirstName":

                    if (string.IsNullOrEmpty(GetLastName(a)) && string.IsNullOrEmpty(GetLastName(b)))
                        return Compare(GetFirstName(a), GetFirstName(b));
                    else
                        return Compare(GetLastName(a), GetLastName(b));

                case "ContactSource":
                    return Compare(GetContactSource(a), GetContactSource(b));

                case "CreateDate":
                    return Compare(a.CreatedDate, b.CreatedDate);

                case "Phone":
                    return Compare(GetPhone(a), GetPhone(b));

                case "Email":
                case "EmailNotLinked":
                    return Compare(GetEmail(a), GetEmail(b));

                case "City":
                    return Compare(GetCity(a), GetCity(b));

                case "State":
                    return Compare(GetState(a), GetState(b));

                case "ContactType":
                    return Compare(GetContactType(a), GetContactType(b));

                default:
                    return 0;

            }
        }

        private int Compare(DateTime? a, DateTime? b)
        {
            if (a.HasValue && b.HasValue)
                return DateTime.Compare(a.Value, b.Value);
            if (a.HasValue && !b.HasValue)
                return 1;
            if (!a.HasValue && b.HasValue)
                return -1;
            return 0;
        }

        private int Compare(string a, string b)
        {
            return String.Compare(a, b, true);//todo: apply current culture if needed
        }
        #endregion

        #region Helpers

        private void HideColumns(List<string> columns)
        {
            foreach (string col in columns)
            {
                SetVisibility(col, false);
            }
        }


        private void SetVisibility(string name, bool visible)
        {
            if (IsListView())
            {
                ContactsRadGrid.Columns.FindByUniqueName(name).Visible = visible;
            }
        }

        private bool IsItemSelected(GridDataItem item)
        {
            if (IsListView())
            {
                CheckBox cb = (CheckBox)((item).Cells[2].Controls[1]);

                return null != cb ? cb.Checked : false;
            }
            else if (IsCardView())
            {
                CheckBox cb = (CheckBox)(item.FindControl("SelectorCheckBox"));

                return null != cb ? cb.Checked : false;
            }

            throw new NotImplementedException();
        }

        private bool IsListView()
        {
            return ViewName.Value == "ListView";
        }

        private bool IsCardView()
        {
            return ViewName.Value == "CardView";
        }
        #endregion

        #region export

        protected void ExportToPDF()
        {
            if (ContactsRadGrid.MasterTableView.GetItems(GridItemType.Item).Count() > 0)
            {

                this.ContactsRadGrid.ExportSettings.ExportOnlyData = true;
                this.ContactsRadGrid.ExportSettings.Pdf.PageHeight = Unit.Parse("162mm");
                this.ContactsRadGrid.ExportSettings.Pdf.PageWidth = Unit.Parse("600mm");
                this.ContactsRadGrid.ExportSettings.OpenInNewWindow = true;

                ContactsRadGrid.Page.Response.ClearHeaders();
                ContactsRadGrid.Page.Response.Cache.SetCacheability(HttpCacheability.Private);

                this.ContactsRadGrid.MasterTableView.AllowPaging = false;
                RaiseBubbleEvent(this, new CommandEventArgs("LoadContacts", null));
                ApplyStyleSheetsForExport(this.ContactsRadGrid.MasterTableView);

                ContactsRadGrid.MasterTableView.Style[HtmlTextWriterStyle.FontFamily] = UNICODE_CSS_FONT_FAMILIES;

                this.ContactsRadGrid.MasterTableView.ExportToPdf();
            }
        }



        protected void ExportToExcel()
        {
            if (ContactsRadGrid.MasterTableView.GetItems(GridItemType.Item).Count() > 0)
            {
                this.ContactsRadGrid.ExportSettings.ExportOnlyData = true;
                this.ContactsRadGrid.ExportSettings.OpenInNewWindow = true;

                this.ContactsRadGrid.MasterTableView.AllowPaging = false;
                RaiseBubbleEvent(this, new CommandEventArgs("LoadContacts", null));


                ApplyStyleSheetsForExport(this.ContactsRadGrid.MasterTableView);


                ContactsRadGrid.Page.Response.ClearHeaders();
                ContactsRadGrid.Page.Response.Cache.SetCacheability(HttpCacheability.Private);

                this.ContactsRadGrid.MasterTableView.ExportToExcel();
            }
        }

        protected void ExportToWord()
        {
            if (ContactsRadGrid.MasterTableView.GetItems(GridItemType.Item).Count() > 0)
            {
                this.ContactsRadGrid.ExportSettings.ExportOnlyData = true;
                this.ContactsRadGrid.ExportSettings.OpenInNewWindow = true;
                ApplyStyleSheetsForExport(this.ContactsRadGrid.MasterTableView);

                ContactsRadGrid.Page.Response.ClearHeaders();
                ContactsRadGrid.Page.Response.Cache.SetCacheability(HttpCacheability.Private);

                this.ContactsRadGrid.MasterTableView.ExportToWord();
            }
        }



        private void ApplyStyleSheetsForExport(GridTableView gridTableView)
        {
            if (null != gridTableView.GetItems(GridItemType.Header) && gridTableView.GetItems(GridItemType.Header).Count() > 0)
            {
                gridTableView.Columns.FindByUniqueName("SelectionChecBox").Visible = false;
                gridTableView.Columns.FindByUniqueName("FollowUp").Visible = false;

                GridItem headerItem = gridTableView.GetItems(GridItemType.Header)[0];
                headerItem.Style["font-size"] = "8pt";
                headerItem.Style["color"] = "black";
                headerItem.Style["height"] = "15px";
                headerItem.Style["font-weight"] = "bold";
                headerItem.Style["vertical-align"] = "middle";
                headerItem.Style["width"] = "10px";
      
                foreach (TableCell tableCell in headerItem.Cells)
                {
                    tableCell.Wrap = true;
                    tableCell.Style["text-align"] = "center";
                    tableCell.Style["font-weight"] = "bold";
                    tableCell.Style["font-size"] = "5pt";
                 }

                GridItem[] items = gridTableView.GetItems(GridItemType.Item);
                GridItem[] alternatingItems = gridTableView.GetItems(GridItemType.AlternatingItem);

                ApplyGridItemStyle(items);
                ApplyGridItemStyle(alternatingItems);
            }
        }

        private void ApplyGridItemStyle(GridItem[] items)
        {
            foreach (var item in items)
            {
                item.Style["color"] = "black";
                item.Style["text-align"] = "left";
                item.Style["vertical-align"] = "top";
                item.Style["font-size"] = "5px";
                item.Style["width"] = "30px";

                foreach (TableCell cell in item.Cells)
                {
                    if (cell.Controls != null && cell.Controls.Count > 1 && cell.Controls[1] is LinkButton)
                    {
                        cell.Text = (cell.Controls[1] as LinkButton).Text;
                    }

                    if (cell.Controls != null && cell.Controls.Count > 1 && cell.Controls[1] is HyperLink)
                    {
                        cell.Text = (cell.Controls[1] as HyperLink).Text;
                    }
                    cell.Width = 40;
                    cell.Wrap = true;
                    cell.BorderWidth = Unit.Pixel(1);
                    cell.BorderColor = Color.Black;
                    cell.Style[HtmlTextWriterStyle.MarginLeft] = "2px";
                }
            }
        }
        #endregion


        protected void checkAllResults_CheckedChanged(object sender, EventArgs e)
        {
            if (flagHidden.Value != "1")
            {
                checkAllResults.Checked = checkAllResultsDown.Checked = (sender as CheckBox).Checked;

                if (!(sender as CheckBox).Checked)
                    selVal.Value = "noselect";
                else
                    selVal.Value = string.Empty;

                Session["selectallres"] = null;
                Session["selectallres"] = (sender as CheckBox).Checked;

                List<Contact_V03> ListDelete = new List<Contact_V03>();
                if (Session["ListDelete"] != null)
                    ListDelete = (List<Contact_V03>)Session["ListDelete"];
                if (checkAllResults.Checked)
                {
                    MyContacts = (List<Contact_V03>)Session["MyContacts"];
                    foreach (Contact_V03 ContactItems in MyContacts)
                    {
                        if (!ListDelete.Contains(ContactItems))
                            ListDelete.Add(ContactItems);
                    }
                }
                else
                {
                    ListDelete.Clear();
                }
                Session["ListDelete"] = ListDelete;
                selectedResult.Value = ListDelete.Count.ToString();
                SelectAllItems = ((CheckBox)sender).Checked;
                RaiseBubbleEvent(this, new CommandEventArgs("SelectAll", true));
                selectedSpan.InnerHtml = selectedResult.Value.Replace("-", "");
                selectedSpanBot.InnerHtml = selectedResult.Value.Replace("-", "");
                totalSpan.InnerHtml = totalResults.Value.Replace("-", "");
                totalSpanBot.InnerHtml = totalResults.Value.Replace("-", "");

                SelectAllItems = ((CheckBox)sender).Checked;

                if (ViewName.Value == "CardView")
                {
                    SelectAllCard.Checked = checkAllResults.Checked;
                }
            }
            else
            {
                List<Contact_V03> ListDelete = new List<Contact_V03>();
                if (Session["ListDelete"] == null)
                    ListDelete = (List<Contact_V03>)Session["MyContacts"];
            }
        }

        protected void SelectorCheckBox_CheckedChanged(Object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            GridEditableItem item = (GridEditableItem)cb.NamingContainer;
            var index =  item.GetDataKeyValue("ContactID");
            
                   if (Session["Selected"] != null)
                        selectedItems = (ArrayList) Session["Selected"];
            
                    if (cb.Checked)
                    {
                        if (!selectedItems.Contains(index))
                        {
                            selectedItems.Add(index);
                        }
                    }
                    else
                    {
                        selectedItems.Remove(index);
                    }
              
            if (selectedItems != null && selectedItems.Count > 0)
                Session["Selected"] = selectedItems;
            
        }

        public void SetListDelete()
        {
            List<Contact_V03> ListDelete = new List<Contact_V03>();
            List<Contact_V03> MyContacts = new List<Contact_V03>();
            MyContacts = (List<Contact_V03>)Session["MyContacts"];
            if (Session["ListDelete"] != null)
                ListDelete = (List<Contact_V03>)Session["ListDelete"];

            foreach (GridDataItem grdItem in ContactsRadGrid.Items)
            {
                CheckBox selectCheckBox = (CheckBox)(grdItem.FindControl("SelectorCheckBox"));

                if (null != selectCheckBox && selectCheckBox.Checked)
                {
                    if (MyContacts != null)
                    {
                        foreach (Contact_V03 ContactItems in MyContacts)
                        {
                            if (ContactItems.ContactID == int.Parse(grdItem.GetDataKeyValue("ContactID").ToString()))
                            {
                                if (!ListDelete.Contains(ContactItems))
                                    ListDelete.Add(ContactItems);
                            }

                        }
                    }
                }
                else
                {
                    Contact_V03 deleteItem = new Contact_V03();
                    foreach (Contact_V03 ContactItems in ListDelete)
                    {
                        if (ContactItems.ContactID == int.Parse(grdItem.GetDataKeyValue("ContactID").ToString()))
                        {
                            deleteItem = ContactItems;

                        }

                    }
                    ListDelete.Remove(deleteItem);

                }
            }
            selectedResult.Value = ListDelete.Count.ToString();
            selectedSpan.InnerHtml = selectedResult.Value.Replace("-", "");
            selectedSpanBot.InnerHtml = selectedResult.Value.Replace("-", "");
            totalSpan.InnerHtml = totalResults.Value.Replace("-", "");
            totalSpanBot.InnerHtml = totalResults.Value.Replace("-", "").Replace("-", "");
            Session["ListDelete"] = ListDelete;

        }

        public void ResetSelectItem()
        {
            selectedItems.Clear();
        }
        public void ResetSelectAllResult()
        {
            if (null != checkAllResults)
            {
                checkAllResults.Checked = false;
            }
            if (null != checkAllResultsDown)
            {
                checkAllResultsDown.Checked = false;
            }
            if (null != selectedResult)
            {
                selectedResult.Value = "0";
            }
            if (null != selectedMail)
            {
                selectedMail.Value = "0";
            }
            if (null != totalResults)
            {
                totalResults.Value = "0";
            }
            if (null != selectedSpan)
            {
                selectedSpan.InnerHtml = "0";
            }
            if (null != selectedSpanBot)
            {
                selectedSpanBot.InnerHtml = "0";
            }
            Session["MyContacts"] = null;
            Session["ListDelete"] = null;
            if (null != SelectAllCard)
            {
                SelectAllCard.Checked = false;
            }
        }

        private int? _maxMailsCount
        {
            get { return ECardsProvider.GetMailingQuotaInPastHours(((MembershipUser<DistributorProfileModel>)Membership.GetUser()).Value.Id, Settings.GetRequiredAppSetting<int>("MailingQuotaLimitDurationHours", 24)); }
        }

        //adding the declaration of as the desiner class allways ignore this.
        protected global::System.Web.UI.WebControls.CheckBox SelectAllCard;

        protected bool IsCheckAllResultChecked()
        {
            if (Session["selectallres"] != null)
                return bool.Parse(Session["selectallres"].ToString());
            else
                return checkAllResults.Checked;
        }

       

        public string GetSelectedCheck()
        {
          RememberSelected();
          
          if(Session["Selected"]!=null)
          {
              selectedItems = (ArrayList) Session["Selected"];
              return selectedItems.Count.ToString();
          }
            return "0";
        }

       
        public void RememberSelected()
        {
            int index = -1;
            foreach (GridDataItem row in ContactsRadGrid.Items)
            {
                index = (int)row.GetDataKeyValue("ContactID");
                bool result = ((CheckBox)row.FindControl("SelectorCheckBox")).Checked;
                
                if(Session["Selected"] != null)
                    selectedItems = (ArrayList)Session["Selected"];

                if (result)
                {
                    if (!selectedItems.Contains(index))
                        selectedItems.Add(index);
                    
                }
                else
                {
                    selectedItems.Remove(index);
                }
            }

            if (selectedItems != null && selectedItems.Count > 0)
                Session["Selected"] = selectedItems;
        }

        public string LocalizeEnumKey<T>(T denum, string resxKeyFormat) where T : IDenum<string>
        {
            string resxKey = string.Format(resxKeyFormat, denum.Key);
            var p = new LocalizationManager();
            return p.GetString(AppRelativeVirtualPath, resxKey, Thread.CurrentThread.CurrentUICulture);
        }
    }
}
