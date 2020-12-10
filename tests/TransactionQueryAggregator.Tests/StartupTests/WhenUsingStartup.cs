using System;
using System.Collections.Generic;
using System.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TestCommon;

namespace TransactionQueryAggregator.Tests.StartupTests
{
    [TestFixture]
    public class WhenUsingStartup
    {
        [Test]
        public void Can_Resolve_Transaction_Service()
        {
            const string expected = "string1;string2";
            var services = new ServiceCollection();
            var classInTest = Arrange(new Dictionary<string, string>
            {
                [nameof(ITransactionQueryAggregatorConfiguration.TransactionQueryServiceEndpointCsv)] = expected
            });

            classInTest.ConfigureServices(services);

            services.BuildServiceProvider().GetRequiredService<ITransactionService>();
        }

        [Test]
        public void Configuration_Can_Be_Parsed()
        {
            const string expected = "string1;string2";
            var services = new ServiceCollection();
            var classInTest = Arrange(new Dictionary<string, string>
            {
                [nameof(ITransactionQueryAggregatorConfiguration.TransactionQueryServiceEndpointCsv)] = expected
            });

            classInTest.ConfigureServices(services);

            var config = services.BuildServiceProvider().GetRequiredService<ITransactionQueryAggregatorConfiguration>();

            Assert.That(config.TransactionQueryServiceEndpointCsv, Is.EqualTo(expected));
        }
        
        [Test]
        public void When_ConfigValue_Is_Missing()
        {
            var services = new ServiceCollection();
            var classInTest = Arrange(new Dictionary<string, string>
            {
                [nameof(ITransactionQueryAggregatorConfiguration.TransactionQueryServiceEndpointCsv)] = null
            });

            Assert.That(() => classInTest.ConfigureServices(services),
                Throws.Exception.InstanceOf<ConfigurationErrorsException>().With.Message.EqualTo(
                    "TransactionQueryServiceEndpointCsv must be provided"));
        }

        [Test]
        public void When_ConfigValue_Is_Invalid()
        {
            var services = new ServiceCollection();
            var classInTest = Arrange(new Dictionary<string, string>
            {
                [nameof(ITransactionQueryAggregatorConfiguration.TransactionQueryServiceEndpointCsv)] = ","
            });

            Assert.That(() => classInTest.ConfigureServices(services),
                Throws.Exception.InstanceOf<ConfigurationErrorsException>().With.Message.EqualTo(
                    "TransactionQueryServiceEndpointCsv was invalid, got ','"));
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

        private static Startup Arrange(Dictionary<string, string> environmentVariables)
        {
            foreach (var (key, value) in environmentVariables)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            return new Startup(configuration);
        }
    }
}
