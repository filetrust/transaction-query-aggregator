using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ExistsAsync.File
{
    [TestFixture]
    public class WhenRequestThrowsRequestFailedException : AzureFileShareTestBase
    {
        private Mock<ShareDirectoryClient> _directory;
        private Mock<ShareFileClient> _fileClient;

        [SetUp]
        public void Setup()
        {
            SharedSetup();

            ShareClient.Setup(s => s.GetRootDirectoryClient())
                .Returns((_directory = new Mock<ShareDirectoryClient>()).Object);

            _directory.Setup(s => s.GetFileClient(It.IsAny<string>()))
                .Returns((_fileClient = new Mock<ShareFileClient>()).Object);
        }

        [Test]
        public async Task False_Is_Returned_If_Error_Is_ParentNotFound()
        {
            var ex = new RequestFailedException(
                500, 
                "Error", 
                ShareErrorCode.ParentNotFound.ToString(),
                null);

            _fileClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            var exists = await ClassInTest.ExistsAsync("some-path.txt", CancellationToken.None);
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

            _fileClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(ex);

            Assert.That(async() => await ClassInTest.ExistsAsync("some-path.txt", CancellationToken.None), Throws.Exception.InstanceOf<RequestFailedException>());
        }
    }
}