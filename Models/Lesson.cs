namespace MusicLearningApp.Models;

public class Lesson
{
    public int Id { get; set; }
    public string Title { get; set; } = "Новый урок";
    public string Description { get; set; } = "Текст описания урока";
    public string VideoPath { get; set; } = "/videos/sample.mp4"; // Путь к видео в wwwroot
}