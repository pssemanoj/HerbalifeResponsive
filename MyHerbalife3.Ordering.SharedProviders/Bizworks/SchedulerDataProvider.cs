using System;
using System.Collections.Generic;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.DistributorCrmSvc;

namespace MyHerbalife3.Ordering.SharedProviders.Bizworks
{
    public class SchedulerDataProvider
    {

        #region Appointment

        /// <summary>
        /// Lists the appointments.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns></returns>
        public static List<Appointment_V01> ListAppointments(string distributorID, DateTime startDate, DateTime endDate)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    ListAppointmentsByDateRequest_V01 requestV01 = new ListAppointmentsByDateRequest_V01
                    {
                        DistributorID = distributorID,
                        StartDate = startDate,
                        EndDate = endDate
                    };
                    ListAppointmentsResponse_V01 responseV01 =
                        proxy.ListAppointments(new ListAppointmentsRequest1(requestV01)).ListAppointmentsResult as ListAppointmentsResponse_V01;
                    //todo: add exception/error handling
                    return responseV01.Appointments;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }


        /// <summary>
        /// Saves the appointment.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="appointment">The appointment.</param>
        /// <param name="isNew">if set to <c>true</c> [is new].</param>
        /// <param name="saveDatesOnly">if set to <c>true</c> [save dates only].</param>
        /// <returns></returns>
        public static int SaveAppointment(string distributorID, Appointment_V01 appointment, bool isNew, bool saveDatesOnly)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var request01 = new SaveAppointmentRequest_V01()
                    {
                        DistributorID = distributorID,
                        Appointment = appointment,
                        IsNew = isNew,
                        UpdateDatesOnly = saveDatesOnly
                    };
                    SaveAppointmentResponse_V01 responseV01 =
                        proxy.SaveAppointment(new SaveAppointmentRequest1(request01)).SaveAppointmentResult as SaveAppointmentResponse_V01;
                    //todo: validate response, handle errors.
                    return responseV01.AppointmentID;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Saves the appointment.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="appointment">The appointment.</param>
        /// <param name="isNew">if set to <c>true</c> [is new].</param>
        /// <param name="saveDatesOnly">if set to <c>true</c> [save dates only].</param>
        /// <param name="bWantEntireResult">if set to true return the entire repsonse</param>
        /// <returns></returns>
        public static SaveAppointmentResponse_V01 SaveAppointment(string distributorID, Appointment_V01 appointment, bool isNew, bool saveDatesOnly, bool bWantEntireResult)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    SaveAppointmentRequest_V01 request01 = new SaveAppointmentRequest_V01()
                    {
                        DistributorID = distributorID,
                        Appointment = appointment,
                        IsNew = isNew,
                        UpdateDatesOnly = saveDatesOnly
                    };
                    SaveAppointmentResponse_V01 responseV01 =
                        proxy.SaveAppointment(new SaveAppointmentRequest1(request01)).SaveAppointmentResult as SaveAppointmentResponse_V01;
                    //todo: validate response, handle errors.
                    return responseV01;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }



        /// <summary>
        /// Gets the appointment by ID.
        /// </summary>
        /// <param name="appointmentID">The appointment ID.</param>
        /// <returns></returns>
        public static Appointment_V01 GetAppointmentByID(int appointmentID)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    GetAppointmentByIDRequest_V01 request01 = new GetAppointmentByIDRequest_V01()
                    {
                        AppointmentID = appointmentID
                    };

                    GetAppointmentByIDResponse_V01 responseV01 = proxy.GetAppointment(new GetAppointmentRequest1(request01)).GetAppointmentResult as GetAppointmentByIDResponse_V01;

                    return responseV01.Appointment;

                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Deletes the appointments by ID.
        /// </summary>
        /// <param name="appointmentIDs">The appointment I ds.</param>
        /// <returns></returns>
        public static bool DeleteAppointmentsByID(int[] appointmentIDs)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var iList = new List<int>();
                    iList.AddRange(appointmentIDs);

                    DeleteAppointmentsByIDRequest_V01 request01 = new DeleteAppointmentsByIDRequest_V01()
                    {
                        AppointmentIDs = iList
                    };

                    DeleteAppointmentsByIDResponse_V01 responseV01 =
                        proxy.DeleteAppointments(new DeleteAppointmentsRequest1(request01)).DeleteAppointmentsResult as DeleteAppointmentsByIDResponse_V01;

                    return responseV01.Status == ServiceResponseStatusType.Success;

                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }

        }

        /// <summary>
        /// Deletes the appointment by ID.
        /// </summary>
        /// <param name="appointmentID">The appointment ID.</param>
        /// <returns></returns>
        public static bool DeleteAppointmentByID(int appointmentID)
        {
            return DeleteAppointmentsByID(new int[] { appointmentID });
        }

        #endregion

        #region Calendar Settings
        /// <summary>
        /// Gets the calendar settings by owner.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <returns></returns>
        public static CalendarSettings_V01 GetCalendarSettingsByOwner(string distributorID)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    var request01 = new GetCalendarSettingsRequest_V01()
                    {
                        DistributorID = distributorID
                    };


                    var response01 =
                        proxy.GetCalendarSettingsByOwner(new GetCalendarSettingsByOwnerRequest(request01)).GetCalendarSettingsByOwnerResult as GetCalendarSettingsResponse_V01;

                    return response01.CalendarSettings;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }


        /// <summary>
        /// Saves the calendar settings.
        /// </summary>
        /// <param name="distributorID">The distributor ID.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="isNew">if set to <c>true</c> [is new].</param>
        /// <returns></returns>
        public static int SaveCalendarSettings(string distributorID, CalendarSettings_V01 settings, bool isNew)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    SaveCalendarSettingsRequest_V01 request01 = new SaveCalendarSettingsRequest_V01()
                    {
                        DistributorID = distributorID,
                        CalendarSettings = settings,
                        IsNew = isNew
                    };

                    SaveCalendarSettingsResponse_V01 response01 =
                        proxy.SaveCalendarSettings(new SaveCalendarSettingsRequest1(request01)).SaveCalendarSettingsResult as SaveCalendarSettingsResponse_V01;

                    return response01.CalendarSettingsID;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }

        #endregion

        #region Task
        public static List<Task_V01> ListTasks(string distributorID)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    ListTasksRequest_V01 requestV01 = new ListTasksRequest_V01
                    {
                        DistributorID = distributorID,

                    };
                    ListTasksResponse_V01 responseV01 =
                        proxy.ListTasks(new ListTasksRequest1(requestV01)).ListTasksResult as ListTasksResponse_V01;
                    //todo: add exception/error handling

                    SortByDueDate(responseV01.Tasks);

                    return responseV01.Tasks;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }



        public static List<Task_V01> ListTasksSummary(string distributorID)
        {
            //todo: when caching implemented, this call can be optimized
            return ListTasks(distributorID);
        }

        public static Task_V01 GetTaskByID(int taskID)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    GetTaskByIDRequest_V01 request01 = new GetTaskByIDRequest_V01()
                    {
                        TaskID = taskID
                    };

                    GetTaskByIDResponse_V01 responseV01 = proxy.GetTask(new GetTaskRequest1(request01)).GetTaskResult as GetTaskByIDResponse_V01;

                    return responseV01.Task;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }

        }

        public static int SaveTask(string distributorID, Task_V01 task, bool isNew)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    SaveTaskRequest_V01 request01 = new SaveTaskRequest_V01()
                    {
                        DistributorID = distributorID,
                        Task = task,
                        IsNew = isNew,
                    };
                    SaveTaskResponse_V01 responseV01 =
                        proxy.SaveTask(new SaveTaskRequest1(request01)).SaveTaskResult as SaveTaskResponse_V01;
                    //todo: validate response, handle errors.
                    return responseV01.TaskID;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }
        }

        public static void DeleteTasksByID(string DistributorID, List<int> taskIDs)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    DeleteTasksByIDRequest_V01 request = new DeleteTasksByIDRequest_V01();
                    request.TaskIDs = taskIDs;

                    DeleteTasksByIDResponse_V01 response = proxy.DeleteTasks(new DeleteTasksRequest(request)).DeleteTasksResult as DeleteTasksByIDResponse_V01;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                    throw ex;
                }
            }

        }

        public static void SaveTasksStatusByIDs(TaskStatusType newStatus, List<int> taskIDs)
        {
            using (var proxy = ServiceClientProvider.GetDistributorCRMServiceProxy())
            {
                try
                {
                    SaveTasksStatusRequest_V01 request = new SaveTasksStatusRequest_V01();
                    request.TaskIDs = taskIDs;
                    request.NewStatus = newStatus;

                    SaveTasksStatusResponse_V01 response = proxy.SaveTasksStatus(new SaveTasksStatusRequest(request)).SaveTasksStatusResult as SaveTasksStatusResponse_V01;
                }
                catch (Exception ex)
                {
                    Log(ex, proxy);
                }
            }
        }


        public static void SaveTasksStatusByID(TaskStatusType newStatus, int taskID)
        {
            List<int> taskIDs = new List<int>();

            taskIDs.Add(taskID);

            SaveTasksStatusByIDs(newStatus, taskIDs);
        }

        private static void SortByDueDate(List<Task_V01> list)
        {
            if (list.Count == 0)
                return;

            list.Sort((p, q) => Compare(p, q));
        }

        public static int Compare(Task_V01 p, Task_V01 q)
        {
            //note sorting is only based on Overrdue status and havind due date
            //. Sorting is not based on status at all.
            int sortOrder = 1;
            int result = 0;

            if (!p.DueTime.HasValue && !q.DueTime.HasValue)
            {
                result = -DateTime.Compare(p.LastUpdateTime, q.LastUpdateTime);
            }

            else if (p.DueTime.HasValue && !q.DueTime.HasValue)
            {
                result = -1;
            }

            else if (!p.DueTime.HasValue && q.DueTime.HasValue)
            {
                result = 1;
            }
            else//both have due time, 
            {
                if (IsOverdue(p) && IsOverdue(q))
                {
                    result = DateTime.Compare(p.DueTime.Value, q.DueTime.Value);
                }
                else if (!IsOverdue(p) && !IsOverdue(q))
                {
                    result = DateTime.Compare(p.DueTime.Value, q.DueTime.Value);
                }
                else if (IsOverdue(p) && !IsOverdue(q))
                {
                    result = -1;
                }
                else if (!IsOverdue(p) && IsOverdue(q))
                {
                    result = 1;
                }

            }

            return sortOrder * result;
        }

        public static bool IsOverdue(Task_V01 task)
        {
            //todo: validate logic
            //note: checking status has to be happen here, don't remove it
            //this method is used on two pages as well.
            return task.Status.Key !=
                HL.DistributorCRM.ValueObjects.Task.TaskStatusType.COMPLE.Key &&
                task.DueTime.HasValue &&
                task.DueTime < DateTime.UtcNow;
        }


        #endregion


        private static void Log(Exception ex, DistributorCRMServiceClient dCrmService)
        {
            WebUtilities.LogServiceExceptionWithContext<IDistributorCRMService>(ex, dCrmService);
        }
    }
}
