<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Error" meta:resourcekey="PageResource1"  %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title runat="server">Error</title>
    <link rel="stylesheet" href="/Content/Global/Controls/Error/css/Error.css" type="text/css" />    
    <link rel="stylesheet" href="/Content/<%= System.Globalization.CultureInfo.CurrentCulture.Name %>/css/<%= System.Globalization.CultureInfo.CurrentCulture.Name %>.css" type="text/css"/>
     <link rel="stylesheet" href="/Content/<%= System.Globalization.CultureInfo.CurrentCulture.Name %>/Error/css/<%= System.Globalization.CultureInfo.CurrentCulture.Name %>_Error.css"  type="text/css" />

</head>
<body>
    <form id="form1" runat="server">
    <asp:Localize ID="_ErrorText" runat="server" 
        meta:resourcekey="_ErrorTextResource1" Text="[An unexpected error has occured.]"></asp:Localize>
    </form>
</body>
</html>
