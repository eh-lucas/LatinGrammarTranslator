namespace LatinGrammarTranslator.Services.WordGeneration.Configuration;

/// <summary>
/// Tema completo com layout e estilos
/// </summary>
public class ThemeConfiguration
{
    /// <summary>
    /// Nome do tema (ex: "Academic", "Modern", "Compact", "Classic")
    /// </summary>
    public string Name { get; set; } = "Default";

    /// <summary>
    /// Descrição do tema
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Configuração de layout e página
    /// </summary>
    public DocumentConfiguration PageLayout { get; set; } = new();

    /// <summary>
    /// Configuração de estilos
    /// </summary>
    public StylesConfiguration Styles { get; set; } = new();
}
