<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PHAddressControl.ascx.cs"
    Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.PHAddressControl" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %><%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<style>
    @media (max-width:768px) {
        .cc-item-separator {
            display:none !important;
        }
    }
    @media (min-width:768px) {        
        .country-code-items{
	        background: #e8e8e8;
            max-width:210px;
	    }
	    .cc-item-one{
		    width:40px !important;
		    margin-right:0px !important;
	    }
	    .cc-item-one > input[type="text"]{
		    border: 0;
		    background: transparent;
            text-align:center;
            font-size:22px;
	    }
	    .cc-item-separator{
		    width:10px !important;
		    margin:0 5px !important;
		    vertical-align:middle !important;
		    height:0px;

	    }
	    .cc-item-separator span{
		    font-size:20px !important;
	    }
	    .cc-item-two{
		    width:40px !important;
		    margin-right:0px !important;
	    }
        .cc-item-three{
            width:75px !important;
        }
        .cc-item-three > input[type="text"]{
            Width:70px;
            border-color:#b8b8b8;
            background:white;
        }
    }
</style>
<div id="gdo-popup-container">

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 three-fields">
        <li class="full last">
            <asp:Label runat="server" ID="lbNombre" Text="Care of Name *" meta:resourcekey="LbNombre"></asp:Label>
            <asp:TextBox runat="server" id="tbNombre" MaxLength="40"></asp:TextBox>
            <div class="error-message">
                <asp:CustomValidator id="cvNombreRequired" runat="server"
                                     OnServerValidate="ValidateRequiredTextBox"
                                     ControlToValidate="tbNombre"
                                     ErrorMessage="Please fill out care of name."
                                     ValidateEmptyText="true"
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>
                <asp:CustomValidator id="cvNombreRegex" runat="server"
                                     OnServerValidate="ValidateCareOfName"
                                     ControlToValidate="tbNombre"
                                     ErrorMessage="Please enter a valid Name."
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>  
            </div>              
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbAddress" MaxLength="40" Text="Street 1 *" meta:resourcekey="LbStreet1"></asp:Label>
            <asp:TextBox runat="server" ID="tbAdddress" MaxLength="40"></asp:TextBox>
            <div class="error-message">
                <asp:CustomValidator id="cvAddressRequired" runat="server"
                                     OnServerValidate="ValidateRequiredTextBox"
                                     ControlToValidate="tbAdddress"
                                     ErrorMessage="Please fill out the street address field."
                                     ValidateEmptyText="true"
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>
            </div>
        </li>
        <li class="full last">
            <asp:Label runat="server" ID="lbAddress2" MaxLength="40" Text="Street 2 *" meta:resourcekey="LbStreet2"></asp:Label>
            <asp:TextBox runat="server" ID="tbAddress2" MaxLength="40"></asp:TextBox>
            <div class="error-message">
                <asp:CustomValidator id="cvAddress2Required" runat="server"
                                     OnServerValidate="ValidateRequiredTextBox"
                                     ControlToValidate="tbAddress2"
                                     ErrorMessage="Please fill out the street 2 address field."
                                     ValidateEmptyText="true"
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>
            </div>
        </li>
        <li class="group last">
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbState" Text="Province *" meta:resourcekey="LbState"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlState" OnSelectedIndexChanged="dnlState_SelectedIndexChanged" OnDataBound="dnlState_DataBound">
                    </asp:DropDownList>
                    <div class="error-message">
                        <asp:CustomValidator id="cvStateRequired" runat="server"
                                     OnServerValidate="ValidateState"
                                     ErrorMessage="Please fill out the state." 
                                     ForeColor="Red"/>
                    </div>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="City/Municipality *" meta:resourcekey="LbCity"></asp:Label>
                    <asp:DropDownList AutoPostBack="True" runat="server" ID="dnlCity" OnSelectedIndexChanged="dnlCity_SelectedIndexChanged" >
                    </asp:DropDownList>
                    <div class="error-message">
                        <asp:CustomValidator id="cvCity" runat="server"
                                     OnServerValidate="ValidateCity"
                                     ErrorMessage="Please fill out the city." 
                                     ForeColor="Red"/>
                    </div>
                </li>
 
                <li class="last">
                    <asp:Label runat="server" ID="lbPostalCode" Text="Zip *" meta:resourcekey="LbPostalCode"></asp:Label>
                    <asp:TextBox runat="server" ID="tbPostalCode" MaxLength="5"></asp:TextBox>
                    <div class="error-message">
                        <asp:CustomValidator id="cvPostalCode" runat="server"
                                     OnServerValidate="ValidateRequiredTextBox"
                                     ControlToValidate="tbPostalCode"
                                     ErrorMessage="Please fill out the zip code."
                                     ValidateEmptyText="true"
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>
                    </div>
                </li>
            </ul>
        </li>
        <li class="group last">
            <asp:Label runat="server" ID="lbCountryCode" Text="Mobile Phone *" meta:resourcekey="LbCountryCode"></asp:Label>
            <ul class="country-code-items">
                <li class="cc-item-one">
                    
                    <asp:TextBox runat="server" id="tbCountryCode" MaxLength="2"></asp:TextBox>
                    
                </li>
                <li class="cc-item-separator">
                    <span>-</span>
                </li>
                <li class="cc-item-two">
                    <%--<asp:Label runat="server" ID="lbAreaCode" Text="Area Code *" meta:resourcekey="LbAreaCode"></asp:Label>--%>
                    <%--<asp:TextBox runat="server" id="tbAreaCode" MaxLength="3"></asp:TextBox>--%>
                    <telerik:RadMaskedTextBox RenderMode="Lightweight" ID="tbAreaCode" runat="server" Mask="9##" Height="33px" Width="40px" style="text-align:center"
                        ValidationGroup="Group1">
                    </telerik:RadMaskedTextBox>
                    <asp:Label runat="server" CssClass="gdo-form-format" ID="lbPostalCodeFormat" Text="Format: 3 numbers no spaces" meta:resourcekey="LbPostalCodeFormat" />
                    
                    
                </li>
                <li class="cc-item-separator">
                    <span>-</span>
                </li>
                <li class="last cc-item-three">
                    <asp:Label runat="server" ID="EmptyLabel1" Text="" />
                    <%--<asp:Label runat="server" ID="lbPhoneNumber" Text="Phone Number *" meta:resourcekey="LbPhoneNumber"></asp:Label>--%>
                    <asp:TextBox runat="server" ID="tbPhoneNumber" MaxLength="8"  ></asp:TextBox>
                    <span class="gdo-form-format">
                        <asp:Localize ID="Localize2" runat="server" Text="Format 7 numbers" meta:resourcekey="LbPhoneNumberFormat"/>
                    </span>
                </li>
            </ul>
            <span class="error-message">
                        <asp:CustomValidator id="cvAreaCodeRequired" runat="server"
                                     OnServerValidate="ValidateRequiredTextBox"
                                     ControlToValidate="tbAreaCode"
                                     ErrorMessage="Please fill out the area code."
                                     ValidateEmptyText="true"
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>
                        <asp:CustomValidator id="cvAreaCodeRegex" runat="server"
                                     OnServerValidate="ValidatePhoneNumber"
                                     ControlToValidate="tbAreaCode"
                                     ErrorMessage="Please fill area code in proper format."
                                     CssClass="error hide"
                                     OnPreRender="Validator_PreRender"/>

                  
            </span>
        </li>
    </ul>
</div>
