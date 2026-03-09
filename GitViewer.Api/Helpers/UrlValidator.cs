using System.Text.RegularExpressions;

namespace GitViewer.Api.Helpers
{
    public class UrlValidator
    {
        private static readonly Regex SshUrlRegex = new Regex(
            @"^(?<user>[a-zA-Z0-9._%+-]+)@(?<host>[a-zA-Z0-9.-]+):(?<path>[\w./-]+)(\.git)?$",
            RegexOptions.Compiled);

        public static bool ValidateUrlWithUriCreate(string url, out Uri? uri, out bool isSsh)
        {
            uri = null;
            isSsh = false;

            if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
            {
                uri = parsedUri;
                return true;
            }

            if (SshUrlRegex.IsMatch(url))
            {
                isSsh = true;
                return true;
            }

            return false;
        }

        // Returns the repository name (e.g. "OmegaPlugin") from SSH or HTTPS repository URLs.
        public static string? ExtractRepoName(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            // HTTPS / HTTP / other URI forms
            if (Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
            {
                var path = parsedUri.AbsolutePath.Trim('/');
                if (string.IsNullOrEmpty(path)) return null;
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var last = segments.Last();
                if (last.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    last = last[..^4];
                return last;
            }

            // SSH: user@host:owner/repo.git
            var m = SshUrlRegex.Match(url);
            if (m.Success)
            {
                var path = m.Groups["path"].Value.Trim('/');
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0) return null;
                var last = segments.Last();
                if (last.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    last = last[..^4];
                return last;
            }

            // Fallback: split on last slash or colon and strip .git
            var trimmed = url.TrimEnd('/');
            var idx = Math.Max(trimmed.LastIndexOf('/'), trimmed.LastIndexOf(':'));
            if (idx >= 0 && idx < trimmed.Length - 1)
            {
                var last = trimmed.Substring(idx + 1);
                if (last.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                    last = last[..^4];
                return last;
            }

            return null;
        }
    }
}
