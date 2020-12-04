using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Store;
using NUnit.Framework;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ConstructorTests
{
    [TestFixture]
    public class WhenConstructing
    {
        [Test]
        public void Constructor_Is_Guarded_Against_Null()
        {
            ConstructorAssertions.ClassIsGuardedAgainstNull<AzureFileShare>();
        }

        [Test]
        public void Constructor_Constructs_With_Mocked_Parameters()
        {
            ConstructorAssertions.ConstructsWithMockedParameters<AzureFileShare>();
        }
    }
}
