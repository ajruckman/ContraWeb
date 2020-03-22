using System.Collections.Generic;
using System.Threading.Tasks;
using API.Component;
using ColorSet.Components;
using Subsegment.Bits;
using Subsegment.Constructs;

namespace Web.Shared
{
    public partial class MainLayout
    {
        private readonly Configuration _configuration = new Configuration();

        private Header      _header;
        private ThemeLoader _themeLoader;

        protected override void OnInitialized()
        {
            _themeLoader = new ThemeLoader(LocalStorage, _configuration.ResourceManifests, "Dark");

            _themeLoader.OnComplete += StateHasChanged;

            _header = new Header(new List<IBit>
            {
                new Space(10),
                new Title("ContraWeb", "/"),
                new Filler(),
                new PageLink("Configure", "/configure"),
                new Space(),
                new Separator(),
                new Space(),
                new PageLink("Rules",     "/rules"),
                new Space(),
                new Separator(),
                new Space(),
                new PageLink("Generate", "/rulegen"),
                new Space(10)
            });
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await _themeLoader.Load();
        }
    }
}