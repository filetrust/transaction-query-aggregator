using System.Collections.Generic;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public abstract class GlasswallHttpRequest
    {
        public string FullPath { get; }
        public HttpMethod HttpMethod { get; }
        public IDictionary<string, string> Headers { get; set; }
        public object Body { get; }

        protected GlasswallHttpRequest(string fullPath, HttpMethod httpMethod, object body = null, IDictionary<string, string> headers = null)
        {
            FullPath = fullPath;
            HttpMethod = httpMethod;
            Body = body;
            Headers = headers ?? new Dictionary<string, string>();
        }
    }
}