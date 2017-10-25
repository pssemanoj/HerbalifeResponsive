<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Advertisement.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Advertisement" %>
  <div class="divclass" id="divPanel" runat="server">

<div  class="divresponsive"  >
      <div  class="maincontent">
         <a style="float:right;position:relative;margin-right:20px;margin-bottom:10px" href="javascript:void(-1)" id="divClode"> <input value="×"  type="button"  style="background-color:white;border-bottom-color:lightgray" runat="server"/> </a> 
    <div>
        <div style="margin-top: -40px;  float: left;">
       <img  src="#"  ID="adviserPic" />
            </div>
    </div>
          </div>
</div>

      </div>
 <style>

     .divresponsive {
         float: left;
         margin-top: 100px;
         padding-bottom: 20px;
     }
     .divclass{
         float:left;
         position:fixed;
         z-index:2000; 
         display:none;
          
     }
    
 </style>
<script type="text/javascript" >
    $("document").ready(function () {
        if ($(document).width() > 800) {
            var width = "" + ($(window).width() / 2 - 200) + "px";
            var height = "" + ($(window).height() / 2 - 200) + "px";
            $(".divresponsive").css("margin-left", width);
            $(".divresponsive").css("margin-top", height);
            $(".divclass").css("width", $(document).width());
            $(".divclass").css("height", $(document).height());
        }
        var promotiondays = 3;
        var curdate = new Date();
        var imgsrs = [];
        for (var i = 0; i < promotiondays; i++) {
            if (i > 0) {
                curdate.setDate(curdate.getDate() - 1);
            }
            var strimg = "" + curdate.getFullYear() + "" + (curdate.getMonth() + 1) + "" + (curdate.getDate());
            var imgurl = "/Content/zh-CN/img/Catalog/icon/adv" + strimg + ".png";
            imgsrs.push({ url: strimg, status: false });
            $.ajax({
                url: imgurl,
                method: "get",
                success: function (req, data, res) {
                    if ($("#adviserPic").length > 0) {
                        if (adviserPic.src.indexOf("#") > -1) {
                            adviserPic.src = this.url;
                            $(".divclass").show();
                            setTimeout(function () {
                                $(".divclass").hide(1000);
                            }, 3000);
                        }
                    }
                }
            })
        }
        $("#divClode").click(function () {
            $(".divclass").hide(1000);
        });
    });
   
</script>
 