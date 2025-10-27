using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class SpinnerComponentModel : HtmlElementComponent
    {
        public SpinnerComponentModel() : base()
        {

        }
        public override void Initialize()
        {
            base.Initialize();

            SetName("Spinner");
        }
    }
}
