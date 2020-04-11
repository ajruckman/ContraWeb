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
        public void TestLogActionsPerHour()
        {
            // Client contraLogClient = new Client("tcp://10.3.0.16:9000?username=contralogmgr&password=contralogmgr&database=contralog");
            ContraLogClient contraLogClient =
                new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralogmgr;Password=contralogmgr;");

            (Dictionary<long, Dictionary<string, dynamic>>, List<string>) actionsPerHour = contraLogClient.LogActionsPerHour();

            Console.WriteLine(JsonConvert.SerializeObject(actionsPerHour));

            foreach (KeyValuePair<long, Dictionary<string, dynamic>> hour in actionsPerHour.Item1)
            {
                Console.WriteLine($"{hour.Key} -> {hour.Value}");
                // foreach (ContraLogClient.HourStat hourStat in counts)
                    // Console.WriteLine($"\t{hourStat.Count} @ {hourStat.Hour}");
            }
        }

        [Test]
        public void TestLogActionCounts()
        {
            ContraLogClient contraLogClient =
                new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralogmgr;Password=contralogmgr;");

            Dictionary<string, long> actionCounts = contraLogClient.LogActionCounts();

            foreach ((string action, long count) in actionCounts)
            {
                Console.WriteLine($"{action} -> {count}");
            }
        }
    }
}