using System.Text.Json;
using System.Text.Json.Serialization;
using Markdig.Syntax;

namespace OliverBooth.Markdown.Renderers.JsonConverters;

public sealed class MarkdownDocumentConverter : JsonConverter<MarkdownDocument>
{
    /// <inheritdoc />
    public override MarkdownDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var document = new MarkdownDocument();
        var blocks = JsonSerializer.Deserialize<Block[]>(ref reader, options) ?? Array.Empty<Block>();

        foreach (Block block in blocks)
        {
            document.Add(block);
        }

        return document;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, MarkdownDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("blocks");

        foreach (Block block in value)
        {
            WriteBlock(writer, options, block);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteBlock(Utf8JsonWriter writer, JsonSerializerOptions options, Block block)
    {
        switch (block)
        {
            case ParagraphBlock paragraphBlock:
            {
                var converter = (JsonConverter<ParagraphBlock>)options.GetConverter(typeof(ParagraphBlock));
                converter.Write(writer, paragraphBlock, options);
                break;
            }

            case HeadingBlock headingBlock:
            {
                var converter = (JsonConverter<HeadingBlock>)options.GetConverter(typeof(HeadingBlock));
                converter.Write(writer, headingBlock, options);
                break;
            }
        }
    }
}
