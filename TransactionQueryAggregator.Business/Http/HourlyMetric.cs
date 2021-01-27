using System;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class HourlyMetric
    {
        public DateTimeOffset Date { get; set; }

        public long Processed { get; set; }
    }
}