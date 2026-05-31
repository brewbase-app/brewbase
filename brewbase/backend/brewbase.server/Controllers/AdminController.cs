using brewbase.server.Dtos;
using brewbase.server.Services;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserListResponseDto>>> GetUsers()
    {
        var users = await _adminService.GetUsersAsync();

        return Ok(users);
    }
    
    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> UpdateUserRole(
        int id,
        [FromBody] AdminUpdateUserRoleDto dto)
    {
        try
        {
            var updated = await _adminService.UpdateUserRoleAsync(id, dto.Role);

            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPatch("block-user/{userId}")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        var result = await _adminService
            .BlockUserAsync(userId);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpPatch("unblock-user/{userId}")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var result = await _adminService
            .UnblockUserAsync(userId);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpPatch("articles/{id}/approve")]
    public async Task<IActionResult> ApproveArticle(int id)
    {
        var result = await _adminService.ApproveArticleAsync(id);

        return result.Status switch
        {
            ArticleApproveStatus.Approved => Ok(),
            ArticleApproveStatus.NotFound => NotFound(),
            ArticleApproveStatus.CoffeeAlreadyHasApprovedWiki => BadRequest(
                new SimpleErrorResponseDto
                {
                    Message = "This coffee already has an approved wiki article."
                }),
            _ => BadRequest()
        };
    }
    
    [HttpPatch("articles/{id}/reject")]
    public async Task<IActionResult> RejectArticle(
        int id,
        ModerateArticleRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Comment))
        {
            return BadRequest(new SimpleErrorResponseDto
            {
                Message = "Komentarz moderacji jest wymagany przy odrzuceniu treści."
            });
        }

        var result = await _adminService
            .RejectArticleAsync(id, dto);

        if (!result)
            return NotFound();

        return Ok();
    }
    
    [HttpGet("articles/pending")]
    public async Task<ActionResult<List<PendingArticleResponseDto>>> GetPendingArticles()
    {
        var articles = await _adminService
            .GetPendingArticlesAsync();

        return Ok(articles);
    }
    
    [HttpGet("reports")]
    public async Task<ActionResult<List<ReportedArticleResponseDto>>>
        GetReports([FromQuery] string scope = "open")
    {
        var reports = await _adminService
            .GetReportsAsync(scope);

        return Ok(reports);
    }

    [HttpPatch("reports/{reportId}/dismiss")]
    public async Task<IActionResult> DismissReport(int reportId)
    {
        return MapReportModerationResult(
            await _adminService.DismissReportAsync(reportId));
    }

    [HttpPatch("reports/{reportId}/uphold")]
    public async Task<IActionResult> UpholdReport(int reportId)
    {
        return MapReportModerationResult(
            await _adminService.UpholdReportAsync(reportId));
    }

    private IActionResult MapReportModerationResult(ReportModerationResult result)
    {
        return result switch
        {
            ReportModerationResult.Success => NoContent(),
            ReportModerationResult.AlreadyResolved => Conflict(
                new SimpleErrorResponseDto
                {
                    Message = "To zgłoszenie zostało już rozpatrzone."
                }),
            ReportModerationResult.ContentNotFound => BadRequest(
                new SimpleErrorResponseDto
                {
                    Message = "Nie udało się usunąć zgłoszonej treści. Treść mogła zostać już usunięta."
                }),
            _ => NotFound(new SimpleErrorResponseDto
            {
                Message = "Nie znaleziono zgłoszenia."
            })
        };
    }
}