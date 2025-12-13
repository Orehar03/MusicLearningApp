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
        _context.Database.EnsureCreated();

        if (!_context.Users.Any(u => u.Email == "admin@admin.com"))
        {
            // Генерируем хеш ТОЛЬКО ОДИН РАЗ через BCrypt
            string adminPassword = "admin";
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 11);

            var admin = new User
            {
                Email = "admin@admin.com",
                PasswordHash = passwordHash, // ✅ Гарантированно правильный хеш
                Name = "Администратор",
                Gender = "Other",
                BirthDate = new DateTime(1990, 1, 1),
                Role = "Admin"
            };
            _context.Users.Add(admin);
            _context.SaveChanges();
            Console.WriteLine($"✅ Админ создан с хешем: {passwordHash}");
        }
        else
        {
            Console.WriteLine("ℹ️ Админ уже существует");
        }
    }
}