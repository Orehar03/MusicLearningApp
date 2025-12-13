using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MusicLearningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationController : ControllerBase
{
    [Authorize] // 🔥 Добавлен атрибут авторизации
    [HttpPost("message")]
    public IActionResult SendMessage([FromBody] MessageModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Text))
            return BadRequest(new { error = "Сообщение не может быть пустым" });

        // Логируем сообщение
        var userId = User.FindFirst("nameid")?.Value;
        Console.WriteLine($"📩 Новое сообщение от пользователя {userId}: {model.Text}");

        return Ok(new { message = "Сообщение отправлено администратору" });
    }
}

public class MessageModel
{
    public string Text { get; set; } = string.Empty;
}