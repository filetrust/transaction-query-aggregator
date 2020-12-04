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
    public class WhenRecursingDirectoryAndActionIsStop : AzureFileShareTestBase
    {
        private const int NumberOfDirectoriesInTree = 5;

        private Mock<IPathFilter> _input;
        private Mock<ShareDirectoryClient> _directoryMock;
        private IEnumerable<string> _output;
        private ShareFileItem[][] _directories;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _directoryMock = new Mock<ShareDirectoryClient>();
            _input = new Mock<IPathFilter>();

            ShareClient.Setup(s => s.GetRootDirectoryClient())
                .Returns(_directoryMock.Object);

            _directories = new[]
            {
                new [] { FilesModelFactory.StorageFileItem(true, "Recurse1", 0) },
                new [] { FilesModelFactory.StorageFileItem(true, "Recurse2", 0) },
                new [] { FilesModelFactory.StorageFileItem(true, "Recurse3", 0) },
                new [] { FilesModelFactory.StorageFileItem(true, "Recurse4", 0) },
                new [] { FilesModelFactory.StorageFileItem(true, "Recurse5", 0) },
                new [] { FilesModelFactory.StorageFileItem(true, "Collect1", 0), FilesModelFactory.StorageFileItem(true, "Collect2", 0) }
            };
            
            var pathActionSequence = _input.SetupSequence(s => s.DecideAction(It.IsAny<string>()));
            var directoryContentsSequence = _directoryMock.SetupSequence(s => s.GetFilesAndDirectoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));
            var pathSequence = _directoryMock.SetupSequence(s => s.Path);

            _directoryMock.Setup(s => s.GetSubdirectoryClient(It.IsAny<string>())).Returns(() => _directoryMock.Object);

            for (var i = 0; i < NumberOfDirectoriesInTree; i++)
            {
                var item = _directories[i];
                directoryContentsSequence.Returns(() => MockPageable(item).Object);
                pathActionSequence.Returns(PathAction.Recurse);
                pathSequence.Returns(string.Join("/", _directories.SelectMany(s => s.Select(x => x.Name)).Take(i)));
            }

            pathSequence.Returns(string.Join("/", _directories.SelectMany(s => s.Select(x => x.Name)).Take(5)));

            for (var i = NumberOfDirectoriesInTree; i < _directories.Length; i++)
            {
                var items = _directories[i];
                directoryContentsSequence.Returns(() => MockPageable(items).Object);

                foreach (var subItem in items)
                    pathActionSequence.Returns(PathAction.Stop);
            }

            _output = await ClassInTest.ListAsync(_input.Object, CancellationToken.None).AsEnumerableAsync();
        }
        
        [Test]
        public void Correct_Items_Are_Returned()
        {
            Assert.That(_output, Has.Exactly(0).Items);
        }

        [Test]
        public void Each_Directory_Is_Listed()
        {
            _directoryMock.Verify(
                s => s.GetFilesAndDirectoriesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Exactly(6));
        }

        [Test]
        public void Each_Directory_Action_Is_Decided()
        {
            foreach (var directory in _directories)
            foreach (var subItem in directory)
                _input.Verify(s => s.DecideAction(It.Is<string>(path => path.EndsWith(subItem.Name))), Times.Once);

            _input.VerifyNoOtherCalls();
        }
    }
}