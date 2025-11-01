using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public partial class HtmlElementComponent 
    {
        [Parameter] public string? CssClass { get; set; }
        [Parameter] public string? CssStyle { get; set; }

        private readonly Dictionary<string, string> _styles = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> _stateStyles = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _classes = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _stateClasses = new(StringComparer.OrdinalIgnoreCase);

        // === CSS CLASS ===
        public void AddClass(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return;

            var parts = className.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
                _classes.Add(part.Trim());
        }

        public void RemoveClass(string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                return;

            var parts = className.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
                _classes.Remove(part.Trim());
        }

        public bool HasClass(string className) =>
            _classes.Contains(className);

        public string GetCssClass()
        {
            // state class'larını güncelle
            UpdateStateClassesAndStyles();

            // CssClass parametresi verilmişse (override durumu)
            if (!string.IsNullOrWhiteSpace(CssClass))
            {
                var merged = new HashSet<string>(
                    CssClass.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                    StringComparer.OrdinalIgnoreCase
                );

                // CssClass içinde state class’lar yoksa ekle
                foreach (var stateCls in _stateClasses)
                {
                    if (!merged.Contains(stateCls))
                        merged.Add(stateCls);
                }

                return string.Join(" ", merged);
            }

            // CssClass verilmemişse: _classes + _stateClasses birleşimi
            var result = new HashSet<string>(_classes, StringComparer.OrdinalIgnoreCase);
            foreach (var stateCls in _stateClasses)
                result.Add(stateCls);

            return string.Join(" ", result);
        }


        // === CSS STYLE ===
        public void AddStyle(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (string.IsNullOrWhiteSpace(value))
            {
                _styles.Remove(key);
                return;
            }

            _styles[key] = value;
        }

        public void RemoveStyle(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _styles.Remove(key);
        }

        public bool HasStyle(string key) =>
            _styles.ContainsKey(key);

        public string? GetStyleValue(string key) =>
            _styles.TryGetValue(key, out var value) ? value : null;

        public string GetCssStyle()
        {
            // state style’ları güncelle
            UpdateStateClassesAndStyles();

            // CssStyle parametresi verilmişse override
            if (!string.IsNullOrWhiteSpace(CssStyle))
            {
                var userStyles = CssStyle.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToDictionary(
                        s => s.Split(':')[0].Trim(),
                        s => s.Split(':')[1].Trim(),
                        StringComparer.OrdinalIgnoreCase
                    );

                foreach (var kv in _stateStyles)
                {
                    if (!userStyles.ContainsKey(kv.Key))
                        userStyles[kv.Key] = kv.Value;
                }

                return string.Join("; ", userStyles.Select(kv => $"{kv.Key}: {kv.Value}"));
            }

            // Kullanıcı style vermediyse _styles + _stateStyles birleşimi
            var merged = new Dictionary<string, string>(_styles, StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _stateStyles)
                merged[kv.Key] = kv.Value;

            return string.Join("; ", merged.Select(kv => $"{kv.Key}: {kv.Value}"));
        }


        private void UpdateStateClassesAndStyles()
        {
            _stateClasses.Clear();
            _stateStyles.Clear();

            if (IsDisabled)
            {
                // class tabanlı stiller
                _stateClasses.Add("opacity-50");
                _stateClasses.Add("cursor-not-allowed");

                // inline style fallback
                //_stateStyles["pointer-events"] = "none";
                //_stateStyles["opacity"] = "0.5";
            }

            if (IsReadonly)
            {
                _stateClasses.Add("bg-light");
                //_stateStyles["background-color"] = "#f8f9fa";
            }
        }


        //public Dictionary<string,string> GetCssStyleAsDictionary()=> _styles;


        //public void CopyStateTo(HtmlElementComponent target)
        //{
        //    if (target == null) return;

        //    //target.IsDisabled = this.IsDisabled;
        //    //target.IsReadonly = this.IsReadonly;
        //    //target.IsVisible = this.IsVisible;

        //    foreach (var cls in _classes)
        //        target.AddClass(cls);

        //    foreach (var (key, value) in _styles)
        //        target.AddStyle(key, value);
        //}
    }
}
