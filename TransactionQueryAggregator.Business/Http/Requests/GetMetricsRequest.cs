using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests
{
    public class GetMetricsRequest : GlasswallHttpRequest
    {
        public GetMetricsRequest(string endpoint, DateTimeOffset fromDate, DateTimeOffset toDate, string token)
            : base($"{endpoint}/api/v1/transactions/metrics?fromDate={FormatDate(fromDate)}&toDate={FormatDate(toDate)}", HttpMethod.Get, null, new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            })
        {
        }

        private static string FormatDate(DateTimeOffset date)
        {
            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        }
    }
}