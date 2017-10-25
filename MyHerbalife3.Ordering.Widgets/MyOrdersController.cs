using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using HL.Common.Logging;
using MyHerbalife3.Ordering.Widgets.Interfaces;
using MyHerbalife3.Ordering.Widgets.Model;
using MyHerbalife3.Shared.Infrastructure.Mvc;

namespace MyHerbalife3.Ordering.Widgets
{
    public class MyOrdersController : ApiController
    {
        private static IMyOrdersSource _myOrdersSource;

        public static void Inject(IMyOrdersSource myOrdersSource)
        {
            _myOrdersSource = myOrdersSource;
        }


        [WebApiCultureSwitching]
        [Authorize]
        public List<MyOrdersViewModel> Get()
        {
            return Get(User.Identity.Name);
        }

        internal List<MyOrdersViewModel> Get(string id)
        {
            try
            {
                var model =
                    _myOrdersSource.GetMyOrders(id);
                return model;
            }
            catch (Exception ex)
            {
                LoggerHelper.Exception("Error", ex, "Failed loading myOrdersViewModel list for id" + id);
            }

            return null;
        }

        [WebApiCultureSwitching]
        [Authorize]
        public async Task<MyOrdersResultModel> Post(GridPageModel data)
        {
            var memberId = User.Identity.Name;
            return await LoadMyOrders(memberId, data);
        }


        private Task<MyOrdersResultModel> LoadMyOrders(string memberId, GridPageModel data)
        {
            return Task<MyOrdersResultModel>.Factory.StartNew(() => DoLoadOrders(memberId, data));
        }

        private static MyOrdersResultModel DoLoadOrders(string memberId, GridPageModel data)
        {
            try
            {
                if (null != data && null != data.filter && null != data.filter.Filters && data.filter.Filters.Any())
                {
                    var filter = data.filter.Filters.FirstOrDefault();

                    if (null != filter && null != filter.Filters && filter.Filters.Any())
                    {
                        var anyFilter = filter.Filters.FirstOrDefault();
                        if (null != anyFilter && null != anyFilter.Value)
                        {
                            var filteredOrders = _myOrdersSource.SearchMyOrders(anyFilter.Value.ToString(), memberId);
                            if (null != filteredOrders && filteredOrders.Any())
                            {
                                return new MyOrdersResultModel
                                {
                                    Items = filteredOrders.Skip(data.skip).Take(data.take),
                                    TotalCount = filteredOrders.Count()
                                };
                            }
                            return new MyOrdersResultModel { Items = new List<MyOrdersViewModel>(), TotalCount = 0 };
                        }
                    }
                }
                var orders = _myOrdersSource.GetMyOrders(memberId);

                if (null == orders)
                {
                    LoggerHelper.Error("_myOrdersSource is returning null for " + memberId);
                    return new MyOrdersResultModel { Items = new List<MyOrdersViewModel>(), TotalCount = 0 };
                }

                if (null == data)
                {
                    return new MyOrdersResultModel { Items = orders, TotalCount = orders.Count };
                }
                return new MyOrdersResultModel
                {
                    Items = orders.Skip(data.skip).Take(data.take),
                    TotalCount = orders.Count
                };
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("DoLoadOrders MyOrdersController is throwing error " + memberId);
                return new MyOrdersResultModel { Items = new List<MyOrdersViewModel>(), TotalCount = 0 };
            }
        }
    }
}