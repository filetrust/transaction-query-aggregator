using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Http.GlasswallFlurlClientTests
{
    public class GlasswallFlurlClientTestBase : UnitTestBase<GlasswallFlurlClient>
    {
        protected Mock<ILogger<GlasswallFlurlClient>> Logger;
        
        public void SharedSetup()
        {
            Logger = new Mock<ILogger<GlasswallFlurlClient>>();
            
            ClassInTest = new GlasswallFlurlClient(Logger.Object);
        }
    }
}