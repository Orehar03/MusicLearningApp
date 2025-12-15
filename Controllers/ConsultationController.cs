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
    private readonly ILogger<ConsultationController> _logger; // Добавляем логгер

    public ConsultationController(ApplicationDbContext context, ILogger<ConsultationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] MessageModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Text))
            return BadRequest(new { error = "Сообщение не может быть пустым" });

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized(new { error = "Не удалось определить пользователя" });

            var userId = int.Parse(userIdClaim.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { error = "Пользователь не найден" });

            var consultationMessage = new ConsultationMessage
            {
                UserId = userId,
                UserName = user.Name,
                Text = model.Text,
                Timestamp = DateTime.UtcNow
            };

            _context.ConsultationMessages.Add(consultationMessage);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Сообщение от пользователя '{user.Name}' (ID: {userId}) успешно сохранено.");
            return Ok(new { message = "Сообщение отправлено администратору" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении сообщения в базу данных.");
            return StatusCode(500, new { error = "Внутренняя ошибка сервера при сохранении сообщения" });
        }
    }
}

public class MessageModel
{
    public string Text { get; set; } = string.Empty;
}