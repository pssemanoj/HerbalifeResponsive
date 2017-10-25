using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHerbalife3.Ordering.Rules
{
    internal class Helper
    {
        #region singleton
        public static Helper Instance = new Helper();

        private Helper() { }
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
