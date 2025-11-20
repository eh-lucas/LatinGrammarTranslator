using AngleSharp;
using Microsoft.Extensions.Configuration;

namespace LatinGrammarTranslator.Services;
public class HtmlService
{
    public object ParseHtml(string html)
    {
        var context = BrowsingContext.New(Configuration.Default);
        var doc = context.OpenAsync(req => req.Content(html)).Result;

        // exemplo simples: retorna uma lista de nodes estruturados
        var nodes = doc.Body.Children.Select(c => new {
            Tag = c.TagName.ToLower(),
            Text = c.TextContent,
            Html = c.OuterHtml
        });

        return nodes;
    }
}

