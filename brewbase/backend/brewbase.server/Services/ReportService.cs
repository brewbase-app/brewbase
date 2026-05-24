using brewbase.server.Dtos;
using brewbase.server.Models;
using brewbase.server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace brewbase.server.Services;

public class ReportService : IReportService
{
    private readonly BrewDbContext _context;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ReportService(
        BrewDbContext context,
        ICurrentUserProvider currentUserProvider)
    {
        _context = context;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<bool> CreateReportAsync(
        int articleId,
        CreateReportRequestDto dto)
    {
        var userId = _currentUserProvider.GetUserId();

        if (userId == null)
            return false;

        var articleExists = await _context.Articles
            .AnyAsync(a => a.Id == articleId);

        if (!articleExists)
            return false;

        var alreadyReported = await _context.Reports
            .AnyAsync(r =>
                r.ArticleId == articleId &&
                r.ReportedByUserId == userId);

        if (alreadyReported)
            return false;

        var report = new Report
        {
            ArticleId = articleId,
            ReportedByUserId = userId.Value,
            Reason = dto.Reason,
            CreatedAt = DateTime.Now
        };

        _context.Reports.Add(report);

        await _context.SaveChangesAsync();

        return true;
    }
    
}