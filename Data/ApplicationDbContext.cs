using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Models;

namespace MusicLearningApp.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Lesson> Lessons { get; set; } = null!;
    public DbSet<Homework> Homeworks { get; set; } = null!;
    public DbSet<Submission> Submissions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Инициализация данных (примеры уроков и домашек)
        modelBuilder.Entity<Lesson>().HasData(
            new Lesson { Id = 1, Title = "Основы нотной грамоты", Description = "Текст", VideoPath = "/videos/lesson1.mp4" },
            new Lesson { Id = 2, Title = "Аккорды для гитары", Description = "Текст", VideoPath = "/videos/lesson2.mp4" }
        );

        modelBuilder.Entity<Homework>().HasData(
            new Homework
            {
                Id = 1,
                Description = "Напишите названия нот в порядке возрастания",
                Deadline = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(7 - (int)DateTime.Now.DayOfWeek).AddHours(23).AddMinutes(59)
            }
        );
    }
}