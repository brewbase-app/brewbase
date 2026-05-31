using brewbase.server.Dtos;

namespace brewbase.server.Services.Interfaces;

public interface IReportService
{
    Task<ReportCreateResult> CreateReportAsync(CreateReportRequestDto dto);
}

public enum ReportCreateResult
{
    Created,
    Invalid,
    Duplicate,
    NotFound
}
