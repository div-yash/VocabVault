using System.ComponentModel.DataAnnotations;

namespace WoWGame.Api.Data.Entities;

public class Player
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = "Player";

    public int CurrentLevel { get; set; } = 1;

    public int Score { get; set; } = 0;
}
