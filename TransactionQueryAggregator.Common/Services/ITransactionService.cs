using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services
{
    public interface ITransactionService
    {
        Task<GetTransactionsResponseV1> GetTransactionsAsync(TransactionPostModelV1 request, CancellationToken cancellationToken);
        Task<GetDetailResponseV1> GetDetailAsync(string fileDirectory, CancellationToken cancellationToken);
    }
}