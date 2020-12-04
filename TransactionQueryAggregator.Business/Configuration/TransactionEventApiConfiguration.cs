using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Configuration
{
    public class TransactionQueryAggregatorConfiguration : ITransactionQueryAggregatorConfiguration
    {
        public string TransactionStoreConnectionStringCsv { get; set; }
        public string ShareName { get; set; }
    }
}