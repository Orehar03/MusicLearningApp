namespace MusicLearningApp.Models;

public class Homework
{
    public int Id { get; set; }
    public string Description { get; set; } = "Описание домашнего задания";
    public DateTime Deadline { get; set; } = DateTime.Now.AddDays(7); // Дедлайн по умолчанию - через неделю
}