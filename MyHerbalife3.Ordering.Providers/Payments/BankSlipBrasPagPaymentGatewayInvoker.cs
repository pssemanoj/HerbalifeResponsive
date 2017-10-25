using System;
using System.Text;
using System.Web;
using HL.Common.Logging;
using HL.Common.ValueObjects;
using MyHerbalife3.Ordering.Providers.Installments;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using OrderCategoryType = MyHerbalife3.Ordering.ServiceProvider.CatalogSvc.OrderCategoryType;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BankSlipBrasPagPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private BankSlipBrasPagPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("BankSlipBrasPagPaymentGateway", paymentMethod, amount)
        {

        }
        public override void Submit()
        {           
            try
            {

                string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
                string id_Loja = _configHelper.GetConfigEntry("paymentGatewayIdLoja");
                string paymentGatewayTransactionType = _configHelper.GetConfigEntry("paymentGatewayTransactionType");
                string paymentGatewayWebServiceBankSlipCallEnable = _configHelper.GetConfigEntry("paymentGatewayWebServiceBankSlipCallEnable");
                string returnUrlApproved =  string.Concat(RootUrl,_configHelper.GetConfigEntry("paymentGatewayReturnUrlApproved"));
                string returnUrl = string.Format("{0}?Agency=BankSlipBrasPag&Order={1}&Status=1", returnUrlApproved, OrderNumber);

                string dataToEncrypt = string.Empty;
                string dataEcnrypted = string.Empty;
                dataToEncrypt=  BuildInfo(paymentGatewayTransactionType);
                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, dataToEncrypt);
                dataEcnrypted =  OrderProvider.SendBrasPagEncryptServiceRequest("1", id_Loja, dataToEncrypt);
                HttpContext.Current.Response.Clear();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"<html><body onload='document.forms[""form""].submit()'>");
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
                sb.AppendFormat("<input type='hidden' name='Id_Loja' value='{0}' >", id_Loja);
                sb.AppendFormat("<input type='hidden' name='crypt' value='{0}' >", dataEcnrypted);
                sb.AppendFormat("<input type='hidden' name='EXTRADYNAMICURL' value='{0}' >", returnUrl); 
                sb.AppendFormat("</form></body></html>");

                 
                string response = sb.ToString();
                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

              
                HttpContext.Current.Response.Write(response);
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
               LoggerHelper.Error(string.Format("Bank Slip Payment Gateway Invoker exception for: order number {0}, DS ID {1} {2}", this.OrderNumber, this._distributorId, ex.Message));
            }
        }

        
        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == type; }) as Charge_V01 ?? new Charge_V01(type, (decimal)0.0);
        }


        private string BuildInfo(string transactionType)
        { 
            decimal taxBrazil = 0.90m;            
            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);
            string DistributorName = DistributorProfileModelHelper.DistributorName(DistributorProfileModel);

            InstallmentConfiguration _installmentsConfiguration;
            DateTime currentDate;
            OrderMonth orderMonth = new OrderMonth(myCart.CountryCode);
            currentDate = orderMonth.CurrentOrderMonth;

            var orderType = "RSO";
            if (myCart.OrderCategory == OrderCategoryType.ETO) orderType = "ETO";
            else if (myCart.OrderCategory == OrderCategoryType.APF) orderType = "APF";
            _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration(myCart.CountryCode, new DateTime(currentDate.Year, currentDate.Month, 1), orderType);

            if (_installmentsConfiguration == null)
            {
                DateTime today = DateTime.Today;
                currentDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            }

            string paymentCode = _configHelper.GetConfigEntry("paymentCode");

            string data ;
            data = "VENDAID=" + this.OrderNumber + "@";
            data = data + "VALOR=" + ((null != myCart.Totals ? _orderAmount : 0) + taxBrazil) + "@";
            data = data + "NOME=" + (null != DistributorName ? DistributorName : string.Empty) + "@";
            data = data + "CODPAGAMENTO=" + paymentCode + "@";
            data = data + "LOGRADOURO=" + (null != myCart.DeliveryInfo.Address.Address.Line1 ? myCart.DeliveryInfo.Address.Address.Line1 : string.Empty) + "@";
            data = data + "BAIRRO=" + (null != myCart.DeliveryInfo.Address.Address.Line2 ? myCart.DeliveryInfo.Address.Address.Line2 : string.Empty) + "@";
            data = data + "CIDADE=" + (null != myCart.DeliveryInfo.Address.Address.City ? myCart.DeliveryInfo.Address.Address.City : string.Empty) + "@";
            data = data + "ESTADO=" + (null != myCart.DeliveryInfo.Address.Address.StateProvinceTerritory ? myCart.DeliveryInfo.Address.Address.StateProvinceTerritory : string.Empty) + "@";
            data = data + "CEP=" + (null != myCart.DeliveryInfo.Address.Address.PostalCode ? myCart.DeliveryInfo.Address.Address.PostalCode : string.Empty) + "@";
            data = data + "INSTRUCOES=" + "Nao aceitar este boleto apos a data de vencimento # ** Tarifa Bancaria:R$ 0,90 # ** Valor da Nota Fiscal: " + _orderAmount + "@";
            data = data + "EXTRA=" + (null != myCart.DistributorID ? myCart.DistributorID : string.Empty) + "@";
            data = data + "PAIS=BRAZIL@";
            data = data + "NOSSONUMERO=" + (null != this.OrderNumber ? this.OrderNumber.Remove(0, 2) : string.Empty) + "@";
            data = data + "DATAVENCIMENTO=" + ( _installmentsConfiguration == null ? currentDate.ToString("MM-dd-yyyy") : _installmentsConfiguration.TicketDueDate.ToString("yyyy-MM-dd") )+ "@";
            data = data + "TRANSACTIONCURRENCY=BRL@";
            data = data + "TRANSACTIONCOUNTRY=BRA@";
            data = data + "TRANSACTIONTYPE=" + transactionType;
           
            return data;
        }

    }
}

