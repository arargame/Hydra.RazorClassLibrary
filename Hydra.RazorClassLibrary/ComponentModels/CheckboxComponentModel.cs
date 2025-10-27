using Hydra.DataModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class CheckboxComponentModel : InputComponentModel<bool>
    {
        public CheckboxComponentModel() : base()
        {
        
        }

        public override void Initialize()
        {
            base.Initialize();

            SetName("Checkbox");

            SetType(HtmlInputType.checkbox);
        }
    }
}
