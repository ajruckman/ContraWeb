using System;
using System.Collections.Generic;
using System.Net;
using Infrastructure.Controller;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class ContraLogTests
    {
        private ContraLogClient? _client;

        [SetUp]
        public void SetUp()
        {
            _client = new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralog_mgr;Password=a6oMaVZZm8nuedax;");
        }

        [Test]
        public void TestLogActionsPerHour()
        {
            (Dictionary<long, Dictionary<string, dynamic>>, List<string>) actionsPerHour = _client!.LogActionsPerHour();

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
            Dictionary<string, int> actionCounts = _client!.LogActionCounts();

            foreach ((string action, long count) in actionCounts)
            {
                Console.WriteLine($"{action} -> {count}");
            }
        }

        [Test]
        public void TestTopBlocked()
        {
            List<(string, int)> blocked = _client!.TopBlocked(DateTime.Now.Subtract(TimeSpan.FromHours(24 * 7 + 5)), 10);

            foreach ((string domain, long c) in blocked)
            {
                Console.WriteLine($"{domain,-50} -> {c}");
            }
        }

        [Test]
        public void TestTopClients()
        {
            List<(string, string, string, int)> top = _client!.TopClients(DateTime.Now.Subtract(TimeSpan.FromDays(7)), 100);

            foreach ((string ip, string hostname, string vendor, int c) in top)
            {
                Console.WriteLine($"{ip} -> {hostname} {vendor} {c}");
            }
        }
    }
}