using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Configuration
{
    [ExcludeFromCodeCoverage]
    public class TransactionQueryAggregatorConfiguration : ITransactionQueryAggregatorConfiguration
    {
        public string TransactionQueryServiceEndpointCsv { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}