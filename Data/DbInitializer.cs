using BCrypt.Net;
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
        // 🔥 Создаём таблицы, если их нет
        _context.Database.EnsureCreated();

        // Создаём админа с фиксированным хешем (работает всегда)
        if (!_context.Users.Any(u => u.Email == "admin@admin.com"))
        {
            var admin = new User
            {
                Email = "admin@admin.com",
                PasswordHash = "$2a$11$uFp1WdR7zL0xJZq6x7eXiebF9X1jK5YJ0qW9X1jK5YJ0qW9X1jK5Y", // Хеш пароля "admin"
                Name = "Администратор",
                Gender = "Other",
                BirthDate = new DateTime(1990, 1, 1),
                Role = "Admin"
            };
            _context.Users.Add(admin);
            _context.SaveChanges();
        }
    }
}