using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WoWGame.Api.Data;
using WoWGame.Api.Data.Entities;
using WoWGame.Api.Models;

namespace WoWGame.Api.Services;

public class LevelService : ILevelService
{
    private readonly WoWGameDbContext _context;
    private readonly ICrosswordGenerator _generator;
    private readonly IHostEnvironment _env;
    private readonly ILogger<LevelService> _logger;
    private List<string> _dictionary = new();

    public LevelService(
        WoWGameDbContext context,
        ICrosswordGenerator generator,
        IHostEnvironment env,
        ILogger<LevelService> logger)
    {
        _context = context;
        _generator = generator;
        _env = env;
        _logger = logger;

        LoadDictionary();
    }

    private void LoadDictionary()
    {
        try
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Resources", "dictionary.txt");
            if (File.Exists(filePath))
            {
                _dictionary = File.ReadAllLines(filePath)
                    .Select(w => w.Trim().ToLowerInvariant())
                    .Where(w => w.Length >= 3)
                    .Distinct()
                    .ToList();
                _logger.LogInformation("Loaded {Count} words from dictionary.txt", _dictionary.Count);
            }
            else
            {
                _logger.LogWarning("dictionary.txt not found at {Path}. Using internal fallback list.", filePath);
                LoadFallbackDictionary();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dictionary.txt. Using internal fallback list.");
            LoadFallbackDictionary();
        }
    }

    private void LoadFallbackDictionary()
    {
        _dictionary = new List<string>
        {
            // Fallback list of words to ensure game works even without resource file
            "silent", "listen", "tinsel", "inlets", "inlet", "line", "sent", "tile", "nest", "lent", "net", "ten", "its", "let", "sit", "lie", "sin", "nil",
            "garden", "danger", "ranged", "anger", "grand", "grade", "read", "dear", "rage", "dare", "drag", "run", "end", "red", "den", "ear", "era", "art", "age",
            "travel", "alert", "alter", "later", "rate", "tear", "late", "tale", "rave", "real", "era", "art", "let", "tar", "tea", "eat", "ale",
            "nature", "tuner", "turn", "rent", "neat", "true", "rue", "run", "net", "ten", "art", "tar", "nut", "eat", "tea", "ant",
            "forest", "store", "forte", "soft", "rose", "fort", "sore", "toes", "rot", "set", "for", "ore", "toe", "est",
            "castle", "scale", "least", "slate", "cleat", "case", "cast", "seal", "seat", "sale", "salt", "tale", "late", "cat", "let", "sat", "sea", "act, eat",
            "wonder", "drown", "drone", "under", "down", "word", "red", "row", "own", "end", "one", "rod", "den", "now",
            "planet", "plate", "panel", "plant", "plane", "neat", "pale", "lane", "plan", "tape", "late", "tale", "pant", "lean", "pen", "pin", "net", "ten",
            "double", "lobe", "bold", "blue", "loud", "bled", "led", "due", "bud", "bed", "old", "lob", "red",
            "friend", "fiend", "finer", "diner", "find", "fire", "fern", "ride", "rend", "fine", "end", "red", "den", "pin", "inn", "fir", "rid"
        };
    }

    public async Task<LevelDto> GetLevelAsync(int levelNumber)
    {
        // 1. Look for the level in the database
        var dbLevel = await _context.Levels.FirstOrDefaultAsync(l => l.LevelNumber == levelNumber);
        if (dbLevel != null)
        {
            _logger.LogInformation("Loaded Level {LevelNumber} from database.", levelNumber);
            return new LevelDto
            {
                LevelNumber = dbLevel.LevelNumber,
                Letters = dbLevel.Letters,
                Words = JsonSerializer.Deserialize<List<string>>(dbLevel.WordsJson) ?? new(),
                Grid = JsonSerializer.Deserialize<List<GridWordPlacementDto>>(dbLevel.GridJson) ?? new(),
                Width = dbLevel.Width,
                Height = dbLevel.Height
            };
        }

        // 2. Generate and save a new level if not found
        _logger.LogInformation("Level {LevelNumber} not found in database. Generating dynamically...", levelNumber);
        return await GenerateNewLevelAsync(levelNumber);
    }

    public async Task<LevelDto> GenerateNewLevelAsync(int levelNumber)
    {
        // Define base word length based on level (starts at 5, increases up to 7)
        int targetLength = 5 + (levelNumber / 5);
        if (targetLength > 7) targetLength = 7;

        var rand = new Random();
        int attempts = 0;
        LevelDto? resultLevel = null;

        while (attempts < 50)
        {
            attempts++;
            
            // Pick a random word of target length from dictionary to serve as base
            var candidateBaseWords = _dictionary
                .Where(w => w.Length == targetLength)
                .ToList();

            if (candidateBaseWords.Count == 0)
            {
                // Fallback to any length if no words match
                candidateBaseWords = _dictionary.Where(w => w.Length >= 5).ToList();
            }

            var baseWord = candidateBaseWords[rand.Next(candidateBaseWords.Count)];
            
            // Sort letters to create circular board input (or scramble it)
            var lettersArray = baseWord.ToCharArray();
            // Shuffle
            for (int i = lettersArray.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                var temp = lettersArray[i];
                lettersArray[i] = lettersArray[j];
                lettersArray[j] = temp;
            }
            string levelLetters = new string(lettersArray).ToUpperInvariant();

            // Find all valid sub-words that can be made from this set of letters
            var validSubWords = FindValidSubWords(levelLetters.ToLowerInvariant());

            if (validSubWords.Count < 4) continue; // Require at least 4 valid words for a good puzzle

            // Generate crossword grid layout
            var layout = _generator.GenerateLayout(validSubWords, levelNumber, levelLetters);

            // Verify that we placed enough words (at least 3 words and at least 60% of available words)
            if (layout.Grid.Count >= 3 && layout.Grid.Count >= (validSubWords.Count * 0.4))
            {
                resultLevel = layout;
                break;
            }
        }

        // If generation failed after many attempts, use a hardcoded default level structure
        if (resultLevel == null)
        {
            _logger.LogWarning("Level generation failed after 50 attempts for level {LevelNumber}. Generating fallback.", levelNumber);
            resultLevel = GetHardcodedFallbackLevel(levelNumber);
        }

        // Save generated level to the database
        try
        {
            var levelEntity = new Level
            {
                LevelNumber = resultLevel.LevelNumber,
                Letters = resultLevel.Letters,
                WordsJson = JsonSerializer.Serialize(resultLevel.Words),
                GridJson = JsonSerializer.Serialize(resultLevel.Grid),
                Width = resultLevel.Width,
                Height = resultLevel.Height
            };

            _context.Levels.Add(levelEntity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved newly generated Level {LevelNumber} to database.", levelNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save generated level {LevelNumber} to database.", levelNumber);
        }

        return resultLevel;
    }

    private List<string> FindValidSubWords(string letters)
    {
        var letterCounts = GetLetterCounts(letters);
        var validWords = new List<string>();

        foreach (var word in _dictionary)
        {
            if (word.Length < 3 || word.Length > letters.Length) continue;

            var wordCounts = GetLetterCounts(word);
            bool isValid = true;
            foreach (var kvp in wordCounts)
            {
                if (!letterCounts.TryGetValue(kvp.Key, out int count) || count < kvp.Value)
                {
                    isValid = false;
                    break;
                }
            }

            if (isValid)
            {
                validWords.Add(word);
            }
        }

        return validWords;
    }

    private Dictionary<char, int> GetLetterCounts(string s)
    {
        var counts = new Dictionary<char, int>();
        foreach (var c in s)
        {
            if (counts.TryGetValue(c, out int value))
            {
                counts[c] = ++value;
            }
            else
            {
                counts[c] = 1;
            }
        }
        return counts;
    }

    private LevelDto GetHardcodedFallbackLevel(int levelNumber)
    {
        // Simple 3-word crossword for "CAT" letters: ACT, CAT, SAT
        return new LevelDto
        {
            LevelNumber = levelNumber,
            Letters = "TACS",
            Words = new List<string> { "CAT", "ACT", "SAT" },
            Grid = new List<GridWordPlacementDto>
            {
                new() { Word = "CAT", StartX = 0, StartY = 1, Direction = "H" },
                new() { Word = "ACT", StartX = 1, StartY = 0, Direction = "V" },
                new() { Word = "SAT", StartX = 0, StartY = 3, Direction = "H" }
            },
            Width = 3,
            Height = 4
        };
    }
}
