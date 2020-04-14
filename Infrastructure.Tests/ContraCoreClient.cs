using System.Threading.Tasks;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class ContraCoreClient
    {
        private Controller.ContraCoreClient _ccc;

        [SetUp]
        public void Setup()
        {
            _ccc = new Controller.ContraCoreClient("contralog");
        }

        [Test]
        public async Task PingTest()
        {
            _ccc.ConnectionComplete.WaitHandle.WaitOne();
            bool success = await _ccc.Ping("Infrastructure.Tests.ContraCoreClient.Setup()");
            Assert.IsTrue(success);
        }
    }
}