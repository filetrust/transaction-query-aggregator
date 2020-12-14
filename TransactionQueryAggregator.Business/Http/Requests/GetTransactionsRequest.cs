using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using System.Collections.Generic;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests
{
    public class GetTransactionsRequest : GlasswallHttpRequest
    {
        public GetTransactionsRequest(string endpoint, TransactionPostModelV1 body, string token)
            : base($"{endpoint}/api/v1/transactions", HttpMethod.Post, body, new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            })
        {
        }
    }
}