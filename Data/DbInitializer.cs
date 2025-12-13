using MusicLearningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MusicLearningApp.Data;

public class DbInitializer
{
    private readonly ApplicationDbContext _context;

    public DbInitializer(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        // Удаляем и создаём заново
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        // Создаём админа с правильным хешем
        var admin = new User
        {
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin", workFactor: 11), // ✅ Генерируем заново
            Name = "Администратор",
            Gender = "Other",
            BirthDate = new DateTime(1990, 1, 1),
            Role = "Admin"
        };
        _context.Users.Add(admin);
        _context.SaveChanges();
        Console.WriteLine($"✅ Админ создан с хешем: {admin.PasswordHash}");

        // Создаём тестовые данные
        SeedTestData();
    }

    private void SeedTestData()
    {
        if (!_context.Lessons.Any())
        {
            _context.Lessons.AddRange(
                new Models.Lesson { Title = "Основы нотной грамоты", Description = "Текст", VideoPath = "/videos/lesson1.mp4" },
                new Models.Lesson { Title = "Аккорды для гитары", Description = "Текст", VideoPath = "/videos/lesson2.mp4" }
            );
            _context.SaveChanges();
        }

        if (!_context.Homeworks.Any())
        {
            var nextMonday = DateTime.Now.AddDays(7 - (int)DateTime.Now.DayOfWeek);
            var deadline = new DateTime(nextMonday.Year, nextMonday.Month, nextMonday.Day, 23, 59, 59);

            _context.Homeworks.Add(new Models.Homework
            {
                Description = "Напишите названия нот в порядке возрастания",
                Deadline = deadline
            });
            _context.SaveChanges();
        }
    }
}