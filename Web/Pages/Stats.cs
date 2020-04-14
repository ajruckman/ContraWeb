using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Newtonsoft.Json;

namespace Web.Pages
{
    public partial class Stats
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            ContraLogClient contraLogClient =
                new ContraLogClient("Host=contralog;Port=9000;Database=contralog;User=contralog_mgr;Password=a6oMaVZZm8nuedax;");

            (Dictionary<long, Dictionary<string, dynamic>> hours, List<string> actions) = contraLogClient.LogActionsPerHour();

            await JSRuntime.InvokeAsync<object>("window.initLogActionsPerHourChart", new object[]
            {
                JsonConvert.SerializeObject(hours),
                JsonConvert.SerializeObject(actions)
            });

            Dictionary<string, long> actionCounts = contraLogClient.LogActionCounts();
            
            await JSRuntime.InvokeAsync<object>("window.initLogActionCountsChart", new object[]
            {
                JsonConvert.SerializeObject(actionCounts)
            });
            
            //

            Dictionary<string, long> leaseVendorCounts = StatsModel.LeaseVendorCounts();

            await JSRuntime.InvokeAsync<object>("window.initLeaseVendorCounts", new object[]
            {
                JsonConvert.SerializeObject(leaseVendorCounts)
            });
            
        }
    }
}