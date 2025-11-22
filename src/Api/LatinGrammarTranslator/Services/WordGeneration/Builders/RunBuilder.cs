using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Formatting;

namespace LatinGrammarTranslator.Services.WordGeneration.Builders;

/// <summary>
/// Builder para criar Run (texto formatado inline)
/// </summary>
public class RunBuilder
{
    private readonly Run _run;
    private readonly RunProperties _runProperties;
    private readonly Text _text;

    public RunBuilder(string text)
    {
        _text = new Text(text);
        _runProperties = new RunProperties();
        _run = new Run();
    }

    /// <summary>
    /// Define o texto (permite mudar após construção)
    /// </summary>
    public RunBuilder WithText(string text)
    {
        _text.Text = text;
        return this;
    }

    /// <summary>
    /// Preserva espaços (importante para textos com múltiplos espaços)
    /// </summary>
    public RunBuilder PreserveSpace()
    {
        _text.Space = SpaceProcessingModeValues.Preserve;
        return this;
    }

    /// <summary>
    /// Aplica negrito
    /// </summary>
    public RunBuilder Bold()
    {
        _runProperties.Append(new Bold());
        return this;
    }

    /// <summary>
    /// Aplica itálico
    /// </summary>
    public RunBuilder Italic()
    {
        _runProperties.Append(new Italic());
        return this;
    }

    /// <summary>
    /// Aplica sublinhado
    /// </summary>
    public RunBuilder Underline()
    {
        _runProperties.Append(new Underline { Val = UnderlineValues.Single });
        return this;
    }

    /// <summary>
    /// Define cor do texto
    /// </summary>
    public RunBuilder Color(string hexColor)
    {
        _runProperties.Append(new Color { Val = hexColor });
        return this;
    }

    /// <summary>
    /// Define fonte
    /// </summary>
    public RunBuilder Font(string fontFamily)
    {
        _runProperties.Append(new RunFonts { Ascii = fontFamily, HighAnsi = fontFamily });
        return this;
    }

    /// <summary>
    /// Define tamanho da fonte
    /// </summary>
    public RunBuilder FontSize(int points)
    {
        _runProperties.Append(new FontSize { Val = FormattingHelper.PointsToHalfPoints(points) });
        return this;
    }

    /// <summary>
    /// Aplica estilo de caractere
    /// </summary>
    public RunBuilder WithStyle(string styleId)
    {
        _runProperties.Append(new RunStyle { Val = styleId });
        return this;
    }

    /// <summary>
    /// Aplica highlight (cor de fundo)
    /// </summary>
    public RunBuilder Highlight(string color)
    {
        var highlightValue = color.ToLowerInvariant() switch
        {
            "yellow" => HighlightColorValues.Yellow,
            "green" => HighlightColorValues.Green,
            "cyan" => HighlightColorValues.Cyan,
            "magenta" => HighlightColorValues.Magenta,
            "blue" => HighlightColorValues.Blue,
            "red" => HighlightColorValues.Red,
            "darkblue" => HighlightColorValues.DarkBlue,
            "darkred" => HighlightColorValues.DarkRed,
            _ => HighlightColorValues.Yellow
        };

        _runProperties.Append(new Highlight { Val = highlightValue });
        return this;
    }

    /// <summary>
    /// Constrói o Run final
    /// </summary>
    public Run Build()
    {
        // Adicionar propriedades se houver
        if (_runProperties.HasChildren)
        {
            _run.Append(_runProperties);
        }

        // Adicionar texto
        _run.Append(_text);

        return _run;
    }

    /// <summary>
    /// Helper para criar run simples
    /// </summary>
    public static Run Simple(string text)
    {
        return new RunBuilder(text).Build();
    }

    /// <summary>
    /// Helper para criar run em negrito
    /// </summary>
    public static Run BoldText(string text)
    {
        return new RunBuilder(text).Bold().Build();
    }

    /// <summary>
    /// Helper para criar run em itálico
    /// </summary>
    public static Run ItalicText(string text)
    {
        return new RunBuilder(text).Italic().Build();
    }
}
