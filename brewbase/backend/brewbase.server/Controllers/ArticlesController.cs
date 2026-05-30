using brewbase.server.Dtos;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleReadService _articleReadService;
    private readonly IArticleWriteService _articleWriteService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ArticlesController(
        IArticleReadService articleReadService,
        IArticleWriteService articleWriteService,
        ICurrentUserProvider currentUserProvider)
    {
        _articleReadService = articleReadService;
        _articleWriteService = articleWriteService;
        _currentUserProvider = currentUserProvider;
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateArticleResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateArticleRequestDto request)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        request.Title = request.Title.Trim();
        request.Content = request.Content.Trim();
        request.Module = request.Module.Trim();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            ModelState.AddModelError(nameof(request.Content), "Content is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Module))
        {
            ModelState.AddModelError(nameof(request.Module), "Module is required.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var created = await _articleWriteService.CreateAsync(userId.Value, request);

        return created.Status switch
        {
            ArticleCreateStatus.Success => CreatedAtAction(
                nameof(GetById),
                new { id = created.Response!.Id },
                created.Response),
            ArticleCreateStatus.InvalidModule => ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.Module)] = ["Invalid module."]
                })),
            ArticleCreateStatus.CoffeeIdNotAllowedForModule => ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.CoffeeId)] = ["CoffeeId is only allowed for coffee module articles."]
                })),
            ArticleCreateStatus.CoffeeNotFound => ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]>
                {
                    [nameof(request.CoffeeId)] = ["Referenced coffee does not exist."]
                })),
            _ => BadRequest()
        };
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ArticleListResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? module, [FromQuery] string? search)
    {
        var articles = await _articleReadService.GetApprovedAsync(module, search);
        return Ok(articles);
    }

    [Authorize]
    [HttpGet("mine")]
    [ProducesResponseType(typeof(List<MyArticleListResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine([FromQuery] string? status)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var articles = await _articleReadService.GetMineAsync(userId.Value, status);
        return Ok(articles);
    }

    [Authorize]
    [HttpGet("mine/{id:int}")]
    [ProducesResponseType(typeof(MyArticleDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMineById(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var article = await _articleReadService.GetMineByIdAsync(id, userId.Value);
        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [Authorize]
    [HttpDelete("mine/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(int id)
    {
        var userId = _currentUserProvider.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _articleWriteService.DeleteMineAsync(id, userId.Value);

        return result switch
        {
            ArticleDeleteResult.Deleted => NoContent(),
            ArticleDeleteResult.NotAllowed => Forbid(),
            _ => NotFound()
        };
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ArticleDetailResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var article = await _articleReadService.GetApprovedByIdAsync(id);
        if (article is null)
        {
            return NotFound();
        }

        return Ok(article);
    }
}
