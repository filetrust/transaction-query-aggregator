using System.Threading;
using System.Threading.Tasks;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public interface IGlasswallHttpClient
    {
        Task<GlasswallHttpResponse<TBody>> SendAsync<TBody>(GlasswallHttpRequest request, CancellationToken cancellationToken) where TBody : class;
    }
}