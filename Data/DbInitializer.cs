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
        _context.Database.Migrate();

        if (!_context.Users.Any(u => u.Email == "admin@admin.com"))
        {
            var admin = new User
            {
                Email = "admin@admin.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), // ПОЛНЫЙ ПУТЬ!
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