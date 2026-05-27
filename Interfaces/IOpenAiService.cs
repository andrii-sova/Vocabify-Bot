namespace KnowlBot.Interfaces;

public interface IOpenAiService
{
    Task<string> TranslateWordsAsync(string words);
    Task<string> DetectTopicAsync(string words);
    Task<string> GenerateWordsByLevelAsync(string level, int count, string? topic, IEnumerable<string> existingWords);
}
