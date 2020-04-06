using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Infrastructure.Schema;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Web.Components.EditableList
{
    public class EditableList
    {
        public delegate (Validation, MarkupString) Validator(string value);

        public delegate string Transformer(string input);

        private readonly bool         _moveDuplicatesToEnd;
        private readonly bool         _preventDuplicates;
        private readonly Validator?   _validator;
        private readonly Transformer? _transformer;
        private readonly List<string> _values = new List<string>();
        private readonly string       _uid;

        public readonly string DefaultValue;
        public readonly bool   Monospace;
        public readonly string Placeholder;

        public EditableList(
            bool         preventDuplicates   = true,
            bool         moveDuplicatesToEnd = false,
            Validator?   validator           = null,
            Transformer? transformer         = null,
            bool         monospace           = true,
            string       placeholder         = "",
            string       defaultValue        = ""
        )
        {
            _preventDuplicates = preventDuplicates;

            if (!_preventDuplicates && _moveDuplicatesToEnd)
                throw new ArgumentException("'moveDuplicatesToEnd' = true, but 'preventDuplicates' = false",
                    nameof(moveDuplicatesToEnd));

            _moveDuplicatesToEnd = moveDuplicatesToEnd;

            _validator   = validator;
            _transformer = transformer;
            _uid         = Guid.NewGuid().ToString().Replace("-", "");

            Monospace    = monospace;
            Placeholder  = placeholder;
            DefaultValue = defaultValue;
        }

        public ReadOnlyCollection<string> Values => _values.AsReadOnly();

        public bool                        Validated => _validator != null;
        public event Action<string>?       OnAdd;
        public event Action<string>?       OnRemove;
        public event Action<List<string>>? OnUpdate;

        public void Add(string value)
        {
            (Validation, MarkupString)? validation = _validator?.Invoke(value);
            if (validation?.Item1 == Validation.Invalid)
                return;

            string? transformed = _transformer?.Invoke(value);
            if (transformed != null)
                value = transformed;

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
            OnUpdate?.Invoke(_values);
        }

        public void Remove(string value)
        {
            _values.Remove(value);
            OnRemove?.Invoke(value);
            OnUpdate?.Invoke(_values);
        }

        public (Validation, MarkupString)? Validate(string value)
        {
            return _validator?.Invoke(value);
        }

        public RenderFragment Render()
        {
            void Fragment(RenderTreeBuilder builder)
            {
                int seq = -1;

                builder.OpenComponent<__EditableList>(++seq);
                builder.SetKey(_uid);
                builder.AddAttribute(++seq, "EditableList", this);
                builder.CloseComponent();
            }

            return Fragment;
        }
    }

    public static class Tests
    {
        public static void Test()
        {
            EditableList _editableList = new EditableList(true, true);

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

        private class ComplexObject : IEquatable<ComplexObject>
        {
            public ComplexObject(int a, string b)
            {
                A = a;
                B = b;
            }

            private int    A { get; }
            private string B { get; }

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
                if (obj.GetType() != GetType()) return false;
                return Equals((ComplexObject) obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(A, B);
            }
        }
    }
}