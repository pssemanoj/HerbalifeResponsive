<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FreightSimulator.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.FreightSimulator" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<asp:Panel ID="Panel1" runat="server">
    <table class="freight-simulation">
        <tr>
            <td>
                <div class="row">
                    <div class="col-sm-1">
                <asp:Label runat="server" ID="lbDestination" Text="Destination:" meta:resourcekey="lbDestinationResource"/>
                    </div>
                    <div class="col-sm-2">
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="ddlState" meta:resourcekey="ddlStateResource"
                    OnSelectedIndexChanged="ddlStateProvince_SelectedIndexChanged">
                </asp:DropDownList>
                    </div>
                    <div class="col-sm-2">
                        <asp:DropDownList AutoPostBack="True" runat="server" ID="ddlCity" meta:resourcekey="ddlCityResource"
                    OnSelectedIndexChanged="ddlCity_SelectedIndexChanged">
                </asp:DropDownList>
                    </div>
                    <div class="col-sm-2">
                        <asp:DropDownList AutoPostBack="False" runat="server" ID="ddlDistrict" meta:resourcekey="ddlDistrictResource">
                </asp:DropDownList>
                    </div>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <div class="row">
                    <div class="col-sm-1">
                <asp:Label runat="server" ID="lbWeight" Text="Weight:" meta:resourcekey="lbWeightResource"/>
                    </div>
                    <div class="col-sm-3">
                        <asp:TextBox runat="server" ID="txtWeight"></asp:TextBox> 
                <ajaxToolkit:FilteredTextBoxExtender ID="ftxtWeight" runat="server" TargetControlID="txtWeight" FilterType="Numbers" />   
                <asp:Label runat="server" ID="lbWeightUnit" Text="(kg)" meta:resourcekey="lbWeightUnitResource"/>                
                    </div>
                    <div class="align-right col-sm-3">
                        <cc1:DynamicButton runat="server" ID="btnCalculate" ButtonType="Forward" Text="Calculate"
                    meta:resourcekey="btnCalculateResource" OnClick="OnCalculate"/>
                    </div>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="pnlMessages" runat="server">
                    <asp:Label ID="lblError" runat="server" CssClass="gdo-error-message-txt"></asp:Label>
                </asp:Panel>                
            </td>
        </tr>
        <tr>
            <td style="margin: 50px 50px 50px 50px;">
                <div runat="server" ID="divResult" Visible="False">
                    <div class="gdo-right-column-header">
                        <h3>
                            <asp:Label ID="lbGridTitle" runat="server" Text="Estimated freight amount" meta:resourcekey="lbGridTitleResource"/>
                        </h3>
                    </div>
                    <div class="gdo-clear gdo-horiz-div"></div>
                    <asp:GridView ID="grvFreightInfo" runat="server" AutoGenerateColumns="False" 
                        meta:resourcekey="grvFreightInfoResource" CssClass="gdo-order-tbl spacepixel" EnableModelValidation="True">
                        <AlternatingRowStyle CssClass="gdo-row-odd gdo-order-tbl-data"></AlternatingRowStyle>
                        <Columns>
                            <asp:BoundField ReadOnly="True" HeaderText="Shipment" meta:resourceKey="hdrShipment"
                              InsertVisible="False" DataField="Shippment">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ReadOnly="True" HeaderText="State" meta:resourceKey="hdrState"
                              InsertVisible="False" DataField="State">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ReadOnly="True" HeaderText="City" meta:resourceKey="hdrCity"
                              InsertVisible="False" DataField="City">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ReadOnly="True" HeaderText="District" meta:resourceKey="hdrDistrict"
                              InsertVisible="False" DataField="District">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ReadOnly="True" HeaderText="Estimated weight" meta:resourceKey="hdrWeight"
                              InsertVisible="False" DataField="Weight">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                            <asp:BoundField ReadOnly="True" HeaderText="Estimated freight" meta:resourceKey="hdrFreight"
                              InsertVisible="False" DataField="Freight">
                                <ItemStyle HorizontalAlign="Center"></ItemStyle>
                            </asp:BoundField>
                        </Columns>
                        <HeaderStyle CssClass="gray_background" Height="30px" />
                        <RowStyle CssClass="gdo-row-even gdo-order-tbl-data" />
                    </asp:GridView>
                </div>
            </td>
        </tr>
    </table>
</asp:Panel>
