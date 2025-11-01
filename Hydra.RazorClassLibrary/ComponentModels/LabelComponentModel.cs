using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class LabelComponentModel : HtmlElementComponentWithValue<string>
    {
        [Parameter]
        public string? For { get; set; }
        public LabelComponentModel() : base()
        {
        }
        public override void Initialize()
        {
            base.Initialize();

            SetName("Label");

            RemoveClass("form-control");
        }
    }
}
