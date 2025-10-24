using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hydra.RazorClassLibrary.Utils
{
    public class BlazorHelper
    {
        public static RenderFragment GetRenderFragment(Type type, Dictionary<string, object>? parameters = null)
        {
            if (!typeof(ComponentBase).IsAssignableFrom(type))
                throw new ArgumentException("Type must be a Blazor component", nameof(type));

            RenderFragment renderFragment = builder =>
            {
                int sequence = 0; 

                builder.OpenComponent(sequence++, type); 

                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {

                        builder.AddAttribute(sequence++, parameter.Key, parameter.Value);
                    }
                }

                builder.CloseComponent(); 
            };

            return renderFragment;
        }

        public static RenderFragment GetSampleRenderFragment()
        {
            var simpleFragment = new RenderFragment(builder =>
                {
                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "class", "text-green-600 font-bold");
                    builder.AddContent(2, "Bu basit bir RenderFragment içeriğidir!");
                    builder.CloseElement();
                });

            return simpleFragment;
        }
    }
}
