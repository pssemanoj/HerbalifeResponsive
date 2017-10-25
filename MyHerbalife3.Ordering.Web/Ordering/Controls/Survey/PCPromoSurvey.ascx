<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PCPromoSurvey.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Survey.PCPromoSurvey" %>
<div class="promo-survey">
    <table style="width: 100%">
        <tr>
            <td colspan="2">
                <asp:Label ID="lblError" Text="" runat="server" ForeColor="#FF0000" Font-Bold="True" Font-Size="10pt"></asp:Label>
            </td>
        </tr>
        <tr>
            <asp:Label ID="lblMessage" Text="一旦您取消了调查，你将无法获得免费的礼物。" ForeColor="#FF0000" Font-Bold="True" Font-Size="10pt" runat="server"></asp:Label>
        </tr>
          <tr>
            <asp:Label ID="lblMessage1" Text="若此次放弃填写，重新登陆后，您可再次参与调研！" ForeColor="#FF0000" Font-Bold="True" Font-Size="10pt" runat="server"></asp:Label>
        </tr>
        <tr>
            <td colspan="2"><b>[尊敬的贵宾客户]</b>：</td>
        </tr>
        <tr>
            <td colspan="2">[您好！感谢您对公司的大力支持和信任！为了更好地倾听您的心声，进一步了解您对公司物流、系统、产品等方面的满意度，我们特开展此次满意度调查活动，敬请填写以下满意度调查问卷，以便我们能为您提供更好的服务。]</td>
        </tr>
        <tr>
            <td colspan="2">&nbsp;</td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:Label ID="lblQuestion1" runat="server" Text="[您觉得目前公司做的最让您满意的地方是什么？]" CssClass="bold"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption1" runat="server" Text="[物流]:" AssociatedControlID="txtOption1"></asp:Label>
                <asp:TextBox ID="txtOption1" MaxLength="100" TabIndex="1" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption2" runat="server" Text="[订购系统]:" AssociatedControlID="txtOption2"></asp:Label>
                <asp:TextBox ID="txtOption2" MaxLength="100" TabIndex="2" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption3" runat="server" Text="[产品]:" AssociatedControlID="txtOption3"></asp:Label>
                <asp:TextBox ID="txtOption3" MaxLength="100" TabIndex="3" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption4" runat="server" Text="[其他]：" AssociatedControlID="txtOption4"></asp:Label>
                <asp:TextBox ID="txtOption4" MaxLength="100" TabIndex="4" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="promosurvey-title">
                <asp:Label ID="lblQuestion2" runat="server" Text="[您觉得目前公司哪些方面尚待改进，请不吝指出。]" CssClass="bold"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption5" runat="server" Text="[物流]：" AssociatedControlID="txtOption5"></asp:Label>
                <asp:TextBox ID="txtOption5" MaxLength="100" TabIndex="5" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption6" runat="server" Text="[订购系统]:" AssociatedControlID="txtOption6"></asp:Label>
                <asp:TextBox ID="txtOption6" MaxLength="100" TabIndex="6" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption7" runat="server" Text="[产品]：" AssociatedControlID="txtOption7"></asp:Label>
                <asp:TextBox ID="txtOption7" MaxLength="100" TabIndex="7" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Label ID="lblOption8" runat="server" Text="[其他]：" AssociatedControlID="txtOption8"></asp:Label>
                <asp:TextBox ID="txtOption8" MaxLength="100" TabIndex="8" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <td class="survey-question">
                <asp:Button ID="btnSubmit" runat="server" Text="提交" OnClick="btnSubmit_Click" CssClass="forward" />
                <asp:Button ID="btnCancel" runat="server" Text="取消" OnClick="btnCancel_OnClick" CssClass="backward" />
            </td>
        </tr>
    </table>

</div>
