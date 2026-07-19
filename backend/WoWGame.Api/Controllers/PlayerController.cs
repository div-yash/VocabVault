using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WoWGame.Api.Data;
using WoWGame.Api.Data.Entities;
using WoWGame.Api.Models;

namespace WoWGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private readonly WoWGameDbContext _context;

    public PlayerController(WoWGameDbContext context)
    {
        _context = context;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetPlayer(string username)
    {
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Username.ToLower() == username.ToLower());
        if (player == null)
        {
            // Create default player
            player = new Player { Username = username, CurrentLevel = 1, Score = 0 };
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }
        return Ok(new PlayerDto
        {
            Username = player.Username,
            CurrentLevel = player.CurrentLevel,
            Score = player.Score
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdatePlayerProgress([FromBody] PlayerDto dto)
    {
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Username.ToLower() == dto.Username.ToLower());
        if (player == null)
        {
            player = new Player
            {
                Username = dto.Username,
                CurrentLevel = dto.CurrentLevel,
                Score = dto.Score
            };
            _context.Players.Add(player);
        }
        else
        {
            player.CurrentLevel = dto.CurrentLevel;
            player.Score = dto.Score;
        }

        await _context.SaveChangesAsync();
        return Ok(dto);
    }
}
