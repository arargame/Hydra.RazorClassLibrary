using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class ComponentDebugger
    {
        public event Action? OnChange;
        public Dictionary<string,string> Attributes { get; set; } = new Dictionary<string, string>();
        public ComponentDebugger() { }

        public void Set(string key, object? value)
        {
            if (value == null)
                Attributes[key] = "null";
            else
                Attributes[key] = value.ToString()!;

            OnChange?.Invoke();
        }

        public string? Get(string key)
        {
            return Attributes.TryGetValue(key, out var val) ? val : null;
        }

        public void Clear() 
        {
            Attributes.Clear();

            OnChange?.Invoke();
        }
    }
}
