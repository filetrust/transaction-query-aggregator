namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration
{
    public interface ITransactionQueryAggregatorConfiguration
    {
        string TransactionStoreConnectionStringCsv { get; }
        string ShareName { get; }
    }
}