using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Configuration;
using LatinGrammarTranslator.Services.WordGeneration.Formatting;

namespace LatinGrammarTranslator.Services.WordGeneration.Styles;

/// <summary>
/// Builder para criar definições de estilo OpenXml
/// </summary>
public class StyleDefinition
{
    private readonly Style _style;
    private readonly StyleRunProperties _runProperties;
    private readonly StyleParagraphProperties? _paragraphProperties;

    private StyleDefinition(string styleId, string styleName, StyleValues styleType)
    {
        _style = new Style
        {
            Type = styleType,
            StyleId = styleId,
            CustomStyle = true
        };

        _style.Append(new StyleName { Val = styleName });
        _style.Append(new BasedOn { Val = "Normal" });

        _runProperties = new StyleRunProperties();

        if (styleType == StyleValues.Paragraph)
        {
            _paragraphProperties = new StyleParagraphProperties();
        }
    }

    /// <summary>
    /// Cria definição de estilo de parágrafo
    /// </summary>
    public static StyleDefinition CreateParagraphStyle(string styleId, string styleName)
    {
        return new StyleDefinition(styleId, styleName, StyleValues.Paragraph);
    }

    /// <summary>
    /// Cria definição de estilo de caractere
    /// </summary>
    public static StyleDefinition CreateCharacterStyle(string styleId, string styleName)
    {
        return new StyleDefinition(styleId, styleName, StyleValues.Character);
    }

    /// <summary>
    /// Aplica configuração de estilo
    /// </summary>
    public StyleDefinition ApplyConfiguration(StyleConfiguration config)
    {
        if (config.FontFamily != null)
        {
            WithFont(config.FontFamily);
        }

        if (config.FontSize.HasValue)
        {
            WithFontSize(config.FontSize.Value);
        }

        if (config.Bold == true)
        {
            WithBold();
        }

        if (config.Italic == true)
        {
            WithItalic();
        }

        if (config.Underline == true)
        {
            WithUnderline();
        }

        if (config.Color != null)
        {
            WithColor(config.Color);
        }

        // Propriedades de parágrafo
        if (_paragraphProperties != null)
        {
            if (config.Alignment != null)
            {
                WithAlignment(config.Alignment);
            }

            if (config.LineSpacing.HasValue)
            {
                WithLineSpacing(config.LineSpacing.Value);
            }

            if (config.SpaceBefore.HasValue)
            {
                WithSpaceBefore(config.SpaceBefore.Value);
            }

            if (config.SpaceAfter.HasValue)
            {
                WithSpaceAfter(config.SpaceAfter.Value);
            }

            if (config.IndentLeft.HasValue)
            {
                WithIndentLeft(config.IndentLeft.Value);
            }

            if (config.IndentRight.HasValue)
            {
                WithIndentRight(config.IndentRight.Value);
            }

            if (config.IndentFirstLine.HasValue)
            {
                WithFirstLineIndent(config.IndentFirstLine.Value);
            }
        }

        return this;
    }

    public StyleDefinition WithFont(string fontFamily)
    {
        _runProperties.Append(new RunFonts { Ascii = fontFamily, HighAnsi = fontFamily });
        return this;
    }

    public StyleDefinition WithFontSize(int points)
    {
        _runProperties.Append(new FontSize { Val = FormattingHelper.PointsToHalfPoints(points) });
        return this;
    }

    public StyleDefinition WithBold()
    {
        _runProperties.Append(new Bold());
        return this;
    }

    public StyleDefinition WithItalic()
    {
        _runProperties.Append(new Italic());
        return this;
    }

    public StyleDefinition WithUnderline()
    {
        _runProperties.Append(new Underline { Val = UnderlineValues.Single });
        return this;
    }

    public StyleDefinition WithColor(string hexColor)
    {
        _runProperties.Append(new Color { Val = hexColor });
        return this;
    }

    public StyleDefinition WithAlignment(string alignment)
    {
        if (_paragraphProperties == null) return this;

        var justificationValue = alignment.ToLowerInvariant() switch
        {
            "left" => JustificationValues.Left,
            "center" => JustificationValues.Center,
            "right" => JustificationValues.Right,
            "justify" => JustificationValues.Both,
            _ => JustificationValues.Left
        };

        _paragraphProperties.Append(new Justification { Val = justificationValue });
        return this;
    }

    public StyleDefinition WithLineSpacing(double spacing)
    {
        if (_paragraphProperties == null) return this;

        _paragraphProperties.Append(new SpacingBetweenLines
        {
            Line = FormattingHelper.LineSpacingToOpenXml(spacing),
            LineRule = LineSpacingRuleValues.Auto
        });
        return this;
    }

    public StyleDefinition WithSpaceBefore(int points)
    {
        if (_paragraphProperties == null) return this;

        var spacing = _paragraphProperties.GetFirstChild<SpacingBetweenLines>() ?? new SpacingBetweenLines();
        spacing.Before = FormattingHelper.PointsToTwips(points).ToString();

        if (spacing.Parent == null)
        {
            _paragraphProperties.Append(spacing);
        }

        return this;
    }

    public StyleDefinition WithSpaceAfter(int points)
    {
        if (_paragraphProperties == null) return this;

        var spacing = _paragraphProperties.GetFirstChild<SpacingBetweenLines>() ?? new SpacingBetweenLines();
        spacing.After = FormattingHelper.PointsToTwips(points).ToString();

        if (spacing.Parent == null)
        {
            _paragraphProperties.Append(spacing);
        }

        return this;
    }

    public StyleDefinition WithIndentLeft(double centimeters)
    {
        if (_paragraphProperties == null) return this;

        var indentation = _paragraphProperties.GetFirstChild<Indentation>() ?? new Indentation();
        indentation.Left = FormattingHelper.CentimetersToDxa(centimeters);

        if (indentation.Parent == null)
        {
            _paragraphProperties.Append(indentation);
        }

        return this;
    }

    public StyleDefinition WithIndentRight(double centimeters)
    {
        if (_paragraphProperties == null) return this;

        var indentation = _paragraphProperties.GetFirstChild<Indentation>() ?? new Indentation();
        indentation.Right = FormattingHelper.CentimetersToDxa(centimeters);

        if (indentation.Parent == null)
        {
            _paragraphProperties.Append(indentation);
        }

        return this;
    }

    public StyleDefinition WithFirstLineIndent(double centimeters)
    {
        if (_paragraphProperties == null) return this;

        var indentation = _paragraphProperties.GetFirstChild<Indentation>() ?? new Indentation();
        indentation.FirstLine = FormattingHelper.CentimetersToDxa(centimeters);

        if (indentation.Parent == null)
        {
            _paragraphProperties.Append(indentation);
        }

        return this;
    }

    /// <summary>
    /// Constrói o Style final
    /// </summary>
    public Style Build()
    {
        // Adicionar propriedades de parágrafo se existirem e tiverem elementos
        if (_paragraphProperties != null && _paragraphProperties.HasChildren)
        {
            _style.Append(_paragraphProperties);
        }

        // Adicionar propriedades de run se tiverem elementos
        if (_runProperties.HasChildren)
        {
            _style.Append(_runProperties);
        }

        return _style;
    }
}
