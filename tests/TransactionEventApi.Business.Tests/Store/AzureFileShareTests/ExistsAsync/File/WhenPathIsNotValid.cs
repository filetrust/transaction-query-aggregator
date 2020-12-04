using System.Threading;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ExistsAsync.File
{
    [TestFixture]
    public class WhenPathIsNotValid : AzureFileShareTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SharedSetup();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void Exception_Thrown(string path)
        {
            Assert.That(() => ClassInTest.ExistsAsync(path, CancellationToken.None), ThrowsArgumentException("path", "Value must not be null or whitespace"));
        }
    }
}