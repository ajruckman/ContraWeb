#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Web.Pages
{
    public partial class Configure
    {
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