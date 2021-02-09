using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Tests.Controllers.TransactionControllerTests.GetMetricsMethod
{
    [TestFixture]
    public class WhenRequestIsValid : TransactionControllerTestBase
    {
        private IActionResult _result;
        private TransactionAnalytics _expected;
        private DateTimeOffset _input1;
        private DateTimeOffset _input2;

        [OneTimeSetUp]
        public async Task OnetimeSetup()
        {
            base.OnetimeSetupShared();

            _expected = new TransactionAnalytics
            {
                Data = new List<AnalyticalHour>()
            };

            Service.Setup(s => s.AggregateMetricsAsync(It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_expected);

            _result = await ClassInTest.GetMetrics(_input1 = DateTimeOffset.UtcNow, _input2 = DateTimeOffset.UtcNow, CancellationToken.None);
        }

        [Test]
        public void Messages_Are_Logged()
        {
            Logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(s => s == 0),
                    It.Is<object>(s => s.ToString() == "Beginning get metrics request"),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()));

            Logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(s => s == 0),
                    It.Is<object>(s => s.ToString() == "Finished get metrics request"),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public void Transactions_Are_Retrieved()
        {
            Service.Verify(
                s => s.AggregateMetricsAsync(
                    It.Is<DateTimeOffset>(x => x == _input1),
                    It.Is<DateTimeOffset>(x => x == _input2),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public void Ok_Is_Returned()
        {
            Assert.That(_result, Is.InstanceOf<OkObjectResult>());

            Assert.That(_result, Is.InstanceOf<OkObjectResult>()
                .With.Property(nameof(OkObjectResult.Value))
                .EqualTo(_expected));
        }
    }
}