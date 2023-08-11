using System.Xml.Serialization;

namespace OliverBooth.Data.Rss;

public sealed class AtomLink
{
    [XmlAttribute("href")]
    public string Href { get; set; } = default!;

    [XmlAttribute("rel")]
    public string Rel { get; set; } = "self";

    [XmlAttribute("type")]
    public string Type { get; set; } = "application/rss+xml";
}
