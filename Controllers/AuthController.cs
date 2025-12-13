using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using MusicLearningApp.Services;

namespace MusicLearningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ApplicationDbContext _context;

    public AuthController(AuthService authService, ApplicationDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterModel model)
    {
        if (_context.Users.Any(u => u.Email == model.Email))
            return BadRequest("Email уже используется");

        // 🔹 Попытка преобразовать строку даты в DateTime
        if (!DateTime.TryParse(model.BirthDate, out var birthDate))
            return BadRequest("Неверный формат даты. Используйте ГГГГ-ММ-ДД.");

        var user = new User
        {
            Email = model.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 11),
            Name = model.Name,
            Gender = model.Gender,
            BirthDate = birthDate, // ← теперь это DateTime, а не строка
            Role = "User"
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return Ok("Регистрация успешна");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        var token = _authService.Authenticate(model.Email, model.Password);
        if (token == null)
            return Unauthorized("Неверные данные");

        return Ok(new { Token = token });
    }
}

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = "Other";
    public string BirthDate { get; set; } = string.Empty; // строка в формате "ГГГГ-ММ-ДД"
}

public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}