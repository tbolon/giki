using System;
using System.IO;
using DotLiquid;

namespace Giki
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = args[0];
            var outDir = Path.Combine(root, "_out");

            Directory.CreateDirectory(outDir);

            Template.FileSystem = new SnippetsFileSystem(Path.Combine(root, "_snippets"));
            foreach (var file in Directory.GetFiles(root, "*.xmd"))
            {
                // 0) content
                var content = File.ReadAllText(file);

                // 1) snippable parsing
                var parts = Snippable.Parse(content, new[] { "yaml", "md" });
                content = parts[1].Content;

                // 2) liquid parsing
                var template = Template.Parse(content);
                content = template.Render();

                // 3) custom markup parsing
                content = GikiParser.Parse(content);

                // 4) commonmark parsing
                content = CommonMark.CommonMarkConverter.Convert(content);

                // 5) write result
                File.WriteAllText(Path.Combine(outDir, Path.GetFileNameWithoutExtension(file) + ".html"), content);
            }
        }
    }

    class SnippetsFileSystem : DotLiquid.FileSystems.IFileSystem
    {
        private readonly string dir;

        public SnippetsFileSystem(string dir)
        {
            this.dir = dir;
        }
        public string ReadTemplateFile(Context context, string templateName)
        {
            var path = Path.Combine(dir, templateName + ".xmd");
            if (!File.Exists(path))
            {
                return string.Empty;
            }

            var content = File.ReadAllText(path);
            var snips = Snippable.Parse(content, new[] { "yaml", "md" });

            return snips[1].Content;
        }
    }
}
