#nullable enable
using Infrastructure.Controller;
using Infrastructure.Schema;

namespace Web.Pages
{
    public partial class Configure
    {
        private Config? Config { get; set; }

        private readonly ConfigController _configController = new ConfigController();

        protected override void OnInitialized()
        {
            Config = _configController.Read();

            if (Config == null) return;
        }
    }
}