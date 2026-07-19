using System.ComponentModel.DataAnnotations;

namespace WoWGame.Api.Data.Entities;

public class WordMeaning
{
    [Key]
    [MaxLength(100)]
    public string Word { get; set; } = string.Empty;

    [Required]
    public string Meaning { get; set; } = string.Empty;

    public DateTime CachedAt { get; set; } = DateTime.UtcNow;
}
