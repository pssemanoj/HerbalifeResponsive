<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ISPAuth.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.KSNet.ISPAuth" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<script type="text/javascript" src="https://www.vpay.co.kr/KVPplugin_ssl_utf_test.js"></script>
<script type="text/javascript">
    _init();	
    
    function _init() 
    {
		StartSmartUpdate();
	}
</script>

<body>
    <form accept-charset="UTF-8" enctype="application/x-www-form-urlencoded" name="ISPFields" action="ISPAuth.aspx" target="AuthFrame" method="post">
		<%-- VPAY verification initialization fields --%>
		<input type="hidden" name="KVP_PGID" value="K0003" />
		<input type="hidden" name="KVP_GOODNAME" value="<%=goodname%>" />
		<input type="hidden" name="KVP_PRICE" value="<%=amount%>" />
		<input type="hidden" name="KVP_CURRENCY" value="WON" />
		<input type="hidden" name="KVP_NOINT_INF" value="none" />
		<input type="hidden" name="KVP_QUOTA_INF" value="<%=installments%>" />

		<%-- VPAY verification result fields --%>
		<input type="hidden" name="KVP_NOINT" />
		<input type="hidden" name="KVP_QUOTA" />
		<input type="hidden" name="KVP_CARDCODE" value="0100" />
		<input type="hidden" name="KVP_CONAME" />
		<input type="hidden" name="KVP_ENCDATA" value=""/>
		<input type="hidden" name="KVP_SESSIONKEY" value=""/>

		<%-- VPAY verification reserved fields --%>
		<input type="hidden" name="KVP_RESERVED1" />
		<input type="hidden" name="KVP_RESERVED2" />
		<input type="hidden" name="KVP_RESERVED3" />

		<input type="hidden" name="AuthorizeDisplay" value="1" />
		<input type="hidden" name="disableStep1" value="1" />
		<input type="hidden" name="disableStep2" value="0" />
		<input type="hidden" name="displayKMPIVar" value="1" />
	</form>

	<script type="text/javascript">
		onISPReturn(document.ISPFields, MakePayMessage(document.ISPFields));

		function onISPReturn(ispFields, verified) 
        {
           var form = document.parentWindow.parent.document.forms[0];
		    if (verified) 
            {
                if (null != document.parentWindow.parent.setReturnedValues) 
                {
                    document.parentWindow.parent.setReturnedValues("", "", "", ispFields.KVP_SESSIONKEY.value, ispFields.KVP_ENCDATA.value, "");
                }
			}
			else 
            {
                if (null != document.parentWindow.parent.setReturnedValues) 
                {
                    document.parentWindow.parent.setReturnedValues("", "", "", "", "", "");
                }
			}
        }
	</script>
</body>
</html>

