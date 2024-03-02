using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Markdig.Helpers;
using Markdig.Syntax;

namespace OliverBooth.Markdown.Renderers.JsonConverters;

public sealed class ParagraphBlockConverter : JsonConverter<ParagraphBlock>
{
    /// <inheritdoc />
    public override ParagraphBlock? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(reader.ValueSpan);
        if (node is not JsonObject jsonObject)
        {
            return null;
        }

        return new ParagraphBlock
        {
            Lines = new StringLineGroup(jsonObject["text"]?.GetValue<string>() ?? string.Empty)
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ParagraphBlock value, JsonSerializerOptions options)
    {
        string text = string.Join('\n', value.Inline);
        writer.WriteStartObject();
        writer.WriteString("type", "paragraph");
        writer.WriteString("text", text);
        writer.WriteEndObject();
    }
}
