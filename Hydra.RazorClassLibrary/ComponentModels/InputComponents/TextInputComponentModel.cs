using Hydra.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels.InputComponents
{
    public class TextInputComponentModel : InputComponentModel<string>
    {
        public TextInputComponentModel() { }
        public override void Initialize()
        {
            base.Initialize();
            SetName("Text Input");
            SetType(HtmlInputType.text);
        }
    }
}
