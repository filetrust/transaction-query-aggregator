using Flurl;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http
{
    public class GlasswallFlurlClient : IGlasswallHttpClient
    {
        private readonly ILogger<GlasswallFlurlClient> _logger;

        public GlasswallFlurlClient(ILogger<GlasswallFlurlClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<GlasswallHttpResponse<TBody>> SendAsync<TBody>(GlasswallHttpRequest request, CancellationToken cancellationToken) where TBody : class
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return InternalSendAsync<TBody>(request, cancellationToken);
        }

        private async Task<GlasswallHttpResponse<TResponse>> InternalSendAsync<TResponse>(
            GlasswallHttpRequest request,
            CancellationToken cancellationToken) where TResponse : class
        {
            _logger.LogInformation($"{request.HttpMethod} {request.FullPath} - request starting..");

            var flurlRequest = new FlurlRequest();

            foreach (var header in request.Headers)
            {
                flurlRequest.WithHeader(header.Key, header.Value);
            }

            flurlRequest.Url = new Url(request.FullPath);

            var response = await flurlRequest
                .SendJsonAsync(request.HttpMethod, request.Body, cancellationToken);

            _logger.LogInformation($"{request.HttpMethod} {request.FullPath} - request finished - {response.StatusCode}");
            
            return new GlasswallHttpResponse<TResponse>
            {
                StatusCode = (HttpStatusCode)response.StatusCode,
                Body = typeof(TResponse) == typeof(string) ? 
                     (await response.GetStringAsync()) as TResponse
                    : await response.GetJsonAsync<TResponse>()
            };
        }
    }
}
