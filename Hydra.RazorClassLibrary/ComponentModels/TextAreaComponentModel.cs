using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class TextAreaComponentModel : HtmlElementComponentWithValue<string>
    {
        public TextAreaComponentModel() : base()
        {
        }
        public override void Initialize()
        {
            base.Initialize();

            SetName("Text Area");
        }
    }
}
