namespace LatinGrammarTranslator.Services.WordGeneration.Configuration;

/// <summary>
/// Configuração de layout e página do documento
/// </summary>
public class DocumentConfiguration
{
    /// <summary>
    /// Tamanho da página (A4, Letter, etc)
    /// </summary>
    public string PageSize { get; set; } = "A4";

    /// <summary>
    /// Se true, usa margens espelhadas (inner/outer) para layout de livro
    /// </summary>
    public bool MirroredMargins { get; set; } = false;

    /// <summary>
    /// Margem interna (binding side) em centímetros
    /// </summary>
    public double MarginInner { get; set; } = 2.5;

    /// <summary>
    /// Margem externa (page edge) em centímetros
    /// </summary>
    public double MarginOuter { get; set; } = 2.0;

    /// <summary>
    /// Margem superior em centímetros
    /// </summary>
    public double MarginTop { get; set; } = 2.5;

    /// <summary>
    /// Margem inferior em centímetros
    /// </summary>
    public double MarginBottom { get; set; } = 2.5;

    /// <summary>
    /// Margem de header em centímetros
    /// </summary>
    public double MarginHeader { get; set; } = 1.25;

    /// <summary>
    /// Margem de footer em centímetros
    /// </summary>
    public double MarginFooter { get; set; } = 1.25;

    /// <summary>
    /// Orientação da página: "portrait" ou "landscape"
    /// </summary>
    public string Orientation { get; set; } = "portrait";
}
