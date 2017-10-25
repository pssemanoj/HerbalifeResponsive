<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactFollowUpForm.ascx.cs"
    Inherits="MyHerbalife3.Bizworks.Web.Controls.ContactFollowUpForm" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<%@ Register TagPrefix="cc1" Namespace="MyHerbalife3.Shared.UI.Controls" Assembly="MyHerbalife3.Shared.UI" %>
<telerik:RadScriptBlock ID="RadScriptBlockFollowups" runat="server">

    <script type="text/javascript">

        function ResetFollowupForm() {
            $("#<%=SubjectTextBox.ClientID%>").val("");
            $("#<%=LocationBox.ClientID%>").val("");
            $("#<%=StartDate.ClientID%>_dateInput_text").val("");
            $("#<%=StartTime.ClientID%>_dateInput_text").val("");
            $("#<%=EndDate.ClientID%>_dateInput_text").val("");
            $("#<%=EndTime.ClientID%>_dateInput_text").val("");
            $("#<%=AllDayEvent.ClientID%>").removeAttr("checked");
            $("#<%=NotesTextBox.ClientID%>").val("");
        }

        function OnOK() {
            //return HideFollowup();
        }

        function OnCancel() {
        
        		//document.getElementById('aspnetForm').<%=SubjectTextBox.ClientID%>.value="";
           
            return HideFollowup();
        }
        
        function onAllDayEventClick(obj)
        {
            if($(obj).attr("checked"))
            {
                $("#startTimePicker").css("visibility", "hidden");
                $("#endTimePicker").css("visibility", "hidden");
            }
            else
            {
            $("#startTimePicker").css("visibility", "visible");
                $("#endTimePicker").css("visibility", "visible");
            }
        }

        $(document).ready(function(){

	        $('#<%=NotesTextBox.ClientID%>').keyup(function(){
		        var max = 200;
		        if($(this).val().length > max){		        
			        $(this).val($(this).val().substr(0, 200));
		        }		        
	        });
        });


    </script>

</telerik:RadScriptBlock>
<asp:HiddenField ID="data" runat="server" />
<table class="popup-layout-table">
    <tbody>
        <tr>
            <td class="popup-content">
                <div class="popwrapper" id="contact-followup-contentPanel">
                    <table id="followup">
                        <tr>
                            <td class="leftcolumn popup-title" colspan="2">
                                <asp:Label runat="server" ID="Header" Text="Follow Up" meta:resourcekey="FollowUpHeader"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn" colspan="2">
                                <asp:Label runat="server" CssClass="requird-fields-notes" ID="requiredFieldLabel"
                                    Text="*Required Fields" meta:resourcekey="RequiredFieldLabel"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label ID="SubjectPrompt" runat="server" Text="*Subject" meta:resourcekey="SubjectPrompt"></asp:Label>
                            </td>
                            <td class="rightcolumn">
                                <asp:TextBox runat="server" ID="SubjectTextBox" EnableViewState="False" meta:resourcekey="SubjectTextBox"></asp:TextBox>
                                <asp:RequiredFieldValidator runat="server" ID="SubjectRequiredValidator" ControlToValidate="SubjectTextBox"
                                    ValidationGroup="ContactFollowUP" ErrorMessage="Subject is required" meta:resourcekey="SubjectRequiredValidator">*</asp:RequiredFieldValidator>
                                <asp:RegularExpressionValidator runat="server" ID="SubjectLengthValidator" ControlToValidate="SubjectTextBox"
                                    ValidationGroup="ContactFollowUP" ErrorMessage="Subject is too long" ValidationExpression="^.{1,50}$"
                                    meta:resourcekey="SubjectLengthValidator">*</asp:RegularExpressionValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label ID="LocationPropmpt" runat="server" Text="Location" meta:resourcekey="LocationPrompt"></asp:Label>
                            </td>
                            <td class="rightcolumn">
                                <asp:TextBox runat="server" ID="LocationBox" MaxLength="50" EnableViewState="False"
                                    meta:resourcekey="LocationBox"></asp:TextBox>
                                <asp:RegularExpressionValidator runat="server" ID="LocationLengthValidator" ControlToValidate="LocationBox"
                                    ValidationGroup="ContactFollowUP" ErrorMessage="Location is too long" ValidationExpression="^.{1,50}$"
                                    meta:resourcekey="LocationLengthValidator">*</asp:RegularExpressionValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label ID="TypePrompt" runat="server" Text="Type:" meta:resourcekey="TypePrompt"></asp:Label>
                            </td>
                            <td>
                                <telerik:RadComboBox ZIndex="2000000000" ID="CalendarTypesRadComboBox"  runat="server"
                                    CssClass="calendar-types-combo" HighlightTemplatedItems="True" 
                                    meta:resourcekey="CalendarTypesRadComboBox">
                                </telerik:RadComboBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label runat="server" ID="StartTimePrompt" Text="Start Time:" meta:resourcekey="StartTimePrompt"></asp:Label>
                            </td>
                            <td>
                                <div id="startDatePicker" style="float: left;">
                                    <telerik:RadDatePicker runat="server" ID="StartDate" CssClass="rsAdvDatePicker" 
                                        Width="100px" MinDate="1900-01-01" ZIndex="10000" SharedCalendarID="SharedCalendar"  Calendar-ShowRowHeaders="false" 
                                        meta:resourcekey="StartDate" Calendar-FastNavigationStep="12">
                                    </telerik:RadDatePicker>
                                </div>
                                <div id="startTimePicker" style="float: left;">
                                    <telerik:RadTimePicker runat="server" ID="StartTime" CssClass="rsAdvTimePicker" Width="70px"
                                         ZIndex="10000" meta:resourcekey="StartTime">
                                        <TimeView ID="TimeView1" runat="server" Columns="3" ShowHeader="true" RenderDirection="Vertical"
                                            Interval="00:30" TimeFormat="h:mm tt" TimeOverStyle-Wrap="false"
                                            TimeStyle-Wrap="false" HeaderStyle-CssClass="timepickerheader" AlternatingTimeStyle-Wrap="false"
                                            AlternatingTimeStyle-CssClass="altrow" HeaderText="Time Picker" />
                                    </telerik:RadTimePicker>
                                </div>
                                <div id="AllDayCheckBox" style="margin-left: 20px; float: left;">
                                    <asp:CheckBox runat="server" ID="AllDayEvent" CssClass="rsAdvChkWrap" meta:resourcekey="AllDayEvent" />
                                    <asp:Label runat="server" ID="Label1" Text="All Day" meta:resourcekey="AllDayPrompt"></asp:Label>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label runat="server" ID="EndTimePrompt" Text="End Time:" meta:resourcekey="EndTimePrompt"></asp:Label>
                            </td>
                            <td>
                                <div id="endDatePicker" style="float: left;">
                                    <telerik:RadDatePicker runat="server" ID="EndDate" CssClass="rsAdvDatePicker" Width="100px"
                                         MinDate="1900-01-01" ZIndex="10000" SharedCalendarID="SharedCalendar"
                                        meta:resourcekey="EndDate" Calendar-FastNavigationStep="12"  Calendar-ShowRowHeaders="false" >
                                    </telerik:RadDatePicker>
                                </div>
                                <div id="endTimePicker" style="float: left;">
                                    <telerik:RadTimePicker runat="server" ID="EndTime" CssClass="rsAdvTimePicker" Width="70px"
                                         ZIndex="10000" meta:resourcekey="EndTime">
                                        <TimeView ID="TimeView2" runat="server" Columns="3" ShowHeader="true" RenderDirection="Vertical"
                                            Interval="00:30" TimeFormat="h:mm tt" TimeOverStyle-Wrap="false"
                                            TimeStyle-Wrap="false" HeaderStyle-CssClass="timepickerheader" AlternatingTimeStyle-Wrap="false"
                                            AlternatingTimeStyle-CssClass="altrow" HeaderText="Time Picker" />
                                    </telerik:RadTimePicker>
                                </div>
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                            </td>
                            <td>
                                <asp:Label ID="lbNote" runat="server" Text="NOTE: Specifying a time will make an item appear in your Clendar, Otherwise it will appear as a task in your task list."
                                    meta:resourcekey="Note"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td class="leftcolumn">
                                <asp:Label ID="DescriptionPrompt" runat="server" Text="Notes" meta:resourcekey="DescriptionPrompt"></asp:Label>
                            </td>
                            <td class="rightcolumn">
                                <asp:TextBox runat="server" ID="NotesTextBox" TextMode="MultiLine" meta:resourcekey="NotesTextBox"></asp:TextBox>
                                <asp:RegularExpressionValidator runat="server" ID="NoteLengthValidator" ControlToValidate="NotesTextBox"
                                    ValidationGroup="ContactFollowUP" ErrorMessage="Notes too long" ValidationExpression="(.|\r|\n){0,200}"
                                    meta:resourcekey="NoteLengthValidator">*</asp:RegularExpressionValidator>
                            </td>
                        </tr>
                        <tr>
                            <td id="error-summary" colspan="2">
                                <asp:ValidationSummary runat="server" ID="ValidationSummary1" CssClass="error-message"
                                    ValidationGroup="ContactFollowUP" DisplayMode="List" meta:resourcekey="ValidationSummary1" />
                            </td>
                        </tr>
                    </table>
                </div>
            </td>
        </tr>
        <tr>
            <td>
                <telerik:RadCalendar runat="server" ID="SharedCalendar" 
                    FastNavigationStep="12" ShowRowHeaders="False" RangeMinDate="1900-01-01" ZIndex="2000000000"
                    meta:resourcekey="SharedCalendar" SelectedDate="" ViewSelectorText="x">
                </telerik:RadCalendar>
                <telerik:RadTimeView ID="SharedTimeView" runat="server" 
                    ZIndex="2000000000" Columns="3" ShowHeader="False" Interval="00:30:00" TimeFormat="h:mm tt"
                    CellSpacing="-1" TimeOverStyle-Wrap="false" AlternatingTimeStyle-CssClass="altrow" HeaderText="Time Picker" meta:resourcekey="SharedTimeView">
                </telerik:RadTimeView>
            </td>
        </tr>
        <tr>
            <td>
                <div style="width: 100%;" class="popup-buttons">
                    <div style="float: right;" id="SilverButton">
                        <cc1:OvalButton ID="CancelButton" runat="server" Coloring="silver" CommandName="Cancel"
                            OnClick="CancelButton_Click" ArrowDirection="" IconPosition="" IconType="" meta:resourcekey="CancelButton">Cancel</cc1:OvalButton>
                    </div>
                    <div style="width: 20px; float: right;">
                        &nbsp;</div>
                    <div style="float: right; margin-left: 20px" id="SilverButton">
                        <cc1:OvalButton ID="SaveButton" runat="server" Coloring="silver" 
                             ValidationGroup="ContactFollowUP" ArrowDirection=""
                            IconPosition="" IconType="" meta:resourcekey="SaveButton">Save</cc1:OvalButton>
                    </div>
                </div>
            </td>
        </tr>
    </tbody>
</table>
