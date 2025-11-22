using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using LatinGrammarTranslator.Services.WordGeneration.Builders;
using LatinGrammarTranslator.Services.WordGeneration.Configuration;
using LatinGrammarTranslator.Services.WordGeneration.Layout;
using LatinGrammarTranslator.Services.WordGeneration.Styles;

namespace LatinGrammarTranslator.Services.WordGeneration;

/// <summary>
/// Builder principal para criar documentos Word com Fluent API
/// </summary>
public class WordDocumentBuilder : IDisposable
{
    private readonly WordprocessingDocument _document;
    private readonly MainDocumentPart _mainPart;
    private readonly Body _body;
    private readonly StylesManager _stylesManager;
    private readonly PageLayoutManager _layoutManager;
    private ThemeConfiguration? _currentTheme;
    private bool _isInitialized = false;
    private bool _disposed = false;

    /// <summary>
    /// Construtor privado (use Create ou CreateFromTheme)
    /// </summary>
    private WordDocumentBuilder(string filePath)
    {
        // Criar documento
        _document = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);

        // Criar parte principal
        _mainPart = _document.AddMainDocumentPart();
        _mainPart.Document = new Document();
        _body = _mainPart.Document.AppendChild(new Body());

        // Inicializar gerenciadores
        _stylesManager = new StylesManager(_mainPart);
        _layoutManager = new PageLayoutManager(_body);
    }

    /// <summary>
    /// Cria novo documento
    /// </summary>
    public static WordDocumentBuilder Create(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        return new WordDocumentBuilder(filePath);
    }

    /// <summary>
    /// Cria documento a partir de tema
    /// </summary>
    public static WordDocumentBuilder CreateFromTheme(string filePath, string themesDirectory, string themeName)
    {
        var builder = Create(filePath);
        var loader = new ConfigurationLoader(themesDirectory);
        var theme = loader.LoadTheme(themeName);

        builder.LoadTheme(theme);

        return builder;
    }

    /// <summary>
    /// Carrega tema
    /// </summary>
    public WordDocumentBuilder LoadTheme(ThemeConfiguration theme)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        _currentTheme = theme;

        // Aplicar estilos
        _stylesManager.ApplyTheme(theme);

        // Aplicar layout
        _layoutManager.ApplyConfiguration(theme.PageLayout);

        _isInitialized = true;

        return this;
    }

    /// <summary>
    /// Carrega tema de arquivo
    /// </summary>
    public WordDocumentBuilder LoadThemeFromFile(string themesDirectory, string themeName)
    {
        var loader = new ConfigurationLoader(themesDirectory);
        var theme = loader.LoadTheme(themeName);

        return LoadTheme(theme);
    }

    /// <summary>
    /// Adiciona página de título
    /// </summary>
    public WordDocumentBuilder AddTitlePage(string title, string? subtitle = null)
    {
        // Título principal - centralizado
        var titlePara = new ParagraphBuilder()
            .WithStyle("Heading1")
            .Align("center")
            .AddText(title)
            .Build();

        _body.Append(titlePara);

        // Subtítulo se fornecido
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            var subtitlePara = new ParagraphBuilder()
                .Align("center")
                .AddRun(r => r
                    .WithText(subtitle)
                    .Italic()
                    .FontSize(14))
                .Build();

            _body.Append(subtitlePara);
        }

        // Espaço
        _body.Append(new Paragraph());

        return this;
    }

    /// <summary>
    /// Adiciona heading (título)
    /// </summary>
    public WordDocumentBuilder AddHeading(string text, int level = 1)
    {
        if (level < 1 || level > 4)
        {
            throw new ArgumentException("Heading level must be between 1 and 4", nameof(level));
        }

        var heading = ParagraphBuilder.Heading(text, level);
        _body.Append(heading);

        return this;
    }

    /// <summary>
    /// Adiciona parágrafo simples
    /// </summary>
    public WordDocumentBuilder AddParagraph(string text, string? styleId = null)
    {
        var paragraph = ParagraphBuilder.Simple(text, styleId);
        _body.Append(paragraph);

        return this;
    }

    /// <summary>
    /// Adiciona parágrafo com builder customizado
    /// </summary>
    public WordDocumentBuilder AddParagraph(Action<ParagraphBuilder> configureParagraph)
    {
        var builder = new ParagraphBuilder();
        configureParagraph(builder);
        _body.Append(builder.Build());

        return this;
    }

    /// <summary>
    /// Adiciona parágrafo com número de seção
    /// </summary>
    public WordDocumentBuilder AddSectionParagraph(string sectionNumber, string text)
    {
        var paragraph = new ParagraphBuilder()
            .WithStyle("Normal")
            .WithSectionNumber(sectionNumber)
            .AddText(text)
            .Build();

        _body.Append(paragraph);

        return this;
    }

    /// <summary>
    /// Adiciona texto latino (aplicará estilo de itálico automaticamente)
    /// </summary>
    public WordDocumentBuilder AddLatinText(string latinText)
    {
        var paragraph = new ParagraphBuilder()
            .WithStyle("LatinText")
            .AddText(latinText)
            .Build();

        _body.Append(paragraph);

        return this;
    }

    /// <summary>
    /// Adiciona tabela
    /// </summary>
    public WordDocumentBuilder AddTable(Action<TableBuilder> configureTable)
    {
        var builder = new TableBuilder();
        configureTable(builder);
        _body.Append(builder.Build());

        return this;
    }

    /// <summary>
    /// Adiciona tabela simples
    /// </summary>
    public WordDocumentBuilder AddTable(string[] headers, string[][] data)
    {
        var table = TableBuilder.Simple(headers, data);
        _body.Append(table);

        return this;
    }

    /// <summary>
    /// Adiciona quebra de página
    /// </summary>
    public WordDocumentBuilder AddPageBreak()
    {
        var paragraph = new Paragraph(
            new Run(
                new Break { Type = BreakValues.Page }
            )
        );

        _body.Append(paragraph);

        return this;
    }

    /// <summary>
    /// Adiciona espaço (parágrafo vazio)
    /// </summary>
    public WordDocumentBuilder AddSpace(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            _body.Append(new Paragraph());
        }

        return this;
    }

    /// <summary>
    /// Inicia novo capítulo (quebra de seção)
    /// </summary>
    public WordDocumentBuilder StartChapter(string chapterTitle)
    {
        // Adicionar quebra de página antes do capítulo
        AddPageBreak();

        // Adicionar título do capítulo
        AddHeading(chapterTitle, 1);

        return this;
    }

    /// <summary>
    /// Adiciona blockquote (citação)
    /// </summary>
    public WordDocumentBuilder AddBlockquote(string text)
    {
        var paragraph = new ParagraphBuilder()
            .WithStyle("Blockquote")
            .AddText(text)
            .Build();

        _body.Append(paragraph);

        return this;
    }

    /// <summary>
    /// Obtém o body do documento para manipulações avançadas
    /// </summary>
    public Body GetBody() => _body;

    /// <summary>
    /// Obtém o gerenciador de estilos para customizações
    /// </summary>
    public StylesManager GetStylesManager() => _stylesManager;

    /// <summary>
    /// Obtém o gerenciador de layout
    /// </summary>
    public PageLayoutManager GetLayoutManager() => _layoutManager;

    /// <summary>
    /// Salva o documento
    /// </summary>
    public void Save()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Document must be initialized with a theme before saving. Call LoadTheme() or LoadThemeFromFile() first.");
        }

        // Salvar estilos
        _stylesManager.Save();

        // Salvar documento
        _document.Save();
    }

    /// <summary>
    /// Salva e fecha o documento
    /// </summary>
    public void Build()
    {
        Save();
        Dispose();
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _document?.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
