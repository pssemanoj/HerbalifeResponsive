<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BrowseContacts.ascx.cs"
    Inherits=" MyHerbalife3.Ordering.Web.Ordering.Controls.Invoices.BrowseContacts" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/ContactsListView.ascx" TagPrefix="Bizworks" TagName="ContactListView" %>
<%@ Register Src="~/Bizworks/Controls/Invoices/ContactsBasicSearch.ascx" TagPrefix="Bizworks" TagName="contactsBasicSearchControl" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI, Version=2011.3.1305.40, Culture=neutral, PublicKeyToken=121fae78165ba3d4" %>
<telerik:RadAjaxLoadingPanel ID="LoadingPanel1" runat="server" meta:resourcekey="LoadingPanel1">
</telerik:RadAjaxLoadingPanel>
<telerik:RadScriptBlock runat="server" ID="MyDownlineScriptBlock1">
    <script language="javascript" type="text/javascript">
        function OnInvoiceContactsCancelClick() {
            return HideInvContactPopup();
        }

        function OnInvoiceContactsClientClick() {
            if (ValidateInvoiceContactSelections()) {
                HideInvContactPopup();
                return true;
            }
            return false;
        }

        function clearContactSelection() {
            $('#contacts-listview').find('input[type=checkbox]').each(function () {
                this.checked = false;
            });
        }
    </script>
</telerik:RadScriptBlock>
<div class="bluecontainer">
    <table>
        <tr>
            <td id="tdCloseButton">
		        <cc1:DynamicButton runat="server" ID="closeContacts" 
                    Text="Close" meta:resourcekey="closeContacts" 
                    OnClick="CloseContacts_OnClick"  OnClientClick="OnInvoiceContactsCancelClick();"
                    ButtonType="Back" />
            </td>
        </tr>
        <tr>
            <td id="basic-search">
                <Bizworks:contactsBasicSearchControl ShowAdvancedSearchLink="false" runat="server"
                    ID="contactsBasicSearch" />
            </td>
        </tr>
        <tr>
            <td id="invoice-buttons">
                <div id="InvoiceButtons" class="buttongroup">
                        <cc1:DynamicButton runat="server" ID="invoiceContactButton" 
                            Text="Invoice This Contact" meta:resourcekey="invoiceContactButton" CssClass="buttongap"
                            OnClick="OnInvoiceContactClicked" OnClientClick="return OnInvoiceContactsClientClick();"
                            ButtonType="Forward" />

                        <cc1:DynamicButton runat="server" ID="deleteCheckedButton" 
                            Text="Delete Checked" meta:resourcekey="deleteCheckedButton"
                            OnClick="DeleteChecked_click"  OnClientClick="return CheckIfAnySelected()"
                            ButtonType="Back" />                    
                </div>
            </td>
        </tr>
        <tr>
            <td id="contacts-listview">
                <Bizworks:ContactListView ID="ContactsListView1" runat="server" />
            </td>
        </tr>
    </table>
</div>
