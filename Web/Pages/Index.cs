using System;
using System.Threading.Tasks;
using FT3;
using Infrastructure.Schema;

namespace Web.Pages
{
    public partial class Index : IDisposable
    {
        private FlareTable<Log> _flareTable1;

        protected override void OnInitialized()
        {
            _flareTable1 = new FlareTable<Log>(
                () => Common.ContraCoreClient.Data(),
                ValueGetter,
                monospace: true,
                rowColorGetter: row =>
                {
                    if (row.Action.StartsWith("block"))
                        return RowColor.Red;
                    if (row.Action.StartsWith("pass"))
                        return RowColor.Green;
                    return RowColor.Undefined;
                }
            );

            Common.ContraCoreClient.OnNewLog += _flareTable1.InvalidateData;

            _flareTable1.RegisterColumn(nameof(Log.Time),   width: "110px");
            _flareTable1.RegisterColumn(nameof(Log.Client), width: "120px");
            _flareTable1.RegisterColumn(nameof(Log.Question));
            _flareTable1.RegisterColumn(nameof(Log.Action),         width: "100px");
            _flareTable1.RegisterColumn(nameof(Log.Answers),        width: "100px", shown: false);
            _flareTable1.RegisterColumn(nameof(Log.ClientHostname), "Hostname",     width: "160px");
            _flareTable1.RegisterColumn(nameof(Log.ClientVendor),   "Vendor",       width: "160px");
        }

        private string ValueGetter(Log log, string id)
        {
            return id switch
            {
                nameof(Log.Time)           => log.Time.ToString("HH:mm:ss.fff"),
                nameof(Log.Client)         => log.Client,
                nameof(Log.Question)       => log.Question,
                nameof(Log.Action)         => log.Action,
                nameof(Log.Answers)        => log.Answers != null ? string.Join(", ", log.Answers) : "",
                nameof(Log.ClientHostname) => log.ClientHostname,
                nameof(Log.ClientVendor)   => log.ClientVendor,
                _                          => ""
            };
        }

        protected override async Task OnInitializedAsync() { }

        public void Dispose()
        {
            Common.ContraCoreClient.OnNewLog -= _flareTable1.InvalidateData;
        }
    }
}