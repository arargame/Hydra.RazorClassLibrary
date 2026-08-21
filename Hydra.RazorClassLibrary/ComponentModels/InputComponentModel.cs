using Hydra.DataModels;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    //NOT: HtmlInputType artık tek kaynaktan gelir: Hydra.DataModels.HtmlInputType.
    //RCL içindeki kopya enum kaldırıldı (backend MetaColumn metasıyla birebir aynı tip kullanılıyor).

    public interface IInputComponentModel<T> : IHtmlElementComponentWithValue<T>
    {
        HtmlInputType Type { get; set; }
        string TypeName { get; }

        InputComponentModel<T> SetType(HtmlInputType type);
    }

    public class InputComponentModel<T> : HtmlElementComponentWithValue<T>
    {
        [Parameter]
        public HtmlInputType Type { get; set; }

        public string TypeName
        {
            get
            {
                if (Type == HtmlInputType.datetime_local)
                    return "datetime-local";

                return Type.ToString();
            }
        }
        public InputComponentModel() : base()
        {
        }
        
        public override void Initialize()
        {
            base.Initialize();

            SetName("Input");

            SetType(HtmlInputType.text);

            SetLabelValue("Input with value");

            AddClass("form-control");
        }



        public override void FillDebuggerAttributes()
        {
            base.FillDebuggerAttributes();

            Debugger?.Set("Type Name", TypeName);
        }

        public InputComponentModel<T> SetType(HtmlInputType type)
        {
            Type = type;
            return this;
        }
    }
}
