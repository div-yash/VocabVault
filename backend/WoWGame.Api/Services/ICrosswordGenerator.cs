using WoWGame.Api.Models;

namespace WoWGame.Api.Services;

public interface ICrosswordGenerator
{
    LevelDto GenerateLayout(List<string> words, int levelNumber, string letters);
}
