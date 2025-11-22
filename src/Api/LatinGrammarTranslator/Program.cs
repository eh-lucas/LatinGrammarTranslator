using LatinGrammarTranslator.Services;
using LatinGrammarTranslator.Services.WordGeneration;
using LatinGrammarTranslator.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("translator", c =>
{
    c.BaseAddress = new Uri("http://translator:5001"); // container python
});

builder.Services.AddSingleton<HtmlService>();
builder.Services.AddScoped<TranslationService>();
builder.Services.AddSingleton<DocumentGenerationService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapPost("/process", async (
    TranslationService translationService,
    HtmlService htmlService,
    TranslationRequest req) =>
{
    var structured = htmlService.ParseHtml(req.Html);

    var translated = await translationService.TranslateStructure(structured);

    return Results.Ok(translated);
});

app.MapPost("/generate-word", (
    DocumentGenerationService docGenService,
    ParsedDocument parsedDoc,
    string? themeName = null) =>
{
    try
    {
        // Usar tema padrão se não especificado
        themeName ??= "academic";

        // Criar diretório temporário para output
        var outputDir = Path.Combine(Path.GetTempPath(), "LatinGrammarTranslator");
        Directory.CreateDirectory(outputDir);

        // Gerar nome do arquivo baseado no título do documento
        var fileName = !string.IsNullOrWhiteSpace(parsedDoc.Title)
            ? $"{SanitizeFileName(parsedDoc.Title)}.docx"
            : $"document_{DateTime.Now:yyyyMMddHHmmss}.docx";

        var outputPath = Path.Combine(outputDir, fileName);

        // Gerar documento
        docGenService.GenerateDocument(parsedDoc, outputPath, themeName);

        // Retornar arquivo para download
        var fileBytes = File.ReadAllBytes(outputPath);

        return Results.File(
            fileBytes,
            contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            fileDownloadName: fileName);
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error generating Word document");
    }
});

app.MapGet("/themes", (DocumentGenerationService docGenService) =>
{
    var themes = docGenService.GetAvailableThemes();
    return Results.Ok(new { themes });
});

app.MapGet("/test-word-generation", () =>
{
    try
    {
        LatinGrammarTranslator.Tests.WordGenerationTest.RunTest();
        return Results.Ok(new { message = "Test completed. Check console output for details." });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error running test");
    }
});

app.Run();

static string SanitizeFileName(string fileName)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
}