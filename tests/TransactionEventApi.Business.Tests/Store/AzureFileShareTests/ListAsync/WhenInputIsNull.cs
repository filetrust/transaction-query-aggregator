using System;
using System.Threading;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Store.AzureFileShareTests.ListAsync
{
    [TestFixture]
    public class WhenInputIsNull : AzureFileShareTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            SharedSetup();
        }
        
        [Test]
        public void Exception_Is_Thrown()
        {
            Assert.That(() => ClassInTest.ListAsync(null, CancellationToken.None),
                Throws.ArgumentNullException.With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("pathFilter"));
        }
    }
}