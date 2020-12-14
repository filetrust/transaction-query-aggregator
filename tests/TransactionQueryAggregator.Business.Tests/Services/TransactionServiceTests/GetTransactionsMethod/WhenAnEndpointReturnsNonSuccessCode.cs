using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.GetTransactionsMethod
{
    [TestFixture]
    public class WhenAnEndpointReturnsNonSuccessCode : TransactionServiceTestBase
    {
        private const string Endpoint1 = "https://localtoast:3232";
        private const string Endpoint2 = "https://localroast:9292";

        private CancellationToken _cancellationToken;
        private TransactionPostModelV1 _input;
        private GetTransactionsResponseV1 _output;
        private GetTransactionsResponseV1File _file;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            Configuration.Setup(s => s.TransactionQueryServiceEndpointCsv)
                .Returns(Endpoint1 + "," + Endpoint2);
            Configuration.Setup(s => s.Username)
                .Returns("Username");
            Configuration.Setup(s => s.Password)
                .Returns("Password");

            HttpClient.Setup(s =>
                    s.SendAsync<string>(It.Is<GetTokenRequest>(x => x.FullPath == $"{Endpoint1}/api/v1/token"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<string>
                {
                    Body = "token",
                    StatusCode = HttpStatusCode.OK
                });

            HttpClient.Setup(s =>
                    s.SendAsync<string>(It.Is<GetTokenRequest>(x => x.FullPath == $"{Endpoint2}/api/v1/token"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<string>
                {
                    Body = "token",
                    StatusCode = HttpStatusCode.OK
                });

            _input = new TransactionPostModelV1
            {
                Filter = new FileStoreFilterV1
                {
                    AllFileIdsFound = true,
                    FileIds = new List<Guid>(),
                    FileTypes = new FileType[] { },
                    PolicyIds = new List<Guid>(),
                    Risks = new List<Risk>(),
                    TimestampRangeEnd = DateTimeOffset.MaxValue,
                    TimestampRangeStart = DateTimeOffset.MaxValue
                }
            };

            HttpClient.Setup(s =>
                    s.SendAsync<GetTransactionsResponseV1>(
                        It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint1}/api/v1/transactions"),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<GetTransactionsResponseV1>
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            HttpClient.Setup(s =>
                    s.SendAsync<GetTransactionsResponseV1>(It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint2}/api/v1/transactions"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<GetTransactionsResponseV1>
                {
                    Body = new GetTransactionsResponseV1
                    {
                        Files = new[]
                        {
                            _file = new GetTransactionsResponseV1File
                            {
                                ActivePolicyId = Guid.NewGuid(),
                                DetectionFileType = FileType.Bmp,
                                Directory = Endpoint1,
                                FileId = Guid.Empty,
                                Risk = Risk.AllowedByNCFS,
                                Timestamp = DateTimeOffset.MaxValue
                            }
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
                             && ValidateBody((TransactionPostModelV1)x.Body)),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<GetTransactionsResponseV1>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint2}/api/v1/transactions"
                             && x.HttpMethod == HttpMethod.Post
                             && ValidateBody((TransactionPostModelV1)x.Body)),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<string>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint1}/api/v1/token"
                             && x.HttpMethod == HttpMethod.Get),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<string>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint2}/api/v1/token"
                             && x.HttpMethod == HttpMethod.Get),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.VerifyNoOtherCalls();
        }

        private bool ValidateBody(TransactionPostModelV1 model)
        {
            return model.Filter.GetType().GetProperties().All(property =>
            {
                var expected = property.GetValue(_input.Filter);
                var actual = property.GetValue(model.Filter);
                return expected?.Equals(actual) ?? false;
            });
        }

        [Test]
        public void Expected_Output_Is_Returned()
        {
            Assert.That(_output, Is.Not.Null);
            Assert.That(_output.Files, Has.Exactly(1).Items);

            Assert.That(_output.Files.First().GetType().GetProperties().All(property =>
            {
                var expected = property.GetValue(_file);
                var actual = property.GetValue(_output.Files.First());
                return expected?.Equals(actual) ?? false;
            }));
        }
    }
}