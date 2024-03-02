using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace OliverBooth.Markdown.Renderers.JsonConverters;

public sealed class HeadingBlockConverter : JsonConverter<HeadingBlock>
{
    /// <inheritdoc />
    public override HeadingBlock? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var node = JsonNode.Parse(reader.ValueSpan);
        if (node is not JsonObject jsonObject)
        {
            return null;
        }

        return new HeadingBlock(null!)
        {
            Level = jsonObject["level"]?.GetValue<int>() ?? 1,
            Lines = new StringLineGroup(jsonObject["text"]?.GetValue<string>() ?? string.Empty)
        };
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, HeadingBlock value, JsonSerializerOptions options)
    {
        if (value.Inline is not { } containerInline)
        {
            return;
        }

        writer.WriteStartObject();
        writer.WriteString("type", "header");
        writer.WriteNumber("level", value.Level);
        writer.WritePropertyName("text");

        foreach (Inline inline in containerInline)
        {
            if (inline is LiteralInline literal)
            {
                var converter = (JsonConverter<LiteralInline>)options.GetConverter(typeof(LiteralInline));
                writer.WriteStringValue(literal.Content.Text);
                converter.Write(writer, literal, options);
            }
        }

        writer.WriteEndObject();
    }
}
