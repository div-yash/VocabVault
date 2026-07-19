using System.ComponentModel.DataAnnotations;

namespace WoWGame.Api.Data.Entities;

public class Level
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int LevelNumber { get; set; }

    [Required]
    [MaxLength(10)]
    public string Letters { get; set; } = string.Empty;

    [Required]
    public string WordsJson { get; set; } = "[]";

    [Required]
    public string GridJson { get; set; } = "[]";

    public int Width { get; set; }
    public int Height { get; set; }
}
