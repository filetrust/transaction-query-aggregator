using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Services;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests
{
    public class TransactionServiceTestBase : UnitTestBase<TransactionService>
    {
        protected Mock<ILogger<ITransactionService>> Logger;
        protected Mock<ITransactionQueryAggregatorConfiguration> Configuration;
        protected Mock<IGlasswallHttpClient> HttpClient;


        public void SharedSetup()
        {
            Logger = new Mock<ILogger<ITransactionService>>();
            Configuration = new Mock<ITransactionQueryAggregatorConfiguration>();
            HttpClient = new Mock<IGlasswallHttpClient>();

            ClassInTest = new TransactionService(
                Logger.Object, 
                Configuration.Object, 
                HttpClient.Object);
        }
    }
}