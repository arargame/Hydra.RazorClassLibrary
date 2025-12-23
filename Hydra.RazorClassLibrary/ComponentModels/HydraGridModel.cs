using Hydra.RazorClassLibrary.ComponentModels;
using Microsoft.AspNetCore.Components;

namespace Hydra.RazorClassLibrary.ComponentModels
{
    public class HydraGridModel<TItem> : HtmlElementComponent
    {
        [Parameter]
        public IEnumerable<TItem>? Items { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ChildContent { get; set; }

        [Parameter] // Columns header definition if needed, or derived from TItem reflection
        public RenderFragment? HeaderContent { get; set; }

        public HydraGridModel() : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            SetName("HydraGrid");
        }
    }
}
