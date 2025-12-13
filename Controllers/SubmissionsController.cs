using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using System.Security.Claims;

namespace MusicLearningApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SubmissionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitHomework([FromBody] SubmissionModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var homework = await _context.Homeworks.FindAsync(model.HomeworkId);
        if (homework == null || DateTime.Now > homework.Deadline)
            return BadRequest("Дедлайн истек или задание не найдено");

        var submission = new Submission
        {
            HomeworkId = model.HomeworkId,
            UserId = userId,
            TextAnswer = model.TextAnswer,
            // FilePath пока не реализован для загрузки файлов
            SubmissionTime = DateTime.Now
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return Ok("Работа отправлена");
    }
}

public class SubmissionModel
{
    public int HomeworkId { get; set; }
    public string? TextAnswer { get; set; }
}