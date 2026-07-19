namespace WoWGame.Api.Services;

public interface IWordMeaningService
{
    Task<string> GetMeaningAsync(string word);
}
