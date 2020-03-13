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
            _ccc = new Controller.ContraCoreClient("127.0.0.1");
        }

        [Test]
        public async Task PingTest()
        {
            await _ccc.ConnectionComplete.Task;
            bool success = await _ccc.Ping("Infrastructure.Tests.ContraCoreClient.Setup()");
            Assert.IsTrue(success);
        }
    }
}