using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyHerbalife3.Ordering.Web.Test.Mocks
{
    public class MockHttpSessionState : HttpSessionStateBase
    {
        public Dictionary<string, object> Store = new Dictionary<string, object>();
        private readonly string _sessionId = Guid.NewGuid().ToString();

        public override object this[string key]
        {
            get
            {
                object result;
                Store.TryGetValue(key, out result);
                return result;
            }
            set
            {
                Store[key] = value;
            }
        }
        public override void Add(string name, object value)
        {
            Store[name] = value;
        }
        public override void Clear()
        {
            Store.Clear();
        }
        public override int Count
        {
            get { return Store.Count; }
        }

        public override string SessionID
        {
            get
            {
                return _sessionId;
            }
        }
    }
}
