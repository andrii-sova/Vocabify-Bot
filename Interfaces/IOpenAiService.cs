namespace VocabifyBot.Interfaces;

public interface IOpenAiService
{
    Task<string> TranslateWordsAsync(string words);
    Task<string> DetectTopicAsync(string words);
}
