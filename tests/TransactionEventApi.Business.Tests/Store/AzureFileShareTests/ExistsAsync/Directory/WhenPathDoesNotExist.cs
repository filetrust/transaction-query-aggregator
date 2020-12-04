using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ExistsAsync.Directory
{
    [TestFixture]
    public class WhenPathDoesNotExist : AzureFileShareTestBase
    {
        private string _input;
        private bool _output;
        private Mock<ShareDirectoryClient> _directory;
        private Mock<Response<bool>> _existsResponse;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            ShareClient.Setup(s => s.GetDirectoryClient(It.IsAny<string>()))
                .Returns((_directory = new Mock<ShareDirectoryClient>()).Object);

            _directory.Setup(s => s.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((_existsResponse = new Mock<Response<bool>>()).Object);

            _existsResponse.Setup(s => s.Value)
                .Returns(false);

            _output = await ClassInTest.ExistsAsync(_input = "some-directory", CancellationToken.None);
        }

        [Test]
        public void Output_Is_Correct()
        {
            Assert.That(_output, Is.False);
        }

        [Test]
        public void Directory_Is_Interrogated()
        {
            ShareClient.Verify(s => s.GetDirectoryClient(It.Is<string>(f => f == _input)), Times.Once);
        }

        [Test]
        public void Path_Is_Checked_For_Existence()
        {
            _directory.Verify(s => s.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _directory.VerifyNoOtherCalls();
        }
    }
}