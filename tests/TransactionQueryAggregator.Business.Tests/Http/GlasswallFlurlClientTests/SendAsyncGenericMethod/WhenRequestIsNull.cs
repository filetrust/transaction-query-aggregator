using System.Threading;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Http.GlasswallFlurlClientTests.SendAsyncGenericMethod
{
    [TestFixture]
    public class WhenRequestIsNull : GlasswallFlurlClientTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SharedSetup();
        }

        [Test]
        public void Exception_Is_Thrown()
        {
            Assert.That(async () => await ClassInTest.SendAsync<object>(null, CancellationToken.None),
                ThrowsArgumentNullException("request"));
        }
    }
}
