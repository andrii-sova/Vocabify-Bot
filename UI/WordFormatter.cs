using System.Text.RegularExpressions;
using KnowlBot.Models;

namespace KnowlBot.UI;

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

        // ParseMode.Markdown (V1) only treats _  *  `  [  \ as special
        return Regex.Replace(text, @"([_\*`\[\\])", @"\$1");
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

    /// <summary>
    /// Parses AI-generated lines that already contain the English phrase embedded:
    /// "[B2] put you off — відбити бажання [пут ю оф] (…)"
    /// Original  = "put you off"
    /// Translation = "put you off — відбити бажання [пут ю оф] (…)"
    /// EnglishLevel = "B2"
    /// </summary>
    public static List<PendingWordEntry> BuildPendingWordsFromGeneration(string generationText)
    {
        var lines = generationText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<PendingWordEntry>();

        foreach (var line in lines)
        {
            var match = CefrPrefix.Match(line);
            if (!match.Success) continue;

            var rest  = line[match.Length..];                       // everything after "[B2] "
            var dash  = rest.IndexOf('—');
            var original = dash >= 0 ? rest[..dash].Trim() : rest.Trim();

            result.Add(new PendingWordEntry
            {
                Original     = original,
                Translation  = rest,                                // full line minus the [LEVEL] prefix
                EnglishLevel = match.Groups[1].Value
            });
        }
        return result;
    }

    /// <summary>Formats a PendingWordEntry for preview display (before saving).</summary>
    public static string FormatPendingLine(PendingWordEntry p)
    {
        var level = p.EnglishLevel is not null ? $"*[{p.EnglishLevel}]* " : string.Empty;
        return $"{level}{EscapeMarkdown(p.Translation)}";
    }
}
