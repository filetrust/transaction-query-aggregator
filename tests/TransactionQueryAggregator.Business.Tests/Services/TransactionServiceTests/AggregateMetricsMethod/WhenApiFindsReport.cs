﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http.Requests;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Enums;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.AnalysisReport;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Models.V1;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Moq;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests.AggregateMetricsMethod
{
    [TestFixture]
    public class WhenApiFindsReport : TransactionServiceTestBase
    {
        private const string Endpoint1 = "https://localtoast:3232";
        private const string Endpoint2 = "https://localroast:9292";

        private CancellationToken _cancellationToken;
        private DateTimeOffset _input1;
        private DateTimeOffset _input2;
        private TransactionAnalytics _output;

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

            Configuration.Setup(s => s.Password)
                .Returns("Password");

            _input1 = DateTimeOffset.UtcNow;
            _input2 = DateTimeOffset.UtcNow;

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
                s.SendAsync<TransactionAnalytics>(It.IsAny<GlasswallHttpRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GlasswallHttpResponse<TransactionAnalytics>
                {
                    Body = new TransactionAnalytics
                    {
                        Data = new List<AnalyticalHour>
                        {
                            new AnalyticalHour()
                        }
                    },
                    StatusCode = HttpStatusCode.OK
                });
            
            _output = await ClassInTest.AggregateMetricsAsync(_input1, _input2, _cancellationToken = new CancellationToken(false));
        }

        [Test]
        public void Expected_Number_Of_Trips_To_Api_Met()
        {
            HttpClient.Verify(s =>
                s.SendAsync<TransactionAnalytics>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint1}/api/v1/transactions/metrics?fromDate={_input1:yyyy-MM-ddTHH:mm:ss}&toDate={_input2:yyyy-MM-ddTHH:mm:ss}" 
                        && x.HttpMethod == HttpMethod.Get),
                    It.Is<CancellationToken>(x => x == _cancellationToken)), Times.Once);

            HttpClient.Verify(s =>
                s.SendAsync<TransactionAnalytics>(It.Is<GlasswallHttpRequest>(
                        x => x.FullPath == $"{Endpoint2}/api/v1/transactions/metrics?fromDate={_input1:yyyy-MM-ddTHH:mm:ss}&toDate={_input2:yyyy-MM-ddTHH:mm:ss}"
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
    }
}