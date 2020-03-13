using System.Threading.Tasks;
using ColorSet.Components;

namespace Web.Shared
{
    public partial class MainLayout
    {
        private readonly Configuration _configuration = new Configuration();
        private          ThemeLoader   _themeLoader;

        protected override void OnInitialized()
        {
            _themeLoader = new ThemeLoader(LocalStorage, _configuration.ResourceManifests, "Dark");

            _themeLoader.OnComplete += StateHasChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await _themeLoader.Load();
        }
    }
}