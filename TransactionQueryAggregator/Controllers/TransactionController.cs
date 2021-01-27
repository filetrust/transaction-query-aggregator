using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Glasswall.Administration.K8.TransactionQueryAggregator.Controllers
{
    [ApiController]
    [Route("api/v1/transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly ITransactionService _transactionService;

        public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
        }

        [HttpPost]
        [ValidateModel]
        public async Task<IActionResult> GetTransactions([Required][FromBody]TransactionPostModelV1 request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Beginning get transactions request");

            var transactions = await _transactionService.GetTransactionsAsync(request, cancellationToken);

            _logger.LogInformation("Finished get transactions request");

            return Ok(transactions);
        }

        [HttpGet]
        [ValidateModel]
        public async Task<IActionResult> GetDetail([Required] [FromQuery] string filePath, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Beginning get detail request");

            var detail = await _transactionService.GetDetailAsync(filePath, cancellationToken);

            _logger.LogInformation("Finished get detail request");

            return Ok(detail);
        }

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] DateTimeOffset fromDate, [FromQuery] DateTimeOffset toDate, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Beginning get metrics request");

            var dateLookupDict = new ConcurrentDictionary<DateTimeOffset, long>();

            await foreach (var (date, count) in _transactionService.AggregateMetricsAsync(fromDate, toDate, cancellationToken))
                dateLookupDict.AddOrUpdate(date, count, (d, c) => c + count);

            _logger.LogInformation("Finished get metrics request");

            return Ok(new
            {
                totalProcessed = dateLookupDict.Sum(f => f.Value),
                data = dateLookupDict.Select(s => new { date = s.Key, processed = s.Value }).OrderBy(f => f.date)
            });
        }
    }
}
