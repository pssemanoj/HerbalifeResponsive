using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyHerbalife3.Ordering.Web.Test
{
    public class IntegrationTestCategoryAttribute : TestCategoryBaseAttribute
    {
        public override IList<string> TestCategories
        {
            get { return new[] {"IntegrationTest"}; }
        }
    }
}