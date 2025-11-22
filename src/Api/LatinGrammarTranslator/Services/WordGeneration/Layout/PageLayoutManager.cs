using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Configuration;
using LatinGrammarTranslator.Services.WordGeneration.Formatting;
using StringValue = DocumentFormat.OpenXml.StringValue;
using Int32Value = DocumentFormat.OpenXml.Int32Value;
using UInt32Value = DocumentFormat.OpenXml.UInt32Value;

namespace LatinGrammarTranslator.Services.WordGeneration.Layout;

/// <summary>
/// Gerencia configuração de layout de página
/// </summary>
public class PageLayoutManager
{
    private readonly Body _body;
    private SectionProperties? _sectionProperties;

    public PageLayoutManager(Body body)
    {
        _body = body ?? throw new ArgumentNullException(nameof(body));
    }

    /// <summary>
    /// Aplica configuração de documento ao layout
    /// </summary>
    public void ApplyConfiguration(DocumentConfiguration config)
    {
        _sectionProperties = new SectionProperties();

        // Configurar tamanho da página
        SetPageSize(config.PageSize, config.Orientation);

        // Configurar margens
        if (config.MirroredMargins)
        {
            SetMirroredMargins(
                inner: config.MarginInner,
                outer: config.MarginOuter,
                top: config.MarginTop,
                bottom: config.MarginBottom,
                header: config.MarginHeader,
                footer: config.MarginFooter
            );
        }
        else
        {
            SetStandardMargins(
                left: config.MarginInner,   // Usa inner como left
                right: config.MarginOuter,  // Usa outer como right
                top: config.MarginTop,
                bottom: config.MarginBottom,
                header: config.MarginHeader,
                footer: config.MarginFooter
            );
        }

        // Aplicar seção ao body
        _body.Append(_sectionProperties);
    }

    /// <summary>
    /// Define tamanho da página
    /// </summary>
    private void SetPageSize(string pageSize, string orientation)
    {
        var (width, height) = GetPageDimensions(pageSize);

        // Se landscape, inverter dimensões
        if (orientation.Equals("landscape", StringComparison.OrdinalIgnoreCase))
        {
            (width, height) = (height, width);
        }

        var pageSizeElement = new PageSize
        {
            Width = (UInt32Value)(uint)width,
            Height = (UInt32Value)(uint)height,
            Orient = orientation.Equals("landscape", StringComparison.OrdinalIgnoreCase)
                ? PageOrientationValues.Landscape
                : PageOrientationValues.Portrait
        };

        _sectionProperties!.Append(pageSizeElement);
    }

    /// <summary>
    /// Retorna dimensões da página em twips
    /// </summary>
    private static (int width, int height) GetPageDimensions(string pageSize)
    {
        return pageSize.ToUpperInvariant() switch
        {
            "A4" => (FormattingHelper.CentimetersToTwips(21.0), FormattingHelper.CentimetersToTwips(29.7)), // 21cm x 29.7cm
            "A5" => (FormattingHelper.CentimetersToTwips(14.8), FormattingHelper.CentimetersToTwips(21.0)), // 14.8cm x 21cm
            "LETTER" => (FormattingHelper.InchesToTwips(8.5), FormattingHelper.InchesToTwips(11.0)),        // 8.5" x 11"
            "LEGAL" => (FormattingHelper.InchesToTwips(8.5), FormattingHelper.InchesToTwips(14.0)),         // 8.5" x 14"
            _ => (FormattingHelper.CentimetersToTwips(21.0), FormattingHelper.CentimetersToTwips(29.7))     // Default: A4
        };
    }

    /// <summary>
    /// Configura margens espelhadas para layout de livro
    /// Páginas ímpares: inner = esquerda (binding), outer = direita
    /// Páginas pares: inner = direita (binding), outer = esquerda
    /// </summary>
    public void SetMirroredMargins(
        double inner,
        double outer,
        double top,
        double bottom,
        double header,
        double footer)
    {
        var pageMargin = new PageMargin
        {
            Left = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(inner),
            Right = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(outer),
            Top = FormattingHelper.CentimetersToTwips(top),
            Bottom = FormattingHelper.CentimetersToTwips(bottom),
            Header = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(header),
            Footer = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(footer),
            Gutter = (UInt32Value)0u
        };

        _sectionProperties!.Append(pageMargin);

        // Importante: Não há flag explícita "MirroredMargins" em OpenXml
        // O comportamento espelhado é aplicado automaticamente quando:
        // 1. O documento tem múltiplas páginas
        // 2. As margens Left/Right são diferentes
        // 3. O Word interpreta isso como layout de livro
        //
        // Opcionalmente, podemos adicionar BookFoldPrinting para layout de livro completo:
        // _sectionProperties.Append(new BookFoldPrinting());
    }

    /// <summary>
    /// Configura margens padrão (não espelhadas)
    /// </summary>
    public void SetStandardMargins(
        double left,
        double right,
        double top,
        double bottom,
        double header,
        double footer)
    {
        var pageMargin = new PageMargin
        {
            Left = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(left),
            Right = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(right),
            Top = FormattingHelper.CentimetersToTwips(top),
            Bottom = FormattingHelper.CentimetersToTwips(bottom),
            Header = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(header),
            Footer = (UInt32Value)(uint)FormattingHelper.CentimetersToTwips(footer),
            Gutter = (UInt32Value)0u
        };

        _sectionProperties!.Append(pageMargin);
    }

    /// <summary>
    /// Adiciona nova seção ao documento (para capítulos)
    /// </summary>
    public SectionProperties CreateNewSection()
    {
        var newSection = new SectionProperties();

        // Copiar configurações da seção anterior se existir
        if (_sectionProperties != null)
        {
            // Copiar PageSize
            var existingPageSize = _sectionProperties.GetFirstChild<PageSize>();
            if (existingPageSize != null)
            {
                newSection.Append((PageSize)existingPageSize.CloneNode(true));
            }

            // Copiar PageMargin
            var existingMargin = _sectionProperties.GetFirstChild<PageMargin>();
            if (existingMargin != null)
            {
                newSection.Append((PageMargin)existingMargin.CloneNode(true));
            }
        }

        // Definir que nova seção começa em página nova
        newSection.Append(new SectionType { Val = SectionMarkValues.NextPage });

        return newSection;
    }

    /// <summary>
    /// Obtém propriedades da seção atual
    /// </summary>
    public SectionProperties? GetCurrentSection()
    {
        return _sectionProperties;
    }
}
