using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.China
{
    /// <summary>
    /// China order status filtering options
    /// </summary>
    public enum OrderStatusFilterType : int
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,

        /// <summary>
        /// 待付款
        /// </summary>
        Payment_Pending = 1,

        /// <summary>
        /// 支付成功
        /// </summary>
        Payment_Success = 2,

        /// <summary>
        /// 支付失败
        /// </summary>
        Payment_Failed = 3,

        /// <summary>
        /// 处理中
        /// </summary>
        In_Progress = 4,

        /// <summary>
        /// 待配送
        /// </summary>
        To_Be_Assign = 5,

        /// <summary>
        /// 配货中
        /// </summary>
        NTS_Printed = 6,

        /// <summary>
        /// 已出库
        /// </summary>
        Complete = 7,

        /// <summary>
        /// 取消订单
        /// </summary>
        Cancel_Order = 8,
    }
}
