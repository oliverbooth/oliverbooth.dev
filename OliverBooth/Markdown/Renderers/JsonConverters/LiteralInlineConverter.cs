using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Markdig.Syntax.Inlines;

namespace OliverBooth.Markdown.Renderers.JsonConverters;

public class LiteralInlineConverter : JsonConverter<LiteralInline>
{
    public override LiteralInline? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        ReadOnlySpan<byte> bytes = reader.ValueSpan;
        Span<char> chars = stackalloc char[bytes.Length];
        Encoding.UTF8.GetChars(bytes, chars);
        return new LiteralInline(chars.ToString());
    }

    public override void Write(Utf8JsonWriter writer, LiteralInline value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Content.Text);
    }
}
