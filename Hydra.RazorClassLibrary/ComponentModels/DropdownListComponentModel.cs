using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class DropdownListComponentModel : HtmlElementComponentWithValue<DropdownListOption>
    {
        [Parameter]
        public List<DropdownListOption> Options { get; set; } = new List<DropdownListOption>();

        [Parameter]
        public EventCallback<DropdownListOption> OnSelectedOptionChanged { get; set; }

        public DropdownListOption SelectedOption
        {
            get { return Value!; }
        }

        [Parameter]
        public string SelectPlaceholderText { get; set; } = "Select";

        [Parameter]
        public bool WithSelectPlaceholder { get; set; }

        public DropdownListComponentModel() : base()
        {
        }
        public override void Initialize()
        {
            base.Initialize();

            SetName("Dropdown List");

            WithSelectPlaceholder = false;
        }

        public DropdownListComponentModel SetOptions(params DropdownListOption[] options)
        {
            return SetOptions(WithSelectPlaceholder, options);
        }

        public DropdownListComponentModel SetOptions(bool withSelectPlaceholder = false, params DropdownListOption[] options)
        {
            Options = new List<DropdownListOption>();

            if (withSelectPlaceholder)
            {
                var placeholder = new DropdownListOption(key: "", value: SelectPlaceholderText, isSelected: true);

                Options.Add(placeholder);

                Value = placeholder; 
            }

            Options.AddRange(options);

            if (!withSelectPlaceholder && Value == null && Options.Any())
            {
                Value = Options.First();

                Value.IsSelected = true;
            }

            return this;
        }


        public void SelectOption(DropdownListOption option)
        {
            foreach (var o in Options)
                o.IsSelected = false;

            option.IsSelected = true;
            Value = option;
        }

        public async Task OnSelectedValueChanged(ChangeEventArgs e)
        {
            var selectedKey = e.Value?.ToString();

            var selectedOption = Options.FirstOrDefault(o => o.Key == selectedKey);

            if (selectedOption != null)
            {
                SelectOption(selectedOption);
                await NotifyValueChangedAsync(selectedOption);

                if (OnSelectedOptionChanged.HasDelegate)
                    await OnSelectedOptionChanged.InvokeAsync(selectedOption);
            }
        }
    }

    public class DropdownListOption
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; } = null;
        public bool IsSelected { get; set; }

        public DropdownListOption(string key, string? value = null, bool isSelected = false)
        {
            Key = key;
            Value = value;
            IsSelected = isSelected;
        }
    }
}
