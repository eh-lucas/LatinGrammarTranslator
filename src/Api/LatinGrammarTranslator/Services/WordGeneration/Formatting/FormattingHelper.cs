namespace LatinGrammarTranslator.Services.WordGeneration.Formatting;

/// <summary>
/// Helper para conversões de unidades e valores de formatação
/// </summary>
public static class FormattingHelper
{
    /// <summary>
    /// Converte pontos (pt) para half-points usados pelo OpenXml
    /// </summary>
    /// <param name="points">Valor em pontos</param>
    /// <returns>Valor em half-points (multiplicado por 2)</returns>
    public static string PointsToHalfPoints(int points)
    {
        return (points * 2).ToString();
    }

    /// <summary>
    /// Converte pontos (pt) para half-points
    /// </summary>
    public static string PointsToHalfPoints(double points)
    {
        return ((int)(points * 2)).ToString();
    }

    /// <summary>
    /// Converte centímetros para twips (twentieths of a point)
    /// 1 cm = 567 twips
    /// </summary>
    /// <param name="centimeters">Valor em centímetros</param>
    /// <returns>Valor em twips</returns>
    public static int CentimetersToTwips(double centimeters)
    {
        return (int)(centimeters * 567);
    }

    /// <summary>
    /// Converte polegadas para twips
    /// 1 inch = 1440 twips
    /// </summary>
    public static int InchesToTwips(double inches)
    {
        return (int)(inches * 1440);
    }

    /// <summary>
    /// Converte pontos para twips
    /// 1 point = 20 twips
    /// </summary>
    public static int PointsToTwips(int points)
    {
        return points * 20;
    }

    /// <summary>
    /// Converte espaçamento de linha (1.0, 1.15, etc) para valor OpenXml
    /// </summary>
    public static string LineSpacingToOpenXml(double lineSpacing)
    {
        // OpenXml usa 240 twips por linha como base
        // 1.0 = 240, 1.15 = 276, 1.5 = 360, 2.0 = 480
        return ((int)(lineSpacing * 240)).ToString();
    }

    /// <summary>
    /// Converte centímetros para DXA (twentieth of a point)
    /// Usado para indentações e margens
    /// </summary>
    public static string CentimetersToDxa(double centimeters)
    {
        return CentimetersToTwips(centimeters).ToString();
    }
}
