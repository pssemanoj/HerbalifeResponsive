<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OrderPaymentResult.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OrderPaymentResult" %>

<% if (RedirectOrder)
   { %>
        <result><%= rtnOk %></result><redirecturl>https://cn.myherbalife.cn/Ordering/Confirm.aspx</redirecturl>
<% } %>