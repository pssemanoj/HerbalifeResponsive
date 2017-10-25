
<%@ Control Language="C#" AutoEventWireup="true" 
    CodeBehind="GetHeaderAssets.ascx.cs" 
    Inherits="MyHerbalife3.Web.Controls.getheader" %>

<%@ Import Namespace="MyHerbalife3.Shared.AssetsAdapter.Extensions" %>  
<%= MyHerbalife3.Shared.AssetsAdapter.AssetsHelper.GetAsset("HeaderAssets",locale,null) %> 