using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class ButtonComponentModel : HtmlElementComponent, IHtmlElementWithText
    {
        [Parameter]
        public string? Text { get; set; }

        public ButtonComponentModel() :base()
        {
        
        }

        public override void Initialize()
        {
            base.Initialize();

            SetName("Button");

            AddClass("btn btn-primary btn-sm");
        }

        public IHtmlElementWithText SetText(string text)
        {
            Text = text;

            return this;
        }
    }
}
