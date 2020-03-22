#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using Infrastructure.Schema;
using NUnit.Framework;

namespace Infrastructure.Tests
{
    public class WhitelistModel
    {
        [Test]
        public void TestList()
        {
            List<Whitelist> rules = Model.WhitelistModel.List();

            foreach (Whitelist rule in rules)
            {
                Console.WriteLine($"{rule.ID,-10} | {rule.Pattern,-20} | {rule.RegexValid}");
            }
        }

        [Test]
        public void TestCreate()
        {
            // Vars

            DateTime now = DateTime.Now;

            var      pattern = @".*\.apple\.com";
            DateTime expires = now.Add(TimeSpan.FromDays(7));

            List<IPAddress> ips = new List<IPAddress>
            {
                IPAddress.Parse("1.1.1.1"),
                IPAddress.Parse("2.2.2.2")
            };
            List<IPNetwork> subnets = new List<IPNetwork>
            {
                IPNetwork.Parse("10.0.0.0/24"),
                IPNetwork.Parse("10.254.0.0/16"),
                IPNetwork.Parse("2001:0db8::/64"),
                IPNetwork.Parse("2001:0db8::1/128")
            };
            List<PhysicalAddress> macs = new List<PhysicalAddress>
            {
                PhysicalAddress.Parse("00-11-22-33-44-55"),
                PhysicalAddress.Parse("11-22-33-44-55-66")
            };
            List<string> vendors = new List<string>
            {
                "Microsoft Corporation",
                "Cisco Systems Inc."
            };
            List<string> hostnames = new List<string>
            {
                "ajr-desktop",
                "ajr-laptop"
            };

            // Create

            Whitelist whitelist = new Whitelist
            {
                Pattern   = pattern,
                Expires   = expires,
                IPs       = ips,
                Subnets   = subnets,
                MACs      = macs,
                Vendors   = vendors,
                Hostnames = hostnames
            };

            Model.WhitelistModel.Create(whitelist);

            Assert.AreNotEqual(whitelist.ID, 0);

            // Compare

            Whitelist? match = Model.WhitelistModel.Find(whitelist.ID);
            Assert.NotNull(match);
            Debug.Assert(match != null, nameof(match) + " != null");

            DateTime dbRounded  = match.Expires.Value.Trim(TimeSpan.TicksPerSecond);
            DateTime nowRounded = now.Add(TimeSpan.FromDays(7)).Trim(TimeSpan.TicksPerSecond);

            Assert.AreEqual(match.Pattern,   @".*\.apple\.com");
            Assert.AreEqual(dbRounded,       nowRounded);
            Assert.AreEqual(match.IPs,       ips);
            Assert.AreEqual(match.Subnets,   subnets);
            Assert.AreEqual(match.MACs,      macs);
            Assert.AreEqual(match.Vendors,   vendors);
            Assert.AreEqual(match.Hostnames, hostnames);

            Assert.True(match.RegexValid);
        }
    }

    public static class Extensions
    {
        // https://stackoverflow.com/a/21704965/9911189
        public static DateTime Trim(this DateTime date, long ticks)
        {
            return new DateTime(date.Ticks - (date.Ticks % ticks), date.Kind);
        }
    }
}