using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Services;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Serialisation;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.Extensions.Logging;
using Moq;
using TestCommon;

namespace TransactionQueryAggregator.Business.Tests.Services.TransactionServiceTests
{
    public class TransactionServiceTestBase : UnitTestBase<TransactionService>
    {
        protected Mock<ILogger<ITransactionService>> Logger;
        protected Mock<IJsonSerialiser> JsonSerialiser;
        protected Mock<IXmlSerialiser> XmlSerialiser;

        protected Mock<IFileShare> Share1;
        protected Mock<IFileShare> Share2;

        public void SharedSetup()
        {
            Logger = new Mock<ILogger<ITransactionService>>();
            JsonSerialiser = new Mock<IJsonSerialiser>();
            XmlSerialiser = new Mock<IXmlSerialiser>();
            Share1 = new Mock<IFileShare>();
            Share2 = new Mock<IFileShare>();

            ClassInTest = new TransactionService(Logger.Object, new [] { Share1.Object, Share2.Object }, JsonSerialiser.Object, XmlSerialiser.Object);
        }
    }
}