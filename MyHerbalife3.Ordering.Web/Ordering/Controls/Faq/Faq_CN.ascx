<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Faq_CN.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Faq.Faq_CN" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.LegacyProviders.ValueObjects" Assembly="MyHerbalife3.Shared.LegacyProviders" %>
<section class="div_1 FAQ_section">
    <div class="div_2 FAQ_title">&nbsp;问题集</div>
    <div class="div_3">
        &nbsp;<span>操作指南</span>
    </div>
    <table border="0" cellspacing="0" cellpadding="0" style="height: 25px">
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <asp:HyperLink ID="OnlineIntro" runat="server" Text="在线订购2.0快速入门" Target="_blank" meta:resourcekey="OnlineIntroResource"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <asp:HyperLink ID="OnlineOrderFaq" runat="server" Text="在线订购问与答" Target="_blank" meta:resourcekey="OnlineOrderFaqResource"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">  
                <asp:HyperLink ID="OnlineOrderTable" runat="server" Text="个人网银大额支付能力表" Target="_blank" meta:resourcekey="OnlineOrderTableResource"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <asp:HyperLink ID="CnpTable" runat="server" Text="CNP支付能力表" Target="_blank" meta:resourcekey="CnpTableResource"></asp:HyperLink>
            </td>
        </tr>
    </table>
    
    <div class="div_3">
        &nbsp;<span>自提点信息</span>
    </div>
    <table border="0" cellspacing="0" cellpadding="0" style="height: 25px">

        <%--<tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <asp:HyperLink ID="PickupInfo1" runat="server" Text="自提点信息－商店自提" Target="_blank" meta:resourcekey="PickupInfo1Resource"></asp:HyperLink>
            </td>
        </tr>--%>
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <asp:HyperLink ID="PickupInfo2" runat="server" Text="自提点信息－自提点自提" Target="_blank" meta:resourcekey="PickupInfo2Resource"></asp:HyperLink>
            </td>
        </tr>
    </table>
    
    <div class="div_3">
        &nbsp;<span>假期配送及自提影响时效</span>
    </div>
    <table border="0" cellspacing="0" cellpadding="0" style="height: 25px">
        <tr>
            <td align="center" style="width: 30px; height: 25px"></td>
            <td align="left" style="width: 600px; height: 25px; font-size: 14px">
                <cc1:ContentReader runat="server" ID="HolidayFileId" CssClass="FAQnoHoliday"/>
            </td>
        </tr>
    </table>
</section>