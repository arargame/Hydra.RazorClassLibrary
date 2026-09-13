namespace Hydra.RazorClassLibrary.Services.Status
{
    /// <summary>
    /// Maps the catalog's symbolic icon names ("lock", "search", ...) to inline SVG.
    ///
    /// Kept in C# rather than inside the .razor files so that the status page and the inline
    /// status banner draw the same icon for the same code, and so that an application that wants
    /// its own icon set has exactly one place to override.
    /// </summary>
    public static class StatusIcons
    {
        private const string Attributes = @"viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round""";

        public static string Svg(string? iconName, int size = 26)
        {
            var body = iconName switch
            {
                "lock" => @"<rect x=""4"" y=""10"" width=""16"" height=""11"" rx=""2""/><path d=""M8 10V7a4 4 0 0 1 8 0v3""/>",
                "search" => @"<circle cx=""11"" cy=""11"" r=""7""/><path d=""m20 20-3.5-3.5""/>",
                "clock" => @"<circle cx=""12"" cy=""12"" r=""9""/><path d=""M12 7v5l3 2""/>",
                "server" => @"<rect x=""3"" y=""4"" width=""18"" height=""7"" rx=""2""/><rect x=""3"" y=""13"" width=""18"" height=""7"" rx=""2""/><path d=""M7 7.5h.01M7 16.5h.01""/>",
                "warning" => @"<path d=""M12 4.5 2.8 20h18.4L12 4.5Z""/><path d=""M12 10v4M12 17.5h.01""/>",
                _ => @"<circle cx=""12"" cy=""12"" r=""9""/><path d=""M12 11v5M12 8h.01""/>",
            };

            return $@"<svg width=""{size}"" height=""{size}"" {Attributes} aria-hidden=""true"">{body}</svg>";
        }
    }
}
