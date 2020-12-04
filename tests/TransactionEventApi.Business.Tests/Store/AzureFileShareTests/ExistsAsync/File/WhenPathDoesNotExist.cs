using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ExistsAsync.File
{
    [TestFixture]
    public class WhenPathDoesNotExist : AzureFileShareTestBase
    {
        private string _input;
        private bool _output;
        private Mock<ShareDirectoryClient> _directory;
        private Mock<ShareFileClient> _fileClient;
        private Mock<Response<bool>> _existsResponse;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            ShareClient.Setup(s => s.GetRootDirectoryClient())
                .Returns((_directory = new Mock<ShareDirectoryClient>()).Object);

            _directory.Setup(s => s.GetFileClient(It.IsAny<string>()))
                .Returns((_fileClient = new Mock<ShareFileClient>()).Object);

            _fileClient.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((_existsResponse = new Mock<Response<bool>>()).Object);

            _existsResponse.Setup(s => s.Value)
                .Returns(false);

            _output = await ClassInTest.ExistsAsync(_input = "some-file.txt", CancellationToken.None);
        }

        [Test]
        public void Output_Is_Correct()
        {
            Assert.That(_output, Is.False);
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
        public void File_Is_Checked_For_Existence()
        {
            _fileClient.Verify(s => s.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _fileClient.VerifyNoOtherCalls();
        }
    }
}