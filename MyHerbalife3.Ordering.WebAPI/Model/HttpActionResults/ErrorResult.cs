using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace MyHerbalife3.Ordering.WebAPI.Model.HttpActionResults
{
    public class ErrorResult : IHttpActionResult
    {

        public ErrorResult(Error errorContent)
        {
            ErrorContent = errorContent;
            StatusCode = errorContent.StatusCode;
            if (HttpContext.Current != null)
                Request = HttpContext.Current.Items["MS_HttpRequestMessage"] as HttpRequestMessage;
        }

        public ErrorResult(Error errorContent, HttpRequestMessage request)
        {
            ErrorContent = errorContent;
            StatusCode = errorContent.StatusCode;
            Request = request;
        }

        /// <summary>
        /// 
        /// </summary>
        private Error ErrorContent { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private HttpRequestMessage Request { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private HttpStatusCode StatusCode { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            return new HttpResponseMessage(StatusCode)
            {
                RequestMessage = Request,
                Content = new ObjectContent<Error>(ErrorContent, new JsonMediaTypeFormatter())
            };
        }
    }
}
