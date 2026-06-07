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

    [HttpPost]
    [ProducesResponseType(typeof(QuickNoteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateQuickNoteRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
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

        var created = await _quickNoteService.CreateAsync(userId.Value, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Returns the current user's quick notes, newest first. Optional search filters by content (case-insensitive substring).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<QuickNoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var notes = await _quickNoteService.GetAllAsync(userId.Value, search);
        return Ok(notes);
    }

    /// <summary>Returns one quick note if it belongs to the current user; otherwise 404.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(QuickNoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var note = await _quickNoteService.GetByIdAsync(id, userId.Value);
        if (note is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Quick note not found." });
        }

        return Ok(note);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(QuickNoteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuickNoteRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
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

        var updated = await _quickNoteService.UpdateAsync(id, userId.Value, request);
        if (updated is null)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Quick note not found." });
        }

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(SimpleErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _quickNoteService.DeleteAsync(id, userId.Value);
        if (!deleted)
        {
            return NotFound(new SimpleErrorResponseDto { Message = "Quick note not found." });
        }

        return NoContent();
    }
}
