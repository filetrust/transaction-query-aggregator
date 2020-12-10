using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public abstract class GlasswallHttpRequest
    {
        public string FullPath { get; }
        public HttpMethod HttpMethod { get; }
        public object Body { get; }

        protected GlasswallHttpRequest(string fullPath, HttpMethod httpMethod, object body = null)
        {
            FullPath = fullPath;
            HttpMethod = httpMethod;
            Body = body;
        }
    }
}