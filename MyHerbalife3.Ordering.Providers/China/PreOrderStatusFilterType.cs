using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Providers.China
{
    /// <summary>
    /// China pre-order status filtering options
    /// </summary>
    public enum PreOrderStatusFilterType : int
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,

        /// <summary>
        /// 已提交
        /// </summary>
        ReadyToSubmit_PreOrder = 12,

        /// <summary>
        /// 取消 
        /// </summary>
        Cancel_PreOrder = 13,

        /// <summary>
        /// 缓  
        /// </summary>
        Hold_PreOrder = 14,
    }
}
