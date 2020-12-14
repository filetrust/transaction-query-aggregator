using System.Collections.Generic;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests
{
    public class GetTransactionDetailRequest : GlasswallHttpRequest
    {
        public GetTransactionDetailRequest(string endpoint, string filePath, string token)
            : base($"{endpoint}/api/v1/transactions?filePath={filePath}", HttpMethod.Get, headers: new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            })
        {
        }
    }
}