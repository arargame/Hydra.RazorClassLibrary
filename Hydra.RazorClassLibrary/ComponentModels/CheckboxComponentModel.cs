using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class CheckboxComponentModel : HtmlElementComponent<bool>
    {
        public CheckboxComponentModel() { }

        public override void Initialize()
        {
            base.Initialize();

            SetName("Checkbox");
        }

        public override void FillDebuggerAttributes()
        {
            base.FillDebuggerAttributes();
        }
    }
}
