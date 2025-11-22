namespace LatinGrammarTranslator.Services.WordGeneration.Configuration;

/// <summary>
/// Configuração de um estilo de texto (parágrafo ou caractere)
/// </summary>
public class StyleConfiguration
{
    /// <summary>
    /// Nome da família de fonte (ex: "Times New Roman", "Calibri")
    /// </summary>
    public string? FontFamily { get; set; }

    /// <summary>
    /// Tamanho da fonte em pontos
    /// </summary>
    public int? FontSize { get; set; }

    /// <summary>
    /// Texto em negrito
    /// </summary>
    public bool? Bold { get; set; }

    /// <summary>
    /// Texto em itálico
    /// </summary>
    public bool? Italic { get; set; }

    /// <summary>
    /// Texto sublinhado
    /// </summary>
    public bool? Underline { get; set; }

    /// <summary>
    /// Cor do texto em hexadecimal (ex: "000000" para preto)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Cor de fundo em hexadecimal (ex: "FFFF00" para amarelo)
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Alinhamento: "left", "center", "right", "justify"
    /// </summary>
    public string? Alignment { get; set; }

    /// <summary>
    /// Espaçamento entre linhas (ex: 1.0, 1.15, 1.5, 2.0)
    /// </summary>
    public double? LineSpacing { get; set; }

    /// <summary>
    /// Espaço antes do parágrafo em pontos
    /// </summary>
    public int? SpaceBefore { get; set; }

    /// <summary>
    /// Espaço depois do parágrafo em pontos
    /// </summary>
    public int? SpaceAfter { get; set; }

    /// <summary>
    /// Indentação à esquerda em centímetros
    /// </summary>
    public double? IndentLeft { get; set; }

    /// <summary>
    /// Indentação à direita em centímetros
    /// </summary>
    public double? IndentRight { get; set; }

    /// <summary>
    /// Indentação da primeira linha em centímetros
    /// </summary>
    public double? IndentFirstLine { get; set; }
}

/// <summary>
/// Dicionário de estilos para um tema
/// </summary>
public class StylesConfiguration
{
    /// <summary>
    /// Estilo normal (padrão para parágrafos)
    /// </summary>
    public StyleConfiguration Normal { get; set; } = new();

    /// <summary>
    /// Estilo para Heading 1
    /// </summary>
    public StyleConfiguration Heading1 { get; set; } = new();

    /// <summary>
    /// Estilo para Heading 2
    /// </summary>
    public StyleConfiguration Heading2 { get; set; } = new();

    /// <summary>
    /// Estilo para Heading 3
    /// </summary>
    public StyleConfiguration Heading3 { get; set; } = new();

    /// <summary>
    /// Estilo para Heading 4
    /// </summary>
    public StyleConfiguration Heading4 { get; set; } = new();

    /// <summary>
    /// Estilo para texto em latim
    /// </summary>
    public StyleConfiguration LatinText { get; set; } = new();

    /// <summary>
    /// Estilo para glosses/traduções
    /// </summary>
    public StyleConfiguration Gloss { get; set; } = new();

    /// <summary>
    /// Estilo para notas/observações
    /// </summary>
    public StyleConfiguration Note { get; set; } = new();

    /// <summary>
    /// Estilo para número de seção (ex: § 153)
    /// </summary>
    public StyleConfiguration SectionNumber { get; set; } = new();

    /// <summary>
    /// Estilo para cabeçalho de tabela
    /// </summary>
    public StyleConfiguration TableHeader { get; set; } = new();

    /// <summary>
    /// Estilo para células de tabela
    /// </summary>
    public StyleConfiguration TableCell { get; set; } = new();

    /// <summary>
    /// Estilo para blockquote/citação
    /// </summary>
    public StyleConfiguration Blockquote { get; set; } = new();
}
