using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Model;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Superset.Logging;
using Superset.Web.State;
using Web.Components.EditableList;

namespace Web.Pages
{
    [Authorize(Roles ="Administrator")]
    public partial class Configure
    {
        private Config? Config { get; set; }

        private bool          _processing;
        private bool          _userCommitted;
        private bool?         _configCommitted;
        private bool?         _configReloaded;
        private EditableList? _searchDomainList;

        private readonly UpdateTrigger _statusUpdated = new UpdateTrigger();

        protected override void OnInitialized()
        {
            Config = ConfigModel.Read();

            if (Config == null) return;

            _sources = Config.Sources.Select(v => new Source(v)).ToList();

            _searchDomainList = new EditableList(
                moveDuplicatesToEnd: true,
                validator: s =>
                {
                    if (string.IsNullOrEmpty(s))
                        return (Validation.Undefined, new MarkupString());

                    if (_validHostnameRegex.IsMatch(s))
                        return (Validation.Valid, new MarkupString("Valid search domain"));

                    return (Validation.Invalid, new MarkupString("Invalid search domain"));
                },
                placeholder: "Add a search domain"
            );

            Config.SearchDomains.ForEach(_searchDomainList.Add);
        }

        // https://stackoverflow.com/a/106223/9911189
        private readonly Regex _validHostnameRegex =
            new Regex(
                @"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$",
                RegexOptions.Compiled);

        private void Commit()
        {
            _processing    = true;
            _userCommitted = false;
            _statusUpdated.Trigger();

            Task.Run(async () =>
            {
                Thread.Sleep(100);

                if (Config != null)
                {
                    Config.Sources       = _sources.Select(v => v.URL).ToList();
                    Config.SearchDomains = _searchDomainList?.Values.ToList();

                    Common.Logger.Info("Committing config.", new Fields {{"New config", Config.ToString()}});

                    try
                    {
                        bool commitSucceeded = ConfigModel.Update(Config);
                        _configCommitted = commitSucceeded;
                    }
                    catch
                    {
                        _configCommitted = false;
                    }

                    try
                    {
                        bool reloadSucceeded = await Common.ContraCoreClient.ReloadConfig();
                        _configReloaded = reloadSucceeded;
                    }
                    catch
                    {
                        _configReloaded = false;
                    }

                    _processing    = false;
                    _userCommitted = true;
                    _statusUpdated.Trigger();
                }
                else
                    Common.Logger.Error("Config is null and cannot be committed.",
                        new ArgumentNullException(nameof(Config), "Config is null and cannot be committed."),
                        printStacktrace: true);
            });
        }
    }
}