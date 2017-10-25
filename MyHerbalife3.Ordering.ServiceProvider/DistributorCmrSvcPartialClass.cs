using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc
{
    public interface IDenum<T> where T : System.IEquatable<T>
    {
        T Key { get; }
    }

    public partial class ContactType : IDenum<string>
    {

    }

    public partial class ContactSourceType : IDenum<string>
    {

    }

    public partial class TaskStatusType
    {
        public static readonly TaskStatusType NOTSTA = new TaskStatusType("NOTSTA", "NotStarted", Int16.Parse("1"), "NotStarted");
        public static readonly TaskStatusType INPROG = new TaskStatusType("INPROG", "InProgress", Int16.Parse("2"), "InProgress");
        public static readonly TaskStatusType COMPLE = new TaskStatusType("COMPLE", "Completed", Int16.Parse("3"), "Completed");
        public static readonly TaskStatusType Empty = new TaskStatusType();

        #region ctor
        private TaskStatusType(System.String key, System.String displayName, System.Int16 displaySequence, System.String description)
        {
            Key = key;
            DisplayName = displayName;
            DisplaySequence = displaySequence;
            Description = description;
        }

        private TaskStatusType() { }
        #endregion
    }

    public partial class AppointmentCategoryType3 : IDenum<string>
    {
        public static readonly AppointmentCategoryType3 APPT = new AppointmentCategoryType3("APPT", "Appointments", Boolean.Parse("True"), "0", "Appointments");
        public static readonly AppointmentCategoryType3 EVENT = new AppointmentCategoryType3("EVENT", "Events", Boolean.Parse("True"), "0", "Events");
        public static readonly AppointmentCategoryType3 FOLLUP = new AppointmentCategoryType3("FOLLUP", "FollowUps ", Boolean.Parse("True"), "0", "FollowUps ");
        public static readonly AppointmentCategoryType3 MEETNG = new AppointmentCategoryType3("MEETNG", "Meetings", Boolean.Parse("True"), "0", "Meetings");
        public static readonly AppointmentCategoryType3 PERSNL = new AppointmentCategoryType3("PERSNL", "Personal", Boolean.Parse("True"), "0", "Personal");
        public static readonly AppointmentCategoryType3 PHCALL = new AppointmentCategoryType3("PHCALL", "PhoneCalls", Boolean.Parse("True"), "0", "PhoneCalls");
        public static readonly AppointmentCategoryType3 OTHER = new AppointmentCategoryType3("OTHER", "Other", Boolean.Parse("True"), "0", "Other");
        public static readonly AppointmentCategoryType3 Empty = new AppointmentCategoryType3();

        #region ctor

        private AppointmentCategoryType3(System.String key, System.String calendarName, System.Boolean isDefault, System.String color, System.String description)
        {
            Key = key;
            CalendarName = calendarName;
            IsDefault = isDefault;
            Color = color;
            Description = description;
        }

        private AppointmentCategoryType3() { }
        #endregion

        public static IEnumerable<AppointmentCategoryType3> Values
        {
            get
            {
                yield return APPT;
                yield return EVENT;
                yield return FOLLUP;
                yield return MEETNG;
                yield return PERSNL;
                yield return PHCALL;
                yield return OTHER;
            }
        }

        #region Type Conversion

        public static bool TryParse(string value, ref AppointmentCategoryType3 result)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            switch (value.ToUpper())
            {
                case "APPT": result = APPT; break;
                case "EVENT": result = EVENT; break;
                case "FOLLUP": result = FOLLUP; break;
                case "MEETNG": result = MEETNG; break;
                case "PERSNL": result = PERSNL; break;
                case "PHCALL": result = PHCALL; break;
                case "OTHER": result = OTHER; break;
                default: return false;
            }
            return true;
        }

        public static AppointmentCategoryType3 Parse(string value)
        {
            AppointmentCategoryType3 result = null;
            if (!TryParse(value, ref result))
            {
                throw new ArgumentException("value");
            }
            return result;
        }

        #endregion
    }

    public partial class TaskPriorityType
    {
        public static readonly TaskPriorityType L = new TaskPriorityType("L", "Low", Int16.Parse("1"), "Low");
        public static readonly TaskPriorityType M = new TaskPriorityType("M", "Medium", Int16.Parse("2"), "Medium");
        public static readonly TaskPriorityType H = new TaskPriorityType("H", "High", Int16.Parse("3"), "High");
        public static readonly TaskPriorityType Empty = new TaskPriorityType();

        #region ctor

        private TaskPriorityType(System.String key, System.String displayName, System.Int16 displaySequence, System.String description)
        {
            Key = key;
            DisplayName = displayName;
            DisplaySequence = displaySequence;
            Description = description;
        }

        private TaskPriorityType() { }
        #endregion

    }

    public partial class AppointmentRecurrenceType
    {
        public static readonly AppointmentRecurrenceType NOTREC = new AppointmentRecurrenceType("NOTREC", "NotRecurring", Int16.Parse("1"), "NotRecurring");
        public static readonly AppointmentRecurrenceType MASTER = new AppointmentRecurrenceType("MASTER", "Master", Int16.Parse("2"), "Master");
        public static readonly AppointmentRecurrenceType OCCURR = new AppointmentRecurrenceType("OCCURR", "Occurrence", Int16.Parse("3"), "Occurrence");
        public static readonly AppointmentRecurrenceType EXCEPT = new AppointmentRecurrenceType("EXCEPT", "Exception", Int16.Parse("4"), "Exception");
        public static readonly AppointmentRecurrenceType Empty = new AppointmentRecurrenceType();

        #region ctor

        private AppointmentRecurrenceType(System.String key, System.String displayName, System.Int16 displaySequence, System.String description)
        {
            Key = key;
            DisplayName = displayName;
            DisplaySequence = displaySequence;
            Description = description;
        }

        private AppointmentRecurrenceType() { }
        #endregion
    }
}
