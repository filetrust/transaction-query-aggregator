using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ILogger<ITransactionService> _logger;
        private readonly ITransactionQueryAggregatorConfiguration _configuration;

        public TransactionService(
            ILogger<ITransactionService> logger, 
            ITransactionQueryAggregatorConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task<GetTransactionsResponseV1> GetTransactionsAsync(GetTransactionsRequestV1 request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return InternalGetTransactionsAsync(request, cancellationToken);
        }

        public Task<GetDetailResponseV1> GetDetailAsync(string fileDirectory, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(fileDirectory))
                throw new ArgumentException("Value must not be null or whitespace", nameof(fileDirectory));

            return InternalTryGetDetailAsync(fileDirectory, cancellationToken);
        }

        private async Task<GetDetailResponseV1> InternalTryGetDetailAsync(string fileDirectory, CancellationToken cancellationToken)
        {
            foreach (var transactionQueryServiceEndpoint in _configuration.TransactionQueryServiceEndpointCsv.Split(',').Select(s => $"{s}/api/v1/transactions"))
            {
                try
                {
                    _logger.LogInformation("Querying transaction query service : '{0}'", transactionQueryServiceEndpoint);
                    
                    var detail = await InternalFlurlEndpoint(transactionQueryServiceEndpoint)
                        .SetQueryParam("filePath", fileDirectory)
                        .GetJsonAsync<GetDetailResponseV1>(cancellationToken);
                    
                    _logger.LogInformation("Queried transaction query service : '{0}' - status: '{1}'", transactionQueryServiceEndpoint, detail?.Status);

                    if (detail == null || detail.Status != DetailStatus.Success)
                        continue;

                    return detail;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                }
            }

            return new GetDetailResponseV1
            {
                Status = DetailStatus.FileNotFound,
                AnalysisReport = null
            };
        }

        private async Task<GetTransactionsResponseV1> InternalGetTransactionsAsync(
            GetTransactionsRequestV1 request, 
            CancellationToken cancellationToken)
        {
            var endpointQueries = _configuration.TransactionQueryServiceEndpointCsv
                .Split(',')
                .Select(transactionQueryServiceEndpoint => TryQueryTransactions($"{transactionQueryServiceEndpoint}/api/v1/transactions", request, cancellationToken))
                .ToList();

            var responses = await Task.WhenAll(endpointQueries);

            return new GetTransactionsResponseV1
            {
                Files = responses.Where(s => s != null).SelectMany(s => s.Files)
            };
        }

        private async Task<GetTransactionsResponseV1> TryQueryTransactions(string endpoint, GetTransactionsRequestV1 requestBody, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Querying transaction query service '{0}'", endpoint);
                if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("Value must be defined", nameof(endpoint));
                var response = await InternalFlurlEndpoint(endpoint).PostJsonAsync(requestBody, cancellationToken);
                return await response.GetJsonAsync<GetTransactionsResponseV1>();
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Could not get transactions from '{0}'", endpoint);
                return null;
            }
        }

        private static string InternalFlurlEndpoint(string endpoint)
        {
            FlurlHttp.ConfigureClient(endpoint, cli =>
                cli.Settings.HttpClientFactory = new UntrustedCertClientFactory());

            return endpoint;
        }
    }

    public class UntrustedCertClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
        }
    }
}