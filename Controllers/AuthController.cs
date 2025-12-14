using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using MusicLearningApp.Services;
using System.Net;
using System.Security.Claims;

namespace MusicLearningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email и пароль обязательны" });

            if (_context.Users.Any(u => u.Email == model.Email))
                return BadRequest(new { error = "Email уже используется" });

            if (!DateTime.TryParse(model.BirthDate, out var birthDate))
                return BadRequest(new { error = "Неверный формат даты. Используйте ГГГГ-ММ-ДД." });

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password, workFactor: 11),
                Name = model.Name,
                Gender = model.Gender,
                BirthDate = birthDate,
                Role = "User"
            };

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new { message = "Регистрация успешна" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка регистрации: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email и пароль обязательны" });

            var token = _authService.Authenticate(model.Email, model.Password);
            if (token == null)
                return Unauthorized(new { error = "Неверные данные" });

            // Добавляем логирование для отладки
            _logger.LogInformation($"Пользователь {model.Email} успешно авторизовался");

            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка входа: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Добавляем логирование для отладки
            _logger.LogInformation($"Запрос данных текущего пользователя: ID={userId}, Role={role}");

            return Ok(new { userId, role });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка получения данных пользователя: {ex.Message}");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
        }
    }
}

public class RegisterModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Gender { get; set; } = "Other";
    public string BirthDate { get; set; } = string.Empty;
}

public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}