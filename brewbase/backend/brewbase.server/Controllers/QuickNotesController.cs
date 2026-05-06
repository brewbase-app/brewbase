using brewbase.server.Dtos;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class QuickNotesController : ControllerBase
{
    private readonly IQuickNoteService _quickNoteService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuickNotesController(
        IQuickNoteService quickNoteService,
        ICurrentUserProvider currentUserProvider)
    {
        _quickNoteService = quickNoteService;
        _currentUserProvider = currentUserProvider;
    }

    /// <summary>Creates a quick note for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuickNoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateQuickNoteRequestDto request)
    {
        if (_currentUserProvider.GetUserId() is null)
        {
            return Unauthorized();
        }

        request.Content = request.Content.Trim();
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            ModelState.AddModelError(nameof(request.Content), "Content is required.");
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _quickNoteService.CreateAsync(request);
        if (created is null)
        {
            return Unauthorized();
        }

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Returns the current user quick notes, newest first. Optional search filters by content (case-insensitive substring).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuickNoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var notes = await _quickNoteService.GetAllForCurrentUserAsync(search);
        if (notes is null)
        {
            return Unauthorized();
        }

        return Ok(notes);
    }

    /// <summary>Returns one quick note if it belongs to the current user; otherwise 404.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuickNoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        if (_currentUserProvider.GetUserId() is null)
        {
            return Unauthorized();
        }

        var note = await _quickNoteService.GetByIdForCurrentUserAsync(id);
        if (note is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Quick note not found." });
        }

        return Ok(note);
    }
}
