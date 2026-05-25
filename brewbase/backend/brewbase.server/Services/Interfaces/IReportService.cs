using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IReportService
{
    Task<bool> CreateReportAsync(int articleId, CreateReportRequestDto dto);

}