<%@ Page Language="C#" MasterPageFile="~/MasterPages/Ordering.master" AutoEventWireup="true" CodeBehind="OnlineInvoiceDetail.aspx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.OnlineInvoiceDetail" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ProductsContent" runat="server">
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server"> </asp:ScriptManagerProxy>
    <script type="text/javascript">
        function isNumberKey(evt)
        {
            evt = (evt) ? evt : window.event;
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                return false;
            }
            return true;
            
        }

        function amountValidator(event,sender) {
            if (event.shiftKey === true) {
                event.preventDefault();
            }
            if ((event.keyCode >= 48 && event.keyCode <= 57) || (event.keyCode >= 96 && event.keyCode <= 105) || event.keyCode === 8 || event.keyCode === 9 || event.keyCode === 37 || event.keyCode === 39 || event.keyCode == 46 || event.keyCode === 190) {

            } else {
                event.preventDefault();
            }

            if ($(sender).val().indexOf('.') !== -1 && event.keyCode == 190)
                event.preventDefault();
        }

        $(document).ready(function () {
            $('#<%=btnOK.ClientID%>').attr('href', 'OnlineInvoice.aspx');
            $('#<%=btnOKError.ClientID%>').attr('href', 'OnlineInvoice.aspx');
            $('#<%=btnCancelPopup.ClientID%>').attr('onclick', 'return false;');
            $('#<%=btnConfirmPopup.ClientID%>').attr('onclick', 'return false;');
            $('#<%=btnConfirm.ClientID%>').css('display', 'none');
             $('#<%=btnSplitConfirm.ClientID%>').css('display', 'none');
            

            $('#<%=btnConfirmPopupSplit.ClientID%>').attr('onclick', 'return false;');
            $('#<%=btnCancelPopupSplit.ClientID%>').attr('onclick', 'return false;');

            

            $('#<%=btnConfirmPopup.ClientID%>').click(function () {
                var bcgDiv = document.getElementById("divBackgroundConfirm");
                var fromtDiv = document.getElementById("divConfirmSubmit");
                bcgDiv.style.display = "none";
                fromtDiv.style.display = "none";
              
                $('#<%=btnConfirm.ClientID%>').click();
            });

            $("#<%= btnCancelPopup.ClientID%>").click(function() {

                var bcgDiv = document.getElementById("divBackgroundConfirm");
                var fromtDiv = document.getElementById("divConfirmSubmit");
                bcgDiv.style.display = "none";
                fromtDiv.style.display = "none";
                  $("#<%=bttnDummy.ClientID%>").removeAttr("disabled");
            });


             $('#<%=btnConfirmPopupSplit.ClientID%>').click(function () {
                 var bcgDiv = document.getElementById("divBackgroundSplitConfirm");
                 var fromtDiv = document.getElementById("divConfirmSplitSubmit");
                bcgDiv.style.display = "none";
                fromtDiv.style.display = "none";
                $('#<%=btnSplitConfirm.ClientID%>').click();
            });

            $("#<%= btnCancelPopupSplit.ClientID%>").click(function() {

                var bcgDiv = document.getElementById("divBackgroundSplitConfirm");
                var fromtDiv = document.getElementById("divConfirmSplitSubmit");
                bcgDiv.style.display = "none";
                fromtDiv.style.display = "none";
                
            $("#<%=btnsplitDummy.ClientID%>").removeAttr("disabled");
            });
        });

        function ShowMessage() {
            var bcgDiv = document.getElementById("divBackground");
            var fromtDiv = document.getElementById("divConfirm");

            bcgDiv.style.display = "block";
            fromtDiv.style.display = "block";
          
        }


        function ShowMessageError() {
            var bcgDiv = document.getElementById("divBackgroundError");
            var fromtDiv = document.getElementById("divConfirmError");

            bcgDiv.style.display = "block";
            fromtDiv.style.display = "block";
           
        }

        function EnableDummy() {
    $('#<%=bttnDummy.ClientID%>').removeAttr('disabled');
        }
  function EnableSpitDummy() {
    $('#<%=btnsplitDummy.ClientID%>').removeAttr('disabled');
        }
        function ShowConfirmationMessage() {
            var bcgDiv = document.getElementById("divBackgroundConfirm");
            var fromtDiv = document.getElementById("divConfirmSubmit");

            bcgDiv.style.display = "block";
            fromtDiv.style.display = "block";
             $("#<%=bttnDummy.ClientID%>").attr("disabled", "disabled");
        }

        function ShowSplitConfirmationMessage() {
            var bcgDiv = document.getElementById("divBackgroundSplitConfirm");
            var fromtDiv = document.getElementById("divConfirmSplitSubmit");

            bcgDiv.style.display = "block";
            fromtDiv.style.display = "block";
            $("#<%=btnsplitDummy.ClientID%>").attr("disabled", "disabled");
        }
        
 
    </script>
    <div id = "divBackground" class="InvoiceBackground" ></div>
    <div id="divConfirm" style="border:solid; border-color:gray; border-width:10px;position: fixed;z-index: 999;display:none;background-color: White; align-content:center;height: 25%; width: 50%; left:250px " >
        <table >
            <tr>
                <td><div align="center" ><asp:Label ID="lblInvoiceOrderTitle" runat="server" meta:resourcekey="InvoiceDetailTitle" Font-Size="XX-Large" Font-Bold="true" ></asp:Label></div></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="ThankYouLabel" meta:resourcekey="InvoiceDetailBillingComplete"></asp:Label></td>
            </tr>
            <tr>
                <td><div align="center"><cc1:DynamicButton ID="btnOK" runat="server" ButtonType="Forward" meta:resourcekey="InvoiceDetailConfirm" /></div></td>
            </tr>
        </table>
    </div>
    
    <div id = "divBackgroundConfirm" class="InvoiceBackground" ></div>
    <div id="divConfirmSubmit" style="border:solid; border-color:gray; border-width:10px;position: fixed;z-index: 999;display:none;background-color: White; width: 200px; height: 100px; align-content:center; top: 50%; left: 50%; margin-top: -50px; margin-left: -100px; " >
        <div style="margin-top:20px;padding-left:40px">
            <asp:Label  runat="server" ID="lblconfirmMessage"  meta:resourcekey="InvoiceDetailConfirmMessage"></asp:Label>
        </div>
        
        
                <div style="margin-top:10px;padding-right:55px">
                       <div  style="float:right"><cc1:DynamicButton ID="btnCancelPopup" runat="server" ButtonType="Cancel" meta:resourcekey="InvoiceDetailButtonCancel" /></div>
                    <div style="float:right"  ><cc1:DynamicButton ID="btnConfirmPopup" runat="server" ButtonType="Forward" meta:resourcekey="InvoiceDetailConfirm" /></div>
                
                </div>
          
    </div>
    <div id = "divBackgroundSplitConfirm" class="InvoiceBackground" ></div>
    <div id="divConfirmSplitSubmit" style="border:solid; border-color:gray; border-width:10px;position: fixed;z-index: 999;display:none;background-color: White; width: 200px; height: 100px; align-content:center; top: 50%; left: 50%; margin-top: -50px; margin-left: -100px; " >
        <table >
            <tr>
                <td align="center"><asp:Label runat="server" ID="Label3" meta:resourcekey="InvoiceDetailConfirmMessage"></asp:Label></td>
            </tr>
            <tr>
                <td><div align="center"><cc1:DynamicButton ID="btnConfirmPopupSplit" runat="server" ButtonType="Forward" meta:resourcekey="InvoiceDetailConfirm" /></div></td>
                <td><div align="center"><cc1:DynamicButton ID="btnCancelPopupSplit" runat="server" ButtonType="Cancel" meta:resourcekey="InvoiceDetailButtonCancel" /></div></td>
            </tr>
        </table>
    </div>
    
    <div id = "divBackgroundError" class="InvoiceBackground" ></div>
    <div id="divConfirmError" style="border:solid; border-color:gray; border-width:10px;position: fixed;z-index: 999;display:none;background-color: White; width: 200px; height: 100px; align-content:center; top: 50%; left: 50%; margin-top: -50px; margin-left: -100px; " >
        <table >
            <tr>
                <td><div align="center" ><asp:Label ID="Label1" runat="server" meta:resourcekey="InvoiceDetailTitle" Font-Size="XX-Large" Font-Bold="true" ></asp:Label></div></td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="Label2" meta:resourcekey="ErrorRdcIdNotFound"></asp:Label></td>
            </tr>
            <tr>
                <td><div align="center"><cc1:DynamicButton ID="btnOKError" runat="server" ButtonType="Forward" meta:resourcekey="InvoiceDetailConfirm" /></div></td>
            </tr>
        </table>
    </div>
    <div id="divDetailSingle" style="border:solid; width:900px;padding: 10px;" runat="server">
        <br />
        <asp:UpdatePanel ID="upLoadOrdersDetail" runat="server" UpdateMode="Always">
        <ContentTemplate>
        <asp:Panel ID="panelSingle" runat="server"  >
        <asp:BulletedList ID="blErrors" runat="server" BulletStyle="Disc" ForeColor="Red"></asp:BulletedList>
        <table>
            <tr>
                <td><asp:Label runat="server" ID="lblBilling" meta:resourcekey="InvoiceDetailBillingAmount"></asp:Label></td>
                <td>
                    <label id="lblInvoiceAmount" runat="server" ></label>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="lblInvoiceType" meta:resourcekey="InvoiceDetailInvoiceType"></asp:Label></td>
                <td>
                    <asp:RadioButtonList ID="rbInvoiceType" RepeatLayout="Flow" runat="server" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rbInvoiceType_SelectedIndexChanged">
                        <asp:ListItem Selected="True" Value="PE" meta:resourcekey="InvoiceDetailRadioButtonPersonal"></asp:ListItem>
                        <asp:ListItem Value="CO" meta:resourcekey="InvoiceDetailRadioButtonCorporate"></asp:ListItem>
                    </asp:RadioButtonList> 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" id="lblInvoiceTitle" meta:resourcekey="InvoiceDetailInvoiceTitle"></asp:Label></td>
                <td style="text-align:left">
                    <asp:RadioButtonList ID="rbInvoiceTitle" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" AutoPostBack="true" OnSelectedIndexChanged="rbInvoiceTitle_SelectedIndexChanged" />
                    <asp:TextBox ID="txtInvoiceTitleCorporate" Visible="false" runat="server" />
                    <asp:TextBox ID="txtInvoiceTitleCustom" runat="server" Visible="false" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="lblMailingAddress" meta:resourcekey="InvoiceDetailMailingAddress"></asp:Label></td>
                <td> 
                    <asp:CheckBox ID="chkDefaultAddr" runat="server" AutoPostBack="true" meta:resourcekey="InvoiceDetailCheckBoxLastAddress" OnCheckedChanged="chkDefaultAddr_CheckedChanged"  />
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <div id="divEdit" runat="server">
                        <asp:Label runat="server" id="provinceEditLabel" meta:resourcekey="InvoiceDetailProvince"></asp:Label>:<asp:DropDownList ID="ddlProvince" AutoPostBack="True" Width="100px" OnSelectedIndexChanged="ddlProvince_SelectedIndexChanged"  runat="server" /> *  
                        <asp:Label runat="server" ID="cityEditLabel" meta:resourcekey="InvoiceDetailCity"></asp:Label>:<asp:DropDownList ID="ddlCity" AutoPostBack="True"  OnSelectedIndexChanged="ddlCity_SelectedIndexChanged" runat="server" Width="100px"  /> *  
                        <asp:Label runat="server" ID="districtEditLabel" meta:resourcekey="InvoiceDetailDistrict"></asp:Label>:<asp:DropDownList ID="ddlDistrict" Width="100px" AutoPostBack="True" OnSelectedIndexChanged="ddlDistrict_SelectedIndexChanged" runat="server"  /> *  
                        <br />
                        <asp:Label runat="server" ID="StreetEditLabel" meta:resourcekey="InvoiceDetailStreet"></asp:Label>: <asp:TextBox ID="txtStreet" runat="server" Width="300px" /> *
                        <br />
                        <asp:Label runat="server" ID="PostalCodeEditLabel" meta:resourcekey="InvoiceDetailPostCode"></asp:Label>:<asp:TextBox ID="txtPostal" runat="server" MaxLength="6" onkeypress="return isNumberKey(event)" /> *
                    </div>
                    <div id="divView" runat="server" visible="false">
                        <asp:Label runat="server" id="provinceViewLabel" meta:resourcekey="InvoiceDetailProvince"></asp:Label>:<asp:Label ID="lblProvice" Text="" runat="server" /> 
                        <asp:Label runat="server" ID="cityViewLabel" meta:resourcekey="InvoiceDetailCity"></asp:Label>:<asp:Label ID="lblCity" Text="" runat="server" /> 
                        <asp:Label runat="server" ID="districtViewLabel" meta:resourcekey="InvoiceDetailDistrict"></asp:Label>:<asp:Label ID="lblDistrict" Text="" runat="server" /> 
                        <br />
                        <asp:Label runat="server" ID="streetViewLabel" meta:resourcekey="InvoiceDetailStreet"></asp:Label>:<asp:label ID="lblStreet" Text="" runat="server" />
                        <br />
                        <asp:Label runat="server" ID="postalCodeViewLabel" meta:resourcekey="InvoiceDetailPostCode"></asp:Label>:<asp:label ID="lblPostal" Text="" runat="server" />
                    </div>
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="RecipientLabel" meta:resourcekey="InvoiceDetailReceiver"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtReceiver" runat="server" /> *
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="phoneLabel" meta:resourcekey="InvoiceDetailPhone"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtPhone" runat="server" onkeypress="return isNumberKey(event)" MaxLength="11" /> * 
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="EmailLabel" meta:resourcekey="InvoiceDetailEmail"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtEmail" runat="server" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="InvoiceContentLabel" meta:resourcekey="InvoiceDetailInvoiceContent"></asp:Label></td>
                <td>
                    <asp:DropDownList ID="ddlInvoiceContent" runat="server" /> 
                </td>
            </tr>
        </table>
        <br />
        <br />
        </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnConfirm" EventName="Click" /> 
            <asp:AsyncPostBackTrigger ControlID="btnCancel" EventName="Click" /> 
        </Triggers>
        </asp:UpdatePanel>
        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
             <h3>页面加载中 ...</h3>
        </ProgressTemplate>
        </asp:UpdateProgress>
        <br />
        <div align="center">
            <asp:Button ID="btnConfirm" meta:resourcekey="InvoiceDetailButtonConfirm" runat="server" OnClick="btnConfirm_Click" />
            <asp:Button ID="btnCancel"  meta:resourcekey="InvoiceDetailButtonCancel" runat="server" OnClick="btnCancel_Click" />
            <asp:Button ID="bttnDummy" meta:resourcekey="InvoiceDetailButtonConfirm" runat="server"  OnClientClick="ShowConfirmationMessage(); return false;" />
        </div>
        <br />
    </div>
    <div id="divDetailSplit" style="border:solid; width:900px;padding: 10px;" runat="server">
        <br />
        <asp:UpdatePanel ID="upLoadsSplit" runat="server" UpdateMode="Always">
        <ContentTemplate>
        <asp:Panel ID="panelSplit" runat="server">
        <asp:BulletedList ID="blErrors2" runat="server" BulletStyle="Disc" ForeColor="Red"></asp:BulletedList>
        <table style="border:none;" width="100%"  >
            <tr">
                <td style="width: 10%"><asp:Label runat="server" ID="SplitAmountLabel" meta:resourcekey="InvoiceDetailBillingAmount"></asp:Label></td>
                <td>
                    <label id="lblSplitAmount" runat="server" ></label>
                    <asp:Label ID="SplitIntoLabel" runat="server" meta:resourcekey="InvoiceDetailSplitInto"></asp:Label>
                    <asp:DropDownList ID="ddlSplitInvoice" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlSplitInvoice_SelectedIndexChanged" Width="100px" />
                    <asp:TextBox ID="txtSplitInvoice" runat="server" Visible="false" Width="10px" />
                    <asp:Label ID="SplitInvoicesLabel" runat="server" meta:resourcekey="InvoiceDetailInvoices"></asp:Label>
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="SplitAmountLabel1" meta:resourcekey="InvoiceDetailAmount1"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtInvoiceAmount1" runat="server" onkeydown="return amountValidator(event,this)"    />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="SplitAmountLabel2" meta:resourcekey="InvoiceDetailAmount2"  ></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtInvoiceAmount2" onkeydown="return amountValidator(event,this)" runat="server" />
                </td>
            </tr>
            <tr>
                <td><asp:Label ID="lblamount3" meta:resourcekey="InvoiceDetailAmount3" runat="server" /></td>
                <td>
                    <asp:TextBox ID="txtInvoiceAmount3" runat="server" onkeydown="return amountValidator(event,this)" />
                </td>
            </tr>
        </table>
        
        <table style="border:none; width: 100%">
            <tr>
                <td style="width: 10%"><asp:Label runat="server" ID="SplitInvoiceType" meta:resourcekey="InvoiceDetailInvoiceType"></asp:Label></td>
                <td>
                    <asp:RadioButtonList ID="rbSplitInvoiceType" RepeatLayout="Flow" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rbSplitInvoiceType_SelectedIndexChanged" AutoPostBack="True">
                        <asp:ListItem meta:resourcekey="InvoiceDetailRadioButtonPersonal" Value="PE" ></asp:ListItem>
                        <asp:ListItem meta:resourcekey="InvoiceDetailRadioButtonCorporate" Value="CO"></asp:ListItem>
                    </asp:RadioButtonList> 
                </td>
                
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="SplitInvoiceTitleLabel"  meta:resourcekey="InvoiceDetailInvoiceTitle"></asp:Label></td>
                <td>
                    <asp:RadioButtonList ID="rbSplitInvoiceTitle" RepeatLayout="Flow" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rbSplitInvoiceTitle_SelectedIndexChanged" AutoPostBack="True">
                        <asp:ListItem  meta:resourcekey="InvoiceDetailRadioButtonPersonal" Value="PE"></asp:ListItem>
                        <asp:ListItem  meta:resourcekey="InvoiceDetailRadioButtonCorporate" Value="CU"></asp:ListItem>
                    </asp:RadioButtonList> 
                    <asp:TextBox ID="txtSplitInvoiceCustom" runat="server" Visible="False" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" id="SplitAddress" meta:resourcekey="InvoiceDetailMailingAddress"></asp:Label></td>
                <td>
                    <asp:CheckBox ID="chkSplitAddressConfirm" AutoPostBack="true" runat="server" meta:resourcekey="InvoiceDetailCheckBoxLastAddress" OnCheckedChanged="chkSplitAddressConfirm_CheckedChanged"  />
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    <div id="divSplitEdit" runat="server">
                        <asp:Label runat="server" ID="SplitProvinceLabel" meta:resourcekey="InvoiceDetailProvince"></asp:Label>:<asp:DropDownList ID="ddlSplitProvince" Width="100px" AutoPostBack="true" runat="server" OnSelectedIndexChanged="ddlSplitProvince_SelectedIndexChanged" /> *
                        <asp:Label runat="server" ID="splitCityLabel" meta:resourcekey="InvoiceDetailCity"></asp:Label>:<asp:DropDownList Width="100px" ID="ddlSplitCity" AutoPostBack="true" runat="server" OnSelectedIndexChanged="ddlSplitCity_SelectedIndexChanged"  /> *
                        <asp:Label runat="server" ID="SplitDistrictLabel" meta:resourcekey="InvoiceDetailDistrict"></asp:Label>:<asp:DropDownList Width="100px" ID="ddlSplitDistrict" AutoPostBack="true" runat="server" OnSelectedIndexChanged="ddlSplitDistrict_SelectedIndexChanged"  /> *
                        <br />
                        <asp:Label runat="server" ID="SplitStreetLabel" meta:resourcekey="InvoiceDetailStreet"></asp:Label>:<asp:TextBox ID="txtSplitStreet" runat="server" /> *
                        <br />
                        <asp:Label runat="server" ID="SplitPostalCodeLabel" meta:resourcekey="InvoiceDetailPostCode"></asp:Label>:<asp:TextBox ID="txtSplitPostal" runat="server" onkeypress="return isNumberKey(event)" MaxLength="6"/> *
                    </div>
                    <div id="divSplitView" runat="server" visible="false">
                        <asp:Label runat="server" ID="SplitProvinceViewLabel" meta:resourcekey="InvoiceDetailProvince"></asp:Label>:<asp:Label ID="lblSplitProvince" Text="" runat="server" /> 
                        <asp:Label runat="server" ID="SplitViewCityLabel" meta:resourcekey="InvoiceDetailCity"></asp:Label>:<asp:Label ID="lblSplitCity" Text="" runat="server" /> 
                        <asp:Label runat="server" ID="SplitViewDistrictLabel" meta:resourcekey="InvoiceDetailDistrict"></asp:Label>:<asp:Label ID="lblSplitDistrict" Text="" runat="server" /> 
                        <br />
                        <asp:Label runat="server" ID="SplitViewStreetLabel" meta:resourcekey="InvoiceDetailStreet"></asp:Label>:<asp:label ID="lblSplitStreet" Text="" runat="server" />
                        <asp:Label runat="server" ID="SplitViewPostalCodeLabel" meta:resourcekey="InvoiceDetailPostCode"></asp:Label>:<asp:label ID="lblSplitPostal" Text="" runat="server" />
                    </div>
                </td>
            </tr>
            <tr>
                <td></td>
                <td>
                    
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" id="SplitRecipientLabel" meta:resourcekey="InvoiceDetailReceiver"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtSplitReceiver" runat="server" />  *
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID ="SplitPhoneLabel" meta:resourcekey="InvoiceDetailPhone"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtSplitPhone" runat="server" onkeypress="return isNumberKey(event)" MaxLength="11" /> *
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" id="SplitEmailLabel" meta:resourcekey="InvoiceDetailEmail"></asp:Label></td>
                <td>
                    <asp:TextBox ID="txtSplitEmail" runat="server" />
                </td>
            </tr>
            <tr>
                <td><asp:Label runat="server" ID="SplitInvoiceContentLabel" meta:resourcekey="InvoiceDetailInvoiceContent"></asp:Label></td>
                <td>
                    <asp:DropDownList ID="ddlSplitInvoiceContext" runat="server" />
                </td>
            </tr>
        </table>
        </asp:Panel>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSplitConfirm" EventName="Click" /> 
            <asp:AsyncPostBackTrigger ControlID="btnSplitCancel" EventName="Click" /> 
        </Triggers>
        </asp:UpdatePanel>
        <asp:UpdateProgress ID="UpdateProgress2" runat="server">
        <ProgressTemplate>
            <h3>页面加载中 ...</h3>
        </ProgressTemplate>
        </asp:UpdateProgress>
        <br />
        <br />
        <div align="center">

        <asp:Button ID="btnSplitConfirm" meta:resourcekey="InvoiceDetailButtonConfirm" runat="server" OnClick="btnSplitConfirm_Click" />
        <asp:Button ID="btnSplitCancel" meta:resourcekey="InvoiceDetailButtonCancel" runat="server" OnClick="btnCancel_Click" />
        <asp:Button ID="btnsplitDummy" meta:resourcekey="InvoiceDetailButtonConfirm" runat="server"  OnClientClick="ShowSplitConfirmationMessage(); return false;" />
        </div>
        <br />
    </div>
</asp:Content>
