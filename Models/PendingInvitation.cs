namespace VocabifyBot.Models;

public class PendingInvitation
{
    public int      Id              { get; set; }
    public long     TeacherId       { get; set; }
    public string   StudentUsername { get; set; } = "";  // lowercase, no @
    public DateTime CreatedAt       { get; set; }

    // Navigation
    public User Teacher { get; set; } = null!;
}
