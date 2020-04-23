using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Component;
using FlareTables;
using Infrastructure.Controller;
using Infrastructure.Model;
using Newtonsoft.Json;
using Subsegment.Bits;
using Subsegment.Constructs;

namespace Web.Pages
{
    public partial class Stats
    {
        private FlareTable<(string, int)>?                 _topBlockedTable;
        private FlareTable<(string, int)>?                 _topPassedTable;
        private FlareTable<(string, string, string, int)>? _topClientsTable;

        private Subheader? _graphicSubheader;

        protected override void OnInitialized()
        {
            _graphicSubheader = new Subheader(new List<IBit>
            {
                new Space(10),
                new Title("Server statistics"),
                new Space(15),
                new Separator(),
                new Space(15),
                new TextLine("Last 7 days")
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            #if DOCKER
            var logClient =
                new ContraLogClient("Host=contralog;Port=9000;Database=contralog;User=contralog_mgr;Password=a6oMaVZZm8nuedax;");
            #else
            var logClient =
                new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralog_mgr;Password=a6oMaVZZm8nuedax;");
            #endif

            (Dictionary<long, Dictionary<string, dynamic>> hours, List<string> actions) =
                logClient.LogActionsPerHour();

            await JSRuntime.InvokeAsync<object>("window.initLogActionsPerHourChart", new object[]
            {
                JsonConvert.SerializeObject(hours),
                JsonConvert.SerializeObject(actions)
            });

            Dictionary<string, int> actionCounts = logClient.LogActionCounts();

            await JSRuntime.InvokeAsync<object>("window.initLogActionCountsChart", new object[]
            {
                JsonConvert.SerializeObject(actionCounts)
            });

            //

            dynamic leaseVendorCounts = StatsModel.LeaseVendorCounts(2);

            await JSRuntime.InvokeAsync<object>("window.initLeaseVendorCounts", new object[]
            {
                JsonConvert.SerializeObject(leaseVendorCounts)
            });

            Console.WriteLine(JsonConvert.SerializeObject(leaseVendorCounts));
            
            //

            var topBlocked =
                logClient.TopBlocked(DateTime.Now.Subtract(TimeSpan.FromDays(7)), 100);

            _topBlockedTable = new FlareTable<(string domain, int c)>(
                () => topBlocked,
                valueGetter: (data, id) => "",
                monospace: true
            );

            _topBlockedTable.RegisterColumn("domain", displayName: "Domain");
            _topBlockedTable.RegisterColumn("c",      displayName: "Count", width: "75px");

            //

            var topPassed =
                logClient.TopPassed(DateTime.Now.Subtract(TimeSpan.FromDays(7)), 100);

            _topPassedTable = new FlareTable<(string domain, int c)>(
                () => topPassed,
                valueGetter: (data, id) => "",
                monospace: true
            );

            _topPassedTable.RegisterColumn("domain", displayName: "Domain");
            _topPassedTable.RegisterColumn("c",      displayName: "Count", width: "75px");

            //

            var topClients =
                logClient.TopClients(DateTime.Now.Subtract(TimeSpan.FromDays(7)), 100);

            _topClientsTable = new FlareTable<(string, string, string, int)>(
                () => topClients,
                valueGetter: (data, id) => "",
                monospace: true
            );

            _topClientsTable.RegisterColumn("client",   displayName: "Client");
            _topClientsTable.RegisterColumn("hostname", displayName: "Hostname");
            _topClientsTable.RegisterColumn("vendor",   displayName: "Vendor");
            _topClientsTable.RegisterColumn("c",        displayName: "Count", width: "75px");

            //

            StateHasChanged();
        }
    }
}