<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MPIAuth.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.KSNet.MPIAuth" EnableViewState="False" EnableEventValidation="False" EnableViewStateMac="False" ValidateRequest="False" ViewStateEncryptionMode="Never" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
<title>KSPay</title>
<meta http-equiv="Content-Type" content="text/html charset=euc-kr"/>

<script type="text/javascript" src="https://www.vpay.co.kr/KVPplugin_ssl_utf_test.js"></script>

<script type="text/javascript">
    function _init() 
    {
        StartSmartUpdate();
    }
</script>
<asp:Literal ID="InitCode" runat="server"></asp:Literal>
</head>
<body >
<form name="Visa3d" action="https://kspay.ksnet.to/totmpi/veri_host.jsp"  target="_self" method="post">
	<input type="hidden"   name="pan"				value=""/>
	<input type="hidden"   name="expiry"			value=""/>
	<input type="hidden"   name="purchase_amount"	value="<%=amount%>"/>
	<input type="hidden"   name="amount"			value="<%=amount%>"/>
	<input type="hidden"   name="description"		value="none"/>
	<input type="hidden"   name="currency"		    value="<%=currencytype%>"/>
	<input type="hidden"   name="recur_frequency"	value=""/>
	<input type="hidden"   name="recur_expiry"	    value=""/>
	<input type="hidden"   name="installments"	    value="<%=installments%>"/>   
	<input type="hidden"   name="device_category"	value="0"/>	
	<input type="hidden"   name="name"			    value="herbalife"/>
	<input type="hidden"   name="url"			    value="<%=rootUrl%>"/>
	<input type="hidden"   name="country"		    value="410"/>
	<input type="hidden" name="returnUrl" value="<%=returnUrl%>"/>
	<input type="hidden" name="dummy" value="<%=dummyUrl%>"/>
	<!-- 1. [This is compulsory]  -->
	<input type="hidden" name="cardcode" value="<%=cardcode%>"/>

<!------------------------------------------------------>
<!-- merinfo : Test value(DPT0006503), Real Value(DPT0002803)                           -->
<!-- bizNo : Herbalife Korea Business Number : KSNet test:1208197322  Hlf: 1208165742             -->
<!-- instType : Installment type ( 1 : normal, 2 : No interest )              -->
<!--                -->
<!------------------------------------------------------>
	<input type="hidden" name="merInfo" value="DPT0002803"/>
	<input type="hidden" name="bizNo" value="1208165742"/>
	<input type="hidden" name="instType" value="1"/>
    <input type="hidden" name="gubun"	value="1130" />
</form>

<asp:Literal ID="ActionCode" runat="server"></asp:Literal>   
  </body>
</html>

