#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Infrastructure.Controller;
using Infrastructure.Schema;
using Superset.Logging;

namespace Web.Pages
{
    public partial class Configure
    {
        private Config? Config { get; set; }

        private readonly ConfigController _configController = new ConfigController();

        private bool? _configCommitted;

        protected override void OnInitialized()
        {
            Config = ConfigController.Read();

            if (Config == null) return;

            _sources = Config.Sources.Select(v => new Source(v)).ToList();
        }

        private void Commit()
        {
            if (Config != null)
            {
                Config.Sources = _sources.Select(v => v.URL).ToList();

                Common.Logger.Info("Committing config.", new Fields {{"New config", Config.ToString()}});
                bool success = ConfigController.Update(Config);

                _configCommitted = success;
            }
            else
                Common.Logger.Error("Config is null and cannot be committed.",
                    new ArgumentNullException(nameof(Config), "Config is null and cannot be committed."),
                    printStacktrace: true);
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