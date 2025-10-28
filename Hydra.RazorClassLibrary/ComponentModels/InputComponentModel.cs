using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public enum HtmlInputType
    {
        button,
        checkbox,
        color,
        date,
        datetime_local,
        email,
        file,
        hidden,
        image,
        month,
        number,
        password,
        radio,
        range,
        reset,
        search,
        submit,
        tel,
        text,
        time,
        url,
        week,
    }

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

            SetCssStyle("form-control");
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
