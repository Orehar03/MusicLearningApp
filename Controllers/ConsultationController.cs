using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MusicLearningApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ConsultationController : ControllerBase
{
    [HttpPost("message")]
    public IActionResult SendMessage([FromBody] MessageModel model)
    {
        // В учебных целях просто логируем сообщение
        Console.WriteLine($"Сообщение от {User.Identity?.Name}: {model.Text}");
        return Ok("Сообщение отправлено администратору");
    }
}

public class MessageModel
{
    public string Text { get; set; } = string.Empty;
}