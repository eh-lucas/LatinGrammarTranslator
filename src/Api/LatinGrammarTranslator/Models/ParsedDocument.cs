using System.Text.Json.Serialization;

namespace LatinGrammarTranslator.Models;

/// <summary>
/// Tipos de nós na estrutura do documento
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NodeType
{
    [JsonPropertyName("h1")] HEADING_1,
    [JsonPropertyName("h2")] HEADING_2,
    [JsonPropertyName("h3")] HEADING_3,
    [JsonPropertyName("h4")] HEADING_4,
    [JsonPropertyName("p")] PARAGRAPH,
    [JsonPropertyName("ol")] LIST_ORDERED,
    [JsonPropertyName("ul")] LIST_UNORDERED,
    [JsonPropertyName("li")] LIST_ITEM,
    [JsonPropertyName("table")] TABLE,
    [JsonPropertyName("tr")] TABLE_ROW,
    [JsonPropertyName("td")] TABLE_CELL,
    [JsonPropertyName("th")] TABLE_HEADER,
    [JsonPropertyName("blockquote")] BLOCKQUOTE,
    [JsonPropertyName("div")] DIV,
    [JsonPropertyName("span")] SPAN,
    [JsonPropertyName("a")] LINK,
    [JsonPropertyName("strong")] STRONG,
    [JsonPropertyName("em")] EMPHASIS
}

/// <summary>
/// Tipos de texto identificados
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TextType
{
    [JsonPropertyName("english")] ENGLISH,
    [JsonPropertyName("latin")] LATIN,
    [JsonPropertyName("gloss")] GLOSS,
    [JsonPropertyName("reference")] REFERENCE,
    [JsonPropertyName("mixed")] MIXED
}

/// <summary>
/// Estilo de formatação aplicado ao texto
/// </summary>
public class FormattingStyle
{
    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }

    [JsonPropertyName("underline")]
    public bool Underline { get; set; }

    [JsonPropertyName("font_size")]
    public string? FontSize { get; set; }

    [JsonPropertyName("font_family")]
    public string? FontFamily { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("padding_left")]
    public string? PaddingLeft { get; set; }

    [JsonPropertyName("text_align")]
    public string? TextAlign { get; set; }
}

/// <summary>
/// Segmento de texto com tipo e formatação
/// </summary>
public class TextSegment
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("text_type")]
    public TextType TextType { get; set; }

    [JsonPropertyName("formatting")]
    public FormattingStyle Formatting { get; set; } = new();

    [JsonPropertyName("html_class")]
    public string? HtmlClass { get; set; }
}

/// <summary>
/// Nó parseado da estrutura HTML
/// </summary>
public class ParsedNode
{
    [JsonPropertyName("node_type")]
    public NodeType NodeType { get; set; }

    [JsonPropertyName("node_id")]
    public string? NodeId { get; set; }

    [JsonPropertyName("text_segments")]
    public List<TextSegment> TextSegments { get; set; } = new();

    [JsonPropertyName("attributes")]
    public Dictionary<string, string> Attributes { get; set; } = new();

    [JsonPropertyName("inline_style")]
    public string? InlineStyle { get; set; }

    [JsonPropertyName("children")]
    public List<ParsedNode> Children { get; set; } = new();

    [JsonPropertyName("section_number")]
    public string? SectionNumber { get; set; }

    [JsonPropertyName("is_footnote")]
    public bool IsFootnote { get; set; }

    [JsonPropertyName("footnote_id")]
    public string? FootnoteId { get; set; }

    [JsonPropertyName("has_cross_reference")]
    public bool HasCrossReference { get; set; }
}

/// <summary>
/// Documento HTML completamente parseado
/// </summary>
public class ParsedDocument
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = "utf-8";

    [JsonPropertyName("nodes")]
    public List<ParsedNode> Nodes { get; set; } = new();

    [JsonPropertyName("sections")]
    public Dictionary<string, ParsedNode> Sections { get; set; } = new();

    [JsonPropertyName("footnotes")]
    public Dictionary<string, ParsedNode> Footnotes { get; set; } = new();

    [JsonPropertyName("stats")]
    public Dictionary<string, object> Stats { get; set; } = new();

    [JsonPropertyName("original_filename")]
    public string? OriginalFilename { get; set; }

    [JsonPropertyName("css_file")]
    public string? CssFile { get; set; }
}
