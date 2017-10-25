<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PriceListGeneratorPrintable.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.PriceListGeneratorPrintable" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Price List Generator</title>
    <style type="text/css">
        
        body {
            padding: 25px;
        }
        .printBtn {
            border: none;
            background-color: #555;
            color: #fff;
            border-radius: 5px;
            padding: 10px;
            font-weight: bold;
            cursor: pointer;
        }

        @media print {

            .printBtn {
            display: none;
            }

            h2 {
                background-color: #7A7A7A;
                -webkit-print-color-adjust: exact; 
            }

            body {
                padding: 0;
            }
        }
    </style>
    <script>
        var d = document;
        var printBtn = d.getElementById('<%= btnPrint.ClientID %>');
        //var form = d.getElementById('form1');
        
        function sendToPrint(e) {
            window.print();
            if (e && e.preventDefault) {
                e.preventDefault();
            }
            return false;
        }
    </script>
</head>
<body>
    <form id="form1" runat="server" style="padding: 30pt;">
        <div align="center">
            <%--<button id="printBtn" onclick="sendToPrint();"><%= GetLocalResourceObject("PrintPDF") as string %></button>--%>
            <asp:Button ID="btnPrint" runat="server" Text="Print Page" OnClientClick="sendToPrint();" CssClass="printBtn" meta:resourcekey="PrintPDF" />
            <asp:Button ID="btnSaveXLS" runat="server" Text="Save To Excel" OnClick="btnSaveXLS_Click" CssClass="printBtn" meta:resourcekey="SaveXLS" />
        </div>
        <div runat="server" id="divHTMLContent">
        <table border="0" width="100%">
            <%--Header--%>
            <tr>
                <td>
                    <img src="/SharedUI/Images/logo_HID_300dpi.jpg" style="width:200px;" runat="server" id="imgLogo"/>
                    <p style="color: #000000; font-family: Verdana; font-size: 8pt;">
                        <asp:Label runat="server" ID="TaxInformation"></asp:Label>
                    </p>
                </td>
                <td align="right">
                    <p style="font-size: 13pt; color: #000000; font-family: Verdana;">
                        <asp:Label runat="server" ID="lblDistributorName"></asp:Label>
                    </p>
                    <p style="font-size: 11pt; color: #000000; font-family: Verdana;">
                        <asp:Label runat="server" ID="lblTaxGeoInformation"></asp:Label>
                    </p>
                </td>
            </tr>
            <asp:Repeater runat="server" ID="reCategory" OnItemCreated="CategoryCreated">
                <ItemTemplate>
                    <tr>
                        <td colspan="2">
                                
                            <div style="margin: 2px 0;">
                                <h2 style="font-weight: bold; font-family:'Arial Narrow'; font-size: 15pt; color: white; padding: 10px 5px 5px 5px; background-color: #7A7A7A; text-transform: uppercase; margin: 5px 0;" class="headerXLS">
                                    <%# DataBinder.Eval(Container.DataItem, "Key").ToString().ToUpper() %>
                                </h2>
                                <asp:Repeater runat="server" ID="reProductCategory" OnItemCreated="ProductCategoryCreated">
                                    <ItemTemplate>
                                        <table border="0" cellspacing="0" width="97%" cellpadding="2" style="margin-left: 3%; margin-top: 1%">
                                            <colgroup>
                                                <col />
                                                <col />
                                                <col />
                                                <col />
                                                <col />
                                                <col />
                                                <col />
                                            </colgroup>
                                            <thead>
                                                <tr align="left">
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="SKUColumn" width="10%"><%= GetLocalResourceObject("SkuColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="ProductColumn"><%= GetLocalResourceObject("ProductNameColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="VolumePointsColumn" width="10%"><%= GetLocalResourceObject("VolumePointsColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="EarnBaseColumn" width="10%"><%= GetLocalResourceObject("EarnBaseColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="CustomerRetailPriceColumn" width="10%"><%= GetLocalResourceObject("RetailPriceColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="RetailPriceColumn" width="10%"><%= GetLocalResourceObject("RetailPriceColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="DiscountedRetailColumn" width="10%"><%= GetLocalResourceObject("DistributorLoadedCostColumnName") as string %></th>
                                                    <th style="font-family: 'Arial Narrow'; font-size: 8pt; font-weight: lighter; color: #000; border-bottom: 1px solid #000000; font-weight: bold; text-align: left" runat="server" id="CustomerPriceColumn" width="10%"><%= GetLocalResourceObject("CustomerPriceColumnName") as string %></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                            </tbody>
                                        </table>
                                        <h3 style="font-weight: lighter; font-family: 'Arial Narrow'; font-size: 12pt; color: #000000; border-bottom: 5px solid #7a7a7a; padding-bottom: 2px; margin: 5px 0; text-transform: uppercase;">
                                            <%# DataBinder.Eval(Container.DataItem, "Key").ToString().ToUpper() %>
                                        </h3>
                                        <asp:Repeater runat="server" ID="reProducts" OnItemCreated="ProductsCreated">
                                            <ItemTemplate>
                                                <h4 style="font-weight: bold; color: #000000; font-family: 'Arial Narrow'; font-size: 9pt; border-bottom: 1px solid #808080; padding-bottom: 5px; margin: 5px 0 0 3%;">
                                                    <%# DataBinder.Eval(Container.DataItem, "Key")%>                                            
                                                </h4>
                                                <table border="0" cellspacing="0" width="97%" cellpadding="2" style="margin-left: 3%">
                                                    <colgroup>
                                                        <col />
                                                        <col />
                                                        <col />
                                                        <col />
                                                        <col />
                                                        <col />
                                                        <col />
                                                    </colgroup>
                                                    <tbody>
                                                        <asp:Repeater runat="server" ID="reProducts" OnItemCreated="ProductCreated">
                                                            <ItemTemplate>
                                                                <tr>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="SKURow" width="10%" class="textmode"><%# DataBinder.Eval(Container.DataItem, "Sku") %></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="ProductRow"><%# DataBinder.Eval(Container.DataItem, "ShortDescription")%></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="VolumePointsRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "VolumePoints")%></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="EarnBaseRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "EarnBase")%></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="CustomerRetailPriceRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "CustomerRetailPrice") %></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="RetailPriceRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "RetailPrice")%></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="DiscountedRetailRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "DistributorLoadedCost")%></td>
                                                                    <td style="font-family: 'Arial Narrow'; font-size: 8pt; color: #000; border-bottom: 1px solid #cccccc" runat="server" id="CustomerPriceRow" width="10%" align="left"><%# DataBinder.Eval(Container.DataItem, "CustomerPrice") %></td>
                                                                </tr>
                                                            </ItemTemplate>
                                                        </asp:Repeater>
                                                    </tbody>
                                                </table>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>
        </div>
    </form>
</body>
</html>
