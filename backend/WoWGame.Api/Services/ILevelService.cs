using WoWGame.Api.Models;

namespace WoWGame.Api.Services;

public interface ILevelService
{
    Task<LevelDto> GetLevelAsync(int levelNumber);
    Task<LevelDto> GenerateNewLevelAsync(int levelNumber);
}
