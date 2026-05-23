using System.Text.RegularExpressions;
using VocabifyBot.Models;

namespace VocabifyBot.UI;

public static class WordFormatter
{
    public static readonly string[] CefrLevels = ["A0", "A1", "A2", "B1", "B2", "C1", "C2"];

    private static readonly Regex CefrPrefix = new(@"^\[(A0|A1|A2|B1|B2|C1|C2)\]\s*", RegexOptions.Compiled);
    private static readonly Regex StripAnnotations = new(@"\[.*?\]|\(.*?\)", RegexOptions.Compiled | RegexOptions.Singleline);

    public static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return Regex.Replace(text, @"([_\*\[\]\(\)~`>#\+\-=\|\{\}\.\!])", @"\\$1");
    }

    public static string Truncate(string s, int maxLength)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= maxLength)
        {
            return s;
        }

        return s[..maxLength].TrimEnd(' ', ',', ';') + "…";
    }

    public static string FormatWordLine(Word w)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(w.EnglishLevel))
        {
            parts.Add($"*[{EscapeMarkdown(w.EnglishLevel)}]*");
        }

        if (!string.IsNullOrWhiteSpace(w.Topic))
        {
            parts.Add($"_{EscapeMarkdown(w.Topic)}_");
        }

        parts.Add(EscapeMarkdown(w.Translation));
        return string.Join(" ", parts);
    }

    public static string ShortUkr(string translation)
    {
        var text = translation ?? string.Empty;
        var dashIndex = text.IndexOf('—');
        if (dashIndex >= 0)
        {
            text = text[(dashIndex + 1)..];
        }

        text = StripAnnotations.Replace(text, string.Empty).Trim().TrimEnd(';', ',', ' ');
        return Truncate(text, 48);
    }

    public static string QuizQuestion(Word w, string direction) =>
        direction == "eu" ? w.OriginalWord : ShortUkr(w.Translation);

    public static string QuizAnswer(Word w, string direction) =>
        direction == "eu" ? ShortUkr(w.Translation) : w.OriginalWord;

    public static List<PendingWordEntry> BuildPendingWords(string inputText, string translationText)
    {
        var inputs = inputText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var translations = translationText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return inputs.Select((original, index) =>
        {
            var raw = index < translations.Length ? translations[index] : original;
            var match = CefrPrefix.Match(raw);

            return new PendingWordEntry
            {
                Original = original,
                Translation = match.Success ? raw[match.Length..] : raw,
                EnglishLevel = match.Success ? match.Groups[1].Value : null
            };
        }).ToList();
    }
}
