using System;

namespace Hydra.RazorClassLibrary.Security
{
    /// <summary>
    /// Put on a Blazor page to say which endpoint permission it needs:
    /// <c>@attribute [HydraRequirePermission("Role", "Select")]</c>, or
    /// <c>@attribute [HydraRequirePermission("Role")]</c> for "any action on Role".
    ///
    /// Optional. Without it a page only requires the user to be signed in — which is already the
    /// deny-by-default baseline <see cref="Components.Security.HydraRouteGuard"/> applies to every
    /// route. Add it where a signed-in user should still be kept out of a screen.
    ///
    /// This governs what is DISPLAYED. The server re-checks the same permission on every API call
    /// it makes, so removing this attribute exposes a screen, never the data behind it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class HydraRequirePermissionAttribute : Attribute
    {
        public string Controller { get; }

        /// <summary>Null means "any action on this controller is enough".</summary>
        public string? Action { get; }

        public HydraRequirePermissionAttribute(string controller, string? action = null)
        {
            Controller = controller;
            Action = action;
        }
    }
}
