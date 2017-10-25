<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactsListView.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.ContactsListView" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/ContactFollowUpForm.ascx" TagPrefix="Bizworks" TagName="ContactFollowUpForm" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<asp:HiddenField runat="server" ID="ViewName" Value="ListView" />
<asp:HiddenField ID="selectedResult" runat="server" Value="0" />
<asp:HiddenField ID="totalResults" runat="server" Value="0" />
<asp:HiddenField ID="flagHidden" runat="server" />
<asp:HiddenField ID="selectAllChecked" runat="server" Value="false" />
<asp:HiddenField ID="maxMails" runat="server" />
<asp:HiddenField ID="selVal" runat="server" />
<asp:HiddenField ID="selText" runat="server" />
<asp:HiddenField ID="selectedMail" runat="server"/>

<telerik:RadScriptBlock ID="RadScriptBlock1" runat="server">
    <script type="text/javascript">
            
        function CheckIfAnySelected() {

            var rdgName = $find("<%=ContactsRadGrid.ClientID %>");
            var MasterTable = rdgName.get_masterTableView();
            var selectedRows = MasterTable.get_dataItems();
            var count = 0;
            var index = 0;
            for (var i = 0; i < selectedRows.length; i++) {
                var row = selectedRows[i];
                var cell = MasterTable.getCellByColumnUniqueName(row, "SelectionChecBox");
                if (cell.children[0].checked) {
                    count++;
                }
            }

            if (count == 0) {
                alert('<%=base.GetLocalResourceString("noContactSelected")%>');
                return false;
            }
            return true;
        }

        function ValidateInvoiceContactSelections() 
        {
       
        var count = '<%=GetSelectedCheck()%>';
        if (count > 1) 
                    {
                        alert('<%=base.GetLocalResourceString("invoiceOneContact")%>');
                        return false;                       
                    }

                    if (count == 0) {
                    
                            alert('<%=base.GetLocalResourceString("checkContact")%>');
                            return false;
                        }

                    
            return true;
        }

        function GetSelectedNames() {
            var rdgName = $find("<%=ContactsRadGrid.ClientID %>");
            var MasterTable = rdgName.get_masterTableView();
            var selectedRows = MasterTable.get_dataItems();
            var count = 0;
            var index = 0;
            for (var i = 0; i < selectedRows.length; i++) {
                var row = selectedRows[i];
                var cell = MasterTable.getCellByColumnUniqueName(row, "SelectionChecBox");
                if (cell.children[0].checked) {
                    count++;
                    if (count > 1) {
                        alert('<%=base.GetLocalResourceString("invoiceOneContact")%>');
                        return false;
                        break;
                    }
                    index = i;
                }
            }

            if (count == 1) {
                var selectedRow = selectedRows[index];
                var selectedFirstNameCell = MasterTable.getCellByColumnUniqueName(selectedRow, "FirstName");
                var selectedLastNameCell = MasterTable.getCellByColumnUniqueName(selectedRow, "LastName");
                var selectedPhoneCell = MasterTable.getCellByColumnUniqueName(selectedRow, "Phone");
                var selectedEmailCell = MasterTable.getCellByColumnUniqueName(selectedRow, "EmailNotLinked");
                var selectedCityCell = MasterTable.getCellByColumnUniqueName(selectedRow, "City");
                var selectedStateCell = MasterTable.getCellByColumnUniqueName(selectedRow, "State");
                var selectedContactIDCell = selectedRow.getDataKeyValue("ContactID");
                var selectedStreetAddress1 = selectedRow.getDataKeyValue("StreetAddress1");
                var selectedStreetAddress2 = selectedRow.getDataKeyValue("StreetAddress2");
                var selectedZip = selectedRow.getDataKeyValue("Zip");


                var invoiceUrl;
                if (typeof (selectedFirstNameCell.text) != 'undefined') {

                    invoiceUrl = atgUrlValue + 'bizworks/copy_contact.jsp?contactId=' + encodeURIComponent(selectedContactIDCell) +
            '&firstName=' + encodeURIComponent(selectedFirstNameCell.text.trim()) + '&lastName=' + encodeURIComponent(selectedLastNameCell.text.trim()) + '&email=' + encodeURIComponent(selectedEmailCell.text.trim()) + '&phoneNumber=' + encodeURIComponent(selectedPhoneCell.text.trim()) +
            '&address=' + encodeURIComponent(selectedStreetAddress1) + '&street=' + encodeURIComponent(selectedStreetAddress2) + '&city=' + encodeURIComponent(selectedCityCell.text.trim()) + '&state=' + encodeURIComponent(selectedStateCell.text.trim()) +
            '&postalCode1=' + encodeURIComponent(selectedZip) + '&postalCode2=' + encodeURIComponent('');

                    // alert(invoiceUrl);
                    //alert(selectedFirstNameCell.text.trim() + ',' + selectedLastNameCell.text.trim() + ',' + selectedPhoneCell.text.trim() + ',' + selectedEmailCell.text.trim() + ',' + selectedCityCell.text.trim() + ',' + selectedStateCell.text.trim());
                }

                else if (typeof (selectedFirstNameCell.textContent) != 'undefined') {

                    invoiceUrl = atgUrlValue + 'bizworks/copy_contact.jsp?contactId=' + encodeURIComponent(selectedContactIDCell) +
            '&firstName=' + encodeURIComponent(selectedFirstNameCell.textContent.trim()) + '&lastName=' + encodeURIComponent(selectedLastNameCell.textContent.trim()) + '&email=' + encodeURIComponent(selectedEmailCell.textContent.trim()) + '&phoneNumber=' + encodeURIComponent(selectedPhoneCell.textContent.trim()) +
            '&address=' + encodeURIComponent(selectedStreetAddress1) + '&street=' + encodeURIComponent(selectedStreetAddress2) + '&city=' + encodeURIComponent(selectedCityCell.textContent.trim()) + '&state=' + encodeURIComponent(selectedStateCell.textContent.trim()) +
            '&postalCode1=' + encodeURIComponent(selectedZip) + '&postalCode2=' + encodeURIComponent('');

                    // alert(invoiceUrl);

                    //  alert(selectedFirstNameCell.textContent.trim() + ',' + selectedLastNameCell.textContent.trim() + ',' + selectedPhoneCell.textContent.trim() + ',' + selectedEmailCell.textContent.trim() + ',' + selectedCityCell.textContent.trim() + ',' + selectedStateCell.textContent.trim());
                }

                else if (typeof (selectedFirstNameCell.innerText) != 'undefined') {

                    invoiceUrl = atgUrlValue + 'bizworks/copy_contact.jsp?contactId=' + encodeURIComponent(selectedContactIDCell) +
            '&firstName=' + encodeURIComponent(selectedFirstNameCell.innerText.trim()) + '&lastName=' + encodeURIComponent(selectedLastNameCell.innerText.trim()) + '&email=' + encodeURIComponent(selectedEmailCell.innerText.trim()) + '&phoneNumber=' + encodeURIComponent(selectedPhoneCell.innerText.trim()) +
            '&address=' + encodeURIComponent(selectedStreetAddress1) + '&street=' + encodeURIComponent(selectedStreetAddress2) + '&city=' + encodeURIComponent(selectedCityCell.innerText.trim()) + '&state=' + encodeURIComponent(selectedStateCell.innerText.trim()) +
            '&postalCode1=' + encodeURIComponent(selectedZip) + '&postalCode2=' + encodeURIComponent('');

                    // alert(invoiceUrl);

                    //  alert(selectedFirstNameCell.innerText.trim() + ',' + selectedLastNameCell.innerText.trim() + ',' + selectedPhoneCell.innerText.trim() + ',' + selectedEmailCell.innerText.trim() + ',' + selectedCityCell.innerText.trim() + ',' + selectedStateCell.innerText.trim());
                }
                parent.location.href = invoiceUrl;
                return true;
            }

            if (count == 0) {
                alert('<%=base.GetLocalResourceString("checkContact")%>');
                return false;
            }
        }

        function OnClientFollowupClick(contactID) {

            var tooltip = $find("<%=FollowUpPopup.ClientID%>");
            ResetFollowupForm();
            tooltip.show();
            $('#<%=ContactFollowUpForm.dataField.ClientID%>').val(contactID);
            $('#<%=ContactFollowUpForm.SubjectTextBoxField.ClientID%>').val('');
            return false;
        }


        function HideFollowupPopup() {

            var tooltip = $find("<%=FollowUpPopup.ClientID%>");
            tooltip.hide();
            $('#<%=ContactFollowUpForm.SubjectTextBoxField.ClientID%>').val('');
            return false;
        }

        function checkallflag() {
            var flagHidden = $get("<%=flagHidden.ClientID%>");
            flagHidden.value = "0";
        }

        function validateSelectAll() {
            var totalResults = $get("<%=totalResults.ClientID%>");
            var selectedResult = $get("<%=selectedResult.ClientID%>");
            var maxMails = $get("<%=maxMails.ClientID%>");
            var selectedSpan = $get("<%=selectedSpan.ClientID%>");
            var selectedSpanBot = $get("<%=selectedSpanBot.ClientID%>");
            var checkAllResults = $get("<%=checkAllResults.ClientID%>");
            var checkAllResultsDown = $get("<%=checkAllResultsDown.ClientID%>");
            var maxMailsSpan = $get("<%=maxMailsSpan.ClientID%>");
            var totalSpan = $get("<%=totalSpan.ClientID%>");
            var totalSpanBot = $get("<%=totalSpanBot.ClientID%>");
            var selectedSpanLabel = $get("<%=selectedSpanLabel.ClientID%>");
            var flagHidden = $get("<%=flagHidden.ClientID%>");
            var SelectAll = $get(SelectAllListID());
            var selectedMail = $get("<%= selectedMail.ClientID %>");
            var email; // = $get(chk.id.replace("SelectorCheckBox", "lnkEmailColumn"));

            var els = document.forms[0].elements;
            for (i = 0; i < els.length; i++) {
                if (els[i].type == "checkbox") {
                    var strSplit = els[i].id.split("_");
                    var c = els[i].id;
                    var a = $get(c);
                    if (SelectAll.checked) {

                        if (strSplit[strSplit.length - 1] == 'SelectorCheckBox') {
                            email = $get(els[i].id.replace("SelectorCheckBox", "lnkEmailColumn"));
                            if (!a.checked) {
                                a.checked = true;
                                selectedResult.value = (parseInt(selectedResult.value) + 1).toString();
                                if (email != 'undefined' && email != null) {
                                    if (email.innerHTML != '') {
                                        selectedMail.value = (parseInt(selectedMail.value) + 1).toString();
                                        maxMails.value = (parseInt(maxMails.value) - 1).toString();
                                    }
                                }
                            }
                        }
                    }
                    else {
                        if (strSplit[strSplit.length - 1] == 'SelectorCheckBox') {
                            email = $get(els[i].id.replace("SelectorCheckBox", "lnkEmailColumn"));
                            a.checked = false;
                            selectedResult.value = (parseInt(selectedResult.value) - 1).toString();
                            if (email != 'undefined' && email != null) {
                                if (email.innerHTML != '') {
                                    selectedMail.value = (parseInt(selectedMail.value) - 1).toString();
                                    maxMails.value = (parseInt(maxMails.value) + 1).toString();
                                }
                            }
                        }
                    }
                }
            }
            selectedSpan.innerHTML = selectedResult.value.replace("-", "");
            selectedSpanBot.innerHTML = selectedResult.value.replace("-", "");
            selectedSpanLabel.innerHTML = selectedMail.value.replace("-", "");
            maxMailsSpan.innerHTML = maxMails.value.replace("-", "");
            if (selectedResult.value == totalResults.value) {
                checkAllResults.checked = true;
                checkAllResultsDown.checked = true;
                SelectAll.checked = true;
                flagHidden.value = "0";
                CheckExceedorRemain();
                //checkAllResults.click();
                return false;
            }
            else {
                flagHidden.value = "1";
                CheckExceedorRemain();
                if (selectedSpan.innerHTML == "0") {
                    checkAllResults.checked = false;
                    checkAllResultsDown.checked = false;
                    //checkAllResults.click();
                }                
                return false;
            }
        }

        function CheckExceedorRemain() {
            try {
                var lblExceed = $get("<%=lblExceed.ClientID%>");
                var lblRemaining = $get("<%=lblRemaining.ClientID%>");
                var maxMails = $get("<%=maxMails.ClientID%>");
                if (parseInt(maxMails.value) < 0) {
                    lblExceed.style.display = "block";
                    lblExceed.style.visibility = "visible";
                    lblRemaining.style.display = "none";
                    lblRemaining.style.visibility = "hidden";
                }
                else {
                    lblRemaining.style.display = "block";
                    lblRemaining.style.visibility = "visible";
                    lblExceed.style.display = "none";
                    lblExceed.style.visibility = "hidden";
                }
            } catch (esc)
           { }
        }

        function validateCheckMsg(chk) {
            var totalResults = $get("<%=totalResults.ClientID%>");
            var selectedResult = $get("<%=selectedResult.ClientID%>");
            var maxMails = $get("<%=maxMails.ClientID%>");
            var selectedSpan = $get("<%=selectedSpan.ClientID%>");
            var selectedSpanBot = $get("<%=selectedSpanBot.ClientID%>");
            var checkAllResults = $get("<%=checkAllResults.ClientID%>");
            var checkAllResultsDown = $get("<%=checkAllResultsDown.ClientID%>");
            var maxMailsSpan = $get("<%=maxMailsSpan.ClientID%>");
            var totalSpan = $get("<%=totalSpan.ClientID%>");
            var totalSpanBot = $get("<%=totalSpanBot.ClientID%>");
            var selectedSpanLabel = $get("<%=selectedSpanLabel.ClientID%>");
            var flagHidden = $get("<%=flagHidden.ClientID%>");
            var SelectAll = $get(SelectAllListID());
            var chkNew = $get(chk.id);
            var selectedMail = $get("<%= selectedMail.ClientID %>");
            var email = $get(chk.id.replace("SelectorCheckBox", "lnkEmailColumn"));
            var intVal;
            if (chkNew.checked == true) {
                selectedResult.value = (parseInt(selectedResult.value) + 1).toString();
                if (email != 'undefined' && email != null) {
                    if (email.innerHTML != '') {
                        selectedMail.value = (parseInt(selectedMail.value) + 1).toString();
                        maxMails.value = (parseInt(maxMails.value) - 1).toString();
                    }
                }
            }
            else {
                selectedResult.value = (parseInt(selectedResult.value) - 1).toString();
                if (email != 'undefined' && email != null) {
                    if (email.innerHTML != '') {
                        selectedMail.value = (parseInt(selectedMail.value) - 1).toString();
                        maxMails.value = (parseInt(maxMails.value) + 1).toString();
                    }
                }
            }
            selectedSpan.innerHTML = selectedResult.value.replace("-", "");
            selectedSpanBot.innerHTML = selectedResult.value.replace("-", "");
            totalSpan.innerHTML = totalResults.value.replace("-", "");
            totalSpanBot.innerHTML = totalResults.value.replace("-", "");
            selectedSpanLabel.innerHTML = selectedMail.value.replace("-", "");
            maxMailsSpan.innerHTML = maxMails.value.replace("-", "");
            if (selectedResult.value == totalResults.value) {
                checkAllResults.checked = true;
                checkAllResultsDown.checked = true;
                SelectAll.checked = true;
                flagHidden.value = "0";
                CheckExceedorRemain();
                return false;
            }
            else {
                flagHidden.value = "1";
                SelectAll.checked = false;
                CheckExceedorRemain();
                return false;
            }
        }

        function SelectAllListID() {
            var textid;
            var eles = document.forms[0].elements;
            for (i = 0; i < eles.length; i++) {
                if (eles[i].type == "checkbox") {
                    var strSplit = eles[i].id.split("_");
                    if (strSplit[strSplit.length - 1] == 'SelectAllList') {
                        textid = eles[i].id;
                        return eles[i].id;
                    }
                }
            }

        }
        function SelectAllFromCode() {
            try {
                var SelectAll = $get(SelectAllListID());
                var elem = document.forms[0].elements;
                for (i = 0; i < elem.length; i++) {
                    if (elem[i].type == "checkbox") {
                        var strSplit = elem[i].id.split("_");

                        var c = elem[i].id;
                        var a = $get(c);

                        if (strSplit[strSplit.length - 1] == 'SelectorCheckBox') {
                            if (!a.checked) {

                                SelectAll.checked = false;

                                break;
                            }
                            else {
                                SelectAll.checked = true;
                            }
                        }
                    }
                }
            } catch (erc) {

            }
        }

        SelectAllFromCode();
        CheckExceedorRemain();   
    </script>
</telerik:RadScriptBlock>
<table class="list-view-table">
    <tr>
        <td class="gridview-container-td">
         <div id="all-result-status" class="all-result-status-container" >
         <div id="selectUpDiv" runat="server" >
             <div id="clearStatus" class="clearDiv">
                <div class="all-result-status-selector-container">
                    <asp:CheckBox ID="checkAllResults" runat="server" Text="Select All Results" Checked="<%# IsCheckAllResultChecked() %>" EnableViewState="true"  meta:resourcekey="SelectAllResults" AutoPostBack="True" onclick="javascript:checkallflag();" oncheckedchanged="checkAllResults_CheckedChanged"  /> 
                    <span id="contacts-selected-status" class="contacts-selected-status-container">  (<span id="selectedSpan" runat="server">0</span>/<span id="totalSpan" runat="server"></span>)</span>
                </div>
            </div>
             <div id="all-result-status-message"  class="all-result-status-message-container"  > 
                <div id="recipients" class="recipientsContainer">
                <span id="selectedSpanLabel" runat="server"></span> <asp:Label runat="server" ID="lblRecipients" Text="recipients" meta:resourcekey="recipients"  >;</asp:Label> 
                <span id="maxMailsSpan" runat="server"></span>  
                </div>
                <div id="remaining" class="remainingContainer">
                <asp:Label runat="server" ID="lblRemaining" Text="remaining" meta:resourcekey="remaining"  CssClass="select-contacts-remaining-continer" ></asp:Label> <asp:Label runat="server" ID="lblExceed" Text="exceeded" CssClass="select-contacts-exceeded-continer" meta:resourcekey="exceeded"></asp:Label> 
                </div>
             </div>
         </div>
         </div>
            <table  class="grid-view-table">
                <tr>
                    <td>
                        <telerik:RadGrid ID="ContactsRadGrid" Width="100%" AllowPaging="True" PageSize="5"
                            runat="server" AllowSorting="True" AutoGenerateColumns="False" 
                            OnItemCommand="ContactsRadGrid_ItemCommand" OnPreRender="ContactsRadGrid_PreRender"
                            OnItemDataBound="ContactsRadGrid_ItemDataBound" OnPageIndexChanged="ContactsRadGrid_PageIndexChanged"
                            OnSortCommand="ContactsRadGrid_SortCommand" OnPageSizeChanged="ContactsRadGrid_PageSizeChanged"
                            meta:resourcekey="ContactsRadGrid">
                            <PagerStyle Mode="NextPrevAndNumeric" Position="TopAndBottom" AlwaysVisible="true"
                                PagerTextFormat="{4} Page {0} of {1}  " ShowPagerText="true" />
                            <SelectedItemStyle />
                            <MasterTableView Width="100%" DataKeyNames="ContactID,PrimaryEmailAddress" AllowFilteringByColumn="false"
                                AllowCustomSorting="true" AllowPaging="true"  ClientDataKeyNames="ContactID,StreetAddress1,StreetAddress2,Zip" GridLines="None" AllowSorting="true">
                                <Columns>
                                    <telerik:GridTemplateColumn UniqueName="SelectionChecBox" AllowFiltering="false">
                                        <HeaderTemplate>
                                            <asp:Label runat="server" ID="SelectAllLabel" Text="Select All" meta:resourcekey="SelectAllLabel"
                                                CssClass="selectAllLabel" ></asp:Label>
                                            <asp:CheckBox runat="server" ID="SelectAllList" AutoPostBack="false" EnableViewState="true"
                                                meta:resourcekey="SelectAll" onclick="javascript:validateSelectAll(this);"/>
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                        
                                            <asp:CheckBox runat="server" AutoPostBack="true" OnCheckedChanged="SelectorCheckBox_CheckedChanged" ID="SelectorCheckBox" Checked="<%# IsSelectAllChecked() %>"  onclick="javascript:validateCheckMsg(this);SelectAllFromCode();"/>
                                           
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="FirstName" HeaderText="First Name" meta:resourcekey="FirstNameColumn">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="FirstNameLabel" Text="<%# GetFirstName(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="LastName" HeaderText="Last Name" meta:resourcekey="LastNameColumn">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="LastNameLabel" Text="<%# GetLastName(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="FullName" HeaderText="Name" SortExpression="Fullname" HeaderStyle-CssClass="rgHeader grid-header-fullname"
                                        meta:resourcekey="FullNameColumn">
                                        <ItemTemplate>
                                            <asp:LinkButton runat="server" ID="NameLinkButton" CommandName="ViewDetails" Text="<%# GetFullName(Container.DataItem) %>"></asp:LinkButton>                                              
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="FullNameNotLinked" HeaderText="Full Name"
                                        SortExpression="Fullname" meta:resourcekey="FullNameColumn">
                                        <ItemTemplate>
                                            <asp:Label runat="server" ID="FullNameNotLinkedLabel" Text="<%# GetFullName(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="ContactSource" HeaderText="Lead Source" SortExpression="ContactSource"
                                        meta:resourcekey="LeadSourceColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label1" runat="server" Text="<%# GetContactSource(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="CreatedDate" HeaderText="Date Created" SortExpression="CreateDate"
                                        meta:resourcekey="DateCreatedColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label2" runat="server" Text="<%# GetDateCrated(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="Phone" HeaderText="Phone" SortExpression="Phone"
                                        meta:resourcekey="PhoneColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label3" runat="server" Text="<%# GetPhone(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="Email" HeaderText="Email" SortExpression="Email"
                                        meta:resourcekey="EmailColumn">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="lnkEmailColumn" runat="server" CommandName="EmailClicked" Text="<%# GetEmail(Container.DataItem) %>"></asp:LinkButton>
                                            <asp:Label ID="lblEmailLinkColumn" runat="server" Text="Unsubscribed" meta:resourcekey="lblEmailLinkColumn" /><br />
                                            <asp:LinkButton ID="lnkInvite" runat="server" CommandName="ReinviteClicked" Text="Invite contact to Opt-In" meta:resourcekey="lnkInvite"></asp:LinkButton>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="EmailNotLinked" HeaderText="Email" SortExpression="Email"
                                        meta:resourcekey="EmailColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="EmailNotLinkedLabel" runat="server" Text="<%# GetEmail(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="City" HeaderText="City" SortExpression="City"
                                        meta:resourcekey="CityColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label4" runat="server" Text="<%# GetCity(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="State" HeaderText="State" SortExpression="State"
                                        meta:resourcekey="StateColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label5" runat="server" Text="<%# GetState(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="Type" HeaderText="Type" SortExpression="ContactType"
                                        meta:resourcekey="TypeColumn">
                                        <ItemTemplate>
                                            <asp:Label ID="Label6" runat="server" Text="<%# GetContactType(Container.DataItem) %>"></asp:Label>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                    <telerik:GridTemplateColumn UniqueName="FollowUp" HeaderText="Follow-Up" meta:resourcekey="FollowUpColumn" HeaderStyle-CssClass="rgHeader FollowUpHeaderClass" HeaderStyle-Wrap="false">
                                        <ItemStyle Wrap="false" />
                                        <ItemTemplate>
                                            <a href="#" onclick='return OnClientFollowupClick(<%# GetID(Container.DataItem) %>);'>
                                                <asp:Localize runat="server" ID="FollowUpLink" meta:resourcekey="FollowUpLink">Follow-Up</asp:Localize>
                                            </a>
                                        </ItemTemplate>
                                    </telerik:GridTemplateColumn>
                                </Columns>
                                <EditFormSettings>
                                    <EditColumn InsertImageUrl="Update.gif" UpdateImageUrl="Update.gif" EditImageUrl="Edit.gif"
                                        CancelImageUrl="Cancel.gif">
                                    </EditColumn>
                                </EditFormSettings>
                            </MasterTableView>
                            
                            <FilterMenu >
                            </FilterMenu>
                        </telerik:RadGrid>
                    
                    </td>
                </tr>
            </table>
        </td>
    </tr>
</table>
<telerik:RadToolTip ZIndex="50001" ID="FollowUpPopup" runat="server" Modal="True"
    ShowEvent="FromCode" HideEvent="ManualClose" Position="Center" RelativeTo="BrowserWindow"
     ManualClose="True" meta:resourcekey="FollowUpPopup">
    <asp:Panel runat="server" ID="Panel1" class="popupwrapper" meta:resourcekey="Panel1">
        <Bizworks:ContactFollowUpForm runat="server" ID="ContactFollowUpForm" />
    </asp:Panel>
</telerik:RadToolTip>
<div id="selectDownDiv" runat="server" >
    <div id="all-result-status-bottom" class="all-result-status-container">
        <div class="all-result-status-selector-container">
            <asp:CheckBox ID="checkAllResultsDown" runat="server" Text="Select All Results" Checked="<%# IsCheckAllResultChecked() %>"  meta:resourcekey="SelectAllResults" AutoPostBack="True" onclick="javascript:checkallflag();" oncheckedchanged="checkAllResults_CheckedChanged"  /> 
            <span id="contacts-selected-status-bottom" class="contacts-selected-status-container">  (<span id="selectedSpanBot" runat="server">0</span>/<span id="totalSpanBot" runat="server"></span>)</span>
        </div>
    </div>
</div>

<script type="text/javascript" language="javascript">

    SelectAllFromCode();
    CheckExceedorRemain();  
</script>

    

