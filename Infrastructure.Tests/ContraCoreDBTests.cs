using System;
using Infrastructure.Model;
using Infrastructure.Schema;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class ContraCoreDBTests
    {
        [Test]
        public void TestDistinctLeaseClients()
        {
            var distinctLeaseClients = StatsModel.DistinctLeaseClients(TimeSpan.FromDays(7));

            foreach (DistinctLeaseClient distinctLeaseClient in distinctLeaseClients)
            {
                Console.WriteLine(distinctLeaseClient.MAC);
            }

            Console.WriteLine(distinctLeaseClients.Count);
        }
    }
}