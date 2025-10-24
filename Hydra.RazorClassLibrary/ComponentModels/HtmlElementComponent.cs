using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public interface IHtmlElementComponent<T>
    {
        Guid Id { get; set; }

        string? Name { get; set; }

        string? Label { get; set; }

        T? Value { get; set; }

        EventCallback<T?> ValueChanged { get; set; }

        bool IsDisabled { get; set; }

        bool IsVisible { get; set; }

        string? CssClass { get; set; }

        Task OnChange(ChangeEventArgs e);//
    }
    public abstract class HtmlElementComponent<T> : ComponentBase,IHtmlElementComponent<T>
    {
        [Parameter]
        public Guid Id { get; set; }
        public string? Name { get; set; }

        // Label veya display adı
        [Parameter]
        public string? Label { get; set; }

        // Input değeri
        [Parameter]
        public T? Value { get; set; }


        // Değer değiştiğinde parent component'e bildirmek için event
        [Parameter]
        public EventCallback<T?> ValueChanged { get; set; }

        // Kullanıcı tarafından pasif hale getirilebilir
        [Parameter]
        public bool IsDisabled { get; set; }

        // Görünürlük kontrolü
        [Parameter]
        public bool IsVisible { get; set; } = true;

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



        protected async Task NotifyValueChangedAsync(T? newValue)
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(newValue);


            Debugger?.Clear();
            FillDebuggerAttributes();
        }

        // Derived component'ler override edebilir
        protected virtual string GetElementCssClass() =>
            $"{CssClass} {(IsDisabled ? "opacity-50 cursor-not-allowed" : "")}".Trim();

        // Component görünür değilse render edilmez
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!IsVisible)
                return;

            base.BuildRenderTree(builder);
        }

        public virtual void FillDebuggerAttributes()
        {
            if (Debugger == null) return;

            Debugger.Set("Id", Id);
            Debugger.Set("Name", Name);
            Debugger.Set("Value", Value);
            Debugger.Set("CssClass", CssClass);
            Debugger.Set("IsDisabled", IsDisabled);
            Debugger.Set("IsVisible", IsVisible);
        }

        public HtmlElementComponent<T> SetContent(RenderFragment content)
        {
            Content = content;
            return this;
        }
        public HtmlElementComponent<T> SetName(string name)
        {
            Name = name;
            return this;
        }

        public virtual async Task OnChange(ChangeEventArgs e)
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
