using System.Collections.Generic;
using System.Threading;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Store;
using Moq;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests
{
    public class AzureFileShareTestBase : UnitTestBase<AzureFileShare>
    {
        protected Mock<ShareClient> ShareClient;

        public void SharedSetup()
        {
            ShareClient = new Mock<ShareClient>();
            ClassInTest = new AzureFileShare(ShareClient.Object);
        }

        protected Mock<AsyncPageable<ShareFileItem>> MockPageable(IEnumerable<ShareFileItem> itemsInOrderOfProcessing)
        {
            var mockedPagable = new Mock<AsyncPageable<ShareFileItem>>();
            mockedPagable.Setup(s => s.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(itemsInOrderOfProcessing.AsAsyncEnumerator());
            return mockedPagable;
        }
    }
}