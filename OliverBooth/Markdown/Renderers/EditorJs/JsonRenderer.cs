using System.Text.Json;
using Markdig.Renderers;
using Markdig.Syntax;

namespace OliverBooth.Markdown.Renderers.EditorJs;

public class JsonRenderer : RendererBase
{
    /// <inheritdoc />
    public override object Render(MarkdownObject markdownObject)
    {
        return JsonDocument.Parse("""{"blocks": []}""");
    }
}
