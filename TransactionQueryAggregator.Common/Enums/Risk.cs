// ReSharper disable InconsistentNaming
namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums
{
    public enum Risk
    {
        Unknown = -1,
        BlockedByPolicy,
        BlockedByNCFS,
        AllowedByPolicy,
        AllowedByNCFS,
        Safe
    }
}