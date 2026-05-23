namespace VocabifyBot.Models;

public class Word
{
    public int      Id            { get; set; }
    public string   OriginalWord  { get; set; } = "";
    public string   Translation   { get; set; } = "";
    public string?  Topic         { get; set; }
    public string?  EnglishLevel  { get; set; }   // CEFR level: A0, A1, A2, B1, B2, C1, C2
    public Guid?    BatchId       { get; set; }   // words saved in the same send-session share a BatchId
    public long     AddedByUserId { get; set; }
    public long     ForStudentId  { get; set; }
    public DateTime CreatedAt     { get; set; }

    // Navigation
    public User AddedBy    { get; set; } = null!;
    public User ForStudent { get; set; } = null!;
}

