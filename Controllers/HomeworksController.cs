using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicLearningApp.Data;
using MusicLearningApp.Models;

namespace MusicLearningApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class HomeworksController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HomeworksController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetHomeworks()
    {
        var homeworks = await _context.Homeworks.ToListAsync();
        return Ok(homeworks);
    }
}