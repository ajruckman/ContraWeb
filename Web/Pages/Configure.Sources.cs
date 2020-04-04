

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Web.Pages
{
    public partial class Configure
    {
        private List<Source>     _sources = new List<Source>();
        private string?          _newSource;
        private ElementReference _newSourceInput;

        protected override void OnAfterRender(bool firstRender)
        {
            if (!firstRender) return;
        }

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

        private async Task RemoveSource(Source source)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", new object[] {$"Delete source '{source.URL}'?"});
            if (confirmed)
                _sources.Remove(source);
        }

        private async Task AddSource()
        {
            if (_newSource == null) return;

            Source n = new Source(_newSource);

            if (!_sources.Contains(n))
            {
                Console.WriteLine("Add: " + _newSource);
                _sources.Add(n);
            }

            _newSource = null;

            await Superset.Web.Utilities.Utilities.FocusElement(JSRuntime, _newSourceInput);
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

        private void OnNewSourceKeyUp(KeyboardEventArgs args)
        {
            if (args.Key == "Enter") AddSource();
        }
    }
}