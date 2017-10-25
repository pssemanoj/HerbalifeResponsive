using System;
using System.ServiceModel;
using HL.Common.Configuration;
using HL.Common.Logging;
using System.Globalization;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers
{
    public static partial class ShoppingCartProvider
    {
        public static OrderTotals GetQuote(Order order, OrderTotals total, bool calcFreight)
        {
            
            var order_V01 = order as Order_V01;
            var request = new QuoteRequest_V02 { Order = order_V01, CalcFreight = calcFreight, Donation = (total as OrderTotals_V02) == null ? decimal.Zero : (total as OrderTotals_V02).Donation};
            var session = SessionInfo.GetSessionInfo(order.DistributorID, CultureInfo.CurrentCulture.Name);
            if (null != session)
            {
                SessionInfo.GetSessionInfo(order.DistributorID, CultureInfo.CurrentCulture.Name).HmsPricing = HLConfigManager.Configurations.CheckoutConfiguration.UseHMSCalc;
            }
            order.DistributorID = order.DistributorID.ToUpper();
            if (Settings.GetRequiredAppSetting("LogCatalogCN", "false").ToLower() == "true")
            {
                LogRequest(OrderCreationHelper.Serialize(request));
            }
            QuoteResponse result = null;
            var proxy = ServiceClientProvider.GetOrderServiceProxy();
            try
            {
                result = proxy.Quote(new QuoteRequest1(request)).QuoteResult;
                if (result.Status == ServiceResponseStatusType.Success && result.Totals != null)
                {
                    if (Settings.GetRequiredAppSetting("LogCatalogCN", "false").ToLower() == "true")
                    {
                        LogRequest(OrderCreationHelper.Serialize(result));
                    }
                    return result.Totals;
                }
                else
                {
                    LoggerHelper.Error(
                        string.Format(
                            "Order Total error, distributor:{0} receive date:{1}, Calc Freight :{3},  error message:{2}",
                            order.DistributorID, order_V01.ReceivedDate.ToString(), order_V01.Messages, calcFreight));
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(
                    string.Format(
                        "Order Total error, distributor:{0} receive date:{1}, Calc Freight:{3},  error message:{2}",
                        order.DistributorID, order_V01.ReceivedDate.ToString(), ex, calcFreight));
            }
            return null;
        }

        public static void LogRequest(string logData)
        {
            Logger.SetLogWriter(new LogWriterFactory().Create(), false);
            LogEntry entry = new LogEntry();
            entry.Message = logData;
            Logger.Write(entry, "PricingRequestResponse");
        }
    }
}
