using System;
using System.Text;
using System.Web;
using System.Xml.Linq;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Providers.Installments;
using MyHerbalife3.Shared.Infrastructure.Extensions;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;

namespace MyHerbalife3.Ordering.Providers.Payments
{
    public class BankSlipPaymentGatewayInvoker : PaymentGatewayInvoker
    {
        private BankSlipPaymentGatewayInvoker(string paymentMethod, decimal amount)
            : base("BankSlipPaymentGateway", paymentMethod, amount)
        {

        }


        public override void Submit()
        {
            String xmlrequest;
            try
            {

                string redirectUrl = _configHelper.GetConfigEntry("paymentGatewayUrl");
                string key = _configHelper.GetConfigEntry("paymentGatewayEncryptionKey");
                string password = _configHelper.GetConfigEntry("paymentGatewayPassword");
                string returnUrl = string.Concat(RootUrl, _config.PaymentGatewayReturnUrlApproved);

                xmlrequest = GenerateXmlRequest(returnUrl, key, password);

                if (RootUrl.Contains("local") || RootUrl.Contains("qa") || RootUrl.Contains("uat"))
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                }

                HttpContext.Current.Response.Clear();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@"<html><body onload='document.forms[""form""].submit()'>");
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", redirectUrl);
                sb.AppendFormat("<input type='hidden' name='CHAVE' value='{0}' >", key);
                sb.AppendFormat("<input type='hidden' name='SENHA' value='{0}' >", password);
                sb.AppendFormat("<input type='hidden' name='ORDEM' value='{0}' >", HttpUtility.HtmlEncode(xmlrequest)); //HttpContext.Current.Server.HtmlEncode(xmlrequest));
                sb.AppendFormat("</form></body></html>");


                string response = sb.ToString();

                PaymentGatewayInvoker.LogMessage(PaymentGatewayLogEntryType.Request, this.OrderNumber, this._distributorId, this._gatewayName, PaymentGatewayRecordStatusType.Unknown, response);

                // HttpContext.Current.Request.Url.AbsoluteUri.Replace("http://br.local.myherbalife.com/Ordering/PaymentGatewayManager.aspx", "http://br.local.myherbalife.com/Ordering/PaymentGatewayManager.aspx?Agency=itau");
                HttpContext.Current.Response.Write(response);
                HttpContext.Current.Response.End();

            }
            catch (Exception ex)
            {
               LoggerHelper.Error(string.Format("Bank Slip Payment Gateway Invoker exception for: order number {0}, DS ID {1} {2}", this.OrderNumber, this._distributorId, ex.Message));
            }
        }


        //Name:GenerateXmlRequest
        //Description:Generate the xml file for the BankSlip Request
        //
        private string GenerateXmlRequest(string returnUrl, string key, string password)
        {
            InstallmentConfiguration _installmentsConfiguration;

            #region "create Skeleton"


            decimal taxBrazil = 0.90m;

            MyHLShoppingCart myCart;
            SessionInfo sessionInfoMyCart = SessionInfo.GetSessionInfo(this._distributorId, this._locale);
            myCart = sessionInfoMyCart.ShoppingCart;
            if (myCart == null)
                myCart = ShoppingCartProvider.GetShoppingCart(this._distributorId, this._locale);

            // Create Skeleton
            XDocument xmlToSend = new XDocument();
            XDeclaration xDeclaration = new XDeclaration("1.0", "utf-8", "no");

            XNamespace aw = "http://wwww.w3.org/2001/XMLSchema-instance";
            XNamespace aw1 = "http://wwww.w3.org/2001/XMLSchema";
            XNamespace xn = "http://www2.superpag.com.br/Schemas";
            xmlToSend.Declaration = xDeclaration;

            XElement xmlRoot = new XElement(xn + "RequisicaoPagamento",
                 new XAttribute(XNamespace.Xmlns + "xsi", aw),
                 new XAttribute(XNamespace.Xmlns + "xsd", aw1));

            xmlToSend.Add(xmlRoot);

            XElement EstabelecimentoComercial_data = new XElement(xn + "EstabelecimentoComercial",
                 new XAttribute("ChaveAutenticacao", key),
                 new XAttribute("SenhaAutenticacao", password));



            XElement Pagamento_data = new XElement(xn + "OrdemPagamento",
                new XAttribute("Codigo", this.OrderNumber));


            EstabelecimentoComercial_data.Add(Pagamento_data);
            xmlRoot.Add(EstabelecimentoComercial_data);

            XElement Consumidor = new XElement(xn + "Consumidor");
            Pagamento_data.Add(Consumidor);

            XElement DetalhesPessoaisDoConsumidor = new XElement(xn + "DetalhesPessoaisDoConsumidor");
            Consumidor.Add(DetalhesPessoaisDoConsumidor);

            string DistributorName = DistributorProfileModelHelper.DistributorName(DistributorProfileModel);
            DetalhesPessoaisDoConsumidor.Add(new XElement(xn + "PessoaFisica", new XAttribute("Nome", null != DistributorName ? DistributorName : string.Empty)));

            XElement Emails = new XElement(xn + "Emails");
            Consumidor.Add(Emails);

            Emails.Add(new XElement(xn + "Email", new XAttribute("Endereco", null != myCart.EmailAddress ? myCart.EmailAddress : string.Empty)));


            XElement Telefones = new XElement(xn + "Telefones");
            Consumidor.Add(Telefones);

            Telefones.Add(new XElement(xn + "Telefone", new XAttribute("Tipo", "1"),
                                                        new XAttribute("CodigoPais", "55"),
                                                        new XAttribute("DDD", null != myCart.DeliveryInfo.Address.AreaCode ? myCart.DeliveryInfo.Address.AreaCode : string.Empty),
                                                        new XAttribute("Numero", null != myCart.DeliveryInfo.Address.Phone ? myCart.DeliveryInfo.Address.Phone : string.Empty)));

            XElement EnderecoCobranca = new XElement(xn + "EnderecoCobranca", "");
            Pagamento_data.Add(EnderecoCobranca);



            string address = string.Empty;
            if (myCart.DeliveryInfo.Address.Address.Line1 != null && myCart.DeliveryInfo.Address.Address.Line1.Length > 60)
            {
                address = myCart.DeliveryInfo.Address.Address.Line1.Substring(0, 60);
            }
            else
            {
                address = myCart.DeliveryInfo.Address.Address.Line1;

            }


            EnderecoCobranca.Add(new XElement(xn + "Endereco", new XAttribute("Logradouro", null != myCart.DeliveryInfo.Address.Address.Line1 ? address : string.Empty),
                                                          new XAttribute("Numero", ""),
                                                          new XAttribute("Cep", null != myCart.DeliveryInfo.Address.Address.PostalCode ? myCart.DeliveryInfo.Address.Address.PostalCode : string.Empty),
                                                          new XAttribute("Complemento", ""),
                                                          new XAttribute("Bairro", null != myCart.DeliveryInfo.Address.Address.Line2 ? myCart.DeliveryInfo.Address.Address.Line2 : string.Empty),
                                                          new XAttribute("Cidade", null != myCart.DeliveryInfo.Address.Address.City ? myCart.DeliveryInfo.Address.Address.City : string.Empty),
                                                          new XAttribute("UF", null != myCart.DeliveryInfo.Address.Address.StateProvinceTerritory ? myCart.DeliveryInfo.Address.Address.StateProvinceTerritory : string.Empty),
                                                          new XAttribute("Pais", "BRAZIL")));


            XElement EnderecoEntrega = new XElement(xn + "EnderecoEntrega", "");
            Pagamento_data.Add(EnderecoEntrega);


            EnderecoEntrega.Add(new XElement(xn + "Endereco", new XAttribute("Logradouro", null != myCart.DeliveryInfo.Address.Address.Line1 ? address : string.Empty),
                                                          new XAttribute("Numero", ""),
                                                          new XAttribute("Cep", null != myCart.DeliveryInfo.Address.Address.PostalCode ? myCart.DeliveryInfo.Address.Address.PostalCode : string.Empty),
                                                          new XAttribute("Complemento", ""),
                                                          new XAttribute("Bairro", null != myCart.DeliveryInfo.Address.Address.Line2 ? myCart.DeliveryInfo.Address.Address.Line2 : string.Empty),
                                                          new XAttribute("Cidade", null != myCart.DeliveryInfo.Address.Address.City ? myCart.DeliveryInfo.Address.Address.City : string.Empty),
                                                          new XAttribute("UF", null != myCart.DeliveryInfo.Address.Address.StateProvinceTerritory ? myCart.DeliveryInfo.Address.Address.StateProvinceTerritory : string.Empty),
                                                          new XAttribute("Pais", "BRAZIL")));

            #region Items

            XElement ItemsDaOrdem = new XElement(xn + "ItensDaOrdem");
            Pagamento_data.Add(ItemsDaOrdem);

            // Fill order items section


            foreach (var item in myCart.ShoppingCartItems)
            {
                ItemsDaOrdem.Add(new XElement(xn + "Item", new XAttribute("Codigo", item.ID),
                                                  new XAttribute("Valor", Math.Round((item.DiscountPrice / item.Quantity), 2)),
                                                  new XAttribute("Quantidade", item.Quantity),
                                                  new XAttribute("Descricao", item.Description)));

            }
            OrderTotals_V01 totals = myCart.Totals as OrderTotals_V01;
            //ItemsDaOrdem.Add(new XElement(xn + "Item", new XAttribute("Codigo", "01"),
            //                                      new XAttribute("Valor", myCart.Totals.DiscountedItemsTotal),
            //                                      new XAttribute("Quantidade", 1),
            //                                      new XAttribute("Descricao", "Desconto")));

            ItemsDaOrdem.Add(new XElement(xn + "Item", new XAttribute("Codigo", "02"),
                                      new XAttribute("Valor", null != totals ? totals.TaxAmount : 0),
                                      new XAttribute("Quantidade", 1),
                                      new XAttribute("Descricao", "Impostos")));

            Charge_V01 pHCharge = new Charge_V01();
            Charge_V01 freightCharge = new Charge_V01();
            if (null != totals && null != totals.ChargeList)
            {
                pHCharge = GetCharge(totals.ChargeList, ChargeTypes.PH);
                freightCharge = GetCharge(totals.ChargeList, ChargeTypes.FREIGHT);
            }

            ItemsDaOrdem.Add(new XElement(xn + "Item", new XAttribute("Codigo", "03"),
                                                  new XAttribute("Valor", (null != pHCharge ? pHCharge.Amount : 0) + (null != freightCharge ? freightCharge.Amount : 0)),
                                                  new XAttribute("Quantidade", 1),
                                                  new XAttribute("Descricao", "Manuseio")));

            ItemsDaOrdem.Add(new XElement(xn + "Item", new XAttribute("Codigo", "04"),
                                                  new XAttribute("Valor", taxBrazil),
                                                  new XAttribute("Quantidade", 1),
                                                  new XAttribute("Descricao", "Tarifa Bancaria")));


            #endregion



            XElement Pagamento = new XElement(xn + "Pagamento", new XAttribute("ValorTotal", (null != myCart.Totals ? _orderAmount : 0) + taxBrazil),
                                                           new XAttribute("Data", System.DateTime.Now.Date.ToString("yyyy-MM-dd")),
                                                           new XAttribute("Batch", "False"),
                                                           new XAttribute("Email", null != myCart.EmailAddress ? myCart.EmailAddress : string.Empty));
            Pagamento_data.Add(Pagamento);

            XElement DetalheDoMeioDePagamento = new XElement(xn + "DetalheDoMeioDePagamento", new XAttribute("MeioPagamento", "BLT"));
            Pagamento.Add(DetalheDoMeioDePagamento);

            DateTime currentDate;
            OrderMonth orderMonth = new OrderMonth(myCart.CountryCode);
            currentDate = orderMonth.CurrentOrderMonth;

            var orderType = "RSO";
            if (myCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO) orderType = "ETO";
            else if (myCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.APF) orderType = "APF";
            _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration(myCart.CountryCode, new DateTime(currentDate.Year, currentDate.Month, 1), orderType);

            if (_installmentsConfiguration == null)
            {
                DateTime today = DateTime.Today;
                currentDate = new DateTime(today.Year, today.Month, 1).AddMonths(1).AddDays(-1);
            }


            DetalheDoMeioDePagamento.Add(new XElement(xn + "Boleto", new XAttribute("Banco", "237"),
                                                            new XAttribute("DataVencimento", (_installmentsConfiguration == null) ? currentDate.ToString("yyyy-MM-dd") : _installmentsConfiguration.TicketDueDate.ToString("yyyy-MM-dd")),
                                                            new XAttribute("NossoNumero", this.OrderNumber.Remove(0, 2)),
                                                            new XAttribute("Instrucoes",
                                                                "Nao aceitar este boleto apos a data de vencimento # ** Tarifa Bancaria:R$ 0,90 # ** Valor da Nota Fiscal: " + _orderAmount + " ")));

            XElement Parcelamento = new XElement(xn + "Parcelamento");
            Pagamento.Add(Parcelamento);

            Parcelamento.Add(new XElement(xn + "Parcelas", new XAttribute("Quantidade", "1"),
                                                       new XAttribute("Juros", "00.00"),
                                                       new XAttribute("TipoJuros", "Aquirer"),
                                                       new XAttribute("FormaParcelamento", "Integral")));

            XElement Setup = new XElement(xn + "Setup", "");
            Pagamento_data.Add(Setup);

            Setup.Add(new XElement(xn + "PostRetorno", new XAttribute("Url", (string.Format("{0}?Agency=BankSlip", returnUrl)))));

            String xmlReadyToSend;
            xmlReadyToSend = xmlToSend.Declaration.ToString() + xmlToSend.Root.ToString();
            return xmlReadyToSend;

            #endregion

        }

        private Charge_V01 GetCharge(ChargeList chargeList, ChargeTypes type)
        {
            return chargeList.Find(delegate(Charge p) { return ((Charge_V01)p).ChargeType == type; }) as Charge_V01 ?? new Charge_V01(type, (decimal)0.0);
        }

    }
}