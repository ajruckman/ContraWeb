using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Superset.Web.State;

namespace Infrastructure.Validation
{
    public class Validator
    {
        private readonly ResultGetter? _overallValidationGetter;

        public delegate Validation ResultGetter();

        private readonly Dictionary<string, ResultGetter> _fields = new Dictionary<string, ResultGetter>();

        private readonly UpdateTrigger _refreshTrigger = new UpdateTrigger();

        public Validator(ResultGetter? overallValidationGetter = null)
        {
            _overallValidationGetter = overallValidationGetter;
        }

        public void Refresh()
        {
            _refreshTrigger.Trigger();
        }

        public void Register(string field, ResultGetter resultGetter)
        {
            _fields.Add(field, resultGetter);
        }

        private RenderFragment RenderResult(Validation result)
        {
            int seq = -1;

            void Fragment(RenderTreeBuilder builder)
            {
                builder.OpenElement(++seq, "div");
                builder.AddAttribute(++seq, "class", "ValidationResultContainer");

                builder.OpenComponent<TriggerWrapper>(++seq);
                builder.AddAttribute(++seq, "Trigger",   _refreshTrigger);
                builder.AddAttribute(++seq, "Protected", true);
                builder.AddAttribute(++seq, "ChildContent", (RenderFragment) (builder2 =>
                {
                    builder2.OpenElement(++seq, "div");
                    builder2.AddAttribute(++seq, "class", result.Result switch
                    {
                        ValidationResult.Invalid => "XMark",
                        ValidationResult.Warning => "ExclamationMark",
                        ValidationResult.Valid   => "CheckMark",
                        _                        => ""
                    });
                    builder2.CloseElement();

                    builder2.OpenElement(++seq, "span");
                    builder2.AddAttribute(++seq, "class", "ValidationText");
                    builder2.AddContent(++seq, result.Message);
                    builder2.CloseElement();
                }));
                builder.CloseComponent();

                builder.CloseElement();
            }

            return Fragment;
        }
        
        public RenderFragment RenderFieldResult(string field)
        {
            Validation result = _fields[field].Invoke();

            return RenderResult(result);
        }

        public RenderFragment RenderOverallResult()
        {
            if (_overallValidationGetter == null)
                throw new ArgumentNullException(nameof(_overallValidationGetter));

            Validation result = _overallValidationGetter.Invoke();

            return RenderResult(result);
        }
    }
}