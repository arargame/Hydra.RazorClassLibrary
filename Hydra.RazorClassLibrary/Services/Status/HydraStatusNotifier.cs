using Hydra.Http;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.RazorClassLibrary.Services.Status
{
    /// <summary>
    /// One thing that went wrong on the wire, already translated into something showable.
    /// </summary>
    public sealed class HydraStatusNotification
    {
        public int StatusCode { get; }

        /// <summary>Catalog entry for the code: title, user-facing sentence, kind, icon.</summary>
        public HydraStatusDefinition Definition { get; }

        /// <summary>
        /// Messages that came back inside the ResponseObject envelope, when the server sent one.
        /// These are more specific than the catalog sentence ("Role/Delete") and are shown under it.
        /// </summary>
        public IReadOnlyList<string> Messages { get; }

        public string? Url { get; }

        public DateTime OccurredAt { get; } = DateTime.Now;

        public HydraStatusNotification(int statusCode, IEnumerable<string>? messages = null, string? url = null)
        {
            StatusCode = statusCode;
            Definition = HydraStatusCatalog.Get(statusCode);
            Messages = messages?.Where(m => !string.IsNullOrWhiteSpace(m)).ToList() ?? new List<string>();
            Url = url;
        }
    }

    /// <summary>
    /// The client-side half of status-code management.
    ///
    /// Before this existed, a non-2xx response was written to the browser console and swallowed —
    /// the caller got null and the screen simply stayed empty, which is why a permission denial
    /// looked exactly like "there is no data". Now every failed call raises a notification here,
    /// and the UI decides what to do with it:
    ///
    ///   * <see cref="StatusMessageComponent"/> shows it inline, without losing the user's place;
    ///   * 401/403 additionally send the user to the /status/{code} page, because those two are
    ///     not "this list is empty", they are "you cannot be here".
    ///
    /// Scoped, so in Blazor Server one instance belongs to one circuit — one user.
    /// </summary>
    public class HydraStatusNotifier
    {
        private readonly NavigationManager _navigationManager;

        public HydraStatusNotifier(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        /// <summary>Raised for every non-successful response. Subscribers must unsubscribe on dispose.</summary>
        public event Action<HydraStatusNotification>? OnStatus;

        /// <summary>The most recent notification, for components that render after the fact.</summary>
        public HydraStatusNotification? Last { get; private set; }

        /// <summary>
        /// Whether 401/403 should navigate away. Turn it off for a screen that wants to handle
        /// refusal inline (for example a widget on a dashboard that should not drag the whole page
        /// away).
        /// </summary>
        public bool RedirectOnAuthenticationErrors { get; set; } = true;

        /// <summary>Route template of the shared status page. Kept here so apps can re-route it.</summary>
        public string StatusPagePathFormat { get; set; } = "/status/{0}";

        /// <summary>Where a 401 sends the user. Signing in again is the actual fix for a 401.</summary>
        public string LoginPath { get; set; } = "/Login";

        public void Raise(int statusCode, IEnumerable<string>? messages = null, string? url = null)
        {
            var notification = new HydraStatusNotification(statusCode, messages, url);

            Last = notification;

            OnStatus?.Invoke(notification);

            if (!RedirectOnAuthenticationErrors)
                return;

            //401 ve 403 farklı yerlere gider, çünkü kullanıcının yapabileceği şey farklı:
            //  401 → kim olduğumuzu bilmiyoruz; giriş ekranı bunu ÇÖZER, o yüzden oraya gönderiyoruz.
            //  403 → kim olduğunu biliyoruz ve yine de olmaz; tekrar giriş yapmak hiçbir şeyi
            //        değiştirmez, o yüzden "permission denied" sayfasında bırakıyoruz.
            if (statusCode == 401)
                NavigateToLogin();
            else if (statusCode == 403)
                NavigateToStatusPage(statusCode);
        }

        public void NavigateToLogin()
        {
            try
            {
                var returnUrl = Uri.EscapeDataString(_navigationManager.ToBaseRelativePath(_navigationManager.Uri));

                _navigationManager.NavigateTo($"{LoginPath}?returnUrl={returnUrl}", forceLoad: false, replace: true);
            }
            catch
            {
                // Navigation is not possible during prerendering.
            }
        }

        public void Clear() => Last = null;

        public void NavigateToStatusPage(int statusCode)
        {
            try
            {
                _navigationManager.NavigateTo(string.Format(StatusPagePathFormat, statusCode));
            }
            catch
            {
                // Navigation is not possible during prerendering; the inline component still shows
                // the message, so a failed redirect must not take the whole render down.
            }
        }
    }
}
