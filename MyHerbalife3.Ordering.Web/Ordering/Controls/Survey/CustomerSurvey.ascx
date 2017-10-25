<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CustomerSurvey.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Survey.CustomerSurvey" %>
<%@ Register TagPrefix="content" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<style type="text/css">
    .cssNeedComment {
        width:75% !important;
        margin-left:10pt;
    }
    .greenCustomerSurvey {
  color: #7AC143;
   font-weight:bold;
}
</style>
<script type="text/javascript">
    function OnSurveyOptionChange(uiObj) {
        var sectList = $(".<%=Css_NeedComment_Section%>");
        for (var i = 0; i < sectList.length; i++) {
            var sectUi = $(sectList[i]);

            var asscUi = $("#" + sectUi.attr("<%=Attribute_ControlToAssociate%>"));
            var needComment = asscUi.is(":checked");

            var vldtUi = $("#" + sectUi.attr("<%=Attribute_ValidatorId%>"));

            // only this will work for ValidatorEnable()
            ValidatorEnable(vldtUi[0], needComment);
            
            if (needComment) 
                vldtUi.css("visibility", "hidden"); // hide the err msg, have to use this approach.
        }
    }

    function NeedCommentPostValidation() {
        if (Page_IsValid) return;

        for (var i = 0; i < Page_Validators.length; i++) {
            var vldt = Page_Validators[i];
            if (vldt.isvalid) continue;

            var uiObj = $("#" + vldt.controltovalidate);
            uiObj.focus();

            var y = 100;
            var mainNav = $("#main-nav");
            if ((mainNav != null) && (mainNav.length > 0)) y = (mainNav.outerHeight() + 100);
            window.scrollBy(0, -y);

            return;
        }
    }

    $(document).ready(function () {
        OnSurveyOptionChange(null); // hide all the need-comment validator on startup
    });
</script>
 <div class="greenCustomerSurvey">
<table style="width: 100%">
     <tr>
            <asp:Label ID="lblMessage" Text="&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 亲爱的伙伴，首先恭喜您成为康宝莱网上调研对象，您只需完成如下调研问卷，就会有一份调研礼品 随您的此次订单发送，先到先得，送完为止噢！"  runat="server"></asp:Label>
        </tr>
          <tr>
              <br/>
              <br/>
             <asp:Label ID="lblMessage1" Text="请您在提交问卷前，仔细阅读以下礼品领取说明：" runat="server"></asp:Label>
        </tr>
    </table>
<div id="dsAdminWrap">
    <h1>

        <asp:Literal ID="TitlePromt" runat="server" meta:resourcekey="TitlePromt" />
    </h1>
         
    <div id="dsAdminContent" class="tutorial-wrap">
       
               1，	当您成功完成此次调研后，并在下单时选择送货上门或自提点自提方式配送，调研礼品将随您的订单发送；
            <br/>
2，	调研机会只有一次，先到先得，送完为止，且将随您此次订单送达收货地址；
              <br/>
3，	如有疑问，请您致电400服务热线咨询。
            <br/>
              <br/>
            如您暂时不方便接受调研，请点击页面下方的“取消”继续购货，谢谢！

        </div>
    </div>
    </div>
   
        <h2>
            <asp:Literal ID="SectionTitlePromt" runat="server" meta:resourcekey="SectionPromt" /></h2>
        <asp:Label CssClass="errMsg" ID="lblErrorMessage" ForeColor="#FF0000" Text="" runat="server" Font-Bold="True" Font-Size="10pt" meta:resourcekey="MissingAnswerError" Visible="false" />
        <div class="content-body tutorial-content tutorial-quiz customer-survey">
            <br/>
              <br/>
            <asp:PlaceHolder ID="plQuestion" runat="server"></asp:PlaceHolder>
        </div>

        <div id="actionsBar"> <%--class="action-buttons">--%>
            <content:DynamicButton ID="btnSave" Text="提交" runat="server" OnClick="BtnSaveClick" OnClientClick="NeedCommentPostValidation()"  CssClass="forward" />
            <content:DynamicButton ID="btnCancel" Text="取消" runat="server" OnClick="BtnCancelClick"  CssClass="backward" CausesValidation="false" />
        </div>
   

