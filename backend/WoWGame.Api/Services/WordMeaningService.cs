using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WoWGame.Api.Data;
using WoWGame.Api.Data.Entities;

namespace WoWGame.Api.Services;

public class WordMeaningService : IWordMeaningService
{
    private readonly WoWGameDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<WordMeaningService> _logger;

    public WordMeaningService(
        WoWGameDbContext context, 
        HttpClient httpClient, 
        ILogger<WordMeaningService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetMeaningAsync(string word)
    {
        var cleanWord = word.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(cleanWord))
        {
            return "Invalid word.";
        }

        // 1. Check local cache (DB)
        var cached = await _context.WordMeanings.FirstOrDefaultAsync(wm => wm.Word == cleanWord);
        if (cached != null)
        {
            _logger.LogInformation("Returning cached meaning for word: {Word}", cleanWord);
            return cached.Meaning;
        }

        // 2. Fetch from Dictionary API
        string definition = "Meaning not found.";
        try
        {
            var apiUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{cleanWord}";
            _logger.LogInformation("Fetching meaning for word from API: {Word}", cleanWord);
            
            var response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                definition = ParseDefinition(content);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Word {Word} not found in Dictionary API.", cleanWord);
                definition = "No definition available in the dictionary.";
            }
            else
            {
                _logger.LogError("Dictionary API returned status code {StatusCode} for word {Word}", response.StatusCode, cleanWord);
                // Return default meaning but do not cache if it's a server error
                return "Could not retrieve meaning at this time.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Dictionary API for word {Word}", cleanWord);
            return "Could not retrieve meaning due to an error.";
        }

        // 3. Cache the meaning in MS SQL Server
        try
        {
            var wordMeaning = new WordMeaning
            {
                Word = cleanWord,
                Meaning = definition,
                CachedAt = DateTime.UtcNow
            };
            _context.WordMeanings.Add(wordMeaning);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache meaning in database for word {Word}", cleanWord);
        }

        return definition;
    }

    private string ParseDefinition(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstEntry = root[0];
                if (firstEntry.TryGetProperty("meanings", out var meanings) && meanings.ValueKind == JsonValueKind.Array && meanings.GetArrayLength() > 0)
                {
                    var definitionParts = new List<string>();

                    // Take up to 2 meanings (different parts of speech)
                    int limit = Math.Min(meanings.GetArrayLength(), 2);
                    for (int i = 0; i < limit; i++)
                    {
                        var meaning = meanings[i];
                        string partOfSpeech = meaning.TryGetProperty("partOfSpeech", out var posVal) ? posVal.GetString() ?? "" : "";
                        
                        if (meaning.TryGetProperty("definitions", out var definitions) && definitions.ValueKind == JsonValueKind.Array && definitions.GetArrayLength() > 0)
                        {
                            string defText = definitions[0].TryGetProperty("definition", out var defVal) ? defVal.GetString() ?? "" : "";
                            if (!string.IsNullOrEmpty(defText))
                            {
                                definitionParts.Add(!string.IsNullOrEmpty(partOfSpeech) 
                                    ? $"({partOfSpeech}) {defText}" 
                                    : defText);
                            }
                        }
                    }

                    if (definitionParts.Count > 0)
                    {
                        return string.Join("; ", definitionParts);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing dictionary JSON response");
        }

        return "Definition structure could not be parsed.";
    }
}
