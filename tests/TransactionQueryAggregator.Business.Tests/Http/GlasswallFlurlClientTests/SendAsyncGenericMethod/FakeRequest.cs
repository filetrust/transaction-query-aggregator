using System.Net.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;

namespace TransactionQueryAggregator.Business.Tests.Http.GlasswallFlurlClientTests.SendAsyncGenericMethod
{
    public class FakeRequest : GlasswallHttpRequest
    {
        public FakeRequest(string fullPath, HttpMethod httpMethod, object body = null) 
            : base(fullPath, httpMethod, body)
        {
        }
    }
}