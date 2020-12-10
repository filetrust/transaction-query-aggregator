using System.Net;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class GlasswallHttpResponse<T>
    {
        public HttpStatusCode? StatusCode { get; set; }

        public T Body { get; set; }
    }
}