using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using System.Xml.Linq;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public interface IHtmlElementComponent
    {
        string? AdditionalCssStyle { get; set; }

        RenderFragment? ChildContent { get; set; }
        string? CssStyle { get; set; }

        ComponentDebugger? Debugger { get; set; }
        Guid Id { get; set; }
        
        bool IsDisabled { get; set; }
        bool IsVisible { get; set; }
        bool IsTested { get; set; }


        string? LabelValue { get; set; }

        string? Name { get; set; }
        string? Placeholder { get; set; }
        
        Task OnChange(ChangeEventArgs e);
    }

    public interface IHtmlElementComponentWithValue<T> : IHtmlElementComponent
    {
        T? Value { get; set; }
        EventCallback<T?> ValueChanged { get; set; }
    }
    public abstract class HtmlElementComponent : ComponentBase, IHtmlElementComponent
    {
        [Parameter]
        public string? AdditionalCssStyle { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string? CssStyle { get; set; }

        public event Action? ContentChanged;

        [Parameter]
        public Guid Id { get; set; }

        [Parameter]
        public bool IsDisabled { get; set; }

        [Parameter]
        public bool IsReadonly { get; set; } = false;


        [Parameter]
        public bool IsVisible { get; set; } = true;

        [Parameter]
        public bool IsTested { get; set; } = false;



        [Parameter]
        public string? LabelValue { get; set; }


        [Parameter]
        public string? Name { get; set; }

        [Parameter]
        public string? Placeholder { get; set; }

        public RenderFragment? Content { get; set; }

        [Parameter]
        public ComponentDebugger? Debugger { get; set; }
        public HtmlElementComponent()
        {
            Initialize();
        }

        public virtual void Initialize()
        {
            Id = Guid.NewGuid();

            SetName("HtmlElementComponent");
        }


        public virtual string GetElementCssStyle() =>
                                $"{CssStyle + AdditionalCssStyle} " +
                                $"{(IsDisabled ? "opacity-50 cursor-not-allowed" : "")} " +
                                $"{(IsReadonly ? "bg-light" : "")}".Trim();


        public virtual string GetElementId() =>
            Id.ToString();


        public virtual void FillDebuggerAttributes()
        {
            if (Debugger == null) return;

            Debugger.Set("AdditionalCssStyle", AdditionalCssStyle);
            Debugger.Set("CssStyle", CssStyle);
            Debugger.Set("Id", Id);
            Debugger.Set("IsDisabled", IsDisabled);
            Debugger.Set("IsReadonly", IsReadonly);
            Debugger.Set("IsVisible", IsVisible);
            Debugger.Set("IsTested", IsTested);
            Debugger.Set("LabelValue", LabelValue);
            Debugger.Set("Name", Name);
            Debugger.Set("Placeholder", Placeholder);
        }

        public HtmlElementComponent SetAdditionalCssStyle(string additionalCssStyle)
        {
            AdditionalCssStyle = additionalCssStyle;

            return this;
        }

        public HtmlElementComponent SetContent(RenderFragment content)
        {
            Content = content;

            ContentChanged?.Invoke();

            return this;
        }

        public HtmlElementComponent SetCssStyle(string cssStyle)
        {
            CssStyle = cssStyle;

            return this;
        }

        public HtmlElementComponent SetDebugger(ComponentDebugger debugger)
        {
            Debugger = debugger;

            return this;
        }

        public HtmlElementComponent SetLabelValue(string labelValue)
        {
            LabelValue = labelValue;

            return this;
        }
        public HtmlElementComponent SetName(string name)
        {
            Name = name;
            return this;
        }

        public HtmlElementComponent SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }


        protected override void OnParametersSet()
        {
            if (IsTested)
            {
                if (Debugger == null)
                    Debugger = new ComponentDebugger();

                if (!Debugger.Attributes.Any())
                {
                    FillDebuggerAttributes();
                }
            }
        }
        public virtual async Task OnChange(ChangeEventArgs e)
        {
            await Task.CompletedTask;
        }
    }

    public abstract class HtmlElementComponentWithValue<T> : HtmlElementComponent, IHtmlElementComponentWithValue<T>
    {

        [Parameter] public T? Value { get; set; }
        [Parameter] public EventCallback<T?> ValueChanged { get; set; }

        [Parameter]
        public bool WithLabel { get; set; }


        public HtmlElementComponentWithValue() : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            SetName("Html Element Component With Value");

            SetPlaceholder("Leave a comment here");

            SetCssStyle("form-control");
        }


        protected async Task NotifyValueChangedAsync(T? newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);

            Debugger?.Clear();
            FillDebuggerAttributes();
        }


        public override void FillDebuggerAttributes()
        {
            base.FillDebuggerAttributes();
            Debugger?.Set("Value", Value);
            Debugger?.Set("WithLabel",WithLabel);
        }

        //protected override void BuildRenderTree(RenderTreeBuilder builder)
        //{
        //    if (!IsVisible) return;

        //    builder.OpenElement(0, "input");
        //    builder.AddAttribute(1, "id", Id.ToString());
        //    builder.AddAttribute(2, "name", Name);
        //    builder.AddAttribute(3, "class", GetElementCssStyle());
        //    builder.AddAttribute(4, "value", Value?.ToString());
        //    builder.AddAttribute(5, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnChange));
        //    if (IsDisabled)
        //        builder.AddAttribute(6, "disabled", true);
        //    builder.CloseElement();
        //}



        public override async Task OnChange(ChangeEventArgs e)
        {
            object? rawValue = e.Value;
            T? parsedValue = default;

            try
            {
                if (typeof(T) == typeof(bool))
                {
                    bool boolVal = rawValue switch
                    {
                        bool b => b,
                        string s => s.Equals("true", StringComparison.OrdinalIgnoreCase),
                        _ => false
                    };
                    parsedValue = (T)(object)boolVal;
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(rawValue?.ToString(), out int intVal))
                        parsedValue = (T)(object)intVal;
                }
                else if (typeof(T) == typeof(double))
                {
                    if (double.TryParse(rawValue?.ToString(), out double dblVal))
                        parsedValue = (T)(object)dblVal;
                }
                else if (typeof(T) == typeof(string))
                {
                    parsedValue = (T)(object?)rawValue?.ToString()!;
                }
                else
                {
                    // Fallback: direkt cast etmeyi dene
                    parsedValue = (T?)Convert.ChangeType(rawValue, typeof(T));
                }
            }
            catch
            {
                // Dönüştürme hatası varsa default kalır
            }

            await NotifyValueChangedAsync(parsedValue);
        }


        protected override void OnParametersSet()
        {
            if (WithLabel)
            {
                LabelValue = "Comments";
            }

            base.OnParametersSet();
        }
    }
        
    
}
