using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Formatting;

namespace LatinGrammarTranslator.Services.WordGeneration.Builders;

/// <summary>
/// Builder para criar parágrafos
/// </summary>
public class ParagraphBuilder
{
    private readonly Paragraph _paragraph;
    private readonly ParagraphProperties _paragraphProperties;
    private readonly List<Run> _runs = new();

    public ParagraphBuilder()
    {
        _paragraph = new Paragraph();
        _paragraphProperties = new ParagraphProperties();
    }

    /// <summary>
    /// Aplica estilo de parágrafo
    /// </summary>
    public ParagraphBuilder WithStyle(string styleId)
    {
        _paragraphProperties.Append(new ParagraphStyleId { Val = styleId });
        return this;
    }

    /// <summary>
    /// Define alinhamento
    /// </summary>
    public ParagraphBuilder Align(string alignment)
    {
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

    /// <summary>
    /// Define indentação à esquerda
    /// </summary>
    public ParagraphBuilder IndentLeft(double centimeters)
    {
        var indentation = _paragraphProperties.GetFirstChild<Indentation>() ?? new Indentation();
        indentation.Left = FormattingHelper.CentimetersToDxa(centimeters);

        if (indentation.Parent == null)
        {
            _paragraphProperties.Append(indentation);
        }

        return this;
    }

    /// <summary>
    /// Define indentação da primeira linha
    /// </summary>
    public ParagraphBuilder FirstLineIndent(double centimeters)
    {
        var indentation = _paragraphProperties.GetFirstChild<Indentation>() ?? new Indentation();
        indentation.FirstLine = FormattingHelper.CentimetersToDxa(centimeters);

        if (indentation.Parent == null)
        {
            _paragraphProperties.Append(indentation);
        }

        return this;
    }

    /// <summary>
    /// Define espaçamento antes do parágrafo
    /// </summary>
    public ParagraphBuilder SpaceBefore(int points)
    {
        var spacing = _paragraphProperties.GetFirstChild<SpacingBetweenLines>() ?? new SpacingBetweenLines();
        spacing.Before = FormattingHelper.PointsToTwips(points).ToString();

        if (spacing.Parent == null)
        {
            _paragraphProperties.Append(spacing);
        }

        return this;
    }

    /// <summary>
    /// Define espaçamento depois do parágrafo
    /// </summary>
    public ParagraphBuilder SpaceAfter(int points)
    {
        var spacing = _paragraphProperties.GetFirstChild<SpacingBetweenLines>() ?? new SpacingBetweenLines();
        spacing.After = FormattingHelper.PointsToTwips(points).ToString();

        if (spacing.Parent == null)
        {
            _paragraphProperties.Append(spacing);
        }

        return this;
    }

    /// <summary>
    /// Adiciona texto simples ao parágrafo
    /// </summary>
    public ParagraphBuilder AddText(string text)
    {
        _runs.Add(RunBuilder.Simple(text));
        return this;
    }

    /// <summary>
    /// Adiciona run customizado ao parágrafo
    /// </summary>
    public ParagraphBuilder AddRun(Run run)
    {
        _runs.Add(run);
        return this;
    }

    /// <summary>
    /// Adiciona run usando builder
    /// </summary>
    public ParagraphBuilder AddRun(Action<RunBuilder> configureRun)
    {
        var runBuilder = new RunBuilder("");
        configureRun(runBuilder);
        _runs.Add(runBuilder.Build());
        return this;
    }

    /// <summary>
    /// Adiciona múltiplos runs
    /// </summary>
    public ParagraphBuilder AddRuns(params Run[] runs)
    {
        _runs.AddRange(runs);
        return this;
    }

    /// <summary>
    /// Adiciona número de seção (ex: §1, §153)
    /// </summary>
    public ParagraphBuilder WithSectionNumber(string number)
    {
        var sectionRun = new RunBuilder(number)
            .Bold()
            .Build();

        _runs.Insert(0, sectionRun);

        // Adicionar espaço depois do número
        _runs.Insert(1, RunBuilder.Simple(" "));

        return this;
    }

    /// <summary>
    /// Constrói o parágrafo final
    /// </summary>
    public Paragraph Build()
    {
        // Adicionar propriedades se houver
        if (_paragraphProperties.HasChildren)
        {
            _paragraph.Append(_paragraphProperties);
        }

        // Adicionar runs
        foreach (var run in _runs)
        {
            _paragraph.Append(run);
        }

        return _paragraph;
    }

    /// <summary>
    /// Helper para criar parágrafo simples
    /// </summary>
    public static Paragraph Simple(string text, string? styleId = null)
    {
        var builder = new ParagraphBuilder().AddText(text);

        if (!string.IsNullOrEmpty(styleId))
        {
            builder.WithStyle(styleId);
        }

        return builder.Build();
    }

    /// <summary>
    /// Helper para criar heading
    /// </summary>
    public static Paragraph Heading(string text, int level)
    {
        return new ParagraphBuilder()
            .WithStyle($"Heading{level}")
            .AddText(text)
            .Build();
    }
}
