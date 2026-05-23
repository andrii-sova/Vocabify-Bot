using VocabifyBot.Interfaces;
using OpenAI;
using OpenAI.Chat;

namespace VocabifyBot.Services;

public class OpenAiService : IOpenAiService
{
    private readonly ChatClient _chat;

    private const string TranslationPrompt = @"You are an English-to-Ukrainian dictionary assistant for language learners.
Translate English words or phrases into Ukrainian following EXACTLY this format for each entry (one entry per line):

[LEVEL] phrase — Ukrainian translation, synonyms; також: additional meaning [Ukrainian Cyrillic pronunciation] (Example sentence — Ukrainian translation)

Rules:
- Each entry must be on a SINGLE line — no line breaks inside an entry
- Start every line with the CEFR level in square brackets: [A0], [A1], [A2], [B1], [B2], [C1], or [C2]
- Pronunciation MUST be in square brackets [] using ONLY Ukrainian Cyrillic letters (e.g. [пут ю оф], [гет ап], [брейк даун])
- Include 1-3 short example sentences with translations in the same parentheses
- List all common Ukrainian meanings separated by commas; use 'також:' for secondary meanings
- Output ONLY the translated entries, nothing else

Example output:
[B2] put you off — відбити бажання, знеохотити, відвернути (від чогось); також: відкласти [пут ю оф] (The smell put me off my food — Запах відбив мені апетит; Don't let his comments put you off — Не дозволяй його коментарям знеохотити тебе)";

    public OpenAiService(string apiKey)
    {
        var client = new OpenAIClient(apiKey);
        _chat = client.GetChatClient("gpt-4o-mini");
    }

    public async Task<string> TranslateWordsAsync(string words)
    {
        var result = await _chat.CompleteChatAsync(
            new SystemChatMessage(TranslationPrompt),
            new UserChatMessage($"Translate these words/phrases (one per line):\n{words.Trim()}")
        );
        return result.Value.Content[0].Text.Trim();
    }

    public async Task<string> DetectTopicAsync(string words)
    {
        var result = await _chat.CompleteChatAsync(
            new SystemChatMessage(
                "Identify the most fitting topic/category for the given English words or phrases. " +
                "Reply with 2-4 words only (e.g. 'Phrasal Verbs', 'Business English', 'B2 Vocabulary', " +
                "'Adjectives', 'Travel & Transport'). No explanation, just the topic name."),
            new UserChatMessage(words.Trim())
        );
        return result.Value.Content[0].Text.Trim();
    }
}
