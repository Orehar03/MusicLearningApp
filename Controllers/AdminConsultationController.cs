using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicLearningApp.Data;
using MusicLearningApp.Models;
using Microsoft.EntityFrameworkCore;

namespace MusicLearningApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminConsultationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AdminConsultationController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages()
    {
        var messages = await _context.ConsultationMessages
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
        return Ok(messages);
    }
}