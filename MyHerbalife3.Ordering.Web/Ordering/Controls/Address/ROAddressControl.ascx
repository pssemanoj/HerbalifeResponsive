<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ROAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.ROAddressControl" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>
<%@ Register TagPrefix="ajaxToolkit" Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit, Version=3.0.11119.25533, Culture=neutral, PublicKeyToken=28f01b0e84b6d53e" %>
<script type="text/javascript" language="javascript">
    function GetNumberHelpText() {
        var tooltip = $find("<%=HMExtNumber.ClientID%>").show();
        event.returnValue = false;
        return false;
    }
</script>
<ajaxToolkit:HoverMenuExtender ID="HMExtNumber" runat="server" TargetControlID="imgNumberHelp"
    PopupControlID="pnlNumberHelpText"/>

<div id="gdo-popup-container">

    <asp:Label runat="server" ID="lbRecipent" Text="Name*:" meta:resourcekey="CareOfName"></asp:Label>
    <asp:TextBox runat="server" ID="txtCareOfName" MaxLength="40" meta:resourcekey="txtCareOfNameResource"></asp:TextBox>

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li><asp:Label runat="server" ID="lbCity" Text="City*:" meta:resourcekey="City"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" 
                meta:resourcekey="ddlCityResource" onselectedindexchanged="dnlCity_SelectedIndexChanged" >
            </asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lbLocality" Text="Locality*:" meta:resourcekey="Locality"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlLocality" 
                meta:resourcekey="dnlLocalityResource" onselectedindexchanged="dnlLocality_SelectedIndexChanged">
            </asp:DropDownList>
        </li>

        <li><asp:Label runat="server" ID="lblStreetType" Text="Street type*:" meta:resourcekey="StreetType"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlStreetType" 
                meta:resourcekey="dnlStreetTypeResource" onselectedindexchanged="dnlStreetType_SelectedIndexChanged" >
            </asp:DropDownList> 
        </li>

        <li><asp:Label runat="server" ID="lblStreet1" Text="Street*:" meta:resourcekey="Street1"></asp:Label>
            <telerik:RadComboBox runat="server" ID="dnlStreet" AllowCustomText="true" MarkFirstMatch="true" AutoPostBack="true"
                EnableTextSelection="true" ShowToggleImage="false" MaxHeight="220"
                Filter="Contains" ></telerik:RadComboBox>
        </li>
    </ul>
    <ul class="gdo-form-label-left gdo-popup-form-label-padding2"> 
        <li class="sameRow"><div>
                <asp:Label runat="server" ID="lblNumber" Text="Number*:" meta:resourcekey="Number"></asp:Label>&nbsp;
                <a href="javascript:GetNumberHelpText();" onclick="return false;" onfocus="blur();">
                    <img id="imgNumberHelp" runat="server" src="/Content/Global/Events/cruise/img/icon_info.gif" height="12" width="12"/>
                </a>
            </div>
            <asp:TextBox runat="server" ID="txtNumber" MaxLength="4" meta:resourcekey="txtNumberResource"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lbBlock" Text="Block:" meta:resourcekey="Block"></asp:Label>
            <asp:TextBox runat="server" ID="txtBlock" MaxLength="20"  meta:resourcekey="txtBlockResource"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblScara" Text="Scara:" meta:resourcekey="Scara"></asp:Label>
            <asp:TextBox runat="server" ID="txtScara" MaxLength="3"  meta:resourcekey="txtScaraResource"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblEtaj" Text="Etaj:" meta:resourcekey="Etaj"></asp:Label>
            <asp:TextBox runat="server" ID="txtEtaj" MaxLength="2" meta:resourcekey="txtEtajResource"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblApartment" Text="Apartament:" meta:resourcekey="Apartment"></asp:Label>
            <asp:TextBox runat="server" ID="txtApartment" MaxLength="3" meta:resourcekey="txtApartmentResource"></asp:TextBox>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblPostal" Text="Postal code*:" meta:resourcekey="PostalCode"></asp:Label>
            <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlPostCode" meta:resourcekey="dnlPostCodeResource">
            </asp:DropDownList>
        </li>

        <li class="sameRow"><asp:Label runat="server" ID="lblPhoneNumber" Text="Telephone*:" meta:resourcekey="PhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="txtPhoneNum" MaxLength="10" meta:resourcekey="txtPhoneNumResource"></asp:TextBox>
            <span class="gdo-form-format">
                <asp:Localize ID="Localize" runat="server" meta:resourcekey="PhoneNumberFormat">Format: 9-10 digits</asp:Localize>
            </span>
        </li>
    </ul>
</div>

<asp:Panel ID="pnlNumberHelpText" runat="server" Style="display: none; width:300px">
    <div class="gdo-popup">
        <asp:Label ID="lblNumberHelp" runat="server" meta:resourcekey="lblNumberHelp">
        </asp:Label>
    </div>
</asp:Panel>