using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.EventHandling;
using MyHerbalife3.Ordering.Providers.Installments;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.SharedProviders.EventHandling;
using HL.PGH.Api;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ViewModel.Model;

namespace MyHerbalife3.Ordering.Web.Ordering.Controls.Payments.PaymentGateways
{
    public partial class PaymentInfo_es_AR : PaymentGatewayControl
    {
        private Cards _cards;
        private ProductsBase _page;
        private InstallmentConfiguration _installmentsConfiguration;
        private bool _enableInstallments;
        private PaymentsConfiguration _paymentsConfig;
        private List<CardInfo> installmentCards;

        [Publishes(MyHLEventTypes.PaymentInfoChanged)]
        public event EventHandler onPaymentInfoChanged;

        protected void Page_Load(object sender, EventArgs e)
        {
            _page = Page as ProductsBase;
            _paymentsConfig = HLConfigManager.Configurations.PaymentsConfiguration;
            _enableInstallments = _paymentsConfig.EnableInstallments;
            installmentCards = new List<CardInfo>();

            GetCardsList();

            if (_cards != null)
            {
                installmentCards = _cards.Card.FindAll(c => c.InstallmentFees.Count > 0);

            if (!IsPostBack)
            {
                    if (installmentCards.Count > 0)
                {
                        ddlCards.DataSource = installmentCards;
                    ddlCards.DataBind();
                    RecalcPage();
                        upInstallments.Update();
                        onPaymentInfoChanged(this, null);
                }
            }
        }
        }


        public override bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                GetPaymentInfo();
            }
            catch (Exception ex)
            {
                if (ddlCards.Items.Count == 0)
                {
                    errorMessage = "Missing CardInfo Data.";
                }
                else
                {
                    errorMessage = ex.Message;
                }
                return false;
            }
            return true;
        }


        public override HL.PGH.Api.PaymentRequest PaymentRequest
        {
            get
            {
                PaymentRequest request = base.PaymentRequest;               
                request.PayCode = !string.IsNullOrEmpty(ddlCards.SelectedValue) ? ddlCards.SelectedValue : "VI";              
                request.PaymentMethod = HL.PGH.Contracts.ValueObjects.PaymentMethodType.CreditCard;
                request.Installments = !string.IsNullOrEmpty(ddlInstallments.SelectedValue) ? ddlInstallments.SelectedValue : "1";
                return request;
            }
        }


        public override Payment GetPaymentInfo()
        {
            var payment = base.GetBasePaymentInfo() as CreditPayment_V01;
            string payCode = ddlCards.SelectedValue;
            if (!string.IsNullOrEmpty(payCode))
            {
                payment.Card.IssuerAssociation = CreditCard.GetCardType(payCode);
            }
            var options = new PaymentOptions_V01();
            options.NumberOfInstallments = Int32.Parse(ddlInstallments.SelectedValue);
            payment.PaymentOptions = options;
            payment.Card.IssuingBankID = _cards.Card.Find(c => c.CardId == ddlCards.SelectedValue).Id.ToString();

            Session.Remove(PaymentGatewayInvoker.PaymentInformation);
            Session.Add(PaymentGatewayInvoker.PaymentInformation, payment);

            return payment;
        }

        private void RecalcPage()
        {
            ResetFees();
            LoadInstallments();
            _page.ShoppingCart.Calculate();
        }


        private void ResetFees()
        {
            _page.ShoppingCart.DeleteItemsFromCart(new List<string> { _cards.FeeSKU });
        }

        /// <summary>
        ///     Set the number of installments allowed
        /// </summary>
        private void LoadInstallments()
        {
            ddlInstallments.Items.Clear();

            try
            {
            int maxInstallmentsForCard =
                    installmentCards.Find(c => c.CardId == ddlCards.SelectedValue).InstallmentFees.Max(i => i.InstallmentNumber);
            int maxAllowedInstallmentsPerStrategy = 0;

            int? maxFeesForInstallmentStrategy = new int();
            if (maxInstallmentsForCard == 0)
            {
                ddlInstallments.Items.Add(new ListItem("1", "1"));
            }
            else
            {
                int maxFeesPermitted = 0;
                switch (_cards.Strategy)
                {
                    case InstallmentStrategy.Price:
                        {
                            maxAllowedInstallmentsPerStrategy =
                               installmentCards.Find(c => c.CardId == ddlCards.SelectedValue)
                                      .PriceStrategy.Max(i => i.MaxInstallments);

                            if (_page.ShoppingCart.Totals != null)
                            {
                                var result = (from c in installmentCards
                                          from i in c.InstallmentFees
                                          where
                                              c.CardId == ddlCards.SelectedValue &&
                                              i.FeeRate <= (_page.ShoppingCart.Totals as OrderTotals_V01).AmountDue
                                          select i.InstallmentNumber);
                            if (result.Count() > 0)
                            {
                                maxFeesForInstallmentStrategy = result.Max();
                            }
                            }
                            break;
                        }
                    case InstallmentStrategy.Volume:
                        {
                            maxAllowedInstallmentsPerStrategy =
                               installmentCards.Find(c => c.CardId == ddlCards.SelectedValue)
                                      .VolumeStrategy.Max(i => i.MaxInstallments);

                            if (_page.ShoppingCart.Totals != null)
                            {
                                var result = (from c in installmentCards
                                          from v in c.VolumeStrategy
                                          where
                                              c.CardId == ddlCards.SelectedValue &&
                                              v.Volume <= (_page.ShoppingCart.Totals as OrderTotals_V01).VolumePoints
                                          select v.MaxInstallments);
                            if (result.Count() > 0)
                            {
                                maxFeesForInstallmentStrategy = result.Max();
                            }
                            }
                            break;
                        }
                    default:
                        {
                            string errorMessage =
                                string.Format(
                                    "Cannot find a valid Credit Card Installment Strategy to use. Cannot calculate fees for Payment Installments",
                                    _cards.Strategy);
                            LoggerHelper.Error(errorMessage);
                            throw new ApplicationException(errorMessage);
                        }
                }

                if (_page.IsEventTicketMode)
                {
                    maxAllowedInstallmentsPerStrategy = _cards.MaxEventTicketInstallments;
                }


                if (maxFeesForInstallmentStrategy.HasValue)
                {
                    maxFeesPermitted = maxAllowedInstallmentsPerStrategy >
                                       int.Parse(maxFeesForInstallmentStrategy.ToString())
                                           ? int.Parse(maxFeesForInstallmentStrategy.ToString())
                                           : maxAllowedInstallmentsPerStrategy;
                }
                else
                {
                    maxFeesPermitted = maxAllowedInstallmentsPerStrategy;
                }

                if (maxFeesPermitted == 0)
                {
                    maxFeesPermitted = 1;
                }

                for (int i = 1; i <= maxFeesPermitted; i++)
                {
                    ddlInstallments.Items.Add(new ListItem(i.ToString(), i.ToString()));
                }
            }
        }
            catch (Exception ex)
            {
                LoggerHelper.Error(string.Format("Exception: {0}", ex.Message));
            }
        }

        /// <summary>
        ///     determines the quantity of "aditional fee" units need to be added on the cart.
        /// </summary>
        /// <param name="skuFeeCuota"></param>
        /// <returns></returns>
        private int CalculateFees()
        {
            int feesDue = 0;
            bool chargeFees = false;
            switch (_cards.Strategy)
            {
                case InstallmentStrategy.Price:
                    {
                        if (_page.ShoppingCart.Totals != null)
                        {
                            var listPriceFees = (from c in installmentCards
                                             from i in c.PriceStrategy
                                             where
                                                 c.CardId == ddlCards.SelectedValue &&
                                                 i.PriceThreshold <= (_page.ShoppingCart.Totals as OrderTotals_V01).AmountDue
                                             select i).ToList();

                        if (null != listPriceFees && listPriceFees.Count > 0)
                        {
                            chargeFees =
                                listPriceFees.Find(p => p.PriceThreshold == listPriceFees.Max(f => f.PriceThreshold))
                                             .ChargeFee;
                        }
                        }
                        break;
                    }
                case InstallmentStrategy.Volume:
                    {
                        if (_page.ShoppingCart.Totals != null)
                        {
                            var listVolumeFees = (from c in installmentCards
                                              from i in c.VolumeStrategy
                                              where
                                                  c.CardId == ddlCards.SelectedValue &&
                                                  i.Volume <= (_page.ShoppingCart.Totals as OrderTotals_V01).VolumePoints
                                              select i).ToList();

                        if (null != listVolumeFees && listVolumeFees.Count > 0)
                        {
                            chargeFees =
                                listVolumeFees.Find(p => p.Volume == listVolumeFees.Max(f => f.Volume)).ChargeFee;
                        }
                        }
                        break;
                    }
                default:
                    {
                        string errorMessage =
                            string.Format(
                                "Cannot find a valid Credit Card Installment Strategy to use. Cannot calculate fees for Payment Installments",
                                _cards.Strategy);
                        LoggerHelper.Error(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }
            }
            if (chargeFees)
            {
                feesDue = GetFeeAmount();
            }

            return feesDue;
        }

        private int GetFeeAmount()
        {
            int installments = 0;
            decimal feePrice = 0.0M;
            decimal totalDue = 0M;
            if (int.TryParse(ddlInstallments.SelectedValue, out installments))
            {
                if (installments > 1)
                {
                    OrderTotals_V01 totals = _page.ShoppingCart.Totals as OrderTotals_V01;
                    SKU_V01 feeSku = null;
                    if (_page.AllSKUS.TryGetValue(_cards.FeeSKU, out feeSku))
                    {
                        feePrice = feeSku.CatalogItem.ListPrice;
                        var fees = new AdditionalFees(_cards);
                        decimal coefficient = fees.GetCoefficient(ddlCards.SelectedValue, installments);
                        if (totals.ItemsTotal > 0)
                        {
                            totalDue = (totals.AmountDue) * (coefficient);
                        }
                    }
                    else
                    {
                        string errorMessage =
                            string.Format(
                                "Cannot find Credit Card Installment SKU {0}. Cannot calculate fees for Payment Installments",
                                _cards.FeeSKU);
                        LoggerHelper.Error(errorMessage);
                        throw new ApplicationException(errorMessage);
                    }
                }
            }

            return (totalDue > 0) ? int.Parse(Math.Ceiling(totalDue / feePrice).ToString()) : 0;
        }

        private decimal TotalProductValue(bool chargeFee)
        {
            decimal total = 0;
            foreach (DistributorShoppingCartItem item in _page.ShoppingCart.ShoppingCartItems)
            {
                if (chargeFee)
                {
                    total += item.DiscountPrice * item.Quantity;
                }
            }
            return total;
        }

        private List<CardInfo> GetCardsList()
        {
            var result = new List<CardInfo>();
            FileStream f = null;
            try
            {
                string orderType = "RSO";
                MyHLShoppingCart shoppingCart = (Page as ProductsBase).ShoppingCart;

                if (_enableInstallments)
                {
                    if (shoppingCart.OrderCategory == ServiceProvider.CatalogSvc.OrderCategoryType.ETO)
                        orderType = "ETO";

                    if (APFDueProvider.containsOnlyAPFSku(shoppingCart.ShoppingCartItems))
                        orderType = "APF";

                    _installmentsConfiguration = InstallmentsProvider.GetInstallmentsConfiguration((Page as ProductsBase).CountryCode, DateTime.Today, orderType);

                    // Check the source for installments config
                    if (!_paymentsConfig.LocalInstallmentsSource)
                    {
                        _cards = _installmentsConfiguration.Cards;
                    }
                    else
                    {
                var serial = new XmlSerializer(typeof(Cards));
                //f = System.IO.File.OpenRead(Server.MapPath("~/Ordering/Controls/Payments/PaymentGateways/CardData_es-AR.xml"));
                f = File.OpenRead(Server.MapPath("~/App_Data/Configuration/CardData_es-AR.xml"));
                _cards = serial.Deserialize(f) as Cards;
            }
                }

                result = _cards != null ? _cards.Card : null;
            }
            catch (Exception ex)
            {
                //string errorMessage =
                //    "Cannot find Credit Card CardData_es-AR.xml. Cannot calculate fees for Payment Installments";
                LoggerHelper.Error(string.Format("\r\nException: {0}", ex.Message));
                //throw new ApplicationException(errorMessage, ex);
            }
            finally
            {
                if (null != f)
                {
                    f.Close();
                }
            }
            return result;
        }

        public class AdditionalFees
        {
            private readonly Cards _cards;
            public int Installments { get; set; }
            public decimal Rate { get; set; }

            public AdditionalFees(Cards cards)
            {
                _cards = cards;
            }

            public AdditionalFees()
            {
            }

            public AdditionalFees Get(string CardId, int installments)
            {
                AdditionalFees found = null;
                var result = (from c in _cards.Card
                              where c.CardId == CardId
                              from i in c.InstallmentFees
                              where i.InstallmentNumber == installments
                              select i);
                if (null != result && result.Count() > 0)
                {
                    found = new AdditionalFees();
                    found.Installments = result.FirstOrDefault().InstallmentNumber;
                    found.Rate = result.FirstOrDefault().FeeRate;
                }

                return found;
            }

            public decimal GetCoefficient(string CardId, int installments)
            {
                var result = (from c in _cards.Card
                              where c.CardId == CardId
                              from i in c.InstallmentFees
                              where i.InstallmentNumber == installments
                              select i.FeeRate);
                if (null != result && result.Count() > 0)
                {
                    return result.FirstOrDefault();
                }
                else
                {
                    return 1;
                }
            }

            public int GetInstallmentsForCard(string CardId)
            {
                var result =
                    (from c in _cards.Card
                     where c.CardId == CardId
                     from i in c.InstallmentFees
                     select i.InstallmentNumber);
                return result.Max();
            }

            public bool CardHasInstallmentFees(string CardId)
            {
                var result =
                    (from c in _cards.Card
                     where c.CardId == CardId
                     from i in c.InstallmentFees
                     select i.InstallmentNumber);
                return (result.ToList().Count() > 0);
            }
        }

        #region Client events

        public void ddlCards_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (installmentCards.Count > 0)
            {
            RecalcPage();
            }

            onPaymentInfoChanged(this, null);
        }

        public void ddlInstallments_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetFees();
            int fees = CalculateFees();
            if (fees > 0)
            {
                var items = new List<ShoppingCartItem_V01>();
                items.Add(new ShoppingCartItem_V01(0, _cards.FeeSKU, fees, DateTime.Now));
                _page.ShoppingCart.AddItemsToCart(items);
                if (_page.ShoppingCart.RuleResults.Any(r => r.Result == RulesResult.Failure))
                {
                    ddlInstallments.SelectedIndex = 0;
                    ShoppingCartRuleResult result =
                        _page.ShoppingCart.RuleResults.FindAll(r => r.Result == RulesResult.Failure).FirstOrDefault();
                    if (result.RuleName == "PurchasingLimits Rules")
                    {
                        lblError.Text =
                            GetLocalResourceObject("AdditionalInstallmentFeesCausePurchasingLimitOverage") as string;
                    }
                    else if (result.RuleName == "SkuLimitation Rules")
                    {
                        lblError.Text =
                            GetLocalResourceObject("AdditionalInstallmentFeesCauseLineItemLimitOverage") as string;
                    }
                    else
                    {
                        lblError.Text = result.Messages[0];
                    }
                }
                _page.ShoppingCart.RuleResults.Clear();
            }
            onPaymentInfoChanged(this, null);
            upInstallments.Update();
        }

        [SubscribesTo(MyHLEventTypes.PaymentOptionsViewChanged)]
        public void PaymentOptionsViewChanged(object sender, EventArgs e)
        {
            var rbl = TabControl as RadioButtonList;
            if (null != rbl)
            {
                if (rbl.SelectedIndex > 1)
                {
                    ResetFees();
                    _page.ShoppingCart.Calculate();

                    onPaymentInfoChanged(this, null);
                }
                else
                {
                    RecalcPage();
                }
            }
        }

        #endregion

        #region Card Info classes

        //public enum InstallmentStrategy
        //{
        //    Price,
        //    Volume
        //}

        //public class Cards
        //{
        //    [XmlElement]
        //    public InstallmentStrategy Strategy { get; set; }

        //    [XmlElement]
        //    public string FeeSKU { get; set; }

        //    [XmlElement]
        //    public int MaxEventTicketInstallments { get; set; }

        //    [XmlElement] public List<CardInfo> Card = new List<CardInfo>();
        //}

        //public class CardInfo
        //{
        //    [XmlElement]
        //    public string CardId { get; set; }

        //    [XmlElement]
        //    public string Name { get; set; }

        //    [XmlElement]
        //    public int Id { get; set; }

        //    [XmlElement]
        //    public List<InstallmentFee> InstallmentFees { get; set; }

        //    [XmlElement]
        //    public List<VolumeStrategy> VolumeStrategy { get; set; }

        //    [XmlElement]
        //    public List<PriceStrategy> PriceStrategy { get; set; }
        //}

        //public class InstallmentFee
        //{
        //    [XmlElement]
        //    public int InstallmentNumber { get; set; }

        //    [XmlElement]
        //    public decimal FeeRate { get; set; }
        //}

        //public class PriceStrategy
        //{
        //    [XmlElement]
        //    public decimal PriceThreshold { get; set; }

        //    [XmlElement]
        //    public int MaxInstallments { get; set; }

        //    [XmlElement]
        //    public bool ChargeFee { get; set; }
        //}

        //public class VolumeStrategy
        //{
        //    [XmlElement]
        //    public decimal Volume { get; set; }

        //    [XmlElement]
        //    public int MaxInstallments { get; set; }

        //    [XmlElement]
        //    public bool ChargeFee { get; set; }
        //}

        #endregion
    }
}