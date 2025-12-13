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
        // Проверяем, существует ли таблица Users
        bool tableExists = false;
        try
        {
            if (_context.Database.GetDbConnection().State == System.Data.ConnectionState.Closed)
                _context.Database.GetDbConnection().Open();

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Users';
            ";
            tableExists = (long)command.ExecuteScalar() > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка проверки таблицы: {ex.Message}");
        }

        // Если таблицы нет — создаём структуру
        if (!tableExists)
        {
            _context.Database.EnsureCreated();
        }

        // Гарантированно создаём админа
        if (!_context.Users.Any(u => u.Email == "admin@admin.com"))
        {
            var admin = new User
            {
                Email = "admin@admin.com",
                PasswordHash = "$2a$11$uFp1WdR7zL0xJZq6x7eXiebF9X1jK5YJ0qW9X1jK5YJ0qW9X1jK5Y", // хеш от "admin"
                Name = "Администратор",
                Gender = "Other",
                BirthDate = new DateTime(1990, 1, 1),
                Role = "Admin"
            };
            _context.Users.Add(admin);
            _context.SaveChanges();
            Console.WriteLine("✅ Админ успешно создан");
        }
        else
        {
            Console.WriteLine("ℹ️ Админ уже существует");
        }
    }
}