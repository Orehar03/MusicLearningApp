using Microsoft.EntityFrameworkCore;
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

    public AuthService(ApplicationDbContext context, JwtSettings jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings;
    }

    public string? Authenticate(string email, string password)
    {
        try
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                Console.WriteLine($"Пользователь не найден: {email}");
                return null;
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                Console.WriteLine($"Пустой хеш для: {email}");
                return null;
            }

            // 🔥 Явно указываем работу с BCrypt
            bool passwordVerified;
            try
            {
                passwordVerified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка верификации пароля: {ex.Message}");
                return null;
            }

            if (!passwordVerified)
            {
                Console.WriteLine($"Неверный пароль для: {email}");
                return null;
            }

            // Генерация токена
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
            var tokenString = tokenHandler.WriteToken(token);

            Console.WriteLine($"Токен создан для: {email}");
            return tokenString;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Критическая ошибка в Authenticate: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return null;
        }
    }
}