<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LocaleSelector.ascx.cs" Inherits="MyHerbalife3.Web.Controls.Logon.LocaleSelector" EnableViewState="false" %>
<%@ Import Namespace="System.Globalization" %>

<select class="_localeSelector">
    <%  
        var locale = CultureInfo.CurrentUICulture.Name;
        foreach (ListItem entry in AllCountries)
        {
    %>
        <option value='<%= entry.Value %>' <%=entry.Value == locale ? "selected" : string.Empty %>><%= entry.Text %></option>
    <%
        }
    %>
</select>