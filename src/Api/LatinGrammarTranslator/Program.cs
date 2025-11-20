var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("translator", c =>
{
    c.BaseAddress = new Uri("http://translator:5001"); // container python
});

builder.Services.AddSingleton<HtmlService>();
builder.Services.AddScoped<TranslationService>();

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

app.Run();