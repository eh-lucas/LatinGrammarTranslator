using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Configuration;

namespace LatinGrammarTranslator.Services.WordGeneration.Styles;

/// <summary>
/// Gerencia estilos do documento Word
/// </summary>
public class StylesManager
{
    private readonly StyleDefinitionsPart _stylesPart;
    private readonly DocumentFormat.OpenXml.Wordprocessing.Styles _styles;
    private readonly Dictionary<string, Style> _createdStyles = new();

    public StylesManager(MainDocumentPart mainPart)
    {
        _stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
        _styles = new DocumentFormat.OpenXml.Wordprocessing.Styles();
        _stylesPart.Styles = _styles;

        // Adicionar estilos padrão do Word
        AddDefaultStyles();
    }

    /// <summary>
    /// Adiciona estilos padrão necessários
    /// </summary>
    private void AddDefaultStyles()
    {
        // Estilo Normal (base para todos os outros)
        var normalStyle = new Style
        {
            Type = StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };
        normalStyle.Append(new StyleName { Val = "Normal" });
        normalStyle.Append(new PrimaryStyle());

        _styles.Append(normalStyle);
        _createdStyles["Normal"] = normalStyle;
    }

    /// <summary>
    /// Aplica tema completo aos estilos do documento
    /// </summary>
    public void ApplyTheme(ThemeConfiguration theme)
    {
        var styles = theme.Styles;

        // Estilos de parágrafo
        AddOrUpdateStyle("Normal", "Normal", styles.Normal);
        AddOrUpdateStyle("Heading1", "Heading 1", styles.Heading1);
        AddOrUpdateStyle("Heading2", "Heading 2", styles.Heading2);
        AddOrUpdateStyle("Heading3", "Heading 3", styles.Heading3);
        AddOrUpdateStyle("Heading4", "Heading 4", styles.Heading4);
        AddOrUpdateStyle("LatinText", "Latin Text", styles.LatinText);
        AddOrUpdateStyle("Gloss", "Gloss", styles.Gloss);
        AddOrUpdateStyle("Note", "Note", styles.Note);
        AddOrUpdateStyle("SectionNumber", "Section Number", styles.SectionNumber);
        AddOrUpdateStyle("Blockquote", "Blockquote", styles.Blockquote);

        // Estilos de tabela
        AddOrUpdateStyle("TableHeader", "Table Header", styles.TableHeader);
        AddOrUpdateStyle("TableCell", "Table Cell", styles.TableCell);
    }

    /// <summary>
    /// Adiciona ou atualiza estilo
    /// </summary>
    private void AddOrUpdateStyle(string styleId, string styleName, StyleConfiguration config)
    {
        // Se já existe, remover
        if (_createdStyles.ContainsKey(styleId))
        {
            var existing = _createdStyles[styleId];
            _styles.RemoveChild(existing);
            _createdStyles.Remove(styleId);
        }

        // Criar novo estilo
        var styleDefinition = StyleDefinition.CreateParagraphStyle(styleId, styleName)
            .ApplyConfiguration(config);

        var style = styleDefinition.Build();

        _styles.Append(style);
        _createdStyles[styleId] = style;
    }

    /// <summary>
    /// Adiciona estilo de parágrafo customizado
    /// </summary>
    public void AddParagraphStyle(string styleId, string styleName, Action<StyleDefinition> configure)
    {
        var styleDefinition = StyleDefinition.CreateParagraphStyle(styleId, styleName);
        configure(styleDefinition);

        var style = styleDefinition.Build();
        _styles.Append(style);
        _createdStyles[styleId] = style;
    }

    /// <summary>
    /// Adiciona estilo de caractere customizado
    /// </summary>
    public void AddCharacterStyle(string styleId, string styleName, Action<StyleDefinition> configure)
    {
        var styleDefinition = StyleDefinition.CreateCharacterStyle(styleId, styleName);
        configure(styleDefinition);

        var style = styleDefinition.Build();
        _styles.Append(style);
        _createdStyles[styleId] = style;
    }

    /// <summary>
    /// Verifica se estilo existe
    /// </summary>
    public bool HasStyle(string styleId)
    {
        return _createdStyles.ContainsKey(styleId);
    }

    /// <summary>
    /// Salva os estilos (chamado automaticamente no dispose do document)
    /// </summary>
    public void Save()
    {
        _stylesPart.Styles.Save();
    }
}
