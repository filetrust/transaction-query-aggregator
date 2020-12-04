using System;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1
{
    public class GetTransactionsResponseV1File
    {
        public DateTimeOffset Timestamp { get; set; }
        public Guid FileId { get; set; }
        public FileType DetectionFileType { get; set; }
        public Risk Risk { get; set; }
        public Guid ActivePolicyId { get; set; }
        public string Directory { get; set; }
    }
}