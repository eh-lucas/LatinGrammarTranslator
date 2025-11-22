using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LatinGrammarTranslator.Services.WordGeneration.Builders;

/// <summary>
/// Builder para criar tabelas
/// </summary>
public class TableBuilder
{
    private readonly Table _table;
    private readonly TableProperties _tableProperties;
    private readonly List<TableRow> _rows = new();
    private int _columnCount = 0;

    public TableBuilder()
    {
        _table = new Table();
        _tableProperties = new TableProperties();

        // Configuração padrão: largura 100%
        _tableProperties.Append(new TableWidth
        {
            Width = "5000",
            Type = TableWidthUnitValues.Pct
        });
    }

    /// <summary>
    /// Define estilo da tabela
    /// </summary>
    public TableBuilder WithStyle(string tableStyleId)
    {
        _tableProperties.Append(new TableStyle { Val = tableStyleId });
        return this;
    }

    /// <summary>
    /// Define largura da tabela em porcentagem (0-100)
    /// </summary>
    public TableBuilder WithWidth(int percentage)
    {
        var width = _tableProperties.GetFirstChild<TableWidth>();
        if (width != null)
        {
            width.Width = (percentage * 50).ToString(); // OpenXml usa 5000 para 100%
        }

        return this;
    }

    /// <summary>
    /// Adiciona bordas à tabela
    /// </summary>
    public TableBuilder WithBorders(bool showBorders = true, string color = "000000", uint size = 12)
    {
        if (showBorders)
        {
            var borders = new TableBorders(
                new TopBorder { Val = BorderValues.Single, Color = color, Size = size },
                new BottomBorder { Val = BorderValues.Single, Color = color, Size = size },
                new LeftBorder { Val = BorderValues.Single, Color = color, Size = size },
                new RightBorder { Val = BorderValues.Single, Color = color, Size = size },
                new InsideHorizontalBorder { Val = BorderValues.Single, Color = color, Size = (UInt32Value)(size / 2) },
                new InsideVerticalBorder { Val = BorderValues.Single, Color = color, Size = (UInt32Value)(size / 2) }
            );

            _tableProperties.Append(borders);
        }

        return this;
    }

    /// <summary>
    /// Adiciona linha de cabeçalho
    /// </summary>
    public TableBuilder AddHeaderRow(params string[] cellTexts)
    {
        if (cellTexts == null || cellTexts.Length == 0)
        {
            throw new ArgumentException("Header row must have at least one cell", nameof(cellTexts));
        }

        if (_columnCount == 0)
        {
            _columnCount = cellTexts.Length;
        }

        var row = new TableRow();

        foreach (var text in cellTexts)
        {
            var cell = CreateHeaderCell(text);
            row.Append(cell);
        }

        _rows.Add(row);
        return this;
    }

    /// <summary>
    /// Adiciona linha de dados
    /// </summary>
    public TableBuilder AddRow(params string[] cellTexts)
    {
        if (cellTexts == null || cellTexts.Length == 0)
        {
            throw new ArgumentException("Row must have at least one cell", nameof(cellTexts));
        }

        if (_columnCount == 0)
        {
            _columnCount = cellTexts.Length;
        }

        var row = new TableRow();

        foreach (var text in cellTexts)
        {
            var cell = CreateDataCell(text);
            row.Append(cell);
        }

        _rows.Add(row);
        return this;
    }

    /// <summary>
    /// Adiciona linha customizada
    /// </summary>
    public TableBuilder AddCustomRow(params TableCell[] cells)
    {
        if (cells == null || cells.Length == 0)
        {
            throw new ArgumentException("Row must have at least one cell", nameof(cells));
        }

        if (_columnCount == 0)
        {
            _columnCount = cells.Length;
        }

        var row = new TableRow();

        foreach (var cell in cells)
        {
            row.Append(cell);
        }

        _rows.Add(row);
        return this;
    }

    /// <summary>
    /// Cria célula de cabeçalho
    /// </summary>
    private TableCell CreateHeaderCell(string text)
    {
        var cell = new TableCell();

        // Propriedades da célula (background, alinhamento, etc)
        var cellProperties = new TableCellProperties(
            new Shading
            {
                Val = ShadingPatternValues.Clear,
                Fill = "D9E2F3" // Azul claro
            },
            new TableCellVerticalAlignment
            {
                Val = TableVerticalAlignmentValues.Center
            }
        );

        cell.Append(cellProperties);

        // Parágrafo com texto em negrito e centralizado
        var paragraph = new ParagraphBuilder()
            .Align("center")
            .AddRun(RunBuilder.BoldText(text))
            .Build();

        cell.Append(paragraph);

        return cell;
    }

    /// <summary>
    /// Cria célula de dados
    /// </summary>
    private TableCell CreateDataCell(string text)
    {
        var cell = new TableCell();

        // Propriedades básicas da célula
        var cellProperties = new TableCellProperties(
            new TableCellVerticalAlignment
            {
                Val = TableVerticalAlignmentValues.Top
            }
        );

        cell.Append(cellProperties);

        // Parágrafo simples
        var paragraph = ParagraphBuilder.Simple(text);
        cell.Append(paragraph);

        return cell;
    }

    /// <summary>
    /// Constrói a tabela final
    /// </summary>
    public Table Build()
    {
        // Adicionar propriedades
        _table.Append(_tableProperties);

        // Adicionar linhas
        foreach (var row in _rows)
        {
            _table.Append(row);
        }

        return _table;
    }

    /// <summary>
    /// Helper para criar tabela simples
    /// </summary>
    public static Table Simple(string[] headers, string[][] data)
    {
        var builder = new TableBuilder()
            .WithBorders()
            .AddHeaderRow(headers);

        foreach (var row in data)
        {
            builder.AddRow(row);
        }

        return builder.Build();
    }
}
