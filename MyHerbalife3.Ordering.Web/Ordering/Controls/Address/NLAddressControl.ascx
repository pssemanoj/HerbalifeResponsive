<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NLAddressControl.ascx.cs" Inherits="MyHerbalife3.Ordering.Web.Ordering.Controls.Address.NLAddressControl" %>
<div id="gdo-popup-container">

    <ul class="gdo-form-label-left gdo-popup-form-label-padding2 three-fields">
        <li class="full last">
            <asp:Label runat="server" ID="lbNombre" Text="Bestemd voor *" meta:resourcekey="LbNombre"></asp:Label>
            <asp:TextBox runat="server" id="tbNombre" MaxLength="30"></asp:TextBox>
            <div class="error-message">
                <asp:CustomValidator id="cvNombreRequired" runat="server"
                                        OnServerValidate="ValidateRequiredTextBox"
                                        ControlToValidate="tbNombre"
                                        ErrorMessage="Please fill out care of name."
                                        ValidateEmptyText="true"
                                        CssClass="error hide"
                                        OnPreRender="Validator_PreRender"/>
            </div>
        </li>
        <li class="group last">
            <ul>
                <li class="two-blocks">
                    <asp:Label runat="server" ID="lbAddress" MaxLength="40" Text="Straatnaam *" meta:resourcekey="LbStreet1"></asp:Label>
                    <asp:TextBox runat="server" ID="tbAdddress" MaxLength="31"></asp:TextBox>
                    <div class="error-message">
                        <asp:CustomValidator id="cvAddressRequired" runat="server"
                                                OnServerValidate="ValidateRequiredTextBox"
                                                ControlToValidate="tbAdddress"
                                                ErrorMessage="Please fill out the street address field."
                                                ValidateEmptyText="true"
                                                CssClass="error hide"
                                                OnPreRender="Validator_PreRender"/>
                        <asp:CustomValidator id="cvAddressRegex" runat="server"
                                                 OnServerValidate="ValidateAddress"
                                                 ControlToValidate="tbAdddress"
                                                 ErrorMessage="Please enter a valid Address."
                                                 CssClass="error hide"
                                                 OnPreRender="Validator_PreRender"/>  
                    </div>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbHouseNumber" MaxLength="40" Text="Huisnummer *" meta:resourcekey="LbHouseNumber"></asp:Label>
                    <asp:TextBox runat="server" ID="tbHouseNumber" MaxLength="6"></asp:TextBox>
                    <div class="error-message">
                        <asp:CustomValidator id="cvHouseNumberRequired" runat="server"
                                                OnServerValidate="ValidateRequiredTextBox"
                                                ControlToValidate="tbHouseNumber"
                                                ErrorMessage="Please fill out the House number field."
                                                ValidateEmptyText="true"
                                                CssClass="error hide"
                                                OnPreRender="Validator_PreRender"/>
                        <asp:CustomValidator id="cvHouseNumberRegex" runat="server"
                                                OnServerValidate="ValidateHouseNumber"
                                                ControlToValidate="tbHouseNumber"
                                                ErrorMessage="Please fill house number in proper format."
                                                CssClass="error hide"
                                                OnPreRender="Validator_PreRender"/>
                    </div>
                </li>
            </ul>
        </li>
        <li class="group last">
            <ul>
                <li>
                    <asp:Label runat="server" ID="lbCity" Text="Plaatsnaam *" meta:resourcekey="LbCity"></asp:Label>
                    <asp:TextBox runat="server" ID="tbCity" MaxLength="30" ></asp:TextBox>
                    <div class="error-message">
                        <asp:CustomValidator id="cvCity" runat="server"
                                        OnServerValidate="ValidateRequiredTextBox"
                                        ControlToValidate="tbCity"
                                        ErrorMessage="Please fill out the city."
                                        ValidateEmptyText="true"
                                        CssClass="error hide"
                                        OnPreRender="Validator_PreRender"/>
                    </div>
                </li>
                <li>
                    <asp:Label runat="server" ID="lbPostalCode" Text="Postcode *" meta:resourcekey="LbPostalCode"></asp:Label>
                    <asp:TextBox runat="server" ID="tbPostalCode" MaxLength="7"></asp:TextBox>
                    <span class="gdo-form-format">
                        <asp:Localize ID="PostCodeTooltip" runat="server" Text="Voorbeeld: 1234 NL (4 cijfers, spatie, 2 hoofdletters)" meta:resourcekey="HouseNumberFormat" />
                    </span>
                    <div class="error-message">
                        <asp:CustomValidator id="cvPostalCode" runat="server"
                                        OnServerValidate="ValidateRequiredTextBox"
                                        ControlToValidate="tbPostalCode"
                                        ErrorMessage="Please fill out the zip code."
                                        ValidateEmptyText="true"
                                        CssClass="error hide"
                                        OnPreRender="Validator_PreRender"/>
                        <asp:CustomValidator id="cvPostalCodeRegex" runat="server"
                                        OnServerValidate="ValidatePostalCode"
                                        ControlToValidate="tbPostalCode"
                                        ErrorMessage="Please fill zip code in proper format."
                                        CssClass="error hide"
                                        OnPreRender="Validator_PreRender"/>
                    </div>
                </li>
                <li class="last">
                    <asp:Label runat="server" ID="lbCountry" Text="Land *" meta:resourcekey="LbCountry"></asp:Label>
                    <asp:TextBox runat="server" ID="tbCountry" MaxLength="40" ReadOnly="True"></asp:TextBox>
                </li>
            </ul>
        </li>
        
        <li class="last">
            <asp:Label runat="server" ID="lbPhoneNumber" Text="Telefoonnummer *" meta:resourcekey="LbPhoneNumber"></asp:Label>
            <asp:TextBox runat="server" ID="tbPhoneNumber" MaxLength="10"></asp:TextBox>
            <div class="error-message">
                <asp:CustomValidator id="cvPhoneNumberRequired" runat="server"
                                OnServerValidate="ValidateRequiredTextBox"
                                ValidateEmptyText="true"
                                ControlToValidate="tbPhoneNumber"
                                ErrorMessage="Please fill out the phone number."
                                CssClass="error hide"
                                OnPreRender="Validator_PreRender"/>
                <asp:CustomValidator id="cvPhoneNumberRegex" runat="server"
                                OnServerValidate="ValidatePhoneNumber"
                                ControlToValidate="tbPhoneNumber"
                                ErrorMessage="Please fill phone number in proper format."
                                CssClass="error hide"
                                OnPreRender="Validator_PreRender"/>
            </div>
        </li>
        <li class="help-text">
            <span class="gdo-form-format">
                <asp:Localize ID="PhoneNumberTooltip" runat="server" Text="Voorbeeld: 9-10 cijfers" meta:resourcekey="LbPhoneNumberFormat" />
            </span>
        </li>
    </ul>
</div>
