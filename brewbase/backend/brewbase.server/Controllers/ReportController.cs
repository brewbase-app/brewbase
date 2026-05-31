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
    [HttpPost]
    public async Task<IActionResult> CreateReport(
        CreateReportRequestDto dto)
    {
        return MapResult(
            await _reportService.CreateReportAsync(dto));
    }

    [Authorize]
    [HttpPost("article/{articleId}")]
    public async Task<IActionResult> ReportArticle(
        int articleId,
        CreateReportRequestDto dto)
    {
        dto.ContentType = "article";
        dto.ContentId = articleId;

        return MapResult(
            await _reportService.CreateReportAsync(dto));
    }

    private IActionResult MapResult(ReportCreateResult result)
    {
        return result switch
        {
            ReportCreateResult.Created => Ok(),
            ReportCreateResult.Duplicate => Conflict(
                new SimpleErrorResponseDto
                {
                    Message = "Ta treść została już przez Ciebie zgłoszona."
                }),
            ReportCreateResult.NotFound => NotFound(
                new SimpleErrorResponseDto
                {
                    Message = "Nie znaleziono zgłaszanej treści."
                }),
            _ => BadRequest(
                new SimpleErrorResponseDto
                {
                    Message = "Nieprawidłowe dane zgłoszenia."
                })
        };
    }
}
