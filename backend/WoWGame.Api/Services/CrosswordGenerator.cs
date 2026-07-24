using WoWGame.Api.Models;

namespace WoWGame.Api.Services;

public class CrosswordGenerator : ICrosswordGenerator
{
    private class PlacedWord
    {
        public string Word { get; set; } = string.Empty;
        public int StartX { get; set; }
        public int StartY { get; set; }
        public string Direction { get; set; } = "H"; // "H" or "V"
    }

    public LevelDto GenerateLayout(List<string> words, int levelNumber, string letters)
    {
        // 1. Sort words by length descending to place longest first
        var sortedWords = words
            .OrderByDescending(w => w.Length)
            .Distinct()
            .Take(8) // Limit to 8 words for a clean level layout
            .ToList();

        if (sortedWords.Count == 0)
        {
            return new LevelDto
            {
                LevelNumber = levelNumber,
                Letters = letters,
                Words = new List<string>(),
                Grid = new List<GridWordPlacementDto>(),
                Width = 0,
                Height = 0
            };
        }

        const int gridSize = 60;
        var grid = new char[gridSize, gridSize];
        for (int r = 0; r < gridSize; r++)
            for (int c = 0; c < gridSize; c++)
                grid[r, c] = '\0';

        var placedWords = new List<PlacedWord>();

        // Place the first (longest) word at the center horizontally
        var firstWord = sortedWords[0];
        int startX0 = gridSize / 2 - firstWord.Length / 2;
        int startY0 = gridSize / 2;

        var firstPlacement = new PlacedWord
        {
            Word = firstWord,
            StartX = startX0,
            StartY = startY0,
            Direction = "H"
        };
        placedWords.Add(firstPlacement);
        for (int i = 0; i < firstWord.Length; i++)
        {
            grid[startY0, startX0 + i] = firstWord[i];
        }

        // Try to place the remaining words
        for (int wIdx = 1; wIdx < sortedWords.Count; wIdx++)
        {
            var word = sortedWords[wIdx];
            PlacedWord? bestPlacement = null;
            int bestScore = int.MinValue;

            // Find all possible intersections with already-placed words
            for (int pIdx = 0; pIdx < placedWords.Count; pIdx++)
            {
                var placed = placedWords[pIdx];

                for (int i = 0; i < placed.Word.Length; i++)
                {
                    for (int j = 0; j < word.Length; j++)
                    {
                        if (placed.Word[i] == word[j])
                        {
                            // We have a character match, try placing the word intersecting here
                            int newStartX = 0;
                            int newStartY = 0;
                            string newDirection = "";

                            if (placed.Direction == "H")
                            {
                                // Placed word is Horizontal, so new word must be Vertical
                                newStartX = placed.StartX + i;
                                newStartY = placed.StartY - j;
                                newDirection = "V";
                            }
                            else
                            {
                                // Placed word is Vertical, so new word must be Horizontal
                                newStartX = placed.StartX - j;
                                newStartY = placed.StartY + i;
                                newDirection = "H";
                            }

                            if (IsValidPlacement(grid, gridSize, word, newStartX, newStartY, newDirection, placed, i, j))
                            {
                                int score = CalculateScore(placedWords, word, newStartX, newStartY, newDirection);
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    bestPlacement = new PlacedWord
                                    {
                                        Word = word,
                                        StartX = newStartX,
                                        StartY = newStartY,
                                        Direction = newDirection
                                    };
                                }
                            }
                        }
                    }
                }
            }

            if (bestPlacement != null)
            {
                placedWords.Add(bestPlacement);
                // Apply to grid
                for (int k = 0; k < word.Length; k++)
                {
                    int x = bestPlacement.Direction == "H" ? bestPlacement.StartX + k : bestPlacement.StartX;
                    int y = bestPlacement.Direction == "H" ? bestPlacement.StartY : bestPlacement.StartY + k;
                    grid[y, x] = word[k];
                }
            }
        }

        // Bounding box cropping
        if (placedWords.Count == 0)
        {
            return new LevelDto
            {
                LevelNumber = levelNumber,
                Letters = letters,
                Words = new List<string>(),
                Grid = new List<GridWordPlacementDto>(),
                Width = 0,
                Height = 0
            };
        }

        int minX = placedWords.Min(w => w.StartX);
        int maxX = placedWords.Max(w => w.Direction == "H" ? w.StartX + w.Word.Length - 1 : w.StartX);
        int minY = placedWords.Min(w => w.StartY);
        int maxY = placedWords.Max(w => w.Direction == "V" ? w.StartY + w.Word.Length - 1 : w.StartY);

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        var gridPlacements = placedWords.Select(pw => new GridWordPlacementDto
        {
            Word = pw.Word,
            StartX = pw.StartX - minX,
            StartY = pw.StartY - minY,
            Direction = pw.Direction
        }).ToList();

        return new LevelDto
        {
            LevelNumber = levelNumber,
            Letters = letters,
            Words = placedWords.Select(pw => pw.Word).ToList(),
            Grid = gridPlacements,
            Width = width,
            Height = height
        };
    }

    private bool IsValidPlacement(
        char[,] grid, 
        int gridSize, 
        string word, 
        int startX, 
        int startY, 
        string direction, 
        PlacedWord intersectingWord,
        int intersectingPlacedCharIndex,
        int intersectingWordCharIndex)
    {
        // 1. Grid boundary check
        if (startX < 0 || startY < 0) return false;
        if (direction == "H" && startX + word.Length > gridSize) return false;
        if (direction == "V" && startY + word.Length > gridSize) return false;

        // Bounding cells before and after the word must be empty
        if (direction == "H")
        {
            if (startX - 1 >= 0 && grid[startY, startX - 1] != '\0') return false;
            if (startX + word.Length < gridSize && grid[startY, startX + word.Length] != '\0') return false;
        }
        else
        {
            if (startY - 1 >= 0 && grid[startY - 1, startX] != '\0') return false;
            if (startY + word.Length < gridSize && grid[startY + word.Length, startX] != '\0') return false;
        }

        int intersectionX = direction == "H" ? startX + intersectingWordCharIndex : startX;
        int intersectionY = direction == "H" ? startY : startY + intersectingWordCharIndex;

        // Check each character position
        for (int k = 0; k < word.Length; k++)
        {
            int x = direction == "H" ? startX + k : startX;
            int y = direction == "H" ? startY : startY + k;

            // Is this cell the intersection point?
            bool isIntersection = (x == intersectionX && y == intersectionY);

            char currentCellChar = grid[y, x];

            if (currentCellChar != '\0')
            {
                // Cell is occupied, character must match exactly
                if (currentCellChar != word[k]) return false;
            }
            else
            {
                // Cell is empty. It should not be adjacent to any other letters in the perpendicular direction
                if (direction == "H")
                {
                    // Check top and bottom cells
                    if (y - 1 >= 0 && grid[y - 1, x] != '\0') return false;
                    if (y + 1 < gridSize && grid[y + 1, x] != '\0') return false;
                }
                else
                {
                    // Check left and right cells
                    if (x - 1 >= 0 && grid[y, x - 1] != '\0') return false;
                    if (x + 1 < gridSize && grid[y, x + 1] != '\0') return false;
                }
            }
        }

        return true;
    }

    private int CalculateScore(List<PlacedWord> placedWords, string word, int startX, int startY, string direction)
    {
        // Intersections add points
        int intersections = 0;

        // Calculate the bounding box of existing placed words
        int currentMinX = placedWords.Min(w => w.StartX);
        int currentMaxX = placedWords.Max(w => w.Direction == "H" ? w.StartX + w.Word.Length - 1 : w.StartX);
        int currentMinY = placedWords.Min(w => w.StartY);
        int currentMaxY = placedWords.Max(w => w.Direction == "V" ? w.StartY + w.Word.Length - 1 : w.StartY);
        int currentArea = (currentMaxX - currentMinX + 1) * (currentMaxY - currentMinY + 1);

        // Bounding box of new layout
        int newMinX = Math.Min(currentMinX, startX);
        int newMaxX = Math.Max(currentMaxX, direction == "H" ? startX + word.Length - 1 : startX);
        int newMinY = Math.Min(currentMinY, startY);
        int newMaxY = Math.Max(currentMaxY, direction == "V" ? startY + word.Length - 1 : startY);
        int newArea = (newMaxX - newMinX + 1) * (newMaxY - newMinY + 1);

        // Count intersections
        foreach (var placed in placedWords)
        {
            for (int i = 0; i < placed.Word.Length; i++)
            {
                int px = placed.Direction == "H" ? placed.StartX + i : placed.StartX;
                int py = placed.Direction == "H" ? placed.StartY : placed.StartY + i;

                for (int j = 0; j < word.Length; j++)
                {
                    int wx = direction == "H" ? startX + j : startX;
                    int wy = direction == "H" ? startY : startY + j;

                    if (px == wx && py == wy && placed.Word[i] == word[j])
                    {
                        intersections++;
                    }
                }
            }
        }

        // Formula: Score = (intersections * 20) - (area expansion) - (aspect ratio imbalance penalty)
        int areaExpansion = newArea - currentArea;
        int newWidth = newMaxX - newMinX + 1;
        int newHeight = newMaxY - newMinY + 1;
        int aspectImbalance = Math.Abs(newWidth - newHeight);
        int ratioPenalty = aspectImbalance * 6; // Penalize non-square layouts
        
        return (intersections * 20) - areaExpansion - ratioPenalty;
    }
}
