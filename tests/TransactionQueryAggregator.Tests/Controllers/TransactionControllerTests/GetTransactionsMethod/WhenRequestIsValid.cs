using System;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Tests.Controllers.TransactionControllerTests.GetTransactionsMethod
{
    [TestFixture]
    public class WhenRequestIsValid : TransactionControllerTestBase
    {
        private TransactionPostModelV1 _input;
        private IActionResult _result;
        private GetTransactionsResponseV1 _expected;

        [OneTimeSetUp]
        public async Task OnetimeSetup()
        {
            base.OnetimeSetupShared();

            _expected = new GetTransactionsResponseV1();

            Service.Setup(s => s.GetTransactionsAsync(It.IsAny<TransactionPostModelV1>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_expected);

            _result = await ClassInTest.GetTransactions(_input = new TransactionPostModelV1(), CancellationToken.None);
        }

        [Test]
        public void Messages_Are_Logged()
        {
            Logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(s => s == 0),
                    It.Is<object>(s => s.ToString() == "Beginning get transactions request"),
                    It.IsAny<Exception>(), 
                    (Func<object, Exception, string>) It.IsAny<object>()));

            Logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.Is<EventId>(s => s == 0),
                    It.Is<object>(s => s.ToString() == "Finished get transactions request"),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()));
        }

        [Test]
        public void Transactions_Are_Retrieved()
        {
            Service.Verify(
                s => s.GetTransactionsAsync(
                    It.Is<TransactionPostModelV1>(x => x == _input),
                    It.IsAny<CancellationToken>()), 
                Times.Once);
        }

        [Test]
        public void Ok_Is_Returned()
        {
            Assert.That(_result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)_result).Value, Is.EqualTo(_expected));
        }
    }
}