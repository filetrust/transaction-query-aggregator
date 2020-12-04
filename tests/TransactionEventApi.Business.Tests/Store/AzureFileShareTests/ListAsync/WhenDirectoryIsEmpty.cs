using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Moq;
using NUnit.Framework;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ListAsync
{
    [TestFixture]
    public class WhenDirectoryIsEmpty : AzureFileShareTestBase
    {
        private Mock<IPathFilter> _input;
        private Mock<ShareDirectoryClient> _directoryMock;
        private IEnumerable<string> _output;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _directoryMock = new Mock<ShareDirectoryClient>();
            _input = new Mock<IPathFilter>();

            ShareClient.Setup(s => s.GetRootDirectoryClient())
                .Returns(_directoryMock.Object);

            var directoryContentsSequence = _directoryMock.SetupSequence(s => s.GetFilesAndDirectoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
            var pathSequence = _directoryMock.SetupSequence(s => s.Path);

            pathSequence.Returns("");
            directoryContentsSequence.Returns(() => MockPageable(Enumerable.Empty<ShareFileItem>()).Object);

            _output = await ClassInTest.ListAsync(_input.Object, It.IsAny<CancellationToken>()).AsEnumerableAsync();
        }
        
        [Test]
        public void Correct_Items_Are_Returned()
        {
            Assert.That(_output, Is.Empty);
        }

        [Test]
        public void Only_Root_Directory_Is_Searched()
        {
            _directoryMock.Verify(s => s.GetFilesAndDirectoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void No_Actions_Are_Decided()
        {
            _input.VerifyNoOtherCalls();
        }
    }
}