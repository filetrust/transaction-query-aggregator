using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests
{
    public class GetTokenRequest : GlasswallHttpRequest
    {
        public GetTokenRequest(string endpoint, string username, string password) 
            : base(string.Format($"{endpoint}/api/v1/token"), HttpMethod.Get, headers: new Dictionary<string, string>
            {
                ["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))
            })
        {
        }
    }
}