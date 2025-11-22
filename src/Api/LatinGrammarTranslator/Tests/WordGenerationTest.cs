using LatinGrammarTranslator.Models;
using LatinGrammarTranslator.Services.WordGeneration;

namespace LatinGrammarTranslator.Tests;

/// <summary>
/// Teste simples para verificar geração de documentos Word
/// </summary>
public static class WordGenerationTest
{
    public static void RunTest()
    {
        Console.WriteLine("=== Teste de Geração de Documentos Word ===\n");

        // Criar documento de teste
        var parsedDoc = CreateSampleDocument();

        // Testar cada tema
        var themes = new[] { "academic", "modern", "compact", "classic" };
        var service = new DocumentGenerationService();

        foreach (var theme in themes)
        {
            try
            {
                Console.WriteLine($"Testando tema: {theme}");

                var outputPath = Path.Combine(
                    Path.GetTempPath(),
                    "LatinGrammarTranslator",
                    $"test_{theme}.docx");

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                service.GenerateDocument(parsedDoc, outputPath, theme);

                var fileInfo = new FileInfo(outputPath);
                Console.WriteLine($"✓ Documento gerado: {outputPath}");
                Console.WriteLine($"  Tamanho: {fileInfo.Length / 1024.0:F2} KB\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Erro ao gerar tema {theme}: {ex.Message}\n");
            }
        }

        Console.WriteLine("=== Teste Concluído ===");
    }

    private static ParsedDocument CreateSampleDocument()
    {
        return new ParsedDocument
        {
            Title = "Latin Grammar Test Document",
            Encoding = "utf-8",
            Nodes = new List<ParsedNode>
            {
                // Heading 1
                new ParsedNode
                {
                    NodeType = NodeType.HEADING_1,
                    SectionNumber = "1",
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "Introduction to Latin Grammar",
                            TextType = TextType.ENGLISH
                        }
                    }
                },

                // Paragraph
                new ParsedNode
                {
                    NodeType = NodeType.PARAGRAPH,
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "Este é um documento de teste para verificar a geração de documentos Word a partir de estruturas parseadas.",
                            TextType = TextType.GLOSS
                        }
                    }
                },

                // Heading 2
                new ParsedNode
                {
                    NodeType = NodeType.HEADING_2,
                    SectionNumber = "1.1",
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "Latin Text Example",
                            TextType = TextType.ENGLISH
                        }
                    }
                },

                // Latin paragraph
                new ParsedNode
                {
                    NodeType = NodeType.PARAGRAPH,
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                            TextType = TextType.LATIN,
                            Formatting = new FormattingStyle
                            {
                                Italic = true
                            }
                        }
                    },
                    Attributes = new Dictionary<string, string>
                    {
                        { "class", "foreign" }
                    }
                },

                // Translation paragraph
                new ParsedNode
                {
                    NodeType = NodeType.PARAGRAPH,
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "O texto acima é um exemplo clássico de texto em latim usado como placeholder.",
                            TextType = TextType.GLOSS
                        }
                    }
                },

                // Heading 2
                new ParsedNode
                {
                    NodeType = NodeType.HEADING_2,
                    SectionNumber = "1.2",
                    TextSegments = new List<TextSegment>
                    {
                        new TextSegment
                        {
                            Text = "Tables and Lists",
                            TextType = TextType.ENGLISH
                        }
                    }
                },

                // Table
                new ParsedNode
                {
                    NodeType = NodeType.TABLE,
                    Children = new List<ParsedNode>
                    {
                        // Header row
                        new ParsedNode
                        {
                            NodeType = NodeType.TABLE_ROW,
                            Children = new List<ParsedNode>
                            {
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_HEADER,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "Caso", TextType = TextType.ENGLISH }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_HEADER,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "Singular", TextType = TextType.ENGLISH }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_HEADER,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "Plural", TextType = TextType.ENGLISH }
                                    }
                                }
                            }
                        },
                        // Data rows
                        new ParsedNode
                        {
                            NodeType = NodeType.TABLE_ROW,
                            Children = new List<ParsedNode>
                            {
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "Nominativo", TextType = TextType.GLOSS }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "rosa", TextType = TextType.LATIN }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "rosae", TextType = TextType.LATIN }
                                    }
                                }
                            }
                        },
                        new ParsedNode
                        {
                            NodeType = NodeType.TABLE_ROW,
                            Children = new List<ParsedNode>
                            {
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "Genitivo", TextType = TextType.GLOSS }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "rosae", TextType = TextType.LATIN }
                                    }
                                },
                                new ParsedNode
                                {
                                    NodeType = NodeType.TABLE_CELL,
                                    TextSegments = new List<TextSegment>
                                    {
                                        new TextSegment { Text = "rosarum", TextType = TextType.LATIN }
                                    }
                                }
                            }
                        }
                    }
                },

                // List
                new ParsedNode
                {
                    NodeType = NodeType.LIST_UNORDERED,
                    Children = new List<ParsedNode>
                    {
                        new ParsedNode
                        {
                            NodeType = NodeType.LIST_ITEM,
                            TextSegments = new List<TextSegment>
                            {
                                new TextSegment { Text = "Primeira declinação", TextType = TextType.GLOSS }
                            }
                        },
                        new ParsedNode
                        {
                            NodeType = NodeType.LIST_ITEM,
                            TextSegments = new List<TextSegment>
                            {
                                new TextSegment { Text = "Segunda declinação", TextType = TextType.GLOSS }
                            }
                        },
                        new ParsedNode
                        {
                            NodeType = NodeType.LIST_ITEM,
                            TextSegments = new List<TextSegment>
                            {
                                new TextSegment { Text = "Terceira declinação", TextType = TextType.GLOSS }
                            }
                        }
                    }
                }
            },
            Stats = new Dictionary<string, object>
            {
                { "total_nodes", 8 },
                { "text_segments", 15 },
                { "tables", 1 },
                { "lists", 1 }
            }
        };
    }
}
