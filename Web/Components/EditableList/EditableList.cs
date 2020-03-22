#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Web.Components.EditableList
{
    public class EditableList
    {
        public event Action<string>? OnAdd;
        public event Action<string>? OnRemove;

        public readonly bool Monospace;

        private readonly bool _preventDuplicates;
        private readonly bool _moveDuplicatesToEnd;

        private readonly List<string> _values = new List<string>();

        public EditableList(bool monospace = true, bool preventDuplicates = true, bool moveDuplicatesToEnd = false)
        {
            Monospace = monospace;

            _preventDuplicates = preventDuplicates;

            if (!_preventDuplicates && _moveDuplicatesToEnd)
                throw new ArgumentException("'moveDuplicatesToEnd' = true, but 'preventDuplicates' = false",
                    nameof(moveDuplicatesToEnd));

            _moveDuplicatesToEnd = moveDuplicatesToEnd;
        }

        public ReadOnlyCollection<string> Values => _values.AsReadOnly();

        public void Add(string value)
        {
            if (_preventDuplicates && _values.Contains(value))
            {
                if (_moveDuplicatesToEnd)
                {
                    _values.Remove(value);
                    _values.Add(value);
                }

                return;
            }

            _values.Add(value);
            OnAdd?.Invoke(value);
        }

        public void Remove(string value)
        {
            _values.Remove(value);
            OnRemove?.Invoke(value);
        }

        public RenderFragment Render()
        {
            void Fragment(RenderTreeBuilder builder)
            {
                int seq = -1;

                builder.OpenComponent<__EditableList>(++seq);
                builder.AddAttribute(++seq, "EditableList", this);
                builder.CloseComponent();
            }

            return Fragment;
        }
    }

    public static class Tests
    {
        private class ComplexObject : IEquatable<ComplexObject>
        {
            public int    A { get; }
            public string B { get; }

            public ComplexObject(int a, string b)
            {
                A = a;
                B = b;
            }

            public bool Equals(ComplexObject? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return A == other.A && B == other.B;
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ComplexObject) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B);
            }
        }

        public static void Test()
        {
            EditableList _editableList = new EditableList(true, true, true);

            _editableList.Add("1");
            _editableList.Add("2");
            _editableList.Add("3");
            Debug.Assert(_editableList.Values.Count == 3);

            _editableList.Remove("1");
            Debug.Assert(_editableList.Values.Count == 2);

            _editableList.Add("2");
            Debug.Assert(_editableList.Values.Count  == 2);
            Debug.Assert(_editableList.Values.Last() == "2");
        }
    }
}