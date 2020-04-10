using System;
using System.Collections.Generic;
using Infrastructure.Controller;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class ContraLogTests
    {
        [Test]
        public void TestConnection()
        {
            // Client contraLogClient = new Client("tcp://10.3.0.16:9000?username=contralogmgr&password=contralogmgr&database=contralog");
            ContraLogClient contraLogClient =
                new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralogmgr;Password=contralogmgr;");

            (Dictionary<long, Dictionary<string, dynamic>>, List<string>) actionsPerHour = contraLogClient.LogActionsPerHour();

            Console.WriteLine(JsonConvert.SerializeObject(actionsPerHour));

            foreach ((string action, List<ContraLogClient.HourStat> counts) in actionsPerHour)
            {
                Console.WriteLine($"{action} ->");
                foreach (ContraLogClient.HourStat hourStat in counts)
                    Console.WriteLine($"\t{hourStat.Count} @ {hourStat.Hour}");
            }
        }
    }
}