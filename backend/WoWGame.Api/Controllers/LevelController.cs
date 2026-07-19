using Microsoft.AspNetCore.Mvc;
using WoWGame.Api.Services;

namespace WoWGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LevelController : ControllerBase
{
    private readonly ILevelService _levelService;

    public LevelController(ILevelService levelService)
    {
        _levelService = levelService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentLevel([FromQuery] int number = 1)
    {
        if (number <= 0) number = 1;
        var level = await _levelService.GetLevelAsync(number);
        return Ok(level);
    }

    [HttpGet("{number}")]
    public async Task<IActionResult> GetLevelByNumber(int number)
    {
        if (number <= 0) return BadRequest("Invalid level number.");
        var level = await _levelService.GetLevelAsync(number);
        return Ok(level);
    }

    [HttpPost("generate/{number}")]
    public async Task<IActionResult> ForceGenerateLevel(int number)
    {
        if (number <= 0) return BadRequest("Invalid level number.");
        var level = await _levelService.GenerateNewLevelAsync(number);
        return Ok(level);
    }
}
