namespace VocabifyBot.Models;

public class TeacherStudent
{
    public long TeacherId { get; set; }
    public long StudentId { get; set; }

    // Navigation
    public User Teacher { get; set; } = null!;
    public User Student { get; set; } = null!;
}
