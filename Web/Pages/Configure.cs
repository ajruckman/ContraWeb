#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Controller;
using Infrastructure.Model;
using Infrastructure.Schema;
using Superset.Logging;
using Superset.Web.State;

namespace Web.Pages
{
    public partial class Configure
    {
        private Config? Config { get; set; }

        private bool  _processing;
        private bool  _userCommitted;
        private bool? _configCommitted;
        private bool? _configReloaded;

        private readonly UpdateTrigger _statusUpdated = new UpdateTrigger();

        protected override void OnInitialized()
        {
            Config = ConfigModel.Read();

            if (Config == null) return;

            _sources = Config.Sources.Select(v => new Source(v)).ToList();
        }

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
                    Config.Sources = _sources.Select(v => v.URL).ToList();

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

        private List<Source> _sources = new List<Source>();
        private string?      _newSource;

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private string? NewSource
        {
            get => _newSource;
            set
            {
                Console.WriteLine("NewSource -> " + value);
                _newSource = value;
            }
        }

        private void RemoveSource(Source source)
        {
            _sources.Remove(source);
        }

        private void AddSource()
        {
            if (_newSource == null) return;

            Source n = new Source(_newSource);

            if (!_sources.Contains(n))
            {
                Console.WriteLine("Add: " + _newSource);
                _sources.Add(n);
            }

            _newSource = null;
        }

        private class Source : IEquatable<Source>
        {
            public Source(string url)
            {
                URL = url;
            }

            public string URL { get; }

            public bool Equals(Source? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return URL == other.URL;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Source) obj);
            }

            public override int GetHashCode()
            {
                return URL.GetHashCode();
            }
        }
    }
}