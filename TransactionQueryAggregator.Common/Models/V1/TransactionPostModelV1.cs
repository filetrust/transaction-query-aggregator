using System.ComponentModel.DataAnnotations;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1
{
    public class TransactionPostModelV1
    {
        [Required]
        public FileStoreFilterV1 Filter { get; set; }
    }
}