using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ExistsAsync.Directory
{
    [TestFixture]
    public class WhenRequestThrowsRequestFailedException : AzureFileShareTestBase
    {
        private Mock<ShareDirectoryClient> _directory;

        [SetUp]
        public void Setup()
        {
            SharedSetup();

            ShareClient.Setup(s => s.GetDirectoryClient(It.IsAny<string>()))
                .Returns((_directory = new Mock<ShareDirectoryClient>()).Object);
        }

        [Test]
        public async Task False_Is_Returned_If_Error_Is_ParentNotFound()
        {
            var ex = new RequestFailedException(
                500, 
                "Error", 
                ShareErrorCode.ParentNotFound.ToString(),
                null);

            _directory.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var exists = await ClassInTest.ExistsAsync("some-path", CancellationToken.None);
            Assert.That(exists, Is.False);
        }
        
        [Test]
        public void Exception_Is_Rethrown_If_Error_Is_ParentNotFound()
        {
            var ex = new RequestFailedException(
                500,
                "Error",
                ShareErrorCode.AccountAlreadyExists.ToString(),
                null);

            _directory.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            Assert.That(async() => await ClassInTest.ExistsAsync("some-path", CancellationToken.None), Throws.Exception.InstanceOf<RequestFailedException>());
        }
    }
}