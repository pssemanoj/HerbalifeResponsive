using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using MyHerbalife3.Ordering.SharedProviders.ViewModel;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Shared.ViewModel.Models;

namespace MyHerbalife3.Ordering.Providers
{
    /// <summary>Encapsulates Order Month logic</summary>
    public class OrderMonth
    {
        #region Enums

        /// <summary>Tracks users OrderMonth selection in a Dual Order Month</summary>
        public enum DualOrderMonthSelection
        {
            NotSelected,
            Previous,
            Current
        }

        #endregion

        #region Embedded classes

        /// <summary>Lightweight persistance for data from OrderMonth. Stash any session data in here</summary>
        [Serializable]
        private class OrderMonthData
        {
            public int CurrentOrderMonth;

            /// <summary>Users OrderMonth selection</summary>
            public DualOrderMonthSelection SelectedOrderMonth = DualOrderMonthSelection.NotSelected;
        }

        #endregion

        #region Events

        [Publishes(MyHLEventTypes.OrderMonthChanged)]
        public event EventHandler OrderMonthChanged;

        #endregion

        #region Consts

        private const string OrderMonthDataSessionKey = "OrderMonthDataSessionKey";

        #endregion

        #region Fields

        private readonly HttpSessionState Session = HttpContext.Current !=null ? HttpContext.Current.Session : null;
        private readonly string _CountryCode = string.Empty;
        private readonly CultureInfo _Culture = Thread.CurrentThread.CurrentCulture;
        private readonly TimeSpan _MonthEndGracePeriod = new TimeSpan(1, 0, 0);
        private readonly List<Control> _OrderMonthControls = new List<Control>();
        private readonly OrderMonthData _OrderMonthData = new OrderMonthData();
        private readonly string _OrderMonthFormat = "yyyyMM";
        private readonly string _OrderMonthLongFormat = "MMM yyyy";
        private readonly List<Label> _OrderMonthNotificationControls = new List<Label>();
        private readonly string _OrderMonthShortFormat = "yyMM";
        private DateTime _CreateDateTime;
        private DateTime _DualOrderMonthDate = DateTime.MinValue;
        private DateTime _MonthEndDate;
        private bool _OrderMonthChanged;

        #endregion

        #region Construction

        public OrderMonth(string countryCode)
        {
            var config = HLConfigManager.Configurations.DOConfiguration;
            if (config.UseGregorianCalendar)
            {
                changeFormatCalendarToGregorian();
            }
            _OrderMonthFormat = config.OrderMonthFormat;
            _OrderMonthLongFormat = config.OrderMonthLongFormat;
            _OrderMonthShortFormat = config.OrderMonthShortFormat;
            _CountryCode = countryCode;
            Initialize();
            ResolveOrderMonth();
        }

        #endregion

        public static  int GetCurrentOrderMonth()
        {
            if (HttpContext.Current.Session != null)
            {
                var sessionOrderMonthData = HttpContext.Current.Session[OrderMonthDataSessionKey] as OrderMonthData;
                if (null != sessionOrderMonthData)
                {
                    return sessionOrderMonthData.CurrentOrderMonth;
                }
            }
            return GetOrderMonthValue();
            
        }
        #region Properties

        /// <summary>Returns the Order Month display string</summary>
        public string OrderMonthString
        {
            get { return calculateOrderMonthstring(); }
            //TBD, this is static as per page load time. Do we want this to be tied to the immediate time (we've already accounted for grace period)
        }

        /// <summary>Returns an alternate Order Month display string</summary>
        public string OrderMonthLongString
        {
            get { return GetOrderMonth().ToString(_OrderMonthLongFormat, _Culture.DateTimeFormat); }
            //TBD, this is static as per page load time. Do we want this to be tied to the immediate time (we've already accounted for grace period)
        }

        /// <summary>Returns an alternate Order Month display string</summary>
        public string OrderMonthShortString
        {
            get { return GetOrderMonth().ToString(_OrderMonthShortFormat, _Culture.DateTimeFormat); }
            //TBD, this is static as per page load time. Do we want this to be tied to the immediate time (we've already accounted for grace period)
        }

        /// <summary>Returns the Dual Order Month display string</summary>
        public string DualOrderMonthString
        {
            get { return HLConfigManager.Configurations.DOConfiguration.OrderMonthFormatLocalProvider ? _DualOrderMonthDate.ToString(_OrderMonthFormat, _Culture.DateTimeFormat) : _DualOrderMonthDate.ToString(_OrderMonthFormat, CultureInfo.InvariantCulture); }
        }

        /// <summary>Determines if we are currently in a Dual Order Month</summary>
        public bool IsDualOrderMonth
        {
            get { return _MonthEndDate < _CreateDateTime && _CreateDateTime < _DualOrderMonthDate; }
            //TBD, this is static as per page load time. Do we want this to be tied to the immediate time (we've already accounted for grace period)
        }

        /// <summary>Remembers if the user explicitly selected the OrderMonth in a Dual Order Month</summary>
        public DualOrderMonthSelection CurrentChosenOrderMonth
        {
            get { return _OrderMonthData.SelectedOrderMonth; }
            set
            {
                _OrderMonthData.SelectedOrderMonth = value;
                if (value == DualOrderMonthSelection.Current)
                {
                    _OrderMonthData.CurrentOrderMonth = OrderMonthNumber;
                }
            }
        }

        /// <summary>Returns the MonthEnd date and time of expiration</summary>
        public DateTime MonthEndDate
        {
            get { return _MonthEndDate; }
        }

        /// <summary>Returns the Dual Order MonthEnd date and time of expiration</summary>
        public DateTime DualOrderMonthEndDate
        {
            get { return _DualOrderMonthDate; }
        }

        /// <summary>Determines if we are still within the grace period for the chosen Order or Dual Order MonthEnd</summary>
        public bool StillWithinGracePeriod
        {
            get { return DateUtils.GetCurrentLocalTime(_CountryCode) < GetOrderMonth() + _MonthEndGracePeriod; }
        }

        //public bool OrderMonthChanged
        //{
        //    get { return _OrderMonthChanged; }
        //}

        private int OrderMonthNumber
        {
            get { return int.Parse(GetOrderMonth().ToString("yyMM", DateTimeFormatInfo.InvariantInfo)); }
        }

        public string PhoenixOrderMonth
        {
            get { return GetOrderMonth().ToString("yyyyMM"); }
        }

        public DateTime CurrentOrderMonth
        {
            get { return GetOrderMonth(); }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// This method provides a way to generate the month string of an order but taking into account any override in the currentSession
        /// </summary>
        /// <param name="currentSession">Reference to the current session</param>
        /// <returns>Month string</returns>
        public string GetOrderMonthString(SessionInfo currentSession)
        {
            if (null != currentSession)
            {
                if (!string.IsNullOrEmpty(currentSession.OrderMonthString))
                {
                    return currentSession.OrderMonthString;
                }
                else
                {
                    return this.OrderMonthString;
                }
            }
            else
            {
                return this.OrderMonthString;
            }
        }

        private bool isPC()
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            if (member!=null)
            {
                var user = member.Value;
                if (user!=null)
                {
                    DistributorOrderingProfile distributorOrderingProfile = DistributorOrderingProfileProvider.GetProfile(user.Id, _CountryCode);
                    return distributorOrderingProfile == null ? false : distributorOrderingProfile.IsPC;
                }
            }
            return false;
        }

        /// <summary>Register the OrderMonth controls for notifications</summary>
        /// <param name="OrderMonthControl">The OrderMonth Control</param>
        /// <param name="OrderMonthChangedNotificationLabel">The OrderMonth Message Label</param>
        public void RegisterOrderMonthControls(Panel OrderMonthPanel,
                                               Label OrderMonthChangedNotificationLabel,
                                               bool isEventTicketMode,
                                               Panel OrderMonthPanelLabel, bool isHAPMode=false)
        {
            if (!isPC() && IsDualOrderMonth && !isEventTicketMode && !isHAPMode &&
                HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled)
            {
                var config = HLConfigManager.Configurations.DOConfiguration;
                string currentMonth = config.OrderMonthFormatLocalProvider
                                          ? DualOrderMonthEndDate.ToString(config.OrderMonthFormat)
                                          : DualOrderMonthEndDate.ToString(config.OrderMonthFormat,
                                                                           CultureInfo.InvariantCulture);
                string previousMonth = config.OrderMonthFormatLocalProvider
                                           ? MonthEndDate.ToString(config.OrderMonthFormat)
                                           : MonthEndDate.ToString(config.OrderMonthFormat, CultureInfo.InvariantCulture);
                var ddlOrderMonth = new DropDownList();
                ddlOrderMonth.Items.Add(new ListItem(currentMonth, DualOrderMonthSelection.Current.ToString()));
                if (config.OrderMonthSort == "Ascending")
                {
                    ddlOrderMonth.Items.Insert(0,
                                               (new ListItem(previousMonth, DualOrderMonthSelection.Previous.ToString())));
                }
                else
                {
                    ddlOrderMonth.Items.Add(new ListItem(previousMonth, DualOrderMonthSelection.Previous.ToString()));
                }
                ddlOrderMonth.SelectedIndexChanged += ddlOrderMonth_SelectedIndexChanged;
                ddlOrderMonth.AutoPostBack = true;
                ddlOrderMonth.EnableViewState = true;
                OrderMonthPanel.Visible = true;
                OrderMonthPanel.Controls.Add(ddlOrderMonth);
                if (config.OrderMonthSort == "Descending")
                {
                    CurrentChosenOrderMonth = (DualOrderMonthSelection)Enum.Parse(typeof(DualOrderMonthSelection), ddlOrderMonth.Items[0].Value);
                }

                if (!config.PreselectedDualOrderMonth)
                {
                    ddlOrderMonth.Items.Insert(0, (new ListItem(string.Empty, OrderMonth.DualOrderMonthSelection.NotSelected.ToString())));

                    if (ddlOrderMonth.Items.FindByValue(CurrentChosenOrderMonth.ToString()) != null)
                        ddlOrderMonth.SelectedValue = CurrentChosenOrderMonth.ToString();
                }
                
                if (CurrentChosenOrderMonth == DualOrderMonthSelection.Current ||
                    CurrentChosenOrderMonth == DualOrderMonthSelection.Previous)
                {
                    if (ddlOrderMonth.Items.FindByValue(CurrentChosenOrderMonth.ToString()) != null)
                        ddlOrderMonth.SelectedValue = CurrentChosenOrderMonth.ToString();
                }

                var sessionOrderMonthData = Session[OrderMonthDataSessionKey] as OrderMonthData;
                if (sessionOrderMonthData != null)
                {
                    if (ddlOrderMonth.Items.FindByValue(sessionOrderMonthData.SelectedOrderMonth.ToString()) != null)
                        ddlOrderMonth.SelectedValue = sessionOrderMonthData.SelectedOrderMonth.ToString();
                }
                
                _OrderMonthControls.Add(ddlOrderMonth);
                OrderMonthPanelLabel.Visible = false;
            }
            else
            {
                var lbOrderMonth = new Label();
                if (isEventTicketMode || !HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled)
                {
                    CurrentChosenOrderMonth =
                        (HLConfigManager.Configurations.DOConfiguration.OrderMonthSort.ToUpper() == "ASCENDING")
                            ? DualOrderMonthSelection.Current
                            : DualOrderMonthSelection.Previous;
                }
                //else
                //{
                //    CurrentChosenOrderMonth = DualOrderMonthSelection.Current;
                //}
                OrderMonthPanelLabel.Visible = true;
                OrderMonthPanel.Visible = false;
                lbOrderMonth.Text = OrderMonthString;
                OrderMonthPanelLabel.Controls.Add(lbOrderMonth);
                _OrderMonthControls.Add(lbOrderMonth);
            }

            //if (null != OrderMonthPanel)
            //{
            //    _OrderMonthControls.Add(OrderMonthPanel);
            //}

            if (null != OrderMonthChangedNotificationLabel)
            {
                _OrderMonthNotificationControls.Add(OrderMonthChangedNotificationLabel);
            }
        }

        public string DualOrderMonthForEventTicket(bool isEventTicketMode, out string orderMonthShortString)
        {
            var lbOrderMonth = new Label();
            if (isEventTicketMode || !HLConfigManager.Configurations.DOConfiguration.OrderMonthEnabled)
            {
                CurrentChosenOrderMonth =
                    (HLConfigManager.Configurations.DOConfiguration.OrderMonthSort.ToUpper() == "ASCENDING")
                        ? DualOrderMonthSelection.Current
                        : DualOrderMonthSelection.Previous;
                orderMonthShortString = OrderMonthShortString;
                return   OrderMonthString;
            }
            orderMonthShortString = string.Empty;
            return string.Empty;
            //else
            //{
            //    CurrentChosenOrderMonth = DualOrderMonthSelection.Current;
            //}
            //OrderMonthPanelLabel.Visible = true;
            //OrderMonthPanel.Visible = false;
            //lbOrderMonth.Text = OrderMonthString;
            //OrderMonthPanelLabel.Controls.Add(lbOrderMonth);
            //_OrderMonthControls.Add(lbOrderMonth);
        }
        public void ddlOrderMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddlOrderMonth = sender as DropDownList;
            CurrentChosenOrderMonth =
                (DualOrderMonthSelection) Enum.Parse(typeof (DualOrderMonthSelection), ddlOrderMonth.SelectedValue);
            var sessionOrderMonthData = Session[OrderMonthDataSessionKey] as OrderMonthData;
            if (sessionOrderMonthData != null)
                sessionOrderMonthData.SelectedOrderMonth = CurrentChosenOrderMonth;
            ResolveOrderMonth();
            foreach (Control ctl in _OrderMonthControls)
            {
                if (ctl is DropDownList)
                {
                    (ctl as DropDownList).SelectedValue = ddlOrderMonth.SelectedValue;
                }
                else if (ctl is Label)
                {
                    (ctl as Label).Text = OrderMonthString;
                }
            }
            OrderMonthChanged(this, null);
        }

        #endregion

        #region Private methods

        /// <summary>Initialize this object</summary>
        private void Initialize()
        {
            _CreateDateTime = DateUtils.GetCurrentLocalTime(_CountryCode);
            var orderMonth = _CreateDateTime;
            var fromDate = DateTimeUtils.GetFirstDayOfMonth(orderMonth);
            var from = new DateTime(fromDate.Year, fromDate.Month, fromDate.Day);
            var toDate = DateTimeUtils.GetLastDayOfMonth(orderMonth);
            var to = new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 59, 59);
            _MonthEndDate = _DualOrderMonthDate = to;
            //List<DualMonthPair> dualMonth = ShoppingCartProvider.GetDualOrderMonthDatesFromService(from, to, _CountryCode);
            if (!isPC())
            {
                var dualMonth = ShoppingCartProvider.GetDualOrderMonthDates(from, to, _CountryCode);
                if (null != dualMonth)
                {
                    foreach (DualMonthPair dmp in dualMonth)
                    {
                        if (_CreateDateTime < dmp.MonthEndDate)
                        {
                            // when MonthEndDate is 2014-12-31 23:59:59.000,  _CreateDateTime is 12/31, _CreateDateTime.Day is 31 and OrderMonth is 11 => exception 
                            int year = Convert.ToInt32(dmp.OrderMonth.Substring(0, 4));
                            int month = Convert.ToInt32(dmp.OrderMonth.Substring(4, 2));
                            var lastDay = DateTimeUtils.GetLastDayOfMonth(new DateTime(year,month,1));
                            _DualOrderMonthDate = dmp.MonthEndDate;
                            _MonthEndDate = new DateTime(year,month,lastDay.Day, 23, 59, 59);

                        //    _MonthEndDate = new DateTime(Convert.ToInt32(dmp.OrderMonth.Substring(0, 4)),
                        //                                 Convert.ToInt32(dmp.OrderMonth.Substring(4, 2)),
                        //                                 _CreateDateTime.Day, 23, 59, 59);
                        }
                        break;
                    }

                    if (HLConfigManager.Configurations.DOConfiguration.ExtendDualOrderMonth > 0)
                    {
                        _DualOrderMonthDate =
                            _DualOrderMonthDate.AddHours(
                                HLConfigManager.Configurations.DOConfiguration.ExtendDualOrderMonth);
                    }
                }
                else
                {
                    LoggerHelper.Error("Failed to get Dual Month info from the Catalog Service");
                }
            }
            _OrderMonthData.CurrentOrderMonth = OrderMonthNumber;

            //Account for any EOM grace period
            //if (_MonthEndGracePeriod.Ticks  > 0)
            //{
            //    _MonthEndDate = to + _MonthEndGracePeriod;
            //    _DualOrderMonthDate += _MonthEndGracePeriod;
            //}
        }

        /// <summary>Resolve the current order month against the users session</summary>
        private void ResolveOrderMonth()
        {
            var sessionOrderMonthData = Session == null ? null : Session[OrderMonthDataSessionKey] as OrderMonthData;
            if (null == sessionOrderMonthData)
            {
                _OrderMonthData.CurrentOrderMonth = OrderMonthNumber;
                sessionOrderMonthData = _OrderMonthData;
                if(Session !=null)
                    Session.Add(OrderMonthDataSessionKey, _OrderMonthData);
            }
            if (IsDualOrderMonth)
            {
                //Get selection of default to previous month
                _OrderMonthData.SelectedOrderMonth = (sessionOrderMonthData.SelectedOrderMonth ==
                                                      DualOrderMonthSelection.NotSelected)
                                                         ? DualOrderMonthSelection.Previous
                                                         : sessionOrderMonthData.SelectedOrderMonth;
            }
            else
            {
                if (null != sessionOrderMonthData)
                {
                    if (sessionOrderMonthData.CurrentOrderMonth < OrderMonthNumber)
                    {
                        _MonthEndDate = _MonthEndDate.AddMonths(-1);
                        _OrderMonthData.CurrentOrderMonth = sessionOrderMonthData.CurrentOrderMonth;
                    }
                }
            }

            _OrderMonthChanged = (!IsDualOrderMonth && OrderMonthNumber > sessionOrderMonthData.CurrentOrderMonth);
            foreach (Control ctl in _OrderMonthControls)
            {
                var lbl = ctl as Label;
                if (null != lbl)
                {
                    lbl.Text = OrderMonthLongString;
                    if (_OrderMonthChanged) lbl.Attributes.Add("class", "red");
                }
                var ddl = ctl as DropDownList;
                if (null != ctl)
                {
                    (ctl as DropDownList).SelectedValue = _OrderMonthData.SelectedOrderMonth.ToString();
                }
            }
            if (_OrderMonthChanged)
            {
                foreach (Label lbl in _OrderMonthNotificationControls)
                {
                    lbl.Text =
                        HttpContext.GetGlobalResourceObject(
                            string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OrderMonthHasFlipped") as
                        string;
                    lbl.Attributes.Add("class", "red");
                }
            }
            //TBD - other logic comparisons between current and previous (session) instances
            sessionOrderMonthData.CurrentOrderMonth = OrderMonthNumber; // _OrderMonthData.CurrentOrderMonth;
            sessionOrderMonthData.SelectedOrderMonth = _OrderMonthData.SelectedOrderMonth;
        }

        private static int GetOrderMonthValue()
        {
            var actualdate = DateUtils.GetCurrentLocalTime(Thread.CurrentThread.CurrentCulture.ToString());
            return int.Parse(actualdate.ToString("yyMM", DateTimeFormatInfo.InvariantInfo));
        }
        /// <summary>Get the current OrderMonth as per user selection or default</summary>
        /// <returns>The Order Month</returns>
        private DateTime GetOrderMonth()
        {        
            switch (_OrderMonthData.SelectedOrderMonth)
            {
                case DualOrderMonthSelection.Previous:
                    {
                        return _MonthEndDate;
                    }
                case DualOrderMonthSelection.Current:
                    {
                        return _DualOrderMonthDate;
                    }
                default:
                    {
                        return _MonthEndDate;
                    }
            }
        }

        private void changeFormatCalendarToGregorian()
        {
            //if (System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar.Equals(new System.Globalization.ThaiBuddhistCalendar()))
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_Culture.Name);
            System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.Calendar = new System.Globalization.GregorianCalendar();
        }

        /// <summary>
        /// Calculates the order monthstring and validate if need to change to Gregorian.
        /// </summary>
        /// <returns></returns>
        private string calculateOrderMonthstring()
        {
            //Calculate the original value
            var omString = HLConfigManager.Configurations.DOConfiguration.OrderMonthFormatLocalProvider ? GetOrderMonth().ToString(_OrderMonthFormat, _Culture.DateTimeFormat) : GetOrderMonth().ToString(_OrderMonthFormat, CultureInfo.InvariantCulture);

            if (HLConfigManager.Configurations.DOConfiguration.UseGregorianCalendar)
            {
                var gregorianYear = DateTime.Now.Year;
                if (omString.Contains(gregorianYear.ToString()))
                {
                    var lenght = omString.Length-4;
                    string monthname;

                    double Num;
                    //validate if the format is MMMM yyyy
                    bool isNum = double.TryParse(omString.Substring(lenght), out Num);
                    if (isNum)
                    {
                        monthname = omString.Substring(0, lenght) + gregorianYear;
                    }
                        //format yyyy MMMM
                    else
                    {
                        monthname = gregorianYear + omString.Substring(4);
                    }

                    omString = monthname;
                    //omString = omString.Substring(0, lenght)+ gregorianYear;
                }
            }
            return omString;
        }

        #endregion
    }


}