using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Test.Mocks
{
    public class MockHttpContext : HttpContextWrapper
    {
        private readonly MockHttpSessionState _session = null;

        public MockHttpContext(HttpContext httpContext)
            : base(httpContext)
        {
            _session = new MockHttpSessionState();
        }

        public override HttpSessionStateBase Session
        {
            get { return _session; }
        }

        public override HttpApplication ApplicationInstance
        {
            get
            {
                return base.ApplicationInstance;
            }
            set
            {
                base.ApplicationInstance = value;
            }
        }
    }
}
