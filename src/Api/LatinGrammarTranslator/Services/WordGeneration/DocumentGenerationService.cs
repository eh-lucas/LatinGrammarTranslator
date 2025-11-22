using LatinGrammarTranslator.Models;
using LatinGrammarTranslator.Services.WordGeneration.Configuration;

namespace LatinGrammarTranslator.Services.WordGeneration;

/// <summary>
/// Serviço principal para gerar documentos Word a partir de ParsedDocument
/// </summary>
public class DocumentGenerationService
{
    private readonly ConfigurationLoader _configLoader;
    private readonly string _themesPath;

    public DocumentGenerationService(string? themesPath = null)
    {
        _themesPath = themesPath ?? Path.Combine(AppContext.BaseDirectory, "Themes");
        _configLoader = new ConfigurationLoader(_themesPath);
    }

    /// <summary>
    /// Gera documento Word a partir de ParsedDocument traduzido
    /// </summary>
    /// <param name="parsedDoc">Documento parseado e traduzido</param>
    /// <param name="outputPath">Caminho para salvar o documento</param>
    /// <param name="themeName">Nome do tema (padrão: "academic")</param>
    public void GenerateDocument(ParsedDocument parsedDoc, string outputPath, string themeName = "academic")
    {
        if (parsedDoc == null)
            throw new ArgumentNullException(nameof(parsedDoc));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path cannot be empty", nameof(outputPath));

        // Carregar tema
        var theme = _configLoader.LoadTheme(themeName);

        // Criar builder de documento
        using var docBuilder = WordDocumentBuilder.Create(outputPath);

        // Aplicar tema
        docBuilder.LoadTheme(theme);

        // Adicionar página de título se houver
        if (!string.IsNullOrWhiteSpace(parsedDoc.Title))
        {
            docBuilder.AddTitlePage(parsedDoc.Title);
        }

        // Processar todos os nós do documento
        foreach (var node in parsedDoc.Nodes)
        {
            ProcessNode(docBuilder, node);
        }

        // Finalizar documento
        docBuilder.Build();
    }

    /// <summary>
    /// Processa um nó e seus filhos recursivamente
    /// </summary>
    private void ProcessNode(WordDocumentBuilder docBuilder, ParsedNode node)
    {
        switch (node.NodeType)
        {
            case NodeType.HEADING_1:
                ProcessHeading(docBuilder, node, 1);
                break;

            case NodeType.HEADING_2:
                ProcessHeading(docBuilder, node, 2);
                break;

            case NodeType.HEADING_3:
                ProcessHeading(docBuilder, node, 3);
                break;

            case NodeType.HEADING_4:
                ProcessHeading(docBuilder, node, 4);
                break;

            case NodeType.PARAGRAPH:
                ProcessParagraph(docBuilder, node);
                break;

            case NodeType.TABLE:
                ProcessTable(docBuilder, node);
                break;

            case NodeType.LIST_ORDERED:
            case NodeType.LIST_UNORDERED:
                ProcessList(docBuilder, node);
                break;

            case NodeType.BLOCKQUOTE:
                ProcessBlockquote(docBuilder, node);
                break;

            case NodeType.DIV:
            case NodeType.SPAN:
                // Processar filhos diretamente para containers genéricos
                foreach (var child in node.Children)
                {
                    ProcessNode(docBuilder, child);
                }
                break;

            // Nós que não geram elementos diretos no Word (já processados pelos pais)
            case NodeType.LIST_ITEM:
            case NodeType.TABLE_ROW:
            case NodeType.TABLE_CELL:
            case NodeType.TABLE_HEADER:
            case NodeType.STRONG:
            case NodeType.EMPHASIS:
            case NodeType.LINK:
                // Estes são processados dentro de seus contextos específicos
                break;
        }
    }

    /// <summary>
    /// Processa cabeçalho (heading)
    /// </summary>
    private void ProcessHeading(WordDocumentBuilder docBuilder, ParsedNode node, int level)
    {
        var text = ExtractTextFromNode(node);

        if (!string.IsNullOrWhiteSpace(text))
        {
            // Adicionar número de seção se existir
            if (!string.IsNullOrWhiteSpace(node.SectionNumber))
            {
                text = $"{node.SectionNumber}. {text}";
            }

            docBuilder.AddHeading(text, level);
        }

        // Processar filhos (pode ter sub-elementos)
        foreach (var child in node.Children)
        {
            ProcessNode(docBuilder, child);
        }
    }

    /// <summary>
    /// Processa parágrafo
    /// </summary>
    private void ProcessParagraph(WordDocumentBuilder docBuilder, ParsedNode node)
    {
        var text = ExtractTextFromNode(node);

        if (!string.IsNullOrWhiteSpace(text))
        {
            // Determinar estilo baseado em classes e atributos
            string? styleId = null;

            // Se tem número de seção, usar estilo de seção
            if (!string.IsNullOrWhiteSpace(node.SectionNumber))
            {
                styleId = "Section";
            }
            // Se é nota de rodapé
            else if (node.IsFootnote)
            {
                styleId = "Footnote";
            }
            // Se tem classe "foreign" (latim), usar estilo latin
            else if (node.Attributes.TryGetValue("class", out var className) &&
                     className?.Contains("foreign") == true)
            {
                styleId = "Latin";
            }

            docBuilder.AddParagraph(text, styleId);
        }

        // Processar filhos
        foreach (var child in node.Children)
        {
            ProcessNode(docBuilder, child);
        }
    }

    /// <summary>
    /// Processa tabela
    /// </summary>
    private void ProcessTable(WordDocumentBuilder docBuilder, ParsedNode node)
    {
        docBuilder.AddTable(tableBuilder =>
        {
            tableBuilder.WithBorders(true);

            bool isFirstRow = true;
            foreach (var rowNode in node.Children.Where(n => n.NodeType == NodeType.TABLE_ROW))
            {
                var cells = rowNode.Children
                    .Where(n => n.NodeType == NodeType.TABLE_CELL || n.NodeType == NodeType.TABLE_HEADER)
                    .Select(cellNode => ExtractTextFromNode(cellNode))
                    .ToArray();

                if (cells.Length > 0)
                {
                    // Primeira linha pode ser cabeçalho
                    if (isFirstRow && rowNode.Children.Any(n => n.NodeType == NodeType.TABLE_HEADER))
                    {
                        tableBuilder.AddHeaderRow(cells);
                        isFirstRow = false;
                    }
                    else
                    {
                        tableBuilder.AddRow(cells);
                        isFirstRow = false;
                    }
                }
            }
        });
    }

    /// <summary>
    /// Processa lista (ordenada ou não ordenada)
    /// </summary>
    private void ProcessList(WordDocumentBuilder docBuilder, ParsedNode node)
    {
        var isOrdered = node.NodeType == NodeType.LIST_ORDERED;
        var items = node.Children.Where(n => n.NodeType == NodeType.LIST_ITEM);

        foreach (var item in items)
        {
            var text = ExtractTextFromNode(item);
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Adicionar marcador ou número
                var prefix = isOrdered ? "• " : "◦ ";
                docBuilder.AddParagraph($"{prefix}{text}", "ListParagraph");
            }
        }
    }

    /// <summary>
    /// Processa blockquote (citação)
    /// </summary>
    private void ProcessBlockquote(WordDocumentBuilder docBuilder, ParsedNode node)
    {
        var text = ExtractTextFromNode(node);

        if (!string.IsNullOrWhiteSpace(text))
        {
            docBuilder.AddParagraph(text, "Quote");
        }

        // Processar filhos
        foreach (var child in node.Children)
        {
            ProcessNode(docBuilder, child);
        }
    }

    /// <summary>
    /// Extrai texto de um nó e seus segmentos
    /// </summary>
    private string ExtractTextFromNode(ParsedNode node)
    {
        if (node.TextSegments.Count > 0)
        {
            // Concatenar todos os segmentos de texto
            return string.Join(" ", node.TextSegments.Select(s => s.Text.Trim()));
        }

        // Se não tem segmentos, tentar extrair dos filhos
        if (node.Children.Count > 0)
        {
            return string.Join(" ", node.Children.Select(ExtractTextFromNode));
        }

        return string.Empty;
    }

    /// <summary>
    /// Lista temas disponíveis
    /// </summary>
    public IEnumerable<string> GetAvailableThemes()
    {
        return _configLoader.GetAvailableThemes();
    }
}
