using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class GetTransactionsRequest : GlasswallHttpRequest
    {
        public GetTransactionsRequest(string endpoint, TransactionPostModelV1 body) 
            : base($"{endpoint}/api/v1/transactions", HttpMethod.Post, body)
        {
        }
    }
}