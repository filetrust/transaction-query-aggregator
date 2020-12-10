using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class GetTransactionDetailRequest : GlasswallHttpRequest
    {
        public GetTransactionDetailRequest(string endpoint, string filePath)
            : base($"{endpoint}/api/v1/transactions?filePath={filePath}", HttpMethod.Get)
        {
        }
    }
}