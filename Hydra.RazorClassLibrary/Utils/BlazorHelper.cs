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
    }
}
