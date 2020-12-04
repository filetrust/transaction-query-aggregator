using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.DownloadAsync
{
    [TestFixture]
    public class WhenPathDoesNotExist : AzureFileShareTestBase
    {
        private string _input;
        private MemoryStream _output;
        private Mock<ShareDirectoryClient> _directory;
        private Mock<ShareFileClient> _fileClient;
        private Mock<Response<ShareFileDownloadInfo>> _response;
        private MemoryStream _expected;
        private Mock<Response<bool>> _existsResponse;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _expected = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7 });

            ShareClient.Setup(s => s.GetRootDirectoryClient())
                .Returns((_directory = new Mock<ShareDirectoryClient>()).Object);

            _directory.Setup(s => s.GetFileClient(It.IsAny<string>()))
                .Returns((_fileClient = new Mock<ShareFileClient>()).Object);

            _fileClient.Setup(s => s.DownloadAsync(
                    It.IsAny<HttpRange>(),
                    It.IsAny<bool>(),
                    It.IsAny<ShareFileRequestConditions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((_response = new Mock<Response<ShareFileDownloadInfo>>()).Object);

            _response.Setup(s => s.Value)
                .Returns(FilesModelFactory.StorageFileDownloadInfo(content: _expected));

            _fileClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((_existsResponse = new Mock<Response<bool>>()).Object);

            _existsResponse.Setup(s => s.Value)
                .Returns(false);

            _output = await ClassInTest.DownloadAsync(_input = "some-path", CancellationToken.None);
        }

        [OneTimeTearDown]
        public async Task Teardown()
        {
            await _expected.DisposeAsync();
        }

        [Test]
        public void Output_Is_Correct()
        {
            Assert.That(_output, Is.Null);
        }

        [Test]
        public void DirectoryClient_Is_Created()
        {
            ShareClient.Verify(s => s.GetRootDirectoryClient(), Times.Once);
        }

        [Test]
        public void Directory_Is_Interrogated()
        {
            _directory.Verify(s => s.GetFileClient(It.Is<string>(f => f == _input)), Times.Once);
        }

        [Test]
        public void File_Download_Is_Not_Triggered()
        {
            _fileClient.Verify(s => s.DownloadAsync(
                It.IsAny<HttpRange>(),
                It.IsAny<bool>(),
                It.IsAny<ShareFileRequestConditions>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void File_Is_Checked_For_Existence()
        {
            _fileClient.Verify(s => s.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}