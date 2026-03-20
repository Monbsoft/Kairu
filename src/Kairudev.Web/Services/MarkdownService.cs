using Markdig;

namespace Kairudev.Web.Services;

public sealed class MarkdownService
{
    private static readonly MarkdownPipeline _pipeline =
        new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

    public string Render(string? md) =>
        string.IsNullOrWhiteSpace(md) ? string.Empty : Markdown.ToHtml(md, _pipeline);

    public static string StatusBadgeClass(string status) => status switch
    {
        "Done" => "bg-success",
        "InProgress" => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}
