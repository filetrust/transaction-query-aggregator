using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using NUnit.Framework;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Http.GlasswallFlurlClientTests.Constructor
{
    [TestFixture]
    public class ConstructorTests
    {
        [Test]
        public void Constructor_Is_Guarded_Against_Null()
        {
            ConstructorAssertions.ClassIsGuardedAgainstNull<GlasswallFlurlClient>();
        }

        [Test]
        public void Constructor_Constructs_With_Mocked_Parameters()
        {
            ConstructorAssertions.ConstructsWithMockedParameters<GlasswallFlurlClient>();
        }
    }
}
