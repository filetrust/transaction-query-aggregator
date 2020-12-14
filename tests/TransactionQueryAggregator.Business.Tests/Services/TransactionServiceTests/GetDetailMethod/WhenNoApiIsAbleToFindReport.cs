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

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.GetDetailMethod
{
    [TestFixture]
    public class WhenNoApiIsAbleToFindReport : TransactionServiceTestBase
    {
        private const string Endpoint1 = "https://localtoast:3232";
        private const string Endpoint2 = "https://localroast:9292";

        private CancellationToken _cancellationToken;
        private string _input;
        private GetDetailResponseV1 _output;

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

            _input = "/mnt/transactions/analysisreport.xml";

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

            HttpClient.Setup(s =>
                    s.SendAsync<GetDetailResponseV1>(It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint1}/api/v1/transactions?filePath={_input}"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<GetDetailResponseV1>
                {
                    Body = new GetDetailResponseV1
                    {
                        Status = DetailStatus.FileNotFound
                    },
                    StatusCode = HttpStatusCode.OK
                });

            HttpClient.Setup(s =>
                    s.SendAsync<GetDetailResponseV1>(It.Is<GlasswallHttpRequest>(x => x.FullPath == $"{Endpoint2}/api/v1/transactions?filePath={_input}"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<GetDetailResponseV1>
                {
                    Body = new GetDetailResponseV1
                    {
                        Status = DetailStatus.FileNotFound
                    },
                    StatusCode = HttpStatusCode.OK
                });

            _output = await ClassInTest.GetDetailAsync(_input, _cancellationToken = new CancellationToken(false));
        }

        [Test]
        public void Expected_Number_Of_Trips_To_Api_Met()
        {
            HttpClient.Verify(s =>
                s.SendAsync<GetDetailResponseV1>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint1}/api/v1/transactions?filePath={_input}" 
                        && x.HttpMethod == HttpMethod.Get),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<GetDetailResponseV1>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint2}/api/v1/transactions?filePath={_input}"
                             && x.HttpMethod == HttpMethod.Get),
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

        [Test]
        public void Expected_Output_Is_Returned()
        {
            Assert.That(_output, Is.Not.Null);
            Assert.That(_output.AnalysisReport, Is.Null);
            Assert.That(_output.Status, Is.EqualTo(DetailStatus.FileNotFound));
        }
    }
}