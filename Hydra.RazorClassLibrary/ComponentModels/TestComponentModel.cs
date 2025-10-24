using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    internal class TestComponentModel : HtmlElementComponent<string>
    {
        public override void Initialize()
        {
            base.Initialize();

            SetName("TestComponentModel");

            Value = "Initial Value";
            //testto commit
        }
    }
}
