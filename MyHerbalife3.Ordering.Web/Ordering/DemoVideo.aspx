<%@ Page Title="Cómo comprar" Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true"
    CodeBehind="DemoVideo.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.DemoVideo"   EnableEventValidation="false" meta:resourcekey="PageResource1" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>

<asp:Content ID="Content3" ContentPlaceHolderID="ProductRecomendationsContent" runat="server">
    <style>
        .box {
    background-color: #0080ff;
    color: white !important;
    padding: 10px 30px;
    text-align: center;
    text-decoration: none;
    display: inline-block;
    width: max-content;
    }
    </style>
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <span>
        <asp:Label ID="message1" Font-Size="Medium" Font-Bold="true" runat="server" Text="message"  meta:resourcekey="message1"></asp:Label>
    </span>
    <br />
    <span>
        <asp:Label ID="message2" runat="server" Text="Message2"  meta:resourcekey="message2"></asp:Label>
    </span>
   <video width="750" height="450" controls="controls">
  <source src="https://httpsak-a.akamaihd.net/4108187551001/4108187551001_5530346940001_5530339060001.mp4?pubId=4108187551001&videoId=5530339060001" type="video/mp4" />
   </video>
    <br />   
    <div style="padding-left:560px">
        <asp:HyperLink class="box"  runat="server"    ID="MenuFAQ" Text="FAQ" Target="_blank"  meta:resourcekey="FAQResource1"></asp:HyperLink>
        </div>
    </asp:Content>