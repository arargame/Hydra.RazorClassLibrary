using System;
using System.Collections.Generic;
using System.Linq;

namespace Hydra.RazorClassLibrary.Utils
{
    public static class UrlHelper
    {
        /// <summary>
        /// Constructs a URL from parts.
        /// </summary>
        /// <example>
        /// <code>
        /// // Path parameter: /Exam/Details/123
        /// BuildUrl("Exam", "Details", pathLikeParameter: "123");
        /// 
        /// // Raw query string: /Exam/Search?status=active&type=final
        /// BuildUrl("Exam", "Search", queryString: "status=active&type=final");
        /// </code>
        /// </example>
        public static string BuildUrl(string controller, string action = "", Dictionary<string, string>? parameters = null, string? pathLikeParameter = null, string? queryString = null)
        {
            var url = controller;
            
            if(!string.IsNullOrEmpty(action))
                url += $"/{action}";

            if (!string.IsNullOrEmpty(pathLikeParameter))
                url += $"/{pathLikeParameter}";

            var queryParts = new List<string>();

            if (parameters != null && parameters.Any())
            {
                var dictQuery = string.Join("&", parameters.Select(p => $"{System.Net.WebUtility.UrlEncode(p.Key)}={System.Net.WebUtility.UrlEncode(p.Value)}"));
                queryParts.Add(dictQuery);
            }

            if (!string.IsNullOrEmpty(queryString))
            {
                queryParts.Add(queryString);
            }

            if (queryParts.Any())
            {
                url += "?" + string.Join("&", queryParts);
            }

            return url;
        }
    }
}
