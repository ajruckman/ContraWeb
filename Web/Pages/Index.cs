using System;
using FT3;
using Infrastructure.Schema;

namespace Web.Pages
{
    public partial class Index : IDisposable
    {
        private FlareTable<Log> _queryLogTable;

        protected override void OnInitialized()
        {
            _queryLogTable = new FlareTable<Log>(
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

            Common.ContraCoreClient.OnNewLog += _queryLogTable.InvalidateData;

            _queryLogTable.RegisterColumn(nameof(Log.Time),   width: "110px");
            _queryLogTable.RegisterColumn(nameof(Log.Client), width: "120px");
            _queryLogTable.RegisterColumn(nameof(Log.Question));
            _queryLogTable.RegisterColumn(nameof(Log.Action),         width: "100px");
            _queryLogTable.RegisterColumn(nameof(Log.Answers),        width: "100px", shown: false);
            _queryLogTable.RegisterColumn(nameof(Log.ClientHostname), "Hostname",     width: "160px");
            _queryLogTable.RegisterColumn(nameof(Log.ClientVendor),   "Vendor",       width: "160px");
            _queryLogTable.RegisterColumn(nameof(Log.ActionButton),   "Action",       width: "100px");
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

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.Index.Dispose()");
            Common.ContraCoreClient.OnNewLog -= _queryLogTable.InvalidateData;
        }
    }
}