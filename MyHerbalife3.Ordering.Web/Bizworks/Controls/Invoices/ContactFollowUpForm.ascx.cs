using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using MyHerbalife3.Ordering.SharedProviders.Bizworks;
using MyHerbalife3.Shared.Infrastructure.ValueObjects;
using MyHerbalife3.Shared.UI;
using MyHerbalife3.Shared.UI.Helpers;
using MyHerbalife3.Shared.ViewModel;
using MyHerbalife3.Shared.ViewModel.Models;
using Telerik.Web.UI;
using System.ComponentModel;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;


namespace MyHerbalife3.Bizworks.Web.Controls
{
    public partial class ContactFollowUpForm : UserControlBase
    {
        public string DateFormat { get; set; }
        public string AdvancedFrom { get; set; }
        public string AdvancedTo { get; set; }
        public HiddenField dataField { get { return this.data; } }
        public TextBox SubjectTextBoxField { get { return this.SubjectTextBox; } }
        public string DistributorID { get; set; }

        private object _dataItem = null;

        [Bindable(true)]
        [DefaultValue(false)]
        public bool NoPostBack { get; set; }

        public int? ContactID
        {
            get
            {
                if (!string.IsNullOrEmpty(data.Value))
                    return int.Parse(data.Value);
                else
                    return null;
            }

            set
            {
                data.Value = value.ToString();
            }
        }

        
        public object DataItem
        {
            get
            {
                return this._dataItem;
            }
            set
            {
                this._dataItem = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var member = (MembershipUser<DistributorProfileModel>) Membership.GetUser();
            DistributorID = member.Value.Id;

            if (DataItem != null)
                ContactID = (int)DataBinder.Eval(DataItem, "ContactID");
            FillDropDowns();

           // if (!NoPostBack)
            //{
                SaveButton.Click += SaveButton_Click;
                CancelButton.Click += CancelButton_Click;
          //  }
            //else
            //{
            //    SaveButton.OnClientClick = "return OnOK();";
            //    CancelButton.OnClientClick = "return OnCancel()";
            //}

            if (!Page.IsPostBack)
            {
                AllDayEvent.Attributes.Add("onclick", "onAllDayEventClick(this);");
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            Save(ContactID);
        }

        /// <summary>
        /// Saves the entires either as appointment or task.
        /// </summary>
        /// <param name="appointmentContactID"></param>
        public void Save(int? appointmentContactID)
        {
            if (string.IsNullOrEmpty(SubjectTextBox.Text))
                return;

            //todo: error handling
            if (IsAppointment() && appointmentContactID.HasValue)
            {
                SaveAppointment(appointmentContactID.Value);
            }
            else
            {
                SaveTask();
            }
        }


        private void SaveTask()
        {
            Task_V01 Task = new Task_V01()
            {
                Subject = SubjectTextBox.Text,
                Notes = NotesTextBox.Text,
                Status = TaskStatusType.NOTSTA,
                Priority = TaskPriorityType.M
            };

            //todo: error handling
            SchedulerDataProvider.SaveTask(DistributorID, Task, true);
        }

        //todo: return true/false to indicate if it failed/succeeded.
        private void SaveAppointment(int appointmentContactID)
        {
            if (string.IsNullOrEmpty(SubjectTextBox.Text))
                return;

            Appointment_V01 appointmnet = new Appointment_V01()
            {
                Attendees = GetAttendees(appointmentContactID),
                Category = GetCategory(),
                StartTime = GetStartTime(),
                EndTime = GetEndTime(),
                Subject = SubjectTextBox.Text,
                Notes = NotesTextBox.Text,
                RecurrenceType = AppointmentRecurrenceType.NOTREC,
                Location = LocationBox.Text,
                RecurringPattern = ""
            };

            //todo: error handling
            SchedulerDataProvider.SaveAppointment(DistributorID, appointmnet, true, false);
        }

        private bool IsAppointment()
        {
            bool dateSpecified = StartDate.SelectedDate.HasValue && EndDate.SelectedDate.HasValue;
            bool timeSpecified = StartTime.SelectedDate.HasValue && EndTime.SelectedDate.HasValue;

            if ((AllDayEvent.Checked && !dateSpecified) ||
                (!AllDayEvent.Checked && !(dateSpecified && timeSpecified)))
            {
                return false;
            }
            else
                return true;
        }
        

        private DateTime GetStartTime()
        {
            DateTime startDate = StartDate.SelectedDate.Value.Date;

            if (AllDayEvent.Checked)
            {
                startDate = startDate.Date;
            }
            else
            {
                TimeSpan startTime = StartTime.SelectedDate.Value.TimeOfDay;
                startDate = startDate.Add(startTime);
            }

            // No localization required, time entered should be time shown
            return startDate;
        }

        private DateTime GetEndTime()
        {
            DateTime endDate = EndDate.SelectedDate.Value.Date;

            if (AllDayEvent.Checked)
            {
                endDate = endDate.Date.AddDays(1);
            }
            else
            {
                TimeSpan endTime = EndTime.SelectedDate.Value.TimeOfDay;
                endDate = endDate.Add(endTime);
            }

            // No localization required, time entered should be time shown
            return endDate;
        }


        private AppointmentCategoryType3 GetCategory()
        {
            if (!String.IsNullOrEmpty(CalendarTypesRadComboBox.SelectedValue))
                return AppointmentCategoryType3.Parse(CalendarTypesRadComboBox.SelectedValue);
            else
                return AppointmentCategoryType3.FOLLUP;
        }

        private List<Contact_V02> GetAttendees(int appointmentContactID)
        {
            List<Contact_V02> attendees = new List<Contact_V02>();
            attendees.Add(new Contact_V02() { ContactID = appointmentContactID });
            return attendees;
        }


        private void FillDropDowns()
        {
            if (Page.IsPostBack)
                return;
            CalendarTypesRadComboBox.Items.Clear();

            foreach (AppointmentCategoryType3 c in AppointmentCategoryType3.Values)
            {
                CalendarTypesRadComboBox.Items.Add(new RadComboBoxItem(LocalizeEnumKey(c, "AppointmentCategoryType_{0}"), c.Key));
            }

            CalendarTypesRadComboBox.SelectedValue = AppointmentCategoryType3.FOLLUP.Key;
        }

        public void Reset()
        {
            FillDropDowns();
            ContactID = null;
            SubjectTextBox.Text = "";
            NotesTextBox.Text = "";
        }

        private DateTime? UtcToLocal(DateTime? d)
        {
            if (d.HasValue)
                return LocalizationHelper.GetClientLocalTimeFromUtc(d.Value);
            else
                return null;
        }

        private DateTime? LocalToUtc(DateTime? d)
        {
            if (d.HasValue)
                return LocalizationHelper.GetUtcFromClientLocalTime(d.Value);
            return null;
        }

        public string LocalizeEnumKey<T>(T denum, string resxKeyFormat) where T : MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc.IDenum<string>
        {
            string resxKey = string.Format(resxKeyFormat, denum.Key);
            var p = new LocalizationManager();
            return p.GetString(AppRelativeVirtualPath, resxKey, Thread.CurrentThread.CurrentUICulture);
        }
    }
}