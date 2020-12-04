using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Files.Shares;
using Glasswall.Administration.K8.TransactionQueryAggregator;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using TestCommon;

namespace TransactionQueryAggregator.Tests.StartupTests
{
    [TestFixture]
    public class WhenUsingStartup : UnitTestBase<Startup>
    {
        [OneTimeSetUp]
        public void Setup()
        {
            ClassInTest = new Startup(Mock.Of<IConfiguration>());
        }

        [Test]
        public void Can_Resolve_Transaction_Service()
        {
            var services = new ServiceCollection();
            
            ClassInTest.ConfigureServices(services);

            Assert.That(services.Any(s =>
                s.ServiceType == typeof(IEnumerable<ShareClient>)), "No share client was added");

            Assert.That(services.Any(s =>
                s.ServiceType == typeof(IEnumerable<IFileShare>)), "No file store was added");

            services.Replace(new ServiceDescriptor(typeof(IEnumerable<ShareClient>),
                new [] { Mock.Of<ShareClient>() }));

            services.BuildServiceProvider().GetRequiredService<ITransactionService>();
        }

        [Test]
        public void Configuration_Can_Be_Parsed()
        {
            var services = new ServiceCollection();

            ClassInTest.ConfigureServices(services);

            Environment.SetEnvironmentVariable(nameof(ITransactionQueryAggregatorConfiguration.TransactionStoreConnectionStringCsv), "string1,string2");
            Environment.SetEnvironmentVariable(nameof(ITransactionQueryAggregatorConfiguration.ShareName), "transactions");

            var config = services.BuildServiceProvider().GetRequiredService<ITransactionQueryAggregatorConfiguration>();

            Assert.That(config.ShareName, Is.EqualTo("transactions"));
            Assert.That(config.TransactionStoreConnectionStringCsv, Is.EqualTo("string1,string2"));
        }
        
        [Test]
        public void When_ConfigValue_Is_Missing()
        {
            var services = new ServiceCollection();

            Environment.SetEnvironmentVariable(nameof(ITransactionQueryAggregatorConfiguration.TransactionStoreConnectionStringCsv), "");
            Environment.SetEnvironmentVariable(nameof(ITransactionQueryAggregatorConfiguration.ShareName), "");

            ClassInTest.ConfigureServices(services);

            Assert.That(() => services.BuildServiceProvider().GetRequiredService<ITransactionQueryAggregatorConfiguration>(),
                Throws.Exception.InstanceOf<ConfigurationBindException>().With.Message.EqualTo(
                    "Error binding configuration: TransactionStoreConnectionStringCsv - Value must be at least 1 characters. Got " + Environment.NewLine + "ShareName - Value must be at least 1 characters. Got "));
        }

        [Test]
        public void Constructor_Constructs_With_Mocked_Params()
        {
            ConstructorAssertions.ConstructsWithMockedParameters<Startup>();
        }

        [Test]
        public void Constructor_Is_Guarded_Against_Null()
        {
            ConstructorAssertions.ClassIsGuardedAgainstNull<Startup>();
        }
    }
}
