<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HAPMiniCartControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HAP.HAPMiniCartControl" %>

<div class="gdo-clear gdo-horiz-div"></div>
<asp:Panel ID="pnlHAPOptions" runat="server" EnableViewState="true">
    <table id="tbHAPOrderType" runat="server">
        <tr>
            <td>
            <asp:Label ID="lblTypeTitle" runat="server" meta:resourcekey="OrderTypeTitle" CssClass="strong" ></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="Label1" runat="server" meta:resourcekey="OrderTypeOption"></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rbPersonal" runat="server" meta:resourcekey="PersonalRBText" GroupName="Saletype" AutoPostBack="true" OnCheckedChanged="Saletype_CheckedChanged" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rbResale" runat="server" meta:resourcekey="ResaleRBText" GroupName="Saletype" AutoPostBack="true" OnCheckedChanged="Saletype_CheckedChanged" />
            </td>
        </tr>
    </table>
    <div class="gdo-clear"></div>
    <table>
        <tr>
            <td>
                <asp:Label ID="lbltitleSchedule" runat="server" meta:resourcekey="ScheduleTitle" ></asp:Label>
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rb1" runat="server" meta:resourcekey="rb1Option" GroupName="hapDates" AutoPostBack="true" OnCheckedChanged="hapSchedule_CheckedChanged" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rb2" runat="server" meta:resourcekey="rb2Option" GroupName="hapDates" AutoPostBack="true" OnCheckedChanged="hapSchedule_CheckedChanged" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:RadioButton ID="rb3" runat="server" meta:resourcekey="rb3Option" GroupName="hapDates" AutoPostBack="true" OnCheckedChanged="hapSchedule_CheckedChanged" />
            </td>
        </tr>
    </table>
</asp:Panel>
<asp:Panel ID="pnlHAPOptionsStatic" runat="server">
    <div>
        <asp:Label ID="lblTypeTitleStatic" runat="server" meta:resourcekey="OrderTypeTitle" CssClass="strong" ></asp:Label><br />
        <asp:Label ID="lblOrderTypeSelected" runat="server" Text="Personal" meta:resourcekey="PersonalRBText"></asp:Label>
    </div>
    <div>
        <asp:Label ID="lbltitleScheduleStatic" runat="server" meta:resourcekey="ScheduleTitle" ></asp:Label><br />
        <asp:Label ID="lblScheduleSelected" runat="server" Text="4th" meta:resourcekey="rb1Option"></asp:Label>
    </div>
</asp:Panel>

