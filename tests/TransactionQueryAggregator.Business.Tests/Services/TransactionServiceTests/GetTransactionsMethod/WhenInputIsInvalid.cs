using System.Threading;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.GetTransactionsMethod
{
    [TestFixture]
    public class WhenInputIsInvalid : TransactionServiceTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SharedSetup();
        }

        [Test]
        public void ArgumentException_Is_Thrown()
        {
            Assert.That(async () => await ClassInTest.GetTransactionsAsync(null, CancellationToken.None), ThrowsArgumentNullException("request"));
        }
    }
}