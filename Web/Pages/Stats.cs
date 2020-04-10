using System.Collections.Generic;
using System.Threading.Tasks;
using Infrastructure.Controller;
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
                new ContraLogClient("Host=10.3.0.16;Port=9000;Database=contralog;User=contralogmgr;Password=contralogmgr;");

            (Dictionary<long, Dictionary<string, dynamic>> hours, List<string> actions) = contraLogClient.LogActionsPerHour();

            await JSRuntime.InvokeAsync<object>("window.initChart", new object[]
            {
                JsonConvert.SerializeObject(hours),
                JsonConvert.SerializeObject(actions)
            });
        }
    }
}