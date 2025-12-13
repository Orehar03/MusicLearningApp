namespace MusicLearningApp.Models;

public class Submission
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public int UserId { get; set; }
    public string? TextAnswer { get; set; }
    public string? FilePath { get; set; } // Путь к загруженному файлу
    public DateTime SubmissionTime { get; set; } = DateTime.Now;
}