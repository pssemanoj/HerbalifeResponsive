using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ExpertPdf.HtmlToPdf;
using HL.Blocks.Caching.SimpleCache;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.ServiceProvider;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.SharedProviders.Invoices
{
    public class InvoiceProvider
    {
        private const string INVOICE_CACHE_KEY_PREFIX = "INVOICE_";
        private static int CACHE_IN_MINUTES;
        private static string orderEndPoint;

        /// <summary>
        ///     Return the Check payment details of the given Distributor
        /// </summary>
        /// <param name="distributorID"></param>
        /// <returns></returns>
        public static List<Invoice> GetInvoices(string distributorID)
        {
            if (CACHE_IN_MINUTES == 0)
            {
                int.TryParse(HL.Common.Configuration.Settings.GetRequiredAppSetting("CommunicationPreferenceCacheExpireMinutes", "20"),
                             out CACHE_IN_MINUTES);
            }

            var cacheKey = INVOICE_CACHE_KEY_PREFIX + distributorID;
            var response = SimpleCache.Retrieve(_ => getInvoicesResponse(distributorID), cacheKey,
                                                new TimeSpan(0, CACHE_IN_MINUTES, 0));
            if (response != null && response.Status == ServiceResponseStatusType.Success
                && null != response.Invoices && response.Invoices.Count > 0)
            {
                return response.Invoices;
            }
            else
            {
                ExpireInvoicesCache(distributorID);
                return null;
            }
        }

        /// <summary>
        ///     Gets the next valid invoice number for the given distributor
        /// </summary>
        /// <param name="distributorID">The ID of the distributor</param>
        /// <returns>The next valid invoice number</returns>
        public static Int64 GetNextDistributorInvoiceNumber(string distributorID)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new GetNextDistributorInvoiceNumberRequest_V01
                {
                    DistributorID = distributorID
                };
                var response =
                    proxy.GetNextDistributorInvoiceNumber(new GetNextDistributorInvoiceNumberRequest1(request)).GetNextDistributorInvoiceNumberResult as GetNextDistributorInvoiceNumberResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success
                    && response.InvoiceNumber > 0)
                {
                    return response.InvoiceNumber;
                }
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: GetNextDistributorInvoiceNumber\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return 0;
        }

        public static List<InvoiceSKU> GetInvoiceSKU(string distributorID, Int64 invoiceID)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new GetInvoiceSkuRequest_V01
                {
                    DistributorID = distributorID,
                    InvoiceID = invoiceID
                };
                var response = proxy.GetInvoiceSku(new GetInvoiceSkuRequest1(request)).GetInvoiceSkuResult as GetInvoiceSkuResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success
                    && null != response.InvoiceSkus && response.InvoiceSkus.Count > 0)
                {
                    return response.InvoiceSkus;
                }
                return null;
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: GetInvoiceSKU\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return null;
        }

        public static bool DeleteInvoice(string distributorID, long invoiceID, long contactInfoID)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new DeleteInvoiceRequest_V01
                {
                    DistributorID = distributorID,
                    InvoiceID = invoiceID,
                    ContactInfoID = contactInfoID
                };
                var response = proxy.DeleteInvoice(new DeleteInvoiceRequest1(request)).DeleteInvoiceResult as DeleteInvoiceResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    ExpireInvoicesCache(distributorID);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: DeleteInvoice\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return false;
        }

        public static Int64 SetInvoice(Invoice invoice)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new SetInvoiceRequest_V01
                {
                    Invoice = invoice
                };
                var response = proxy.SetInvoice(new SetInvoiceRequest1(request)).SetInvoiceResult as SetInvoiceResponse_V01;
                if (response != null && response.Status == ServiceResponseStatusType.Success)
                {
                    ExpireInvoicesCache(invoice.DistributorID);
                    return response.InvoiceID;
                }
                return 0;
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: SetInvoice\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return 0;
        }

        public static Invoice GetDetailedInvoice(string distributorID, int distributorInvoiceNumber)
        {
            var allInvoices = GetInvoices(distributorID);
            if (null != allInvoices && allInvoices.Count > 0)
            {
                var invoices = allInvoices.Where(i => i.DistributorInvoiceNumber == distributorInvoiceNumber);
                if (null != invoices && invoices.Count() > 0)
                {
                    var invoice = invoices.First();
                    var invoiceSkus = GetInvoiceSKU(distributorID, invoice.ID);
                    if (null != invoiceSkus && invoiceSkus.Count > 0)
                    {
                        invoice.InvoiceSkus = invoiceSkus;
                        return invoice;
                    }
                    return invoice;
                }
            }
            return null;
        }

        public static long GetDistributorInvoiceNumber(string distributorID, long invoiceID)
        {
            var allInvoices = GetInvoices(distributorID);
            if (null != allInvoices && allInvoices.Count > 0)
            {
                var invoices = allInvoices.Where(i => i.ID == invoiceID);
                if (null != invoices && invoices.Count() > 0)
                {
                    var invoice = invoices.First();
                    return invoice.DistributorInvoiceNumber;
                }
            }
            return 0;
        }

        public static long GetInvoiceIDFromDistributorInvoiceNumber(string distributorID, long distributorInvoiceNumber)
        {
            var allInvoices = GetInvoices(distributorID);
            if (null != allInvoices && allInvoices.Count > 0)
            {
                var invoices = allInvoices.Where(i => i.DistributorInvoiceNumber == distributorInvoiceNumber);
                if (null != invoices && invoices.Count() > 0)
                {
                    var invoice = invoices.First();
                    if (null != invoice)
                    {
                        return invoice.ID;
                    }
                }
            }
            return 0;
        }

        public static long GetInvoiceContactInfoID(string distributorID, long invoiceID)
        {
            var allInvoices = GetInvoices(distributorID);
            if (null != allInvoices && allInvoices.Count > 0)
            {
                var invoices = allInvoices.Where(i => i.ID == invoiceID);
                if (null != invoices && invoices.Count() > 0)
                {
                    var invoice = invoices.First();
                    if (null != invoice)
                    {
                        return invoice.ContactInfoID;
                    }
                }
            }
            return 0;
        }

        public static List<Invoice> SearchInvoice(InvoiceSearchFilter invoiceSearchFilter, string distributorID)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var allInvoices = GetInvoices(distributorID);

                if (null == allInvoices)
                {
                    return null;
                }

                if (invoiceSearchFilter != null)
                {
                    if (invoiceSearchFilter.SearchByDateOrAmount)
                    {
                        return DoSearchByDateOrAmount(invoiceSearchFilter, allInvoices);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(invoiceSearchFilter.SKU) ||
                            !string.IsNullOrEmpty(invoiceSearchFilter.Description))
                        {
                            var request = new SearchInvoiceRequest_V01
                            {
                                DistributorID = distributorID,
                                SKU = invoiceSearchFilter.SKU,
                                Description = invoiceSearchFilter.Description
                            };
                            var response = proxy.SearchInvoice(new SearchInvoiceRequest1(request)).SearchInvoiceResult as SearchInvoiceResponse_V01;

                            if (response != null && response.Status == ServiceResponseStatusType.Success
                                && null != response.InvoiceIDs && response.InvoiceIDs.Count > 0)
                            {
                                var invoicesBySkuOrDesc = allInvoices.Where(i => response.InvoiceIDs.Any(a => a == i.ID));
                                if (null != invoicesBySkuOrDesc && invoicesBySkuOrDesc.Count() > 0)
                                {
                                    return invoicesBySkuOrDesc.ToList();
                                }
                                return null;
                            }
                        }
                        else
                        {
                            return DoSearchByOtherFields(invoiceSearchFilter, allInvoices);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: SearchInvoice\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return null;
        }

        private static List<Invoice> DoSearchByOtherFields(InvoiceSearchFilter invoiceSearchFilter,
                                                           IEnumerable<Invoice> allInvoices)
        {
            //shan - mar 09, 2012 - match the values without case sensitive
            //Displays invoices related to a particular contact
            //Fix to Defect#39425
            if (!string.IsNullOrEmpty(invoiceSearchFilter.FirstName) &&
                !string.IsNullOrEmpty(invoiceSearchFilter.LastName))
            {
                allInvoices = allInvoices.Where(
                    i => i.FirstName.ToUpper().Equals(invoiceSearchFilter.FirstName.ToUpper())
                         && i.LastName.ToUpper().Equals(invoiceSearchFilter.LastName.ToUpper()))
                                         .OrderByDescending(i => i.ID);
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.FirstName))
            {
                allInvoices =
                    allInvoices.Where(i => i.FirstName.ToLower().Contains(invoiceSearchFilter.FirstName.ToLower()));
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.LastName))
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.LastName.ToLower().Contains(invoiceSearchFilter.LastName.ToLower()));
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.StreetAddress))
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.Address1.ToLower().Contains(invoiceSearchFilter.StreetAddress.ToLower()));
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.City))
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.City.ToLower().Contains(invoiceSearchFilter.City.ToLower()));
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.State))
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.State.ToLower().Contains(invoiceSearchFilter.State.ToLower()));
            }
            else if (!string.IsNullOrEmpty(invoiceSearchFilter.ZipCode))
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.PostalCode.ToLower().Contains(invoiceSearchFilter.ZipCode.ToLower()));
            }
            else if (invoiceSearchFilter.InvoiceTotal.HasValue)
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.TotalDue == invoiceSearchFilter.InvoiceTotal);
            }
            else if (invoiceSearchFilter.TotalVolumePoints.HasValue)
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.TotalVolumePoints == invoiceSearchFilter.TotalVolumePoints);
            }

            if (null != allInvoices && allInvoices.Count() > 0)
            {
                return allInvoices.ToList();
            }
            return null;
        }

        private static List<Invoice> DoSearchByDateOrAmount(InvoiceSearchFilter invoiceSearchFilter,
                                                            IEnumerable<Invoice> allInvoices)
        {
            if (null == invoiceSearchFilter.FromDate &&
                null != invoiceSearchFilter.ToDate)
            {
                allInvoices =
                    allInvoices.Where(
                        i => i.InvoiceDate <= invoiceSearchFilter.ToDate);
            }

            if (null != invoiceSearchFilter.FromDate &&
                null == invoiceSearchFilter.ToDate)
            {
                allInvoices =
                    allInvoices.Where(i => i.InvoiceDate >= invoiceSearchFilter.FromDate &&
                                           i.InvoiceDate <= DateTime.Now);
            }

            if (null != invoiceSearchFilter.FromDate &&
                null != invoiceSearchFilter.ToDate)
            {
                allInvoices =
                    allInvoices.Where(i => i.InvoiceDate >= invoiceSearchFilter.FromDate &&
                                           i.InvoiceDate <= invoiceSearchFilter.ToDate);
            }

            //begin - shan - mar 14, 2012 - filter either by null or 0
            //field can be left empty or 0 can be entered..
            //if empty..dont consider as criteria..if 0..filter based on that..
            //if empty..null should have been set from UI
            //if (invoiceSearchFilter.StartAmount == null && invoiceSearchFilter.EndAmount > 0)
            //{
            //    invoiceSearchFilter.StartAmount = 0;
            //    allInvoices = allInvoices.
            //        Where(i => i.TotalDue >= invoiceSearchFilter.StartAmount &&
            //        i.TotalDue <= invoiceSearchFilter.EndAmount);
            //}

            //if (invoiceSearchFilter.StartAmount > 0 && invoiceSearchFilter.EndAmount == null)
            //{
            //    invoiceSearchFilter.EndAmount = 0;
            //    allInvoices = allInvoices.
            //        Where(i => i.TotalDue >= invoiceSearchFilter.StartAmount);
            //}

            //if ((invoiceSearchFilter.StartAmount > 0 && invoiceSearchFilter.EndAmount > 0) ||
            //    (invoiceSearchFilter.StartAmount.HasValue && invoiceSearchFilter.EndAmount.HasValue))
            //{
            //    allInvoices = allInvoices.
            //        Where(i => i.TotalDue >= invoiceSearchFilter.StartAmount &&
            //        i.TotalDue <= invoiceSearchFilter.EndAmount);
            //}
            if (null == invoiceSearchFilter.StartAmount
                && 0 < invoiceSearchFilter.EndAmount)
            {
                allInvoices = allInvoices.
                    Where(i => i.TotalDue <= invoiceSearchFilter.EndAmount);
            }

            if (0 < invoiceSearchFilter.StartAmount
                && null == invoiceSearchFilter.EndAmount)
            {
                allInvoices = allInvoices.
                    Where(i => i.TotalDue >= invoiceSearchFilter.StartAmount);
            }

            if (invoiceSearchFilter.StartAmount.HasValue
                && invoiceSearchFilter.EndAmount.HasValue)
            {
                allInvoices = allInvoices.
                    Where(i => i.TotalDue >= invoiceSearchFilter.StartAmount &&
                               i.TotalDue <= invoiceSearchFilter.EndAmount);
            }
            //end

            if (null != allInvoices && allInvoices.Count() > 0)
            {
                return allInvoices.ToList();
            }
            return null;
        }

        /// <summary>
        ///     method to get the check payment details response from service
        /// </summary>
        /// <param name="distributorID"></param>
        /// <returns></returns>
        private static GetInvoicesResponse_V01 getInvoicesResponse(string distributorID)
        {
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                var request = new GetInvoicesRequest_V01
                {
                    DistributorID = distributorID,
                };
                var response = proxy.GetInvoices(new GetInvoicesRequest1(request)).GetInvoicesResult as GetInvoicesResponse_V01;
                return response;
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception("OrderService: getInvoicesResponse\n", ex));
            }
            finally
            {
                if (proxy.State != CommunicationState.Closed)
                {
                    proxy.Close();
                }
                proxy = null;
            }
            return null;
        }

        /// <summary>
        ///     explicit method to clean up the check payment details response
        /// </summary>
        /// <param name="distributorID"></param>
        /// <returns></returns>
        public static bool ExpireInvoicesCache(string distributorID)
        {
            try
            {
                string cacheKey = INVOICE_CACHE_KEY_PREFIX + distributorID;
                SimpleCache.Expire(typeof(GetInvoicesResponse_V01), cacheKey);
                return true;
            }
            catch (Exception ex)
            {
                WebUtilities.LogExceptionWithContext(ex);

                LoggerHelper.Exception("System.Exception", new Exception(
                                                               "OrderService: Fail to clear GetInvoices cache for DistributorID: " + distributorID + " \n", ex));
            }
            return false;
        }

        public static byte[] GenerateInvoicePdf(string htmlContent, string licenseKey)
        {
            var pdfConverter = new PdfConverter();
            pdfConverter.LicenseKey = licenseKey;
            pdfConverter.InternetSecurityZone = InternetSecurityZone.LocalMachine;
            var pdfBytes = pdfConverter.GetPdfBytesFromHtmlString(htmlContent);
            return pdfBytes;
        }
    }
}