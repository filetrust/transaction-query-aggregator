using System.Collections.Generic;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class GetMetricsResponseV1
    {
        public long TotalProcessed { get; set; }

        public IEnumerable<HourlyMetric> Data { get; set; }
    }
}