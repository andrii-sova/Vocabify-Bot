namespace VocabifyBot.Models;

public class WordStat
{
    public int      Id           { get; set; }
    public int      WordId       { get; set; }
    public long     StudentId    { get; set; }
    public int      CorrectCount { get; set; }
    public int      WrongCount   { get; set; }
    public DateTime LastSeenAt   { get; set; }

    // Navigation
    public Word Word    { get; set; } = null!;
    public User Student { get; set; } = null!;
}
