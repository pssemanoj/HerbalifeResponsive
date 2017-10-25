<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PGHInterface.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PGHInterface" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=ISO-8859-1">
    <meta content="text/html; charset=UTF-8" http-equiv="Content-Type">
    <title>Authentication</title>
    

    <script type="text/javascript" language="javascript">
        function LoadURL()
        {
<%--            document.forms[0].action = "<%=CurrentSession.ThreeDSecuredCardInfo.AcsUrl%>";
            document.forms[0].submit();--%>
        }

    </script>
    
</head>
<body onload="LoadURL()">
    <form name="form3D" method="post" action="">
        <input type='hidden' name='PaReq' value="" runat="server" id="PaReq" />
        <input type='hidden' name='TermUrl' value="" runat="server" id="TermUrl" />
        <input type='hidden' name='MD' value="" runat="server" id="MD" />
    </form>
</body>
</html>


