using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Helper
{
    public class GeneralHelper
    {
        #region singleton
        private GeneralHelper() { }
        public static GeneralHelper Instance = new GeneralHelper();
        #endregion

        /// <summary>
        /// Return true if the List is not null and contains at least 1 data.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool HasData(IList list)
        {
            return (list != null) && (list.Count > 0);
        }
    }
}