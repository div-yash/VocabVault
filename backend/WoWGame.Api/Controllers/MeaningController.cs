using Microsoft.AspNetCore.Mvc;
using WoWGame.Api.Services;

namespace WoWGame.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeaningController : ControllerBase
{
    private readonly IWordMeaningService _meaningService;

    public MeaningController(IWordMeaningService meaningService)
    {
        _meaningService = meaningService;
    }

    [HttpGet("{word}")]
    public async Task<IActionResult> GetMeaning(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return BadRequest("Word is empty.");
        var meaning = await _meaningService.GetMeaningAsync(word);
        return Ok(new { word = word, meaning = meaning });
    }
}
