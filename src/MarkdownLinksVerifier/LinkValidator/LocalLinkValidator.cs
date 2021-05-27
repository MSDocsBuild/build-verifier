﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace MarkdownLinksVerifier.LinkValidator
{
    internal class LocalLinkValidator : ILinkValidator
    {
        private readonly string _baseDirectory;

        public LocalLinkValidator(string baseDirectory) => _baseDirectory = baseDirectory;

        public bool IsValid(string link, string filePath)
        {
            // Consider [text]() as valid. It redirects to the current directory.
            if (string.IsNullOrEmpty(link))
            {
                return true;
            }

            if (link.StartsWith('#'))
            {
                return IsHeadingValid(filePath, link[1..]);
            }

            link = link.Replace("%20", " ", StringComparison.Ordinal);
            string relativeTo = _baseDirectory;

            if (link.StartsWith('/'))
            {
                // Links that start with / are relative to the repository root.
                // TODO: Does it work locally (e.g. in VS Code)? Consider a warning for it.
                relativeTo = Directory.GetCurrentDirectory();
            }

            string? headingIdWithoutHash = null;
            int lastIndex = link.LastIndexOf('#');
            if (lastIndex != -1)
            {
                // TODO: Add a warning if headingIdWithoutHash is empty here?
                // e.g: [Text](file.md#)
                headingIdWithoutHash = link[(lastIndex + 1)..];
                link = link.Substring(0, lastIndex);
            }

            // Remove query parameters.
            lastIndex = link.LastIndexOf('?');
            if (lastIndex != -1)
            {
                link = link.Substring(0, lastIndex);
            }

            string path = Path.GetFullPath(Path.Join(relativeTo, link));
            if (headingIdWithoutHash is null)
            {
                return File.Exists(path) || Directory.Exists(path);
            }
            else
            {
                return File.Exists(path) && IsHeadingValid(path, headingIdWithoutHash);
            }
        }

        private static bool IsHeadingValid(string path, string headingIdWithoutHash)
        {
            if (path.StartsWith("tab/", StringComparison.Ordinal))
            {
                // MSDocs-specific syntax. This syntax works only in an H1.
                // e.g: `# [Visual Studio 15.6 and earlier](#tab/vs156)`
                // But I'm returning true for everything.
                return true;
            }

            if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseAutoIdentifiers(AutoIdentifierOptions.GitHub).Build(); // TODO: Is AutoIdentifierOptions.GitHub the correct value to use?
            MarkdownDocument document = Markdown.Parse(File.ReadAllText(path), pipeline);
            return document.Descendants<HeadingBlock>().Any(heading => headingIdWithoutHash == heading.GetAttributes().Id) ||
                document.Descendants<HtmlInline>().Any(html => IsValidHtml(html.Tag, headingIdWithoutHash));

            // Hacky approach!
            static bool IsValidHtml(string tag, string headingIdWithoutHawsh)
            {
                return Regex.Match(tag, @"^<a\s+?(name|id)\s*?=\s*?""(.+?)""").Groups[2].Value == headingIdWithoutHawsh;
            }
        }
    }
}
