using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Moq;
using NUnit.Framework;
using System;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.GetTransactionsMethod
{
    [TestFixture]
    public class WhenAnEndpointThrowsException : TransactionServiceTestBase
    {
        private const string Endpoint1 = "https://localtoast:3232";
        private const string Endpoint2 = "https://localroast:9292";

        private CancellationToken _cancellationToken;
        private TransactionPostModelV1 _input;
        private GetTransactionsResponseV1 _output;
        private Exception _exception;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            Configuration.Setup(s => s.TransactionQueryServiceEndpointCsv)
                .Returns(Endpoint1 + "," + Endpoint2);

            _input = new TransactionPostModelV1();

            HttpClient.Setup(s =>
                    s.SendAsync<GetTransactionsResponseV1>(
                        It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint1}/api/v1/transactions"),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(_exception = new Exception());

            HttpClient.Setup(s =>
                    s.SendAsync<GetTransactionsResponseV1>(It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint2}/api/v1/transactions"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<GetTransactionsResponseV1>
                {
                    Body = new GetTransactionsResponseV1
                    {
                        Files = new[]
                        {
                            new GetTransactionsResponseV1File(),
                            new GetTransactionsResponseV1File()
                        }
                    },
                    StatusCode = HttpStatusCode.OK
                });

            _output = await ClassInTest.GetTransactionsAsync(_input, _cancellationToken = new CancellationToken(false));
        }

        [Test]
        public void Expected_Number_Of_Trips_To_Api_Met()
        {
            HttpClient.Verify(s =>
                s.SendAsync<GetTransactionsResponseV1>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint1}/api/v1/transactions"
                             && x.HttpMethod == HttpMethod.Post
                             && x.Body == _input),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<GetTransactionsResponseV1>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint2}/api/v1/transactions"
                             && x.HttpMethod == HttpMethod.Post
                             && x.Body == _input),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.VerifyNoOtherCalls();
        }

        [Test]
        public void Expected_Output_Is_Returned()
        {
            Assert.That(_output, Is.Not.Null);
            Assert.That(_output.Files, Has.Exactly(2).Items);
            Assert.That(_output.Count, Is.EqualTo(2));
        }
    }
}