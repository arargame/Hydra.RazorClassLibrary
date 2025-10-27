using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Diagnostics;
using System.Xml.Linq;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public interface IHtmlElementComponent
    {
        Guid Id { get; set; }
        string? Name { get; set; }
        string? Label { get; set; }
        bool IsDisabled { get; set; }
        bool IsVisible { get; set; }
        string? CssClass { get; set; }
        RenderFragment? ChildContent { get; set; }

        Task OnChange(ChangeEventArgs e);
    }

    public interface IHtmlElementComponentWithValue<T> : IHtmlElementComponent
    {
        T? Value { get; set; }
        EventCallback<T?> ValueChanged { get; set; }
    }
    public abstract class HtmlElementComponent : ComponentBase, IHtmlElementComponent
    {

        public event Action? ContentChanged;
        [Parameter]
        public Guid Id
        {
            get;
            set;
        }

        [Parameter]
        public string? Name { get; set; }

        // Label veya display adı
        [Parameter]
        public string? Label { get; set; }

        //// Input değeri
        //[Parameter]
        //public T? Value { get; set; }


        //// Değer değiştiğinde parent component'e bildirmek için event
        //[Parameter]
        //public EventCallback<T?> ValueChanged { get; set; }

        // Kullanıcı tarafından pasif hale getirilebilir
        [Parameter]
        public bool IsDisabled { get; set; }

        // Görünürlük kontrolü
        [Parameter]
        public bool IsVisible { get; set; } = true;

        [Parameter]
        public bool IsTested { get; set; } = false;

        // Ek CSS sınıfları
        [Parameter]
        public string? CssClass { get; set; }

        // İçerik (örneğin label veya slot)
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

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



        //protected async Task NotifyValueChangedAsync(T? newValue)
        //{
        //    Value = newValue;
        //    await ValueChanged.InvokeAsync(newValue);

        //    Debugger?.Clear();
        //    FillDebuggerAttributes();
        //}

        // Derived component'ler override edebilir
        public virtual string GetElementCssClass() =>
            $"{CssClass} {(IsDisabled ? "opacity-50 cursor-not-allowed" : "")}".Trim();

        // Component görünür değilse render edilmez
        //protected override void BuildRenderTree(RenderTreeBuilder builder)
        //{
        //    if (!IsVisible)
        //        return;

        //    builder.OpenElement(0, "HtmlElementComponent");
        //    builder.AddAttribute(1, "id", Id.ToString());
        //    builder.AddAttribute(2, "name", Name);

        //    builder.AddAttribute(4, "class", GetElementCssClass());

        //    if (Value is not null)
        //        builder.AddAttribute(5, "value", Value);

        //    if (IsDisabled)
        //        builder.AddAttribute(6, "disabled", true);

        //    builder.AddAttribute(7, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnChange));

        //    builder.CloseElement();
        //}

        public virtual void FillDebuggerAttributes()
        {
            if (Debugger == null) return;

            Debugger.Set("Id", Id);
            Debugger.Set("Name", Name);
            //Debugger.Set("Value", Value);
            Debugger.Set("CssClass", CssClass);
            Debugger.Set("IsDisabled", IsDisabled);
            Debugger.Set("IsVisible", IsVisible);
        }

        public HtmlElementComponent SetContent(RenderFragment content)
        {
            Content = content;

            ContentChanged?.Invoke();

            return this;
        }

        public HtmlElementComponent SetDebugger(ComponentDebugger debugger)
        {
            Debugger = debugger;

            return this;
        }
        public HtmlElementComponent SetName(string name)
        {
            Name = name;
            return this;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            //Initialize();
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

        public HtmlElementComponentWithValue() : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            SetName("Html Element Component With Value");
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
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!IsVisible) return;

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "id", Id.ToString());
            builder.AddAttribute(2, "name", Name);
            builder.AddAttribute(3, "class", GetElementCssClass());
            builder.AddAttribute(4, "value", Value?.ToString());
            builder.AddAttribute(5, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, OnChange));
            if (IsDisabled)
                builder.AddAttribute(6, "disabled", true);
            builder.CloseElement();
        }

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
    }
        
    
}
