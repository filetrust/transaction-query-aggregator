using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Util;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Logging;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Business.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ILogger<ITransactionService> _logger;
        private readonly ITransactionQueryAggregatorConfiguration _configuration;
        private readonly IGlasswallHttpClient _httpClient;

        public TransactionService(
            ILogger<ITransactionService> logger, 
            ITransactionQueryAggregatorConfiguration configuration,
            IGlasswallHttpClient httpClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public Task<GetTransactionsResponseV1> GetTransactionsAsync(TransactionPostModelV1 request, CancellationToken cancellationToken)
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

        public async Task<TransactionAnalytics> AggregateMetricsAsync(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken cancellationToken)
        {
            var aggregated = new TransactionAnalytics { Data = new List<AnalyticalHour>() };

            foreach (var endpoint in _configuration.TransactionQueryServiceEndpointCsv.Split(','))
            {
                _logger.LogInformation("Requesting data from Transaction query service '{0}'", endpoint);

                var token = await GetToken(endpoint, cancellationToken);

                var response = await _httpClient.SendAsync<TransactionAnalytics>(
                    new GetMetricsRequest(endpoint, fromDate, toDate, token),
                    cancellationToken);

                _logger.LogInformation("Requested data from Transaction query service '{0}' - {1}", endpoint, response.StatusCode);

                if (response.Body == null)
                    continue;
                
                foreach (var newHour in response.Body.Data)
                {
                    var aggregatedHour = aggregated.Data.FirstOrDefault(f => f.Date == newHour.Date);

                    if (aggregatedHour == null)
                    {
                        aggregated.Data.Add(aggregatedHour = new AnalyticalHour
                        {
                            Date = newHour.Date, 
                            ProcessedByOutcome = new Dictionary<string, long>(),
                            ProcessedByNcfs = new Dictionary<string, long>()
                        });
                    }

                    aggregatedHour.Processed += newHour.Processed;
                    aggregatedHour.SentToNcfs += newHour.SentToNcfs;
                    aggregated.TotalProcessed += newHour.Processed;
                    aggregated.TotalSentToNcfs += newHour.SentToNcfs;

                    if (newHour.ProcessedByNcfs != null)
                        aggregatedHour.ProcessedByNcfs.Merge(newHour.ProcessedByNcfs);
                    if (newHour.ProcessedByOutcome != null)
                        aggregatedHour.ProcessedByOutcome.Merge(newHour.ProcessedByOutcome);
                }
            }

            return aggregated;
        }

        private async Task<GetDetailResponseV1> InternalTryGetDetailAsync(string fileDirectory, CancellationToken cancellationToken)
        {
            foreach (var endpoint in _configuration.TransactionQueryServiceEndpointCsv.Split(','))
            {
                _logger.LogInformation("Requesting data from Transaction query service '{0}'", endpoint);

                var token = await GetToken(endpoint, cancellationToken);

                var response = await _httpClient.SendAsync<GetDetailResponseV1>(
                    new GetTransactionDetailRequest(endpoint, fileDirectory, token),
                    cancellationToken);

                _logger.LogInformation("Requested data from Transaction query service '{0}' - {1}", endpoint, response.StatusCode);

                if (response.Body == null || response.Body?.Status != DetailStatus.Success)
                    continue;

                return response.Body;
            }

            return new GetDetailResponseV1
            {
                Status = DetailStatus.FileNotFound,
                AnalysisReport = null
            };
        }

        private async Task<string> GetToken(string endpoint, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Requesting token from Transaction query service '{0}'", endpoint);

            var response = await _httpClient.SendAsync<string>(
                    new GetTokenRequest(endpoint, _configuration.Username, _configuration.Password),
                cancellationToken);

            return response.Body;
        }

        private async Task<GetTransactionsResponseV1> InternalGetTransactionsAsync(
            TransactionPostModelV1 request, 
            CancellationToken cancellationToken)
        {
            var endpointQueries = _configuration.TransactionQueryServiceEndpointCsv
                .Split(',')
                .Select(transactionQueryServiceEndpoint => TryQueryTransactions(transactionQueryServiceEndpoint, request, cancellationToken))
                .ToList();

            var responses = await Task.WhenAll(endpointQueries);

            return new GetTransactionsResponseV1
            {
                Files = responses.Where(s => s != null).SelectMany(s => s.Files)
            };
        }

        private async Task<GetTransactionsResponseV1> TryQueryTransactions(string endpoint, TransactionPostModelV1 requestBody, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Requesting data from Transaction query service '{0}'", endpoint);

                var token = await GetToken(endpoint, cancellationToken);

                var response = await _httpClient.SendAsync<GetTransactionsResponseV1>(
                    new GetTransactionsRequest(endpoint, requestBody, token), 
                    cancellationToken);

                _logger.LogInformation("Requested data from Transaction query service '{0}' - {1}", endpoint, response.StatusCode);

                return response.Body;
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, "Could not get transactions from '{0}'", endpoint);
                return null;
            }
        }
    }
}