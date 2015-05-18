using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Giki
{
    public static class Snippable
    {
        public static List<SnipPart> Parse(string text, string[] formats)
        {
            var index = 0;
            var start = 0;

            formats = formats ?? new string[0];
            var parts = new List<SnipPart>(formats != null ? formats.Length : 2);
            foreach (Match match in Regex.Matches(text, @"-8<-+(-\^-([^-]+)-+)?(-v-([^-]+)-+)?-+\r?$", RegexOptions.CultureInvariant | RegexOptions.Multiline))
            {
                EnsureList(parts, index);

                if (match.Groups[2].Success)
                {
                    parts[index].Format = match.Groups[2].Value;
                }
                if (match.Groups[4].Success)
                {
                    EnsureList(parts, index + 1);
                    parts[index].Format = match.Groups[4].Value;
                }

                parts[index].Content = text.Substring(start, match.Index).Trim();
                start = match.Index + match.Length;
                index++;
            }

            EnsureList(parts, index);
            parts[index].Content = text.Substring(start).Trim();

            return parts;
        }

        private static void EnsureList(List<SnipPart> list, int index)
        {
            while (list.Count <= index)
            {
                list.Add(new SnipPart());
            }
        }
    }

    public class SnipPart
    {
        public string Format { get; set; }
        public string Content { get; set; }
    }
}
