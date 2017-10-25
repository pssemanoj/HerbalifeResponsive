#region

using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Security;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.Providers.Payments;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using MyHerbalife3.Ordering.ServiceProvider.OrderChinaSvc;
using MyHerbalife3.Shared.ViewModel.Models;
using MyHerbalife3.Ordering.ServiceProvider.OrderSvc;
using Newtonsoft.Json;
using MyHerbalife3.Shared.Infrastructure.Extensions;

#endregion
namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    /// <summary>
    ///     Announcement controller
    /// </summary>
    [CustomResponseHeader]
    public class MobileQuickPayController : ApiController
    {
        internal IMobileQuickPayProvider _mobileQuickPayController;
        private readonly MobileQuoteHelper _mobileQuoteHelper; 

        /// <summary>
        ///     ctor
        /// </summary>
        public MobileQuickPayController()
        {
            _mobileQuickPayController = new MobileQuickPayProvider();
            _mobileQuoteHelper = new MobileQuoteHelper();
        }
            
          [HttpGet]
          [CamelCasingFilter]
          [UserTokenAuthenticate]
          [ActionName("GetBindedCards")]
          public MobileResponseWrapper GetBindedCards(string disID)
          {
            var result = new List<BindedCardswithBanklist>();
            var phoneNumber = DistributorOrderingProfileProvider.GetPhoneNumberForCN(disID).Trim();
            var cn99BillQuickPayProvider = new CN_99BillQuickPayProvider();
            var tins = DistributorOrderingProfileProvider.GetTinList(disID, true);
            var tin = tins.Find(t => t.ID == "CNID");

            PaymentInformation bindedCard = null;

            List<PaymentInformation> bindedCards = cn99BillQuickPayProvider.LoadStorableDataQuick(phoneNumber, disID);

            var usageDebit = BankUsage.UsedByQPDebit;
            var debitCards = BankInfoProvider.GetAvailableBanks(usageDebit);

            var usageCredit = BankUsage.UsedByQPCredit;
            var creditCards = BankInfoProvider.GetAvailableBanks(usageCredit);
            
            //QD
            if (debitCards != null && debitCards.Count > 0)
            {
                foreach (BankInformation item in debitCards)
                {
                    if (bindedCards != null && bindedCards.Count > 0)
                        bindedCard = (from a in bindedCards where a.CardType.Trim() == "QD" && a.Alias.Trim() == item.BankCode.Trim() select a).FirstOrDefault();
                    
                                var currentBindedCard = new BindedCardswithBanklist()
                                {
                                    BankCode = item.BankCode,
                                    BankName = item.BankName,
                                    CardType = "QD",
                                };

                    if (bindedCard != null && bindedCard.CardHolder != null)
                            {
                        currentBindedCard.BindedCard = new BindedCardViewModel();
                        currentBindedCard.BindedCard.CardHolderName = bindedCard.CardHolder.First;
                        currentBindedCard.BindedCard.CardNumber = bindedCard.CardNumber;
                            }

                                result.Add(currentBindedCard);
                    bindedCard = null;
                }
            }

            //QC
            if (creditCards != null && creditCards.Count > 0)
                        {
                foreach (BankInformation item in creditCards)
                            {
                    if (bindedCards != null && bindedCards.Count > 0)
                        bindedCard = (from a in bindedCards where a.CardType.Trim() == "QC" && a.Alias.Trim() == item.BankCode.Trim() select a).FirstOrDefault();

                                var currentBindedCard = new BindedCardswithBanklist()
                                {
                                    BankCode = item.BankCode,
                                    BankName = item.BankName,
                                    CardType = "QC",
                                };

                    if (bindedCard != null && bindedCard.CardHolder != null)
                                {
                        currentBindedCard.BindedCard = new BindedCardViewModel();
                        currentBindedCard.BindedCard.CardHolderName = bindedCard.CardHolder.First;
                        currentBindedCard.BindedCard.CardNumber = bindedCard.CardNumber;
            }

                            result.Add(currentBindedCard);
                    bindedCard = null;
                }
            }

            var response = new MobileResponseWrapper
              {
                  Data = new BindedCardswithBanklistResponseVewModel { BankList = result, PhoneNumber = phoneNumber, CNID = tin.IDType.Key.Trim() },
              };
              return response;
          }

          [HttpPost]
          [CamelCasingFilter]
          [UserTokenAuthenticate]
        [ActionName("MobilePin")]
          public OrderResponseViewModel RequestMobilePin(OrderRequestViewModel request)
          {
            string obj = JsonConvert.SerializeObject(request);
            List<ValidationErrorViewModel> errors = null;
              request.Data.OrderMemberId = string.IsNullOrEmpty(request.Data.OrderMemberId) && !string.IsNullOrEmpty(request.Data.CustomerId) ? request.Data.CustomerId : request.Data.OrderMemberId;
              var viewModel = request.Data;
              var shoppingcart = _mobileQuoteHelper.PriceCart(ref viewModel, ref errors);
              var quickPayresponseviewModel = new OrderResponseViewModel();
              if (null != errors && errors.Any())
              {
                quickPayresponseviewModel.ValidationErrors = errors;
                  return quickPayresponseviewModel;
              }
              var orderTotalsV01 = shoppingcart.Totals as ServiceProvider.OrderSvc.OrderTotals_V01;
              var orderTotalsV02 = shoppingcart.Totals as ServiceProvider.OrderSvc.OrderTotals_V02;
              decimal amount;
              var order = _mobileQuickPayController.Getorder(viewModel, ref errors, orderTotalsV02, orderTotalsV01,
                                                             out amount);
              var ordernumber = order.OrderID;
              if (string.IsNullOrWhiteSpace(ordernumber))
              {
                  ordernumber = _mobileQuickPayController.GetOrderNumber(amount, shoppingcart.CountryCode, shoppingcart.DistributorID);
              }

              //Order_V01 order, string ordernumber, MyHLShoppingCart cart
              var cn99BillQuickPayProvider = new CN_99BillQuickPayProvider();
            var hasrequestedmobilepin = cn99BillQuickPayProvider.RequestMobilePinForPurchase(ordernumber, order, shoppingcart, request.Data.MemberId);
              if (hasrequestedmobilepin)
              {
                  var quickPayPaymentViewModel = request.Data.Payments[0] as QuickPayPaymentViewModel;
                  if (quickPayPaymentViewModel != null)
                {
                    quickPayPaymentViewModel.StorablePAN = cn99BillQuickPayProvider.StorablePAN;
                      quickPayPaymentViewModel.Token = cn99BillQuickPayProvider.Token;
              }

              viewModel.OrderNumber = ordernumber;
            }

            if(!string.IsNullOrEmpty(cn99BillQuickPayProvider.LastErrorMessage))
            {
                if (errors == null)
                    errors = new List<ValidationErrorViewModel>();

                errors.Add(new ValidationErrorViewModel { Code = 126000, Reason = cn99BillQuickPayProvider.LastErrorMessage });
            }

              quickPayresponseviewModel.Data = viewModel;

            if (null != errors && errors.Any())
            {
                quickPayresponseviewModel.ValidationErrors = errors;
            }

            JObject json = JObject.Parse(obj);
            MobileActivityLogProvider.ActivityLog(json, quickPayresponseviewModel, request.Data.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        request.Data.Locale);

            return quickPayresponseviewModel;
          }
    }
}
