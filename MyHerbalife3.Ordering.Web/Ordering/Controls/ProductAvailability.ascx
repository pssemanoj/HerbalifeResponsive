<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductAvailability.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductAvailability" %>

    <div runat="server" id="divGreen">
        <table border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td nowrap="nowrap">
                    <asp:Image ID="uxGreen" runat="server" ImageUrl="/Content/Global/Products/Img/circle_green.gif" meta:resourcekey="uxGreenResource1" />
                    <asp:Label CssClass="styleGreen" runat="server" ID="uxGreenLabel" meta:resourcekey="uxGreenResource1" Text="Available"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
    <div runat="server" id="divRed">
        <table border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td nowrap="nowrap">
                    <asp:Image ImageAlign="AbsMiddle" ID="uxRed" runat="server" ImageUrl="/Content/Global/Products/Img/circle_red.gif" meta:resourcekey="uxRedResource1" />
                    <asp:Label CssClass="styleRed" runat="server" ID="uxRedLabel" meta:resourcekey="uxRedResource1" Text="Unavailable"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
    <div runat="server" id="divYellow">
        <table border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td nowrap="nowrap">
                    <asp:Image ImageAlign="AbsMiddle" ID="uxYellow" runat="server" ImageUrl="/Content/Global/Products/Img/circle_orange.gif" meta:resourcekey="uxYellowResource1" />
                    <asp:Label CssClass="styleOrange" runat="server" ID="uxYellowLabel" meta:resourcekey="uxYellowResource1" Text="Available for Backorder"></asp:Label>
                </td>
            </tr>
        </table>
    </div>
    <div runat="server" id="divBlue">
        <table border="0" cellspacing="0" cellpadding="0">
            <tr>
                <td nowrap="nowrap">
                    <asp:Image ImageAlign="AbsMiddle" ID="uxBlue" runat="server" ImageUrl="/Content/Global/Products/Img/circle_blue.gif" meta:resourcekey="uxBlueResource1" />
                    <asp:Label CssClass="styleBlue" runat="server" ID="uxBlueLabel" meta:resourcekey="uxBlueResource1" Text="Unavailable in primary warehouse"></asp:Label>
                </td>
            </tr>
        </table>
    </div>

