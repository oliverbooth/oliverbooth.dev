using Markdig.Renderers;
using Markdig.Syntax;

namespace OliverBooth.Markdown.Renderers.EditorJs.ObjectRenderers;

public class HeadingObjectRenderer : IMarkdownObjectRenderer
{
    public bool Accept(RendererBase renderer, Type objectType)
    {
        return renderer.GetType() == typeof(JsonRenderer) && objectType == typeof(HeadingBlock);
    }

    public void Write(RendererBase renderer, MarkdownObject objectToRender)
    {
        
    }
}
