<%@ Page Title="" 
    Language="C#" 
    MasterPageFile="~/MasterPages/Ordering.master" 
    AutoEventWireup="true" 
    CodeBehind="Error.aspx.cs" 
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Error"   %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
<br/>

    <div id="ContentDisplay">
        <cc1:ContentReader ID="_ContentReader" runat="server" ContentPath="" SectionName="Ordering" Visible="false" ValidateContent="false"/>
    </div>

    <asp:BulletedList runat="server" ID="uxErrores" Font-Bold="True" ForeColor="Red"></asp:BulletedList>

<br/>
</asp:Content>
