using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using HL.Common.Utilities;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.ViewModel.Model;
using MyHerbalife3.Shared.ViewModel.Models;
using LoggerHelper = HL.Common.Logging.LoggerHelper;
using HL.PGH.Contracts.ValueObjects;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.CommunicationSvc;
using Order = MyHerbalife3.Ordering.ServiceProvider.SubmitOrderBTSvc.Order;
using PaymentGatewayRecordStatusType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayRecordStatusType;
using PaymentGatewayLogEntryType = MyHerbalife3.Ordering.ServiceProvider.OrderSvc.PaymentGatewayLogEntryType;

namespace MyHerbalife3.Ordering.Providers.Communication
{
    public class CommunicationSvcProvider
    {

        #region contants

        private const string EmailConfirmationType = "OrderConfirmationToDS";
        private const string EmailDeclinedType = "PaymentDeclinedEmail";
        private const string EmailInProcessingType = "OrderInProcessing";

        #endregion

        public static bool SendInvoiceEmail(InvoiceModel invoice)
        {
            var proxy = ServiceClientProvider.GetCommunicationServiceProxy();
            try
            {
                if (null != proxy)
                {
                    var dataToSend = ConvertInvoiceToSendData(invoice);
                    var request = new TriggeredSendRequestRequest_V01();
                    request.Data = dataToSend;
                    var response = proxy.SendTriggeredMessage(new SendTriggeredMessageRequest(request)).SendTriggeredMessageResult;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                    else
                    {
                        LoggerHelper.Error(string.Format("CommunicationSvcProvider: Error sending Invoice Email invoice: {0}, member id: {1} status:{2}.", invoice.DisplayMemberInvoiceNumber, invoice.MemberId, response.Status));
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("CommunicationSvcProvider: Error sending Invoice Email invoice: {0}, member id: {1} Exception Message:{2}.", invoice.DisplayMemberInvoiceNumber, invoice.MemberId, ex.Message));
                return false;
            }
        }

        private static GdoOrderEmailSendData_V01 ConvertInvoiceToSendData(InvoiceModel invoice)
        {
            var locale = CultureInfo.CurrentCulture.Name;
            var uniqueNumnber = DateTime.Now.ToString().Replace("-", string.Empty).Replace(":", string.Empty).
                Replace("AM", string.Empty).Replace("/", string.Empty).Replace(" ", string.Empty).Replace("PM", string.Empty);
            GdoOrderEmailSendData_V01 orderData = null;
            orderData = new GdoOrderEmailSendData_V01();

            orderData.EmailType = "InvoiceConfirmation";
            orderData.OrderId = !HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? invoice.DisplayMemberInvoiceNumber : uniqueNumnber + invoice.DisplayMemberInvoiceNumber;
            orderData.PaymentType = GdoPaymentTypes.CreditCard;
            orderData.BillingAddress = GetEmailAddress(invoice.MemberAddress);
            orderData.BillingAddress.Line1 = !HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice
                ? invoice.MemberPhoneNumber
                : orderData.BillingAddress.Line1;
            orderData.DataKey = uniqueNumnber + invoice.DisplayMemberInvoiceNumber;
            orderData.DeliveryTimeEstimated = string.Empty;
            orderData.Distributor = new Distributor
            {
                Contact =
                    new ContactInfo
                    {
                        Email = invoice.OtherEmail?? invoice.Email ?? string.Empty,
                        PhoneNumber = invoice.Phone ?? string.Empty
                    },
                DistributorId = invoice.MemberId ?? string.Empty,
                FirstName = invoice.FirstName ?? string.Empty,
                LastName = invoice.LastName ?? string.Empty,
                Locale = locale,
                MiddleName = string.Empty,
            };
            orderData.Shipment = new Shipment
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                MiddleName = string.Empty,
                PickupName = string.Empty,
                ShippingDate = DateTime.Now,
                ShippingMethod = invoice.ShippingMethod ?? string.Empty
            };
           
            orderData.ShippingAddress = GetEmailAddress(invoice.Address);
            orderData.SpecialInstructions = invoice.Notes ?? string.Empty;
            orderData.Payments = GetEmailPayments(invoice).ToArray();
            orderData.GrandTotal = invoice.InvoicePrice.TotalDue;
            orderData.GrandTotalSpecified = true;
            orderData.OrderMonth = invoice.DisplayMemberInvoiceNumber;
            orderData.OrderSubmittedDate = invoice.InvoiceDate;
            orderData.OrderSubmittedDateSpecified = true;
            orderData.Tax = invoice.InvoicePrice.CalcTaxAmount;
            orderData.TaxSpecified = true;
            orderData.Logistics = Convert.ToDecimal(invoice.InvoicePrice.ShippingAmount);
            orderData.LogisticsSpecified = true;
            orderData.ICMS = Convert.ToDecimal(invoice.InvoicePrice.MemberTax);
            orderData.ICMSSpecified = true;
            orderData.IPI = Convert.ToDecimal(0);
            orderData.IPISpecified = true;
            orderData.TotalCollateralRetail = Convert.ToDecimal(invoice.InvoicePrice.TotalYourPrice);
            orderData.TotalCollateralRetailSpecified = true;
            orderData.TotalDiscountAmount = Convert.ToDecimal(invoice.InvoicePrice.CalcDiscountAmount);
            orderData.TotalDiscountAmountSpecified = true;
            orderData.TotalDiscountPercentage = Convert.ToDecimal(invoice.InvoicePrice.DiscountPercentage);
            orderData.TotalDiscountPercentageSpecified = true;
            orderData.PickupLocation = string.Empty;
            orderData.PickupTime = string.Empty;
            orderData.TotalDiscountRetail = Convert.ToDecimal(invoice.InvoicePrice.SubTotal);
            orderData.TotalDiscountRetailSpecified = true;
            orderData.TotalEarnBase = Convert.ToDecimal(invoice.InvoicePrice.TotalEarnBase); // calculated value
            orderData.TotalEarnBaseSpecified = true;
            orderData.TotalPackagingHandling = invoice.InvoicePrice.ShippingAmount;
            orderData.TotalPackagingHandlingSpecified = true;
            orderData.TotalProductRetail = Convert.ToDecimal(invoice.InvoicePrice.Profit);
            orderData.TotalProductRetailSpecified = true;
            orderData.TotalRetail = Convert.ToDecimal(invoice.InvoicePrice.MemberTotal);
            orderData.TotalRetailSpecified = true;
            orderData.TotalPromotionalRetail = Convert.ToDecimal(0);
            orderData.TotalPromotionalRetailSpecified = true;
            orderData.TotalVolumePoints = invoice.InvoicePrice.TotalVolumePoints;
            orderData.TotalVolumePointsSpecified = true;
            orderData.VolumePointsRate = string.Empty;
            orderData.ShippingHandling = HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? invoice.InvoicePrice.ShippingAmount : invoice.InvoicePrice.CalcShippingAmount;
            orderData.ShippingHandlingSpecified = true;
            orderData.SubTotal = Convert.ToDecimal(invoice.InvoicePrice.SubTotal);
            orderData.SubTotalSpecified = true;
            orderData.InvoiceOption = invoice.DisplayInvoiceType;
            orderData.MarketingFund = Convert.ToDecimal(invoice.InvoicePrice.MemberFreight);
            orderData.MarketingFundSpecified = true;
            orderData.PaymentOption = HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? "" : invoice.DisplayPaymentType;
            orderData.LocalTaxCharge = Convert.ToDecimal(invoice.InvoicePrice.TaxAmount);
            orderData.LocalTaxChargeSpecified = true;
            orderData.TaxedNet = Convert.ToDecimal(0);
            orderData.TaxedNetSpecified = true;
            orderData.PurchaseType = !HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? invoice.DisplayReceiptChannel : invoice.Type ?? string.Empty;
            orderData.HFFMessage = invoice.DisplayInvoiceStatus;
            orderData.Locale = locale;
            orderData.Messages = GetMessages(invoice).ToArray();
            orderData.OrderLines = GetEmailSkuLines(invoice, uniqueNumnber).ToArray();

            return orderData;
        }


        private static List<Message> GetMessages(InvoiceModel invoice)
        {
            var messages = new List<Message>();
            var message = new Message();
            switch (invoice.ApplicationCountryCode)
            {
                case "GB":
                    //for UK add the VAT number into aditional information message
                    message.MessageType = "aditionalInformation";
                    message.MessageValue = !string.IsNullOrEmpty(invoice.Vat) ? invoice.Vat : "Non-VAT Registered";
                    messages.Add(message);
                    break;
                case "KR":
                    //for UK add the VAT number into aditional information message
                    message.MessageType = "aditionalInformation";
                    message.MessageValue = !string.IsNullOrEmpty(invoice.Vat) ? invoice.Vat : "";
                    messages.Add(message);
                    break;
                case "CA":
                case "TT":
                case "JM":
                    if (!string.IsNullOrEmpty(invoice.NotificationURL))
                    {
                        message = new Message
                        {
                            MessageType = "aditionalInformation",
                            MessageValue = invoice.NotificationURL,

                        };
                        messages.Add(message);
                    }
                    break;

            }
            messages.Add(new Message
            {
                MessageType = "billingEmail",
                MessageValue = !string.IsNullOrEmpty(invoice.MemberEmailAddress) ? invoice.MemberEmailAddress : ""
            });
            var billingName = string.Format("{0}{1}", !string.IsNullOrEmpty(invoice.MemberFirstName) ? invoice.MemberFirstName + " " : "", !string.IsNullOrEmpty(invoice.MemberLastName) ? invoice.MemberLastName : "").Trim();
            messages.Add(new Message
            {
                MessageType = "billingName",
                MessageValue = billingName
            });
            return messages;
        }

        private static List<TriggeredGDOOrderConfirmationOrderLine> GetEmailSkuLines(InvoiceModel invoice, string uniqueNumber)
        {
            var orderSkus = new List<TriggeredGDOOrderConfirmationOrderLine>();
            if (HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ||  invoice.DisplayReceiptChannel != "Club Visit/Sale")
            {
                foreach (var sku in invoice.InvoiceLines)
                {
                    var toAdd = new TriggeredGDOOrderConfirmationOrderLine_V01()
                    {
                        OrderID = HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? invoice.DisplayMemberInvoiceNumber + uniqueNumber : invoice.DisplayMemberInvoiceNumber,
                        DistributorCost = Convert.ToDecimal(sku.TotalRetailPrice),
                        DistributorCostSpecified = true,
                        EarnBase = Convert.ToDecimal(sku.EarnBase),
                        EarnBaseSpecified = true,
                        Flavor = sku.ProductType ?? string.Empty,
                        ItemDescription = sku.ProductName ?? string.Empty,
                        LineTotal = HLConfigManager.Configurations.DOConfiguration.AddrerssVelidationInvoice ? Convert.ToDecimal(sku.TotalEarnBase) : Convert.ToDecimal(sku.TotalRetailPrice),
                        LineTotalSpecified = true,
                        PriceWithCharges = Convert.ToDecimal(sku.YourPrice),
                        PriceWithChargesSpecified = true,
                        Quantity = sku.Quantity,
                        QuantitySpecified = true,
                        SkuId = sku.Sku,
                        UnitPriceSpecified = true,
                        UnitPrice = Convert.ToDecimal(sku.RetailPrice),
                        VolumePoints = Convert.ToDecimal(sku.VolumePoint),
                        VolumePointsSpecified = true,
                    };
                    orderSkus.Add(toAdd);
                }
            }
            else
            {
                var toAdd = new TriggeredGDOOrderConfirmationOrderLine_V01()
                {
                    OrderID = invoice.DisplayMemberInvoiceNumber,
           
                ItemDescription = invoice.ClubInvoice.ClubRecieptProductName,
                    Quantity = Convert.ToInt32(invoice.ClubInvoice.ClubRecieptQuantity),
                    QuantitySpecified = true,
                    LineTotal = decimal.Parse(invoice.ClubInvoice.ClubRecieptDisplayTotalDue, CultureInfo.InvariantCulture),// Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptDisplayTotalDue),
                    LineTotalSpecified = true,
                    VolumePoints = decimal.Parse(invoice.ClubInvoice.ClubRecieptTotalVolumePoints, CultureInfo.InvariantCulture),// Convert.ToDecimal(invoice.ClubInvoice.ClubRecieptTotalVolumePoints),
                    VolumePointsSpecified = true
                };

                orderSkus.Add(toAdd);
            }
            return orderSkus;
        }

        private static List<PaymentInfo> GetEmailPayments(InvoiceModel invoice)
        {
            var listPayment = new List<PaymentInfo>();
            var newpayment = new PaymentInfo
            {
                Authorization = string.Empty,
                Amount = 0,
                BankName = string.Empty,
                CardNumber = string.Empty,
                CardType = string.Empty,
                ExpirationDate = string.Empty,
                Installments = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                PaymentCode = string.Empty,
                TransactionType = string.Empty
            };
            listPayment.Add(newpayment);
            return listPayment;
        }

        private static GenericAddress GetEmailAddress(InvoiceAddressModel address)
        {
            var genericAddress = address != null
                ? new GenericAddress
                {
                    City = address.City ?? string.Empty,
                    Country = address.Country ?? string.Empty,
                    CountyDistrict = address.County ?? string.Empty,
                    Line1 = address.Address1 ?? string.Empty,
                    Line2 = address.Address2 ?? string.Empty,
                    Line3 = string.Empty,
                    Line4 = string.Empty,
                    State = address.State ?? string.Empty,
                    Zip = address.PostalCode ?? string.Empty
                }
                : new GenericAddress
                {
                    City = string.Empty,
                    Country = string.Empty,
                    CountyDistrict = string.Empty,
                    Line1 = string.Empty,
                    Line2 = string.Empty,
                    Line3 = string.Empty,
                    Line4 = string.Empty,
                    State = string.Empty,
                    Zip = string.Empty
                };
            return genericAddress;
        }

        public bool SendEmailConfirmation(string orderNumber, string paymentType)
        {
            var proxy = ServiceClientProvider.GetCommunicationServiceProxy();
            try
            {
                var holder = OrderProvider.GetPaymentGatewayOrder(orderNumber);
                if (holder != null)
                {
                    GdoOrderEmailSendData_V01 dataToSend = new GdoOrderEmailSendData_V01();
                    switch (paymentType)
                    {
                        case "Abandoned":
                        case "PaymentDeclined":
                        case "PaymentDeclinedOldOrder":
                        case "PaymentDeclinedMaxTries":
                            dataToSend = getDataDeclinedEmailConfirmation(holder.BTOrder, EmailDeclinedType, orderNumber);
                            break;
                        case "OrderSubmitted":
                            dataToSend = getDataEmailConfirmation(holder.BTOrder, EmailConfirmationType, orderNumber);
                            break;
                        case "Processing":
                            dataToSend = getDataInProcessingEmailConfirmation(holder.BTOrder, EmailInProcessingType, orderNumber);
                            break;
                    }

                    var request = new TriggeredSendRequestRequest_V01();
                    request.Data = dataToSend;
                    var response = proxy.SendTriggeredMessage(new SendTriggeredMessageRequest(request)).SendTriggeredMessageResult;
                    if (response.Status == ServiceResponseStatusType.Success)
                    {
                        return true;
                    }
                    else
                    {
                        var ex = new ApplicationException(string.Format("CommunicationSvcProvider: Error sending SendTriggeredMessage order:{0}.", orderNumber));
                        WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                ex = new ApplicationException(string.Format("CommunicationSvcProvider: Error sending SendTriggeredMessage order:{0}.", orderNumber), ex);
                WebUtilities.LogServiceExceptionWithContext(ex, proxy);
                return false;
            }
        }

        private List<TriggeredGDOOrderConfirmationOrderLine> GetOrderSkus(Order order, string orderNumber)
        {
            var orderSkus = new List<TriggeredGDOOrderConfirmationOrderLine>();
            foreach (var sku in order.OrderItems)
            {
                var toAdd = new TriggeredGDOOrderConfirmationOrderLine_V01()
                {
                    OrderID = orderNumber, //order.OrderID,
                    DistributorCost = Convert.ToDecimal(sku.DistributorCost),
                    EarnBase = Convert.ToDecimal(sku.EarnBase),
                    Flavor = sku.Flavor ?? string.Empty,
                    ItemDescription = sku.ItemDescription ?? string.Empty,
                    LineTotal = Convert.ToDecimal(sku.LineTotal),
                    PriceWithCharges = Convert.ToDecimal(sku.PriceWithCharges),
                    Quantity = sku.Quantity,
                    SkuId = sku.SKU,
                    UnitPrice = Convert.ToDecimal(sku.UnitPrice),
                    VolumePoints = Convert.ToDecimal(sku.VolumePoints)
                };
                orderSkus.Add(toAdd);
            }
            return orderSkus;
        }

        private List<PaymentInfo> GetOrderPayments(Order order, string orderNumber)
        {
            List<PaymentInfo> listPayment = new List<PaymentInfo>();
            foreach (var payment in order.Payments)
            {
                var newpayment = new PaymentInfo
                {
                    Authorization = payment.AuthNumber ?? string.Empty,
                    Amount = payment.Amount,
                    BankName = payment.BankName ?? string.Empty,
                    CardNumber = payment.AccountNumber ?? string.Empty,
                    CardType = payment.PaymentCode ?? string.Empty,
                    ExpirationDate = payment.Expiration.ToString(),
                    Installments = payment.NumberOfInstallments.ToString(),
                    FirstName = payment.NameOnAccount ?? string.Empty,
                    LastName = string.Empty,
                    PaymentCode = payment.PaymentCode ?? string.Empty,
                    TransactionType = payment.TransactionType ?? string.Empty
                };
                listPayment.Add(newpayment);
            }
            return listPayment;
        }

        private List<Message> GetOrderMessages(Order order, string orderNumber)
        {
            var listMsg = new List<Message>();
            ResponseContext _responseContext;
            if (null != order.Messages)
            {
                listMsg.AddRange(order.Messages.Select(msg => new Message()
                {
                    MessageType = msg.MessageType,
                    MessageValue = msg.MessageValue
                }));
            }

            if (order.Locale == "sr-RS")
            {
                try
                {
                    string paymentGatewayLogs = string.Empty;
                    StringBuilder text = new StringBuilder();
                    PaymentGatewayRecordStatusType orderStatus = OrderProvider.GetPaymentGatewayRecordStatus(orderNumber);
                    if (orderStatus == PaymentGatewayRecordStatusType.Declined)
                    {
                        paymentGatewayLogs = OrderProvider.GetPaymentGatewayLog(orderNumber, PaymentGatewayLogEntryType.Response).Where(l => l.Contains("Error:=") || l.Contains("result:=") || l.Contains("Error=") || l.Contains("result=")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(paymentGatewayLogs))
                        {
                            _responseContext = new ResponseContext(paymentGatewayLogs);
                            if (_responseContext.PostedValues.AllKeys.Contains("Error"))
                            {
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["paymentid"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "PaymentCode").ToString()) + " " + _responseContext.PostedValues["paymentid"] + "|");
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionCode").ToString()) + " |");
                                }
                            }
                            else
                            if (_responseContext.PostedValues.AllKeys.Contains("result"))
                            {
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["paymentid"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "PaymentCode").ToString()) + " " + _responseContext.PostedValues["paymentid"] + "|");
                                }
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["tranid"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionCode").ToString()) + " " + _responseContext.PostedValues["tranid"] + "|");
                                }
                            }
                            listMsg.Add(new Message { MessageType = "aditionalInformation", MessageValue = text.ToString() });
                        }
                    }
                    else
                    {
                        paymentGatewayLogs = OrderProvider.GetPaymentGatewayLog(orderNumber, PaymentGatewayLogEntryType.Response).Where(l => l.Contains("QueryString: Agency=NestPay") || l.Contains("QueryString: Agency:=NestPay")).FirstOrDefault();
                        if (!string.IsNullOrEmpty(paymentGatewayLogs))
                        {
                            _responseContext = new ResponseContext(paymentGatewayLogs);
                            if (_responseContext.PostedValues.AllKeys.Contains("oid"))
                            {
                                // order ID
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["oid"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "OrderIdForMail").ToString()) + " " + _responseContext.PostedValues["oid"] + "|");
                                }

                                // Authorization Code
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["AuthCode"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "AuthCode").ToString()) + " " + _responseContext.PostedValues["AuthCode"] + "|");
                                }

                                // Payment Status
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["Response"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "PaymentStatus").ToString()) + " " + _responseContext.PostedValues["Response"] + "|");
                                }

                                // Transaction Status Code
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["ProcReturnCode"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionStatusCode").ToString()) + " " + _responseContext.PostedValues["ProcReturnCode"] + "|");
                                }

                                // Transaction ID
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["TransId"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionId").ToString()) + " " + _responseContext.PostedValues["TransId"] + "|");
                                }

                                // Transaction Date
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["EXTRA.TRXDATE"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionDate").ToString()) + " " + _responseContext.PostedValues["EXTRA.TRXDATE"] + "|");
                                }

                                // Status code for the 3D transaction
                                if (!string.IsNullOrEmpty(_responseContext.PostedValues["mdStatus"]))
                                {
                                    text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "Transaction3DStatus").ToString()) + " " + _responseContext.PostedValues["mdStatus"] + "|");
                                }
                            }
                            listMsg.Add(new Message { MessageType = "aditionalInformation", MessageValue = text.ToString() });
                        }
                        else
                        {
                            paymentGatewayLogs = OrderProvider.GetPaymentGatewayLog(orderNumber, PaymentGatewayLogEntryType.Response).Where(l => l.Contains("result=CAPTURED") || l.Contains("result:=CAPTURED")).FirstOrDefault();
                            if (!string.IsNullOrEmpty(paymentGatewayLogs))
                            {
                                _responseContext = new ResponseContext(paymentGatewayLogs);
                                if (_responseContext.PostedValues.AllKeys.Contains("result"))
                                {
                                    if (!string.IsNullOrEmpty(_responseContext.PostedValues["paymentid"]))
                                    {
                                        text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "PaymentCode").ToString()) + " " + _responseContext.PostedValues["paymentid"] + "|");
                                    }
                                    if (!string.IsNullOrEmpty(_responseContext.PostedValues["tranid"]))
                                    {
                                        text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "TransactionCode").ToString()) + " " + _responseContext.PostedValues["tranid"] + "|");
                                    }
                                    if (!string.IsNullOrEmpty(_responseContext.PostedValues["auth"]))
                                    {
                                        text.Append(string.Format(HttpContext.GetGlobalResourceObject(string.Format("{0}_ErrorMessage", HLConfigManager.Platform), "AutorizationCode").ToString()) + " " + _responseContext.PostedValues["auth"] + "|");
                                    }
                                }
                                listMsg.Add(new Message { MessageType = "aditionalInformation", MessageValue = text.ToString() });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("CommunicationSvcProvider: Error getting payment information for Order {0}, error: {1} .", orderNumber, ex.Message));
                }
            }
            return listMsg;
        }

        private static NameValueCollection GetRequestVariables(string requestData)
        {
            NameValueCollection result = new NameValueCollection();
            List<string> items = new List<string>(requestData.Split(new char[] { ';' }));
            foreach (string item in items)
            {
                string[] elements = item.Split(new char[] { '=' });
                if (elements.Length == 2)
                {
                    result.Add(elements[0], elements[1]);
                }
            }

            return result;
        }

        private GdoPaymentTypes GetPaymentType(Order order)
        {
            var code = order.Payments.Select(x => x.PaymentCode).FirstOrDefault();
            //generic codes for CC
            if (code == "MC" || code == "AX" || code == "VI")
            {
                return GdoPaymentTypes.CreditCard;
            }
            return GdoPaymentTypes.Wire;
        }

        private GenericAddress GetAddress(Order order)
        {
            var address = order.Shipment.Address;
            var genericAddress = address != null
                ? new GenericAddress
                {
                    City = address.City ?? string.Empty,
                    Country = address.Country ?? string.Empty,
                    CountyDistrict = address.CountyDistrict ?? string.Empty,
                    Line1 = address.Line1 ?? string.Empty,
                    Line2 = address.Line2 ?? string.Empty,
                    Line3 = address.Line3 ?? string.Empty,
                    Line4 = address.Line4 ?? string.Empty,
                    State = address.StateProvinceTerritory ?? string.Empty,
                    Zip = address.PostalCode ?? string.Empty
                }
                : null;
            return genericAddress;
        }

        private GdoOrderEmailSendData_V01 getDataInProcessingEmailConfirmation(Order order, string emailType, string orderNumber)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            ////Get the order items
            var orderSkus = GetOrderSkus(order, orderNumber);
            GdoOrderEmailSendData_V01 orderData = null;
            orderData = new GdoOrderEmailSendData_V01()
            {
                EmailType = emailType,
                OrderId = orderNumber,// "0199",
                Locale = order.Locale,
                Distributor = new Distributor
                {
                    Contact =
                        new ContactInfo
                        {
                            Email = order.Email,
                            PhoneNumber = order.Shipment.Phone
                        },
                    DistributorId = order.DistributorID,
                    FirstName = member.Value.FirstName,
                    LastName = member.Value.LastName,
                    Locale = order.Locale,
                    MiddleName = member.Value.MiddleName
                },
                Payments = GetOrderPayments(order, orderNumber).ToArray(),
                OrderSubmittedDate = order.ReceivedDate,
                OrderSubmittedDateSpecified = true
            };
            orderData.OrderLines = orderSkus.ToArray();
            return orderData;
        }

        private GdoOrderEmailSendData_V01 getDataDeclinedEmailConfirmation(Order order, string emailType, string orderNumber)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            ////Get the order items

            GdoOrderEmailSendData_V01 orderData = null;
            orderData = new GdoOrderEmailSendData_V01()
            {
                //Dummy Data
                EmailType = emailType,
                OrderId = orderNumber,// "0199",
                Locale = order.Locale,
                Distributor = new Distributor
                {
                    Contact =
                        new ContactInfo
                        {
                            Email = order.Email,
                            PhoneNumber = order.Shipment.Phone
                        },
                    DistributorId = order.DistributorID,
                    FirstName = member.Value.FirstName,
                    LastName = member.Value.LastName,
                    Locale = order.Locale,
                    MiddleName = member.Value.MiddleName
                },
                OrderSubmittedDate = order.ReceivedDate,
                OrderSubmittedDateSpecified = true
            };
            orderData.Messages = this.GetOrderMessages(order, orderNumber).ToArray();
            return orderData;
        }

        private GdoOrderEmailSendData_V01 getDataEmailConfirmation(Order order, string emailType, string orderNumber)
        {
            var member = (MembershipUser<DistributorProfileModel>)Membership.GetUser();
            GdoOrderEmailSendData_V01 orderData = null;
            if (order.EmailInfo == null)
            {
                order.EmailInfo = new ServiceProvider.SubmitOrderBTSvc.EmailInfo();
            }
            orderData = new GdoOrderEmailSendData_V01()
            {
                EmailType = emailType,
                OrderId = orderNumber,
                PaymentType = GetPaymentType(order),
                BillingAddress = GetAddress(order),
                DataKey = orderNumber,
                DeliveryTimeEstimated = order.EmailInfo.DeliveryTimeEstimated ?? string.Empty,
                Distributor = new Distributor
                {
                    Contact =
                        new ContactInfo
                        {
                            Email = order.Email,
                            PhoneNumber = order.Shipment.Phone
                        },
                    DistributorId = order.DistributorID,
                    FirstName = member.Value.FirstName,
                    LastName = member.Value.LastName,
                    Locale = order.Locale,
                    MiddleName = member.Value.MiddleName,
                },
                Shipment = new Shipment
                {
                    FirstName = order.Shipment.Recipient,
                    LastName = "",
                    MiddleName = "",
                    PickupName = order.Shipment.Recipient,
                    ShippingDate = DateTime.Now,
                    ShippingMethod = order.Shipment.ShippingMethodID
                },
                ShippingAddress = GetAddress(order),
                SpecialInstructions = order.EmailInfo.SpecialInstructions ?? string.Empty,
                Payments = GetOrderPayments(order, orderNumber).ToArray(),
                GrandTotal = order.Pricing.AmountDue,
                OrderMonth = order.OrderMonth,
                OrderSubmittedDate = DateTime.Parse(DateUtils.GetCurrentLocalTime(order.Locale).ToString()),
                OrderSubmittedDateSpecified = true,
                Tax = order.Pricing.TaxAmount,
                Logistics = Convert.ToDecimal(order.Pricing.LogisticsCharge),
                ICMS = Convert.ToDecimal(order.Pricing.IcmsTax),
                IPI = Convert.ToDecimal(order.Pricing.IpiTax),
                TotalCollateralRetail = Convert.ToDecimal(order.Pricing.TotalCollateralRetail),
                TotalDiscountAmount = Convert.ToDecimal(order.Pricing.DiscountedItemsTotal),
                TotalDiscountPercentage = Convert.ToDecimal(order.DiscountPercentage),
                PickupLocation = order.EmailInfo.PickUpLocation,
                PickupTime = string.Empty,
                TotalDiscountRetail = Convert.ToDecimal(order.Pricing.TotalCollateralRetail),
                TotalEarnBase = Convert.ToDecimal("0"),// calculated value
                TotalPackagingHandling = order.Pricing.PHAmount,
                TotalProductRetail = Convert.ToDecimal(order.Pricing.TotalProductRetail),
                TotalRetail = Convert.ToDecimal(order.Pricing.TotalProductRetail),
                TotalPromotionalRetail = Convert.ToDecimal(order.Pricing.TotalPromotionalRetail),
                TotalVolumePoints = order.Pricing.VolumePoints,
                VolumePointsRate = order.Pricing.VolumePointsRate ?? string.Empty,
                ShippingHandling = order.Pricing.PHAmount,
                InvoiceOption = order.EmailInfo.InvoiceOption,
                MarketingFund = Convert.ToDecimal(order.Pricing.TotalMarketingFund),
                PaymentOption = order.EmailInfo.PaymentOption,
                LocalTaxCharge = Convert.ToDecimal(order.Pricing.LocalTaxCharge),
                TaxedNet = Convert.ToDecimal(order.Pricing.TaxedNet),
                PurchaseType = order.EmailInfo.PurchaseType ?? string.Empty,// HttpContext.GetGlobalResourceObject(string.Format("{0}_Rules", HLConfigManager.Platform),order.PurchaseCategory.ToString(), CultureInfo.CurrentCulture)as string
                HFFMessage = order.EmailInfo.HffMessage ?? string.Empty,
                Locale = order.Locale,
                Messages = GetOrderMessages(order, orderNumber).ToArray()
            };
            orderData.OrderLines = GetOrderSkus(order, orderNumber).ToArray();
            return orderData;
        }
    }
}
