using System.Text.RegularExpressions;

namespace LatinGrammarTranslator.Services.WordGeneration.Configuration;

/// <summary>
/// Exceção lançada quando configuração é inválida
/// </summary>
public class InvalidConfigurationException : Exception
{
    public InvalidConfigurationException(string message) : base(message)
    {
    }

    public InvalidConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Validador de configurações de tema
/// </summary>
public static class ConfigurationValidator
{
    private static readonly HashSet<string> ValidPageSizes = new()
    {
        "A4", "A5", "Letter", "Legal"
    };

    private static readonly HashSet<string> ValidAlignments = new()
    {
        "left", "center", "right", "justify"
    };

    private static readonly HashSet<string> ValidOrientations = new()
    {
        "portrait", "landscape"
    };

    // Fontes comuns disponíveis na maioria dos sistemas
    private static readonly HashSet<string> ValidFonts = new()
    {
        "Times New Roman", "Arial", "Calibri", "Cambria", "Georgia",
        "Garamond", "Palatino", "Book Antiqua", "Courier New",
        "Consolas", "Verdana", "Tahoma", "Trebuchet MS"
    };

    /// <summary>
    /// Valida configuração completa do tema
    /// </summary>
    public static void Validate(ThemeConfiguration theme)
    {
        if (theme == null)
        {
            throw new InvalidConfigurationException("Theme configuration cannot be null");
        }

        if (string.IsNullOrWhiteSpace(theme.Name))
        {
            throw new InvalidConfigurationException("Theme name cannot be empty");
        }

        ValidateDocumentConfiguration(theme.PageLayout);
        ValidateStylesConfiguration(theme.Styles);
    }

    /// <summary>
    /// Valida configuração de documento
    /// </summary>
    private static void ValidateDocumentConfiguration(DocumentConfiguration config)
    {
        // Validar tamanho de página
        if (!ValidPageSizes.Contains(config.PageSize))
        {
            throw new InvalidConfigurationException(
                $"Invalid page size '{config.PageSize}'. Valid options: {string.Join(", ", ValidPageSizes)}");
        }

        // Validar orientação
        if (!ValidOrientations.Contains(config.Orientation.ToLowerInvariant()))
        {
            throw new InvalidConfigurationException(
                $"Invalid orientation '{config.Orientation}'. Valid options: {string.Join(", ", ValidOrientations)}");
        }

        // Validar margens (devem ser positivas e razoáveis)
        ValidateMargin(config.MarginInner, "MarginInner");
        ValidateMargin(config.MarginOuter, "MarginOuter");
        ValidateMargin(config.MarginTop, "MarginTop");
        ValidateMargin(config.MarginBottom, "MarginBottom");
        ValidateMargin(config.MarginHeader, "MarginHeader", 0.5, 5);
        ValidateMargin(config.MarginFooter, "MarginFooter", 0.5, 5);

        // Validar que margens não ultrapassam tamanho da página
        double totalVertical = config.MarginTop + config.MarginBottom;
        double totalHorizontal = config.MarginInner + config.MarginOuter;

        if (totalVertical > 20)
        {
            throw new InvalidConfigurationException(
                $"Total vertical margins ({totalVertical}cm) are too large. Must be less than 20cm.");
        }

        if (totalHorizontal > 15)
        {
            throw new InvalidConfigurationException(
                $"Total horizontal margins ({totalHorizontal}cm) are too large. Must be less than 15cm.");
        }
    }

    /// <summary>
    /// Valida valor de margem
    /// </summary>
    private static void ValidateMargin(double value, string name, double min = 0.5, double max = 10)
    {
        if (value < min || value > max)
        {
            throw new InvalidConfigurationException(
                $"{name} must be between {min} and {max} cm. Got: {value}");
        }
    }

    /// <summary>
    /// Valida configuração de estilos
    /// </summary>
    private static void ValidateStylesConfiguration(StylesConfiguration styles)
    {
        // Validar cada estilo
        ValidateStyle(styles.Normal, "Normal");
        ValidateStyle(styles.Heading1, "Heading1");
        ValidateStyle(styles.Heading2, "Heading2");
        ValidateStyle(styles.Heading3, "Heading3");
        ValidateStyle(styles.Heading4, "Heading4");
        ValidateStyle(styles.LatinText, "LatinText");
        ValidateStyle(styles.Gloss, "Gloss");
        ValidateStyle(styles.Note, "Note");
        ValidateStyle(styles.SectionNumber, "SectionNumber");
        ValidateStyle(styles.TableHeader, "TableHeader");
        ValidateStyle(styles.TableCell, "TableCell");
        ValidateStyle(styles.Blockquote, "Blockquote");
    }

    /// <summary>
    /// Valida um estilo individual
    /// </summary>
    private static void ValidateStyle(StyleConfiguration style, string styleName)
    {
        if (style == null) return;

        // Validar fonte
        if (style.FontFamily != null && !ValidFonts.Contains(style.FontFamily))
        {
            throw new InvalidConfigurationException(
                $"Style '{styleName}': Font '{style.FontFamily}' is not in the list of valid fonts. " +
                $"Valid fonts: {string.Join(", ", ValidFonts)}");
        }

        // Validar tamanho de fonte
        if (style.FontSize.HasValue)
        {
            if (style.FontSize.Value < 6 || style.FontSize.Value > 72)
            {
                throw new InvalidConfigurationException(
                    $"Style '{styleName}': FontSize must be between 6 and 72 points. Got: {style.FontSize.Value}");
            }
        }

        // Validar cor
        if (style.Color != null)
        {
            ValidateHexColor(style.Color, styleName, "Color");
        }

        if (style.BackgroundColor != null)
        {
            ValidateHexColor(style.BackgroundColor, styleName, "BackgroundColor");
        }

        // Validar alinhamento
        if (style.Alignment != null && !ValidAlignments.Contains(style.Alignment.ToLowerInvariant()))
        {
            throw new InvalidConfigurationException(
                $"Style '{styleName}': Invalid alignment '{style.Alignment}'. " +
                $"Valid options: {string.Join(", ", ValidAlignments)}");
        }

        // Validar espaçamento entre linhas
        if (style.LineSpacing.HasValue)
        {
            if (style.LineSpacing.Value < 0.5 || style.LineSpacing.Value > 3.0)
            {
                throw new InvalidConfigurationException(
                    $"Style '{styleName}': LineSpacing must be between 0.5 and 3.0. Got: {style.LineSpacing.Value}");
            }
        }

        // Validar espaços antes/depois
        if (style.SpaceBefore.HasValue)
        {
            if (style.SpaceBefore.Value < 0 || style.SpaceBefore.Value > 144)
            {
                throw new InvalidConfigurationException(
                    $"Style '{styleName}': SpaceBefore must be between 0 and 144 points. Got: {style.SpaceBefore.Value}");
            }
        }

        if (style.SpaceAfter.HasValue)
        {
            if (style.SpaceAfter.Value < 0 || style.SpaceAfter.Value > 144)
            {
                throw new InvalidConfigurationException(
                    $"Style '{styleName}': SpaceAfter must be between 0 and 144 points. Got: {style.SpaceAfter.Value}");
            }
        }

        // Validar indentações
        if (style.IndentLeft.HasValue)
        {
            ValidateIndent(style.IndentLeft.Value, styleName, "IndentLeft");
        }

        if (style.IndentRight.HasValue)
        {
            ValidateIndent(style.IndentRight.Value, styleName, "IndentRight");
        }

        if (style.IndentFirstLine.HasValue)
        {
            ValidateIndent(style.IndentFirstLine.Value, styleName, "IndentFirstLine");
        }
    }

    /// <summary>
    /// Valida cor hexadecimal
    /// </summary>
    private static void ValidateHexColor(string color, string styleName, string propertyName)
    {
        var hexPattern = @"^[0-9A-Fa-f]{6}$";
        if (!Regex.IsMatch(color, hexPattern))
        {
            throw new InvalidConfigurationException(
                $"Style '{styleName}': {propertyName} must be a valid 6-digit hex color (e.g., '000000'). Got: '{color}'");
        }
    }

    /// <summary>
    /// Valida indentação
    /// </summary>
    private static void ValidateIndent(double value, string styleName, string propertyName)
    {
        if (value < -5 || value > 10)
        {
            throw new InvalidConfigurationException(
                $"Style '{styleName}': {propertyName} must be between -5 and 10 cm. Got: {value}");
        }
    }
}
