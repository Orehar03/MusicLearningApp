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
    private readonly IWebHostEnvironment _env;

    public SubmissionsController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ЭТОТ МЕТОД НУЖЕН ДЛЯ АДМИНА
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllSubmissions()
    {
        var submissions = await _context.Submissions
            .Include(s => s.User)
            .Include(s => s.Homework)
            .OrderByDescending(s => s.SubmissionTime)
            .ToListAsync();

        return Ok(submissions);
    }

    // ЭТОТ МЕТОД ДЛЯ ОТПРАВКИ РАБОТЫ
    [HttpPost]
    public async Task<IActionResult> SubmitHomework([FromForm] SubmissionModel model)
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
            SubmissionTime = DateTime.Now
        };

        if (model.File != null && model.File.Length > 0)
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }

            submission.FilePath = "/uploads/" + uniqueFileName;
        }

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();
        return Ok("Работа отправлена");
    }
}

public class SubmissionModel
{
    public int HomeworkId { get; set; }
    public string? TextAnswer { get; set; }
    public IFormFile? File { get; set; }
}