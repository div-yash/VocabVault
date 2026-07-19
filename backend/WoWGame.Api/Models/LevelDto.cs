namespace WoWGame.Api.Models;

public class LevelDto
{
    public int LevelNumber { get; set; }
    public string Letters { get; set; } = string.Empty;
    public List<string> Words { get; set; } = new();
    public List<GridWordPlacementDto> Grid { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
}

public class GridWordPlacementDto
{
    public string Word { get; set; } = string.Empty;
    public int StartX { get; set; }
    public int StartY { get; set; }
    public string Direction { get; set; } = "H"; // "H" for Horizontal, "V" for Vertical
}

public class WordMeaningDto
{
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
}

public class PlayerDto
{
    public string Username { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int Score { get; set; }
}
