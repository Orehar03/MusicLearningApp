using System.ComponentModel.DataAnnotations;

namespace MusicLearningApp.Models;

public class Submission
{
    public int Id { get; set; }
    public int HomeworkId { get; set; }
    public int UserId { get; set; }

    [Required]
    [StringLength(2000)]
    public string? TextAnswer { get; set; }

    // Свойство для хранения пути к файлу
    public string? FilePath { get; set; }

    public DateTime SubmissionTime { get; set; }

    // Навигационные свойства для связи с другими таблицами
    public User User { get; set; } = null!;
    public Homework Homework { get; set; } = null!; // <-- ЭТО СВОЙСТВО БЫЛО ДОБАВЛЕНО
}