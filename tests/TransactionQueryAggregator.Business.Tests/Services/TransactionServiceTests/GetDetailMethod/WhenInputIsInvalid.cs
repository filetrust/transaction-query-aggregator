using NUnit.Framework;
using System.Threading;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.GetDetailMethod
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
        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void ArgumentException_Is_Thrown(string directory)
        {
            Assert.That(async() => await ClassInTest.GetDetailAsync(directory, CancellationToken.None), ThrowsArgumentException("fileDirectory", "Value must not be null or whitespace"));
        }
    }
}