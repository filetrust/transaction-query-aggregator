namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration
{
    public interface ITransactionQueryAggregatorConfiguration
    {
        string TransactionQueryServiceEndpointCsv { get; }
    }
}