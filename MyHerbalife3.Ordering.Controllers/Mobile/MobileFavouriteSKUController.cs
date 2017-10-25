#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Interfaces;
using MyHerbalife3.Ordering.Providers;
using MyHerbalife3.Ordering.Providers.Mobile;
using MyHerbalife3.Ordering.ViewModel.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MyHerbalife3.Ordering.WebAPI.Attributes;
using MyHerbalife3.Ordering.ServiceProvider.CatalogSvc;

#endregion

namespace MyHerbalife3.Ordering.Controllers.Mobile
{
    [CustomResponseHeader]
    public class MobileFavouriteSKUController : ApiController
    {
        private readonly IMobileOrderProvider _iMobileOrderProvider;
        private readonly MobileFavouriteSKUProvider _mobileFavouriteProvider;
        private readonly MobileQuoteHelper _mobileQuoteHelper;

        public MobileFavouriteSKUController()
        {
            //_mobileQuoteHelper = new MobileQuoteHelper();
            //_iMobileOrderProvider = new MobileOrderProvider(_mobileQuoteHelper);
            _mobileFavouriteProvider = new MobileFavouriteSKUProvider();
        }

        /// <summary>
        ///     Gets the orders for the query
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public MobileResponseWrapper Get([FromUri] FavouriteSKUItemViewModel query)
        {
            try
            {
                List<FavouriteSKUItemResponseViewModel> result  = null;

                if (query == null)
                {
                    throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete GetFavouriteSKU information", 999999);
                }
                string obj = JsonConvert.SerializeObject(query);
                
                query.Locale = Thread.CurrentThread.CurrentCulture.Name != null ? Thread.CurrentThread.CurrentCulture.Name : null;

                if (string.IsNullOrEmpty(query.MemberId) || string.IsNullOrEmpty(query.Locale))
                {
                    return new MobileResponseWrapper { Data = new FavouriteSKUResponseViewModel { FavouriteSKUs = null, RecordCount = 0 }};
                }

                GetFavouriteParam request = new GetFavouriteParam { DistributorID = query.MemberId, Locale= query.Locale };

                result = _mobileFavouriteProvider.GetFavouriteSKUs(request);
                if (result != null)
                {
                    var response = new MobileResponseWrapper
                    {
                        Data =
                            new FavouriteSKUResponseViewModel
                            {
                                FavouriteSKUs = result,
                                RecordCount = result.Count()
                            }
                    };
                    JObject json = JObject.Parse(obj);
                    MobileActivityLogProvider.ActivityLog(json, response, query.DistributorID, true,
                       this.Request.RequestUri.ToString(),
                       this.Request.Headers.ToString(),
                       this.Request.Headers.UserAgent.ToString(),
                       query.Locale);

                    return response;
                }

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString() + query.MemberId);
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Get Favourite SKUs" + ex.Message, 404);
            }
            var responseWrapper =  new MobileResponseWrapper
            {
                Data = new FavouriteSKUResponseViewModel { FavouriteSKUs = null, RecordCount = 0}
            };

            MobileActivityLogProvider.ActivityLog(query, responseWrapper, query.MemberId, true,
                        this.Request.RequestUri.ToString(),
                        this.Request.Headers.ToString(),
                        this.Request.Headers.UserAgent.ToString(),
                        query.Locale);

            return responseWrapper;
        }

        /// <summary>
        ///     Below controller method is used for create/update order
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [CamelCasingFilter]
        [SetCulture]
        [UserTokenAuthenticate]
        public GetFavouriteResponseWrapper Post(SetFavouriteRequestViewModel request, string memberId)
        {
            if (request == null)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Set Favourite SKU information", 999998);
            }

            if(request.Data.Favourites.Count < 1)
            {
                throw CreateException(HttpStatusCode.BadRequest, "Invalid or Incomplete Set Favourite SKU information", 999998);
            }
            string obj = JsonConvert.SerializeObject(request);
            var response = new GetFavouriteResponseWrapper
            {
                ValidationErrors = new List<ValidationErrorViewModel>(),
                Data = new FavouriteSetSKUResponseViewModel
                {
                     Favourites = new List<FavouriteSetSKUResponseViewModelItem>()
                }
            };

            try
            {
                string locale = Thread.CurrentThread.CurrentCulture.Name;

                Dictionary<string, SKU_V01> allSKU = CatalogProvider.GetAllSKU(locale);
                List<FavouriteSKUUpdateItemViewModel> favorList = request.Data.Favourites;
                List<ValidationErrorViewModel> errors = new List<ValidationErrorViewModel>();

                IEnumerable<FavouriteSKUUpdateItemViewModel> availableList = favorList.Where(x => allSKU.Select(y => y.Key).Contains(x.ProductSKU));
                IEnumerable<FavouriteSKUUpdateItemViewModel> exceptList = favorList.Except(availableList);
                

                List<FavouriteSKUUpdateItemViewModel> SKUs = new List<FavouriteSKUUpdateItemViewModel>();
                foreach (var fv in availableList)
                {
                    var sku = allSKU.Where(x => x.Key == fv.ProductSKU); //.Where(x => x.Key == fv.ProductSKU).Select(x => x.Value);

                    if (sku != null)
                    {
                        SKU_V01 pd = (SKU_V01)sku.First().Value;
                        SKUs.Add(new FavouriteSKUUpdateItemViewModel { productID = int.Parse(pd.CatalogItem.StockingSKU), ProductSKU = fv.ProductSKU, Action = fv.Action });
                    }
                }

                string skuList="";
                foreach(var sku in SKUs)
                    skuList += sku.productID + "," + sku.ProductSKU + "," + sku.Action + "|";


                SetFavouriteParam query = new SetFavouriteParam { 

                    DistributorID = memberId,
                    Locale = Thread.CurrentThread.CurrentCulture.Name,
                    SKUList = skuList
                };

                var result = _mobileFavouriteProvider.SetFavouriteSKUs(query, ref errors);

                if(result)
                {
                    foreach (var s in SKUs)
                        response.Data.Favourites.Add(new FavouriteSetSKUResponseViewModelItem { productSKU = s.ProductSKU, Updated = true});

                    if (exceptList.Any())
                    {
                        foreach (var ex in exceptList)
                            response.Data.Favourites.Add(new FavouriteSetSKUResponseViewModelItem { productSKU = ex.ProductSKU, Updated = false, reason = "SKU of " + ex.ProductSKU + " not part of product master" });

                       response.ValidationErrors.Add(
                           new ValidationErrorViewModel
                           {
                               Message = "Update partially successful!"
                           });
                    }
                }
                else
                {
                    response.ValidationErrors.Add(
                           new ValidationErrorViewModel
                           {
                               Message = "Update not successful, kindly contact the administrator!"
                           });
                }
                JObject json = JObject.Parse(obj);
                MobileActivityLogProvider.ActivityLog(json, response, memberId, true,
                    this.Request.RequestUri.ToString(),
                    this.Request.Headers.ToString(),
                    this.Request.Headers.UserAgent.ToString(), 
                    locale);


                return response;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                throw CreateException(HttpStatusCode.InternalServerError,
                    "Internal server errror searching for Set Favourite SKU" + ex.Message, 404);

            }
        }

        #region Private Method


        private HttpResponseException CreateException(HttpStatusCode httpStatusCode, string reasonText, int errorCode)
        {
            var error = new HttpError(reasonText) { { "code", errorCode } };
            return new HttpResponseException(Request.CreateErrorResponse(httpStatusCode, error));
        }

        #endregion

    }
}