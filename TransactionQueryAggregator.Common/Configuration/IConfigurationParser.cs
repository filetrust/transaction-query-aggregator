namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration
{
    public interface IConfigurationParser
    {
        TConfiguration Parse<TConfiguration>() where TConfiguration : new();
    }
}