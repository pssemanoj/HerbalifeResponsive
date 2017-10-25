<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ProductLink.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.ProductLink" %>
<div>
    <div>
        <asp:Label class="blue" ID="lbFactSheets" runat="server" Text="Fact Sheets" 
            meta:resourcekey="lbFactSheetsResource1"></asp:Label>
    </div>
    <asp:Repeater runat="server" ID="uxProductLinks">
        <ItemTemplate>
            <a runat="server" id="pdfLinkIcon" target='<%# Eval("Target") %>' href='<%# Eval("Path") %>'>
                <img id="Img1" runat="server" alt="PDF Icon" src='<%# GetIconSrc(Eval("IconPath") as string ) %>' />
           </a> <a runat="server" id="pdfLink" target='<%# Eval("Target") %>' href='<%# Eval("Path") %>'><asp:Label 
                runat="server" ID="description" Text='<%# Eval("Title") %>' 
                meta:resourcekey="descriptionResource1"></asp:Label>
            </a>
        </ItemTemplate>
    </asp:Repeater>
</div>

