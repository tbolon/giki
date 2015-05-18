using System.Text.RegularExpressions;

namespace Giki
{
    internal static class GikiParser
    {
        public static string Parse(string content)
        {
            content = Regex.Replace(content, @"\[\[(?<Link>[^|\]]+)(\|(?<Text>\|[^\]]+))?\]\]", InnerLinkReplace);

            return content;
        }

        private static string InnerLinkReplace(Match match)
        {
            var link = match.Groups["Link"].Value;
            var text = match.Groups["Text"].Success ? match.Groups["Text"].Value : link;

            link = Regex.Replace(link, @"['-]", string.Empty, RegexOptions.CultureInvariant);
            link = Regex.Replace(link, @"[\s]", "-", RegexOptions.CultureInvariant);

            return string.Format("[{0}]({1}.html)", text, link);
        }
    }
}
