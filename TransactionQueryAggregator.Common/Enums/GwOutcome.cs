﻿namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums
{
    public enum GwOutcome
    {
        Unknown = -1, // If the metadata is not available yet
        Replace,
        Unmodified,
        Failed
    }
}