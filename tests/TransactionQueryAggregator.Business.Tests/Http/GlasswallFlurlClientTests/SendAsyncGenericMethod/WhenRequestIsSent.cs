using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using NUnit.Framework;

namespace TransactionQueryAggregator.Business.Tests.Http.GlasswallFlurlClientTests.SendAsyncGenericMethod
{
    [TestFixture]
    public class WhenRequestIsSent : GlasswallFlurlClientTestBase
    {
        private GlasswallHttpResponse<FakeResponse> _output;
        private FakeRequest _input;
        private HttpTest _test;
        private FakeResponse _expected;

        [OneTimeSetUp]
        public async Task Setup()
        {
            SharedSetup();

            _test = new HttpTest();

            _test.RespondWithJson(_expected = new FakeResponse { SomeProperty = "test" });

            _output = await ClassInTest.SendAsync<FakeResponse>(
                _input = new FakeRequest("https://path.com", HttpMethod.Delete, new object()), 
                new CancellationToken());
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _test?.Dispose();
        }

        [Test]
        public void Request_Is_Sent()
        {
            _test.ShouldHaveCalled(_input.FullPath)
                .With(f => f.Request.Verb == _input.HttpMethod)
                .WithRequestBody(string.Empty)
                .Times(1);
        }

        [Test]
        public void Response_Is_Retrieved()
        {
            Assert.That(_output, Is.Not.Null);
            Assert.That(_output.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(_output.Body, Is.Not.Null);
            Assert.That(_output.Body.SomeProperty, Is.EqualTo(_expected.SomeProperty));
        }
    }
}
