using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MusicLearningApp.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public string? Authenticate(string email, string password)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return null;

            // Защита от пустого или некорректного хеша
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                Console.WriteLine($"⚠️ Пустой хеш пароля для пользователя: {email}");
                return null;
            }

            // Проверяем пароль
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                Console.WriteLine($"❌ Неверный пароль для: {email}");
                return null;
            }

            // Генерация JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"🔥 Критическая ошибка в AuthService.Authenticate: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return null; // Возвращаем null вместо падения
        }
    }
}