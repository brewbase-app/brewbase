using brewbase.server.Dtos;
using brewbase.server.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace brewbase.server.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize]
    [HttpPost("article/{articleId}")]
    public async Task<IActionResult> ReportArticle(
        int articleId,
        CreateReportRequestDto dto)
    {
        var result = await _reportService
            .CreateReportAsync(articleId, dto);

        if (!result)
            return BadRequest();

        return Ok();
    }
}