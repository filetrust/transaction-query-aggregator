using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services
{
    public interface ITransactionService
    {
        Task<GetTransactionsResponseV1> GetTransactionsAsync(TransactionPostModelV1 request, CancellationToken cancellationToken);
        Task<GetDetailResponseV1> GetDetailAsync(string fileDirectory, CancellationToken cancellationToken);
        Task<TransactionAnalytics> AggregateMetricsAsync(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken);
    }

    public class TransactionAnalytics
    {
        public long TotalProcessed { get; set; }

        public List<AnalyticalHour> Data { get; set; }

    }

    public class AnalyticalHour
    {
        public long Processed { get; set; }

        // public long Pending { get; set; }

        public long SentToNcfs { get; set; }

        public DateTimeOffset Date { get; set; }

        public Dictionary<string, long> ProcessedByOutcome { get; set; }

        public Dictionary<string, long> ProcessedByNcfs { get; set; }
    }
}