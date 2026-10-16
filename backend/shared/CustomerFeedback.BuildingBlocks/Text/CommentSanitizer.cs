using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomerFeedback.BuildingBlocks.Text;

public static partial class CommentSanitizer
{
    public static string? Sanitize(string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments))
        {
            return null;
        }

        var decoded = WebUtility.HtmlDecode(comments);
        var withoutTags = HtmlTagRegex().Replace(decoded, string.Empty);
        var builder = new StringBuilder(withoutTags.Length);
        foreach (var ch in withoutTags)
        {
            if (char.IsControl(ch) && ch is not '\t' and not '\n' and not '\r')
            {
                continue;
            }

            builder.Append(ch);
        }

        var cleaned = builder.ToString().Trim();
        return cleaned.Length == 0 ? null : cleaned[..Math.Min(cleaned.Length, 2000)];
    }

    [GeneratedRegex("<.*?>", RegexOptions.Singleline)]
    private static partial Regex HtmlTagRegex();
}
