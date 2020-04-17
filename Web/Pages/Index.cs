using System;
using System.Threading.Tasks;
using FlareTables;
using Infrastructure.Schema;
using Infrastructure.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Subsegment.Bits;
using Subsegment.Constructs;
using Superset.Web.State;

namespace Web.Pages
{
    public partial class Index : IDisposable
    {
        [CascadingParameter]
        private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

        [CascadingParameter]
        public string ClientIP { get; set; }

        private readonly UpdateTrigger _statusChangeTrigger = new UpdateTrigger();

        private          UserRole.Roles?  _clientRole;
        private          FlareTable<Log>? _queryLogTable;
        private          string?          _errorMessage;
        private          Subheader?       _subheader;

        protected override void OnInitialized()
        {
            BuildTable();

            Common.ContraCoreClient.OnNewLog       += OnNewLog;
            Common.ContraCoreClient.OnStatusChange += OnContraCoreClientStatusChange;

            ContraWebAuthStateProvider.AuthenticationStateChanged += _ => BuildTable();
        }

        private void OnNewLog(Log? v)
        {
            if (v != null)
            {
                _queryLogTable!.PrependRow(v, 1000);
            }
            else
            {
                _queryLogTable!.InvalidateData();
            }
        }

        private void OnContraCoreClientStatusChange()
        {
            Console.WriteLine("Status change");
            BuildSubheader();
            _statusChangeTrigger.Trigger();
        }

        private void BuildSubheader()
        {
            _subheader = new Subheader();
            _subheader.Add(new Space(10));
            _subheader.Add(new Title("Real-time query log"));
            _subheader.Add(new Space(10));

            if (Common.ContraCoreClient.Connected)
            {
                _subheader.Add(new Chip("Connected to ContraCore",
                    backgroundColor: "var(--ColorSet_ThemeColor_SolidGreen_70)", textColor: "white"));
            }
            else
            {
                _subheader.Add(new Chip("Disconnected from ContraCore; log out-of-date",
                    backgroundColor: "var(--ColorSet_ThemeColor_SolidRed_30)", textColor: "white"));
            }

            if (_clientRole == UserRole.Roles.Undefined)
            {
                _subheader.Add(new Chip("Restricted view",
                    backgroundColor: "var(--ColorSet_ThemeColor_SolidBlue_30)", textColor: "white"));
            }
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

        private void BuildTable()
        {
            _clientRole = Utility.GetRole(AuthenticationStateTask ?? throw new Exception("AuthenticationStateTask was not set"))
                                 .Result;

            BuildSubheader();

            Console.WriteLine(_clientRole + " => " + ClientIP);
            if (string.IsNullOrEmpty(ClientIP) && _clientRole == UserRole.Roles.Undefined)
            {
                _errorMessage = "Could not read client IP and user is unauthenticated; not showing data";
                return;
            }

            _queryLogTable = new FlareTable<Log>(
                () => Common.ContraCoreClient.Data(_clientRole == UserRole.Roles.Undefined ? ClientIP : null),
                ValueGetter,
                monospace: true,
                // fixedLayout:true,
                rowColorGetter: row =>
                {
                    if (row.Action.StartsWith("block"))
                        return RowColor.Red;
                    if (row.Action.StartsWith("pass"))
                        return RowColor.Green;

                    return RowColor.Undefined;
                }
            );

            // Common.ContraCoreClient.OnNewLog += _queryLogTable.InvalidateData;

            _queryLogTable.RegisterColumn(nameof(Log.Time),   width: "80px");
            _queryLogTable.RegisterColumn(nameof(Log.Client), width: "120px");
            _queryLogTable.RegisterColumn(nameof(Log.Question));
            _queryLogTable.RegisterColumn(nameof(Log.Action),         width: "100px");
            _queryLogTable.RegisterColumn(nameof(Log.Answers),        width: "100px", shown: false);
            _queryLogTable.RegisterColumn(nameof(Log.ClientHostname), "Hostname",     width: "160px");
            _queryLogTable.RegisterColumn(nameof(Log.ClientVendor),   "Vendor",       width: "195px");

            if (_clientRole != UserRole.Roles.Undefined)
                _queryLogTable.RegisterColumn(nameof(Log.ActionButton), "", width: "60px", filterable: false,
                    sortable: false);

            StateHasChanged();
        }

        public void Dispose()
        {
            Common.Logger.Debug("Web.Pages.Index.Dispose()");
            Common.ContraCoreClient.OnNewLog       -= OnNewLog;
            Common.ContraCoreClient.OnStatusChange -= OnContraCoreClientStatusChange;

            ContraWebAuthStateProvider.AuthenticationStateChanged += _ => BuildTable();
        }
    }
}