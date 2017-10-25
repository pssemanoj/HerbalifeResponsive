#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web.Http;
using HL.Common.Configuration;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;
using MyHerbalife3.Ordering.ServiceProvider;
using MyHerbalife3.Ordering.ViewModel.Model;

#endregion

namespace MyHerbalife3.Ordering.Providers.Mobile
{
    public class MobileQuoteProvider : IMobileQuoteProvider
    {
        public const string CatalogServiceSettingKey = "IAGlobalOrderingCatalogUrl";

        private readonly IMobileDualOrderMonthProvider _dualOrderMonthProvider;
        private readonly MobileQuoteHelper _mobileQuoteHelper;

        public MobileQuoteProvider(IMobileDualOrderMonthProvider dualOrderMonthProvider, MobileQuoteHelper mobileQuoteHelper)
        {
            _dualOrderMonthProvider = dualOrderMonthProvider;
            _mobileQuoteHelper = mobileQuoteHelper;
        }

        public QuoteResponseViewModel Quote(OrderViewModel order, ref List<ValidationErrorViewModel> errors)
        {
            try
            {
                if (null == errors)
                {
                    errors = new List<ValidationErrorViewModel>();
                }
                if (ValidateOrderViewModel(order))
                {
                    if (_mobileQuoteHelper.CheckIfOrderCompleted(order.Id))
                    {
                        errors.Add(
                            new ValidationErrorViewModel
                            {
                                Code = 109999,
                                Reason = string.Format("order for guid {0} already completed", order.Id)
                            });
                        string countrydup = order.Locale.Substring(3, 2);
                        DualOrderMonthViewModel orderMonthDatadup = _dualOrderMonthProvider.GetDualOrderMonth(countrydup);
                        return new QuoteResponseViewModel
                        {
                            Order = order,
                            DualOrderMonth =
                                new DualOrderMonthViewModel
                                {
                                    PreviousOrderMonthEndDate = orderMonthDatadup != null ? orderMonthDatadup.PreviousOrderMonthEndDate : string.Empty,
                                    PreviousOrderMonth = orderMonthDatadup != null ? orderMonthDatadup.PreviousOrderMonth : 0,
                                },
                        };
                    }

                    var shoppingCart = _mobileQuoteHelper.PriceCart(ref order, ref errors, checkSkuAvalability:true, checkUnSupportedAddress:true);
                    if (shoppingCart != null)
                    {
                        var ruleErrors= CheckForError(shoppingCart.RuleResults);
                        if (null != ruleErrors && ruleErrors.Any())
                        {
                            errors.AddRange(ruleErrors);
                        }
                        if (errors != null && errors.Count > 1 && errors.Find(f => f.Code.Equals(110430)) != null)
                        {
                            errors.RemoveAll(e => e.Code.Equals(10416));
                        }
                        string country = order.Locale.Substring(3, 2);
                        DualOrderMonthViewModel orderMonthData = _dualOrderMonthProvider.GetDualOrderMonth(country);
                        var dualOrderMonth = new DualOrderMonthResponseViewModel
                        {
                            PreviousOrderMonth = orderMonthData != null? orderMonthData.PreviousOrderMonth :0,
                            PreviousOrderMonthEndDate = orderMonthData != null? orderMonthData.PreviousOrderMonthEndDate : string.Empty
                        };
                        var result = new QuoteResponseViewModel
                        {
                            Order = order,
                            DualOrderMonth =
                                new DualOrderMonthViewModel
                                {
                                    PreviousOrderMonthEndDate = dualOrderMonth.PreviousOrderMonthEndDate,
                                    PreviousOrderMonth = dualOrderMonth.PreviousOrderMonth
                                },
                        };
                        SaveMobileQuoteResponse(result, shoppingCart.ShoppingCartID);
                        return result;
                    }
                }
                errors.Add(
                    new ValidationErrorViewModel
                    {
                        Code = 100416,
                        Reason = "Quote failed"
                    });
                return new QuoteResponseViewModel
                {
                    Order = order,
                };
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("System.Exception", ex, "Exception in Quote method MobileQuoteProvider");
                errors.Add(
                    new ValidationErrorViewModel
                    {
                        Code = 100416,
                        Reason = "Quote failed"
                    });
                return new QuoteResponseViewModel
                {
                    Order = order,
                };
            }
        }

        //validating necessary nodes
        public bool ValidateOrderViewModel(OrderViewModel order)
        {
            //order.OrderItems == null ||    order.OrderItems.Count<=0 -- can be null, china donations does not have skus
            if (order.Shipping == null)
            {
                throw new Exception("Invalid or Incomplete Order information");
            }
            return true;
        }

        private static List<ValidationErrorViewModel> CheckForError(IEnumerable<ShoppingCartRuleResult> ruleResults)
        {
            if (null == ruleResults || !ruleResults.Any() || ruleResults.Count() == 0 )
            {
                return null;
            }

            IEnumerable<ShoppingCartRuleResult> rulesWithErrors = ruleResults.Where(
                r => r.Result == RulesResult.Errors || r.Result == RulesResult.Failure);
            if (null == rulesWithErrors || !rulesWithErrors.Any())
            {
                return null;
            }

            var validationErrors = new List<ValidationErrorViewModel>();
            foreach (ShoppingCartRuleResult rulesWithError in rulesWithErrors)
            {
                validationErrors.AddRange(
                    rulesWithError.Messages.Select(ConstructValidationError)
                        .Where(validationError => null != validationError));
            }
            return validationErrors;
        }

        private static ValidationErrorViewModel ConstructValidationError(string message)
        {
            var validationError = new ValidationErrorViewModel();
            if (!string.IsNullOrEmpty(message) && message.Contains("@"))
            {
                string[] errorParts = message.Split('@');
                int code = 0;

                if (errorParts.Length == 3)
                {
                    Int32.TryParse(errorParts[0] ?? "0", out code);
                    if (code == 1)
                    {
                        return null;
                    }
                    validationError.Code = code;
                    validationError.Reason = errorParts[1] ?? string.Empty;
                    validationError.Skus = new List<SkuErrorModel>{new SkuErrorModel{Sku = errorParts[2] ?? string.Empty, MaxQuantity = 0}};
                    return validationError;
                }
                if (errorParts.Length == 2)
                {
                    validationError.Code = 10416;
                    validationError.Reason = errorParts[0] ?? string.Empty;
                    validationError.Skus = null;
                    return validationError;
                }
            }
            validationError.Code = 10416;
            validationError.Reason = "Quote Failed";
            validationError.Skus = null;
            return validationError;
        }

        private static void SaveMobileQuoteResponse(QuoteResponseViewModel model, int shoppingCartId)
        {
            CatalogInterfaceClient proxy = null;
            using (proxy = ServiceClientProvider.GetCatalogServiceProxy())
            {
                try
                {
                    var request = new InsertMobileOrderDetailRequest_V01();

                    MobileOrderDetail mobileOrderDetail =
                        MobileOrderProvider.ModelConverter.ConvertOrderViewModelToMobileOrderDetail(model.Order,
                            shoppingCartId);
                    mobileOrderDetail.AddressId = model.Order.Shipping != null && model.Order.Shipping.Address != null
                        ? model.Order.Shipping.Address.CloudId
                        : Guid.Empty;
                    mobileOrderDetail.CustomerId = model.Order.CustomerId;
                    mobileOrderDetail.OrderStatus = "Quoted";

                    request.MobileOrderDetail = mobileOrderDetail;
                    InsertMobileOrderDetailResponse response = proxy.InsertMobileOrderDetail(new InsertMobileOrderDetailRequest1(request)).InsertMobileOrderDetailResult;
                    // Check response for error.
                    if (response == null || response.Status != ServiceResponseStatusType.Success)
                    {
                        LoggerHelper.Error(string.Format("SaveMobileQuoteResponse error Order: {0} Message: {1}",
                            model.Order.OrderNumber, response));
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.Error(string.Format("SaveMobileQuoteResponse Exception Order: {0} Message: {1}",
                        model.Order.OrderNumber, ex));
                }
            }
        }
    }
}