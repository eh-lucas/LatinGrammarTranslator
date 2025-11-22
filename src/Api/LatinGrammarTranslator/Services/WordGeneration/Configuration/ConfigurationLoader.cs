using System.Text.Json;
using System.Text.Json.Serialization;

namespace LatinGrammarTranslator.Services.WordGeneration.Configuration;

/// <summary>
/// Carrega configurações de tema de arquivos JSON
/// </summary>
public class ConfigurationLoader
{
    private readonly string _themesDirectory;
    private readonly Dictionary<string, ThemeConfiguration> _cachedThemes = new();
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Construtor
    /// </summary>
    /// <param name="themesDirectory">Diretório onde os arquivos JSON de temas estão localizados</param>
    public ConfigurationLoader(string themesDirectory)
    {
        if (string.IsNullOrWhiteSpace(themesDirectory))
        {
            throw new ArgumentException("Themes directory cannot be empty", nameof(themesDirectory));
        }

        _themesDirectory = themesDirectory;

        if (!Directory.Exists(_themesDirectory))
        {
            throw new DirectoryNotFoundException($"Themes directory not found: {_themesDirectory}");
        }
    }

    /// <summary>
    /// Carrega tema do arquivo JSON
    /// </summary>
    /// <param name="themeName">Nome do tema (sem extensão .json)</param>
    /// <returns>Configuração do tema</returns>
    public ThemeConfiguration LoadTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            throw new ArgumentException("Theme name cannot be empty", nameof(themeName));
        }

        // Verificar cache
        if (_cachedThemes.TryGetValue(themeName, out var cachedTheme))
        {
            return cachedTheme;
        }

        // Montar caminho do arquivo
        var fileName = themeName.EndsWith(".json") ? themeName : $"{themeName}.json";
        var filePath = Path.Combine(_themesDirectory, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Theme file not found: {filePath}. Available themes: {string.Join(", ", GetAvailableThemes())}");
        }

        try
        {
            // Ler e parsear JSON
            var json = File.ReadAllText(filePath);
            var theme = JsonSerializer.Deserialize<ThemeConfiguration>(json, _jsonOptions);

            if (theme == null)
            {
                throw new InvalidConfigurationException($"Failed to deserialize theme from {filePath}");
            }

            // Validar configuração
            ConfigurationValidator.Validate(theme);

            // Cachear tema
            _cachedThemes[themeName] = theme;

            return theme;
        }
        catch (JsonException ex)
        {
            throw new InvalidConfigurationException($"Invalid JSON in theme file '{filePath}': {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is not InvalidConfigurationException)
        {
            throw new InvalidConfigurationException($"Error loading theme '{themeName}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Lista todos os temas disponíveis no diretório
    /// </summary>
    /// <returns>Lista de nomes de temas</returns>
    public List<string> GetAvailableThemes()
    {
        try
        {
            return Directory.GetFiles(_themesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name!)
                .OrderBy(name => name)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidConfigurationException($"Error reading themes directory: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Limpa o cache de temas
    /// </summary>
    public void ClearCache()
    {
        _cachedThemes.Clear();
    }

    /// <summary>
    /// Salva tema em arquivo JSON
    /// </summary>
    /// <param name="theme">Tema a salvar</param>
    /// <param name="fileName">Nome do arquivo (com ou sem .json)</param>
    public void SaveTheme(ThemeConfiguration theme, string fileName)
    {
        if (theme == null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        }

        // Validar antes de salvar
        ConfigurationValidator.Validate(theme);

        // Montar caminho
        var fullFileName = fileName.EndsWith(".json") ? fileName : $"{fileName}.json";
        var filePath = Path.Combine(_themesDirectory, fullFileName);

        try
        {
            var json = JsonSerializer.Serialize(theme, _jsonOptions);
            File.WriteAllText(filePath, json);

            // Atualizar cache
            var themeName = Path.GetFileNameWithoutExtension(fullFileName);
            _cachedThemes[themeName] = theme;
        }
        catch (Exception ex)
        {
            throw new InvalidConfigurationException($"Error saving theme to '{filePath}': {ex.Message}", ex);
        }
    }
}
