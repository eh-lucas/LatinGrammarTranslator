namespace LatinGrammarTranslator.Services;
public class TranslationService
{
    private readonly IHttpClientFactory _clientFactory;

    public TranslationService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<object> TranslateStructure(object structuredNodes)
    {
        var client = _clientFactory.CreateClient("translator");

        var response = await client.PostAsJsonAsync("/translate", structuredNodes);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<object>();
    }
}

