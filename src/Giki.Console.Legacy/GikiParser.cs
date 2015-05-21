using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Giki
{
	internal static class GikiParser
	{
		public static string Parse(string content)
		{
			// wikilinks
			content = Regex.Replace(content, @"\[\[(?<Link>[^|\]]+)(\|(?<Text>[^\]]+))?\]\]", InnerLinkReplace);

			// tables
			content = Regex.Replace(content, @"^{\|(.*?)\n\|}\r?$", TableReplace, RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

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

		private static string TableReplace(Match match)
		{
			var builder = new StringBuilder(match.Length);
			var hasTr = false;
			var inTd = false;

			foreach (var line in match.Groups[0].Value.Split('\n'))
			{
				if (line.StartsWith("{|"))
				{
					var attr = line.Substring(2).Trim();
					if (!string.IsNullOrEmpty(attr))
					{
						builder.AppendFormat("<table {0}>", attr);
					}
					else
					{
						builder.AppendFormat("<table>");
					}
				}
				else if (line.StartsWith("|}"))
				{
					builder.AppendLine("</table>");
				}
				else if (line.StartsWith("|+"))
				{
					builder.Append("<caption>")
						.Append(line.Substring(2).Trim().ToLowerInvariant())
						.AppendLine("</caption>");
				}
				else if (line.StartsWith("|-"))
				{
					if (inTd)
						builder.AppendLine().AppendLine("</td>");

					if (hasTr)
						builder.AppendLine("</tr>");

					var attr = line.Substring(2).Trim();
					if (!string.IsNullOrEmpty(attr))
					{
						builder.AppendFormat("<tr {0}>", attr.ToLowerInvariant()).AppendLine();
					}
					else
					{
						builder.AppendLine("<tr>");
					}

					hasTr = true;
				}
				else if (line.StartsWith("|"))
				{
					if (inTd)
						builder.AppendLine().AppendLine("</td>");

					if (!hasTr)
						builder.AppendLine("<tr>");

					
					var lineContent = line.Substring(2).Trim();

					var cells = lineContent.Split(new[] { "||" } ,  StringSplitOptions.None );
					if (cells.Length > 1)
					{
						foreach (var cell in cells)
						{
							builder.AppendLine("<td>").AppendLine();
							builder.Append(cell.Trim());
							builder.AppendLine().AppendLine("</td>");
						}
					}
					else

					{
						var pipeIndex = lineContent.IndexOf('|');
						if (pipeIndex > 0)
						{
							builder.Append("<td ")
								.Append(lineContent.Substring(0, pipeIndex-1).ToLowerInvariant())
								.AppendLine(">").AppendLine();

							lineContent = lineContent.Substring(pipeIndex + 1);
						}
						else
						{

							builder.AppendLine("<td>").AppendLine();
						}

						builder.Append(lineContent);



						inTd = true;
					}
				}
				else
				{
					builder.Append(line).Append('\n');
				}
			}

			return builder.ToString();
		}
	}
}
