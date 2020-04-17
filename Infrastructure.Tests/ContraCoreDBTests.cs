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
            var distinctLeaseClients = LeaseModel.LeaseDetailsByMAC(TimeSpan.FromDays(7));

            foreach (Lease distinctLeaseClient in distinctLeaseClients)
            {
                Console.WriteLine(distinctLeaseClient.MAC);
            }

            Console.WriteLine(distinctLeaseClients.Count);
        }
    }
}