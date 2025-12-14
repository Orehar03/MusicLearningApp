using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using System.Security.Claims;

namespace MusicLearningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Внедряем DbContext через конструктор
    public ConsultationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Text))
            return BadRequest(new { error = "Сообщение не может быть пустым" });

        // Получаем ID текущего пользователя
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized(new { error = "Не удалось определить пользователя" });

        var userId = int.Parse(userIdClaim.Value);

        // Ищем пользователя в БД, чтобы получить его имя
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return NotFound(new { error = "Пользователь не найден" });

        // Создаем и сохраняем сообщение в базу данных
        var consultationMessage = new ConsultationMessage
        {
            UserId = userId,
            UserName = user.Name, // Сохраняем имя
            Text = model.Text,
            Timestamp = DateTime.UtcNow
        };

        _context.ConsultationMessages.Add(consultationMessage);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Сообщение отправлено администратору" });
    }
}

public class MessageModel
{
    public string Text { get; set; } = string.Empty;
}