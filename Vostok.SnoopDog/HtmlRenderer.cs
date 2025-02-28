using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vostok.SnoopDog.Core;
using Vostok.SnoopDog.Core.Issues;
using Vostok.SnoopDog.Core.Metrics;
using Vostok.SnoopDog.Core.Stats;

namespace Vostok.SnoopDog
{
    public class HtmlRendererBase
    {
        public HtmlRendererBase(TextWriter writer)
        {
            Writer = writer;
        }

        protected TextWriter Writer { get; }

        protected Tag Tag(string tagName, params (string attribute, string value)[] attributes)
            => new Tag(Writer, tagName, attributes);
    }

    public class HtmlRenderer : HtmlRendererBase
    {
        // ReSharper disable once InconsistentNaming
        private const string NA = "n/a";

        private static readonly string LongHexFormatString = $"X{sizeof (long)*2}";

        public HtmlRenderer(TextWriter writer)
            : base(writer)
        {
        }

        private static string CssDefinition { get; } = @"
<style type=""text/css"">
	table { border-collapse: collapse; display: block; width: 100%; overflow: auto; }
	td, th { padding: 6px 13px; border: 1px solid #ddd; }
	tr { background-color: #fff; border-top: 1px solid #ccc; }
	tr:nth-child(even) { background: #f8f8f8; }
    .mono {font-family: monospace}
</style>";

        public void Render(Report[] reports)
        {
            using (Tag("head"))
            {
                Writer.WriteLine(CssDefinition);
            }

            using (Tag("body"))
                foreach (var report in reports)
                    Render(report);
        }

        private void Render(Report report)
        {
            Writer.WriteLine(@"<h1>Report</h1>");

            if (report.Metrics.Any())
            {
                Writer.WriteLine(@"<h2>Metrics</h2>");
                Render(report.Metrics);
            }

            if (report.Issues.Any())
            {
                Writer.WriteLine(@"<h2>Issues</h2>");
                foreach (var issue in report.Issues)
                    Render(issue);
            }

            if (report.Stats.Any())
            {
                Writer.WriteLine(@"<h2>Statistics</h2>");
                foreach (var stat in report.Stats)
                    Render(stat);
            }
        }

        private void Render(IReadOnlyList<Metric> reportMetrics)
        {
            RenderTable(
                reportMetrics,
                ("Metric name", metric => WriteMonospace(metric.Name)),
                ("Value", metric => WriteNumericTableValue(metric.Value.ToString())));
        }

        private void Write(string str)
        {
            Writer.WriteLine(
                string.Join(
                    "<br>",
                    str.Split('\n')
                       .Select(HttpUtility.HtmlEncode)));
        }

        private void WriteMonospace(string str)
        {
            var isFirstLine = true;

            foreach (var line in str.Split('\n')
               .Select(HttpUtility.HtmlEncode))
            {
                if (!isFirstLine)
                    Writer.Write("<br>");
                else
                    isFirstLine = false;

                using (Tag("span", (attribute: "class", value: "mono")))
                    Writer.WriteLine(line);
            }
        }

        private void Render(Stat stat)
        {
            using (Tag("h3"))
                Write(stat.Title);

            using (new DetailsTag(
                Writer,
                () =>
                {
                    using (Tag("span"))
                        Write(stat.Description);
                }))
            {
                if (stat is StackTraceStat sts)
                {
                    RenderTable(
                        sts.StackTraceInfos.OrderByDescending(s => s.ThreadsCount),
                        ("Threads count", s => WriteNumericTableValue(s.ThreadsCount.ToString())),
                        ("Thread ids", s => RenderPossiblyExpandableList(
                            s.ThreadIds,
                            ", ",
                            10,
                            5,
                            id => WriteMonospace(id.ToString()))
                        ),
                        ("Stack trace", s => RenderStackTrace(s.StackTrace)));
                }
                else if (stat is TypesStat ts)
                {
                    using (Tag("h4"))
                        Write("Top 20 by objects count");
                    RenderTypesStatsTable(ts.TypesStats.OrderByDescending(s => s.Value.Count).Take(20));

                    using (Tag("h4"))
                        Write("Top 20 by total size");
                    RenderTypesStatsTable(ts.TypesStats.OrderByDescending(s => s.Value.TotalSize).Take(20));
                }
            }
        }

        private void RenderPossiblyExpandableList<T>(
            ICollection<T> elements,
            string separator,
            int threshold,
            int head,
            Action<T> renderElement = null)
        {
            if (renderElement is null)
                renderElement = e => Writer.Write(HttpUtility.HtmlEncode(e.ToString()));

            if (elements.Count <= threshold)
                RenderSeparating(elements);
            else
                using (new DetailsTag(Writer, Title))
                    RenderSeparating(elements.Skip(head));

            void Title()
            {
                foreach (var e in elements.Take(head))
                {
                    renderElement(e);
                    Writer.Write(separator);
                }
            }

            void RenderSeparating(IEnumerable<T> elems)
            {
                var isFirstLine = true;

                foreach (var e in elems)
                {
                    if (!isFirstLine)
                        Writer.Write(separator);
                    else
                        isFirstLine = false;

                    renderElement(e);
                }
            }
        }

        private void RenderStackTrace(string[] s)
            => RenderPossiblyExpandableList(s, "<br>", 7, 4, WriteMonospace);

        private void RenderTypesStatsTable(IEnumerable<KeyValuePair<string, TypeStat>> typesStats)
        {
            RenderTable(
                typesStats,
                ("Type name", s => WriteMonospace(s.Key)),
                ("Method Table", s => WriteMonospace(s.Value.MethodTable?.ToString(LongHexFormatString) ?? NA)),
                ("Objects count", s => WriteNumericTableValue(s.Value.Count.ToString())),
                ("Total objects size", s => WriteNumericTableValue(s.Value.TotalSize.ToString())));
        }

        private void WriteNumericTableValue(string value)
        {
            using (Tag("p", ("class", "mono"), ("align", "right")))
                Write(value);
        }

        private void RenderTable<T>(IEnumerable<T> elements, params ValueTuple<string, Action<T>>[] headAndSelector)
        {
            using (Tag("table"))
            {
                using (Tag("tr"))
                    foreach (var head in headAndSelector)
                        using (Tag("th"))
                            Write(head.Item1);

                foreach (var e in elements)
                    using (Tag("tr"))
                        foreach (var selector in headAndSelector)
                            using (Tag("td"))
                                selector.Item2(e);
            }
        }

        private void Render(IIssue issue)
        {
            using (Tag("h3"))
                Write(issue.Title);
            Write(issue.Message);

            if (issue is UnhandledExceptionIssue ex)
            {
                using (Tag("h4"))
                    Write("Exception type");
                WriteMonospace(ex.ExceptionType);

                using (Tag("h4"))
                    Write("Exception message");
                using (new DetailsTag(Writer))
                    WriteMonospace(ex.ExceptionMessage);

                using (Tag("h4"))
                    Write("Stack trace");
                RenderStackTrace(ex.StackTrace);
            }
        }
    }

    public class Tag : HtmlRendererBase, IDisposable
    {
        public Tag(TextWriter writer, string tagName, params (string attribute, string value)[] attributes)
            :
            base(writer)
        {
            TagName = tagName;

            Writer.WriteLine($"<{tagName} {string.Join(" ", attributes.Select(RenderAttribute))}>");
        }

        private string TagName { get; }

        public void Dispose()
        {
            Writer.WriteLine($"</{TagName}>");
        }

        private static string RenderAttribute((string attribute, string value) attribute)
        {
            return $"{attribute.attribute}=\"{attribute.value}\"";
        }
    }

    internal class DetailsTag : Tag
    {
        public DetailsTag(TextWriter writer, Action title = null)
            : base(writer, "details")
        {
            if (title is null)
                title = () => Writer.WriteLine("Click to expand");

            using (Tag("summary"))
                title();
        }
    }
}