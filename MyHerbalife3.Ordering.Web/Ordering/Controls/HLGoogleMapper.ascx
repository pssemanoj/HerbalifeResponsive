<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HLGoogleMapper.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.HLGoogleMapper" %>
<%@ Import Namespace="MyHerbalife3.Ordering.Configuration.ConfigurationManagement" %>
<head>
    <title></title>

   
    
</head>
<body>
   <% if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasBaiduMap)
      { %> 
    <style type="text/css">
		body, html{width: 100%;height: 100%;margin:0;font-family:"微软雅黑";}
		#l-map{width:100%;}
		</style>

    <%--<script type="text/javascript" src="http://api.map.baidu.com/api?v=2.0&ak=C5f07c85b67ca043453d32fbdda7b3d6"></script>--%>
    <div id="l-map"></div>
	<div id="r-result">
		<%--<input type="button" value="Get Address" onclick="bdGEO()" />--%>
		<div id="result"></div>
	</div>
    <% } %>
     <% else if (HLConfigManager.Configurations.PickupOrDeliveryConfiguration.HasGoogleMap)
        { %> 
           <script type="text/javascript" src="https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false&callback=initialize"></script>
     <div id="dvMap" style="width: 500px; height: 300px;border:2px solid #ccc">
     </div>
    <% } %>
    
</body>